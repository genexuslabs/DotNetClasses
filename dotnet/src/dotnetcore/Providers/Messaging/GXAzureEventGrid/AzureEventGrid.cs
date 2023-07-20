using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using GeneXus.Messaging.Common;
using GeneXus.Services;
using GeneXus.Utils;

namespace GeneXus.Messaging.GXAzureEventGrid
{
	public class AzureEventGrid : EventRouterBase, IEventRouter
	{
		public static string Name = "AZUREEVENTGRID";
		private EventGridPublisherClient _client;
		private string _endpoint;
		private string _accessKey;
		public AzureEventGrid() : this(null)
		{
		}
		public AzureEventGrid(GXService providerService) : base(providerService)
		{
			Initialize(providerService);
		}
		private void Initialize(GXService providerService)
		{
			ServiceSettings serviceSettings = new(PropertyConstants.EVENT_ROUTER, Name, providerService);
			_endpoint = serviceSettings.GetEncryptedPropertyValue(PropertyConstants.URI_ENDPOINT);
			_accessKey = serviceSettings.GetEncryptedPropertyValue(PropertyConstants.ACCESS_KEY);

			//Package Azure.Identity cannot be referenced because it conflicts with Microsoft.Identity.Client pack version
			//used transitively by gxmail and dynamoDB projects.

			/*if (string.IsNullOrEmpty(_accessKey))

				//Try using Active Directory authentication
				_client = new EventGridPublisherClient(
				new Uri(_endpoint),
				new DefaultAzureCredential());*/


			if (!string.IsNullOrEmpty(_endpoint)) { 

				_client = new EventGridPublisherClient(
					new Uri(_endpoint),
					new AzureKeyCredential(_accessKey));
			}
			else
				throw new Exception("Endpoint URI must be set.");
		}
		public override string GetName()
		{
			return Name;
		}

