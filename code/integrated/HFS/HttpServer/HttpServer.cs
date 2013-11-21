using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using SharpCompress.Writer.Zip;
using SharpCompress.Common;

namespace HFS.HttpServer
{
    public class File
    {
        public String ID { get; set; }
        public String FileName { get; set; }
        public String Path { get; set; }
        public List<String> Labels { get; set; }
        public DateTime Date { get; set; }
        public Int64 Size { get; set; }
    }

    class HttpServer
    {
        public enum Encoding { None, GZip, Deflate }

        public UInt16 Port
        {
            get
            {
                return port;
            }

            set
            {
                if (!running)
                    port = value;
            }
        }

        public Boolean Running { get { return running; } }
        public Encoding ResponseEncoding { get; set; }
        public String Root
        {
            get
            {
                return root;
            }

            set
            {
                if (Directory.Exists(value))
                {
                    root = value;
                }
            }
        }

        private TcpListener tcpListener;
        private Boolean running;
        private AutoResetEvent tcpClientConnected;
        private String root;
        private UInt16 port;

        public List<File> Files { get; set; }


        public HttpServer()
        {
            ResponseEncoding = HttpServer.Encoding.None;
            running = false;
            tcpClientConnected = new AutoResetEvent(false);
        }

        public void Start()
        {
            if (!running)
            {
                tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), Port);
                tcpListener.Start();

                running = true;

                while (running)
                {
                    tcpListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), tcpListener);

