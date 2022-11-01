using System;
using System.IO;


namespace TrivialLogger
{
    public class LogEntry : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private FileStream _fileHandle;
        private StreamWriter _sWriter;

        public LogEntry()
        {
        }
        public LogEntry(string path)
        {
            this.LogFilePath = path;
        }

        public string LogFilePath
        {
            get;
            set;
        }

        public DateTime LastUpdated
        {
            get;
            private set;
        }

        public virtual void Dispose()
        {
            if (_fileHandle != null)
            {
                _fileHandle.Close();
                _fileHandle.Dispose();
            }
            if(_sWriter != null)
            {
                _sWriter.Close();
                _sWriter.Dispose();
            }
        }

        public bool OpenFile()
        {
            log.Info($"Opening entry {this.LogFilePath}");
            try
            {
                this._fileHandle = new FileStream(this.LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                this._sWriter = new StreamWriter(_fileHandle);
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
            return false;
        }

        public bool Write(string logEntry)
        {
            try
            {
                if(this._fileHandle == null)
                {
                    this.OpenFile();
                }
                log.Debug($"Writing and flushing {logEntry}");
                this._sWriter.WriteLine(logEntry);
                this._sWriter.Flush();
                this.LastUpdated = DateTime.Now;
                return true;
            }
            catch(Exception e)
            {
                log.Error(e.Message);
            }
            return false;
        }
    }
}
