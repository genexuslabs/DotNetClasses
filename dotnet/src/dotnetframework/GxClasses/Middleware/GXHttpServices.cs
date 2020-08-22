namespace GeneXus.Http
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;

	using GeneXus.Application;
	using GeneXus.Configuration;
	using GeneXus.Data.NTier;
	using GeneXus.Encryption;
	using GeneXus.Metadata;
	using GeneXus.Mime;
	using GeneXus.Security;
	using GeneXus.Utils;
	using GeneXus.XML;
	using GeneXus.WebControls;

	using log4net;
	using Jayrock.Json;
	using System.Web.SessionState;
	using System.Web;
#if NETCORE
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Http.Extensions;
	using System.Net;
	using GeneXus.Web.Security;
	using System.Linq;
	using GeneXus.Procedure;
#else
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Net;
	using GeneXus.Notifications;
	using Web.Security;
	using System.Runtime.Serialization;
	using System.Diagnostics;
#endif

	internal class GXValidService : GXHttpHandler, IRequiresSessionState
	{
		public GXValidService()
		{
			this.context = new GxContext();

		}
		public override void webExecute()
		{
			try
			{
				NameValueCollection parms = context.HttpContext.Request.GetQueryString();

				string gxobj = parms["object"];
				string attribute = parms["att"];
				string json = null;

				GxStringCollection gxparms = new GxStringCollection();
				if (parms.Count > 2)
				{
					for (int i = 2; i < parms.Count; i++)
						gxparms.Add(parms[i]);
				}
				if (!string.IsNullOrEmpty(gxobj) && !string.IsNullOrEmpty(attribute))
				{
					string nspace;
					if (!Config.GetValueOf("AppMainNamespace", out nspace))
						nspace = "GeneXus.Programs";
					GXHttpHandler handler = (GXHttpHandler)ClassLoader.GetInstance(gxobj, nspace + "." + gxobj, null);
					handler.initialize();

					json = (string)handler.GetType().InvokeMember("rest_" + attribute.ToUpper(), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, handler, new object[] { gxparms });
					handler.GetType().InvokeMember("cleanup", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, handler, null);
				}
				if (!string.IsNullOrEmpty(json))
				{
                    if (context.IsMultipartRequest)
                        this.context.HttpContext.Response.ContentType = MediaTypesNames.TextHtml;
                    else
                        this.context.HttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
#if NETCORE
					this.context.HttpContext.Response.Write(json);
#else
					this.context.HttpContext.Response.Output.WriteLine(json);
#endif

				}
				else
				{
					this.SendResponseStatus(404, "Resource not found");
				}
			}
			catch (Exception ex)
			{
				SendResponseStatus(500, ex.Message);
				HttpHelper.SetResponseStatusAndJsonError(context.HttpContext, "500", ex.Message);
			}
			finally
			{
				try
				{
					context.CloseConnections();
				}
				catch
				{

				}
			}

		}

	}
	internal class GXMultiCall : GXHttpHandler, IRequiresSessionState
	{
		static string EXECUTE_METHOD = "execute";

		public GXMultiCall()
		{
			this.context = new GxContext();
		}
		public override void webExecute()
		{
#if NETCORE
			GxRestWrapper handler = null;
#else
			Utils.GxRestService handler = null;
#endif
			try
			{
				HttpRequest req = context.HttpContext.Request;
				string gxobj = GetNextPar().ToLower();
				string jsonStr = (new StreamReader(req.GetInputStream())).ReadToEnd();
				GxSimpleCollection<JArray> parmsColl = new GxSimpleCollection<JArray>();
				if (!string.IsNullOrEmpty(jsonStr))
				{
					parmsColl.FromJSonString(jsonStr);
				}

				string nspace;
				if (!Config.GetValueOf("AppMainNamespace", out nspace))
					nspace = "GeneXus.Programs";
#if NETCORE
				var controllerInstance = ClassLoader.FindInstance(gxobj, nspace, gxobj, new Object[] { context }, Assembly.GetEntryAssembly());
				GXProcedure proc = controllerInstance as GXProcedure;
				if (proc != null)
				{
					handler = new GxRestWrapper(proc, localHttpContext, context as GxContext);
				}
				else
				{
					var sdtInstance = ClassLoader.FindInstance(Config.CommonAssemblyName, nspace, $"Sdt{gxobj}", new Object[] { context }, Assembly.GetEntryAssembly()) as GxSilentTrnSdt;
					if (sdtInstance != null)
						handler = new GXBCRestService(sdtInstance, localHttpContext, context as GxContext);
				}
#else
				handler = (Utils.GxRestService)ClassLoader.FindInstance(gxobj, nspace, gxobj + "_services", null, null);
#endif
				handler.RunAsMain = false;

				ParameterInfo[] pars = handler.GetType().GetMethod(EXECUTE_METHOD).GetParameters();
				int ParmsCount = pars.Length;
				object[] convertedparms = new object[ParmsCount];

				if (parmsColl.Count > 0)
				{
					foreach (JArray parmValues in parmsColl)
					{
						int idx = 0;
						for (int i = 0; i < ParmsCount; i++)
						{
							if (!pars[i].IsOut)
								convertedparms[i] = convertparm(pars, i, parmValues[idx]);
							idx++;
						}
						handler.GetType().InvokeMember(EXECUTE_METHOD, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, handler, convertedparms);
					}
				}
			}
			catch (GxClassLoaderException cex)
			{
				SendResponseStatus(404, cex.Message);
				HttpHelper.SetResponseStatusAndJsonError(context.HttpContext, "404", cex.Message);
			}
			catch (Exception ex)
			{
				SendResponseStatus(500, ex.Message);
				HttpHelper.SetResponseStatusAndJsonError(context.HttpContext, "500", ex.Message);
			}
			finally
			{
				if (handler != null)
				{
					handler.RunAsMain = true;
					handler.Cleanup();
				}
			}
		}

	}

	public class GXReorServices : GXHttpHandler
	{
		static readonly ILog log = LogManager.GetLogger(typeof(GXReorServices));
		static Assembly _reorAssembly;
		static Assembly _gxDataInitializationAssembly;
		readonly string[] reorArgs = { "-force", "-ignoreresume", "-nogui", "-noverifydatabaseschema" };
		readonly string[] dataArgs = null;
		const string DataInitialization = "DataInitialization";
		const string ERROR_LINE = ">>>Error";
		public GXReorServices()
		{
			this.context = new GxContext();
		}
		protected override void sendCacheHeaders()
		{
		}
		public override void sendAdditionalHeaders()
		{
		}
		protected override void SetCompression(HttpContext httpContext)
		{
			
		}
		public override void webExecute()
		{
			string commandType = GetNextPar();
			TextWriter cOut = Console.Out;
			int code = 0;
			try
			{
				using (var responseWriter = new HttpResponseWriter(context.HttpContext.Response))
				{
					Console.SetOut(responseWriter);
					if (commandType == DataInitialization)
					{
						code = (int)ClassLoader.InvokeStatic(DataInitializationAssembly, "GeneXus.Utils.GXDataInitialization", "Main", new object[] { dataArgs });
					}
					else //REORG 
					{
						code = (int)ClassLoader.InvokeStatic(ReorAssembly, "GeneXus.Forms.ReorgStartup", "Main", new object[] { reorArgs });
					}
				}
				if (code != 0)
				{
					GXLogging.Error(log, "Error executing ", commandType);
					context.HttpContext.Response.Write(ERROR_LINE);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Error executing {commandType}:", code.ToString(), ex);
				context.HttpContext.Response.Write(ex.Message);
				context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}
			finally
			{
				Console.SetOut(cOut);
			}
		}
		public static Assembly DataInitializationAssembly
		{
			get
			{
				try
				{
					if (_gxDataInitializationAssembly == null)
					{
						_gxDataInitializationAssembly = Assembly.Load(new AssemblyName("GXDataInitialization"));
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error loading GXDataInitializationAssembly", ex);
				}
				return _gxDataInitializationAssembly;
			}
		}
		
		public static Assembly ReorAssembly
		{
			get
			{
				try
				{
					if (_reorAssembly == null)
					{
						_reorAssembly = Assembly.Load(new AssemblyName("Reor"));
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error loading Reor", ex);
				}
				return _reorAssembly;
			}
		}
	}
	
	class HttpResponseWriter : TextWriter
	{
		private HttpResponse response;

		public HttpResponseWriter(HttpResponse response)
		{
			this.response = response;
			this.response.ContentType = MediaTypesNames.TextHtml;
		}
		
		public override Encoding Encoding { get { return Encoding.UTF8; } }

		public override void Write(string value)
		{
			response.Write(value);
#if !NETCORE
			response.Flush();
#endif
			base.Write(value);
		}

		public override void WriteLine(string value)
		{
			Write(value);
			Write(Environment.NewLine);
		}
	}

	internal class GXResourceProvider : GXHttpHandler
	{
		internal static string PROVIDER_NAME = "GXResourceProvider.aspx";

		public GXResourceProvider()
		{
			this.context = new GxContext();
		}

		public override void webExecute()
		{
			string resourceType = this.GetNextPar();
			if (string.Compare(resourceType.Trim(), "image", true) == 0)
			{
				string imageGUID = this.GetNextPar();
				string kbId = this.GetNextPar();
				string theme = this.GetNextPar();
				this.context.setAjaxCallMode();
				this.context.SetDefaultTheme(theme);
				if (Guid.TryParse(imageGUID, out Guid sanitizedGuid))
				{
					string imagePath = this.context.GetImagePath(sanitizedGuid.ToString(), kbId, theme);
					if (!string.IsNullOrEmpty(imagePath))
					{
						this.context.HttpContext.Response.Clear();
						this.context.HttpContext.Response.ContentType = MediaTypesNames.TextPlain;
#if NETCORE
						this.context.HttpContext.Response.Write(imagePath);
#else
						this.context.HttpContext.Response.Output.WriteLine(imagePath);
						this.context.HttpContext.Response.End();
#endif
						return;
					}
				}
			}
			this.SendResponseStatus(404, "Resource not found");
		}
	}




	internal class GXObjectUploadServices : GXHttpHandler, IReadOnlySessionState
	{
		public GXObjectUploadServices()
		{
			this.context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
		}

		public override void webExecute()
		{
			try
			{
				if (context.isMultipartRequest())
				{
					localHttpContext.Response.ContentType = MediaTypesNames.TextPlain;
					var r = new List<UploadFile>();
					var fileCount = localHttpContext.Request.GetFileCount();
					for (var i =0; i< fileCount; i++)
					{
						var hpf = localHttpContext.Request.GetFile(i);
						string fileName = string.Empty;
						string[] files = hpf.FileName.Split(new char[] { '\\' });
						if (files.Length > 0)
							fileName = files[files.Length - 1];
						else
							fileName = hpf.FileName;

						string ext = FileUtil.GetFileType(fileName);
						string savedFileName = FileUtil.getTempFileName(Preferences.getTMP_MEDIA_PATH(), FileUtil.GetFileName(fileName), string.IsNullOrEmpty(ext) ? "tmp" : ext);
						GxFile gxFile = new GxFile(Preferences.getTMP_MEDIA_PATH(), savedFileName);

						gxFile.Create(hpf.InputStream);

						GXFileWatcher.Instance.AddTemporaryFile(gxFile);

                        r.Add(new UploadFile()
						{
							name = fileName,
							size = gxFile.GetLength(),
							url = gxFile.GetPath(),
							type = context.GetContentType(ext),
							extension = ext,
							thumbnailUrl = gxFile.GetPath(),
                            path = savedFileName
						});
					}
					UploadFilesResult result = new UploadFilesResult() { files = r };
					var jsonObj = JSONHelper.Serialize(result);
					localHttpContext.Response.Write(jsonObj);
				}
				else
				{
					Stream istream = localHttpContext.Request.GetInputStream();
					String contentType = localHttpContext.Request.ContentType;
					String ext = context.ExtensionForContentType(contentType);

					string fileName = FileUtil.getTempFileName(Preferences.getTMP_MEDIA_PATH(), "BLOB", string.IsNullOrEmpty(ext) ? "tmp" : ext);
                    GxFile file = new GxFile(Preferences.getTMP_MEDIA_PATH(), fileName);
                    file.Create(istream);

					JObject obj = new JObject();
					fileName = file.GetURI();
                  
                    String fileGuid =  Guid.NewGuid().ToString("N");
                    String fileToken= GxRestPrefix.UPLOAD_PREFIX + fileGuid;
                    CacheAPI.FilesCache.Set(fileGuid, fileName, GxRestPrefix.UPLOAD_TIMEOUT);
					obj.Put("object_id", fileToken);
					localHttpContext.Response.AddHeader("GeneXus-Object-Id", fileToken);
					localHttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
					localHttpContext.Response.StatusCode = 201;
					localHttpContext.Response.Write(obj.ToString());
				}
			}
			catch (Exception e)
			{
				SendResponseStatus(500, e.Message);
				HttpHelper.SetResponseStatusAndJsonError(localHttpContext, HttpStatusCode.InternalServerError.ToString(), e.Message);
			}
			finally
			{
				try
				{
					context.CloseConnections();
				}
				catch
				{

				}
			}

		}
		protected override bool IntegratedSecurityEnabled
		{
			get
			{
				string value;
				return (Config.GetValueOf("EnableIntegratedSecurity", out value) && value.Equals("1"));

			}
		}

		protected override GAMSecurityLevel IntegratedSecurityLevel
		{
			get
			{
				return GAMSecurityLevel.SecurityObject;
			}
		}

		protected override string IntegratedSecurityPermissionName
		{
			get
			{
				return base.IntegratedSecurityPermissionName;
			}
		}
	}
	public class UploadFilesResult
	{
		public List<UploadFile> files;
	}
	public class UploadFile
	{
		public string url { get; set; }
		public string thumbnailUrl { get; set; }
		public string name { get; set; }
		public long size { get; set; }
		public string type { get; set; }
		public string path { get; set; }
		public string extension { get; set; }
	}
	internal class GXOAuthLogout : GXHttpHandler, IRequiresSessionState
	{
		public GXOAuthLogout()
		{
			this.context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
		}

		public override void webExecute()
		{
			try
			{
				GxSecurityProvider.Provider.oauthlogout(context);
				localHttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
				localHttpContext.Response.StatusCode = 200;
				localHttpContext.Response.Write(new JObject().ToString());
				context.CloseConnections();
			}
			catch (Exception e)
			{
				localHttpContext.Response.Write(e.Message);
				localHttpContext.Response.StatusCode = 500;
			}
		}

	}

	internal class GXOAuthUserInfo : GXHttpHandler, IRequiresSessionState
	{
		public GXOAuthUserInfo()
		{
			this.context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
		}

		public override void webExecute()
		{
			try
			{
				String userJson;
				bool isOK;
				GxSecurityProvider.Provider.oauthgetuser(context, out userJson, out isOK);
				localHttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
				localHttpContext.Response.StatusCode = 200;
				localHttpContext.Response.Write(userJson);
				context.CloseConnections();
			}
			catch (Exception e)
			{
				localHttpContext.Response.Write(e.Message);
				localHttpContext.Response.StatusCode = 500;
			}
		}
		protected override bool IntegratedSecurityEnabled
		{
			get
			{
				string value;
				return (Config.GetValueOf("EnableIntegratedSecurity", out value) && value.Equals("1"));

			}
		}

		protected override GAMSecurityLevel IntegratedSecurityLevel
		{
			get
			{
				return GAMSecurityLevel.SecurityObject;
			}
		}

		protected override string IntegratedSecurityPermissionName
		{
			get
			{
				return base.IntegratedSecurityPermissionName;
			}
		}
	}

	internal class GXOAuthAccessToken : GXHttpHandler, IRequiresSessionState
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Http.GXOAuthAccessToken));
		public GXOAuthAccessToken()
		{
			this.context = new GxContext();
		}

		public override void webExecute()
		{
			bool isRefreshToken = false;
			bool isDevice = false;
			bool isExternalSDAuth = false;
			String clientId = cgiGet("client_id");
			String clientSecret = cgiGet("client_secret");
			String grantType = cgiGet("grant_type");
			String nativeToken = cgiGet("native_token");
			String nativeVerifier = cgiGet("native_verifier");
			String avoid_redirect = cgiGet("avoid_redirect");
			String additional_parameters = cgiGet("additional_parameters");
			String refreshToken = "";
			String userName = string.Empty;
			String userPassword = string.Empty;
			String scope = string.Empty;
			string URL = string.Empty;
			bool flag = false;
			try
			{
				DataStoreUtil.LoadDataStores(context);

				if (grantType.Equals("refresh_token", StringComparison.OrdinalIgnoreCase))
				{
					refreshToken = cgiGet("refresh_token");
					isRefreshToken = true;
				}
				else if (grantType.Equals("device", StringComparison.OrdinalIgnoreCase))
				{
					isDevice = true;
				}
				else if (!string.IsNullOrEmpty(nativeToken))
				{
					isExternalSDAuth = true;
				}
				else
				{
					userName = cgiGet("username");
					userPassword = cgiGet("password");
					scope = cgiGet("scope");
				}

				OutData gamout;
				GxResult result;
				if (isRefreshToken)
				{
					result = GxSecurityProvider.Provider.refreshtoken(context, clientId, clientSecret, refreshToken, out gamout, out flag);
				}
				else if (isDevice)
				{
					result = GxSecurityProvider.Provider.logindevice(context, clientId, clientSecret, out gamout, out flag);
				}
				else if (isExternalSDAuth)
				{
					result = GxSecurityProvider.Provider.externalauthenticationfromsdusingtoken(context, grantType, nativeToken, nativeVerifier, clientId, clientSecret, ref scope, additional_parameters, out gamout, out flag);
				}
				else if (String.IsNullOrEmpty(additional_parameters))
				{
					result = GxSecurityProvider.Provider.oauthauthentication(context, grantType, userName, userPassword, clientId, clientSecret, scope, out gamout, out URL, out flag);				
				}
				else
				{
					result = GxSecurityProvider.Provider.oauthauthentication(context, grantType, userName, userPassword, clientId, clientSecret, scope, additional_parameters, out gamout, out URL, out flag);
				}

				localHttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
				if (!flag)
				{
					localHttpContext.Response.StatusCode = 401;
					if (result != null)
					{
						string messagePermission = result.Description;
						HttpHelper.SetResponseStatusAndJsonError(context.HttpContext, result.Code, messagePermission);
						if (GXUtil.ContainsNoAsciiCharacter(messagePermission))
						{
							messagePermission = string.Format("{0}{1}", GxRestPrefix.ENCODED_PREFIX, Uri.EscapeDataString(messagePermission));
						}
						localHttpContext.Response.AddHeader(HttpHeader.AUTHENTICATE_HEADER, HttpHelper.OatuhUnauthorizedHeader(context.GetServerName(), result.Code, messagePermission));
					}
				}
				else
				{
					if (!isDevice && !isRefreshToken && (gamout == null || String.IsNullOrEmpty((string)gamout["gxTpr_Access_token"])))
					{
						if (string.IsNullOrEmpty(avoid_redirect))
							localHttpContext.Response.StatusCode = 303;
						else
							localHttpContext.Response.StatusCode = 200;
						localHttpContext.Response.AddHeader("location", URL);
						JObject jObj = new JObject();
						jObj.Put("Location", URL);
						localHttpContext.Response.Write(jObj.ToString());
					}
					else
					{
						localHttpContext.Response.StatusCode = 200;
						localHttpContext.Response.Write(gamout.JsonString);
						
					}
				}
				context.CloseConnections();
			}
			catch (Exception e)
			{
				localHttpContext.Response.StatusCode = 404;
				localHttpContext.Response.Write(e.Message);
				GXLogging.Error(log, string.Format("Error in access_token service clientId:{0} clientSecret:{1} grantType:{2} userName:{3} scope:{4}", clientId, clientSecret, grantType, userName, scope), e);
			}
		}

	}

}



