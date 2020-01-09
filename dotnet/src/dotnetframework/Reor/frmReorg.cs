using System;
using System.Collections;
#if !NETCORE
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
#endif
using GeneXus.Reorg;
using GeneXus.Resources;
using System.Threading;
using System.IO;
using GeneXus.Configuration;
using System.Text;
using System.Diagnostics;
using GeneXus.Application;
using GeneXus.Utils;

namespace GeneXus.Forms
{
	public class ReorgStartup
	{

		public static int errorCode = -1;
		public static bool onlyRecordCount = false;
		public static bool ignoreresume = false;
		public static bool noprecheck = false;
		public static bool force = false;
		public static GXReorganization gxReorganization;

		const string REORGPGM_GEN = "reorgpgm.gen";
		const string CLIENT_EXE_CONFIG = "client.exe.config";

		[STAThread]
		public static int Main(string[] args)
		{
#if NETCORE
			var configFilePath = Path.Combine(FileUtil.GetStartupDirectory(), CLIENT_EXE_CONFIG);
			if (File.Exists(configFilePath))
			{
				GeneXus.Configuration.Config.ConfigFileName = configFilePath;
			}
#else
			if (File.Exists(CLIENT_EXE_CONFIG))
			{
				GeneXus.Configuration.Config.ConfigFileName = CLIENT_EXE_CONFIG;
			}
			else if (File.Exists("../" + CLIENT_EXE_CONFIG))
			{
				GeneXus.Configuration.Config.ConfigFileName = "../"+ CLIENT_EXE_CONFIG;
			}else
			{
				string configPath = Path.Combine(GxContext.StaticPhysicalPath(), CLIENT_EXE_CONFIG);
				if (File.Exists(configPath))
					GeneXus.Configuration.Config.ConfigFileName = configPath;
			}
#endif
			bool nogui = false;
			bool notexecute = false;
			foreach (string sw in args)
			{

				if (sw.ToLower().StartsWith("\\config:") || sw.ToLower().StartsWith("-config:"))
				{
					string configFile = sw.Substring(8);
					GeneXus.Configuration.Config.ConfigFileName = configFile;
					REORG_FILE_3 = Path.Combine(Path.GetDirectoryName(configFile) , REORGPGM_GEN);
				}
				else if (sw.ToLower().Trim() == "-nogui")
					nogui = true;
				else if (sw.ToLower().Trim() == "-force")
					force = true;
				else if (sw.ToLower().Trim() == "-recordcount")
					onlyRecordCount = true;
				else if (sw.ToLower().Trim() == "-ignoreresume")
					ignoreresume = true;
				else if (sw.ToLower().Trim() == "-noverifydatabaseschema")
					noprecheck = true;
				else if (sw.ToLower().Trim() == "-donotexecute")
					notexecute = true;
			}

			if (notexecute)
			{
				SetStatus(GXResourceManager.GetMessage("GXM_dbnotreorg"));
				errorCode = 0;
			}
			else
			{
				try
				{
					GxContext.isReorganization = true;
					gxReorganization = GetReorgProgram();
					if (nogui)
					{
#if !NETCORE
						try
						{
							Console.OutputEncoding = Encoding.Default;
						}
						catch (IOException) //Docker: System.IO.IOException: The parameter is incorrect. 
						{
							try
							{
								Console.OutputEncoding = Encoding.UTF8;
							}
							catch (IOException)
							{

							}
						}
#endif
						GXReorganization._ReorgReader = new NoGuiReorg();
						GXReorganization.printOnlyRecordCount = onlyRecordCount;
						GXReorganization.ignoreResume = ignoreresume;
						GXReorganization.noPreCheck = noprecheck;
						if (gxReorganization.GetCreateDataBase())
						{
							GXReorganization.SetCreateDataBase();
						}

						if (ReorgStartup.reorgPending())
						{
							if (gxReorganization.BeginResume())
							{
								gxReorganization.ExecForm();
							}
							if (GXReorganization.Error)
							{
								SetStatus(GXResourceManager.GetMessage("GXM_reorgnotsuccess"));
								if (GXReorganization.ReorgLog != null && GXReorganization.ReorgLog.Count > 0)
									SetStatus((string)GXReorganization.ReorgLog[GXReorganization.ReorgLog.Count - 1]);
								errorCode = -1;

							}
							else
							{
								SetStatus(GXResourceManager.GetMessage("GXM_reorgsuccess"));
								errorCode = 0;
							}
						}
						else
						{
							SetStatus(GXResourceManager.GetMessage("GXM_ids_noneeded"));
							errorCode = 0;
						}
					}
#if !NETCORE
					else
					{
						FreeConsole();
						System.Windows.Forms.Application.Run(new GuiReorg());
					}
#endif
				}
				catch (Exception ex)
				{
#if !NETCORE
					if (GXUtil.IsBadImageFormatException(ex) && !GXUtil.ExecutingRunX86())
					{
						GXReorganization.CloseResumeFile();
						int exitCode;
						if (GXUtil.RunAsX86(GXUtil.REOR, args, DataReceived, out exitCode))
						{
							return exitCode;
						}
						else
						{
							SetStatus(GXResourceManager.GetMessage("GXM_reorgnotsuccess"));
							SetStatus(GXResourceManager.GetMessage("GXM_callerr", GXUtil.RUN_X86));
							errorCode = -1;
						}
					}
					else
#endif
					{
						SetStatus(GXResourceManager.GetMessage("GXM_reorgnotsuccess"));
						SetStatus(ex.Message);
						
						errorCode = -1;
					}
				}

			}
			return errorCode;
		}
		protected static void DataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e != null && e.Data != null)
				SetStatus(e.Data);
			else if (sender!=null)
			{
				string senderStr = sender as string;
				if (senderStr!=null)
					SetStatus(senderStr);
			}
		}

		private static GXReorganization GetReorgProgram()
		{
			GXReorganization rg = null;
			try
			{
				string ns;
				string assmblyName;
				string nme;
				if (Config.GetValueOf("ReorgAssemblyName", out nme))
					assmblyName = nme;
				else
					assmblyName = "";

				if (Config.GetValueOf("AppMainNamespace", out nme))
					ns = nme;
				else
					ns = "GeneXus.Programs";
				if (assmblyName.Length == 0)
					rg = (GXReorganization)GeneXus.Metadata.ClassLoader.GetInstance("Reorganization", ns + ".reorganization", null);
				else
					rg = (GXReorganization)GeneXus.Metadata.ClassLoader.GetInstance(assmblyName, ns + ".reorganization", null);
			}
			catch (Exception ex)
			{
				if (GXUtil.IsBadImageFormatException(ex))
				{
					throw ex;
				}
				else {
					rg = (GXReorganization)GeneXus.Metadata.ClassLoader.GetInstance("Reorganization", "GeneXus.Programs.reorganization", null);
				}
			}
			return rg;
		}

		public static bool reorgPending()
		{
			return (File.Exists(REORG_FILE_1) || File.Exists(REORG_FILE_2) || File.Exists(REORG_FILE_3) || File.Exists(REORG_FILE_4) || force);
		}
		public static void EndReorg()
		{
			if (File.Exists(REORG_FILE_1)) File.Delete(REORG_FILE_1);
			if (File.Exists(REORG_FILE_2)) File.Delete(REORG_FILE_2);
			if (File.Exists(REORG_FILE_3)) File.Delete(REORG_FILE_3);
			if (File.Exists(REORG_FILE_4)) File.Delete(REORG_FILE_4); 
			if (GXReorganization.printOnlyRecordCount) GXReorganization.DeleteResumeFile();
			
		}
