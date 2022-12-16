using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Jayrock.Json;
using GeneXus.SD.Store.Model;
using System.Collections.Concurrent;
using System.Net.Http;

namespace GeneXus.SD.Store.Platforms
{
	public class AppleStoreStoreManager : IStoreManager
	{
		private const string APPLE_STORE_VALIDATION_URL_PROD = "https://buy.itunes.apple.com/verifyReceipt";
		private const string APPLE_STORE_VALIDATION_URL_SANDBOX = "https://sandbox.itunes.apple.com/verifyReceipt";
		private string _iTunesStorePassword;

		public AppleStoreStoreManager(string iTunesStorePassword)
		{
			_iTunesStorePassword = iTunesStorePassword;
		}

		public StorePurchase GetPurchase(string productId, PurchaseResult purchaseResult)
		{
			StorePurchase p = new StorePurchase();

			string purchaseToken = purchaseResult.PurchaseId;

			if (string.IsNullOrEmpty(purchaseToken))
			{
				throw new StoreInvalidPurchaseException("PurchaseToken not found in Purchase Transaction Data");
			}

			if (string.IsNullOrEmpty(_iTunesStorePassword))
			{
				throw new StoreConfigurationException("iTunes Store Password cannot be empty");
			}

			try
			{
				p = ValidatePurchase(purchaseToken, purchaseResult, PurchaseEnvironment.Production);
			}
			catch (StoreResponseEnvironmentException)
			{
				p = ValidatePurchase(purchaseToken, purchaseResult, PurchaseEnvironment.Sandbox);
			}
			return p;
		}

		public enum PurchaseEnvironment
		{
			Production, Sandbox
		}

		private ConcurrentDictionary<string, string> dataCache = new ConcurrentDictionary<string, string>();

		private StorePurchase ValidatePurchase(string purchaseToken, PurchaseResult purchaseResult, PurchaseEnvironment env)
		{
			StorePurchase p = null;

			string responseString;
			string url = (env == PurchaseEnvironment.Production) ? APPLE_STORE_VALIDATION_URL_PROD : APPLE_STORE_VALIDATION_URL_SANDBOX;

			JObject inputObj = new JObject();
			inputObj.Put("receipt-data", purchaseResult.TransactionData.Trim());
			inputObj.Put("password", _iTunesStorePassword);

			string key = Util.GetHashString(inputObj.ToString());
			if (!dataCache.TryGetValue(key, out responseString))
			{
				
				HttpStatusCode code = DoPost(inputObj.ToString(), url, out responseString);
				switch (code)
				{
					case HttpStatusCode.OK:
						break;
					default:
						throw new StoreInvalidPurchaseException("");
				}
				dataCache.TryAdd(key, responseString);
			}
			
			JObject jResponse = Util.FromJSonString(responseString);

			if (jResponse.Contains("status"))
			{
				int statusCode = (int)jResponse["status"];
				switch (statusCode)
				{
					case 21000:
						throw new StoreResponsePurchaseException("The App Store could not read the JSON object you provided.");
					case 21002:
						throw new StoreResponsePurchaseException("The data in the receipt-data property was malformed or missing.");
					case 21003:
						throw new StoreResponsePurchaseException("The receipt could not be authenticated.");
					case 21004:
						throw new StoreResponsePurchaseException("The shared secret you provided does not match the shared secret on file for your account.");
					case 21005:
						throw new StoreResponsePurchaseException("The receipt server is not currently available.");
					case 21006:
						throw new StoreResponsePurchaseException("Could not handle 21006 status response");
					case 21007:
						string value;
						dataCache.TryRemove(key, out value);
						throw new StoreResponseEnvironmentException("This receipt is from the test environment, but it was sent to the production environment for verification. Send it to the test environment instead.");
					case 21008:
						break;
					case 0:						
						break;
					default:
						throw new StoreResponsePurchaseException($"Could not handle '{statusCode}' status response");
				}
				bool found = false;
				if (jResponse.Contains("receipt"))
				{
					JObject receipt = (JObject)jResponse["receipt"];
					if (receipt.Contains("in_app"))
					{
						JArray purchases = (JArray)receipt["in_app"];
						foreach (JObject purchase in purchases)
						{
							if (purchase.Contains("transaction_id") && (string)purchase["transaction_id"] == purchaseToken)
							{
								found = true;
								p = ParsePurchase(purchase);
								String ATT_ORIG_TRN_ID = "original_transaction_id";
								if (p.PurchaseStatus == PurchaseStatus.Expired && p.ProductType == (int)ProductType.Subscription && purchase.Contains(ATT_ORIG_TRN_ID) && jResponse.Contains("latest_receipt_info"))
								{
									String originalTransactionId = (string)purchase[ATT_ORIG_TRN_ID];
									JArray latestInfo = (JArray)jResponse["latest_receipt_info"];
									List<StorePurchase> list = new List<StorePurchase>();
									foreach (JObject latestPurchase in latestInfo)
									{
										if (latestPurchase.Contains(ATT_ORIG_TRN_ID) && (string)latestPurchase[ATT_ORIG_TRN_ID] == originalTransactionId)
										{
											p = ParsePurchase(latestPurchase);
											list.Add(p);
											if (p.PurchaseStatus == PurchaseStatus.Valid)
											{
												break;
											}
										}
									}
									if (p.PurchaseStatus != PurchaseStatus.Valid && list.Count > 0)
									{
										list = list.OrderByDescending(sp => sp.Subscription.Expiration).ToList();
										p = list.First();
									}
								}
								else
								{
									break;
								}
							}
						}
					}

				}
				if (!found)
				{
					throw new StorePurchaseNotFoundException("Purchase Id not found inside Apple Receipt");
				}
			}
			else
			{
				throw new StoreResponsePurchaseException("Aplle Store validation servers seems to be unavailable.");
			}
			if (p!=null && p.ProductType == (int)ProductType.Subscription)
			{
				p.Custom.OriginalPurchase = purchaseResult;
			}
			return p;
		}

