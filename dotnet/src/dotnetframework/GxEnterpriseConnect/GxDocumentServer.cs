using System;
using SAP.Middleware.Connector;
using System.Threading.Tasks;
using System.IO;
using System.Text;


namespace GeneXus.SAP
{
	class GxDocument
	{
		private const int BLOB_LENGTH = 1022;
		public static void SetServerParameters(RfcFunctionMetadata functionMetadata, bool setTextMetadata)
		{
			functionMetadata.AddParameter(new RfcParameterMetadata("FNAME", RfcDataType.CHAR, 256, 512, RfcDirection.IMPORT, false));
			functionMetadata.AddParameter(new RfcParameterMetadata("MODE", RfcDataType.CHAR, 1, 2, RfcDirection.IMPORT, true));			
			functionMetadata.AddParameter(new RfcParameterMetadata("ERROR", RfcDataType.INT4, 4, 4, RfcDirection.EXPORT, false));

			RfcStructureMetadata tabLine = new RfcStructureMetadata("BLOB");
			tabLine.AddField(new RfcFieldMetadata("LINE", RfcDataType.BYTE, BLOB_LENGTH, 0, BLOB_LENGTH, 0));
			tabLine.Lock();
			RfcTableMetadata tabMeta = new RfcTableMetadata("BLOB", tabLine);
			functionMetadata.AddParameter(new RfcParameterMetadata("BLOB", tabMeta, RfcDirection.TABLES, false));

			if (setTextMetadata)
			{
				functionMetadata.AddParameter(new RfcParameterMetadata("LENGTH", RfcDataType.INT4, 4, 4, RfcDirection.EXPORT, false));
				RfcStructureMetadata tabLine3 = new RfcStructureMetadata("TEXT");
				tabLine3.AddField(new RfcFieldMetadata("LINE", RfcDataType.CHAR, 120, 0, 120, 0));
				RfcTableMetadata tabMeta3 = new RfcTableMetadata("TEXT", tabLine3);
				functionMetadata.AddParameter(new RfcParameterMetadata("TEXT", tabMeta3, RfcDirection.TABLES, false));
			}
			else {
				functionMetadata.AddParameter(new RfcParameterMetadata("LENGTH", RfcDataType.INT4, 4, 4, RfcDirection.IMPORT, false));
			}
		}
	}

	class GxDocumentSender
	{
		public static void rfcServerErrorOccured(Object server, RfcServerErrorEventArgs errorEventData)
		{
			// Technical problem in server connection (network, logon data, out-of-memory, etc.)
			Console.WriteLine(errorEventData.Error.ToString());
		}


		public static RfcServer CreateDocumentServer(string DocServerName)
		{
			RfcCustomRepository repo = new RfcCustomRepository("DMS");

			RfcFunctionMetadata FTP_CLIENT_TO_R3 = new RfcFunctionMetadata("FTP_CLIENT_TO_R3");
			GxDocument.SetServerParameters(FTP_CLIENT_TO_R3, true);
		
			repo.AddFunctionMetadata(FTP_CLIENT_TO_R3);

			Type[] handlers = new Type[1] { typeof(FTP_CLIENT_TO_R3Handler) };
			RfcServer server = RfcServerManager.GetServer(DocServerName, handlers, repo);

			server.RfcServerError += rfcServerErrorOccured;

			return server;
		}

		static class FTP_CLIENT_TO_R3Handler
		{
			private const int BLOB_LENGTH = 1022;

