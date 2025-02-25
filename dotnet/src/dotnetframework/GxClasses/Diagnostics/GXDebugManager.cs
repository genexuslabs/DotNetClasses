//#define _DEBUG_DEBUGGER
//#define _LOG_WRITER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using GeneXus.Application;
using static GeneXus.Diagnostics.GXDebugStream;

namespace GeneXus.Diagnostics
{
	public class GXDebugManager
	{
		internal const string _LOG_WRITER = "_LOG_WRITER";
		public const short GXDEBUG_VERSION = 2;
#if NETCORE 
		internal const GXDebugGenId GENERATOR_ID = GXDebugGenId.NET;
#else
		internal const GXDebugGenId GENERATOR_ID = GXDebugGenId.NETFRAMEWORK;
#endif

		internal const int PGM_INFO_NO_PARENT = 0;
		internal static double MICRO_FREQ = Stopwatch.Frequency / 10e6D;

		private static bool initialized;
		private static GXDebugManager m_Instance;
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxContext>();
		public static GXDebugManager Instance
		{
			get
			{
				if (!initialized)
				{
					lock (sessionLock)
					{
						if (!initialized)
						{
							initialized = true;
							try
							{
								m_Instance = new GXDebugManager(); 
							}catch(Exception ex)
							{
								GXLogging.Error(log, $"GXDebugManager", ex);
							}
						}
					}
				}
				return m_Instance;
			}
		}
		private static int BUFFER_INITIAL_SIZE = 16383;
		private const long TICKS_NOT_SET = long.MaxValue;
		private const long TICKS_NOT_NEEDED = 0;
		private static string FileName = $"gxperf.gxd";

		private static readonly char[] InvalidFilenameChars = Path.GetInvalidFileNameChars();
		private GXDebugManager()
		{
			Current = new GXDebugItem[BUFFER_INITIAL_SIZE];
			Next = new GXDebugItem[BUFFER_INITIAL_SIZE];
			for(int i = 0; i < BUFFER_INITIAL_SIZE; i++)
			{
				Current[i] = new GXDebugItem();
				Next[i] = new GXDebugItem();
			}
			SessionGuid = Guid.NewGuid();
			waitSaveEvent = new AutoResetEvent(false);
			PushSystem((int)GXDebugMsgCode.INITIALIZE, DateTime.UtcNow);
		}

		internal GXDebugInfo GetDbgInfo(IGxContext context, int objClass, int objId, int dbgLines, long hash)
		{
			lock (sessionLock)
			{
				GXDebugInfo dbgInfo = new GXDebugInfo(NewSId(), context, new KeyValuePair<int, int>(objClass, objId));
				if (!pgmInfoTable.Contains(dbgInfo.Key))
				{
					KeyValuePair<int, long> pgmInfoObj = new KeyValuePair<int, long>(dbgLines, hash);
					pgmInfoTable.Add(dbgInfo.Key);
					PushSystem(GXDebugMsgCode.PGM_INFO.ToByte(), new KeyValuePair<object, object>(dbgInfo.Key, pgmInfoObj));
				}
				if (parentTable.TryGetValue(context.ClientID, out GXDebugInfo parentDbgInfo))
				{
					dbgInfo.Parent = parentDbgInfo;
				}
				dbgInfo.RegisterPgm(parentDbgInfo);
				parentTable[context.ClientID] = dbgInfo;
				return dbgInfo;
			}
		}

		private Guid SessionGuid;
		private int LastSId;
		private int NewSId() => Interlocked.Increment(ref LastSId);

		private GXDebugItem[] Current, Next, ToSave;
		private bool saving = false;
		private int dbgIndex = 0;
		private readonly object saveLock = new object();
		private readonly object mSaveLock = new object();
		private static readonly object sessionLock = new object();
		private ConcurrentDictionary<string, GXDebugInfo> parentTable = new ConcurrentDictionary<string, GXDebugInfo>();
		private HashSet<KeyValuePair<int, int>> pgmInfoTable = new HashSet<KeyValuePair<int, int>>();
		private AutoResetEvent waitSaveEvent;

		internal GXDebugItem PushSystem(int cmdCode, object arg = null) => mPush(null, GXDebugMsgType.SYSTEM, cmdCode, 0, arg);
		internal GXDebugItem Push(GXDebugInfo dbgInfo, int lineNro, int colNro = 0) => mPush(dbgInfo, GXDebugMsgType.PGM_TRACE, lineNro, colNro, null);
		internal GXDebugItem PushPgm(GXDebugInfo dbgInfo, int ParentSId, KeyValuePair<int, int> PgmKey) => mPush(dbgInfo, GXDebugMsgType.REGISTER_PGM, ParentSId, 0, PgmKey);
		internal GXDebugItem PushRange(GXDebugInfo dbgInfo, int lineNro, int colNro, int lineNro2, int colNro2)
		{
			if((colNro != 0 || colNro2 != 0))
				return mPush(dbgInfo, GXDebugMsgType.PGM_TRACE_RANGE_WITH_COLS, lineNro, lineNro2, new KeyValuePair<int, int>(colNro, colNro2));
			else return mPush(dbgInfo, GXDebugMsgType.PGM_TRACE_RANGE, lineNro, lineNro2, null);
		}