#if NETCORE
		static string REORG_FILE_1 = Path.Combine(FileUtil.GetStartupDirectory(), "..", REORGPGM_GEN);
		static string REORG_FILE_2 = Path.Combine(FileUtil.GetStartupDirectory(), REORGPGM_GEN);
#else
		static string REORG_FILE_1 = Path.Combine(System.Windows.Forms.Application.StartupPath, "..", REORGPGM_GEN);
		static string REORG_FILE_2 = Path.Combine(System.Windows.Forms.Application.StartupPath, REORGPGM_GEN);

#endif
		static string REORG_FILE_4 = Path.Combine(GxContext.StaticPhysicalPath(), REORGPGM_GEN);
		static string REORG_FILE_3 = "";
		public static void SetStatus(string msg)
		{
			Console.WriteLine(msg);
		}
				[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern int FreeConsole();

	}
#if !NETCORE
	
	public class GuiReorg : System.Windows.Forms.Form, IReorgReader
	{
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btnExecute;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.ListView output;
		private System.Windows.Forms.ColumnHeader Message;
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ColumnHeader Status;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.PictureBox imgReorg;
		private System.Windows.Forms.PictureBox imgEqual;
		private System.Windows.Forms.PictureBox imgOk;
		private System.Windows.Forms.PictureBox imgCancel;
		private System.Windows.Forms.GroupBox groupBox1;

		private Hashtable outputIdMapping;

#region IReorgReader Implementation
		
		void IReorgReader.NotifyMessage(string msg, Object[] args)
		{
			output.Items.Insert(output.Items.Count, GetString(msg, args), GetImage(msg));
		}
		void IReorgReader.NotifyMessage(int id, string msg, Object[] args)
		{
			object pos = outputIdMapping[id];
			if (pos != null)
			{
				output.Items[(int)pos].Text = GetString(msg, args, false);
			}
			else
			{
				int last = output.Items.Count;
				output.Items.Insert(last, GetString(msg, args, false));
				outputIdMapping[id] = last;
			}
		}
		void IReorgReader.NotifyStatus(int id, ReorgBlockStatusInfo rs)
		{
			object pos = outputIdMapping[id];
			if (pos != null)
			{
				if (output.Items[(int)pos].SubItems.Count > 1)
				{
					string statTxt = StatusText.Get(rs);
					if (output.Items[(int)pos].SubItems[1].Text != statTxt)
					{
						output.Items[(int)pos].SubItems[1].Text = statTxt;
						output.Items[(int)pos].BackColor = statusColor(rs.Status);
						output.Invalidate(output.Items[(int)pos].GetBounds(System.Windows.Forms.ItemBoundsPortion.Entire));
					}
				}
				else
				{
					output.Items[(int)pos].SubItems.Add(StatusText.Get(rs));
					output.Items[(int)pos].BackColor = statusColor(rs.Status);
				}
			}
		}
		void IReorgReader.NotifyError()
		{
			imgCancel.Visible = true;
			imgOk.Visible = false;
			imgEqual.Visible = false;
			imgReorg.Visible = false;
			reorgRunning = false;
			btnClose.Enabled = true;
			lblStatus.Text = GXResourceManager.GetMessage("GXM_ids_failed");
			ReorgStartup.errorCode = -1;

		}
		
		void IReorgReader.NotifyEnd(string id)
		{
			timer1.Enabled = false;
			reorgRunning = false;
			ReorgStartup.EndReorg();
			btnClose.Enabled = true;
			try
			{
				imgCancel.Visible = false;
				imgOk.Visible = true;
				imgEqual.Visible = false;
				imgReorg.Visible = false;
			}
			catch
			{
			}
			output.Items.Insert(output.Items.Count, GXResourceManager.GetMessage("GXM_ids_ok"));
			lblStatus.Text = GXResourceManager.GetMessage("GXM_ids_ok");
			ReorgStartup.errorCode = 0;
			
		}
#endregion
		Color statusColor(ReorgBlockStatus rs)
		{
			switch (rs)
			{
				case ReorgBlockStatus.Pending:
					return System.Drawing.Color.LightYellow;
				case ReorgBlockStatus.Executing:
					return System.Drawing.Color.LightSalmon;
				case ReorgBlockStatus.Ended:
					return System.Drawing.Color.LightGreen;
				case ReorgBlockStatus.Error:
					return System.Drawing.Color.Red;
				default:
					return System.Drawing.Color.MidnightBlue;
			}
		}
		public GuiReorg()
		{
			
			InitializeComponent();
			
			outputIdMapping = new Hashtable();
			btnExecute.Text = GXResourceManager.GetMessage("GXM_ids_execute");
			btnClose.Text = GXResourceManager.GetMessage("GXM_ids_close");
			this.Text = GXResourceManager.GetMessage("GXM_ids_title");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

#region Windows Form Designer generated code
		
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReorgStartup));
			this.Message = new System.Windows.Forms.ColumnHeader();
			this.output = new System.Windows.Forms.ListView();
			this.Status = new System.Windows.Forms.ColumnHeader();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnClose = new System.Windows.Forms.Button();
			this.btnExecute = new System.Windows.Forms.Button();
			this.lblStatus = new System.Windows.Forms.Label();
			this.imgReorg = new System.Windows.Forms.PictureBox();
			this.imgEqual = new System.Windows.Forms.PictureBox();
			this.imgOk = new System.Windows.Forms.PictureBox();
			this.imgCancel = new System.Windows.Forms.PictureBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.imgReorg)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imgEqual)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imgOk)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imgCancel)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// Message
			// 
			this.Message.Text = "";
			this.Message.Width = 350;
			// 
			// output
			// 
			this.output.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.output.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.output.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Message,
            this.Status});
			this.output.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.output.LabelWrap = false;
			this.output.Location = new System.Drawing.Point(8, 64);
			this.output.Name = "output";
			this.output.ShowItemToolTips = true;
			this.output.Size = new System.Drawing.Size(432, 264);
			this.output.TabIndex = 0;
			this.output.UseCompatibleStateImageBehavior = false;
			this.output.View = System.Windows.Forms.View.Details;
			// 
			// Status
			// 
			this.Status.Text = "";
			this.Status.Width = 400;
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.btnClose);
			this.groupBox2.Controls.Add(this.btnExecute);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.groupBox2.Location = new System.Drawing.Point(0, 349);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(448, 48);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
			// 
			// btnClose
			// 
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Location = new System.Drawing.Point(112, 16);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(88, 24);
			this.btnClose.TabIndex = 0;
			this.btnClose.Click += new System.EventHandler(this.button1_Click);
			// 
			// btnExecute
			// 
			this.btnExecute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnExecute.Location = new System.Drawing.Point(16, 16);
			this.btnExecute.Name = "btnExecute";
			this.btnExecute.Size = new System.Drawing.Size(88, 24);
			this.btnExecute.TabIndex = 0;
			this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
			// 
			// lblStatus
			// 
			this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblStatus.Location = new System.Drawing.Point(56, 16);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(312, 24);
			this.lblStatus.TabIndex = 0;
			this.lblStatus.Click += new System.EventHandler(this.lblStatus_Click);
			// 
			// imgReorg
			// 
			this.imgReorg.Image = ((System.Drawing.Image)(resources.GetObject("imgReorg.Image")));
			this.imgReorg.Location = new System.Drawing.Point(24, 16);
			this.imgReorg.Name = "imgReorg";
			this.imgReorg.Size = new System.Drawing.Size(16, 16);
			this.imgReorg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.imgReorg.TabIndex = 1;
			this.imgReorg.TabStop = false;
			// 
			// imgEqual
			// 
			this.imgEqual.Image = ((System.Drawing.Image)(resources.GetObject("imgEqual.Image")));
			this.imgEqual.Location = new System.Drawing.Point(32, 16);
			this.imgEqual.Name = "imgEqual";
			this.imgEqual.Size = new System.Drawing.Size(16, 24);
			this.imgEqual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.imgEqual.TabIndex = 1;
			this.imgEqual.TabStop = false;
			// 
			// imgOk
			// 
			this.imgOk.Image = ((System.Drawing.Image)(resources.GetObject("imgOk.Image")));
			this.imgOk.Location = new System.Drawing.Point(32, 16);
			this.imgOk.Name = "imgOk";
			this.imgOk.Size = new System.Drawing.Size(16, 16);
			this.imgOk.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.imgOk.TabIndex = 1;
			this.imgOk.TabStop = false;
			// 
			// imgCancel
			// 
			this.imgCancel.Image = ((System.Drawing.Image)(resources.GetObject("imgCancel.Image")));
			this.imgCancel.Location = new System.Drawing.Point(32, 16);
			this.imgCancel.Name = "imgCancel";
			this.imgCancel.Size = new System.Drawing.Size(16, 16);
			this.imgCancel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.imgCancel.TabIndex = 1;
			this.imgCancel.TabStop = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.lblStatus);
			this.groupBox1.Controls.Add(this.imgReorg);
			this.groupBox1.Controls.Add(this.imgEqual);
			this.groupBox1.Controls.Add(this.imgOk);
			this.groupBox1.Controls.Add(this.imgCancel);
			this.groupBox1.Location = new System.Drawing.Point(0, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(456, 320);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
			// 
			// FrmReorg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(448, 397);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.output);
			this.Controls.Add(this.groupBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmReorg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FrmReorg_Closing);
			this.Load += new System.EventHandler(this.FrmReorg_Load_1);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.imgReorg)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imgEqual)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imgOk)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imgCancel)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
