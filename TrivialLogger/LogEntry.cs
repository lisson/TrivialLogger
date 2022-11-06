﻿using System;
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
            if (_sWriter != null)
            {
                _sWriter.Close();
            }
            if (_fileHandle != null)
            {
                _fileHandle.Close();
            }
        }

        public bool OpenFile()
        {
            log.Info($"Opening entry {this.LogFilePath}");
            try
            {
                this._fileHandle = new FileStream(this.LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                this._sWriter = new StreamWriter(_fileHandle);
                long endPoint = this._fileHandle.Length;
                this._fileHandle.Seek(endPoint, SeekOrigin.Begin);
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
