using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HFS.HttpServer
{
    class HttpRequest
    {
        public String Method { get { return method; } }
        public String Uri { get { return uri; } }
        public String Version { get { return version; } }
        public Dictionary<String, String> Headers { get { return headers; } }
        public String Data { get { return data; } }
        public Dictionary<String, String> GETQuery { get { return getQuery; } }
        public String BaseUrl { get { return baseUrl; } }
        public List<String> AcceptEncoding { get { return acceptEncoding; } }


        private String method;
        private String uri;
        private String version;
        private Dictionary<String, String> headers;
        private String data;
        private Dictionary<String, String> getQuery;
        private String baseUrl;
        private List<String> acceptEncoding;


        public HttpRequest(String method, String uri, String version, Dictionary<String, String> headers, String data)
        {
            this.method = method;
            this.uri = uri;
            this.version = version;
            this.headers = headers;
            this.data = data;

            if (headers.ContainsKey("Accept-Encoding"))
                acceptEncoding = headers["Accept-Encoding"].Split(',').Select(x => x.Trim()).ToList();

            getQuery = new Dictionary<string, string>();

            Int32 pos = uri.IndexOf('?');
            if (pos != -1)
            {
                baseUrl = uri.Substring(0, pos);

                foreach (String keyValue in uri.Substring(pos + 1).Split(new char[] { '&' }))
                {
                    Int32 eqPos = keyValue.IndexOf('=');
                    if (eqPos != -1)
                    {
                        String key = keyValue.Substring(0, eqPos);

                        if (!getQuery.ContainsKey(key))
                            getQuery.Add(key, keyValue.Substring(eqPos + 1));
                    }
                }
            }
            else
                baseUrl = uri;
        }
    }
}
