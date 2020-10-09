using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Spatial;
using Simple.OData.Client;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.OData.Edm;
using Microsoft.Data.Edm;
using System.Data.Common;
using System.Linq.Expressions;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Net.Http;
using GeneXus.Http;
using System.Collections.Concurrent;

namespace GeneXus.Data.NTier
{

	public class CurrentOfManager
	{
		private IDictionary<string, ODataDataReader> m_CurrentOfManager = new Dictionary<string, ODataDataReader>();
		internal void AddQuery(string CursorId, ODataDataReader DataReader)
		{
			m_CurrentOfManager.Remove(CursorId);
			m_CurrentOfManager.Add(CursorId, DataReader);
		}

		public void RemoveQuery(string CursorId)
		{
			m_CurrentOfManager.Remove(CursorId);
		}

		internal ODataDataReader GetQuery(string CursorId)
		{
			return m_CurrentOfManager[CursorId];
		}
	}

	public class ODataDBService : GxService
	{
		public ODataDBService(string id, string providerId) : base(id, providerId, typeof(ODataConnection))
		{
		}
	}

	public class ODataConnection : ServiceConnection
	{
		private GXODataClient client;
		private ODataClientSettings clientSettings;
		protected string m_OriginalConnectionString;
		private string m_ODataConnectionString;

		public override string ConnectionString
		{
			get
			{
				return m_ODataConnectionString;
			}

			set
			{
				m_OriginalConnectionString = value;
				InitializeConnection();
			}
		}

		private void InitializeConnection()
		{
			DbConnectionStringBuilder builder = new DbConnectionStringBuilder(false);
			builder.ConnectionString = m_OriginalConnectionString;
			ICredentials credentials = null;
			string serviceUri = "";
			string user = null;
			string password = null;
			string sapLoginBO = null, b1SessionId = null;
			string SAP = null, SAPcsrfToken = null;
			string metadataLocation = $"{ Application.GxContext.StaticPhysicalPath() }METADATA{ Path.DirectorySeparatorChar }SERVICES{ Path.DirectorySeparatorChar }";
			bool hasUserMetadataLocation = false;
			bool? poolConnections = null;
			bool sapB1ByToken = false;
			ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12; // Framework 4.5.x does not have TLS 1.2 enabled by default
			if (builder.TryGetValue("User Id", out object userId) && builder.TryGetValue("Password", out object pass))
			{
				user = userId.ToString();
				password = pass.ToString();
				credentials = new NetworkCredential(user, password);
			}
			if (builder.TryGetValue("Data Source", out object url))
			{
				serviceUri = url.ToString();
				clientSettings = new ODataClientSettings(serviceUri, credentials);
				clientSettings.IgnoreUnmappedProperties = true;
			}
			else throw new ArgumentException("Data source url cannot be empty");
			if (builder.TryGetValue("headers", out object headers))
			{
				if (!string.IsNullOrWhiteSpace(headers.ToString()))
				{
					IList<KeyValuePair<string, string>> requestHeaders = new List<KeyValuePair<string, string>>();
					foreach (string header in headers.ToString().Split(new char[] { '#' }))
					{
						string[] headerInfo = header.ToString().Split(new char[] { ':' });
						if (headerInfo.Length == 2)
							requestHeaders.Add(new KeyValuePair<string, string>(headerInfo[0], headerInfo[1]));
					}
					if (requestHeaders.Count > 0)
					{
						clientSettings.BeforeRequest += request =>
						{
							foreach (KeyValuePair<string, string> header in requestHeaders)
							{
								request.Headers.Add(header.Key, header.Value);
							}
						};
					}
				}
			}
			if (builder.TryGetValue("CheckOptimisticConcurrency", out object oCheckOptimisticConcurrency))
			{
				string checkOptimisticConcurrency = oCheckOptimisticConcurrency.ToString().Trim();
				HashSet<string> checkOptimisticConcurrencyEntities = null;
				if (!checkOptimisticConcurrency.Equals("true") && !checkOptimisticConcurrency.Equals("all"))
				{
					checkOptimisticConcurrencyEntities = new HashSet<string>(checkOptimisticConcurrency.ToLowerInvariant().Split(new char[] { ',' }));
				}

				clientSettings.BeforeRequest += request =>
				{
					if ((request.Method.Method == "PUT" ||
						request.Method.Method == "PATCH" ||
						request.Method.Method == "MERGE" ||
						request.Method.Method == "DELETE"))
					{
						if (!request.Headers.IfMatch.Contains(System.Net.Http.Headers.EntityTagHeaderValue.Any))
						{
							if (checkOptimisticConcurrencyEntities == null)
								request.Headers.IfMatch.Add(System.Net.Http.Headers.EntityTagHeaderValue.Any);
							else
							{
								try
								{
									string target = request.RequestUri.Segments[request.RequestUri.Segments.Length - 1].Split(new char[] { '(' })[0].ToLowerInvariant();
									if (checkOptimisticConcurrencyEntities.Contains(target))
										request.Headers.IfMatch.Add(System.Net.Http.Headers.EntityTagHeaderValue.Any);
								}
								catch { }
							}
						}
					}
				};
			}
			if (builder.TryGetValue("SapLogin", out object saplogin))
			{
				user = user ?? string.Empty;
				password = password ?? string.Empty;
				sapLoginBO = saplogin.ToString();
				object b1value;
				if (builder.TryGetValue("B1SESSION", out b1value))
					b1SessionId = b1value.ToString();
			}
			if (builder.TryGetValue("SapLoginMethod", out object sapLoginMethod))
			{
				sapB1ByToken = sapLoginMethod.ToString().Trim().Equals("token", StringComparison.InvariantCultureIgnoreCase);
			}
			if (builder.TryGetValue("MetadataLocation", out object metadatavalue))
			{
				metadataLocation = $"{metadatavalue.ToString()}{Path.DirectorySeparatorChar}";
				hasUserMetadataLocation = true;
			}
			if (builder.TryGetValue("AllowUnsecure", out object auvalue))
			{
				if (auvalue.ToString().Trim().Equals("True", StringComparison.InvariantCultureIgnoreCase))
				{
					ServicePointManager.ServerCertificateValidationCallback +=
						delegate (object sender, X509Certificate certificate,
												X509Chain chain,
												SslPolicyErrors sslPolicyErrors)
						{
							return true;
						};
				}
			}
			if (builder.TryGetValue("SAP", out object sapvalue) && user != null && password != null)
			{
				SAP = sapvalue.ToString();
			}
			if (builder.TryGetValue("RecordNotFoundServiceCodes", out object notFoundServiceCodes))
			{
				RecordNotFoundServiceCodes = RecordNotFoundServiceCodes ?? new HashSet<string>(notFoundServiceCodes.ToString().Split(new char[] { ',' }));
			}
			if (builder.TryGetValue("RecordAlreadyExistsServiceCodes", out object alreadyExistsServiceCodes))
			{
				RecordAlreadyExistsServiceCodes = RecordAlreadyExistsServiceCodes ?? new HashSet<string>(alreadyExistsServiceCodes.ToString().Split(new char[] { ',' }));
			}
			if (builder.TryGetValue("Pooling", out object poolingValue))
			{
				poolConnections = poolingValue.ToString().Trim().Equals("True", StringComparison.InvariantCultureIgnoreCase);
			}

			if (SAP != null)
			{
				clientSettings.BeforeRequest += delegate (System.Net.Http.HttpRequestMessage message)
				{
					message.Headers.Add("x-csrf-token", SAPcsrfToken ?? "Fetch");
				};
				clientSettings.AfterResponse += delegate (System.Net.Http.HttpResponseMessage response)
				{
					if (response.Headers.Contains("x-csrf-token"))
						SAPcsrfToken = response.Headers.GetValues("x-csrf-token").FirstOrDefault();
				};
			}

			if (sapLoginBO != null || sapB1ByToken)
			{
				RecordNotFoundServiceCodes = RecordNotFoundServiceCodes ?? new HashSet<string>(Enumerable.Repeat("-2028", 1));
				RecordAlreadyExistsServiceCodes = RecordAlreadyExistsServiceCodes ?? new HashSet<string>(Enumerable.Repeat("-2035", 1));

				clientSettings.PayloadFormat = ODataPayloadFormat.Json;
				isSAPBO = true; SAPProtocolVersionUpdated = false;
				clientSettings.Properties = clientSettings.Properties ?? new Dictionary<string, object>();
				clientSettings.Properties[ODataClientSettings.ExtraProperties.STRINGIZE_DATETIME_VALUES] = true;
			}
			if (sapLoginBO != null || b1SessionId != null || sapB1ByToken)
			{
				SapB1LoginHandler.InitializeHandler(clientSettings, serviceUri, sapLoginBO, user, password, sapB1ByToken ? null : b1SessionId, sapB1ByToken);
				poolConnections = poolConnections ?? true;
			}

			if(poolConnections == true)
			{
				string poolKey = Utils.GXUtil.GetHash($"{serviceUri}/{user}:{password}");
				clientSettings.OnCreateMessageHandler = () => PoolableOnCreateMessageHandler(poolKey);
			}

			clientSettings.OnTrace = OnTrace;
			if (user != null && password != null &&
				builder.TryGetValue("force_auth", out object forceAuth) && forceAuth.ToString().Equals("y", StringComparison.InvariantCultureIgnoreCase))
			{   // When the service uses with Basic authentication but is not sending the Challenge after giving the response 401.Unauthorized
				// if the property force_auth = is set then the authorization header is sent in advance
				clientSettings.BeforeRequest += request =>
				{
					request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{ user }:{ password }")));
				};
			}
			string metadataFile = $"{metadataLocation}{DataSource}.xml";
			if (File.Exists(metadataFile))
				clientSettings.MetadataDocument = File.ReadAllText(metadataFile);
			else
			{
				metadataFile = hasUserMetadataLocation ? $"..\\{ metadataFile }" : $"{ Application.GxContext.StaticPhysicalPath() }..\\METADATA{ Path.DirectorySeparatorChar }SERVICES{ Path.DirectorySeparatorChar }{DataSource}.xml";
				if (File.Exists(metadataFile))
					clientSettings.MetadataDocument = File.ReadAllText(metadataFile);
			}
			m_ODataConnectionString = serviceUri;
		}

