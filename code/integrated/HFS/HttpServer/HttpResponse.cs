using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HFS.HttpServer
{
    class HttpResponse
    {
        public String Version { get; set; }
        public Int32 StatusCode { get; set; }
        public Dictionary<String, String> Headers { get; set; }
        //public String Data { get; set; }
        public Stream Stream { get; set; }

        public HttpResponse()
        {
            Version = "1.1";
            StatusCode = 200;
            Headers = new Dictionary<String, String>();
            //Data = String.Empty;
            Stream = new MemoryStream();
        }
    }
}
