using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneXus.Utils;
using Jayrock.Json;
using GeneXus;
using log4net;
using GeneXus.SD.Store.Model;
using GeneXus.SD.Store.Platforms;

namespace GeneXus.SD.Store
{
	public class StoreManager
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(StoreManager));

		public int ErrCode
		{
			get; set;
		}

		public String ErrDescription
		{
			get; set;
		}

		public int GetPurchase(GxUserType gxStoreConfig, string productId, GxUserType gxPurchaseResult, GxUserType gxStorePurchase)
		{
			PurchaseResult purchase = JSONHelper.Deserialize<PurchaseResult>(gxPurchaseResult.ToJSonString());
			StorePurchase sp = null;
			int errCode =  GetPurchaseImpl(gxStoreConfig, productId, purchase, out sp);
			if (errCode == 0)
			{
				gxStorePurchase.FromJSonString(sp.ToJson());
			}
			return errCode;
		}

		public bool AcknowledgePurchase(GxUserType gxStoreConfig, string productId, GxUserType gxPurchaseResult)
		{
			IStoreManager storeMgr = null;
			int errCode = GetManager(gxStoreConfig, 2, out storeMgr);
			GooglePlayStoreManager mgr = (GooglePlayStoreManager)storeMgr;
			PurchaseResult purchase = JSONHelper.Deserialize<PurchaseResult>(gxPurchaseResult.ToJSonString());
			try
			{
				return mgr.AcknowledgePurchase(productId, purchase);
			}
			catch (StoreConfigurationException e)
			{
				errCode = 3;
				ErrDescription = e.Message;
			}
			catch (StoreInvalidPurchaseException e)
			{
				errCode = 2;
				ErrDescription = e.Message;
			}
			catch (StoreServerException e)
			{
				errCode = 4;
				ErrDescription = e.Message;
			}
			catch (StoreException e)
			{
				errCode = 10;
				ErrDescription = e.Message;
			}
			return false;
		}

		private int GetPurchaseImpl(GxUserType gxStoreConfig, string productId, PurchaseResult purchase, out StorePurchase storePurchase)
		{
			storePurchase = new StorePurchase();
			int errCode;
			IStoreManager storeMgr;
			ErrDescription = string.Empty;
			
			errCode = GetManager(gxStoreConfig, purchase.Platform, out storeMgr);
			if (errCode == 0)
			{				
				try
				{
					productId = productId.Trim();
					purchase.PurchaseId = purchase.PurchaseId.Trim();
					purchase.ProductIdentifier = purchase.ProductIdentifier.Trim();
					storePurchase = storeMgr.GetPurchase(productId, purchase);					
					errCode = 0;
				}
				catch (StoreConfigurationException e)
				{
					errCode = 3;
					ErrDescription = e.Message;
				}
				catch (StoreInvalidPurchaseException e)
				{
					errCode = 2;
					ErrDescription = e.Message;
				}
				catch (StoreServerException e)
				{
					errCode = 4;
					ErrDescription = e.Message;
				}
				catch (StoreException e)
				{
					errCode = 10;
					ErrDescription = e.Message;
				}

			}
			ErrCode = errCode;
			return errCode;
		}

		public String GetPurchases(GxUserType gxStoreConfig, GxUserType purchasesInformation)
		{
			GxSimpleCollection<GxUserType> gxStorePurchases = new GxSimpleCollection<GxUserType>();			
			
			PurchasesInformation pInformation = JSONHelper.Deserialize<PurchasesInformation>(purchasesInformation.ToJSonString()); ;
			
			IStoreManager storeMgr;
			ErrCode = GetManager(gxStoreConfig, pInformation.PurchasePlatform, out storeMgr);
			if (storeMgr != null)
			{
				Type type = Type.GetType("GeneXus.Core.genexus.sd.store.SdtStorePurchase, GeneXus.Core.Common", true);
				List<StorePurchase> purchases = storeMgr.GetPurchases(pInformation);
				foreach (StorePurchase p in purchases)
				{															
					GxUserType storePurchase = (GxUserType)Activator.CreateInstance(type);
					storePurchase.FromJSonString(p.ToJson());
					gxStorePurchases.Add(storePurchase);
				}
			}
			return gxStorePurchases.ToJSonString(false);			
		}

		public int GetManager(GxUserType gxStoreConfig, int platform, out IStoreManager mgr)
		{
			Init();
			JObject storeConfig = (JObject)gxStoreConfig.GetJSONObject();
			mgr = null;
			int errCode = 1;
			switch (platform)
			{
				case 2:
					string appleKey;
					if (GetConfigValue("appleKey", storeConfig, out appleKey))
					{
						mgr = new AppleStoreStoreManager(appleKey);
						errCode = 0;
					}
					break;
				case 1:
					string sAccount, certPath, certPassword;

					if (GetConfigValue("googleServiceAccount", storeConfig, out sAccount) &&
						GetConfigValue("googleCertificate", storeConfig, out certPath) &&
						GetConfigValue("googleCertificatePassword", storeConfig, out certPassword))
					{
						mgr = new GooglePlayStoreManager() { CertificatePassword = certPassword, CertificatePath = certPath, ServiceAccountEmail = sAccount };
						errCode = 0;
					}
					break;
				default:
					throw new StoreInvalidPurchaseException("StoreManager Platform not implemented");
			}
			ErrCode = errCode;
			return errCode;
		}

		public static bool GetConfigValue(string key, JObject storeConfig, out string value)
		{
			value = string.Empty;
			if (storeConfig.Contains(key))
			{
				value = (string)storeConfig[key];
			}
			if (string.IsNullOrEmpty(value))
			{
				GXLogging.Error(log, string.Format("{0} must be specified", key));
			}

			return !string.IsNullOrEmpty(value);
		}

		private void Init()
		{
			ErrCode = 0;
			ErrDescription = string.Empty;
		}
	}
}