		private GXDebugItem mPush(GXDebugInfo dbgInfo, GXDebugMsgType msgType, int arg1, int arg2, object argObj = null)
		{
			lock (saveLock)
			{
				if (ToSave != null)
				{
					Save(ToSave);
					ToSave = null;
				}
				GXDebugItem currentItem = Current[dbgIndex];
				currentItem.DbgInfo = dbgInfo;
				currentItem.MsgType = msgType;
				currentItem.Arg1 = arg1;
				currentItem.Arg2 = arg2;
				currentItem.ArgObj = argObj;
				switch(msgType)
				{
					case GXDebugMsgType.SYSTEM:
						{
							switch ((GXDebugMsgCode)arg1)
							{
								case GXDebugMsgCode.INITIALIZE:
								case GXDebugMsgCode.EXIT:
								case GXDebugMsgCode.OBJ_CLEANUP:
								case GXDebugMsgCode.PGM_INFO:
									currentItem.Ticks = TICKS_NOT_NEEDED;
									break;
								default: currentItem.Ticks = TICKS_NOT_SET; break;
							}
						}
						break;
					case GXDebugMsgType.REGISTER_PGM:
						{
							currentItem.Ticks = TICKS_NOT_NEEDED;
						}
						break;
					default:
						currentItem.Ticks = TICKS_NOT_SET;
						break;
				}
				dbgIndex++;
				if (dbgIndex == Current.Length)
				{
					bool mSaving = false;
					lock (mSaveLock)
					{
						mSaving = saving;
					}
					if(mSaving)
						waitSaveEvent.WaitOne();
					ToSave = Current;
					GXDebugItem[] swap = Current;
					Current = Next;
					Next = swap;
					pgmInfoTable.Clear();
					dbgIndex = 0;
				}
				return currentItem;
			}
		}

		private void Save(GXDebugItem[] ToSave, int saveTop = -1, bool saveInThread = true)
		{
			int saveCount = 0;
			if (saveTop == -1)
			{
				ToSave = Next;
				saveTop = ToSave.Length;
				for (int idx = 0; idx < saveTop; idx++)
				{
					if (ToSave[idx].Ticks == TICKS_NOT_SET)
					{ 
						GXDebugItem swap = ToSave[idx];
						ToSave[idx] = Current[dbgIndex];
						Current[dbgIndex] = swap;
						ClearDebugItem(ToSave[idx]);
						ToSave[idx].MsgType = GXDebugMsgType.SKIP;
						ToSave[idx].ArgObj = swap;
						dbgIndex++;
						if (dbgIndex == Current.Length)
						{ 
							int lastTop = Current.Length;
							GXDebugItem[] tempL = new GXDebugItem[lastTop + BUFFER_INITIAL_SIZE];
							Array.Copy(Current, tempL, lastTop);
							Current = tempL;
							tempL = new GXDebugItem[lastTop + BUFFER_INITIAL_SIZE];
							Array.Copy(Next, tempL, lastTop);
							Next = tempL;
							for (int i = lastTop; i < Current.Length; i++)
							{
								Current[i] = new GXDebugItem();
								Next[i] = new GXDebugItem();
							}
						}
					}
					else saveCount++;
				}
			}
			else if(saveTop == 0)
				return;
			else saveCount = saveTop;
			lock (mSaveLock)
			{
				Debug.Assert(!saving, "Already saving");
				saving = true;
			}
			if(saveInThread)
				ThreadPool.QueueUserWorkItem(mSave, new object[] { ToSave, saveTop, saveCount });
			else mSave(new object[] { ToSave, saveTop, saveCount });
		}

		public static void OnExit()
		{
			if(initialized)
			{
				Instance.OnExit(null);
			}
		}

		internal void OnExit(GXDebugInfo dbgInfo)
		{
			PushSystem((int)GXDebugMsgCode.EXIT);
			lock (saveLock)
			{
				for (int i = 0; i < dbgIndex; i++)
				{
					if (Current[i].Ticks == TICKS_NOT_SET)
						Current[i].Ticks = TICKS_NOT_NEEDED;
				}
			}
			Save();
		}

		internal void OnCleanup(GXDebugInfo dbgInfo)
		{
			PushSystem((int)GXDebugMsgCode.OBJ_CLEANUP, dbgInfo.SId);
			lock (sessionLock)
			{
				if (dbgInfo.Parent != null)
					parentTable[dbgInfo.context.ClientID] = dbgInfo.Parent;
				else
				{
					parentTable.TryRemove(dbgInfo.context.ClientID, out GXDebugInfo oldParent);
					if(!GxContext.Current.IsStandalone)
						Save();
				}
			}
		}

		private void Save()
		{
			lock (saveLock)
			{
				if (ToSave != null)
				{
					Save(ToSave);
					ToSave = null;
				}
				Save(Current, dbgIndex, false);
				dbgIndex = 0;
			}
		}

		private void ClearDebugItem(GXDebugItem dbgItem)
		{
			dbgItem.MsgType = GXDebugMsgType.INVALID;
			dbgItem.Arg1 = 0;
			dbgItem.Arg2 = 0;
			dbgItem.ArgObj = null;
		}

