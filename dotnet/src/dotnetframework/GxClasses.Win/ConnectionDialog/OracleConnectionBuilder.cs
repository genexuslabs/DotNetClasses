using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

namespace ConnectionBuilder
{
	
	public class OracleConnectionDialog : System.Windows.Forms.Form, IConnectionDialog
	{
		private System.Windows.Forms.Button testConnection;
		private System.Windows.Forms.CheckBox checkBoxBlankPwd;
		private System.Windows.Forms.Label label4;
		private DklTextBox password;
		private DklTextBox userName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cboServers;
		private System.Windows.Forms.Panel panel1;
		private System.Data.OleDb.OleDbConnection connectionTest;
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.PictureBox pictureBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OracleConnectionDialog()
		{
			
			InitializeComponent();
			
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
	
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(OracleConnectionDialog));
			this.testConnection = new System.Windows.Forms.Button();
			this.checkBoxBlankPwd = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.password = new ConnectionBuilder.DklTextBox();
			this.userName = new ConnectionBuilder.DklTextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cboServers = new System.Windows.Forms.ComboBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.connectionTest = new System.Data.OleDb.OleDbConnection();
			this.OK = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// testConnection
			// 
			this.testConnection.Location = new System.Drawing.Point(144, 160);
			this.testConnection.Name = "testConnection";
			this.testConnection.Size = new System.Drawing.Size(104, 24);
			this.testConnection.TabIndex = 9;
			this.testConnection.Text = "&Test Connection";
			this.testConnection.Click += new System.EventHandler(this.testConnection_Click);
			// 
			// checkBoxBlankPwd
			// 
			this.checkBoxBlankPwd.Location = new System.Drawing.Point(8, 56);
			this.checkBoxBlankPwd.Name = "checkBoxBlankPwd";
			this.checkBoxBlankPwd.Size = new System.Drawing.Size(104, 16);
			this.checkBoxBlankPwd.TabIndex = 8;
			this.checkBoxBlankPwd.Text = "&Blank password";
			this.checkBoxBlankPwd.CheckedChanged += new System.EventHandler(this.checkBoxBlankPwd_CheckedChanged_1);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 56);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(240, 16);
			this.label4.TabIndex = 3;
			this.label4.Text = "2. Enter &information to log on to the server:";
			// 
			// password
			// 
			this.password.Location = new System.Drawing.Point(88, 32);
			this.password.Name = "password";
			this.password.PasswordChar = '*';
			this.password.Size = new System.Drawing.Size(120, 20);
			this.password.TabIndex = 7;
			this.password.Text = "";
			// 
			// userName
			// 
			this.userName.Location = new System.Drawing.Point(88, 8);
			this.userName.Name = "userName";
			this.userName.Size = new System.Drawing.Size(120, 20);
			this.userName.TabIndex = 5;
			this.userName.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 24);
			this.label3.TabIndex = 6;
			this.label3.Text = "&Password:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 24);
			this.label2.TabIndex = 4;
			this.label2.Text = "&User name:";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(184, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "1. &Enter a Oracle Server:";
			// 
			// cboServers
			// 
			this.cboServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
			this.cboServers.Location = new System.Drawing.Point(8, 24);
			this.cboServers.Name = "cboServers";
			this.cboServers.Size = new System.Drawing.Size(184, 21);
			this.cboServers.TabIndex = 2;
			this.cboServers.SelectedIndexChanged += new System.EventHandler(this.cboServers_SelectedIndexChanged_1);
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.password,
																				 this.userName,
																				 this.label2,
																				 this.label3,
																				 this.checkBoxBlankPwd});
			this.panel1.Location = new System.Drawing.Point(8, 72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(240, 80);
			this.panel1.TabIndex = 4;
			// 
			// OK
			// 
			this.OK.Location = new System.Drawing.Point(32, 200);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(104, 24);
			this.OK.TabIndex = 10;
			this.OK.Text = "OK";
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// Cancel
			// 
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.Location = new System.Drawing.Point(144, 200);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(104, 24);
			this.Cancel.TabIndex = 11;
			this.Cancel.Text = "Cancel";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Bitmap)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(208, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(32, 32);
			this.pictureBox1.TabIndex = 42;
			this.pictureBox1.TabStop = false;
			// 
			// OracleConnectionDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.Cancel;
			this.ClientSize = new System.Drawing.Size(258, 238);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.pictureBox1,
																		  this.Cancel,
																		  this.OK,
																		  this.panel1,
																		  this.testConnection,
																		  this.label4,
																		  this.label1,
																		  this.cboServers});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OracleConnectionDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Connection String";
			this.Load += new System.EventHandler(this.UserControl1_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new OracleConnectionDialog());
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
			password.Enabled = ! checkBoxBlankPwd.Checked;
		}

		private void UserControl1_Load(object sender, System.EventArgs e)
		{

		}

		private bool ParseConnection(string sValue)
		{
			string[] stringPairs = sValue.Split(';');
			for (int i = 0; i < stringPairs.Length ; i++)
			{
				string [] sNameValue = stringPairs[i].Split('=');
				if (sNameValue.Length == 2)
				{
					SetProperty(sNameValue[0], sNameValue[1]);
				}
			}
			return true;
		}

		private const string DATA_SOURCE			= "DATA SOURCE";
		private const string USER_ID				= "USER ID";
		private const string USER_PASSWORD				= "PASSWORD";

		private void SetProperty(string sPropName, string sPropValue)
		{
			string sProperty = sPropName.ToUpper().Trim();
			
			switch (sProperty)
			{

				case DATA_SOURCE:
					Server = sPropValue;
					break;
				case USER_ID:
					userName.Text = sPropValue;
					break;
				case USER_PASSWORD:
					password.Text = sPropValue;
					break;

			}
		}

		private void comboBases_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

		private void testConnection_Click(object sender, System.EventArgs e)
		{
			try
			{
				connectionTest = new System.Data.OleDb.OleDbConnection(ConnectionString);
				connectionTest.Open();
				connectionTest.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}
			MessageBox.Show("Test connection succeeded", "DeKlarit", MessageBoxButtons.OK , MessageBoxIcon.Information);
		}

		private void cboServers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
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
				return cboServers.Text;
			}
		
			set
			{
				cboServers.Text = value;
			}
		}

		public string UserAndPassword
		{
			get
			{
				string sPassword = (password.Enabled)? password.Text : "";
				return String.Format("User ID={0}{1}", userName.Text,  ";Password=" + sPassword );
			}
		}

		public string ConnectionString
		{
			get 
			{
				string sConn = String.Format("Provider={0};Persist Security Info={1};{2};Data Source={3};",
					"MSDAORA.1",  "True" , UserAndPassword, Server);
				return sConn;
			}
			set
			{
				ParseConnection(value);
			}

		}

		public bool Show(string initString)
		{
			ConnectionString = initString;
			return ShowDialog() == DialogResult.OK;
		}

		private void OK_Click(object sender, System.EventArgs e)
		{
			if (Server.Length == 0)
				MessageBox.Show("Please specify a server name", "DeKlarit", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else
				DialogResult = DialogResult.OK;
			
		}
	}
}
