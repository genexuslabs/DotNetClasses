using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Services;
using Jayrock.Json;
using Google.Apis.AndroidPublisher.v3.Data;
using GeneXus.Application;
using System.IO;
using Google;
using GeneXus.SD.Store.Model;
using System.Collections.Concurrent;

namespace GeneXus.SD.Store.Platforms
{
	public class GooglePlayStoreManager : IStoreManager
	{
		private static ConcurrentDictionary<String, ProductType> productTypesCache = new ConcurrentDictionary<string, ProductType>();

		public string ServiceAccountEmail { get; set; }
		public string CertificatePath { get; set; }
		public string CertificatePassword { get; set; }

		private ServiceAccountCredential _credential;

		public StorePurchase GetPurchase(string productId, PurchaseResult purchaseResult)
		{
			StorePurchase sP;
			AndroidPublisherService service;
			string packageName, token;
			InitializeImpl(purchaseResult, out sP, out service, out packageName, out token);

			try
			{
				ProductType productType = GetPurchaseType(productId.Trim(), service, packageName.Trim());

				if (productType == ProductType.Subscription)
				{
					SubscriptionPurchase s = service.Purchases.Subscriptions.Get(packageName, productId, token).Execute();

					sP = new StorePurchase()
					{
						ProductIdentifier = productId,
						PurchaseId = token,
						ProductType = (int)ProductType.Subscription,
						PurchaseDate = Util.FromUnixTime(s.StartTimeMillis.GetValueOrDefault(0)),
						
						Subscription = new StorePurchaseSubscription()
						{
							Expiration = Util.FromUnixTime(s.ExpiryTimeMillis.GetValueOrDefault(0)),
							FirstPurchased = Util.FromUnixTime(s.StartTimeMillis.GetValueOrDefault(0))
						},

						Custom = new StorePurchaseCustom()
						{
							Consumed = false,
							CancelReason = s.CancelReason,
							IsTrialPeriod = false,
							WillAutoRenew = s.AutoRenewing.GetValueOrDefault(false),
							Quantity = 1,
							OriginalPurchase = purchaseResult,
							AcknowledgementState = s.AcknowledgementState.Value
						}
					};

					if (sP.Subscription.Expiration <= DateTime.Now)
					{
						sP.PurchaseStatus = PurchaseStatus.Expired;
					}
					else
					{
						if (sP.Custom.CancelReason.HasValue)
						{
							int cancelReason = sP.Custom.CancelReason.Value;
							if (cancelReason == 0)
								sP.PurchaseStatus = PurchaseStatus.Valid;
							if (cancelReason == 1)
								sP.PurchaseStatus = PurchaseStatus.Cancelled;
						}
						else
						{
							sP.PurchaseStatus = PurchaseStatus.Valid;
						}
					}
				}
				else
				{
					ProductPurchase p = service.Purchases.Products.Get(packageName, productId, token).Execute();
					sP = new StorePurchase()
					{
						ProductIdentifier = productId,
						PurchaseId = token,
						ProductType = (int)ProductType.Product,
						PurchaseDate = Util.FromUnixTime(p.PurchaseTimeMillis.GetValueOrDefault(0)),

						Custom = new StorePurchaseCustom()
						{
							Consumed = (p.ConsumptionState.HasValue && p.ConsumptionState.Value == 1),
							IsTrialPeriod = false,
							Quantity = 1,
							OriginalPurchase = purchaseResult,
							AcknowledgementState = p.AcknowledgementState.Value
						}
					};
					if (p.PurchaseState == 1)
					{
						sP.PurchaseStatus = PurchaseStatus.Cancelled;
					}
					else
					{
						if (p.ConsumptionState == 1) 
						{
							sP.PurchaseStatus = PurchaseStatus.Expired;
						}
						else
						{
							sP.PurchaseStatus = PurchaseStatus.Valid;
						}
					}
				}
			}
			catch (GoogleApiException e)
			{
				switch (e.HttpStatusCode)
				{
					case System.Net.HttpStatusCode.NotFound:
						throw new StorePurchaseNotFoundException("Google Play Purchase Token was not found");
					default:
						throw new StoreException(e.Message, e);
				}
			}
			catch (Exception e)
			{
				throw new StoreException(e.Message, e);
			}
			return sP;
		}

