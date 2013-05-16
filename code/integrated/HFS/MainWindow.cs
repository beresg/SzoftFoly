using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace HFS
{
    public partial class MainWindow : Form
    {
        private String CURRENT_PATH = @"c:\";
        private List<HFS.HttpServer.File> files = new List<HFS.HttpServer.File>();
        private HttpServer.HttpServer server;
        private LinkedList<String> LAST_PATH_LIST = new LinkedList<string>();
        private Thread thread;
        private Int32 idCounter;

        public MainWindow()
        {
            InitializeComponent();

            idCounter = 0;

            server = new HttpServer.HttpServer();
            
            server.Port = 8888;
            server.Files = files;

            server.ResponseEncoding = HttpServer.HttpServer.Encoding.GZip;
            server.Root = "static/";

            thread = new Thread(server.Start);
            thread.Start();

            LoadFiles(CURRENT_PATH);
            tbPath.Text = CURRENT_PATH;
        }

        private void LoadFiles(String path)
        {
            CURRENT_PATH = path;

            tvLocal.Nodes.Clear();

            DirectoryInfo nodeDirInfo = new DirectoryInfo(path);
            TreeNode item = null;

            // first extremal item ..
            item = new TreeNode("..", 1, 1);
            item.Tag = null;
            tvLocal.Nodes.Add(item);

            try
            {
                foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
                {
                    item = new TreeNode(dir.Name, 1, 1);
                    item.Tag = path;
                    tvLocal.Nodes.Add(item);
                }

                foreach (FileInfo file in nodeDirInfo.GetFiles())
                {
                    item = new TreeNode(file.Name, 2, 2);
                    item.Tag = path;

                    tvLocal.Nodes.Add(item);
                }
            }
            catch (Exception)
            {}
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            if (tvRemote.Nodes.ContainsKey(tbTag.Text) == false)
            {
                tvRemote.Nodes.Add(tbTag.Text, tbTag.Text, 0, 0);
                epTag.Clear();
            }
            else
            {
                epTag.SetError(tbTag, "A hozzáadni kívánt tag már létezik!");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (server.Running)
                server.Stop();

            Application.Exit();
        }

        private void tvRemote_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode destinationNode = ((TreeView)sender).GetNodeAt(pt);
                TreeNode sourceNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                if (destinationNode != null && destinationNode.Parent == null)
                {
                    destinationNode.Nodes.Add(sourceNode.Text, sourceNode.Text, 3, 3);
                    destinationNode.Expand();

                    String filepath = sourceNode.Tag.ToString() + Path.DirectorySeparatorChar + sourceNode.Text;

                    IEnumerable<HFS.HttpServer.File> file = server.Files.Where(x => x.Path== filepath);

                    if (file.Count() == 0)
                    {
                        FileInfo fi = new FileInfo(filepath);

                        server.Files.Add(new HttpServer.File()
                        {
                            ID = idCounter.ToString(),
                            FileName = sourceNode.Text,
                            Labels = new List<string>() { destinationNode.Text },
                            Path = filepath,
                            Size = fi.Length,
                            Date = fi.LastWriteTime
                        });

                        idCounter++;
                    }
                    else
                        file.First().Labels.Add(destinationNode.Text);
                }
            }
        }

        private void stopServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (server.Running)
            {
                server.Stop();
                thread.Abort();
                
                this.toolStripSplitButton.Image = global::HFS.Properties.Resources.stop;
                startServerToolStripMenuItem.Enabled = true;
                stopServerToolStripMenuItem.Enabled = false;
            }
        }

        private void startServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!server.Running)
            {
                thread = new Thread(server.Start);
                thread.Start();

                this.toolStripSplitButton.Image = global::HFS.Properties.Resources.play;
                startServerToolStripMenuItem.Enabled = false;
                stopServerToolStripMenuItem.Enabled = true;
            }
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            if (tvRemote.SelectedNode != null)
            {
                TreeNode selectedNode = tvRemote.SelectedNode;

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

                tvRemote.Nodes.Remove(selectedNode);

            }

            btRemove.Enabled = false;
        }

        private void tvLocal_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void tvRemote_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void tbTag_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(tbTag.Text))
                btAdd.Enabled = false;
            else
                btAdd.Enabled = true;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server.Running)
                server.Stop();

            Application.Exit();
        }

        private void tvRemote_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
                btRemove.Enabled = false;
            else
                btRemove.Enabled = true;
        }

        private void tvLocal_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                String path = e.Node.Tag.ToString();
                LAST_PATH_LIST.AddLast(CURRENT_PATH);
                LoadFiles(path + Path.DirectorySeparatorChar + e.Node.Text);
                tbPath.Text = path + Path.DirectorySeparatorChar + e.Node.Text;
            }
            else if (LAST_PATH_LIST.Count > 0)
            {
                String path= LAST_PATH_LIST.Last.Value;
                LAST_PATH_LIST.RemoveLast();
                LoadFiles(path);
                tbPath.Text = path;
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsWindow sw = new SettingsWindow(server.Port);
            if (sw.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!server.Running)
                {
                    server.Port = sw.Port;
                }
                else
                    MessageBox.Show("Szerver futása közben nem lehet portot állítani!");
            }
        }

        private void btGo_Click(object sender, EventArgs e)
        {
            String path = tbPath.Text;
            LoadFiles(path);
        }
    }
}