		private static ConcurrentDictionary<string, HttpClientHandler> PoolableConnections;
		private HttpMessageHandler PoolableOnCreateMessageHandler(string poolKey)
		{
			PoolableConnections = PoolableConnections ?? new ConcurrentDictionary<string, HttpClientHandler>();
			if (PoolableConnections.TryGetValue(poolKey, out HttpClientHandler clientHandler))
			{
				GxService.log_msg($"Reusing pooled connection { clientHandler.GetHashCode() }.");
			}
			else clientHandler = PoolableConnections.GetOrAdd(poolKey, (str) =>
				{
					HttpClientHandler newHandler = new HttpClientHandler();
					if (clientSettings.Credentials != null)
					{
						newHandler.Credentials = clientSettings.Credentials;
						newHandler.PreAuthenticate = true;
						clientSettings.OnApplyClientHandler?.Invoke(newHandler);
					}
					GxService.log_msg($"Adding pooled connection { newHandler.GetHashCode() }.");
					return newHandler;
				});
			clientSettings.OnApplyClientHandler?.Invoke(clientHandler);
			return clientHandler;
		}

		class SapB1LoginHandler
		{
			private string user, password;
			private string sloginInfo;
			private Uri loginUri, serviceUri;
			private DateTime expiryDT;
			private const string SESSION_INFO_ID = "B1SESSION";
			private const string SESSION_COOKIE_NAME = "B1SESSION";
			private const string SESSION_INFO_EXPIRY = "B1SESSION_EXPIRY";
			private const string SESSION_EXPIRY_NEVER = "NEVER";
			private IGxSession gxSession;

			private bool sapB1ByToken, sapB1ByTokenReacquire = false, toRemoveCookie = false;

			private SapB1LoginHandler(ODataClientSettings clientSettings, string serviceUri, string sapLoginBO, string user, string password, string b1SessionId, bool sapB1ByToken)
			{
				this.user = user;
				this.password = password;
				this.sapB1ByToken = sapB1ByToken;
				expiryDT = b1SessionId != null ? DateTime.MaxValue : DateTime.MinValue;

				sloginInfo = string.Format("{{\"UserName\":\"{0}\",\"Password\":\"{1}\",\"CompanyDB\":\"{2}\"}}", user, password, sapLoginBO);
				string loginBase = serviceUri.TrimEnd(new char[] { '/' });
				string loginUrl = string.Format("{0}/Login", loginBase);
				loginUri = new Uri(loginUrl);
				this.serviceUri = new Uri(serviceUri);

				gxSession = Application.GxContext.Current.GetSession();
				if(b1SessionId != null)
				{
					b1Cookie = new Cookie(SESSION_COOKIE_NAME, b1SessionId, "/", loginUri.Host);
					b1Cookie.Expires = expiryDT;
				}
				else GetStoredSession();

				clientSettings.BeforeRequest += LoginHandler;
				clientSettings.OnApplyClientHandler += ClientHandler;
			}

			private bool GetStoredSession()
			{
				sapB1ByTokenReacquire = toRemoveCookie = false;
				object sessionExpiry = gxSession.GetObject(SESSION_INFO_EXPIRY);
				if (gxSession.Get(SESSION_INFO_ID) != null)
				{
					if (sessionExpiry is DateTime)
						expiryDT = DateTime.SpecifyKind((DateTime)sessionExpiry, DateTimeKind.Local);
					else
					{
						string sessionExpiryStr = sessionExpiry as string;
						if (sessionExpiry != null || sapB1ByToken)
						{
							if (sessionExpiryStr?.Equals(SESSION_EXPIRY_NEVER) == true)
								expiryDT = DateTime.MaxValue;
							else if (sessionExpiryStr != null && DateTime.TryParse(sessionExpiryStr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out expiryDT))
							{
								gxSession.SetObject(SESSION_INFO_EXPIRY, expiryDT);
							}
							else
							{
								expiryDT = DateTime.MaxValue;
								sapB1ByTokenReacquire = sapB1ByToken;
							}
						}
					}
					
					if (expiryDT >= DateTime.Now)
					{
						string b1SessionId = gxSession.Get(SESSION_INFO_ID);
						if (!string.IsNullOrEmpty(b1SessionId))
						{
							b1Cookie = new Cookie(SESSION_COOKIE_NAME, b1SessionId, "/", loginUri.Host);
							b1Cookie.Expires = expiryDT;
							return true;
						}
					}
				}
				toRemoveCookie = sapB1ByToken;
				return false;
			}

			private CookieContainer CurrentCookieContainer = null;
			private Cookie b1Cookie = null;
			private void ClientHandler(HttpClientHandler handler)
			{
				CurrentCookieContainer = handler.CookieContainer;
				bool expiredCookie = b1Cookie != null && DateTime.Now >= b1Cookie.Expires;
				if(toRemoveCookie)
				{
					Cookie cookie = CurrentCookieContainer.GetCookies(serviceUri)[SESSION_COOKIE_NAME];
					if(cookie != null)
						cookie.Expired = true;
					toRemoveCookie = false;
				}
				if (expiredCookie || CurrentCookieContainer.GetCookies(serviceUri)[SESSION_COOKIE_NAME] == null)
				{					
					if ((!expiredCookie && b1Cookie != null) || GetStoredSession())
						AddCookieToContainer();
				}
			}

			private void LoginHandler(System.Net.Http.HttpRequestMessage request)
			{
				request.Headers.ExpectContinue = false;
				if (DateTime.Now >= expiryDT || sapB1ByTokenReacquire)
				{
					if (sapB1ByToken)
					{
						if (GetStoredSession())
							AddCookieToContainer();
					}
					else
					{
						using (WebClient login = new WebClient())
						{
							login.Credentials = new NetworkCredential(user, password);
							bool originalExpect100Continue = ServicePointManager.Expect100Continue;
							ServicePointManager.Expect100Continue = false;
							try
							{
								string loginResponse = login.UploadString(loginUri, sloginInfo);
								string cookie = login.ResponseHeaders.Get("Set-Cookie");
								string b1SessionId;
								if (!string.IsNullOrEmpty(cookie))
								{
									int cookieStart = cookie.IndexOf("B1SESSION=");
									if (cookieStart >= 0)
									{
										int cookieEnd = cookie.IndexOf(';', cookieStart);
										b1SessionId = cookie.Substring(cookieStart + 10, cookieEnd - cookieStart - 10);
										int span = 6;
										int index = loginResponse.IndexOf("SessionTimeout");
										if (index > 0)
										{
											loginResponse = loginResponse.Substring(index);
											index = loginResponse.IndexOf(':');
											if (index > 0)
											{
												loginResponse = loginResponse.Substring(index + 1).Trim();
												for (index = 0; index < loginResponse.Length && Char.IsDigit(loginResponse[index]); index++) { }
												span = int.Parse(loginResponse.Substring(0, index));
											}
										}
										GxService.log_msg($"Acquired B1Session. Expires in { span } minutes.");
										expiryDT = DateTime.Now.AddMinutes(span - 1);

										gxSession.Set(SESSION_INFO_ID, b1SessionId);
										gxSession.SetObject(SESSION_INFO_EXPIRY, expiryDT);
										b1Cookie = new Cookie("B1SESSION", b1SessionId, "/", loginUri.Host);
										b1Cookie.Expires = expiryDT;
										AddCookieToContainer();
									}
								}
							}
							finally
							{
								ServicePointManager.Expect100Continue = originalExpect100Continue;
							}
						}
					}
				}
			}

			private void AddCookieToContainer()
			{
				if (CurrentCookieContainer != null)
				{
					CurrentCookieContainer.Add(b1Cookie);
					string expiryMsg = expiryDT != DateTime.MaxValue ? $" Expires in { Convert.ToInt32(expiryDT.Subtract(DateTime.Now).TotalMinutes) } minutes." : string.Empty;
					GxService.log_msg($"Acquired B1Session.{ expiryMsg }");
				}
			}

			internal static void InitializeHandler(ODataClientSettings clientSettings, string serviceUri, string sapLoginBO, string user, string password, string b1SessionId, bool sapB1ByToken)
			{
				SapB1LoginHandler sapLoginHandler = new SapB1LoginHandler(clientSettings, serviceUri, sapLoginBO, user, password, b1SessionId, sapB1ByToken);
			}
		}

		private void OnTrace(string msg, object[] args)
		{
			GxService.log_msg(String.Format(msg, args));
		}

		public override void Open()
		{
			client = new GXODataClient(new GXODataClientSettings(clientSettings, !isSAPBO));
			base.Open();
		}

		public override IDataReader ExecuteReader(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			return new ODataDataReader(this, client, cursorDef, parms, behavior);
		}