		private StorePurchase ParsePurchase(JObject purchase)
		{
			StorePurchase p;
			ProductType pType = ProductType.Product;
			if (purchase.Contains("web_order_line_item_id"))
			{
				pType = ProductType.Subscription;
			}

			p = new StorePurchase()
			{
				ProductIdentifier = GetValueDefault("product_id", purchase, string.Empty),
				PurchaseId = GetValueDefault("transaction_id", purchase, string.Empty),
				PurchaseDate = GetDateValueFromMS("purchase_date_ms", purchase),
				ProductType = (int)pType,
				PurchaseStatus = PurchaseStatus.Valid,
				Custom = new StorePurchaseCustom()
				{
					Quantity = Int32.Parse(GetValueDefault("quantity", purchase, string.Empty)),
					IsTrialPeriod = GetValueDefault("is_trial_period", purchase, "false") == "true"
				}
			};

			if (pType == ProductType.Subscription)
			{
				p.Subscription = new StorePurchaseSubscription()
				{
					Expiration = GetDateValueFromMS("expires_date_ms", purchase),
					FirstPurchased = GetDateValueFromMS("original_purchase_date_ms", purchase)
				};				
				if (p.Subscription.Expiration < DateTime.Now)
				{
					p.PurchaseStatus = PurchaseStatus.Expired;
				}
			}

			return p;
		}

		private string GetValueDefault(string key, JObject obj, string defaultValue)
		{
			string value = string.Empty;
			TryGetValue(key, obj, out value);
			return value;
		}

		private DateTime GetDateValueFromMS(string key, JObject obj)
		{
			string value = GetValueDefault(key, obj, "0");
			return (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(value));
		}

		private bool TryGetValue(string key, JObject obj, out string value)
		{
			value = null;
			if (obj.Contains(key))
			{
				value = obj[key].ToString();
				return true;
			}
			return false;
		}

		private static HttpStatusCode DoPost(string postData, string url, out string response)
		{
			try
			{
				using (var client = new HttpClient())
				{
					using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
					{
						request.Content = new StringContent(postData, Encoding.ASCII, "application/json");
						HttpResponseMessage result = client.SendAsync(request).GetAwaiter().GetResult();
						response = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
						return result.StatusCode;
					}
				}
			}
			catch (Exception e)
			{
				throw new StoreServerException("Apple Store Server exception", e);
			}
		}

		public List<StorePurchase> GetPurchases(PurchasesInformation pInfo)
		{
			List<StorePurchase> list = new List<StorePurchase>();
			foreach (PurchaseResult p in pInfo.Purchases)
			{				
				p.TransactionData = pInfo.AppleReceipt;
				try {
					StorePurchase sp = this.GetPurchase(p.ProductIdentifier, p);
					if (!list.Contains(sp))
					{
						list.Add(sp);
						
					}
				}
				catch
				{
					
				}
			}
			list.RemoveAll(sp => sp.ProductType == (int)ProductType.Subscription && sp.PurchaseStatus != PurchaseStatus.Valid);
			return list;
		}
	}
}
