using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace HFS
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            ListDirectory(tvLocal, "C:\\");
        }

        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);

            var directoryNodes = CreateDirectoryNode(rootDirectoryInfo);

            if (directoryNodes != null)
                treeView.Nodes.Add(directoryNodes);
        }

        private static TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {

            var directoryNode = new TreeNode(directoryInfo.Name);
            foreach (var directory in directoryInfo.GetDirectories())
            {
                try
                {
                    directoryNode.Nodes.Add(CreateDirectoryNode(directory));
                }
                catch (Exception)
                {
                }
            }
            foreach (var file in directoryInfo.GetFiles())
            {
                try
                {
                    directoryNode.Nodes.Add(new TreeNode(file.Name));
                }
                catch (Exception)
                {
                }
            }

            return directoryNode;
        }
    }
}
