using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using GeneXus.Encryption;
using System.Threading;
using GeneXus.Data.ADO;
using GeneXus.Data;
using GeneXus.Application;
using GeneXus.Data.NTier;
using GeneXus.Configuration;

namespace GxConfig
{
    
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.ComboBox cbDataStores;
        
        private System.ComponentModel.Container components = null;

        private string workingFile;
        private System.Windows.Forms.TextBox tbUserName;
        private System.Windows.Forms.TextBox tbUserPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbDatasource;
        private System.Windows.Forms.TextBox tbDB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbDBMS;
        private Hashtable dataStores;
        private Button saveCheck;
        string currentlySelected;
        public Form1(string wf)
        {
            
            InitializeComponent();

            workingFile = wf;
            currentlySelected = "";
            tbUserPassword.UseSystemPasswordChar = true;
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
			this.tbUserName = new System.Windows.Forms.TextBox();
			this.tbUserPassword = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btSave = new System.Windows.Forms.Button();
			this.cbDataStores = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tbDatasource = new System.Windows.Forms.TextBox();
			this.tbDB = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.tbDBMS = new System.Windows.Forms.TextBox();
			this.saveCheck = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// tbUserName
			// 
			this.tbUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbUserName.Location = new System.Drawing.Point(101, 127);
			this.tbUserName.Name = "tbUserName";
			this.tbUserName.Size = new System.Drawing.Size(294, 20);
			this.tbUserName.TabIndex = 0;
			// 
			// tbUserPassword
			// 
			this.tbUserPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbUserPassword.Location = new System.Drawing.Point(101, 151);
			this.tbUserPassword.Name = "tbUserPassword";
			this.tbUserPassword.Size = new System.Drawing.Size(294, 20);
			this.tbUserPassword.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 132);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 15);
			this.label1.TabIndex = 1;
			this.label1.Text = "Name";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 154);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 17);
			this.label2.TabIndex = 1;
			this.label2.Text = "Password";
			// 
			// btSave
			// 
			this.btSave.Location = new System.Drawing.Point(24, 192);
			this.btSave.Name = "btSave";
			this.btSave.Size = new System.Drawing.Size(149, 24);
			this.btSave.TabIndex = 2;
			this.btSave.Text = "Save";
			this.btSave.Click += new System.EventHandler(this.btSave_Click);
			// 
			// cbDataStores
			// 
			this.cbDataStores.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cbDataStores.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbDataStores.Location = new System.Drawing.Point(27, 10);
			this.cbDataStores.Name = "cbDataStores";
			this.cbDataStores.Size = new System.Drawing.Size(368, 21);
			this.cbDataStores.TabIndex = 3;
			this.cbDataStores.SelectedIndexChanged += new System.EventHandler(this.cbDataStores_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 83);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 20);
			this.label3.TabIndex = 7;
			this.label3.Text = "Server/DS";
			// 
			// tbDatasource
			// 
			this.tbDatasource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbDatasource.Location = new System.Drawing.Point(101, 79);
			this.tbDatasource.Name = "tbDatasource";
			this.tbDatasource.Size = new System.Drawing.Size(294, 20);
			this.tbDatasource.TabIndex = 5;
			// 
			// tbDB
			// 
			this.tbDB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbDB.Location = new System.Drawing.Point(101, 103);
			this.tbDB.Name = "tbDB";
			this.tbDB.Size = new System.Drawing.Size(294, 20);
			this.tbDB.TabIndex = 4;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 107);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 16);
			this.label4.TabIndex = 6;
			this.label4.Text = "Database";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(24, 60);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 15);
			this.label5.TabIndex = 9;
			this.label5.Text = "DBMS";
			// 
			// tbDBMS
			// 
			this.tbDBMS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbDBMS.Enabled = false;
			this.tbDBMS.Location = new System.Drawing.Point(101, 55);
			this.tbDBMS.Name = "tbDBMS";
			this.tbDBMS.Size = new System.Drawing.Size(294, 20);
			this.tbDBMS.TabIndex = 8;
			// 
			// saveCheck
			// 
			this.saveCheck.Location = new System.Drawing.Point(246, 192);
			this.saveCheck.Name = "saveCheck";
			this.saveCheck.Size = new System.Drawing.Size(149, 24);
			this.saveCheck.TabIndex = 10;
			this.saveCheck.Text = "Save and Test Connection";
			this.saveCheck.UseVisualStyleBackColor = true;
			this.saveCheck.Click += new System.EventHandler(this.saveCheck_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(424, 236);
			this.Controls.Add(this.saveCheck);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.tbDBMS);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.tbDatasource);
			this.Controls.Add(this.tbDB);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cbDataStores);
			this.Controls.Add(this.btSave);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tbUserName);
			this.Controls.Add(this.tbUserPassword);
			this.Controls.Add(this.label2);
			this.Name = "Form1";
			this.Text = "GX Configuration";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

        [STAThread]
        static void Main(string[] args)
        {
            string fileName;
            if (args.Length == 0)
                fileName = "web.config";
            else
                fileName = args[0];

			Application.Run(new Form1(fileName));
		}

		private Hashtable loadConfig(string fileName)
		{
			Hashtable ds = new Hashtable();
			try
			{
				XmlTextReader xmlr = new XmlTextReader(fileName);
				while (xmlr.Read())
				{
					if (xmlr.Name == "add")
					{
						string infoKey = xmlr.GetAttribute("key");
						if (infoKey != null && infoKey.StartsWith("Connection-"))
						{
							int startDSName = infoKey.IndexOf("-");
							int endDSName = infoKey.IndexOf("-", startDSName + 1);
							string dataStoreName = infoKey.Substring(startDSName + 1, endDSName - startDSName - 1);
							DataStoreInfo s = (DataStoreInfo)(ds[dataStoreName]);
							if (s == null)
							{
								s = new DataStoreInfo(dataStoreName);
								ds.Add(dataStoreName, s);
							}

							if (infoKey.StartsWith("Connection-" + s.Name.Trim() + "-User"))
							{
								s.UserName = xmlr.GetAttribute("value");
							}
							if (infoKey.StartsWith("Connection-" + s.Name.Trim() + "-Password"))
							{
								s.UserPassword = xmlr.GetAttribute("value");
							}
							if (infoKey.StartsWith("Connection-" + s.Name.Trim() + "-DBMS"))
							{
								s.DBMS = xmlr.GetAttribute("value");
							}
							if (infoKey.StartsWith("Connection-" + s.Name.Trim() + "-Datasource"))
							{
								s.Datasource = xmlr.GetAttribute("value");
							}
							if (infoKey.StartsWith("Connection-" + s.Name.Trim() + "-DB"))
							{
								s.DBName = xmlr.GetAttribute("value");
							}
						}
					}
				}
				xmlr.Close();
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show(fileName + " not found.");
			}
			return ds;
		}
		private void btSave_Click(object sender, System.EventArgs e)
		{
			Save();
		}

        private void Save()
        {
            // Save latest control 
            saveCurrentDataStoreInfo();
            // Load config
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(workingFile);
            if (xmlDoc == null)
            {
                MessageBox.Show("Invalid XML format");
                return;
            }
            // Update config
            updateDocument(xmlDoc.DocumentElement.FirstChild);

			short retry = 3;
			while (retry >= 0)
			{
				try
				{
					xmlDoc.Save(workingFile);
					break;
				}
				catch (Exception ex)
				{
					if (retry == 0)
						MessageBox.Show("Could not save file: " + ex.Message);
				}
				retry--;
				Thread.Sleep(5000);
			}
		}

        private void updateDocument(XmlNode nextNode)
        {
            while (nextNode != null)
            {
                if (nextNode.HasChildNodes)   // Config nodes have no children
                {
                    
                    updateDocument(nextNode.FirstChild);
                }
                else
                {
                    if (nextNode.Attributes != null && nextNode.Attributes.Count > 0)		// Config nodes have attris
                    {
                        XmlAttribute xmlAttKey = nextNode.Attributes["key"];
                        XmlAttribute xmlAttValue = nextNode.Attributes["value"];
                        if (xmlAttKey != null && xmlAttValue != null)
                        {
                            if (xmlAttKey.Value.StartsWith("Connection"))
                            {
                                foreach (DataStoreInfo dsi in dataStores.Values)
                                {
                                    if (xmlAttKey.Value == "Connection-" + dsi.Name.Trim() + "-DB")
                                        xmlAttValue.Value = dsi.DBName;
                                    else if (xmlAttKey.Value == "Connection-" + dsi.Name.Trim() + "-Datasource")
                                        xmlAttValue.Value = dsi.Datasource;
                                    else if (xmlAttKey.Value == "Connection-" + dsi.Name.Trim() + "-User")
                                        xmlAttValue.Value = dsi.UserName;
                                    else if (xmlAttKey.Value == "Connection-" + dsi.Name.Trim() + "-Password")
                                        xmlAttValue.Value = dsi.UserPassword;
                                }
                            }
                        }
                    }
                }
                
                nextNode = nextNode.NextSibling;
            }
        }
        private void Form1_Load(object sender, System.EventArgs e)
        {
            dataStores = loadConfig(workingFile);
            foreach (DataStoreInfo dsi in dataStores.Values)
            {
                this.cbDataStores.Items.Add(dsi.Name);
            }
            if (this.cbDataStores.Items.Count > 0)
            {
                this.cbDataStores.SelectedIndex = 0;                
            }
        }
        private void btClose_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

		private void cbDataStores_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			saveCurrentDataStoreInfo();
			currentlySelected = this.cbDataStores.Text;
			tbDBMS.Text = ((DataStoreInfo)(dataStores[cbDataStores.SelectedItem.ToString()])).DBMS;
			tbDatasource.Text = decrypt(((DataStoreInfo)(dataStores[cbDataStores.SelectedItem.ToString()])).Datasource);
			tbDB.Text = decrypt(((DataStoreInfo)(dataStores[cbDataStores.SelectedItem.ToString()])).DBName);
			tbUserName.Text = decrypt(((DataStoreInfo)(dataStores[cbDataStores.SelectedItem.ToString()])).UserName);
			tbUserPassword.Text = decrypt(((DataStoreInfo)(dataStores[cbDataStores.SelectedItem.ToString()])).UserPassword);

		}
		private void saveCurrentDataStoreInfo()
		{
			if (currentlySelected.Length > 0)
			{
				DataStoreInfo dsi = (DataStoreInfo)(dataStores[currentlySelected]);
				if (dsi != null)
				{
					dsi.DBName = encrypt(tbDB.Text);
					dsi.Datasource = encrypt(tbDatasource.Text);
					dsi.UserName = encrypt(tbUserName.Text);
					dsi.UserPassword = encrypt(tbUserPassword.Text);
				}
			}
		}

		private string decrypt(string cfgBuf)
		{
			string ret = string.Empty;
			try
			{
				if (!CryptoImpl.Decrypt(ref ret, cfgBuf))
				{
					ret = cfgBuf;
				}
			}
			catch (Exception)
			{
			}
			return ret;
		}

		private string encrypt(string value)
		{
			return CryptoImpl.Encrypt(value, Crypto.GetServerKey());
		}

		private bool CheckConnection()
		{
			bool connected = false;
			if (!string.IsNullOrEmpty(currentlySelected))
			{
				Config.ConfigFileName = workingFile;
				GxContext context = new GxContext();

				DataStoreUtil.LoadDataStores(context);
				IGxDataStore dsDefault = context.GetDataStore(currentlySelected);
				try
				{
					dsDefault.Connection.Open();
				}
				catch (Exception e)
				{
					MessageBox.Show(Messages.ConnectionError + " - " + e.Message, Messages.ConnectionErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				if (dsDefault.Connection.Opened)
				{
					MessageBox.Show(Messages.ConnectionOK, Messages.ConnectionOKTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
					connected = true;
				}
			}
			return connected;

		}

		private void saveCheck_Click(object sender, EventArgs e)
		{
			Save();
			CheckConnection();
		}
	}
	public class DataStoreInfo
	{
		string name;
		string userName;
		string dbms;
		string userPassword;
		string dataSource;
		string dbName;
		public DataStoreInfo(string name)
		{
			this.name = name;
		}
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}
		public string UserPassword
		{
			get { return userPassword; }
			set { userPassword = value; }
		}
		public string DBMS
		{
			get { return dbms; }
			set { dbms = value; }
		}
		public string Datasource
		{
			get { return dataSource; }
			set { dataSource = value; }
		}
		public string DBName
		{
			get { return dbName; }
			set { dbName = value; }
		}
	}
}
