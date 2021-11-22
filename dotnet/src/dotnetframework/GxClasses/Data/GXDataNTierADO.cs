using System;
using System.Data;
using System.Collections;
using GeneXus.Data.ADO;
using GeneXus.Configuration;
using GeneXus.Utils;
using System.IO;
using log4net;
using GeneXus.Application;
using System.Collections.Generic;
using GeneXus.Services;
using System.Net;
using GeneXus.Storage;

namespace GeneXus.Data.NTier.ADO
{
    public class GXFatErrorFieldGetter : IFieldGetter
    {
        GxCommand _gxDbCommand;
        public GXFatErrorFieldGetter(GxCommand gxDbCommand)
        {
            _gxDbCommand = gxDbCommand;
        }
        #region IFieldGetter Members

        public IDataReader DataReader
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public bool wasNull(int id)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public short getShort(int id)
        {
            return (short)_gxDbCommand.ErrorRecord(id - 1);
        }

        public int getInt(int id)
        {
            return (int)_gxDbCommand.ErrorRecord(id - 1);
        }

        public long getLong(int id)
        {
            return (long)_gxDbCommand.ErrorRecord(id - 1);
        }

        public double getDouble(int id)
        {
            return (double)_gxDbCommand.ErrorRecord(id - 1);
        }

        public decimal getDecimal(int id)
        {
            return (decimal)_gxDbCommand.ErrorRecord(id - 1);
        }

        public string getString(int id, int size)
        {
            return (string)_gxDbCommand.ErrorRecord(id - 1);
        }
        public DateTime getDateTime(int id, Boolean precision)
        {
            return (DateTime)_gxDbCommand.ErrorRecord(id - 1);
        }
        public DateTime getDateTime(int id)
        {
            return (DateTime)_gxDbCommand.ErrorRecord(id - 1);
        }

        public string getLongVarchar(int id)
        {
            return (String)_gxDbCommand.ErrorRecord(id - 1);
        }

        public DateTime getGXDateTime(int id, Boolean precision)
        {
            return (DateTime)_gxDbCommand.ErrorRecord(id - 1);
        }
        public DateTime getGXDateTime(int id)
        {
            return (DateTime)_gxDbCommand.ErrorRecord(id - 1);
        }

        public DateTime getGXDate(int id)
        {
            return (DateTime)_gxDbCommand.ErrorRecord(id - 1);
        }

        public string getBLOBFile(int id)
        {
            return getBLOBFile(id, "tmp", "");
        }

        public string getBLOBFile(int id, string extension, string name)
        {
            string fileName = FileUtil.getTempFileName(_gxDbCommand.Conn.BlobPath, name, extension, GxFileType.Private);
            return getBLOBFile(id, extension, name, fileName, true);
        }

        private string getBLOBFile(int id, string extension, string name, string fileName, bool temporary)
        {
            FileStream fs;
            BinaryWriter bw;
            byte[] outbyte = (byte[])_gxDbCommand.ErrorRecord(id - 1);
            try
            {
                if (outbyte.Length == 0)
                    return "";
				using (fs = FileUtil.FileStream(ref fileName, FileMode.OpenOrCreate, FileAccess.Write, _gxDbCommand.Conn.BlobPath))
				{
					using (bw = new BinaryWriter(fs))
					{
						bw.Write(outbyte);
						bw.Flush();
					}
				}
				if (temporary)
                    GXFileWatcher.Instance.AddTemporaryFile(new GxFile(_gxDbCommand.Conn.BlobPath, new GxFileInfo(fileName, _gxDbCommand.Conn.BlobPath), GxFileType.PrivateAttribute), _gxDbCommand.Conn.DataStore.Context);
                fileName = new FileInfo(fileName).FullName;
            }
            catch (IOException e)
            {
                throw (new GxADODataException(e));
            }

            return fileName;

        }

        public string getMultimediaFile(int id, string gxdbFileUri)
        {
            if (!GXDbFile.IsFileExternal(gxdbFileUri))
            {
                string fileName = GXDbFile.GetFileNameFromUri(gxdbFileUri);
                if (!String.IsNullOrEmpty(fileName))
				{
					string filePath = PathUtil.SafeCombine(_gxDbCommand.Conn.MultimediaPath, fileName);
					try
					{
						GxFile file = new GxFile(string.Empty, filePath, GxFileType.DefaultAttribute);
						if (file.Exists())
						{
							return filePath;
						}
						else
						{
							return getBLOBFile(id, FileUtil.GetFileType(gxdbFileUri), FileUtil.GetFileName(gxdbFileUri), filePath, false);
						}
					}
					catch (ArgumentException)
					{
						return "";
					}
				}
            }

            return "";
        }

        public string getMultimediaUri(int id)
        {			
			return GXDbFile.ResolveUri(getVarchar(id), true, _gxDbCommand.Conn.DataStore.Context);
        }

		public string getMultimediaUri(int id, bool absUrl)
		{
			return GXDbFile.ResolveUri(getVarchar(id), absUrl,  _gxDbCommand.Conn.DataStore.Context);
		}

		public string getVarchar(int id)
        {
            return (String)_gxDbCommand.ErrorRecord(id - 1);
        }

        public decimal getBigDecimal(int id, int dec)
        {
            return (Decimal)_gxDbCommand.ErrorRecord(id - 1);
        }