#endregion

		private void FrmReorg_Load(object sender, System.EventArgs e)
		{
		}

		private void FrmReorg_Load_1(object sender, System.EventArgs e)
		{
			imgCancel.Visible = false;
			imgOk.Visible = false;
			imgEqual.Visible = false;
			imgReorg.Visible = false;
			EnableButtons();
		}

		private void EnableButtons()
		{
			if (!ReorgStartup.reorgPending())
			{
				btnExecute.Enabled = false;
				btnClose.Enabled = true;
				lblStatus.Text = GXResourceManager.GetMessage("GXM_ids_noneeded");
				imgCancel.Visible = false;
				imgOk.Visible = false;
				imgEqual.Visible = true;
				imgReorg.Visible = false;
			}
			else
			{
				btnExecute.Enabled = true;
				imgCancel.Visible = false;
				imgOk.Visible = false;
				imgEqual.Visible = false;
				imgReorg.Visible = true; ;
				lblStatus.Text = GXResourceManager.GetMessage("GXM_ids_needed");
			}
		}
		private void button1_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void btnExecute_Click(object sender, System.EventArgs e)
		{
			if (ReorgStartup.reorgPending())
			{
				btnExecute.Enabled = false;
				GXReorganization._ReorgReader = this;
				GXReorganization.ignoreResume = ReorgStartup.ignoreresume;
				GXReorganization.noPreCheck = ReorgStartup.noprecheck;
				GXReorganization.printOnlyRecordCount = ReorgStartup.onlyRecordCount;
				if (ReorgStartup.gxReorganization.GetCreateDataBase())
				{
					GXReorganization.SetCreateDataBase();
				}

				//
				// Start the thread
				//
				try
				{
					if (ReorgStartup.gxReorganization.BeginResume())
					{
						m_oThread = new Thread(new ThreadStart(ReorgStartup.gxReorganization.ExecForm));
						reorgRunning = true;
						btnClose.Enabled = false;

						lblStatus.Text = GXResourceManager.GetMessage("GXM_ids_run");
						m_oThread.Start();
						timer1.Enabled = true;
					}
					else
					{
						throw new Exception();
					}
				}
				catch (Exception)
				{
					imgCancel.Visible = true;
					imgOk.Visible = false;
					imgEqual.Visible = false;
					imgReorg.Visible = false;
					reorgRunning = false;
					btnClose.Enabled = true;
					lblStatus.Text = GXResourceManager.GetMessage("GXM_ids_failed");
					ReorgStartup.errorCode = -1;
					return;
				}
				imgCancel.Visible = false;
				imgOk.Visible = false;
				imgEqual.Visible = false;
				imgReorg.Visible = false;
				btnExecute.Enabled = false;

			}
			else
			{
				MessageBox.Show(GXResourceManager.GetMessage("GXM_ids_noneeded"), "GeneXus.NET");
				Close();
			}
		}
		private string GetString(string msg, Object[] args)
		{
			return GetString(msg, args, true);
		}
		private string GetString(string msg, Object[] args, bool indent)
		{
			if (indent && msg != null && !Char.IsNumber(msg, 0))
				msg = "                    " + msg;
			
			return msg;
		}
		private int GetImage(string msg)
		{
			if (msg != null && (msg[0] == '-' || msg[0] == '*' || msg[0] == '|'))
				return 1;
			return 0;
		}
		private bool reorgRunning = false;
		private void FrmReorg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (reorgRunning)
			{
				MessageBox.Show(GXResourceManager.GetMessage("GXM_ids_run"), "GeneXus.NET");
				e.Cancel = true;
			}

		}

		private void groupBox2_Enter(object sender, System.EventArgs e)
		{

		}

		private void imgCancel_Click(object sender, System.EventArgs e)
		{

		}

		private Thread m_oThread;
		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (m_oThread != null && !m_oThread.IsAlive)
			{
				imgOk.Visible = false;
				imgEqual.Visible = false;
				imgReorg.Visible = false;
				reorgRunning = false;
				imgCancel.Visible = true;
				btnClose.Enabled = true;
				lblStatus.Text = GXResourceManager.GetMessage("GXM_ids_failed");
				ReorgStartup.errorCode = -1;
			}
		}

		private void groupBox1_Enter(object sender, System.EventArgs e)
		{

		}

		private void lblStatus_Click(object sender, System.EventArgs e)
		{

		}
		public bool GuiDialog
		{
			get { return true; }
		}

	}
