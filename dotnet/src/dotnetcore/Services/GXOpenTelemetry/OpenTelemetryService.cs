namespace GXOpenTelemetry
{
    public class OpenTelemetryService
    {

		public static  Instance
		{
			get
			{
				if (instance == null)
				{
					lock (syncRoot)
					{
						if (instance == null)
						{

							GXServices services = GXServices.Instance;
							if (services != null)
							{
								GXService providerService = services.Get(GXServices.CACHE_SERVICE);
								if (providerService != null)
								{
									GXLogging.Debug(log, "Loading CACHE_PROVIDER: ", providerService.ClassName);
									try
									{
#if NETCORE
										Type type = AssemblyLoader.GetType(providerService.ClassName);
#else
										Type type = Type.GetType(providerService.ClassName, true, true);
#endif
										instance = (ICacheService)Activator.CreateInstance(type);
										if (providerService.Properties.ContainsKey(FORCE_HIGHEST_TIME_TO_LIVE))
										{
											int ttl;
											if (Int32.TryParse(providerService.Properties.Get(FORCE_HIGHEST_TIME_TO_LIVE), out ttl) && ttl == 1)
											{
												forceHighestTimetoLive = true;
											}
										}
									}
									catch (Exception e)
									{
										GXLogging.Error(log, "Couldn't create CACHE_PROVIDER as ICacheService: ", providerService.ClassName, e);
										throw e;
									}
								}
							}
							if (instance == null)
							{
								GXLogging.Debug(log, "Loading Default CACHE_PROVIDER InMemoryCache");
								instance = new InProcessCache();
							}
						}
					}
				}

				return instance;
			}
		}

	}
}