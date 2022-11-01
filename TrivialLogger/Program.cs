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
        static void Main(string[] args)
        {
            int port = 8000;
            var server = new LogServer(port, ConfigurationManager.AppSettings["LogRoot"]);
            Console.Out.WriteLine($"Listening on port {port}");
            Console.Out.WriteLine(ConfigurationManager.AppSettings["LogRoot"]);
            server.Listen();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("I'm out of here");
        }
    }
}