		private void InitializeImpl(PurchaseResult purchaseResult, out StorePurchase sP, out AndroidPublisherService service, out string packageName, out string token)
		{
			sP = null;
			Initialize();

			service = new AndroidPublisherService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = _credential,
				ApplicationName = "GeneXus Application",
			});
			JObject trnData = JSONHelper.ReadJSON<JObject>(purchaseResult.TransactionData);

			packageName = (string)trnData["packageName"];
			token = (string)trnData["purchaseToken"];
			if (string.IsNullOrEmpty(packageName))
			{
				throw new StoreInvalidPurchaseException("PackageName not found in Purchase Transaction Data");
			}
			if (string.IsNullOrEmpty(token))
			{
				throw new StoreInvalidPurchaseException("PurchaseToken not found in Purchase Transaction Data");
			}
		}

		private static ProductType GetPurchaseType(string productId, AndroidPublisherService service, string packageName)
		{
			ProductType productType = ProductType.Subscription;
			string key = packageName + "_" + productId;
			if (!productTypesCache.TryGetValue(key, out productType))
			{
				var inApp = service.Inappproducts.Get(packageName, productId).Execute();
				if (inApp.PurchaseType == "managedUser")
				{
					productType = ProductType.Product;
				}
				if (inApp.PurchaseType == "subscription")
				{
					productType = ProductType.Subscription;
				}
				productTypesCache[key] = productType;
			}

			return productType;
		}

		public List<StorePurchase> GetPurchases(PurchasesInformation pInfo)
		{
			int failedTrns = 0;
			List<StorePurchase> purchases = new List<StorePurchase>();
			Exception lastExcp = null;
			foreach (var p in pInfo.Purchases)
			{
				try
				{
					StorePurchase sP = GetPurchase(p.ProductIdentifier, p);
					purchases.Add(sP);
				}
				catch (Exception e)
				{
					failedTrns++;
					lastExcp = e;
				}
			}
			if (lastExcp != null && failedTrns == pInfo.Purchases.Count)
			{
				throw lastExcp;
			}
			return purchases;
		}

		public void Initialize()
		{
			String serviceAccountEmail = ServiceAccountEmail;
			string certPath = CertificatePath;

			if (string.IsNullOrEmpty(serviceAccountEmail))
			{
				throw new StoreConfigurationException("Google Play Service Account Email cannot be empty");
			}

			if (!System.IO.Path.IsPathRooted(certPath))
			{
				string privatePath = System.IO.Path.Combine(GxContext.Current.GetPhysicalPath(), "private");
				certPath = System.IO.Path.Combine(privatePath, certPath);
			}
			if (!File.Exists(certPath))
			{
				throw new StoreConfigurationException("Google Play Certificate was not found");
			}
			X509Certificate2 certificate = null;

			try
			{
				certificate = new X509Certificate2(certPath, CertificatePassword, X509KeyStorageFlags.Exportable);
			}
			catch (Exception e)
			{
				throw new StoreConfigurationException("Google Play Certificate could not be loaded", e);
			}

			_credential = new ServiceAccountCredential(
			   new ServiceAccountCredential.Initializer(serviceAccountEmail)
			   {
				   Scopes = new[] { "https://www.googleapis.com/auth/androidpublisher" }
			   }.FromCertificate(certificate));

		}

		public bool AcknowledgePurchase(string productId, PurchaseResult purchaseResult)
		{
			StorePurchase sP;
			AndroidPublisherService service;
			string packageName, token;
			InitializeImpl(purchaseResult, out sP, out service, out packageName, out token);

			try
			{
				ProductType productType = GetPurchaseType(productId.Trim(), service, packageName.Trim());
								
				if (productType == ProductType.Product)
				{
					ProductPurchasesAcknowledgeRequest ack = new ProductPurchasesAcknowledgeRequest();
					var request = service.Purchases.Products.Acknowledge(ack, packageName, productId, token);
					string response = request.Execute();
					if (string.IsNullOrEmpty(response))
					{
						return true;
					}
				}

				if (productType == ProductType.Subscription)
				{
					SubscriptionPurchasesAcknowledgeRequest ack = new SubscriptionPurchasesAcknowledgeRequest();
					var request = service.Purchases.Subscriptions.Acknowledge(ack, packageName, productId, token);
					string response = request.Execute();
					if (string.IsNullOrEmpty(response))
					{
						return true;
					}
				}

			}
			catch (Exception e)
			{
				throw e;
			}
			return false;
		}
	}
}
