using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Etier.IconHelper;

namespace YodaClient
{

    public partial class Form1 : Form
    {

        #region Variables
        private IconListManager iconListManager;
        private string currentDirectory;
        private EnterpriseDT.Net.Ftp.FTPFile[] fileInfos;
        private ListViewSorter lvSorter;

        #endregion

        public Form1()
        {
            InitializeComponent();
            //this.Icon = Properties.Resources.yoda1;
            columnHeader1.Width = 183 - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
            
            iconListManager = new Etier.IconHelper.IconListManager(smallIconList, Etier.IconHelper.IconReader.IconSize.Small);
            currentDirectory = "/polling/data/pos";

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            lvSorter = new ListViewSorter();
            listView1.ListViewItemSorter = lvSorter;

        }

        private void ConnectFTP()
        {
            try
            {
                ftpConnection1.Connect();
                //ftpConnection1.ChangeWorkingDirectory(currentDirectory);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                MessageBox.Show("Socket Error: " + e.ErrorCode + " " + e.Message);
                return;
            }
            catch (EnterpriseDT.Net.Ftp.FTPException e)
            {
                MessageBox.Show("FTP Error: " + e.Message);
                ftpConnection1.Close();
                return;
            }
            return;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region FTP Events

        private void ftpConnection1_Connecting(object sender, EnterpriseDT.Net.Ftp.FTPConnectionEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Connecting...";
        }

        private void ftpConnection1_Connected(object sender, EnterpriseDT.Net.Ftp.FTPConnectionEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Connected";
        }

        private void ftpConnection1_Closing(object sender, EnterpriseDT.Net.Ftp.FTPConnectionEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Disconnecting...";
        }

        private void ftpConnection1_Closed(object sender, EnterpriseDT.Net.Ftp.FTPConnectionEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Disconnected";
        }

        private void ftpConnection1_DirectoryListed(object sender, EnterpriseDT.Net.Ftp.FTPDirectoryListEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Directory listed";
        }

        private void ftpConnection1_DirectoryListing(object sender, EnterpriseDT.Net.Ftp.FTPDirectoryListEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Directory Listing...";
        }

        private void ftpConnection1_Downloading(object sender, EnterpriseDT.Net.Ftp.FTPFileTransferEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Downloading...";
        }

        private void ftpConnection1_Downloaded(object sender, EnterpriseDT.Net.Ftp.FTPFileTransferEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Download Complete";
        }

        private void ftpConnection1_LoggingIn(object sender, EnterpriseDT.Net.Ftp.FTPLogInEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Authenticating...";
        }

        private void ftpConnection1_LoggedIn(object sender, EnterpriseDT.Net.Ftp.FTPLogInEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Authenticated";
        }

        #endregion

        private void Form1_Shown(object sender, EventArgs e)
        {
            ConnectFTP();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ftpConnection1.IsConnected)
            {
                try
                {
                    ftpConnection1.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        private void ftpConnection1_ServerDirectoryChanging(object sender, EnterpriseDT.Net.Ftp.FTPDirectoryEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "Changing Directory...";
        }

        private void ftpConnection1_ServerDirectoryChanged(object sender, EnterpriseDT.Net.Ftp.FTPDirectoryEventArgs e)
        {
            if(!e.NewDirectory.StartsWith("/polling/data/pos"))
            {
                try
                {
                    ftpConnection1.ChangeWorkingDirectory("/polling/data/pos");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("FTP Error: " + ex.Message);
                }
                return;
            }
            
            this.toolStripStatusLabel1.Text = "Directory Changed";
            currentDirectory = e.NewDirectory;
            this.label1.Text = currentDirectory;
            RefreshList();
        }

        private void RefreshList()
        {
            MyListViewItem item = new MyListViewItem();

            listView1.BeginUpdate();
            listView1.Items.Clear();

            item.Text = "..";
            item.ImageIndex = iconListManager.GetIcon(IconListManager.IconTypes.Up);
            item.IsDir = true;
            listView1.Items.Add(item);

            try
            {
                
                fileInfos = ftpConnection1.GetFileInfos();
            }
            catch (Exception e)
            {
                MessageBox.Show("FTP Error: " + e.Message);
                return;
            }
            
            foreach (EnterpriseDT.Net.Ftp.FTPFile file in fileInfos)
            {
                if(file.Name.StartsWith("."))
                    continue;

                item = new MyListViewItem();
                item.Text = file.Name;

                if (file.Dir == true)
                {
                    item.ImageIndex = iconListManager.GetIcon(IconListManager.IconTypes.FolderClosed);
                    item.SubItems.Add("0");
                    item.IsDir = true;
                }
                else
                {
                    item.ImageIndex = iconListManager.GetIcon(file.Name);
                    item.SubItems.Add(file.Size.ToString());
                    item.IsDir = false;
                }
                item.SubItems.Add(file.LastModified.ToString());
                listView1.Items.Add(item);
            }
            listView1.Sort();
            listView1.EndUpdate();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvSorter.Order == SortOrder.Ascending)
                {
                    lvSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvSorter.SortColumn = e.Column;
                lvSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();

        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            MyListViewItem item = (MyListViewItem)listView1.SelectedItems[0];
            try
            {
                if (item.Index == 0)
                {
                    if (currentDirectory == "/polling/data/pos")
                        return;
                    else
                    {
                        ftpConnection1.ChangeWorkingDirectoryUp();
                        return;
                    }
                }
                else if (item.IsDir)
                    ftpConnection1.ChangeWorkingDirectory(currentDirectory + "/" + item.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("FTP Error: " + ex.Message);
            }
            return;
        }
    }
}

class MyListViewItem : ListViewItem
{
    private bool isDir;

    public bool IsDir
    {
        get { return isDir; }
        set { isDir = value; }
    }
}
