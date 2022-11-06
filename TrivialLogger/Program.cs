using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrivialLogger
{
    class Program
    {
        static LogServer server;
        static void Main(string[] args)
        {
            int port = 8000;
            server = new LogServer(port, ConfigurationManager.AppSettings["LogRoot"]);
            Console.Out.WriteLine($"Listening on port {port}");
            Console.Out.WriteLine(ConfigurationManager.AppSettings["LogRoot"]);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            server.Listen();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            server.Cleanup();
        }
    }
}