		private bool isSAPBO = false, SAPProtocolVersionUpdated = false;
		public override int ExecuteNonQuery(ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			int rows = 1;
			try
			{
				ODataQuery queryObj = (cursorDef.Query as ODataQuery);
				Func<GXODataClient, IDataParameterCollection, GXODataClient> action = queryObj.query;
				Task task = null;
				ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;

				if (isSAPBO && !SAPProtocolVersionUpdated)
				{

					ISession session = client.Session;
					try
					{
						if (session != null && session?.Adapter?.AdapterVersion != AdapterVersion.V4)
						{
							SAPProtocolVersionUpdated = true;
							session.Adapter.ProtocolVersion = "3.0";
						}
					}
					catch
					{ // If the first thing that is done is an update, the adapter has not yet been charged, we force it here
						Task.Run(() => (session.GetType().GetTypeInfo().GetDeclaredMethod("ResolveAdapterAsync").Invoke(session, new object[] { null }) as Task)).ConfigureAwait(false).GetAwaiter().GetResult();
						if (session?.Adapter?.AdapterVersion != AdapterVersion.V4)
						{
							SAPProtocolVersionUpdated = true;
							session.Adapter.ProtocolVersion = "3.0";
						};
					}
				}

				switch (queryObj.cursorType)
				{
					case ServiceCursorDef.CursorType.Insert:
						taskAwaiter = Task.Run(() => (task = action(client, parms).InsertEntryAsync(true))).ConfigureAwait(false).GetAwaiter();
						taskAwaiter.GetResult();
						break;
					case ServiceCursorDef.CursorType.Update:
						{
							if (queryObj.updates == null)
							{

								Task<IEnumerable<ODataEntry>> updTask = null;
								ConfiguredTaskAwaitable<IEnumerable<ODataEntry>>.ConfiguredTaskAwaiter taskAwaiterWithResults;
								taskAwaiterWithResults = Task.Run(() => (updTask = action(client, parms).UpdateEntriesAsync(true))).ConfigureAwait(false).GetAwaiter();
								int skipCount = 0;
								IEnumerable<ODataEntry> items;
								for (items = taskAwaiterWithResults.GetResult(); items.Count() > 1;)
								{
									skipCount += items.Count();
									taskAwaiterWithResults = Task.Run(() => (updTask = action(client, parms).UpdateEntriesAsync(true, skipCount))).ConfigureAwait(false).GetAwaiter();
									items = taskAwaiterWithResults.GetResult();
								}
								rows = skipCount + items.Count();
								task = updTask;
							}
							else
							{
								task = ApplyLinkUpdates(queryObj, parms, action, true);
							}
						}
						break;
					case ServiceCursorDef.CursorType.Delete:
						{
							Task<int> updTask = null;
							ConfiguredTaskAwaitable<int>.ConfiguredTaskAwaiter taskAwaiterWithResults;
							taskAwaiterWithResults = Task.Run(() => (updTask = action(client, parms).DeleteEntriesAsync())).ConfigureAwait(false).GetAwaiter();
							rows = taskAwaiterWithResults.GetResult();
							while (taskAwaiterWithResults.GetResult() > 1)
							{
								taskAwaiterWithResults = Task.Run(() => (updTask = action(client, parms).DeleteEntriesAsync())).ConfigureAwait(false).GetAwaiter();
								rows += taskAwaiterWithResults.GetResult();
							}
							task = updTask;
						}
						break;
					case ServiceCursorDef.CursorType.Select:
						{
							if (queryObj.continuation == null && queryObj.updates == null)
								goto default;
							if (queryObj.continuation != null)
							{
								Task<ODataEntry> findTask = action(client, parms).FindEntryAsync();
								taskAwaiter = Task.Run(() => (task = queryObj.continuation.queryWithCont(client, parms, findTask).UpdateEntryAsync(true))).ConfigureAwait(false).GetAwaiter();
								taskAwaiter.GetResult();
								break;
							}
							else
							{
								task = ApplyLinkUpdates(queryObj, parms, action, false);
							}
						}
						break;
					default:
						Debug.Assert(false, "Invalid Cursor Type");
						throw new InvalidOperationException();
				}

				if (task == null || task.IsFaulted)
				{
					Debug.Assert(false, "ExecuteNonQuery failed!");
					if (task != null)
						throw task.Exception.Flatten();
					else throw new InvalidOperationException();
				}
				else return rows;
			}catch(ArgumentNullException e)
			{
				throw GetRecordNotFoundException(e);
			}
			catch (WebRequestException e)
			{
				throw GetWebRequestException(e);
			}
			catch (AggregateException e)
			{
				Exception baseE = e.Flatten().GetBaseException();
				if (baseE is ArgumentNullException)
					throw GetRecordNotFoundException(baseE);
				else throw GetAggregateException(e);
			}
		}

		private Task ApplyLinkUpdates(ODataQuery queryObj, IDataParameterCollection parms, Func<GXODataClient, IDataParameterCollection, GXODataClient> action, bool baseUpd)
		{
			Task<ODataEntry> updTask = null;
			ConfiguredTaskAwaitable<ODataEntry>.ConfiguredTaskAwaiter taskAwaiterWithLinks;
			if (baseUpd)
				taskAwaiterWithLinks = Task.Run(() => (updTask = action(client, parms).UpdateEntryAsync(true))).ConfigureAwait(false).GetAwaiter();
			else
				taskAwaiterWithLinks = Task.Run(() => (updTask = action(client, parms).FindEntryAsync())).ConfigureAwait(false).GetAwaiter();

			ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter;
			Task task = null;
			taskAwaiterWithLinks.GetResult();
			foreach (ODataQuery linkQuery in queryObj.updates)
			{
				IDictionary<string, object> linkDict = linkQuery.setEntity(parms);
				// It can be link or unlink
				if (linkDict.Values.Any(value => !IsNullOrEmpty(value)))
					taskAwaiter = Task.Run(() => (task = linkQuery.queryWithCont(client, parms, updTask).LinkEntryAsync(ODataDynamic.ExpressionFromReference(linkQuery.entity), linkDict))).ConfigureAwait(false).GetAwaiter();
				else
					taskAwaiter = Task.Run(() => (task = linkQuery.queryWithCont(client, parms, updTask).UnlinkEntryAsync(ODataDynamic.ExpressionFromReference(linkQuery.entity)))).ConfigureAwait(false).GetAwaiter();
				taskAwaiter.GetResult();
			}
			return task;
		}

		private static bool IsNullOrEmpty(object value)
		{
			if (value == null ||
				(value is string && (string.IsNullOrEmpty(value as string))) ||
				value.ToString().Equals("0") ||
				value is DateTime && ((DateTime)value == DateTime.MinValue)
				)
				return true;
			return false;
		}

		internal static Exception GetRecordNotFoundException(Exception e)
		{
			return new ServiceException(ServiceError.RecordNotFound, e);
		}

		internal static Exception GetRecordAlreadyExistsException(Exception e)
		{
			return new ServiceException(ServiceError.RecordAlreadyExists, e);
		}

		internal static Exception GetAggregateException(AggregateException e)
		{
			if (e.InnerException is WebRequestException webRequestException)
			{
				return new ArgumentException(string.Format("{0}\n{1}", webRequestException.Message, webRequestException.Response), webRequestException);
			}
			else if (e.InnerException is Microsoft.OData.ODataException ||
					 e.InnerException is Microsoft.Data.OData.ODataException ||
					 e.InnerException is UnresolvableObjectException)
				return e.InnerException;
			else
				return e;
		}

		HashSet<string> RecordNotFoundServiceCodes;
		HashSet<string> RecordAlreadyExistsServiceCodes;

		internal Exception GetWebRequestException(WebRequestException webRequestException)
		{
			if ((RecordNotFoundServiceCodes != null || RecordAlreadyExistsServiceCodes != null) && webRequestException.Code == HttpStatusCode.BadRequest)
			{
				using (Stream responseStream = new MemoryStream(Encoding.UTF8.GetBytes(webRequestException.Response)))
				{
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ODataResponseJSONError));
					ODataResponseJSONError odataResponseError = (ODataResponseJSONError)jsonSerializer.ReadObject(responseStream);
					if (odataResponseError != null)
					{
						if (RecordNotFoundServiceCodes?.Contains(odataResponseError.error.code) == true)
							return GetRecordNotFoundException(webRequestException);
						else if (RecordAlreadyExistsServiceCodes?.Contains(odataResponseError.error.code) == true)
							return GetRecordAlreadyExistsException(webRequestException);
					}
				}
			}