		private void mSave(object state1)
		{
			object[] state = state1 as object[];
			GXDebugItem[] Data = null;
			int saveTop=0, saveCount=0;
			if (state != null)
			{
				Data = state[0] as GXDebugItem[];
				saveTop = (int)state[1];
				saveCount = (int)state[2];
			}
			if (Data != null)
			{
				lock (mSaveLock)
				{

					int idx = 0;
					try
					{
						string FQFileName = Path.IsPathRooted(FileName) ? FileName : Path.Combine(GxContext.Current.GetPhysicalPath(), FileName);
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
						using (GXDebugStream stream = new GXDebugStream(FQFileName, FileMode.Append))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
						{
							stream.WriteHeader(SessionGuid, (short)(GXDEBUG_VERSION << 4 | GENERATOR_ID.ToByte()), saveCount);
#if _DEBUG_DEBUGGER
						Console.WriteLine("mSave-" + saveTop);
#endif
							LogWriter.Append($"\n  SaveTop = {saveTop}\n");
							for (; idx < saveTop; idx++)
							{
								GXDebugItem dbgItem = Data[idx];
#if _DEBUG_DEBUGGER
							Console.WriteLine($"item({idx}): { dbgItem }");
#endif
								LogWriter.Log($"<{idx}:{dbgItem}>");
								switch (dbgItem.MsgType)
								{
									case GXDebugMsgType.SYSTEM:
										{
											stream.WriteByte((byte)(dbgItem.MsgType.ToByte() | ((GXDebugMsgCode)dbgItem.Arg1).ToByte()));
											switch ((GXDebugMsgCode)dbgItem.Arg1)
											{
												case GXDebugMsgCode.INITIALIZE:
													stream.WriteLong(((DateTime)dbgItem.ArgObj).ToUniversalTime().Ticks);
													break;
												case GXDebugMsgCode.OBJ_CLEANUP:
													stream.WriteVLUInt((int)dbgItem.ArgObj);
													break;
												case GXDebugMsgCode.EXIT:
													break;
												case GXDebugMsgCode.PGM_INFO:
													KeyValuePair<object, object> info = (KeyValuePair<object, object>)dbgItem.ArgObj;
													stream.WriteVLUInt(((KeyValuePair<int, int>)info.Key).Key);
													stream.WriteVLUInt(((KeyValuePair<int, int>)info.Key).Value);
													stream.WriteVLUInt(((KeyValuePair<int, long>)info.Value).Key);
													stream.WriteInt(((KeyValuePair<int, long>)info.Value).Value);
													break;
												default:
													throw new ArgumentException($"Invalid DbgItem: {dbgItem}");
											}
										}
										break;
									case GXDebugMsgType.PGM_TRACE:
										{
											stream.WritePgmTrace(dbgItem.DbgInfo.SId, dbgItem.Arg1, dbgItem.Arg2, dbgItem.Ticks);
										}
										break;
									case GXDebugMsgType.PGM_TRACE_RANGE:
									case GXDebugMsgType.PGM_TRACE_RANGE_WITH_COLS:
										{
											stream.WriteByte(dbgItem.MsgType.ToByte());
											stream.WriteVLUInt(dbgItem.DbgInfo.SId);
											stream.WriteVLUInt(dbgItem.Arg1);
											stream.WriteVLUInt(dbgItem.Arg2);
											if (dbgItem.MsgType == GXDebugMsgType.PGM_TRACE_RANGE_WITH_COLS)
											{
												stream.WriteVLUInt(((KeyValuePair<int, int>)dbgItem.ArgObj).Key);
												stream.WriteVLUInt(((KeyValuePair<int, int>)dbgItem.ArgObj).Value);
											}
										}
										break;
									case GXDebugMsgType.REGISTER_PGM:
										{
											stream.WriteByte(dbgItem.MsgType.ToByte());
											stream.WriteVLUInt(dbgItem.DbgInfo.SId);
											stream.WriteVLUInt(dbgItem.Arg1);
											stream.WriteVLUInt(((KeyValuePair<int, int>)dbgItem.ArgObj).Key);
											stream.WriteVLUInt(((KeyValuePair<int, int>)dbgItem.ArgObj).Value);
										}
										break;
									case GXDebugMsgType.SKIP:
										continue;
								}
								ClearDebugItem(dbgItem);
								LogWriter.LogPosition(stream);
							}
							LogWriter.Append($"\n ENDSAVE ");
						}
						LogWriter.Flush();
					}
					catch (Exception ex)
					{
						GXLogging.Warn(log, $"GXDebugManager: Cannot write debug file", ex);
					}

					saving = false;
					waitSaveEvent.Set();
				}
			}
		}

		internal static void Config(string configInfo)
		{
			FileName = configInfo;
			if (FileName.IndexOfAny(InvalidFilenameChars) >= 0)
				throw new ArgumentException($"GXDebugManager: Invalid File Name { configInfo }");
		}
	}

	internal class GXDebugItem
	{
		public IGXDebugInfo DbgInfo;
		public int Arg1;
		public int Arg2;
		public object ArgObj;
		public long Ticks;

