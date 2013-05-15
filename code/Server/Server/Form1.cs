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

namespace HFS
{
    public partial class Form1 : Form
    {
        List<HFS.HttpServer.Label> labels;
        List<HFS.HttpServer.File> files;

        HttpServer.HttpServer server;

        public Form1()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            labels = new List<HFS.HttpServer.Label>();
            labels.Add(new HFS.HttpServer.Label { Name = "zene", Parent = "" });
            labels.Add(new HFS.HttpServer.Label { Name = "pop", Parent = "zene" });
            labels.Add(new HFS.HttpServer.Label { Name = "rock", Parent = "zene" });
            labels.Add(new HFS.HttpServer.Label { Name = "soundtrack", Parent = "zene" });

            files = new List<HFS.HttpServer.File>();
            files.Add(new HFS.HttpServer.File { Name = "pop1.mp3", Label = "pop", Labels = new List<String>() { "zene", "pop" } });
            files.Add(new HFS.HttpServer.File { Name = "pop2.mp3", Label = "pop", Labels = new List<String>() { "zene", "pop" } });
            files.Add(new HFS.HttpServer.File { Name = "pop3.mp3", Label = "pop", Labels = new List<String>() { "zene", "pop" } });
            files.Add(new HFS.HttpServer.File { Name = "rock1.mp3", Label = "rock", Labels = new List<String>() { "zene", "rock" } });
            files.Add(new HFS.HttpServer.File { Name = "rock2.mp3", Label = "rock", Labels = new List<String>() { "zene", "rock" } });
            files.Add(new HFS.HttpServer.File { Name = "zene1.mp3", Label = "soundtrack", Labels = new List<String>() { "zene", "soundtrack" }, ID = "1", Location = @"D:\ELTE\backup\2013tavasz\szoftverfolyamat2\SzoftFoly\code\Server\Server\bin\Debug\zene1.mp3" });
            files.Add(new HFS.HttpServer.File { Name = "zene2.mp3", Label = "soundtrack", Labels = new List<String>() { "zene", "soundtrack" }, ID = "2", Location = @"D:\ELTE\backup\2013tavasz\szoftverfolyamat2\SzoftFoly\code\Server\Server\bin\Debug\zene2.mp3" });
            files.Add(new HFS.HttpServer.File { Name = "zene3.mp3", Label = "soundtrack", Labels = new List<String>() { "zene", "soundtrack" }, ID = "3", Location = @"D:\ELTE\backup\2013tavasz\szoftverfolyamat2\SzoftFoly\code\Server\Server\bin\Debug\zene3.mp3" });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            server = new HttpServer.HttpServer();
            server.Port = 8888;
            server.Files = files;
            server.Labels = labels;
            server.ResponseEncoding = HttpServer.HttpServer.Encoding.None;
            server.Root = @"static\";

            Thread thread = new Thread(server.Start);
            thread.Start();
        }


    }
}