                    tcpClientConnected.WaitOne();
                }
            }
        }

        public void Stop()
        {
            if (running)
            {
                running = false;

                tcpListener.Stop();
            }
        }

        public void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            tcpClientConnected.Set();

            try
            {
                Process(listener.EndAcceptTcpClient(ar));
            }
            catch (Exception)
            {}
        }

        String ReadLine(NetworkStream ns)
        {
            StringBuilder sb = new StringBuilder();
            bool lastR = false;
            byte b;
            int i;
            bool end = false;

            while(!end)
            {
                i = ns.ReadByte();
                if (i == -1)
                    end = true;

                b = (byte)i;

                if (b == (byte)'\r')
                {
                    lastR = true;
                }
                else if (b == (byte)'\n' && lastR)
                {
                    end = true;
                }
                else
                {
                    if (lastR)
                    {
                        lastR = false;
                        sb.Append((byte)'\r');
                    }

                    sb.Append((char)b);
                }

            }

            return sb.ToString();
        }

        private void Process(object obj)
        {
            TcpClient client = obj as TcpClient;

            if (client != null)
            {
                NetworkStream ns = client.GetStream();

                String line = ReadLine(ns);

                if (line != null)
                {
                    Regex regex = new Regex("^(GET|POST) (.*?) HTTP\\/(.*)");
                    Match match = regex.Match(line);

                    if (match.Success)
                    {
                        String method = match.Groups[1].Value;
                        String path = match.Groups[2].Value;
                        String version = match.Groups[3].Value;
                        Dictionary<String, String> headers = new Dictionary<String, String>();
                        String data = String.Empty;

                        regex = new Regex("(.*?):(.*)");
                        while ((line = ReadLine(ns)) != String.Empty)
                        {
                            match = regex.Match(line);
                            if (match.Success && !headers.ContainsKey(match.Groups[1].Value))
                                headers.Add(match.Groups[1].Value, match.Groups[2].Value);
                        }

                        Int32 contentLength;
                        if (headers.ContainsKey("Content-Length") && Int32.TryParse(headers["Content-Length"], out contentLength))
                        {
                            byte[] buffer = new byte[contentLength];
                            int read = 0;

                            while(read != contentLength)
                                read += ns.Read(buffer, read, contentLength-read);

                            data = ByteArrayToString(buffer);
                        }
                        
                        HttpRequest request = new HttpRequest(method, path, version, headers, data);
                        HttpResponse response = ProcessRequest(request);

                        SendResponse(ns, response, request.AcceptEncoding);
                    }
                }
                else
                {
                    HttpResponse response = new HttpResponse();
                    response.StatusCode = 400;

                    SendResponse(ns, response, new List<String>());
                }
                ns.Close();

                client.Close();
            }
        }

        private HttpResponse ProcessRequest(HttpRequest request)
        {
            HttpResponse response = new HttpResponse();

            StreamWriter sw = new StreamWriter(response.Stream);

            switch (request.BaseUrl)
            {
                case "/":
                    if (System.IO.File.Exists(root + "index.html"))
                    {
                        using (StreamReader sr = new StreamReader(root + "index.html"))
                        {
                            response.Headers.Add("Content-Type", "text/html; charset=\"utf-8\"");

                            String fileContent = sr.ReadToEnd();
                            fileContent = fileContent.Replace("<div id=\"labels\"></div>", "<div id=\"labels\">" + LabelList("") + "</div>");
                            fileContent = fileContent.Replace("<div id=\"files\"></div>", "<div id=\"files\">" + FileList("") + "</div>");

                            sw.Write(fileContent);
                        }
                    }
                    else
                        response.StatusCode = 404;

                    break;
                case "/GetFileList":
                    response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                    sw.Write(FileListToJSON());
                    break;
                case "/PostTest":
                    if(request.POSTQuery!=null)
                    foreach (KeyValuePair<String, String> pair in request.POSTQuery)
                    {
                        sw.Write(pair.Key + " = " + pair.Value + "<br />");
                    }

                    if (request.Files!=null)
                        foreach (HFS.HttpServer.HttpRequest.File file in request.Files)
                        {
                            using (BinaryWriter bw = new BinaryWriter(System.IO.File.Open("upload/" + file.Filename, FileMode.Create)))
                            {
                                byte[] chars = StringToByteArray(file.Content);
                                bw.Write(chars);
                            }
                        }
                    break;
                default:
                    String path = request.BaseUrl.Substring(1);
                    if (System.IO.File.Exists(root + path))
                    {
                        FileInfo fi = new FileInfo(root + path);
                        DateTime lastWrite = fi.LastWriteTimeUtc;

                        response.Headers.Add("Date", DateTime.Now.ToUniversalTime().ToString("r"));
                        response.Headers.Add("Last-Modified", lastWrite.ToString("r"));

                        if (!request.Headers.ContainsKey("If-Modified-Since") ||
                            request.GETQuery.Count > 0 || DateTime.Parse(request.Headers["If-Modified-Since"]).ToUniversalTime() < lastWrite)
                        {
                            using (StreamReader sr = new StreamReader(root + path))
                            {
                                if (path.EndsWith(".js"))
                                {
                                    String fileContent = sr.ReadToEnd();

                                    foreach (KeyValuePair<String, String> kvp in request.GETQuery)
                                    {
                                        fileContent = fileContent.Replace("<?=" + kvp.Key + "?>", kvp.Value);
                                    }

                                    sw.Write(fileContent);
                                }
                                else
                                    sr.BaseStream.CopyTo(sw.BaseStream);
                            }
                        }
                        else
                        {
                            response.StatusCode = 304;
                        }
                    }
                    else
                    {
                        if (request.BaseUrl.StartsWith("/ajax/"))
                        {
                            response.Headers.Add("Content-Type", "text/html; charset=utf-8");

                            String selectedLabel = request.BaseUrl.Substring(6, request.BaseUrl.Length - 7);

                            sw.Write(LabelList(selectedLabel) + ";" + FileList(selectedLabel));
                        }
                        else
                            if (request.BaseUrl.StartsWith("/GetFiles/"))
                            {
                                String selectedLabel = request.BaseUrl.Substring(10, request.BaseUrl.Length - 10);

                                response.Headers.Add("Content-Type", "application/zip; charset=utf-8");
                                response.Headers.Add("Content-Disposition", "inline; filename=\"" + selectedLabel + ".zip\"");


                                MemoryStream ms = new MemoryStream();
                                {
                                    CompressionInfo ci = new CompressionInfo();
                                    ci.DeflateCompressionLevel = SharpCompress.Compressor.Deflate.CompressionLevel.Default;
                                    ci.Type = CompressionType.Deflate;

                                    using (ZipWriter zw = new ZipWriter(ms, ci, ""))
                                    {
                                        foreach (File file in Files.Where(x => x.Labels.Contains(selectedLabel)))
                                        {
                                            zw.Write(file.FileName, new FileStream(file.Path, FileMode.Open, FileAccess.Read), null);
                                        }
                                    }

                                    ms.Seek(0, SeekOrigin.Begin);
                                    response.Stream = ms;
                                }
                            }
                            else
                                if (request.BaseUrl.StartsWith("/GetFile/"))
                                {
                                    String id = request.BaseUrl.Substring(9, request.BaseUrl.Length - 9);
                                    File file = Files.Where(x => x.ID == id).FirstOrDefault();

                                    if (file != default(File) && System.IO.File.Exists(file.Path))
                                    {
                                        //response.Headers.Add("Content-Type", mime_type+"; charset=utf-8");
                                        response.Headers.Add("Content-Disposition", "inline; filename=\"" + file.FileName + "\"");
                                        response.Stream = System.IO.File.OpenRead(file.Path);
                                    }
                                    else
                                        response.StatusCode = 404;
                                }
                                else
                                {
                                    response.Headers.Add("Content-Type", "text/html; charset=\"utf-8\"");

                                    String selectedLabel = request.BaseUrl.Substring(1, request.BaseUrl.Length - 2);

                                    using (StreamReader sr = new StreamReader("frame.html"))
                                    {
                                        String fileContent = sr.ReadToEnd();
                                        fileContent = fileContent.Replace("<div id=\"labels\"></div>", "<div id=\"labels\">" + LabelList(selectedLabel) + "</div>");
                                        fileContent = fileContent.Replace("<div id=\"files\"></div>", "<div id=\"files\">" + FileList(selectedLabel) + "</div>");

                                        sw.Write(fileContent);
                                    }
                                }
                    }
                    break;
            }

            sw.Flush();

            return response;
        }

        private void SendResponse(NetworkStream ns, HttpResponse response, List<String> acceptEncoding)
        {
            StreamWriter sw = new StreamWriter(ns);

            sw.WriteLine("HTTP/" + response.Version + " " + response.StatusCode + " " + StatusCodeToMessage(response.StatusCode));

            foreach (KeyValuePair<String, String> header in response.Headers)
                sw.WriteLine(header.Key + ": " + header.Value);

            if (ResponseEncoding == Encoding.GZip && acceptEncoding.Contains("gzip"))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    response.Stream.Seek(0, SeekOrigin.Begin);
                    using(GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                        response.Stream.CopyTo(gZipStream);

                    byte[] compressed = stream.ToArray();

                    if (!response.Headers.ContainsKey("Content-Length") && compressed.Length > 0)
                        sw.WriteLine("Content-Length: " + compressed.Length);

                    sw.WriteLine("Content-Encoding: gzip");
                    sw.WriteLine();
                    sw.Flush();
                    ns.Write(compressed, 0, compressed.Length);
                }
            }
            else
                if (ResponseEncoding == Encoding.Deflate && acceptEncoding.Contains("deflate"))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        response.Stream.Seek(0, SeekOrigin.Begin);
                        using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Compress))
                            response.Stream.CopyTo(deflateStream);

                        byte[] compressed = stream.ToArray();

                        if (!response.Headers.ContainsKey("Content-Length") && compressed.Length > 0)
                            sw.WriteLine("Content-Length: " + compressed.Length);

                        sw.WriteLine("Content-Encoding: deflate");
                        sw.WriteLine();
                        sw.Flush();
                        ns.Write(compressed, 0, compressed.Length);
                    }
                }
                else
                {
                    response.Stream.Flush();

                    if (response.Stream.Length != 0)
                    {
                        if (!response.Headers.ContainsKey("Content-Length"))
                            sw.WriteLine("Content-Length: " + response.Stream.Length);

                        response.Stream.Seek(0, SeekOrigin.Begin);

                        sw.WriteLine();
                        sw.Flush();
                        response.Stream.CopyTo(ns);
                    }
                }

            sw.Close();
        }

        private String StatusCodeToMessage(Int32 statusCode)
        {
            switch (statusCode)
            {
                case 200:
                    return "OK";
                case 304:
                    return "Not Modified";
                case 400:
                    return "Bad Request";
                case 403:
                    return "Forbidden";
                case 404:
                    return "Not Found";
                case 500:
                    return "Internal Server Error";
            }

            return String.Empty;
        }


        private String LabelList(String parentName)
        {
            String s = String.Empty;
            List<String> labels = new List<string>();

            if (Files != null)
                foreach (List<String> label in Files.Select(x => x.Labels))
                    labels = labels.Union(label).ToList();

            foreach(String label in labels)
                {
                    
                    s += "<a href=\"javascript:void(0)\" onClick=\"loadLabel('" + label + "')\">" + label + "</a><br />";
                }

            return s;
        }

        private String FileList(String labelName)
        {
            String s = String.Empty;

            if (Files != null)
                foreach (File file in Files.Where(x => x.Labels.Contains(labelName)))
                {
                    s += "<a href=\"/GetFile/" + file.ID + "\">" + file.FileName + "</a><br />";
                }

            return s;
        }

        private String FileListToJSON()
        {
            StringBuilder json = new StringBuilder();

            json.Append("{\"id\": \"structure\",\"items\": [");

            foreach (File file in Files)
            {
                json.Append("{");
                json.Append("\"id\" : \"" + file.ID + "\",");
                json.Append("\"size\" : \"" + file.Size + "\",");
                json.Append("\"name\" : \"" + Path.GetFileName(file.Path) + "\",");
                json.Append("\"date\" : \"" + file.Date + "\",");
                json.Append("\"labels\" : [");
                foreach (String label in file.Labels)
                    json.Append("\"" + label + "\",");

                if (json[json.Length - 1] == ',')
                    json[json.Length - 1] = ']';
                else
                    json.Append(']');

                json.Append("},");

            }

            if (json[json.Length - 1] == ',')
                json[json.Length - 1] = ']';
            else
                json.Append(']');

            json.Append("}");

            return json.ToString();
        }

        public static String ByteArrayToString(Byte[] bytes)
        {
            if (bytes == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (Byte b in bytes)
                sb.Append((Char)b);

            return sb.ToString();
        }

        public static Byte[] StringToByteArray(String s)
        {
            Byte[] bs = new Byte[s.Length];
            Int32 i = 0;

            foreach (Char ch in s)
            {
                bs[i] = (Byte)ch;
                ++i;
            }

            return bs;
        }

        public void SendMessageToClients(string p)
        {
            throw new NotImplementedException();
        }

        public bool AllowFileUpload { get; set; }

        public int MaxClientNumber { get; set; }
    }
}