#endif
	public class NoGuiReorg : IReorgReader
	{
		private Hashtable outputMsg;
		private Hashtable outputStatus;

		public NoGuiReorg()
		{
			outputMsg = new Hashtable();
			outputStatus = new Hashtable();
		}
		public void NotifyError()
		{
		}

		public void NotifyMessage(string msg, Object[] args)
		{
			if (args != null)
			{
				Console.WriteLine(msg, args);
			}
			else
			{
				Console.WriteLine(msg);
			}
		}
		void IReorgReader.NotifyMessage(int id, string msg, Object[] args)
		{
			object pos = outputMsg[id];
			if (pos == null)
			{
				Console.WriteLine(msg);
				outputMsg[id] = msg;
				outputStatus[id] = ReorgBlockStatus.Pending;
			}
		}
		void IReorgReader.NotifyStatus(int id, ReorgBlockStatusInfo rs)
		{
			object msg = outputMsg[id];
			object lastStatus = outputStatus[id];
			if (msg != null && lastStatus != null)
			{
				if (rs.Status != (ReorgBlockStatus)lastStatus)
				{
					outputStatus[id] = rs.Status;
					Console.WriteLine((string)msg + " " + StatusText.Get(rs));
				}
			}
		}
		void IReorgReader.NotifyEnd(string id)
		{
			ReorgStartup.EndReorg();
		}

		public bool GuiDialog
		{
			get { return false; }
		}

	}

	public class StatusText
	{
		public static string Get(ReorgBlockStatusInfo rs)
		{
			switch (rs.Status)
			{
				case ReorgBlockStatus.Pending:
					{
						if (rs.OtherStatusInfo.Length > 0)
							return rs.OtherStatusInfo;
						else
							return GXResourceManager.GetMessage("GXM_pending");
					}
				case ReorgBlockStatus.Executing:
					return GXResourceManager.GetMessage("GXM_started");
				case ReorgBlockStatus.Ended:
					return GXResourceManager.GetMessage("GXM_ended");
				case ReorgBlockStatus.Error:
					return GXResourceManager.GetMessage("GXM_errtitle") + rs.OtherStatusInfo;
				default:
					return "!";
			}
		}
	}
}