		public GXDebugMsgType MsgType { get; internal set; }

		public override string ToString() => $"{ MsgType }/{ DbgInfo?.SId }:{ ToStringArg1() }-{ Arg2 }-{ ArgObj }{ ToStringTicks() }";
		private string ToStringArg1() => MsgType == GXDebugMsgType.SYSTEM ? $"{ (GXDebugMsgCode)Arg1 }" : $"{ Arg1 }";
		private string ToStringTicks() => MsgType == GXDebugMsgType.PGM_TRACE ? $" elapsed:{ Ticks }" : string.Empty;
	}

	[Flags]
	internal enum GXDebugMsgType : byte
	{
		SYSTEM = 0x80,
		PGM_TRACE = 0x00,
		REGISTER_PGM = SYSTEM | 0x40,
		PGM_TRACE_RANGE = SYSTEM | 0x20,
		PGM_TRACE_RANGE_WITH_COLS = PGM_TRACE_RANGE | 0x01,
		INVALID = 0xFE,  
		SKIP = 0xFF,     
		TRACE_HAS_COL = 0x40,
		TRACE_HAS_SID  = 0x30,
		TRACE_HAS_LINE1 = 0x08,
	}

	internal enum GXDebugMsgCode : byte
	{
		INITIALIZE = 0,
		OBJ_CLEANUP = 1,
		EXIT = 2,
		PGM_INFO = 3,

		MASK_BITS = 0x3
	}

	internal enum GXDebugGenId : byte
	{
		NETFRAMEWORK = 1,
		JAVA = 2,
		NET = 3,

		INVALID = 0xF
	}

	public interface IGXDebugInfo
	{
		int SId { get; set; }
	}

	public class GXDebugInfoLocal : IGXDebugInfo
	{
		public int SId { get; set; }
	}

	public class GXDebugInfo : IGXDebugInfo
	{
		internal IGxContext context;
		internal KeyValuePair<int, int> Key { get; private set; }
		public GXDebugInfo Parent { get; internal set; }
		public int SId { get; set ; }

		internal Stopwatch stopwatch;
		internal GXDebugItem LastItem;

		public GXDebugInfo(int SId, IGxContext context, KeyValuePair<int, int> dbgKey)
		{
			this.SId = SId;
			this.context = context;
			Key = dbgKey;
			LastItem = null;
			stopwatch = new Stopwatch();
		}

		internal void Trk(int lineNro, int colNro = 0)
		{
			UpdateTicks();
			LastItem = GXDebugManager.Instance.Push(this, lineNro, colNro);
			stopwatch.Restart();
		}

		internal void TrkRng(int lineNro, int colNro, int lineNro2, int colNro2)
		{
			UpdateTicks();
			LastItem = GXDebugManager.Instance.PushRange(this, lineNro, colNro, lineNro2, colNro2);
			stopwatch.Restart();
		}

		internal void OnExit()
		{
			UpdateTicks();
			GXDebugManager.Instance.OnExit(this);
		}

		internal void RegisterPgm(GXDebugInfo parentDbgInfo)
		{
			GXDebugManager.Instance.PushPgm(this, parentDbgInfo != null ? parentDbgInfo.SId : GXDebugManager.PGM_INFO_NO_PARENT, Key);
			if(parentDbgInfo != null)
			{
				parentDbgInfo.UpdateTicks();
				parentDbgInfo.LastItem = null;
				parentDbgInfo.stopwatch.Restart();
			}
		}

		internal void OnCleanup()
		{
			UpdateTicks();
			GXDebugManager.Instance.OnCleanup(this);
		}

		internal void UpdateTicks()
		{
			if (LastItem != null)
				LastItem.Ticks = (long)(stopwatch.ElapsedTicks / GXDebugManager.MICRO_FREQ);
		}

	}

	static class ESCAPE_METHODS
	{
		public static byte ToByte(this GXDebugStream.ESCAPE escape)
		{
			return (byte)escape;
		}

		public static byte ToByte(this GXDebugMsgType item)
		{
			return (byte)item;
		}

		public static byte ToByte(this GXDebugMsgCode code)
		{
			return (byte)code;
		}

		public static byte ToByte(this GXDebugGenId genId)
		{
			return (byte)genId;
		}
	}

	internal class GXDebugStream : FileStream
	{
		public enum ESCAPE : byte
		{
			PROLOG = 0,
			EPILOG = 1,
			TRIPLE_FF = 3
		}

		public static readonly byte[] PROLOG = { 0xFF, 0xFF, 0xFF, ESCAPE.PROLOG.ToByte() };
		public static readonly byte[] EPILOG = { 0xFF, 0xFF, 0xFF, ESCAPE.EPILOG.ToByte() };

		public GXDebugStream(string FileName, FileMode fileMode) : base(FileName, fileMode, fileMode == FileMode.Open ? FileAccess.Read : (fileMode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None)
		{
			LogWriter.SetLogName($"{FileName}.log");
			Last = 0;
			LastLast = 0;
			InitializeNewBlock();
		}

		private bool _disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if(base.CanWrite)
					WriteEpilog();
			}
			_disposed = true;
			base.Dispose(disposing);
		}

