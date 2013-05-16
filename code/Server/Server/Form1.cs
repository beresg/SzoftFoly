using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Threading;
using System.Net.Sockets;
using HFS.HttpServer;
using System.Drawing;

namespace HFS
{
    public partial class Form1 : Form
    {
        //List<HFS.HttpServer.Label> labels;
        List<HFS.HttpServer.File> files;

        HttpServer.HttpServer server;

        String path = @"c:\";
        Int32 idCounter = 10;

        public Form1()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            /*labels = new List<HFS.HttpServer.Label>();
            labels.Add(new HFS.HttpServer.Label { Name = "zene", Parent = "" });
            labels.Add(new HFS.HttpServer.Label { Name = "pop", Parent = "zene" });
            labels.Add(new HFS.HttpServer.Label { Name = "rock", Parent = "zene" });
            labels.Add(new HFS.HttpServer.Label { Name = "soundtrack", Parent = "zene" });**/

            files = new List<HFS.HttpServer.File>();
            files.Add(new HFS.HttpServer.File { FileName = "pop1.mp3", Labels = new List<String>() { "zene", "pop" } });
            files.Add(new HFS.HttpServer.File { FileName = "pop2.mp3", Labels = new List<String>() { "zene", "pop" } });
            files.Add(new HFS.HttpServer.File { FileName = "pop3.mp3", Labels = new List<String>() { "zene", "pop" } });
            files.Add(new HFS.HttpServer.File { FileName = "rock1.mp3", Labels = new List<String>() { "zene", "rock" } });
            files.Add(new HFS.HttpServer.File { FileName = "rock2.mp3", Labels = new List<String>() { "zene", "rock" } });
            files.Add(new HFS.HttpServer.File { FileName = "zene1.mp3", Labels = new List<String>() { "zene", "soundtrack" }, ID = "1", Path = @"D:\ELTE\backup\2013tavasz\szoftverfolyamat2\SzoftFoly\code\Server\Server\bin\Debug\zene1.mp3" });
            files.Add(new HFS.HttpServer.File { FileName = "zene2.mp3", Labels = new List<String>() { "zene", "soundtrack" }, ID = "2", Path = @"D:\ELTE\backup\2013tavasz\szoftverfolyamat2\SzoftFoly\code\Server\Server\bin\Debug\zene2.mp3" });
            files.Add(new HFS.HttpServer.File { FileName = "zene3.mp3", Labels = new List<String>() { "zene", "soundtrack" }, ID = "3", Path = @"D:\ELTE\backup\2013tavasz\szoftverfolyamat2\SzoftFoly\code\Server\Server\bin\Debug\zene3.mp3" });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            server = new HttpServer.HttpServer();
            server.Port = 8888;
            server.Files = files;
            //server.Labels = labels;
            server.ResponseEncoding = HttpServer.HttpServer.Encoding.None;
            server.Root = @"static\";
            //server.Root = "";

            Thread thread = new Thread(server.Start);
            thread.Start();

            this.treeView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_DragEnter);
            this.treeView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
             
            listView1.ItemDrag += new ItemDragEventHandler(listView1_ItemDrag);

            LoadFiles();
        }

        void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if(((ListViewItem)e.Item).SubItems[1].Text=="File")
                DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Add(textBox1.Text);
        }

        private void treeView_DragEnter(object sender,
            System.Windows.Forms.DragEventArgs e)
        {

            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);


            //if (DestinationNode != null && DestinationNode.Level == 0)
            e.Effect = DragDropEffects.Move;
            //else
            //    e.Effect = DragDropEffects.None;
        }

        private void treeView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.ListViewItem", false))
            {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);
                ListViewItem lvi = (ListViewItem)e.Data.GetData("System.Windows.Forms.ListViewItem");
                if (DestinationNode!=null && DestinationNode.Parent == null)
                {
                    DestinationNode.Nodes.Add(lvi.Text);
                    DestinationNode.Expand();
                    IEnumerable<HFS.HttpServer.File> file = server.Files.Where(x => x.Path == path + Path.DirectorySeparatorChar + lvi.Text);

                    if (file.Count() == 0)
                    {
                        server.Files.Add(new HttpServer.File()
                        {
                            ID = idCounter.ToString(),
                            FileName = lvi.Text,
                            Labels = new List<string>() { DestinationNode.Text },
                            Path = path + Path.DirectorySeparatorChar + lvi.Text
                            
                        });

                        idCounter++;
                    }
                    else
                    {
                        file.First().Labels.Add(DestinationNode.Text);
                    }
                }
            }
        }        

        private void LoadFiles()
        {
            listView1.Items.Clear();

            DirectoryInfo nodeDirInfo = new DirectoryInfo(path);
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[] {
                    new ListViewItem.ListViewSubItem(item, "Directory"), 
                    new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToShortDateString())
                };
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[] {
                    new ListViewItem.ListViewSubItem(item, "File"),
                    new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToShortDateString())
                };

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server.Stop();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count>0 && listView1.SelectedItems[0].SubItems[1].Text == "Directory")
            {
                path = path + Path.DirectorySeparatorChar + listView1.SelectedItems[0].Text;
                textBox2.Text = path;
                LoadFiles();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                TreeNode selectedNode = treeView1.SelectedNode;

                switch (selectedNode.Level)
                {
                    case 0:
                        List<HFS.HttpServer.File> files = server.Files.Where(x => x.Labels.Contains(selectedNode.Text)).ToList();

                        for (Int32 i = 0; i < files.Count; ++i)
                        {
                            files[i].Labels.Remove(selectedNode.Text);

                            if (files[i].Labels.Count == 0)
                                server.Files.Remove(files[i]);
                        }

                        break;
                    case 1:                        
                        String fileName = selectedNode.Text;
                        String labelName = selectedNode.Parent.Text;

                        files = server.Files.Where(x => x.FileName == fileName && x.Labels.Contains(labelName)).ToList();

                        if (files.Count == 1)
                        {
                            if (files[0].Labels.Count == 1)
                                server.Files.Remove(files[0]);
                            else
                                files[0].Labels.Remove(labelName);

                        }


                        break;
                }

                treeView1.Nodes.Remove(selectedNode);

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            path = textBox2.Text;
            LoadFiles();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = Directory.GetParent(path);

            if (di != null)
            {
                path = di.FullName;
                textBox2.Text = path;
                LoadFiles();
            }
        }
    }
}
