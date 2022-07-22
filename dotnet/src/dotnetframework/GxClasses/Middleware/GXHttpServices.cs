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
	using GxClasses.Web.Middleware;

#else
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Net;
	using GeneXus.Notifications;
	using Web.Security;
	using System.Runtime.Serialization;
	using System.Diagnostics;
#endif

	internal class GXMultiCall : GXHttpHandler, IRequiresSessionState
	{
		static string EXECUTE_METHOD = "execute";

		public GXMultiCall()
		{
			this.context = new GxContext();
		}
#if NETCORE
		static GXRouting gxRouting;
		GXRouting GetRouting()
		{
			if (gxRouting == null)
				gxRouting = new GXRouting(string.Empty);
			return gxRouting;
		}
#endif
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
#if NETCORE

				handler = GetRouting().GetController(context.HttpContext, new ControllerInfo() { Name = gxobj.Replace('.',Path.DirectorySeparatorChar)});
				if (handler ==null) {
					throw new GxClassLoaderException($"{gxobj} not found");
				}
#else
				string nspace;
				if (!Config.GetValueOf("AppMainNamespace", out nspace))
					nspace = "GeneXus.Programs";
				handler = (GxRestService)ClassLoader.FindInstance(gxobj, nspace, gxobj + "_services", null, null);
#endif
				handler.RunAsMain = false;

#if NETCORE
				GXBaseObject worker = handler.Worker;
#else
				GxRestService worker = handler;
#endif
				ParameterInfo[] pars = worker.GetType().GetMethod(EXECUTE_METHOD).GetParameters();

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
						worker.GetType().InvokeMember(EXECUTE_METHOD, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, worker, convertedparms);
					}
				}
			}
			catch (GxClassLoaderException cex)
			{
				HttpHelper.SetUnexpectedError(context.HttpContext, HttpStatusCode.NotFound, cex);
			}
			catch (Exception ex)
			{
				HttpHelper.SetUnexpectedError(context.HttpContext, HttpStatusCode.InternalServerError, ex);
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

	
	internal class GXObjectUploadServices : GXHttpHandler, IReadOnlySessionState
	{
		public GXObjectUploadServices()
		{
			this.context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
		}
		public GXObjectUploadServices(IGxContext ctx)
		{
			this.context = ctx;
		}

		public override void webExecute()
		{
			try
			{
				string ext, fName;
				if (context.isMultipartRequest())
				{
					localHttpContext.Response.ContentType = MediaTypesNames.TextPlain;
					var r = new List<UploadFile>();
					var fileCount = localHttpContext.Request.GetFileCount();
					for (var i = 0; i < fileCount; i++)
					{
						string fileGuid = GxUploadHelper.GetUploadFileGuid();
						string fileToken = GxUploadHelper.GetUploadFileId(fileGuid);
						var hpf = localHttpContext.Request.GetFile(i);
						fName = string.Empty;
						string[] files = hpf.FileName.Split(new char[] { '\\' });
						if (files.Length > 0)
							fName = files[files.Length - 1];
						else
							fName = hpf.FileName;

						ext = FileUtil.GetFileType(fName);
						string tempDir = Preferences.getTMP_MEDIA_PATH();
						GxFile gxFile = new GxFile(tempDir, FileUtil.getTempFileName(tempDir), GxFileType.PrivateAttribute);

						gxFile.Create(hpf.InputStream);

						string uri = gxFile.GetURI();
						string url = (PathUtil.IsAbsoluteUrl(uri)) ? uri : context.PathToUrl(uri);

						r.Add(new UploadFile()
						{
							name = fName,
							size = gxFile.GetLength(),
							url = url,
							type = context.GetContentType(ext),
							extension = ext,
							thumbnailUrl = url,
							path = fileToken
						});
						GxUploadHelper.CacheUploadFile(fileGuid, Path.GetFileName(fName), ext, gxFile, context);
					}
					UploadFilesResult result = new UploadFilesResult() { files = r };
					var jsonObj = JSONHelper.Serialize(result);
					localHttpContext.Response.Write(jsonObj);
				}
				else
				{					
					WcfExecute(localHttpContext.Request.GetInputStream(), localHttpContext.Request.ContentType, (long)localHttpContext.Request.ContentLength);
				}
			}
			catch (Exception e)
			{
				HttpHelper.SetUnexpectedError(localHttpContext, HttpStatusCode.InternalServerError, e);
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

		internal void WcfExecute(Stream istream, string contentType, long streamLength)
		{
			string ext, fName;
			ext = context.ExtensionForContentType(contentType);
			string tempDir = Preferences.getTMP_MEDIA_PATH();			
			GxFile file = new GxFile(tempDir, FileUtil.getTempFileName(tempDir), GxFileType.PrivateAttribute);			
			file.Create(new NetworkInputStream(istream, streamLength));

			JObject obj = new JObject();
			fName = file.GetURI();
			string fileGuid = GxUploadHelper.GetUploadFileGuid();
			string fileToken = GxUploadHelper.GetUploadFileId(fileGuid);

			obj.Put("object_id", fileToken);
			localHttpContext.Response.AddHeader("GeneXus-Object-Id", fileGuid);
			localHttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
			HttpHelper.SetResponseStatus(localHttpContext, HttpStatusCode.Created, string.Empty);
			localHttpContext.Response.Write(obj.ToString());

			GxUploadHelper.CacheUploadFile(fileGuid, $"{Path.GetFileNameWithoutExtension(fName)}.{ext}", ext, file, context);
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
	}

	/// <summary>
	///	Custom Network Stream for direct not multiparts uploads that do not support length operations
	/// </summary>
	public class NetworkInputStream : Stream
	{
		private Stream innerStream;
		private long streamLength;

		public NetworkInputStream(Stream s, long length): base()
		{
			innerStream = s;
			streamLength = length;			
		}

		public override bool CanRead => innerStream.CanRead;

		public override bool CanSeek => innerStream.CanSeek;

		public override bool CanWrite => innerStream.CanWrite;

		public override long Length => streamLength;

		public override long Position { get => innerStream.Position; set => innerStream.Position = value; }

		public override void Flush()
		{
			innerStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return innerStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return innerStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			innerStream.SetLength(value);
			streamLength = value;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			innerStream.Write(buffer, offset, count);
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
				localHttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
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
				localHttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
				localHttpContext.Response.Write(userJson);
				context.CloseConnections();
			}
			catch (Exception e)
			{
				localHttpContext.Response.Write(e.Message);
				localHttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
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
					if (result != null)
					{
						string messagePermission = result.Description;

						HttpHelper.SetGamError(context.HttpContext, result.Code, messagePermission);
						if (GXUtil.ContainsNoAsciiCharacter(messagePermission))
						{
							messagePermission = string.Format("{0}{1}", GxRestPrefix.ENCODED_PREFIX, Uri.EscapeDataString(messagePermission));
						}
						localHttpContext.Response.AddHeader(HttpHeader.AUTHENTICATE_HEADER, HttpHelper.OatuhUnauthorizedHeader(context.GetServerName(), result.Code, messagePermission));
					}
					else
					{
						HttpHelper.SetResponseStatus(context.HttpContext, HttpStatusCode.Unauthorized, string.Empty);
					}
				}
				else
				{
					if (!isDevice && !isRefreshToken && (gamout == null || String.IsNullOrEmpty((string)gamout["gxTpr_Access_token"])))
					{
						if (string.IsNullOrEmpty(avoid_redirect))
							localHttpContext.Response.StatusCode = (int)HttpStatusCode.RedirectMethod;
						else
							localHttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
						localHttpContext.Response.AddHeader("location", URL);
						JObject jObj = new JObject();
						jObj.Put("Location", URL);
						localHttpContext.Response.Write(jObj.ToString());
					}
					else
					{
						localHttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
						localHttpContext.Response.Write(gamout.JsonString);
						
					}
				}
				context.CloseConnections();
			}
			catch (Exception e)
			{
				localHttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
				localHttpContext.Response.Write(e.Message);
				GXLogging.Error(log, string.Format("Error in access_token service clientId:{0} clientSecret:{1} grantType:{2} userName:{3} scope:{4}", clientId, clientSecret, grantType, userName, scope), e);
			}
		}

	}

}