		private void WriteProlog(short version)
		{
			WriteRaw(PROLOG, 0, PROLOG.Length);
			WriteVLUShort(version);
		}

		private void WriteEpilog()
		{
			WriteRaw(EPILOG, 0, EPILOG.Length);
		}

		public void Write(byte[] data) => Write(data, 0, data.Length);
		public void WriteRaw(byte[] data, int from, int length)
		{
			LogWriter.LogWriteRaw(data, from, length);
			base.Write(data, from, length);
		}
		public void WriteRaw(byte value)
		{
			LogWriter.LogWriteByte(value);
			base.WriteByte(value);
		}

		public override void Write(byte[] data, int offset, int count)
		{
			while (count-- > 0)
				WriteByte(data[offset++]);
		}

		private readonly byte[] SINGLE_BUFFER = new byte []{ 0 };
		public int ReadRaw() => base.Read(SINGLE_BUFFER, 0, 1) > 0 ? SINGLE_BUFFER[0] : -1;

		private byte Last, LastLast;
		public override void WriteByte(byte value)
		{
			LogWriter.LogWriteByte(value);
			base.WriteByte(value);
			if (value == 0xFF &&
			   value == Last &&
			   value == LastLast)
			{
				LogWriter.Append("<3xFF> ");
				WriteRaw(ESCAPE.TRIPLE_FF.ToByte());
				Last = LastLast = 0;
			}
			else
			{
				LastLast = Last;
				Last = value;
			}
		}

		public void WriteVLUInt(int value)
		{
			if (value < 0) throw new ArgumentException("Cannot handle negative values");
			else if (value > 0x1FFFFFFFL)
				throw new ArgumentException("Cannot handle > 29bit values");
			LogWriter.LogWrite("VLUI", value);
			if (value < 0x80)
				WriteByte((byte)value);
			else if (value < 0x4000)
				WriteVLUShort((short)(value & 0x3FFF));
			else
			{
				WriteVLUShort((short)(value & 0x3FFF | 0x4000) );
				WriteVLUShort((short)(value >> 14));
			}
		}

		public void WriteVLUShort(short value)
		{
			if (value < 0) throw new ArgumentException("Cannot handle negative values");
			LogWriter.LogWrite("VLUS", value);
			if (value < 0x80)
				WriteByte((byte)value);
			else
			{
				WriteByte((byte)((value & 0x7F) | 0x80));
				WriteByte((byte)(value >> 7));
			}
		}

		public void WriteHeader(Guid sessionGuid, short version, int saveCount)
		{
			LogWriter.Log("Header", ( "Guid", sessionGuid), ( "Version", version), ("SaveCount", saveCount));
			LogWriter.LogPosition(this);

			WriteProlog(version);
			WriteVLUInt(saveCount);
			Write(sessionGuid.ToByteArray());
		}

		public void WriteLong(long value)
		{
			for (int i = 0; i < 8; i++)
			{
				WriteByte((byte)(value & 0xFF));
				value >>= 8;
			}
		}

		public void WriteInt(long value)
		{
			for (int i = 0; i < 4; i++)
			{
				WriteByte((byte)(value & 0xFF));
				value >>= 8;
			}
		}

		private int LastSId, LastLine1;
		internal void WritePgmTrace(int SId, int line1, int col, long ticks)
		{
			byte cmd = GXDebugMsgType.PGM_TRACE.ToByte();
			if (col != 0)
				cmd |= GXDebugMsgType.TRACE_HAS_COL.ToByte();
			bool hasSId = false;
			switch (SId - LastSId)
			{
				case 0:  break;
				case 1:  cmd |= 0x10; break;
				case -1: cmd |= 0x20; break;
				default: cmd |= GXDebugMsgType.TRACE_HAS_SID.ToByte(); hasSId = true; break;
			}
			int difLine1 = line1 - LastLine1;
			bool hasLine1 = false;
			if (difLine1 < 8 && difLine1 > -8)
				cmd |= (byte)(difLine1 & 0x0F);
			else
			{
				cmd |= GXDebugMsgType.TRACE_HAS_LINE1.ToByte();
				hasLine1 = true;
			}
			WriteByte(cmd);
			WriteScaledLong(ticks);
			if (hasSId)
				WriteVLUInt(SId);
			if (hasLine1)
				WriteVLUInt(line1);
			if (col != 0)
				WriteVLUInt(col);

			LastSId = SId;
			LastLine1 = line1;
		}

		public void WriteScaledLong(long N)
		{
			if (N < 0) throw new ArgumentException("Cannot handle negative values");
			LogWriter.LogWrite("SCAL", N);

			int m = 0;
			while (N > 31)
			{
				N -= 32;
				m++;
				if (m == 8)
				{
					WriteByte(0);
					N++;
					m = 0;
				}
				N >>= 1;
			}
			if(m == 7 && N == 31)
			{
				WriteByte(0);
				WriteByte(0xFF);
			}else WriteByte((byte)~((m << 5) | (byte)N));
		}