			[RfcServerFunction(Name = "FTP_CLIENT_TO_R3", Default = false)]
			public static void handleRequest(RfcServerContext context, IRfcFunction function)
			{
				String fname;

				fname = function.GetString("FNAME");
				fname = fname.Replace("#", "");
				fname = fname.Replace("?", "");
				using (Stream source = File.OpenRead(fname))
				{
					try
					{
						byte[] file = new byte[BLOB_LENGTH];
						int bytesread;
						Int32 totallenght = 0;

						IRfcTable blobtable = function.GetTable("BLOB");

						while ((bytesread = source.Read(file, 0, file.Length)) > 0)
						{
							IRfcStructure blobstruct = blobtable.Metadata.LineType.CreateStructure();
							blobstruct.SetValue("LINE", file);
							blobtable.Append(blobstruct);
							totallenght += bytesread;
						}
						function.SetValue("LENGTH", totallenght);
						function.SetValue("BLOB", blobtable);

					}
					catch (IOException e)
					{
						// unfortunately there is no way of transmitting error details back to sap, so we better log it here,
						// if we want to keep the chance of trouble-shooting later, what exactly went wrong...
						Console.WriteLine(e.ToString());
						function.SetValue("ERROR", 3);
					}
					finally
					{
						if (source != null)
						{
							try
							{
								source.Close();
							}
							catch (IOException) { }
						}
					}
				}
			}
		}
	}

	class GxDocumentReceiver 
	{
		private const int BLOB_LENGTH = 1022;

		public static void rfcServerErrorOccured(Object server, RfcServerErrorEventArgs errorEventData)
		{
			// Technical problem in server connection (network, logon data, out-of-memory, etc.)
			Console.WriteLine(errorEventData.Error.ToString());
		}
		
		public static RfcServer CreateDocumentServer(string DocServerName)
		{
			RfcCustomRepository repo = new RfcCustomRepository("DMS");
			RfcFunctionMetadata FTP_R3_TO_CLIENT = new RfcFunctionMetadata("FTP_R3_TO_CLIENT");
			GxDocument.SetServerParameters(FTP_R3_TO_CLIENT, false);
			FTP_R3_TO_CLIENT.Lock();

			repo.AddFunctionMetadata(FTP_R3_TO_CLIENT);

			Type[] handlers = new Type[1] { typeof(FTP_R3_TO_CLIENTHandler) };
			RfcServer server = RfcServerManager.GetServer(DocServerName, handlers, repo);

			server.RfcServerError += rfcServerErrorOccured;
			return server;
		}

		static class FTP_R3_TO_CLIENTHandler
		{
			[RfcServerFunction(Name = "FTP_R3_TO_CLIENT", Default = false)]
			public static void handleRequest(RfcServerContext context, IRfcFunction function)
			{
				String fname;
				int length;
				IRfcTable blob;

				// In the case of BAPI_DOCUMENT_CHECKOUTVIEW2, MODE is always binary, so the MODE and TEXT parameters of FTP_R3_TO_CLIENT can be ignored.
				fname = function.GetString("FNAME");
				length = function.GetInt("LENGTH");
				blob = function.GetTable("BLOB");
				FileStream outFile = null;

				try
				{
					outFile = File.Open(fname, FileMode.Create);
					bool hasNextRow = false;
					if (blob.RowCount > 0)
					{
						hasNextRow = true;
						blob.CurrentIndex = 0;
					}

					byte[] buffer = new byte[BLOB_LENGTH];
					while (length > BLOB_LENGTH)
					{
						if (hasNextRow)
						{
							blob.GetByteArray(0, buffer, 0);
							outFile.Write(buffer, 0, BLOB_LENGTH);
							length -= BLOB_LENGTH;
							if (blob.CurrentIndex + 1 < blob.RowCount) blob.CurrentIndex++;
							else hasNextRow = false;
						}
						else throw new IOException("Not enough data in table BLOB (" + (BLOB_LENGTH * blob.RowCount).ToString() + ") for requested file size (" + length.ToString() + ")");
					}
					if (length > 0)
					{
						if (hasNextRow)
						{
							blob.GetByteArray(0, buffer, 0);
							outFile.Write(buffer, 0, length);
						}
						else throw new IOException("Not enough data in table BLOB (" + (BLOB_LENGTH * blob.RowCount).ToString() + ") for requested file size (" + length.ToString() + ")");
					}
				}
				catch (IOException e)
				{
					// Unfortunately there is no way of transmitting error details back to SAP, so we better log it here,
					// if we want to keep the chance of trouble-shooting later, what exactly went wrong...
					Console.WriteLine(e.ToString());
					function.SetValue("ERROR", 3);
				}
				finally
				{
					if (outFile != null)
					{
						try
						{
							outFile.Close();
						}
						catch (IOException) { }
					}
				}
			}
		}
	}
}
