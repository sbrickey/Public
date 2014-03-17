using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmtpSimulator.Core;

namespace SmtpSimulator
{
    class SMTPServerHost
    {
        private static bool _simulatorRunning = false;

        static void Main(string[] args)
        {
            var server = new Core.SmtpSimulator(System.Net.IPAddress.Any, 25);
            server.Started += new Core.SmtpSimulator.StartedEventHandler(() =>
            {
                Console.WriteLine("SMTP Simulator started");
                Console.WriteLine("Press ESC to stop");

                _simulatorRunning = true;
            });
            server.Stopped += new Core.SmtpSimulator.StoppedEventHandler(() =>
            {
                _simulatorRunning = false;

                Console.WriteLine("SMTP Simulator stopped");
            });

            server.Start();

            while (_simulatorRunning)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                        server.Stop();
                }
                System.Threading.Thread.Sleep(0);
            }
            
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

    } // class SMTPServerHost
} // namespace