		public GXDebugItem ReadPgmTrace(GXDebugItem dbgItem, byte cmd)
		{
			dbgItem = dbgItem ??  new GXDebugItem();
			dbgItem.MsgType = GXDebugMsgType.PGM_TRACE;
			dbgItem.DbgInfo = dbgItem.DbgInfo ?? new GXDebugInfoLocal();
			dbgItem.ArgObj = null;

			GXDebugMsgType cmdMsgType = (GXDebugMsgType)cmd;
			bool hasSId = (cmd & 0x30) == GXDebugMsgType.TRACE_HAS_SID.ToByte();
			bool hasLine1 = (cmd & 0x0F) == GXDebugMsgType.TRACE_HAS_LINE1.ToByte();
			bool hasCol = cmdMsgType.HasFlag(GXDebugMsgType.TRACE_HAS_COL);
			dbgItem.Ticks = ReadScaledLong();
			if (hasSId)
				dbgItem.DbgInfo.SId = ReadVLUInt();
			else
			{
				switch ((cmd >> 4) & 0x3)
				{
					case 0: dbgItem.DbgInfo.SId = LastSId; break;
					case 1: dbgItem.DbgInfo.SId = LastSId + 1; break;
					case 2: dbgItem.DbgInfo.SId = LastSId - 1; break;
				}
			}
			if (hasLine1)
				dbgItem.Arg1 = ReadVLUInt();
			else
			{
				byte difLine1 = (byte)(cmd & 0xF);
				if((difLine1 & 0x08) == 0x08)
					difLine1 |= 0xF0;
				dbgItem.Arg1 = LastLine1 + (sbyte)difLine1;
			}
			if (hasCol)
				dbgItem.Arg2 = ReadVLUInt();
			else dbgItem.Arg2 = 0;

			LastSId = dbgItem.DbgInfo.SId;
			LastLine1 = dbgItem.Arg1;
			return dbgItem;
		}

		public override int ReadByte()
		{
			int intValue = base.ReadByte();
			if (intValue != -1)
			{
				byte value = (byte)intValue;
				if (value == 0xFF &&
				   value == Last &&
				   value == LastLast)
				{
					Last = LastLast = 0;
					intValue = ReadRaw();
					if (intValue == -1)
						return -1;
					value = (byte)intValue;
					if (value == ESCAPE.TRIPLE_FF.ToByte())
						return 0xFF;
					base.Seek(-4, SeekOrigin.Current);
					throw new InvalidOperationException($"Unexpected escape sequence at position { Position }. Looking for { ESCAPE.TRIPLE_FF.ToByte() }, got { value }");
				}
				else
				{
					LastLast = Last;
					Last = value;
				}
			}
			return intValue;
		}

		public short ReadVLUShort()
		{
			int value;
			if ((value = ReadByte()) == -1)
				throw new EndOfStreamException();
			if (value < 0x80)
				return (short)value;
			short shortValue = (short)(value & 0x7F);
			if ((value = ReadByte()) == -1)
				throw new EndOfStreamException();
			return (short)(shortValue | ((short)(value << 7)));
		}

		public int ReadVLUInt()
		{
			int value = ReadVLUShort();
			if (value < 0x80)
				return value;
			int intValue = (value & 0x3FFF);
			if ((value & 0x4000) == 0)
				return intValue;
			else
				return intValue | (ReadVLUShort() << 14);
		}

		public long ReadScaledLong()
		{
			long longValue = 0;
			int prefixCount = -1;
			do
			{
				if ((longValue = ReadByte()) == -1)
					throw new EndOfStreamException();
				prefixCount++;
			} while (longValue == 0);
			longValue = (~longValue) & 0xFF;
			int m = (int)((longValue >> 5) & 0x7);
			long scale = 0;
			for (int i = 0; i < m; i++)
			{
				scale <<= 1;
				scale += 1;
			}
			longValue &= 0x1F;
			for (; prefixCount > 0; prefixCount--)
			{
				longValue <<= 8;
				scale <<= 8;
				scale += 251;
			}

			return (scale << 5) + (longValue << m);
		}