        public bool getBool(int id)
        {
            return (bool)_gxDbCommand.ErrorRecord(id - 1);
        }
        public Guid getGuid(int id)
        {
            return (Guid)_gxDbCommand.ErrorRecord(id - 1);
        }
        public IGeographicNative getGeospatial(int id)
        {
            return new Utils.Geospatial((object)_gxDbCommand.ErrorRecord(id - 1));
        }
        #endregion
    }
    public class GXFatFieldGetter : IFieldGetter
    {
        static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Data.NTier.ADO.GXFatFieldGetter));
        IGxDbCommand _gxDbCommand;
        IDataReader _DR; 
        public GXFatFieldGetter(GxCommand gxDbCommand)
        {
            _gxDbCommand = gxDbCommand;
        }
		void TraceRow(params string[] list)
		{
			if (_gxDbCommand.HasMoreRows)
			{
				GXLogging.Trace(log, list);
			}
		}
        public IDataReader DataReader
        {
            get { return _DR; }
            set { _DR = value; }
        }
        public short getShort(int id)
        {
			short value = _gxDbCommand.Db.GetShort(_gxDbCommand, _DR, id - 1);
			TraceRow("getShort - index : ", id.ToString(), " value:", value.ToString());
			return value;
        }
        public int getInt(int id)
        {
            int value = _gxDbCommand.Db.GetInt(_gxDbCommand, _DR, id - 1);
			TraceRow("getInt - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
		public bool getBool(int id)
		{
			bool value = _gxDbCommand.Db.GetBoolean(_gxDbCommand, _DR, id - 1);
			TraceRow("getBool - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
        public Guid getGuid(int id)
        {
            Guid value = _gxDbCommand.Db.GetGuid(_gxDbCommand, _DR, id - 1);
			TraceRow("getGuid - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
        public long getLong(int id)
        {
            long value = _gxDbCommand.Db.GetLong(_gxDbCommand, _DR, id - 1);
			TraceRow("getLong - index : ", id.ToString(), " value:", value.ToString());
			return value;

		}
        public double getDouble(int id)
        {
            double value= _gxDbCommand.Db.GetDouble(_gxDbCommand, _DR, id - 1);
			TraceRow("getDouble - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
        public Decimal getDecimal(int id)
        {
            Decimal value= _gxDbCommand.Db.GetDecimal(_gxDbCommand, _DR, id - 1);
			TraceRow("getDecimal - index : ", id.ToString(), " value:", value.ToString());
			return value;

		}
        public string getString(int id, int size)
        {
            String value = _gxDbCommand.Db.GetString(_gxDbCommand, _DR, id - 1, size);
			TraceRow("getString - index : ", id.ToString(), " value:", (value!=null ? value.ToString(): string.Empty));
			return value;
		}
        public DateTime getDateTime(int id)
        {
            DateTime value = _gxDbCommand.Db.GetDateTime(_gxDbCommand, _DR, id - 1);
			TraceRow("getDateTime - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
        public DateTime getDateTime(int id, Boolean precision)
        {
			DateTime value;
            if (precision) {
                value = _gxDbCommand.Db.GetDateTimeMs(_gxDbCommand, _DR, id - 1);
            }
            else {
                value = _gxDbCommand.Db.GetDateTime(_gxDbCommand, _DR, id - 1);
            }
			TraceRow("getDateTime - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
        public DateTime getDate(int id)
        {
            DateTime value = _gxDbCommand.Db.GetDate(_gxDbCommand, _DR, id - 1);
			TraceRow("getDate - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
        public string getLongVarchar(int id)
        {
            string value = _gxDbCommand.Db.GetString(_gxDbCommand, _DR, id - 1);
			TraceRow("getLongVarchar - index : ", id.ToString(), " value:", (value!=null ? value.ToString(): string.Empty));
			return value;
		}
        public DateTime getGXDateTime(int id, Boolean precision)
        {
            DateTime value = DateTimeUtil.DBserver2local(getDateTime(id, precision), _gxDbCommand.Conn.ClientTimeZone);
			TraceRow("getDateTime - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
        public DateTime getGXDateTime(int id)
        {
			DateTime value = DateTimeUtil.DBserver2local(getDateTime(id,false), _gxDbCommand.Conn.ClientTimeZone);
			TraceRow("getGXDateTime - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
		public DateTime getGXDate(int id)
        {
            DateTime value = getDate(id);
			TraceRow("getGXDate - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}
        public string getBLOBFile(int id)
        {
            string value= getBLOBFile(id, "tmp", "");
			TraceRow("getBLOBFile - index : ", id.ToString(), " value:", (value!=null ? value.ToString() : string.Empty));
			return value;
		}

        public string getBLOBFile(int id, string extension, string name)
        {
            string fileName = FileUtil.getTempFileName(_gxDbCommand.Conn.BlobPath, name, extension, GxFileType.Private);
            String value = getBLOBFile(id, extension, name, fileName, true);
			TraceRow("getBLOBFile - index : ", id.ToString(), " value:", (value!=null ? value.ToString() : string.Empty));
			return value;
		}

        private string getBLOBFile(int id, string extension, string name, string fileName, bool temporary, GxFileType fileType = GxFileType.PrivateAttribute)
        {
            GxFile file = null;
            Stream fs = null;
            BinaryWriter bw = null;
            int bufferSize = 4096;
            byte[] outbyte = new byte[bufferSize];
            long retval;
			long startIndex;
            bool streamClosed = false;
            try
            {
                
                startIndex = 0;

                retval = _gxDbCommand.Db.GetBytes(_gxDbCommand, _DR, id - 1, startIndex, outbyte, 0, bufferSize);

                if (retval == 0)
                    return "";

				using (fs = new MemoryStream())
				{
					using (bw = new BinaryWriter(fs))
					{

						while (retval == bufferSize)
						{
							bw.Write(outbyte);
							bw.Flush();

							startIndex += bufferSize;
							retval = _gxDbCommand.Db.GetBytes(_gxDbCommand, _DR, id - 1, startIndex, outbyte, 0, bufferSize);
						}

						bw.Write(outbyte, 0, (int)retval);
						bw.Flush();

						fs.Seek(0, SeekOrigin.Begin);

						file = new GxFile(_gxDbCommand.Conn.BlobPath, fileName, fileType);
						file.Create(fs);

					}
				}
				streamClosed = true;

				TraceRow("GetBlobFile fileName:" + fileName + ", retval bytes:" + retval);

                if (temporary)
                    GXFileWatcher.Instance.AddTemporaryFile(file, _gxDbCommand.Conn.DataStore.Context);

				fileName = file.GetURI();
            }
            catch (IOException e)
            {
                if (!file.Exists())
                {
                    GXLogging.Error(log, "Return getBLOBFile Error Can't read BLOB field into " + fileName, e);
                    throw (new GxADODataException(e));
                }
                else
                {
                    GXLogging.Warn(log, "Return getBLOBFile Error Can't write BLOB field into " + fileName, e);
                }
            }
            finally
            {
                if (!streamClosed)
                {
                    try
                    {
						if (bw != null)
                            bw.Close();
                        if (fs != null)
                            fs.Close();
					}
					catch (Exception ex)
                    {
                        GXLogging.Error(log, "getBLOBFile Close Stream Error", ex);
                    }
                }
            }
            return fileName;
        }

        public string getMultimediaFile(int id, string gxdbFileUri)
        {
            if (!GXDbFile.IsFileExternal(gxdbFileUri))
            {
                string fileName = GXDbFile.GetFileNameFromUri(gxdbFileUri);
                if (!String.IsNullOrEmpty(fileName))
                {
					string filePath = PathUtil.SafeCombine(_gxDbCommand.Conn.MultimediaPath, fileName);

					try
					{
						GxFile file = new GxFile(string.Empty, filePath, GxFileType.DefaultAttribute);

						if (file.Exists())
						{
							return file.GetURI();
						}
						else
						{
							return getBLOBFile(id, FileUtil.GetFileType(gxdbFileUri), FileUtil.GetFileName(gxdbFileUri), filePath, false, GxFileType.DefaultAttribute);
						}
					}
					catch (ArgumentException)
					{
						return "";
					}
                }
            }

            return "";
        }

        public string getMultimediaUri(int id)
        {
            return getMultimediaUri(id, true);
        }

		public string getMultimediaUri(int id, bool absUrl)
		{
			return GXDbFile.ResolveUri(getVarchar(id), absUrl, _gxDbCommand.Conn.DataStore.Context);
		}

		public string getVarchar(int id)
        {
            string value = _gxDbCommand.Db.GetString(_gxDbCommand, _DR, id - 1);
			TraceRow("getVarchar - index : ", id.ToString(), " value:", (value != null ? value.ToString() : string.Empty));
			return value;
		}
        public decimal getBigDecimal(int id, int dec)
        {
			decimal value =_gxDbCommand.Db.GetDecimal(_gxDbCommand, _DR, id - 1);
			TraceRow("getBigDecimal - index : ", id.ToString(), " value:", value.ToString());
			return value;
		}

        public IGeographicNative getGeospatial(int id)
        {
            IGeographicNative value = _gxDbCommand.Db.GetGeospatial(_gxDbCommand, _DR, id - 1);
			TraceRow("getGeospatial - index : ", id.ToString(), " value:", (value != null ? value.ToString() : string.Empty));
			return value;
		}

        public bool wasNull(int id)
        {
            return _gxDbCommand.Db.IsDBNull(_gxDbCommand, _DR, id - 1);
        }

    }
    public class GXFatFieldSetter : IFieldSetter
    {
		protected static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXFatFieldSetter));
		GxCommand _gxDbCommand;
        public GXFatFieldSetter(GxCommand gxDbCommand)
        {
            _gxDbCommand = gxDbCommand;
        }
        public void SetParameter(int id, IGeographicNative parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, _gxDbCommand.Db.Net2DbmsGeo(GXType.Undefined, parm));
        }
		public void SetParameter(int id, IGeographicNative parm, GXType type)
		{

			_gxDbCommand.SetParameter(id - 1, _gxDbCommand.Db.Net2DbmsGeo(type, parm));
		}
		public List<ParDef> ParameterDefinition 
		{
				get{ return _gxDbCommand.ParmDefinition; }
		}
		public void SetParameterObj(int id, object parm)
		{

			_gxDbCommand.SetParameter(id - 1, parm);
		}
		public void SetParameter(int id, bool parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, parm);
        }
        public void SetParameter(int id, Guid parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, parm);
        }
        public void SetParameter(int id, short parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, parm);
        }
        public void SetParameter(int id, int parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, parm);
        }
        public void SetParameter(int id, long parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, parm);
        }
        public void SetParameter(int id, double parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, parm);
        }
        public void SetParameter(int id, Decimal parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, parm);
        }
        public void SetParameter(int id, string parm)
        {
            
            _gxDbCommand.SetParameter(id - 1, parm);
        }
        public void SetParameterLVChar(int id, string parm)
        {
            
            _gxDbCommand.SetParameterLVChar(id - 1, parm);
        }
		public void SetParameterBlob(int id, string parm, bool dbBlob)
		{
			
			_gxDbCommand.SetParameterBlob(id - 1, parm, dbBlob);
		}
		public void SetParameterChar(int id, string parm)
        {
            
            _gxDbCommand.SetParameterChar(id - 1, parm);
        }
        public void SetParameterVChar(int id, string parm)
        {
            
            _gxDbCommand.SetParameterVChar(id - 1, parm);
        }
        public void SetParameterMultimedia(int id, string parm, string multimediaParm)
        {
            SetParameterMultimedia(id, parm, multimediaParm, null, null);
        }
		//Insert image from local file in web transacion, storage enabled, image_gxi = myimage.jpg, image = http://amazon...s3./PrivateTempStorage/myimage.jpg (after a getMultimediaValue(imgTransaction003Image_Internalname, ref  A175TransactionImage, ref  A40000TransactionImage_GXI);
		//Update image from dataprovider (KB image object), storage enabled, image_gxi = "file:///C:/Models/Cahttea/Data017/web/Resources/Carmine/myimage.png", image: ".\\Resources/Carmine/myimage.png"
		//Insert KB image from Dataprovider in dynamic transaction image_gxi vacio, image= .\Resources\myimage.png,
		//Second execution of Dataprovider that updates images, CategoryImage = calendar.Link(), image_gxi=https://chatteatest.s3.amazonaws.com/TestPGXReleased/Category/CategoryImage/calendar_dc0ca2d9335a484cbdc2d21fc7568af7.png, copy falla, multimediaUri = image_gxi;
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
		public void SetParameterMultimedia(int id, string image_gxi, string image, string tableName, string fieldName)
{
			GXLogging.Debug(log, "SetParameterMultimedia image_gxi:", image_gxi + " image:" + image);
			bool storageServiceEnabled = !string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(fieldName) && (GXServices.Instance != null && GXServices.Instance.Get(GXServices.STORAGE_SERVICE) != null);
			string imageUploadName=image;
			if (GxUploadHelper.IsUpload(image))
			{
				imageUploadName = GxUploadHelper.UploadName(image);
				image = GxUploadHelper.UploadPath(image);
			}
			if (GxUploadHelper.IsUpload(image_gxi))
			{
				image_gxi = GxUploadHelper.UploadPath(image_gxi);
			}

			if (String.IsNullOrEmpty(image))
			{
				_gxDbCommand.SetParameter(id - 1, image_gxi);
			}
			else
			{
				string multimediaUri = string.Empty;
				if (storageServiceEnabled)
				{
					//image_gxi not empty => process image_gxi
					if (!String.IsNullOrEmpty(image_gxi))
					{
						if (PathUtil.IsAbsoluteUrl(image_gxi)) //http://, https://, ftp://
						{
							//file is already on the cloud p.e. https://s3.amazonaws.com/Test/PublicTempStorage/multimedia/Image_ad013b5b050c4bf199f544b5561d9b92.png
							//Must be copied to https://s3.amazonaws.com/Test/TableName/FieldName/Image_ad013b5b050c4bf199f544b5561d9b92.png
							if (ServiceFactory.GetExternalProvider().TryGetObjectNameFromURL(image_gxi, out _)) 
							{
								try
								{
									multimediaUri = ServiceFactory.GetExternalProvider().Copy(image_gxi, GXDbFile.GenerateUri(image_gxi, !GXDbFile.HasToken(image_gxi), false), tableName, fieldName, GxFileType.DefaultAttribute);
									GXLogging.Debug(log, "Copy file already in ExternalProvider:", multimediaUri);
								}
								catch (Exception ex)
								{
									multimediaUri = image_gxi;
									//it is trying to copy an object to itself without changing the object's metadata
									GXLogging.Warn(log, ex, "Copy file to itself filed in ExternalProvider:", image_gxi);
								}
							}
							else //image_gxi url is in another cloud
							{
								try
								{
#pragma warning disable SYSLIB0014 // WebClient
									using (var fileStream = new MemoryStream(new WebClient().DownloadData(image_gxi)))
									{
										//Cannot pass Http Stream directly, because some Providers (AWS S3) does not support Http Stream.
										multimediaUri = ServiceFactory.GetExternalProvider().Save(fileStream, GXDbFile.GenerateUri(image_gxi, !GXDbFile.HasToken(image_gxi), false), tableName, fieldName, GxFileType.DefaultAttribute);
										GXLogging.Debug(log, "Upload external file to ExternalProvider:", multimediaUri);
									}
#pragma warning disable SYSLIB0014 // WebClient
								}
								catch (WebException)
								{
									multimediaUri = image_gxi;
								}
							}
						}
						else 
						{
							Uri uri;
							if (Uri.TryCreate(image_gxi, UriKind.Absolute, out uri)) //file://
							{
								string fileFullName = uri.AbsolutePath;
								Stream fileStream = new FileStream(fileFullName, FileMode.Open, FileAccess.Read);
								string fileName = PathUtil.GetValidFileName(fileFullName, "_");
								using (fileStream)
								{
									multimediaUri = ServiceFactory.GetExternalProvider().Save(fileStream, GXDbFile.GenerateUri(fileName, !GXDbFile.HasToken(fileName), false), tableName, fieldName, GxFileType.DefaultAttribute);
									GXLogging.Debug(log, "Upload file (_gxi) to ExternalProvider:", multimediaUri);
								}
							}
							else //relative image_gxi name=> Assume image is a local file on the cloud because storageService is Enabled. 
							{
								try
								{
									string imageRelativePath = image;
									if (StorageFactory.TryGetProviderObjectName(ServiceFactory.GetExternalProvider(), image, out string objectName)) 
									{
										imageRelativePath = objectName;
									}
									multimediaUri = ServiceFactory.GetExternalProvider().Copy(imageRelativePath, GXDbFile.GenerateUri(image_gxi, !GXDbFile.HasToken(image_gxi), false), tableName, fieldName, GxFileType.DefaultAttribute);
									GXLogging.Debug(log, "Copy external file in ExternalProvider:", multimediaUri);
								}
								catch(Exception e)
								{
									GXLogging.Warn(log, e, "Could not copy external file in ExternalProvider:", image);
									//If Image is not in Cloud Storage, then we look if we can find it in the hard drive. This is the case for Relative paths using Image.FromImage()
									//If file is not available, exception must be thrown																	
									multimediaUri = PushToExternalProvider(new FileStream(image, FileMode.Open, FileAccess.Read), GXDbFile.GenerateUri(image_gxi, !GXDbFile.HasToken(image_gxi), false), tableName, fieldName);
									GXLogging.Debug(log, "Upload file to ExternalProvider:", multimediaUri);								
								}
							}
						}
					}
					//image_gxi is empty => process image
					else if (!String.IsNullOrEmpty(image))
					{
						string fileName = PathUtil.GetValidFileName(imageUploadName, "_");

						try
						{
							Stream fileStream;
							if (!PathUtil.IsAbsoluteUrl(image)) //Assume it is a local file 
							{
								image = Path.Combine(GxContext.StaticPhysicalPath(), image);
								fileStream = new FileStream(image, FileMode.Open, FileAccess.Read);
							}
							else
							{
#pragma warning disable SYSLIB0014 // WebClient
								WebClient c = new WebClient();
								fileStream = new MemoryStream(c.DownloadData(image));
								//Cannot pass Http Stream directly, because some Providers (AWS S3) does not support Http Stream.
#pragma warning disable SYSLIB0014 // WebClient
							}
							string externalFileName = GXDbFile.GenerateUri(fileName, !GXDbFile.HasToken(fileName), false);
							multimediaUri = PushToExternalProvider(fileStream, externalFileName, tableName, fieldName);
						}
						catch (WebException)//403 forbidden, parm = url in external provider that has been deleted
						{
							multimediaUri = image_gxi;
						}
					}
				}
				//image_gxi not empty => process image_gxi
				else if (!String.IsNullOrEmpty(image_gxi))
					multimediaUri = GXDbFile.GenerateUri(PathUtil.GetValidFileName(image_gxi, "_"), !GXDbFile.HasToken(image_gxi), true);
				//image_gxi is empty => process image
				else if (!String.IsNullOrEmpty(image))
					multimediaUri = GXDbFile.GenerateUri(PathUtil.GetValidFileName(imageUploadName, "_"), !GXDbFile.HasToken(imageUploadName), true);
				_gxDbCommand.SetParameter(id - 1, multimediaUri);
			}
		}
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'


		private static string PushToExternalProvider(Stream fileStream, string externalFileName, string tableName, string fieldName)
		{
			string multimediaUri;
			using (fileStream)
			{
				multimediaUri = ServiceFactory.GetExternalProvider().Save(fileStream, externalFileName, tableName, fieldName, GxFileType.DefaultAttribute);
				GXLogging.Debug(log, "Upload file to ExternalProvider:", multimediaUri);
			}

			return multimediaUri;
		}

		public void SetParameter(int id, DateTime parm)
        {
            _gxDbCommand.SetParameter(id - 1, _gxDbCommand.Db.Net2DbmsDateTime((IDbDataParameter)_gxDbCommand.Parameters[id - 1], DateTimeUtil.ResetMilliseconds(parm)));
        }
        public void SetParameterDatetime(int id, DateTime parm)
        {
            SetParameterDatetime(id, parm, false);
        }
        public void SetParameterDatetime(int id, DateTime parm, Boolean precision)
        {
            DateTime shifted = parm;
            shifted = DateTimeUtil.Local2DBserver(parm, _gxDbCommand.Conn.ClientTimeZone);
            DateTime param2 = (precision) ? shifted : DateTimeUtil.ResetMilliseconds(shifted);
            _gxDbCommand.SetParameter(id - 1, _gxDbCommand.Db.Net2DbmsDateTime((IDbDataParameter)_gxDbCommand.Parameters[id - 1], param2));
        }
        public void RegisterOutParameter(int id, Object type)
        {
            _gxDbCommand.SetParameterDir(id - 1, ParameterDirection.Output);
        }
        public void RegisterInOutParameter(int id, Object type)
        {
            _gxDbCommand.SetParameterDir(id - 1, ParameterDirection.InputOutput);
        }
        public void setNull(int id, Object sqlType)
        {
            _gxDbCommand.SetParameter(id - 1, DBNull.Value);
        }

		public void SetParameterRT(string name, string value)
		{
			_gxDbCommand.SetParameterRT(name, value);
		}

		public void RestoreParametersRT()
		{
			_gxDbCommand.RestoreParametersRT();
		}
	}
    public class Cursor : ICursor
    {
        protected static readonly ILog log = log4net.LogManager.GetLogger(typeof(Cursor));
        protected int _state = 0;   
        protected string _name;
        protected string _stmt;
        protected string[] _stmtParameters;
        protected string[] _staticParameters;
        protected short _updatable;
        protected short _blockSize;
        protected int _batchSize;
        protected IDataReader _DR;
        protected GxCommand _gxDbCommand;
        protected int _status;
        protected IFieldSetter _fldSetter;
        protected IFieldGetter _fldGetter;
        protected IFieldGetter _fldBufferGetter;
        protected Object[] _colBinds;
        protected Object[] _parmBinds;
        protected GxErrorMask _errMask;
        public static int GX_NOMASK;
        public int TTL;
        protected bool hasNested = true;
        protected bool isForFirst;
        protected bool _closed;
        protected object _cast;
        protected CursorDef _cursorDef;
        public static int EOF = 101;

        public Cursor(string name, string stmt, GxErrorMask nmask, ICollection parmBinds, short blockSize)
        {
            _name = name;
            _stmt = stmt;
            _blockSize = blockSize;
            _state = 1;
            _stmtParameters = Array.Empty<string>();
            
            _parmBinds = (object[])parmBinds;
            _errMask = nmask;
            _closed = true;
            _cursorDef = null;
        }
        public Cursor(string name, string stmt, GxErrorMask nmask, ICollection parmBinds, short blockSize, int timeToLive, bool hasNestedCursor, bool isForFirst)
            : this(name, stmt, nmask, parmBinds, blockSize)
        {
            TTL = timeToLive;
            this.hasNested = hasNestedCursor;
            this.isForFirst = isForFirst;
            _closed = true;
        }

        public Cursor(string name, string stmt, GxErrorMask nmask, ICollection parmBinds, short blockSize, int timeToLive, bool hasNestedCursor, bool isForFirst, String[] staticPars)
            : this(name, stmt, nmask, parmBinds, blockSize, timeToLive, hasNestedCursor, isForFirst)
        {
            _staticParameters = staticPars;
        }

        public void createCursor(IGxDataStore ds, GxErrorHandler errorHandler)
        {

            if (_state >= 2)
                            
            {
                return;
            }
            _stmt = (_staticParameters == null)? _stmt : String.Format(_stmt, _staticParameters);
            _gxDbCommand = new GxCommand(ds.Db, _stmt, _updatable, ds, "", _name, TTL, hasNested, isForFirst, errorHandler, _batchSize);
            _gxDbCommand.IsCursor = true;
            if (_blockSize > 0)
                _gxDbCommand.FetchSize = Convert.ToUInt16(_blockSize);
            bindParms(_parmBinds);
			_gxDbCommand.AfterCreateCommand();
            _fldGetter = new GXFatFieldGetter(_gxDbCommand);
            _fldSetter = new GXFatFieldSetter(_gxDbCommand);
            _state = 2;
            _gxDbCommand.ErrorMask = _errMask;

        }
        protected virtual void bindParms(Object[] ptb)
        {
            int pos = 1;
			if (ptb != null)
			{
				_gxDbCommand.ClearParameters();
				if (ptb.Length > 0)
				{
					//Backward compatibility
					if (ptb[0] is Object[])
					{
						foreach (Object[] p in ptb)
						{
							if (p.Length > 4 && p[4].Equals("rt"))
								continue;
							_gxDbCommand.AddParameter((string)p[0], p[1], (int)(p[2]), (int)(p[3]));
							pos++;
						}
					}
					else
					{
						foreach (ParDef p in ptb)
						{
							if (p.Return)
								continue;
							_gxDbCommand.AddParameter(p.Name, p.GxType, p.Size, p.Scale);
							_gxDbCommand.ParmDefinition.Add(p);
							pos++;
						}
					}
				}
			}
		}
		List<ParDef> ICursor.DynamicParameters => _dynamicParameters;
		List<ParDef> _dynamicParameters = new List<ParDef>();
		protected virtual void bindDynamicParms(Object[] ptb)
		{
			int pos = 1;
			if (ptb != null)
			{
				_dynamicParameters.Clear();
				if (ptb.Length > 0)
				{
					if (ptb[0] is ParDef)
					{
						foreach (ParDef p in ptb)
						{
							if (p.Return)
								continue;
							_dynamicParameters.Add(p);
							pos++;
						}
					}
				}
			}
		}
        public virtual void OnCommitEvent(object instance, string method)
        {
            throw (new GxADODataException("OnCommitEvent operation not allowed in this type of cursor. Cursor" + _name));
        }
        public virtual void addRecord(Object[] parms)
        {
            throw (new GxADODataException("AddRecord operation not allowed in this type of cursor. Cursor" + _name));
        }
        public int RecordCount
        {
            get { return _gxDbCommand.RecordCount; }
        }
        public virtual short[] preExecute(int cursorNum, IDataStoreProviderBase connectionProvider, IGxDataStore ds)
        {
            _status = 0;
            return null;
        }

        public void setDynamicOrder(string[] parameters) {
            _cursorDef.setDynamicOrder(parameters);
        }
        public virtual void execute()
        {
            throw (new GxADODataException("execute operation not allowed in this type of cursor. Cursor" + _name));
        }
        public virtual void readNext()
        {
            throw (new GxADODataException("readNext operation not allowed in this type of cursor. Cursor" + _name));
        }
        public virtual int readNextErrorRecord()
        {
            throw (new GxADODataException("readNext operation not allowed in this type of cursor. Cursor" + _name));
        }
        public virtual void executeBatch()
        {
            throw (new GxADODataException("executeBatch operation not allowed in this type of cursor. Cursor" + _name));
        }
        public virtual int getStatus()
        {
            return _status;
        }
        public virtual int close()
        {
            if (_gxDbCommand != null && !_closed)
            {
                _gxDbCommand.Close();
                _closed = true;
            }
            return 0;
        }
        public IFieldGetter getFieldGetter()
        {
            return _fldGetter;
        }
        public IFieldGetter getBufferFieldGetter()
        {
            return _fldBufferGetter;
        }
        public IFieldSetter getFieldSetter()
        {
            return _fldSetter;
        }
        public string SQLStatement
        {
            get { return _stmt; }
        }
        public string Id
        {
            get { return _name; }
        }
        public int BatchSize
        {
            get
            {
                return _batchSize;
            }
            set
            {
                _batchSize = value;
            }
        }
    }
    public class CursorDef
    {
        private string _name;
        private string _stmt;
        private string[] _staticParameters;
        private GxErrorMask _nmask;
        private IDataStoreHelper _parent;
        private Object[] _parmBinds;
        private short _blockSize;
        private int _timeToLive;
        private bool _hasNested;
        private bool _isForFirst;

        public CursorDef(string name, string stmt, bool current, GxErrorMask nmask, bool hold,
            IDataStoreHelper parent, Object[] parmBinds, short blockSize, int cachingCategory, bool hasNested)
        {
            _name = name;
            _stmt = stmt;
            _nmask = nmask;
            _parent = parent;
            _parmBinds = parmBinds;
            _blockSize = blockSize;
            _timeToLive = (int)Preferences.CachingTTLs(cachingCategory);
            _hasNested = hasNested;
            
        }
        public CursorDef(string name, string stmt, bool current, GxErrorMask nmask, bool hold,
          IDataStoreHelper parent, Object[] parmBinds, short blockSize, int cachingCategory, bool hasNested,
          bool isForFirst) : this(name, stmt, current, nmask, hold, parent, parmBinds, blockSize,
          cachingCategory, hasNested)
        {
            _isForFirst = isForFirst;

        }
        public CursorDef(string name, string stmt, bool current, GxErrorMask nmask, bool hold,
            IDataStoreHelper parent, Object[] parmBinds, short blockSize, int cachingCategory, bool hasNested,
            bool isForFirst,  string[] staticParameters) : this(name, stmt, current, nmask, hold, parent, parmBinds, blockSize,
            cachingCategory, hasNested, isForFirst)
        {            
            _staticParameters = staticParameters;

        }
        public CursorDef(string name, string stmt, GxErrorMask nmask, Object[] parmBinds)
        {
            _name = name;
            _stmt = stmt;
            _nmask = nmask;
            _parmBinds = parmBinds;
        }
        public CursorDef(string name, string stmt, GxErrorMask nmask, Object[] parmBinds, short blockSize)
        {
            
            _name = name;
            _stmt = stmt;
            _nmask = nmask;
            _parmBinds = parmBinds;
            
        }
        public bool IsForFirst
        {
            get { return _isForFirst; }
        }
        public string Name
        {
            get { return _name; }
        }
        public string Stmt
        {
            get { return _stmt; }
        }
        public GxErrorMask Nmask
        {
            get { return _nmask; }
        }
        public ICollection ParmBinds
        {
            get { return _parmBinds; }
        }
        public short BlockSize
        {
            get { return _blockSize; }
        }
        public int TimeToLive
        {
            get { return _timeToLive; }
        }
        public bool HasNested
        {
            get { return _hasNested; }
        }
        public IDataStoreHelper Parent
        {
            get { return _parent; }
        }

        public void setDynamicOrder(String[] parameters)
        {
            _staticParameters = parameters;
        }

        public string[] getDynamicOrder()
        {
            return _staticParameters;
        }
    }

    public class ForEachCursor : Cursor
    {
        private bool dynamicStmt;
        IDataStoreHelper parent;

        public ForEachCursor(string name, string stmt, bool current, GxErrorMask nmask, bool hold, Object obj, Object[] parmBinds, short blockSize) : base(name, stmt, nmask,  parmBinds, blockSize)
        {
            throw (new GxNotImplementedException());
            
        }
        public ForEachCursor(string name, string stmt, bool current, GxErrorMask nmask, bool hold, Object obj, Object[] parmBinds, short blockSize, int timeToLive, bool hasNested) : base(name, stmt, nmask,  parmBinds, blockSize, timeToLive, hasNested, false)
        {
            throw (new GxNotImplementedException());
            
        }

        public ForEachCursor(string name, string stmt, bool current, GxErrorMask nmask, bool hold, Object obj, Object[] parmBinds, short blockSize, int timeToLive) : base(name, stmt, nmask,  parmBinds, blockSize, timeToLive, true, false)
        {
            throw (new GxNotImplementedException());
            
        }

        public ForEachCursor(CursorDef def) : base(def.Name, def.Stmt, def.Nmask, def.ParmBinds, def.BlockSize, def.TimeToLive, def.HasNested, def.IsForFirst, def.getDynamicOrder())
        {
            dynamicStmt = def.Stmt.Length == 7 && def.Stmt.ToLower().Equals("scmdbuf");
            parent = def.Parent;
            _cursorDef = def;
        }
        protected override void bindParms(Object[] ptb)
        {
            if (!dynamicStmt)
                base.bindParms(ptb);
        }
        public override short[] preExecute(int cursorNum, IDataStoreProviderBase connectionProvider, IGxDataStore ds)
        {
            base.preExecute(cursorNum, connectionProvider, ds);
            short[] parmHasValue = null;
            
            if (dynamicStmt)
            { 
                Object[] dynStmt = parent.getDynamicStatement(cursorNum, connectionProvider.context, connectionProvider.getDynConstraints());
                if (dynStmt == null && parent is DataStoreHelperBase)
                    dynStmt = ((DataStoreHelperBase)parent).getDynamicStatement(cursorNum, connectionProvider.getDynConstraints());
                _stmt = (string)dynStmt[0];

				bindDynamicParms(_parmBinds);
                List<object> newParmBinds = new List<object>();
                parmHasValue = (short[])dynStmt[1];
                for (int i = 0; i < _parmBinds.Length; i++)
                {
                    if (parmHasValue[i] == 0)
                        newParmBinds.Add(_parmBinds[i]);
                }
                base.bindParms(newParmBinds.ToArray());
                GXLogging.Debug(log, "ForEachCursor.preExecute, DynamicStatement: " + _stmt);
                _gxDbCommand.CommandText = _stmt;
				_gxDbCommand.AfterCreateCommand();
            }
            _gxDbCommand.DynamicStmt = dynamicStmt;
            _gxDbCommand.CursorDef = _cursorDef;
            return parmHasValue;
        }

        public override void execute()
        {
            if (_state < 2)
                throw (new GxADODataException("Could not execute ForEachCursor:" + _name + " not defined."));
            _status = 0;

            try
            {
                if (!_closed)
                    close();
            }
            catch (Exception e)
            {
                GXLogging.Error(log, "ForEachCursor.Execute Error at CloseCursor'", e);
            }
            if (dynamicStmt)
                _gxDbCommand.DelDupPars();
            _gxDbCommand.FetchData(out _DR);
            _gxDbCommand.Conn.UncommitedChanges = true;
            _fldGetter.DataReader = _DR;

            if (_gxDbCommand.Status == 1 || _gxDbCommand.Status == 103 || _gxDbCommand.Status == 500)
            {
                _status = _gxDbCommand.Status;
            }
            else if (!_gxDbCommand.HasMoreRows)
                _status = Cursor.EOF;
            _closed = false;
        }
        public override void readNext()
        {
            if (_state < 2)
                throw (new GxADODataException("Could not readNext in ForEachCursor:" + _name + "."));
            _status = 0;

            if (!_DR.Read())
            {
                _status = Cursor.EOF;
                _gxDbCommand.HasMoreRows = false;
            }
            else if (_gxDbCommand.Status == 1 || _gxDbCommand.Status == 103 || _gxDbCommand.Status == 500)
            {
                _status = _gxDbCommand.Status;
            }

        }
    }
    public class UpdateCursor : Cursor
    {
        public UpdateCursor(CursorDef def) : base(def.Name, def.Stmt, def.Nmask, def.ParmBinds, 0)
        {
            _updatable = 1;
            _cursorDef = def;
        }
        public override void execute()
        {
            GXLogging.Debug(log, "UpdateCursor.Execute, name'" + _name + "'");
            if (_state < 2)
                throw (new GxADODataException("Could not execute UpdateCursor:" + _name + " not defined."));
            _status = 0;
            _gxDbCommand.CursorDef = _cursorDef;
            _gxDbCommand.ExecuteStmt(); 

            if (_gxDbCommand.Status == 1 || _gxDbCommand.Status == 103 || _gxDbCommand.Status == 500)
            {
                _status = _gxDbCommand.Status;
            }
            else if (!_gxDbCommand.HasMoreRows)
                _status = Cursor.EOF;
            _closed = false;
        }

    }
    public class BatchUpdateCursor : Cursor
    {
        public BatchUpdateCursor(CursorDef def)
            : base(def.Name, def.Stmt, def.Nmask, def.ParmBinds, 0)
        {
            _updatable = 1;
            _cursorDef = def;
        }
        public override void execute()
        {
            GXLogging.Debug(log, "BatchUpdateCursor.Execute, name'" + _name + "'");
            if (_state < 2)
                throw (new GxADODataException("Could not execute BatchUpdateCursor:" + _name + " not defined."));
            _status = 0;
            _gxDbCommand.ExecuteBatch();
            _gxDbCommand.Conn.UncommitedChanges = true;

            if (_gxDbCommand.Status == 1 || _gxDbCommand.Status == 103 || _gxDbCommand.Status == 500)
            {
                _status = _gxDbCommand.Status;
            }
            else if (!_gxDbCommand.HasMoreRows)
                _status = Cursor.EOF;
            _closed = false;
            _fldBufferGetter = new GXFatErrorFieldGetter(_gxDbCommand);
        }

        public override int readNextErrorRecord()
        {
            return _gxDbCommand.ReadNextErrorRecord();
        }
        public override void OnCommitEvent(object instance, string method)
        {
            _gxDbCommand.OnCommitEvent(instance, method);
        }
        public override void addRecord(Object[] parms)
        {
            _gxDbCommand.AddRecord(parms);
        }
    }
    public class DirectStatement : Cursor
    {
        public DirectStatement(string stmt, GxErrorMask nmask, Object[] parmBinds, short blockSize) : base("", stmt, nmask,  parmBinds, blockSize)
        {
        }
    }
    public class CallCursor : Cursor
    {
        public CallCursor(CursorDef def) : this(def.Name, def.Stmt, def.Nmask, def.ParmBinds)
        {
            _updatable = 1;
            isForFirst = true;
            hasNested = false;
            _cursorDef = def;
        }
        public CallCursor(string name, string stmt, GxErrorMask nmask, ICollection parmBinds)
            : base(name, stmt, nmask,  parmBinds, 1)
        {
        }

        public override void execute()
        {
            GXLogging.Debug(log, "CallCursor.Execute, name'" + _name + "'");
            if (_state < 2)
                throw (new GxADODataException("Could not execute CallCursor:" + _name + " not defined."));
            _status = 0;

            _gxDbCommand.FetchDataRPC(out _DR);
            _gxDbCommand.Conn.UncommitedChanges = true;

            _fldGetter.DataReader = _DR;


            if (_gxDbCommand.Status == 1 || _gxDbCommand.Status == 103 || _gxDbCommand.Status == 500)
            {
                _status = _gxDbCommand.Status;
            }
            else if (!_gxDbCommand.HasMoreRows)
                _status = Cursor.EOF;
            _closed = false;
        }
        public override void readNext()
        {
            if (_state < 2)
                throw (new GxADODataException("Could not readNext in CallCursor:" + _name + "."));
            _status = 0;

            if (!_DR.Read())
            {
                _status = Cursor.EOF;
                _gxDbCommand.HasMoreRows = false;
            }
            else if (_gxDbCommand.Status == 1 || _gxDbCommand.Status == 103 || _gxDbCommand.Status == 500)
            {
                _status = _gxDbCommand.Status;
            }

        }

    }
}