			if (webRequestException.Message == "Not Found")
				return GetRecordNotFoundException(webRequestException);
			return new ArgumentException(string.Format("{0}\n{1}", webRequestException.Message, webRequestException.Response), webRequestException);
		}
	}

	public class CurrentOf
	{
		string CursorName;
		IDictionary<string, object> CurrentEntry;
		CurrentOfManager CurrentOfManager;
		public CurrentOf(CurrentOfManager CurrentOfManager, string CursorName)
		{
			this.CurrentOfManager = CurrentOfManager;
			this.CursorName = CursorName;
			CurrentEntry = CurrentOfManager.GetQuery(CursorName).CurrentOfEntry;
		}

		public CurrentOf Set(IODataMap key, object parmValue)
		{
			key.SetValue(CurrentEntry, parmValue);
			return this;
		}
		public IDictionary<string, object> End()
		{
			return CurrentOfManager.GetQuery(CursorName).CurrentOf;
		}

	}

	internal class ODataDataReader : IDataReader
	{
		private Func<GXODataClient, IDataParameterCollection, GXODataClient> action;
		IODataMap[] selectList;
		string[] allSelectedKeys;

		private GXODataClient client;
		private ODataFeedAnnotations annotations;
		private IDataParameterCollection parms;
		private ODataConnection conn;
		private ConfiguredTaskAwaitable<IEnumerable<IDictionary<string, object>>>.ConfiguredTaskAwaiter taskAwaiter;
		private Task<IEnumerable<IDictionary<string, object>>> task;
		private IEnumerator<IDictionary<string, object>> data;
		private IEnumerator<IDictionary<string, object>> flattenedData;
		private IDictionary<string, object> currentEntry;
		private IDictionary<string, object> currentRecord;
		private CommandBehavior behavior;
		private ServiceCursorDef cursorDef;
		private readonly char[] separator = new char[] { '&' };

		public ODataDataReader(ODataConnection conn, GXODataClient client, ServiceCursorDef cursorDef, IDataParameterCollection parms, CommandBehavior behavior)
		{
			this.conn = conn;
			this.client = client;
			this.parms = parms;
			this.behavior = behavior;
			this.cursorDef = cursorDef;
			conn.State = ConnectionState.Executing;
#if NETCORE
			try
			{
				ODataQuery test = (ODataQuery)cursorDef.Query;				
				Console.WriteLine($"Test Passed!: {test}");
			}catch(Exception e)
			{
				Console.WriteLine("Test Failed: " + cursorDef.Query);
				Console.WriteLine($"ODataException: \n{e.Message}\n{e.StackTrace}");
			}
#endif
			ODataQuery query = cursorDef.Query as ODataQuery;
			action = query.query;
			selectList = query.selectList;
			allSelectedKeys = null;
			annotations = new ODataFeedAnnotations();
			try
			{
				switch (behavior)
				{
					case CommandBehavior.SingleRow:
						taskAwaiter = Task.Run(() => task = client.FindEntriesAsync(SingleRow(action(client, parms).GetCommandTextAsync().Result), annotations)).ConfigureAwait(false).GetAwaiter();
						break;
					default:
						taskAwaiter = Task.Run(() => task = client.FindEntriesAsync(action(client, parms).GetCommandTextAsync().Result, annotations)).ConfigureAwait(false).GetAwaiter();
						break;
				}
			}
			catch (AggregateException e)
			{
				throw ODataConnection.GetAggregateException(e);
			}

			(cursorDef.Parent as DataStoreHelperOData).CurrentOfManager.AddQuery(cursorDef.Name, this);
			IsClosed = false;
			data = null;
			currentEntry = null;
			currentRecord = null;
		}

		internal IDictionary<string, object> CurrentOf { get { return currentRecord; } }
		internal IDictionary<string, object> CurrentOfEntry { get { return currentEntry; } }

		private string SingleRow(string query)
		{
			return string.Join("&", query.Split(separator).Where(item => !item.StartsWith("$orderby")));
		}

		public bool NextResult()
		{
			throw new NotImplementedException();
		}

		private bool MoveNext()
		{
			if (flattenedData != null)
			{
				if (flattenedData.MoveNext())
				{
					currentEntry = flattenedData.Current;
					return true;
				}
				else
				{
					flattenedData.Dispose();
				}
			}
			if (data.MoveNext())
			{
				IDictionary<string, object> record = CleanRecord(data.Current);
				if (record == null || record.Count == 0)
					return MoveNext();
				flattenedData = FlattenRecords(record).GetEnumerator();
				return MoveNext();
			}
			else
			{
				if (flattenedData != null)
					flattenedData.Dispose();
				flattenedData = null;
				return false;
			}
		}

		private IDictionary<string, object> CleanRecord(IDictionary<string, object> record)
		{
			currentRecord = new Dictionary<string, object>(record);
			if (allSelectedKeys == null)
				allSelectedKeys = selectList.Select(selItem => selItem.GetName(NewServiceContext())).Distinct().ToArray();
			foreach (string key in record.Keys.ToArray())
			{
				string[] allKeys = RecordKeys(record, key).ToArray();
				if (!allSelectedKeys.Intersect(allKeys).Any())
					record.Remove(key);
				else if (allSelectedKeys.Contains(key))
				{ // If a field that is collection is selected but the entity is empty, the record is skipped
					if (record[key] is IEnumerable<object> entityList && !entityList.Any())
						return null;
				}
			}
			return record;
		}

		private IEnumerable<string> RecordKeys(IDictionary<string, object> record, string key)
		{
			yield return key;
			if (record[key] is IEnumerable<object> entityList && entityList.Any())
			{
				if (entityList.First() is IDictionary<string, object> entityItem)
				{
					foreach (string innerKey in entityItem.Keys.ToArray())
					{
						foreach (string subkey in RecordKeys(entityItem, innerKey))
							yield return subkey;
					}
				}
			}
		}

		private IEnumerable<IDictionary<string, object>> FlattenRecords(IDictionary<string, object> record)
		{
			DataStoreHelperOData.ODataMapCol mapColItem = null;
			record = FlattenRecord(record);
			if (record.Any(item => item.Value is IEnumerable<object> || item.Value is IEnumerable<IDictionary<string, object>>))
			{
				string entityKey = record.Keys.FirstOrDefault(key => record[key] is IEnumerable<IDictionary<string, object>>);
				if (entityKey != null)
				{
					IEnumerable<IDictionary<string, object>> entityList = record[entityKey] as IEnumerable<IDictionary<string, object>>;
					record.Remove(entityKey);
					if (entityList.Any())
					{
						if (NeedFlattenRecord(entityKey))
						{
							foreach (IDictionary<string, object> subRecord in entityList)
							{
								IDictionary<string, object> oneRecord = new Dictionary<string, object>(subRecord);
								foreach (string skey in record.Keys)
									if (!oneRecord.ContainsKey(skey))
										oneRecord.Add(skey, record[skey]);
									else Debug.Assert(oneRecord[skey] == null || record[skey] == null || oneRecord[skey].Equals(record[skey]), $"key already in dictionary: { skey } - value: { oneRecord[skey] } - ignoring: { record[skey] }");
								foreach (IDictionary<string, object> r in FlattenRecords(oneRecord))
									yield return r;
							}
						}
						else
						{
							foreach (IDictionary<string, object> subRecord in entityList)
							{
								IDictionary<string, object> oneRecord = new Dictionary<string, object>(record);
								oneRecord.Add(entityKey, subRecord);
								foreach (IDictionary<string, object> r in FlattenRecords(oneRecord))
									yield return r;
							}

						}
					}
					else yield return record;
				}
				else
				{
					entityKey = record.Keys.FirstOrDefault(key => record[key] is IEnumerable<object>);
					if (entityKey != null)
					{
						IEnumerable<object> memberCol = record[entityKey] as IEnumerable<object>;
						record.Remove(entityKey);
						if (memberCol.Any())
						{
							foreach (object memberItem in memberCol)
							{
								IDictionary<string, object> oneRecord = new Dictionary<string, object>(record);
								oneRecord.Add(new KeyValuePair<string, object>(entityKey, memberItem));
								foreach (IDictionary<string, object> r in FlattenRecords(oneRecord))
									yield return r;
							}
						}
						else yield return record;
					}
					else yield return record;
				}
			}
			else if (record.Any(item => item.Value is IDictionary<string, object> && (mapColItem = NeedFlattenMemberCol(item.Key, record)) != null))
			{
				IDictionary<string, object> entity = record[mapColItem.GetName(NewServiceContext())] as IDictionary<string, object>;
				string entityKey = mapColItem.GetKey(NewServiceContext());
				IEnumerable<object> memberCol = entity[entityKey] as IEnumerable<object>;
				if (memberCol.Any())
				{
					foreach (object memberItem in memberCol)
					{
						IDictionary<string, object> oneEntity = new Dictionary<string, object>(entity);
						oneEntity.Remove(entityKey);
						oneEntity.Add(entityKey, memberItem);

						IDictionary<string, object> oneRecord = new Dictionary<string, object>(record);
						string name = mapColItem.GetName(NewServiceContext());
						oneRecord.Remove(name);
						oneRecord.Add(name, oneEntity);
						foreach (IDictionary<string, object> r in FlattenRecords(oneRecord))
							yield return r;
					}
				}
				else
				{
					entity.Remove(entityKey);
					yield return record;
				}
			}
			else yield return record;
		}

		private bool NeedFlattenRecord(string entityKey)
		{
			return !selectList.Any(selItem => selItem is DataStoreHelperOData.ODataMapExt && (selItem as DataStoreHelperOData.ODataMapExt).GetName(NewServiceContext()) == entityKey);
		}

		private DataStoreHelperOData.ODataMapCol NeedFlattenMemberCol(string entityKey, IDictionary<string, object> record)
		{
			return selectList.FirstOrDefault(selItem => selItem is DataStoreHelperOData.ODataMapCol &&
											(selItem as DataStoreHelperOData.ODataMapCol).GetName(NewServiceContext()) == entityKey &&
											(record[entityKey] as IDictionary<string, object>).ContainsKey((selItem as DataStoreHelperOData.ODataMapCol).GetKey(NewServiceContext())) &&
											(record[entityKey] as IDictionary<string, object>)[((selItem as DataStoreHelperOData.ODataMapCol).GetKey(NewServiceContext()))] is IEnumerable<object>) as DataStoreHelperOData.ODataMapCol;
		}

		private IDictionary<string, object> FlattenRecord(IDictionary<string, object> record)
		{

			while (record.Any(item => item.Value is IDictionary<string, object> && NeedFlattenRecord(item.Key)))
			{
				string entityKey = record.Keys.First(key => record[key] is IDictionary<string, object> && NeedFlattenRecord(key));
				IDictionary<string, object> entity = record[entityKey] as IDictionary<string, object>;
				record.Remove(entityKey);
				foreach (string subKey in entity.Keys)
				{
					try
					{
						record.Add(subKey, entity[subKey]);
					}
					catch (ArgumentException)
					{

					}
				}
			}
			return record;
		}

		internal IOServiceContext NewServiceContext()
		{
			return new ODataServiceContext(client);
		}

		public bool Read()
		{
			while (!IsClosed)
			{
				conn.State = ConnectionState.Fetching;
				if (data == null)
				{
					try
					{
						data = taskAwaiter.GetResult()?.GetEnumerator();
					}
					catch (AggregateException e)
					{
						throw ODataConnection.GetAggregateException(e);
					}
					catch (WebRequestException e)
					{
						Exception toThrow = conn.GetWebRequestException(e);
						if (behavior == CommandBehavior.SingleRow && IsRecordNotFoundException(toThrow))
							return false;
						else throw toThrow;
					}
					if (data == null)
					{
						Close();
						break;
					}
				}
				if (MoveNext())
				{
					return true;
				}
				else if (annotations.NextPageLink != null)
				{
					data.Dispose();
					data = null;
					if (flattenedData != null)
					{
						flattenedData.Dispose();
						flattenedData = null;
					}
					task.Dispose();
					try
					{
						taskAwaiter = Task.Run(() => task = client.FindEntriesAsync(annotations.NextPageLink.AbsoluteUri, annotations)).ConfigureAwait(false).GetAwaiter();
					}
					catch (AggregateException e)
					{
						throw new GxADODataException(e.Flatten().InnerException);
					}
					continue;
				}
				else
				{
					Close();
					break;
				}
			}
			return false;
		}

		private bool IsRecordNotFoundException(Exception e) => (e as AggregateException)?.Message == ServiceError.RecordNotFound;

		public object this[string name]
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public object this[int i]
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Depth
		{
			get
			{
				return 0;
			}
		}

		public int FieldCount
		{
			get
			{
				return currentEntry != null ? selectList.Length : 0;
			}
		}

		public bool IsClosed { get; internal set; }

		public int RecordsAffected
		{
			get
			{
				return -1;
			}
		}

		public void Close()
		{
			if (IsClosed)
				return;
			(cursorDef.Parent as DataStoreHelperOData).CurrentOfManager.RemoveQuery(cursorDef.Name);
			IsClosed = true;
			annotations = null;
			Debug.Assert(task != null && (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted));
			if (task != null &&
				(task.Status == TaskStatus.RanToCompletion ||
				task.Status == TaskStatus.Faulted ||
				task.Status == TaskStatus.Canceled))
				task.Dispose();
			if (conn.State != ConnectionState.Closed)
				conn.State = ConnectionState.Open;
		}

		public void Dispose()
		{
			Close();
		}

		public bool GetBoolean(int i)
		{
			return (bool)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public byte GetByte(int i)
		{
			return (byte)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			byte[] bytes = selectList[i].GetValue(NewServiceContext(), currentEntry) as byte[];
			Debug.Assert(bytes != null && fieldOffset <= bytes.Length);
			long len = bytes.Length - fieldOffset;
			if (len > length)
				len = length;
			Array.Copy(bytes, fieldOffset, buffer, bufferoffset, len);
			return len;
		}

		public char GetChar(int i)
		{
			return (char)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public DateTime GetDateTime(int i)
		{
			object obj = selectList[i].GetValue(NewServiceContext(), currentEntry);
			if (obj is DateTime dt)
				return dt;
			else if (obj is DateTimeOffset dto)
				return dto.UtcDateTime;
			else if (obj is TimeOfDay timeOfDay)
				return new DateTime(timeOfDay.Ticks);
			else throw new NotImplementedException();
		}

		public decimal GetDecimal(int i)
		{
			return (decimal)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public double GetDouble(int i)
		{
			return (double)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public Type GetFieldType(int i)
		{
			return selectList[i].GetValue(NewServiceContext(), currentEntry).GetType();
		}

		public float GetFloat(int i)
		{
			return (float)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public Guid GetGuid(int i)
		{
			return (Guid)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public short GetInt16(int i)
		{
			return (short)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public int GetInt32(int i)
		{
			return (int)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public long GetInt64(int i)
		{
			return (long)selectList[i].GetValue(NewServiceContext(), currentEntry);
		}

		public string GetName(int i)
		{
			return selectList[i].GetName(NewServiceContext());
		}

		public int GetOrdinal(string name)
		{
			for (int idx = 0; idx < selectList.Length; idx++)
				if (selectList[idx].GetName(NewServiceContext()).Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return idx;
			throw new IndexOutOfRangeException(string.Format("Field {0} not found", name));
		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			return selectList[i].GetValue(NewServiceContext(), currentEntry) as string;
		}

		public object GetValue(int i)
		{
			object value = selectList[i].GetValue(NewServiceContext(), currentEntry);
			if (value is Geography geoValue)
			{
				string geoStr = WellKnownTextSqlFormatter.Create().Write(geoValue);
				geoStr = geoStr.Substring(geoStr.IndexOf(';') + 1);
				return geoStr;
			}else if (value is DateTimeOffset dto)
				return dto.UtcDateTime;
			else if (value is TimeOfDay timeOfDay)
				return new DateTime(timeOfDay.Ticks);
			else if (value is TimeSpan ts)
				return DateTime.MinValue.Add(ts);
			return value;
		}

		public int GetValues(object[] values)
		{
			int count = Math.Min(values.Length, selectList.Length);
			for (int idx = 0; idx < count; idx++)
			{
				try
				{
					values[idx] = GetValue(idx) ?? DBNull.Value;
				}
				catch (KeyNotFoundException)
				{
					values[idx] = DBNull.Value;
				}
			}
			return count;
		}

		public bool IsDBNull(int i)
		{
			bool isNull = !currentEntry.ContainsKey(selectList[i].GetName(NewServiceContext())) || selectList[i].GetValue(NewServiceContext(), currentEntry) == null;
			if (!isNull && selectList[i] is DataStoreHelperOData.ODataMapCol)
			{
				isNull = !(currentEntry[selectList[i].GetName(NewServiceContext())] as IDictionary<string, object>).ContainsKey((selectList[i] as DataStoreHelperOData.ODataMapCol).GetKey(NewServiceContext()));
			}
			return isNull;
		}
	}

	internal class ODataDbCommand : ServiceCommand
	{
		internal ODataDbCommand(IDbConnection conn) : base(conn)
		{
		}
	}

	public class QueryExpression
	{
		public string For { get; set; }
		public string[] Select { get; set; }
	}

	public class DataStoreHelperOData : DynServiceDataStoreHelper
	{
		public char[] likeChars = new char[] { '%' };
		public static dynamic DynExp = ODataDynamic.Expression;

		public CurrentOfManager CurrentOfManager { get; internal set; } = new CurrentOfManager();

		public override Guid GetParmGuid(IDataParameterCollection parms, string parm)
		{
			Guid.TryParse(GetParmObj(parms, parm) as string, out Guid guidValue);
			return guidValue;
		}

		public override string GetParmStr(IDataParameterCollection parms, string parm)
		{
			return GetParmObj(parms, parm) as string;
		}

		public override int GetParmInt(IDataParameterCollection parms, string parm)
		{
			return Int32.TryParse(GetParmObj(parms, parm) as string, out int res) ? res : 0;
		}

		public override decimal GetParmFP(IDataParameterCollection parms, string parm)
		{
			return Decimal.TryParse(GetParmObj(parms, parm) as string, out decimal res) ? res : 0M;
		}

		public override DateTime GetParmDate(IDataParameterCollection parms, string parm)
		{
			return DateTime.TryParse(GetParmObj(parms, parm) as string, out DateTime res) ? res : DateTime.MinValue;
		}

		public override TimeSpan GetParmTime(IDataParameterCollection parms, string parm)
		{
			return TimeSpan.TryParse(GetParmObj(parms, parm) as string, out TimeSpan res) ? res : TimeSpan.Zero;
		}

		public override object GetParmObj(IDataParameterCollection parms, string parm)
		{
			GetParmObj(parms, parm, out object value);
			return value;
		}

		internal bool GetParmObj(IDataParameterCollection parms, string parm, out object value)
		{
			for (int idx = 0; idx < parms.Count; idx++)
			{
				if (parms[idx] is IDataParameter sParm && sParm.ParameterName.Equals(parm))
				{
					if (sParm.Value is DBNull)
					{
						value = null;
						return false;
					}
					string parmType = sParm.Value?.GetType().ToString();
					if (parmType == "Microsoft.SqlServer.Types.SqlGeography" || parmType == "NetTopologySuite.Geometries.Point")
					{
						//must use Convertible because the object WellKnownTextSqlFormatter returns does not implement IConvertible and SimpleOData does not take it as "IsAssignableFrom"
						string geoStr = sParm.Value.ToString();
						if (string.Equals(geoStr, "GEOMETRYCOLLECTION EMPTY"))
							geoStr = "POINT(0.0 0.0)";
						value = new Convertible(WellKnownTextSqlFormatter.Create().Read<Geography>(new StringReader(geoStr)));
						return true;
					}
					string parmValue = Convert.ToString(sParm.Value);
					if (parm.StartsWith("l"))
					{
						parmValue = parmValue.TrimEnd(likeChars);
					}
					value = parmValue;
					return true;
				}
			}
			Debug.Assert(false, string.Format("Unknown parameter: {0}", parm));
			throw new GxADODataException(string.Format("Unknown parameter: {0}", parm));
		}

		public override Guid? GetParmUGuid(IDataParameterCollection parms, string parm)
		{
			if (!GetParmObj(parms, parm, out object value))
				return null;
			Guid.TryParse(value as string, out Guid guidValue);
			return guidValue;
		}

		public override int? GetParmUInt(IDataParameterCollection parms, string parm)
		{
			if (!GetParmObj(parms, parm, out object value))
				return null;
			return Int32.TryParse(value as string, out int res) ? res : 0;
		}

		public override decimal? GetParmUFP(IDataParameterCollection parms, string parm)
		{
			if (!GetParmObj(parms, parm, out object value))
				return null;
			return Decimal.TryParse(value as string, out decimal res) ? res : 0M;
		}

		public override DateTime? GetParmUDate(IDataParameterCollection parms, string parm)
		{
			if (!GetParmObj(parms, parm, out object value))
				return null;
			return DateTime.TryParse(value as string, out DateTime res) ? res : DateTime.MinValue;
		}

		public override TimeSpan? GetParmUTime(IDataParameterCollection parms, string parm)
		{
			if (!GetParmObj(parms, parm, out object value))
				return null;
			return TimeSpan.TryParse(value as string, out TimeSpan res) ? res : TimeSpan.Zero;
		}

		public override DateTime? GetParmUDateTime(IDataParameterCollection parms, string parm)
		{
			return GetParmUDate(parms, parm);
		}


		public ODataQuery GetQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, IODataMap[] selectList)
		{
#if NETCORE
			ODataQuery oquery = new ODataQuery(query, selectList);
			Console.WriteLine($"GetQuery: {oquery}");
			return oquery;
#else
			return new ODataQuery(query, selectList);
#endif
		}

		public class Query
		{
			private object mDataStoreHelper;

			public string TableName { get; set; } = String.Empty;
			public string[] Projection { get; set; } = Array.Empty<string>();
			public string[] OrderBys { get; set; } = Array.Empty<string>();
			public string[] Filters { get; set; } = Array.Empty<string>();

			public Query(object dataStoreHelper)
			{
				mDataStoreHelper = dataStoreHelper;
			}
			public Query For(string v)
			{
				TableName = v;
				return this;
			}

			public Query Select(string[] columns)
			{
				Projection = columns;
				return this;
			}
			public Query OrderBy(string[] orders)
			{
				OrderBys = orders;
				return this;
			}

			public Query Filter(string[] filters)
			{
				Filters = filters;
				return this;
			}

			public Query SetMaps(IODataMap[] iODataMap)
			{
				return this;
			}
		}

		public ODataQuery GetQueryCommand(Func<FluentCommand, IDataParameterCollection, FluentCommand> query, IODataMap[] selectList)
		{
			return new ODataQuery(query, selectList);
		}

		public ODataQuery GetQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, string queryType)
		{
			return new ODataQuery(query, queryType);
		}

		public ODataQuery GetQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, ODataQuery continuation)
		{
			return new ODataQuery(query, continuation);
		}

		public ODataQuery GetQuery(Func<GXODataClient, IDataParameterCollection, Task<ODataEntry>, GXODataClient> query, string queryType)
		{
			return new ODataQuery(query, queryType);
		}

		public ODataQuery GetQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, ODataQuery[] updates, string queryType)
		{
			return new ODataQuery(query, updates, queryType);
		}

		public ODataQuery GetQuery(Func<GXODataClient, IDataParameterCollection, Task<ODataEntry>, GXODataClient> query, string entity, Func<IDataParameterCollection, IDictionary<string, object>> setEntity)
		{
			return new ODataQuery(query, entity, setEntity);
		}

		public IDictionary<string, object> Entity { get { return new Dictionary<string, object>(); } }

		public IDictionary<string, object> Set(Task<ODataEntry> task, string baseEntity, IDictionary<string, object> setEntity)
		{
			if (task.Result == null)
				throw new ArgumentNullException("Key not found");
			IDictionary<string, object> currentEntity = task.Result.AsDictionary();
			if (currentEntity.ContainsKey(baseEntity))
			{
				object baseEntityObj = currentEntity[baseEntity];
				if (baseEntityObj is IList<object>)
				{

					IList<object> baseEntityList = baseEntityObj as IList<object>;
					if (baseEntityList.Any(entity => IsSameEntity(entity, setEntity, false)))
						throw new ServiceException(ServiceError.RecordAlreadyExists);
					else baseEntityList.Add(setEntity);
				}
				else
				{
					currentEntity.Remove(baseEntity);
					currentEntity.Add(baseEntity, setEntity);
				}
			}
			else currentEntity.Add(baseEntity, setEntity);
			return currentEntity;
		}

		public IDictionary<string, object> Remove(Task<ODataEntry> task, string baseEntity, IDictionary<string, object> setEntity)
		{
			if (task.Result == null)
				throw new ArgumentNullException("Key not found");
			IDictionary<string, object> currentEntity = task.Result.AsDictionary();
			bool found = false;
			if (currentEntity.ContainsKey(baseEntity))
			{
				object baseEntityObj = currentEntity[baseEntity];
				if (baseEntityObj is IList<object>)
				{
					IList<object> baseEntityList = baseEntityObj as IList<object>;
					for (int idx = 0; idx < baseEntityList.Count; idx++)
					{
						if (IsSameEntity(baseEntityList[idx], setEntity, false))
						{
							found = true;
							baseEntityList.RemoveAt(idx);
							break;
						}
					}
				}
				else
				{
					if (IsSameEntity(currentEntity, setEntity, false))
						found = currentEntity.Remove(baseEntity);
				}
			}
			if (!found)
			{
				ArgumentNullException nullExc = new ArgumentNullException(baseEntity, "Key not found");
				throw new AggregateException(new Exception[] { nullExc });
			}
			return currentEntity;

		}

		private bool IsSameEntity(object entry, object setEntity, bool fullCheck)
		{
			if (entry is DynamicODataEntry && setEntity is IDictionary<string, object>)
			{
				IDictionary<string, object> entryDict = (entry as DynamicODataEntry).AsDictionary();
				IDictionary<string, object> setEntityDict = setEntity as IDictionary<string, object>;
				if (fullCheck)
					return (entryDict.Count == setEntityDict.Count) &&
						entryDict.All(kvPair => setEntityDict.ContainsKey(kvPair.Key) && IsSameEntity(kvPair.Value, setEntityDict[kvPair.Key], fullCheck));
				else
					return (entryDict.Count >= setEntityDict.Count) &&
						entryDict.All(kvPair => !setEntityDict.ContainsKey(kvPair.Key) || IsSameEntity(kvPair.Value, setEntityDict[kvPair.Key], fullCheck));

			}
			else if (entry is IList<object> && setEntity is IList<object>)
			{
				IList<object> entryList = entry as IList<object>;
				IList<object> setEntityList = setEntity as IList<object>;
				return (entryList.Count == setEntityList.Count) &&
					entryList.Zip(setEntityList, (left, right) => IsSameEntity(left, right, fullCheck)).All(item => item);
			}
			else return entry.Equals(setEntity);
		}

		public IDictionary<string, object> Set(Task<ODataEntry> task, string key, object parmValue)
		{
			if (task.Result == null)
				throw new ArgumentNullException("Key not found");
			return task.Result.AsDictionary().Set(key, parmValue);
		}

		public CurrentOf CurrentOf(string CursorName)
		{
			return new CurrentOf(CurrentOfManager, CursorName);
		}

		public IDictionary<string, object> Remove(Task<ODataEntry> task, string key, object parmValue)
		{
			if (task.Result == null)
				throw new ArgumentNullException("Key not found");
			return task.Result.AsDictionary().Remove(key, parmValue);
		}

		public ODataMapName Map(string name)
		{
			return new ODataMapName(name);
		}

		public ODataMapDomain MapDomain(Type domain, string name)
		{
			return new ODataMapDomain(domain, name);
		}

		public string MapDomainName(Type domainType, int? value)
		{
			if (value == null)
				value = 0;
			MethodInfo domain = domainType.GetMethod("getDescription", BindingFlags.Public | BindingFlags.Static);
			return domain.Invoke(null, new object[] { null, (long)value }) as string;
		}

		public string MapDomainName(Type domainType, string sValue)
		{
			if (!Int32.TryParse(sValue, out int value))
				value = 0;
			return MapDomainName(domainType, value);
		}

		public ODataMapExt Ext(string entity, IODataMap map)
		{
			return new ODataMapExt(entity, map);
		}

		public ODataMapExt Ext(string entity, string name)
		{
			return new ODataMapExt(entity, Map(name));
		}

		public ODataMapCol MapCol(string entity, IODataMap map)
		{
			return new ODataMapCol(entity, map);
		}

		public ODataMapCol MapCol(string entity, string name)
		{
			return new ODataMapCol(entity, Map(name));
		}

		public class ODataMapExt : IODataMap
		{
			string entity;
			protected IODataMap map;
			public ODataMapExt(string entity, IODataMap map)
			{
				this.entity = entity;
				this.map = map;
			}

			public virtual object GetValue(IOServiceContext context, IDictionary<string, object> currentEntry)
			{
				string key = context.Entity(entity) as string;
				if (currentEntry.ContainsKey(key))
				{
					object currentEntryObj = currentEntry[key];
					if (currentEntryObj is IDictionary<string, object> currentEntryExt)
						return map.GetValue(context, currentEntryExt);
					else if (currentEntryObj is IEnumerable<IDictionary<string, object>> currentEntryCol && currentEntryCol.Any())
						return map.GetValue(context, currentEntryCol.First()); // If the server returned a collection for this entity, return data from the first item (SapB1)
				}
				return null;
			}

			public virtual void SetValue(IDictionary<string, object> currentEntry, object value)
			{
				if (!currentEntry.ContainsKey(entity))
					currentEntry.Add(entity, new Dictionary<string, object>());
				map.SetValue(currentEntry[entity] as IDictionary<string, object>, value);
			}

			public string GetName(IOServiceContext context)
			{
				return context.Entity(entity) as string;
			}
		}

		public class ODataMapCol : ODataMapExt
		{
			public ODataMapCol(string entity, IODataMap map) : base(entity, map)
			{
			}

			public string GetKey(IOServiceContext context)
			{
				return map.GetName(context);
			}
		}

		public class ODataMapName : ServiceMapName
		{
			public ODataMapName(string name) : base(name)
			{

			}
		}

		public class ODataMapDomain : ODataMapName
		{
			private MethodInfo domain;

			public ODataMapDomain(Type domainType, string name) : base(name)
			{
				domain = domainType.GetMethod("getValue", BindingFlags.Public | BindingFlags.Static);
			}

			public override object GetValue(IOServiceContext context, IDictionary<string, object> currentEntry)
			{
				object value = base.GetValue(context, currentEntry);
				return value != null ? domain.Invoke(null, new object[] { value }) : value;
			}
		}
	}

	internal class ODataServiceContext : IOServiceContext
	{
		GXODataClient client;
		string baseEntity;
		public ODataServiceContext(GXODataClient client)
		{
			this.client = client;
			baseEntity = client.BaseEntity;
		}
		public object Entity(string entity)
		{
			return client.Entity(baseEntity, entity);
		}
	}

	public class ODataQuery
	{
		internal Func<GXODataClient, IDataParameterCollection, GXODataClient> query;
		internal IODataMap[] selectList;
		internal ServiceCursorDef.CursorType cursorType;
		internal Func<GXODataClient, IDataParameterCollection, Task<ODataEntry>, GXODataClient> queryWithCont;
		internal ODataQuery continuation;
		internal ODataQuery[] updates;
		private string queryType;
		internal string entity;
		internal Func<IDataParameterCollection, IDictionary<string, object>> setEntity;
		internal Func<FluentCommand, IDataParameterCollection, FluentCommand> command;

		public ODataQuery(Func<FluentCommand, IDataParameterCollection, FluentCommand> command, IODataMap[] selectList)
		{
			this.selectList = selectList;
			this.command = command;
			cursorType = ServiceCursorDef.CursorType.Select;
		}

		public ODataQuery(Func<GXODataClient, IDataParameterCollection, Task<ODataEntry>, GXODataClient> queryWithCont, string sCursorType) : this((Func<GXODataClient, IDataParameterCollection, GXODataClient>)null, sCursorType)
		{
			this.queryWithCont = queryWithCont;
			this.query = null;
		}

		public ODataQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, ODataQuery continuation) : this(query, "CONT")
		{
			this.continuation = continuation;
		}

		public ODataQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, IODataMap[] selectList)
		{
			this.query = query;
			this.selectList = selectList;
			cursorType = ServiceCursorDef.CursorType.Select;
		}

		public ODataQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, string sCursorType) : this(query, (IODataMap[])null, sCursorType)
		{
		}

		public ODataQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, IODataMap[] selectList, string sCursorType)
		{
			this.query = query;
			this.selectList = selectList;
			SetQueryType(sCursorType);
		}

		private void SetQueryType(string sCursorType)
		{
			this.queryType = sCursorType;
			switch (sCursorType)
			{
				case "INS": cursorType = ServiceCursorDef.CursorType.Insert; break;
				case "UPD": cursorType = ServiceCursorDef.CursorType.Update; break;
				case "DLT": cursorType = ServiceCursorDef.CursorType.Delete; break;
				case "CONT": cursorType = ServiceCursorDef.CursorType.Select; break;
				default: cursorType = ServiceCursorDef.CursorType.Select; break;
			}
		}

		public ODataQuery(Func<GXODataClient, IDataParameterCollection, GXODataClient> query, ODataQuery[] updates, string queryType)
		{
			this.query = query;
			this.queryWithCont = null;
			this.updates = updates;
			SetQueryType(queryType);
		}

		public ODataQuery(Func<GXODataClient, IDataParameterCollection, Task<ODataEntry>, GXODataClient> query, string entity, Func<IDataParameterCollection, IDictionary<string, object>> setEntity)
		{
			this.queryWithCont = query;
			this.query = null;
			this.entity = entity;
			this.setEntity = setEntity;
			SetQueryType("UPD");
		}
	}

	public class GXODataExpression : ODataExpression
	{
		public GXODataExpression(string reference, object value) : base(reference, value)
		{
		}
	}

	public class GXODataClientSettings
	{
		internal GXODataClientSettings(ODataClientSettings settings, bool allowSelectOnExpand)
		{
			this.settings = settings;
			this.allowSelectOnExpand = allowSelectOnExpand;
		}
		internal ODataClientSettings settings { get; set; }
		internal bool allowSelectOnExpand { get; set; }
	}

	public class GXODataClient
	{
		static IDictionary<string, IDictionary<string, IDictionary<string, string>>> entityMappers = new Dictionary<string, IDictionary<string, IDictionary<string, string>>>();
		static IDictionary<string, IDictionary<string, string>> rootMappers = new Dictionary<string, IDictionary<string, string>>();
		ODataClientSettings settings;
		GXODataClientSettings gxSettings;
		public GXODataClient(GXODataClientSettings gxSettings)
		{
			this.gxSettings = gxSettings;
			settings = gxSettings.settings;
			Client = new ODataClient(settings);
			InitializeEntityMapper();
		}

		IDictionary<string, IDictionary<string, string>> entityMapper;
		IDictionary<string, string> rootMapper;
		private void InitializeEntityMapper()
		{
			if (entityMappers.TryGetValue(settings.BaseUri.AbsoluteUri, out entityMapper))
			{
				rootMapper = rootMappers[settings.BaseUri.AbsoluteUri];
				return;
			}
			IODataAdapter Adapter = GetAdapter();
			string metadata = settings.MetadataDocument;
			entityMapper = new Dictionary<string, IDictionary<string, string>>();
			entityMappers.Add(settings.BaseUri.AbsoluteUri, entityMapper);
			rootMapper = new Dictionary<string, string>();
			rootMappers.Add(settings.BaseUri.AbsoluteUri, rootMapper);

			if (metadata == null)
			{
				metadata = Task.Run(() => (Client.GetMetadataAsStringAsync())).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			switch (Adapter.AdapterVersion)
			{
				case AdapterVersion.V4:
					{
						Microsoft.OData.Edm.IEdmModel model = Adapter.Model as Microsoft.OData.Edm.IEdmModel;
						Microsoft.OData.Edm.IEdmEntityContainer EntityContainer = model.EntityContainer;

						IDictionary<object, IList<string>> entitySetTypes = new Dictionary<object, IList<string>>();
						foreach (Microsoft.OData.Edm.IEdmEntitySet entitySet in model.EntityContainer.EntitySets())
						{
							if (!entitySetTypes.TryGetValue(entitySet.EntityType(), out IList<string> entitySets))
							{
								entitySets = new List<string>();
								entitySetTypes.Add(entitySet.EntityType(), entitySets);
							}
							entitySets.Add(entitySet.Name);
						}
						foreach (Microsoft.OData.Edm.IEdmEntitySet entitySet in model.EntityContainer.EntitySets())
						{
							Microsoft.OData.Edm.IEdmEntityType type = entitySet.EntityType();
							InitializeEntityMapperV4(entityMapper, type, entitySetTypes);
							rootMapper.Add(entitySet.Name, type.Name);
						}
					}
					break;
				case AdapterVersion.V3:
				default:
					{
						Microsoft.Data.Edm.IEdmModel model = Adapter.Model as Microsoft.Data.Edm.IEdmModel;
						IDictionary<object, IList<string>> entitySetTypes = new Dictionary<object, IList<string>>();
						foreach (Microsoft.Data.Edm.IEdmEntitySet entitySet in model.EntityContainers().SelectMany(entityContainer => entityContainer.EntitySets()))
						{
							if (!entitySetTypes.TryGetValue(entitySet.ElementType, out IList<string> entitySets))
							{
								entitySets = new List<string>();
								entitySetTypes.Add(entitySet.ElementType, entitySets);
							}
							entitySets.Add(entitySet.Name);
						}
						foreach (Microsoft.Data.Edm.IEdmEntitySet entitySet in model.EntityContainers().SelectMany(entityContainer => entityContainer.EntitySets()))
						{
							Microsoft.Data.Edm.IEdmEntityType type = entitySet.ElementType;
							InitializeEntityMapperV3(entityMapper, type, entitySetTypes);
							rootMapper.Add(entitySet.Name, type.Name);
						}
					}
					break;
			}
		}

		public void InitializeEntityMapperV4(IDictionary<string, IDictionary<string, string>> entityMapper, Microsoft.OData.Edm.IEdmEntityType type, IDictionary<object, IList<string>> entitySetTypesMap)
		{
			if (!entityMapper.TryGetValue(type.Name, out IDictionary<string, string> currentMapper))
			{
				currentMapper = new Dictionary<string, string>();
				entityMapper.Add(type.Name, currentMapper);
			}
			foreach (Microsoft.OData.Edm.IEdmNavigationProperty navProp in type.NavigationProperties())
			{
				string navPropName = navProp.Name;
				Microsoft.OData.Edm.IEdmEntityType navPropType = navProp.ToEntityType();
				if (!currentMapper.ContainsKey(navPropName))
				{
					currentMapper.Add(navPropName, navPropName);
					if (entitySetTypesMap.ContainsKey(navPropType))
					{
						foreach (string entitySetName in entitySetTypesMap[navPropType].Where(name => !currentMapper.ContainsKey(name)))
							currentMapper.Add(entitySetName, navPropName);
					}
					else
					{
						if (!currentMapper.ContainsKey(navPropType.Name))
							currentMapper.Add(navPropType.Name, navPropName);
					}
					if (!navPropType.Equals(type))
						InitializeEntityMapperV4(entityMapper, navPropType, entitySetTypesMap);
				}
			}
		}

		public void InitializeEntityMapperV3(IDictionary<string, IDictionary<string, string>> entityMapper, Microsoft.Data.Edm.IEdmEntityType type, IDictionary<object, IList<string>> entitySetTypesMap)
		{
			if (!entityMapper.TryGetValue(type.Name, out IDictionary<string, string> currentMapper))
			{
				currentMapper = new Dictionary<string, string>();
				entityMapper.Add(type.Name, currentMapper);
			}
			foreach (Microsoft.Data.Edm.IEdmNavigationProperty navProp in type.NavigationProperties())
			{
				string navPropName = navProp.Name;
				Microsoft.Data.Edm.IEdmEntityType navPropType = navProp.ToEntityType();
				if (!currentMapper.ContainsKey(navPropName))
				{
					currentMapper.Add(navPropName, navPropName);
					if (entitySetTypesMap.ContainsKey(navPropType))
					{
						foreach (string entitySetName in entitySetTypesMap[navPropType].Where(name => !currentMapper.ContainsKey(name)))
							currentMapper.Add(entitySetName, navPropName);
					}
					else
					{
						if (!currentMapper.ContainsKey(navPropType.Name))
							currentMapper.Add(navPropType.Name, navPropName);
					}
					if (!navPropType.Equals(type))
						InitializeEntityMapperV3(entityMapper, navPropType, entitySetTypesMap);
				}
			}
		}

		private IODataAdapter GetAdapter()
		{
			ISession session = Session;
			if (session != null)
			{
				Task.Run(() => (session.GetType().GetTypeInfo().GetDeclaredMethod("ResolveAdapterAsync").Invoke(session, new object[] { null }) as Task)).ConfigureAwait(false).GetAwaiter().GetResult();
				return session.Adapter;
			}
			return null;
		}

		internal ISession Session { get { return Client.GetType().GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Client) as ISession; } }

		public ODataClient Client { get; internal set; }
		public IBoundClient<ODataEntry> BoundClient { get; internal set; }

		internal string BaseEntity;

		public GXODataClient For(ODataExpression expression)
		{
			BaseEntity = Entity(expression.Reference);
			BoundClient = ((IODataClient)Client).For(expression);
			return this;
		}

		internal string Entity(string name)
		{
			return rootMapper.ContainsKey(name) ? rootMapper[name] : name;
		}

		internal string Entity(string fromEntity, string name)
		{
			if (fromEntity == null)
				return Entity(name);
			if (entityMapper.ContainsKey(fromEntity) && entityMapper[fromEntity].TryGetValue(name, out string entityName))
			{
				return entityName;
			}
			else return name;
		}

		private GXODataClient Apply(IBoundClient<ODataEntry> boundClient)
		{
			BoundClient = boundClient;
			return this;
		}

		public GXODataClient Select(params ODataExpression[] columns)
		{
			string[] select = ApplySelectMappings(columns, 1);
			if (select.Length != columns.Length && !gxSettings.allowSelectOnExpand)
			{
				string[] entititesToFullySelect =
							columns
								.Select(reference => reference.Reference.Split(separator))
								.Where(sReferences => sReferences.Length != 1)
								.Select(sReferences => $"{ this.Entity(BaseEntity, sReferences.FirstOrDefault()) }~{ String.Join("/", sReferences.Skip(1).ToArray()) }")
								.ToArray();

				return Apply(BoundClient.Select(select).Select(entititesToFullySelect));
			}else return Apply(BoundClient.Select(select));
		}

		public GXODataClient Select(Object[] columns)
		{
			return Select(columns.OfType<ODataExpression>().ToArray());
		}

		public GXODataClient Expand(params ODataExpression[] associations)
		{
			return Apply(BoundClient.ExpandMap(ApplyEntityMappings(associations)));
		}

		private static char[] separator = new char[] { '/' };
		private static string strSeparator = "/";
		private string[] ApplySelectMappings(ODataExpression[] references, int minus)
		{
			return references
					.Select(reference => reference.Reference.Split(separator))
					.Where(sReferences => gxSettings.allowSelectOnExpand || sReferences.Length == minus)
					.Select(sReferences => ApplyMapping(BaseEntity, sReferences, minus))
					.ToArray();
		}

		private KeyValuePair<string, string>[] ApplyEntityMappings(ODataExpression[] references)
		{
			return references
					.Select(reference => new KeyValuePair<string, string>(reference.Reference, ApplyMapping(BaseEntity, reference.Reference.Split(separator), 0)))
					.ToArray();
		}

		private string ApplyMapping(string Entity, string[] references, int minus)
		{
			for (int idx = 0; idx < references.Length - minus; idx++)
			{
				string reference = references[idx];
				references[idx] = this.Entity(Entity, reference);
			}
			return string.Join(strSeparator, references);
		}

		private ODataExpression[] ApplyMappingsEx(ODataExpression[] references, int minus)
		{
			return references.Select(reference => ApplyMapping(BaseEntity, reference, minus)).ToArray();
		}

		private ODataExpression ApplyMapping(string Entity, ODataExpression reference, int minus)
		{
			string[] references = reference.Reference.Split(separator);
			bool changes = false;
			for (int idx = 0; idx < references.Length - minus; idx++)
			{
				string currentReference = references[idx];
				references[idx] = this.Entity(Entity, currentReference);
				changes |= !reference.Equals(references[idx]);
			}
			return changes ? new GXODataExpression(string.Join(strSeparator, references), reference.Value) : reference;
		}
		public GXODataClient Set(CurrentOf CurrentOf)
		{
			IDictionary<string, object> entity = CurrentOf.End();
			return Apply(BoundClient.Key(entity).Set(entity));
		}

		public GXODataClient OrderBy(params ODataExpression[] columns)
		{
			return Apply(BoundClient.OrderBy(ApplyMappingsEx(columns, 0)));
		}

		public GXODataClient OrderByDescending(params ODataExpression[] columns)
		{
			return Apply(BoundClient.OrderByDescending(ApplyMappingsEx(columns, 0)));
		}

		public GXODataClient ThenBy(params ODataExpression[] columns)
		{
			return Apply(((dynamic)BoundClient).ThenBy(ApplyMappingsEx(columns, 0)));
		}

		public GXODataClient ThenByDescending(params ODataExpression[] columns)
		{
			return Apply(((dynamic)BoundClient).ThenByDescending(ApplyMappingsEx(columns, 0)));
		}

		public GXODataClient Filter(ODataExpression expression)
		{
			try
			{ // In the conditions of type Att = parmStr it does a parmStr.rtrim in order to support 
			  // FK from an SQL table to an OData entity. In the SQL table the FK is left with spaces at the end.
			  // de tener una FK desde una tabla SQL hacia una entidad OData. En la tabla SQL la FK queda con espacios al final
				if (GetFieldValue<ExpressionType>(expression, "_operator").Equals(ExpressionType.Equal) &&
				   !string.IsNullOrEmpty(GetPropertyValue<string>(GetFieldValue<ODataExpression>(expression, "_left"), "Reference")))
				{
					ODataExpression right = GetFieldValue<ODataExpression>(expression, "_right");
					if (!object.ReferenceEquals(right, null))
					{
						string value = GetPropertyValue<string>(right, "Value");
						if (value != null)
						{
							SetPropertyValue<string>(right, "Value", value.TrimEnd(' '));
						}
					}
				}
			}
			catch { }

			return Apply(BoundClient.Filter(expression));
		}

		public T GetFieldValue<T>(object obj, string name)
		{
			if (obj == null)
				return default(T);
			var field = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			return field != null ? (T)field.GetValue(obj) : default(T);
		}

		public T GetPropertyValue<T>(object obj, string name)
		{
			if (obj == null)
				return default(T);
			var field = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			return field != null ? (T)field.GetValue(obj) : default(T);
		}

		public void SetPropertyValue<T>(object obj, string name, object value)
		{
			if (obj != null)
			{
				var field = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				field?.SetValue(obj, value);
			}
		}

		public GXODataClient Set(params ODataExpression[] value)
		{
			return Apply(BoundClient.Set(ApplyMappingsEx(value, 0)));
		}
		public GXODataClient Set(IDictionary<string, object> value)
		{
			return Apply(BoundClient.Set(value));
		}

		public GXODataClient Key(ODataEntry entryKey)
		{
			return Apply(BoundClient.Key(entryKey));
		}

		public GXODataClient Key(params object[] entryKey)
		{
			return Apply(BoundClient.Key(entryKey));
		}

		public Task<IDictionary<string, object>> FindEntryAsync(string commandText)
		{
			return ((IODataClient)Client).FindEntryAsync(commandText);
		}

		public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, ODataFeedAnnotations annotations)
		{
			return ((IODataClient)Client).FindEntriesAsync(commandText, annotations);
		}

		public Task<ODataEntry> UpdateEntryAsync(bool resultRequired)
		{
			return BoundClient.UpdateEntryAsync(resultRequired);
		}

		public Task<ODataEntry> FindEntryAsync()
		{
			return BoundClient.FindEntryAsync();
		}

		public Task<string> GetCommandTextAsync()
		{
			return BoundClient.GetCommandTextAsync();
		}

		public Task UnlinkEntryAsync(ODataExpression expression)
		{
			return BoundClient.UnlinkEntryAsync(expression);
		}

		public Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
		{
			return BoundClient.LinkEntryAsync(expression, linkedEntryKey);
		}

		public Task<IEnumerable<ODataEntry>> UpdateEntriesAsync(bool resultRequired)
		{
			return BoundClient.UpdateEntriesAsync(resultRequired);
		}
		public Task<IEnumerable<ODataEntry>> UpdateEntriesAsync(bool resultRequired, int skipCount)
		{
			return BoundClient.Skip(skipCount).UpdateEntriesAsync(resultRequired);
		}

		public Task<ODataEntry> InsertEntryAsync(bool resultRequired)
		{
			return BoundClient.InsertEntryAsync(resultRequired);
		}

		public Task<int> DeleteEntriesAsync()
		{
			return BoundClient.DeleteEntriesAsync();
		}
	}

	public class ODataResponseJSONError
	{
		public ODataResponseJSONErrorItem error { get; set; }
	}

	public class ODataResponseJSONErrorItem
	{
		public string code { get; set; }
		public object message { get; set; }
	}

}