		public byte[] ReadFully(byte[] data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				int value = ReadByte();
				if(value == -1)
					throw new EndOfStreamException();
				data[i] = (byte)value;
			}
			return data;
		}

		public long ReadLong()
		{
			long value, longValue = 0;
			for (int i = 0; i < 64; i += 8)
			{
				if ((value = ReadByte()) == -1)
					throw new EndOfStreamException();
				longValue |= (value << i);
			}
			return longValue;
		}

		public long ReadIntAsLong()
		{
			long value, longValue = 0;
			for (int i = 0; i < 32; i += 8)
			{
				if ((value = ReadByte()) == -1)
					throw new EndOfStreamException();
				longValue |= (value << i);
			}
			return longValue;
		}

		public void InitializeNewBlock()
		{
			LastSId = 0;
			LastLine1 = 0;
		}

		internal class LogWriter
		{
			private static string LogWriterName = "GXDebugCoverage.log";
			public static object LogWriterLock = new object();
			private static StringBuilder m_Buffer = new StringBuilder();

			[Conditional(GXDebugManager._LOG_WRITER)]
			public static void SetLogName(string name)
			{
				LogWriterName = name;
			}

			[Conditional(GXDebugManager._LOG_WRITER)]
			public static void LogPosition(GXDebugStream stream)
			{
				Append($"\nPos: {stream.Position}\n");
			}

			[Conditional(GXDebugManager._LOG_WRITER)]
			public static void Append(string msg)
			{
				lock (LogWriterLock)
				{
					m_Buffer.Append(msg);
				}
			}

			[Conditional(GXDebugManager._LOG_WRITER)]
			public static void Flush()
			{
				try
				{
					lock (LogWriterLock)
					{
						File.AppendAllText(LogWriterName, m_Buffer.ToString());
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.StackTrace);
				}
				m_Buffer.Clear();
			}

			[Conditional(GXDebugManager._LOG_WRITER)]
			internal static void Log(string title, params (string key, object value)[] values)
			{
				StringBuilder sb = new StringBuilder();
				Append($"=========================================\n{title}\n");
				if (values != null)
				{
					foreach ((string key, object value) in values)
					{
						sb.Append($"  {key} = {value?.ToString()}\n");
					}
				}
				Append(sb.ToString());
			}

			[Conditional(GXDebugManager._LOG_WRITER)]
			internal static void LogWrite(string type, long value) => Append($"{type}:{value} ");

			[Conditional(GXDebugManager._LOG_WRITER)]
			internal static void LogWriteByte(byte value) => Append($"({value.ToString("X2")}) ");

			[Conditional(GXDebugManager._LOG_WRITER)]
			internal static void LogWriteRaw(byte[] data, int from, int length)
			{
				lock (LogWriterLock)
				{
					for (int i = 0; i < length; i++)
						m_Buffer.Append($"({data[from+i].ToString("X2")}) ");
				}
			}
		}
	}

