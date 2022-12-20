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
using CommunityToolkit.Mvvm.ComponentModel;

namespace TrivialLogger
{
    public class LogServer : ObservableRecipient
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _listenPort;
        private HttpListener _listener;
        private Hashtable _logTable;
        private LogRequestQueue m_queue;
        private string _LogRoot;

        public LogServer(int p, string root)
        {
            this._listenPort = p;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{p}/");
            _logTable = new Hashtable();
            m_queue = new LogRequestQueue();
            _LogRoot = root;

        }

        public void Listen()
        {
            var fullPath = Path.GetFullPath(this._LogRoot);
            if(!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            m_queue.PropertyChanged += M_queue_PropertyChanged;
            StatsTracker.Collect();
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
                request.responseObject = context.Response;
                log.Info($"{request.SourceHost} {request.LogPath}: {request.LogMessage}");
                m_queue.Enqueue(request);
            }
        }

        private void M_queue_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            log.Info($"Signal Name: {e.PropertyName}");
            switch (e.PropertyName)
            {
                case "RequestQueue":
                    log.Info($"Queue changed. Queue size: {m_queue.RequestQueue.Count}");
                    break;
                default:
                    break;
            }
            WriteRequest();
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

        public async Task<bool> WriteRequest()
        {
            DateTime startTime = DateTime.Now;
            var request = m_queue.Dequeue();
            if(request == null)
            {
                return true;
            }
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
            _ResponseSuccess(request.responseObject);
            var delta = DateTime.Now - startTime;
            StatsTracker.IncrementRequestsServiced();
            StatsTracker.AddServiceTime(delta.Milliseconds);
            log.Info($"Request serviced in {delta.TotalMilliseconds} ms");
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
            ros.Close();
        }

        private void _ResponseSuccess(HttpListenerResponse responseObject)
        {
            responseObject.StatusCode = (int)HttpStatusCode.OK;
            responseObject.StatusDescription = "OK";
            _Respond(responseObject, "Success");
        }

        private void _ResponseFailure(HttpListenerResponse responseObject)
        {
            responseObject.StatusCode = (int)HttpStatusCode.InternalServerError;
            _Respond(responseObject, "Failure");
        }
    }
}
