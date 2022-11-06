using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrivialLogger
{
    public class LogRequest
    {
        public LogRequest() { }
        public string LogPath
        {
            get;
            set;
        }
        public string LogMessage
        {
            get;
            set;
        }

        public string SourceHost
        {
            get;
            set;
        }
    }
}