#if _DEBUG_DEBUGGER
	public class GXDebugReader
	{
		class GXDebugError
		{
			public long Position { get; set; }
			public string Msg { get; set; }

			public GXDebugError(long position, string msg)
			{
				Position = position;
				Msg = msg;
			}
		}
		private readonly string FileName;

		public GXDebugReader(string FileName)
		{
			this.FileName = FileName;
		}

		public static int Main(string[] args)
		{
			try
			{
				Console.WriteLine("GXDebugReader");
				string filename = args.Length > 0 ? args[0] : $"gxperf.gxd";
				GXDebugReader reader = new GXDebugReader(filename);
				reader.Dump();
				return 0;
			}catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return -1;
			}
		}

		private void Dump()
		{
			using (GXDebugStream stream = new GXDebugStream(FileName, FileMode.Open))
			{
				GXDebugItem[] items;
				Guid SessionGuid;
				GXDebugGenId GenId;
				IList<GXDebugError> errors = new List<GXDebugError>();
				long lastPosition = stream.Position;
				while ((items = ReadItems(stream, out SessionGuid, out GenId, errors)) != null)
				{
					DumpAndClearErrors(errors);
					if (items.Length > 0)
					{
						Console.WriteLine($"SessionGuid: { SessionGuid } - Gen: { GenId}");
						Console.WriteLine($"Item Count: { items.Length } - bits/message: { (int)(((stream.Position - lastPosition) << 3) / items.Length) }");
						for (int idx = 0; idx < items.Length; idx++)
							Console.WriteLine($"\tItem({ idx }): { items[idx] }");
						Console.WriteLine();
					}
					lastPosition = stream.Position;
				}
				DumpAndClearErrors(errors);
			}
		}

		private void DumpAndClearErrors(IList<GXDebugError> errors)
		{
			if (errors.Count > 0)
			{
				Console.WriteLine("Errors:");
				foreach (GXDebugError gxerror in errors)
					Console.WriteLine($"\t{ gxerror.Position }: { gxerror.Msg }");
				errors.Clear();
			}
		}

		private GXDebugItem[] ReadItems(GXDebugStream stream, out Guid sessionGuid, out GXDebugGenId genId, IList<GXDebugError> errors)
		{
			sessionGuid = Guid.Empty;
			genId = GXDebugGenId.INVALID;
			stream.InitializeNewBlock();
			if (!FindEscape(stream, GXDebugStream.ESCAPE.PROLOG, errors))
				return null;
			short versionAndGenId = stream.ReadVLUShort();
			genId = (GXDebugGenId)(versionAndGenId & 0xF);
			short version = (short)(versionAndGenId >> 4);
			if(version != GXDebugManager.GXDEBUG_VERSION)
			{
				errors.Add(new GXDebugError(stream.Position, $"Cannot parse version { version } blocks"));
				FindEscape(stream, GXDebugStream.ESCAPE.EPILOG, errors);
				return new GXDebugItem[0];
			}
			int itemCount = stream.ReadVLUInt();
			if(itemCount == 0)
			{
				errors.Add(new GXDebugError(stream.Position, $"Invalid block. Item count = 0"));
				FindEscape(stream, GXDebugStream.ESCAPE.EPILOG, errors);
				return new GXDebugItem[0];
			}
			byte[] guid = new byte[16];
			sessionGuid = new Guid(stream.ReadFully(guid));
			GXDebugItem[] items = new GXDebugItem[itemCount];
			for(int idx = 0; idx < itemCount; idx++)
			{
				items[idx] = ReadItem(stream, errors);
				if(items[idx].MsgType == GXDebugMsgType.INVALID)
				{
					while(++idx < itemCount)
						items[idx] = NewInvalidItem();
					break;
				}
			}
			FindEscape(stream, GXDebugStream.ESCAPE.EPILOG, errors);
			return items;
		}

		private GXDebugItem NewInvalidItem()
		{
			GXDebugItem invalidItem = new GXDebugItem();
			invalidItem.MsgType = GXDebugMsgType.INVALID;
			return invalidItem;
		}

		private GXDebugItem ReadItem(GXDebugStream stream, IList<GXDebugError> errors)
		{
			int value;
			if ((value = stream.ReadByte()) == -1)
				throw new EndOfStreamException();
			GXDebugItem dbgItem = new GXDebugItem();
			if((value & GXDebugMsgType.SYSTEM.ToByte()) == GXDebugMsgType.SYSTEM.ToByte())
			{
				switch(value & 0xFC)
				{
					case 0xC0:
						{
							dbgItem.MsgType = GXDebugMsgType.REGISTER_PGM;
							dbgItem.DbgInfo = new GXDebugInfoLocal();
							dbgItem.DbgInfo.SId = stream.ReadVLUInt();
							dbgItem.Arg1 = stream.ReadVLUInt();
							dbgItem.ArgObj = new KeyValuePair<int, int>(stream.ReadVLUInt(), stream.ReadVLUInt());
						}
						break;
					case 0xA0:
						{
							dbgItem.MsgType = GXDebugMsgType.PGM_TRACE_RANGE;
							dbgItem.DbgInfo = new GXDebugInfoLocal();
							dbgItem.DbgInfo.SId = stream.ReadVLUInt();
							dbgItem.Arg1 = stream.ReadVLUInt();
							dbgItem.Arg2 = stream.ReadVLUInt();
							if((value & GXDebugMsgType.PGM_TRACE_RANGE_WITH_COLS.ToByte()) == GXDebugMsgType.PGM_TRACE_RANGE_WITH_COLS.ToByte())
								dbgItem.ArgObj = new KeyValuePair<int, int>(stream.ReadVLUInt(), stream.ReadVLUInt());
							else dbgItem.ArgObj = new KeyValuePair<int, int>(0, 0);
						}
						break;
					case 0x80:
						{
							dbgItem.MsgType = GXDebugMsgType.SYSTEM;
							dbgItem.Arg1 = value & GXDebugMsgCode.MASK_BITS.ToByte();
							switch ((GXDebugMsgCode)(dbgItem.Arg1))
							{
								case GXDebugMsgCode.INITIALIZE:
									dbgItem.ArgObj = new DateTime(stream.ReadLong(), DateTimeKind.Utc);
									break;
								case GXDebugMsgCode.OBJ_CLEANUP:
									dbgItem.ArgObj = stream.ReadVLUInt();
									break;
								case GXDebugMsgCode.PGM_INFO:
									{
										KeyValuePair<int, int> pgmKey = new KeyValuePair<int, int>(stream.ReadVLUInt(), stream.ReadVLUInt());
										KeyValuePair<int, long> pgmInfo = new KeyValuePair<int, long>(stream.ReadVLUInt(), stream.ReadIntAsLong());
										dbgItem.ArgObj = new KeyValuePair<object, object>(pgmKey, pgmInfo);
									}
									break;
								case GXDebugMsgCode.EXIT:
									break;
								default:
									dbgItem.MsgType = GXDebugMsgType.INVALID;
									errors.Add(new GXDebugError(stream.Position, $"Invalid Debug Item (type={ dbgItem.MsgType } - { value })"));
									break;
							}
						}
						break;
					default:
						dbgItem.MsgType = GXDebugMsgType.INVALID;
						errors.Add(new GXDebugError(stream.Position, $"Invalid Debug Item (type={ value })"));
						break;
				}
			}
			else
			{
				stream.ReadPgmTrace(dbgItem, (byte)value);
			}

			return dbgItem;
		}

		private bool FindEscape(GXDebugStream stream, GXDebugStream.ESCAPE escape, IList<GXDebugError> errors)
		{
			int byteValue;
			int escapeCount = 0;
			long errLoc = stream.Position;
			while((byteValue = stream.ReadRaw()) != -1)
			{
				if (byteValue == 0xFF)
				{
					escapeCount++;
					if (escapeCount == 3)
					{
						byteValue = stream.ReadRaw();
						if (byteValue != -1 && (byte)byteValue == escape.ToByte())
							return true;
						else
						{
							if (errLoc != -1)
							{
								errors.Add(new GXDebugError(errLoc, $"Cannot find escape sequence. Looking for { escape.ToByte() }, got { byteValue }"));
								errLoc = -1;
							}
							escapeCount = 0;
						}
					}
				}
				else
					escapeCount = 0;
			}
			if(stream.Position != errLoc && errLoc != -1)
				errors.Add(new GXDebugError(errLoc, "Cannot find escape sequence"));
			return false;
		}
	}
#endif

}
