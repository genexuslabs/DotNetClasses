using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Collections.Generic;
using Genexus.RegistryUtilities;
using Microsoft.Win32;

namespace ConnectionBuilder
{
	
	public class SQLConnectionDialog : System.Windows.Forms.Form, IConnectionDialog
	{
		private System.Windows.Forms.Button testConnection;
		private System.Windows.Forms.ComboBox cboDatabase;
		private System.Windows.Forms.CheckBox checkBoxBlankPwd;
		private System.Windows.Forms.Label label4;
		DklTextBox password;
		DklTextBox userName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton optUsrAndPwd;
		private System.Windows.Forms.RadioButton optWndIntegrated;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cboServers;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.LinkLabel newDatabase;
		private System.Data.OleDb.OleDbConnection connectionTest;
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Button Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SQLConnectionDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.testConnection = new System.Windows.Forms.Button();
			this.cboDatabase = new System.Windows.Forms.ComboBox();
			this.checkBoxBlankPwd = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.password = new ConnectionBuilder.DklTextBox();
			this.userName = new ConnectionBuilder.DklTextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.optUsrAndPwd = new System.Windows.Forms.RadioButton();
			this.optWndIntegrated = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.cboServers = new System.Windows.Forms.ComboBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label5 = new System.Windows.Forms.Label();
			this.newDatabase = new System.Windows.Forms.LinkLabel();
			this.connectionTest = new System.Data.OleDb.OleDbConnection();
			this.OK = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// testConnection
			// 
			this.testConnection.Location = new System.Drawing.Point(176, 256);
			this.testConnection.Name = "testConnection";
			this.testConnection.Size = new System.Drawing.Size(104, 24);
			this.testConnection.TabIndex = 14;
			this.testConnection.Text = "&Test Connection";
			this.testConnection.Click += new System.EventHandler(this.testConnection_Click);
			// 
			// cboDatabase
			// 
			this.cboDatabase.Location = new System.Drawing.Point(8, 224);
			this.cboDatabase.Name = "cboDatabase";
			this.cboDatabase.Size = new System.Drawing.Size(192, 21);
			this.cboDatabase.TabIndex = 12;
			this.cboDatabase.DropDown += new System.EventHandler(this.cboDatabase_DropDown);
			// 
			// checkBoxBlankPwd
			// 
			this.checkBoxBlankPwd.Enabled = false;
			this.checkBoxBlankPwd.Location = new System.Drawing.Point(8, 104);
			this.checkBoxBlankPwd.Name = "checkBoxBlankPwd";
			this.checkBoxBlankPwd.Size = new System.Drawing.Size(104, 16);
			this.checkBoxBlankPwd.TabIndex = 10;
			this.checkBoxBlankPwd.Text = "&Blank password";
			this.checkBoxBlankPwd.CheckedChanged += new System.EventHandler(this.checkBoxBlankPwd_CheckedChanged_1);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 56);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(240, 16);
			this.label4.TabIndex = 3;
			this.label4.Text = "2. &Enter information to log on to the server:";
			// 
			// password
			// 
			this.password.Enabled = false;
			this.password.Location = new System.Drawing.Point(88, 80);
			this.password.Name = "password";
			this.password.PasswordChar = '*';
			this.password.Size = new System.Drawing.Size(120, 20);
			this.password.TabIndex = 9;
			// 
			// userName
			// 
			this.userName.Enabled = false;
			this.userName.Location = new System.Drawing.Point(88, 56);
			this.userName.Name = "userName";
			this.userName.Size = new System.Drawing.Size(120, 20);
			this.userName.TabIndex = 7;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 24);
			this.label3.TabIndex = 8;
			this.label3.Text = "&Password:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 24);
			this.label2.TabIndex = 6;
			this.label2.Text = "&User name:";
			// 
			// optUsrAndPwd
			// 
			this.optUsrAndPwd.Location = new System.Drawing.Point(8, 24);
			this.optUsrAndPwd.Name = "optUsrAndPwd";
			this.optUsrAndPwd.Size = new System.Drawing.Size(240, 32);
			this.optUsrAndPwd.TabIndex = 5;
			this.optUsrAndPwd.Text = "&Use a specific user name and password:";
			this.optUsrAndPwd.CheckedChanged += new System.EventHandler(this.optUsrAndPwd_CheckedChanged);
			// 
			// optWndIntegrated
			// 
			this.optWndIntegrated.Checked = true;
			this.optWndIntegrated.Location = new System.Drawing.Point(8, 8);
			this.optWndIntegrated.Name = "optWndIntegrated";
			this.optWndIntegrated.Size = new System.Drawing.Size(208, 16);
			this.optWndIntegrated.TabIndex = 4;
			this.optWndIntegrated.TabStop = true;
			this.optWndIntegrated.Text = "Use &Windows NT Integrated security";
			this.optWndIntegrated.CheckedChanged += new System.EventHandler(this.optWndIntegrated_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(184, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "1. &Select or enter a SQL Server:";
			// 
			// cboServers
			// 
			this.cboServers.Location = new System.Drawing.Point(8, 24);
			this.cboServers.Name = "cboServers";
			this.cboServers.Size = new System.Drawing.Size(270, 21);
			this.cboServers.TabIndex = 2;
			this.cboServers.SelectedIndexChanged += new System.EventHandler(this.cboServers_SelectedIndexChanged_1);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.optWndIntegrated);
			this.panel1.Controls.Add(this.optUsrAndPwd);
			this.panel1.Controls.Add(this.password);
			this.panel1.Controls.Add(this.userName);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.checkBoxBlankPwd);
			this.panel1.Location = new System.Drawing.Point(8, 72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(272, 128);
			this.panel1.TabIndex = 4;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 208);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(200, 16);
			this.label5.TabIndex = 11;
			this.label5.Text = "3. Select &database on the server";
			// 
			// newDatabase
			// 
			this.newDatabase.Location = new System.Drawing.Point(200, 224);
			this.newDatabase.Name = "newDatabase";
			this.newDatabase.Size = new System.Drawing.Size(80, 24);
			this.newDatabase.TabIndex = 13;
			this.newDatabase.TabStop = true;
			this.newDatabase.Text = "New database";
			this.newDatabase.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.newDatabase.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.newDatabase_LinkClicked);
			// 
			// OK
			// 
			this.OK.Location = new System.Drawing.Point(56, 296);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(104, 24);
			this.OK.TabIndex = 15;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Cancel
			// 
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(176, 296);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(104, 24);
			this.Cancel.TabIndex = 16;
			this.Cancel.Text = "Cancel";
			// 
			// SQLConnectionDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(290, 326);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.newDatabase);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.testConnection);
			this.Controls.Add(this.cboDatabase);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cboServers);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SQLConnectionDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Connection String";
			this.Load += new System.EventHandler(this.UserControl1_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.Run(new SQLConnectionDialog());
		}

		private void optWndIntegrated_CheckedChanged(object sender, System.EventArgs e)
		{
			userName.Enabled = false;
			password.Enabled = false;
			checkBoxBlankPwd.Enabled = false;

		}

		private void optUsrAndPwd_CheckedChanged(object sender, System.EventArgs e)
		{
			userName.Enabled = true;
			password.Enabled = true;
			checkBoxBlankPwd.Enabled = true;
		}

		private void checkBoxBlankPwd_CheckedChanged(object sender, System.EventArgs e)
		{
			password.Enabled = !checkBoxBlankPwd.Checked;
		}

		private void UserControl1_Load(object sender, System.EventArgs e)
		{
			LoadLocalInstances();
		}

	
		private void comboBases_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

		public static List<string> LocalSQLInstances()
		{
			List<string> validInstances = new List<string>();

			try
			{
				bool is64 = RegistryHelper.IsWow64Registry();
				RegistryKey localKey;
				if (is64)
				{
					 localKey =
									   RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine,
										   RegistryView.Registry64);
				}
				else
				{
					localKey =
							RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine,
								RegistryView.Registry32);
				}

				RegistryKey sqlKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server", false);
				String[] instances = (sqlKey.GetValue(@"InstalledInstances") as string[]);
				if (instances == null || instances.Length == 0)
					instances = new[] { "SQLEXPRESS" };

				foreach (String element in instances)
				{
					var elementKey = sqlKey.OpenSubKey(element + @"\MSSQLServer\CurrentVersion");
					string version = null;
					if (elementKey != null)
						version = elementKey.GetValue("CurrentVersion") as string;
				
					if (version != null)
					{
						string instanceName = element;
						if (instanceName.StartsWith("MSSQL$"))
							instanceName = instanceName.Substring(6);

						string iName = $"{Environment.MachineName}\\{instanceName}";

						if (instanceName == "MSSQLSERVER")
							iName = Environment.MachineName;
						else if (element == "SQLEXPRESS")
							iName = $"{Environment.MachineName}\\SQLEXPRESS";

						if (!validInstances.Contains(iName))
							validInstances.Add(iName);
					}
				}
			}

			catch { } // we couldn't read from the registry, this can be a security problem, just let the user add the name it consider but do not raise any exception.
		
			return validInstances;
		}

		private void LoadLocalInstances()
		{
			Debug.Assert(false);
			List<string> instances = LocalSQLInstances();
			int index = 0; int selectedIndex = -1;
			if (instances != null && instances.Count > 0)
			{
				foreach (String element in instances)
				{
					cboServers.Items.Add(element);
					index++;
				}

				if (cboServers.Items.Count > selectedIndex && selectedIndex > -1 && cboServers.Text.Length == 0)
				{
					cboServers.SelectedIndex = selectedIndex;
				}
			}
		}


		private void cboDatabase_DropDown(object sender, System.EventArgs e)
		{
			if (TestConnection(out string sMessage))
			{
				try
				{
					var command = connectionTest.CreateCommand();
					command.CommandText = "select * from master.sys.databases";

					var reader = command.ExecuteReader();

					Cursor = Cursors.WaitCursor;
					while (reader.Read())
					{
						try
						{
							this.cboDatabase.Items.Add(reader.GetValue(0).ToString());
						}
						catch
						{
						}
					}
					this.cboDatabase.Sorted = true;
					if (this.cboDatabase.Items.Count > 0)
					{
						this.cboDatabase.SelectedIndex = 0;
						this.cboDatabase.Enabled = true;
					}
					else
					{
						this.cboDatabase.Enabled = false;
						this.cboDatabase.Text = "<No databases found>";
					}
					this.Cursor = Cursors.Default;
				}
				catch
				{
					Cursor = Cursors.Default;
				}
			}
		}
	

		bool TestConnection(out string sMessage)
		{
			try
			{
				SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
				connectionTest = new System.Data.OleDb.OleDbConnection(sqlConnectionStringBuilder.ConnectionString);
				connectionTest.Open();
				sMessage = "";
				return true;
			}
			catch (Exception ex)
			{
				sMessage = ex.Message;
				return false;
			}

		}

		void DisplayMessage(string sMessage , bool bError)
		{
			MessageBox.Show(sMessage, "Genexus", MessageBoxButtons.OK, (bError)? MessageBoxIcon.Error : MessageBoxIcon.Information);
		}

		private void testConnection_Click(object sender, System.EventArgs e)
		{
			if (!TestConnection(out string sMessage))
			{
				DisplayMessage(sMessage, true);
			}
			else
			{
				connectionTest.Close();
				DisplayMessage("Test connection succeeded", false);
			}
		}

		private void cboServers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}


		private bool ExecuteCommand(string sCommand)
		{
			string sMsg;
			if (TestConnection(out sMsg))
			{
				try
				{
					System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(sCommand, connectionTest);
					cmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					connectionTest.Close();
					throw (ex);
				}
				return true;
			}
			throw (new Exception(sMsg));
		}

		private void newDatabase_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (TestConnection(out string sMessage))
			{
				try
				{
					NewDatabase dlgNewDatabase = new NewDatabase();
					if (dlgNewDatabase.ShowDialog(this) == DialogResult.OK)
					{
						Cursor = Cursors.WaitCursor;
						string dbName = dlgNewDatabase.DatabaseName;
						if (dbName.Length > 0)
						{
							try
							{
								string sCommand = "CREATE DATABASE " + dbName;
								if (ExecuteCommand(sCommand))
								{
									connectionTest.Close();
									int iNew = cboDatabase.Items.Add(dbName);
									cboDatabase.SelectedIndex = iNew;
								}
							}
							catch (Exception ex)
							{
								DisplayMessage(ex.Message, true);
							}
						}
					}	
				}
				catch (Exception ex)
				{
					DisplayMessage(ex.Message, true);
				}
				Cursor = Cursors.Default;
			}
		}

		private void checkBoxBlankPwd_CheckedChanged_1(object sender, System.EventArgs e)
		{
			password.Enabled = !checkBoxBlankPwd.Checked;
		}

		private void cboServers_SelectedIndexChanged_1(object sender, System.EventArgs e)
		{
		
		}

	


		public string Server
		{
			get
			{
				if (cboServers.SelectedIndex >= 0)
					return cboServers.SelectedItem.ToString();
				return cboServers.Text;
			}
		
			set
			{
				cboServers.Text = value;
			}
		}

		public string Database
		{
			get
			{
				if (cboDatabase.SelectedIndex >= 0)
					return cboDatabase.SelectedItem.ToString();
				return cboDatabase.Text;
			}
			set
			{
				cboDatabase.Text = value;
			}
		}

		public string UserAndPassword
		{
			get
			{
				if (IntegratedSecurity)
				{
					return "Integrated Security=SSPI";
				}
				else
				{
					string sPassword = (password.Enabled)? password.Text : "";
					return String.Format("User ID={0}{1}", userName.Text, ";Password=" + sPassword );
				}
			}
		}

		public bool IntegratedSecurity
		{
			get
			{
				return optWndIntegrated.Checked;
			}
			set
			{
				optWndIntegrated.Checked = value;
			}
		}

		public bool PersistSecurityInfo
		{
			get
			{
				return true;
			}

			
		}

		
		public string ConnectionString
		{
			get 
			{
				
				string sConn = String.Format("Provider={0};Persist Security Info={1};{2};Initial Catalog={3};Data Source={4};",
					"SQLOLEDB.1", PersistSecurityInfo.ToString(), UserAndPassword, Database, Server);
				return sConn;
			}
			set
			{
				ParseConnection(value);
			}
		}


		public string IDBConnectionString
		{
			get 
			{
				string sConn = String.Format("Persist Security Info={0};{1};Initial Catalog={2};Data Source={3};",
					 PersistSecurityInfo.ToString(), UserAndPassword, Database, Server);
				return sConn;
			}
			set
			{
				ParseConnection(value);
			}
		}

		private bool ParseConnection(string sValue)
		{
			string[] stringPairs = sValue.Split( new char[] { ';'} );
			for (int i = 0; i < stringPairs.Length ; i++)
			{
				string [] sNameValue = stringPairs[i].Split(new char[] { '=' });
				if (sNameValue.Length == 2)
				{
					SetProperty(sNameValue[0], sNameValue[1]);
				}
			}
			return true;
		}

		private const string INITIAL_CATALOG		= "INITIAL CATALOG";
		private const string DATA_SOURCE			= "DATA SOURCE";
		private const string INTEGRATED_SECURITY	= "INTEGRATED SECURITY";
		private const string USER_ID				= "USER ID";
		private const string USER_PASSWORD				= "PASSWORD";


		private void SetProperty(string sPropName, string sPropValue)
		{
			string sProperty = sPropName.ToUpper().Trim();
		
			switch (sProperty)
			{
				case INITIAL_CATALOG:
					Database = sPropValue;
					break;
				case DATA_SOURCE:
					Server = sPropValue;
					break;
				case INTEGRATED_SECURITY:
					IntegratedSecurity = (sPropValue.ToUpper().Trim() == "SSPI");
					break;
				case USER_ID:
					optUsrAndPwd.Checked = true;
					userName.Text = sPropValue;
					break;
				case USER_PASSWORD:
					password.Text = sPropValue;
					break;

			}
		}

		public bool Show(string sInitString)
		{
			ConnectionString = sInitString;
			return ShowDialog() == DialogResult.OK;
		}

	

		private void userName_TextChanged(object sender, System.EventArgs e)
		{
		
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			if (Database.Length == 0)
				DisplayMessage("Please specify a database name", true);
			else if (Server.Length == 0)
				DisplayMessage("Please specify a server name", true);
			else
				DialogResult = DialogResult.OK;
			
		}


	}
}
