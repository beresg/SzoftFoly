using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HFS.HttpServer
{
    class HttpRequest
    {
        public String Method { get { return method; } }
        public String Uri { get { return uri; } }
        public String Version { get { return version; } }
        public Dictionary<String, String> Headers { get { return header; } }
        public String Data { get { return data; } }
        public Dictionary<String, String> GETQuery { get { return getQuery; } }
        public Dictionary<String, String> POSTQuery { get { return postQuery; } }
        public List<File> Files { get { return files; } }
        public String BaseUrl { get { return baseUrl; } }
        public List<String> AcceptEncoding { get { return acceptEncoding; } }


        private String method;
        private String uri;
        private String version;
        private Dictionary<String, String> header;
        private String data;
        private Dictionary<String, String> getQuery;
        private Dictionary<String, String> postQuery;
        private List<File> files;
        private String baseUrl;
        private List<String> acceptEncoding;

        public class File
        {
            private String name;
            private String filename;
            private String content;
            private Dictionary<String, String> header;

            public String Name { get { return name; } }
            public String Filename { get { return filename; } }
            public String Content { get { return content; } }
            public Dictionary<String, String> Headers { get { return header; } }

            public File(String name, String filename, String content, Dictionary<String, String> header)
            {
                this.name = name;
                this.filename = filename;
                this.content = content;
                this.header = header;
            }
        }


        public HttpRequest(String method, String uri, String version, Dictionary<String, String> headers, String data)
        {
            files = new List<File>();
            getQuery = new Dictionary<string, string>();
            postQuery = new Dictionary<string, string>();


            this.method = method;
            this.uri = uri;
            this.version = version;
            this.header = headers;
            this.data = data;

            if (headers.ContainsKey("Accept-Encoding"))
                acceptEncoding = headers["Accept-Encoding"].Split(',').Select(x => x.Trim()).ToList();

            getQuery = new Dictionary<string, string>();

            Int32 pos = uri.IndexOf('?');
            if (pos != -1)
            {
                baseUrl = uri.Substring(0, pos);

                getQuery = ParseQuery(uri.Substring(pos + 1));
            }
            else
                baseUrl = uri;


            //Parsing POST request
            if (method == "POST" && data != null)
            {
                if (!headers.ContainsKey("Content-Type") || !headers["Content-Type"].Contains("multipart/form-data"))
                {
                    postQuery = ParseQuery(data);
                }
                else
                {
                    pos = headers["Content-Type"].IndexOf("boundary=");

                    if (pos != -1)
                    {
                        String boundary = "--" + headers["Content-Type"].Substring(pos + 9);

                        pos = boundary.Length+2;
                        String s;

                        Int32 next;

                        while ((next = data.IndexOf(boundary, pos)) != -1)
                        {
                            s = data.Substring(pos, next-pos);

                            Int32 pos2 = s.IndexOf("\r\n\r\n");
                            String headersString = s.Substring(0, pos2);
                            String value = s.Substring(pos2 + 4);

                            Regex regex = new Regex("(.*?):(.*)");
                            MatchCollection matches = regex.Matches(headersString);

                            if (matches.Count == 1 && matches[0].Groups[1].Value == "Content-Disposition")
                            {
                                Regex regex2 = new Regex("form-data; name=\"(.*?)\"");
                                Match match = regex2.Match(matches[0].Groups[2].Value);

                                if (match.Success)
                                {
                                    postQuery.Add(match.Groups[1].Value, value);
                                }
                            }
                            else
                            {
                                Dictionary<String, String> header = new Dictionary<string, string>();

                                Regex regex2 = new Regex("form-data; name=\"(.*?)\"; filename=\"(.*?)\"");
                                Match match = regex2.Match(matches[0].Groups[2].Value);

                                if (match.Success)
                                {
                                    for (Int32 i = 1; i < matches.Count; ++i)
                                    {
                                        header.Add(matches[i].Groups[1].Value, matches[i].Groups[2].Value);
                                    }

                                    files.Add(new File(match.Groups[1].Value, match.Groups[2].Value, value, header));
                                }


                                //Int32 matchLength = matches[matches.Count-1].Index+matches[matches.Count-1].Length-1;
                                //String value = s.Substring(matchLength + 4, s.Length - matchLength - 6);

                            }

                            pos = next + boundary.Length + 2;
                        }
                    }
                }
            }
        }

        private Dictionary<String, String> ParseQuery(String queryString)
        {
            Dictionary<String, String> query = new Dictionary<String, String>();

            foreach (String keyValue in queryString.Split(new char[] { '&' }))
            {
                Int32 eqPos = keyValue.IndexOf('=');
                if (eqPos != -1)
                {
                    String key = keyValue.Substring(0, eqPos);

                    if (!query.ContainsKey(key))
                        query.Add(key, keyValue.Substring(eqPos + 1));
                }
            }

            return query;
        }
    }
}
