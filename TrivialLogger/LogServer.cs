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
using System.Text.RegularExpressions;

namespace TrivialLogger
{
    public class LogServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _listenPort;
        private HttpListener _listener;
        private Hashtable _logTable;
        private string _LogRoot;

        public LogServer(int p, string root)
        {
            this._listenPort = p;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{p}/");
            _logTable = new Hashtable();
            _LogRoot = root;

        }

        public void Listen()
        {
            var fullPath = Path.GetFullPath(this._LogRoot);
            if(!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            _listener.Start();
            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                HttpListenerRequest req = context.Request;

                log.Info($"Received request for {req.Url}");
                string body;
                using (var stream = req.InputStream)
                {
                    var reader = new StreamReader(stream);
                    body = reader.ReadToEnd();
                    log.Info(body);
                }
                LogRequest request = JsonSerializer.Deserialize<LogRequest>(body);
                request.SourceHost = req.RemoteEndPoint.Address.ToString();
                log.Info($"{request.SourceHost} {request.LogPath}: {request.LogMessage}");
                try
                {
                    if(this.WriteRequest(request))
                    {
                        this._ResponseSuccess(context.Response);
                        continue;
                    }
                    this._ResponseSuccess(context.Response);
                }
                catch(Exception e)
                {
                    log.Error(e.Message);
                    this._ResponseFailure(context.Response);
                }
            }
        }

        public void Cleanup()
        {
            log.Info("Cleaning up !");
            foreach(var key in this._logTable.Keys)
            {
                var entry = ((LogEntry)this._logTable[key]);
                log.Info($"Closing {entry.LogFilePath}");
                entry.Dispose(); ;
            }
        }

        public bool WriteRequest(LogRequest request)
        {
            var FileName = request.LogPath.Split('\\').LastOrDefault();
            if (!this._IsLegalFileName(FileName))
            {
                log.Error($"Illegal filename: {FileName}");
                return false;
            }
            var paths = new String[]
            {
                this._LogRoot,
                request.SourceHost.Replace(':', '_'),
                FileName
            };
            var folder = Path.GetFullPath(Path.Combine(this._LogRoot, request.SourceHost.Replace(':', '_')));
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fullPath = Path.GetFullPath(Path.Combine(paths));

            if (!this._logTable.ContainsKey(fullPath))
            {
                this._logTable.Add(fullPath, new LogEntry(fullPath));
            }    
            var entry = (LogEntry)this._logTable[fullPath];
            string timestamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            entry.Write($"{timestamp}: {request.LogMessage}");
            return true;
        }

        private bool _IsLegalFileName(string filename)
        {
            Regex pattern = new Regex("[<>:\"/|?*;,\t\r ]");
            if(pattern.Match(filename).Success)
            {
                return false;
            }
            return true;
        }

        private void _Respond(HttpListenerResponse responseObject, string message)
        {
            responseObject.Headers.Set("Content-Type", "text/plain");

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            responseObject.ContentLength64 = buffer.Length;

            Stream ros = responseObject.OutputStream;
            ros.Write(buffer, 0, buffer.Length);
        }

        private void _ResponseSuccess(HttpListenerResponse responseObject)
        {
            _Respond(responseObject, "Success");
        }

        private void _ResponseFailure(HttpListenerResponse responseObject)
        {
            _Respond(responseObject, "Failure");
        }
    }
}
