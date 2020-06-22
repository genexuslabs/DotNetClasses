using GeneXus.Utils;
using Jayrock.Json;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace GeneXus.Application
{
	public class GXBCRestService :GxRestWrapper 
	{
		GxSilentTrnSdt _worker;
		private const string INSERT_OR_UPDATE_PARAMETER = "insertorupdate";
		private const string DEFAULT_PARAMETER = "_default";
		private const string LOAD_METHOD = "Load";
		private const string CHECK_PARAMETER = "check";

		public GXBCRestService(GxSilentTrnSdt sdt, HttpContext context, IGxContext gxContext) :base(context, gxContext)
		{
			_worker = sdt;
		}
		protected override GXBaseObject Worker => _worker.trn as GXBaseObject;
		public override Task Post()
		{
			try
			{
				if (!IsAuthenticated())
				{
					return Task.CompletedTask;
				}
				bool gxcheck = IsRestParameter(CHECK_PARAMETER);
				bool gxinsertorupdate = IsRestParameter(INSERT_OR_UPDATE_PARAMETER);

				GxSilentTrnSdt entity = (GxSilentTrnSdt)Activator.CreateInstance(_worker.GetType(), new Object[] { _gxContext });
				var entity_interface = MakeRestType(entity);
				entity_interface = ReadRequestBodySDTObj(entity_interface.GetType());

				var worker_interface = MakeRestType(_worker);
				
				worker_interface.GetType().GetMethod("CopyFrom").Invoke(worker_interface, new object[] { entity_interface });
				if (gxcheck)
				{
					_worker.Check();
				}
				else
				{
					if (gxinsertorupdate)
					{
						_worker.InsertOrUpdate();
					}
					else
					{
						_worker.Save();
					}
				}
				if (_worker.Success())
				{
					if (!gxcheck)
					{
						_worker.trn.context.CommitDataStores();
						SetStatusCode(HttpStatusCode.Created);
					}
					SetMessages(_worker.trn.GetMessages());
					return Serialize(MakeRestType(_worker));
				}
				else
				{
					Cleanup();
					return ErrorCheck(_worker.trn);
				}
			}
			catch (Exception e)
			{
				return WebException(e);
			}
			finally
			{
				Cleanup();
			}
		}
		private string[] SplitParameters(object parameters)
		{
			string sparameters = parameters as string;
			if (!string.IsNullOrEmpty(sparameters)) { 
				return sparameters.Split(',');
			}
			return null;
		}
		public override Task Get(object parameters)
		{
			try
			{
				if (!IsAuthenticated())
				{
					return Task.CompletedTask;
				}
				string[] key = SplitParameters(parameters);
				if (key!=null)
				{
					if (key != null && key.Length > 0 && key[0] == DEFAULT_PARAMETER)
					{
						_worker.getTransaction().GetInsDefault();
					}
					else
					{
						ReflectionHelper.CallBCMethod(_worker, LOAD_METHOD, key);
					}
					if (_worker.Success())
					{
						SetMessages(_worker.trn.GetMessages());
						return Serialize(MakeRestType(_worker));
					}
					else
					{
						return ErrorCheck(_worker.trn);
					}
				}
				else
				{
					return SetError(((int)HttpStatusCode.NotFound).ToString(), HttpStatusCode.NotFound.ToString());
				}
			}
			catch (Exception e)
			{
				return WebException(e);
			}
			finally
			{
				Cleanup();
			}
		}
		public override Task Delete(object parameters)
		{
			try
			{
				if (!IsAuthenticated())
				{
					return Task.CompletedTask;
				}
				string[] key = SplitParameters(parameters);
				if (key != null)
				{ 
					ReflectionHelper.CallBCMethod(_worker, LOAD_METHOD, key);
					_worker.Delete();
					if (_worker.Success())
					{
						_worker.trn.context.CommitDataStores();
						SetMessages(_worker.trn.GetMessages());
						return Serialize(MakeRestType(_worker));
					}
					else
					{
						return ErrorCheck(_worker.trn);
					}
				}
				else
				{
					return SetError(((int)HttpStatusCode.NotFound).ToString(), HttpStatusCode.NotFound.ToString());
				}
			}
			catch (Exception e)
			{
				return WebException(e);
			}
			finally
			{
				Cleanup();
			}
		}
		public override Task Put(object parameters)
		{
			try
			{
				if (!IsAuthenticated())
				{
					return Task.CompletedTask;
				}
				string[] key = SplitParameters(parameters);
				if (key != null)
				{
					bool gxcheck = IsRestParameter(CHECK_PARAMETER);
					GxSilentTrnSdt entity = (GxSilentTrnSdt)Activator.CreateInstance(_worker.GetType(), new Object[] { _gxContext });
					var entity_interface = MakeRestType(entity);
					entity_interface = ReadRequestBodySDTObj(entity_interface.GetType());
					string entityHash = entity_interface.GetType().GetProperty("Hash").GetValue(entity_interface) as string;

					ReflectionHelper.CallBCMethod(_worker, LOAD_METHOD, key);
					var worker_interface = MakeRestType(_worker);
					string currentHash = worker_interface.GetType().GetProperty("Hash").GetValue(worker_interface) as string;
					if (entityHash == currentHash)
					{
						worker_interface.GetType().GetMethod("CopyFrom").Invoke(worker_interface, new object[] { entity_interface });
						if (gxcheck)
						{
							_worker.Check();
						}
						else
						{
							_worker.Save();
						}
						if (_worker.Success())
						{
							if (!gxcheck)
							{
								_worker.trn.context.CommitDataStores();
							}
							SetMessages(_worker.trn.GetMessages());
							worker_interface.GetType().GetProperty("Hash").SetValue(worker_interface, null);
							return Serialize(worker_interface);
						}
						else
						{
							Cleanup();
							return ErrorCheck(_worker.trn);
						}
					}
					else
					{
						return SetError(HttpStatusCode.Conflict.ToString(), _worker.trn.context.GetMessage("GXM_waschg", new object[] { this.GetType().Name }));
					}
				}
				else
				{
					return SetError(((int)HttpStatusCode.NotFound).ToString(), HttpStatusCode.NotFound.ToString());
				}
			}
			catch (Exception e)
			{
				return WebException(e);
			}
			finally
			{
				Cleanup();
			}
		}

		private object ReadRequestBodySDTObj(Type type)
		{
			using (var reader = new StreamReader(_httpContext.Request.Body))
			{
				var sdtData = reader.ReadToEnd();
				using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sdtData)))
				{
					DataContractJsonSerializer serializer = new DataContractJsonSerializer(type);
					return serializer.ReadObject(stream);
				}
			}
		}
	
		public Task ErrorCheck(IGxSilentTrn trn)
		{
			if (trn.Errors() == 1)
			{
				msglist msg = trn.GetMessages();
				if (msg.Count > 0)
				{
					msglistItem msgItem = (msglistItem)msg[0];
					if (msgItem.gxTpr_Id.Contains("NotFound"))
						return SetError("404", msgItem.gxTpr_Description);
					else if (msgItem.gxTpr_Id.Contains("WasChanged"))
						return SetError("409", msgItem.gxTpr_Description);
					else
						return SetError("400", msgItem.gxTpr_Description);
				}
			}
			return Task.CompletedTask;

		}
		public override void Cleanup()
		{
			_worker.trn.cleanup();
			base.Cleanup();
		}
	}
}