		public bool SendEvent(GXCloudEvent gxCloudEvent, bool binaryData)
		{
			CloudEvent evt = ToCloudEvent(gxCloudEvent, binaryData);
			bool success = false;
			try
			{
				Task<bool> task;
				if (_client != null)
				{
					task = Task.Run<bool>(async () => await sendEvtAsync(evt).ConfigureAwait(false));
					success = task.Result;
				}
				else
				{
					throw new Exception("There was an error at the Event Grid initialization.");
				}
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return success;
		}
		public bool SendEvents(IList<GXCloudEvent> gxCloudEvents, bool binaryData)
		{
			List<CloudEvent> evts = new List<CloudEvent>();
			foreach (GXCloudEvent e in gxCloudEvents)
				evts.Add(ToCloudEvent(e, binaryData));

			bool success = false;
			try
			{
				Task<bool> task;
				if (_client != null)
				{
					task = Task.Run<bool>(async () => await sendEvtsAsync(evts).ConfigureAwait(false));
					success = task.Result;
				}
				else
				{
					throw new Exception("There was an error at the Event Grid initialization.");
				}
			}
			catch (AggregateException ae)
			{
				throw ae;
			}
			return success;
		}
		public bool SendCustomEvents(string jsonString, bool isBinary)
		{
			if (string.IsNullOrEmpty(jsonString))
			{
				throw new Exception("Events cannot be empty.");
			}
			try
			{
				List<GXEventGridSchema> evts = JsonSerializer.Deserialize<List<GXEventGridSchema>>(jsonString);

				IList<EventGridEvent> eventGridEvents = new List<EventGridEvent>();
				foreach (GXEventGridSchema e in evts)
					eventGridEvents.Add(ToEventGridSchema(e,isBinary));

				bool success = false;
				try
				{
					Task<bool> task;
					if (_client != null)
					{
						task = Task.Run<bool>(async () => await sendEventGridSchemaEventsAsync(eventGridEvents).ConfigureAwait(false));
						success = task.Result;
					}
					else
					{
						throw new Exception("There was an error at the Event Grid initialization.");
					}
				}
				catch (AggregateException ae)
				{
					throw ae;
				}
				return success;
			}
			catch (JsonException)
			{
				try
				{
					GXEventGridSchema evt = JsonSerializer.Deserialize<GXEventGridSchema>(jsonString);
					bool success = false;
					try
					{
						Task<bool> task;
						if (_client != null)
						{
							task = Task.Run<bool>(async () => await sendEventGridSchemaEventAsync(ToEventGridSchema(evt, isBinary)).ConfigureAwait(false));
							success = task.Result;
						}
						else
						{
							throw new Exception("There was an error at the Event Grid initialization.");
						}
					}
					catch (AggregateException ae)
					{
						throw ae;
					}
					return success;
				}
				catch (JsonException)
				{
					throw new Exception("jsonEvents parameter format is no correct. Valid format is AzureEventGrid.EventGridSchema SDT.");
				}
			}
		}

		#region Async methods
		/// <summary>
		/// Send asynchronously an event formatted as Azure EventGrid Schema.
		/// </summary>
		/// <param name="evt"></param>
		/// <returns></returns>
		private async Task<bool> sendEventGridSchemaEventAsync(EventGridEvent evt)
		{
			try
			{
				await _client.SendEventAsync(evt).ConfigureAwait(false);
				return true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		/// <summary>
		/// Send asynchronously a list of events formatted as Azure EventGrid Schema.
		/// </summary>
		/// <param name="evts"></param>
		/// <returns></returns>
		private async Task<bool> sendEventGridSchemaEventsAsync(IEnumerable<EventGridEvent> evts)
		{
			try
			{
				await _client.SendEventsAsync(evts).ConfigureAwait(false);
				return true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		/// <summary>
		/// Send asynchronously an event formatted as CloudEvent Schema.
		/// </summary>
		/// <param name="cloudEvent"></param>
		/// <returns></returns>
		private async Task<bool> sendEvtAsync(CloudEvent cloudEvent)
		{
			try
			{
				await _client.SendEventAsync(cloudEvent).ConfigureAwait(false);
				return true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		/// <summary>
		/// Send asynchronously a list of CloudEvent Schema formatted events.
		/// </summary>
		/// <param name="cloudEvents"></param>
		/// <returns></returns>
		private async Task<bool> sendEvtsAsync(IEnumerable<CloudEvent> cloudEvents)
		{
			try
			{
				await _client.SendEventsAsync(cloudEvents).ConfigureAwait(false);
				return true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		#endregion

		#region TransformMethods
		public bool GetMessageFromException(Exception ex, SdtMessages_Message msg)
		{
			try
			{
				RequestFailedException az_ex = (RequestFailedException)ex;
				msg.gxTpr_Id = az_ex.ErrorCode.ToString();
				msg.gxTpr_Description = az_ex.Message;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		private EventGridEvent ToEventGridSchema(GXEventGridSchema gxEventGridSchema, bool isBinary)
		{
			EventGridEvent evt;
			if (isBinary && !string.IsNullOrEmpty(gxEventGridSchema.data))
			{
				BinaryData binaryData = new BinaryData(gxEventGridSchema.data);
				evt = new EventGridEvent(gxEventGridSchema.subject, gxEventGridSchema.eventtype, gxEventGridSchema.dataversion, binaryData);}
			else
				evt = new EventGridEvent(gxEventGridSchema.subject, gxEventGridSchema.eventtype, gxEventGridSchema.dataversion, gxEventGridSchema.data, null);
			if (!string.IsNullOrEmpty(gxEventGridSchema.id))
				evt.Id = gxEventGridSchema.id;
			if (!string.IsNullOrEmpty(gxEventGridSchema.topic))
				evt.Topic = gxEventGridSchema.topic;
			if (gxEventGridSchema.eventtime != DateTime.MinValue)
				evt.EventTime = gxEventGridSchema.eventtime;

			return evt;
		}
		private CloudEvent ToCloudEvent(GXCloudEvent gxCloudEvent, bool isBinaryData)
		{
			CloudEvent evt;
			if (string.IsNullOrEmpty(gxCloudEvent.data))
				evt = new CloudEvent(gxCloudEvent.source, gxCloudEvent.type, null);
			else
			{
				if (!isBinaryData)
				{ 
					if (string.IsNullOrEmpty(gxCloudEvent.datacontenttype))
						gxCloudEvent.datacontenttype = "application/json";
					evt = new CloudEvent(gxCloudEvent.source, gxCloudEvent.type, BinaryData.FromString(gxCloudEvent.data),gxCloudEvent.datacontenttype,CloudEventDataFormat.Json);
				}
				else
				{
					if (string.IsNullOrEmpty(gxCloudEvent.datacontenttype))
						gxCloudEvent.datacontenttype = "application/octet-stream";
					evt = new CloudEvent(gxCloudEvent.source, gxCloudEvent.type, BinaryData.FromString(gxCloudEvent.data), gxCloudEvent.datacontenttype,CloudEventDataFormat.Binary);
				}
			}
			if (!string.IsNullOrEmpty(gxCloudEvent.id))
				evt.Id = gxCloudEvent.id;
			if (!string.IsNullOrEmpty(gxCloudEvent.dataschema))
				evt.DataSchema = gxCloudEvent.dataschema;
			if (!string.IsNullOrEmpty(gxCloudEvent.subject))
				evt.Subject = gxCloudEvent.subject;
			if (gxCloudEvent.time != DateTime.MinValue)
				evt.Time = gxCloudEvent.time;
			return evt;
		}
		#endregion
	}

	[DataContract]
	public class GXEventGridSchema
	{
		[DataMember]
		public string topic { get; set; }
		[DataMember]
		public string eventtype { get; set; }
		[DataMember]
		public string id { get; set; }
		[DataMember]
		public string subject { get; set; }
		[DataMember]
		public string data { get; set; }
		[DataMember]
		public string dataversion { get; set; }

		[DataMember]
		public DateTime eventtime { get; set; }

		[DataMember]
		public string metadataversion { get; set; }


	}
}