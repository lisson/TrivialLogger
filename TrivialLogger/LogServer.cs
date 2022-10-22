using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrivialLogger
{
    public class LogServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _listenPort;
        private HttpListener _listener;
        private Hashtable _logTable;

        public LogServer(int p)
        {
            this._listenPort = p;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{p}/");
            _logTable = new Hashtable();
        }

        public void Listen()
        {
            _listener.Start();
            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                HttpListenerRequest req = context.Request;

                log.Info($"Received request for {req.Url}");
                using (var stream = req.InputStream)
                {
                    var reader = new StreamReader(stream);
                    var body = reader.ReadToEnd();
                    log.Info(body);
                    LogRequest request = JsonSerializer.Deserialize<LogRequest>(body);
                    log.Info($"{request.LogPath}: {request.LogMessage}");
                    this.WriteRequest(request);
                }

                HttpListenerResponse resp = context.Response;
                resp.Headers.Set("Content-Type", "text/plain");

                string data = "Hello there!";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                resp.ContentLength64 = buffer.Length;

                Stream ros = resp.OutputStream;
                ros.Write(buffer, 0, buffer.Length);
            }
        }

        public bool WriteRequest(LogRequest request)
        {
            var fullPath = Path.GetFullPath(request.LogPath);
            if (!this._logTable.ContainsKey(fullPath))
            {
                this._logTable.Add(fullPath, new LogEntry(fullPath));
            }    
            var entry = (LogEntry)this._logTable[fullPath];
            entry.Write(request.LogMessage);
            return true;
        }


    }
}
