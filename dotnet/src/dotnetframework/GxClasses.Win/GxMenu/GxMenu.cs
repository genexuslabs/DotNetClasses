using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace GeneXus.Forms
{
	
	public class GxMenu : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TreeView menuTree;
		private System.Windows.Forms.ContextMenu treeContext;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		public bool canExit = true;
		private int _intX;
		private int _intY;
			/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GxMenu()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			LoadMenu();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
			this.menuTree = new System.Windows.Forms.TreeView();
			this.treeContext = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// menuTree
			// 
			this.menuTree.ContextMenu = this.treeContext;
			this.menuTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.menuTree.HotTracking = true;
			this.menuTree.ImageIndex = -1;
			this.menuTree.Location = new System.Drawing.Point(0, 0);
			this.menuTree.Name = "menuTree";
			this.menuTree.SelectedImageIndex = -1;
			this.menuTree.Size = new System.Drawing.Size(280, 293);
			this.menuTree.TabIndex = 0;
			this.menuTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.menuTree_MouseDown);
			this.menuTree.Click += new System.EventHandler(this.menuTree_Click);
			this.menuTree.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.menuTree_KeyPress);
			// 
			// treeContext
			// 
			this.treeContext.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuItem1,
																						this.menuItem2});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Always On Top";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.Text = "Transparent";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// GxMenu
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(280, 293);
			this.Controls.Add(this.menuTree);
			this.Name = "GxMenu";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "GeneXus Developer Menu";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.GxMenu_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		public virtual void LoadMenu()
		{
		}
		public virtual void ExecuteOption(string pgmName)
		{
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
            MenuItem menuItem = sender as MenuItem;
			if ( this.Opacity < 1)
			{
				this.Opacity = 1;
                menuItem.Checked = false;
			}

			else
			{
				this.Opacity = 0.7;
                menuItem.Checked = true;
			}

		}
		private void menuItem1_Click(object sender, System.EventArgs e)
		{
            MenuItem menuItem = sender as MenuItem;
			if (this.TopMost == true)
			{
				this.TopMost = false;
                menuItem.Checked = false;
			}
			else 
			{
				this.TopMost = true;
                menuItem.Checked = true;
			}
		}
		
		private void menuTree_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			_intX = e.X	;
			_intY = e.Y ;
		}
		private void menuTree_Click(object sender, System.EventArgs e)
		{
			TreeView objTreeView = (TreeView)sender;
			TreeNode objNode = null;
			TreeNode objNode0 = objTreeView.GetNodeAt(_intX, _intY);
			
			if (_intY >= objNode0.Bounds.Top && _intY <= objNode0.Bounds.Bottom &&
                _intX >= objNode0.Bounds.Left && _intX <= objNode0.Bounds.Right)
				objNode = objNode0;

			if (objNode != null)
			{
				if (objNode.Tag != null)
					ExecuteOption(objNode.Tag.ToString());
			}
		}
		private void menuTree_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
				if (menuTree.SelectedNode != null)
				{
					if (menuTree.SelectedNode.GetNodeCount(false) > 0)
					{
						if ( menuTree.SelectedNode.IsExpanded)
							menuTree.SelectedNode.Collapse();
						else
							menuTree.SelectedNode.Expand();
					}
					else
						if (menuTree.SelectedNode.Tag != null)
							ExecuteOption(menuTree.SelectedNode.Tag.ToString());
				}
		}

		private void GxMenu_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (! canExit )
				e.Cancel = true;
		}

		public TreeNodeCollection Nodes
		{
			get	{ return menuTree.Nodes; }
		}
	}
}
