using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrivialLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new LogServer(8000);
            server.Listen();
        }
    }
}
