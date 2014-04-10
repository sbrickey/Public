using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // circuit breaker condition
            bool PrimaryExecution_Outage = false;
            bool PrimaryExecution_LongDelay = false;

            // Configure circuit breaker
            CircuitBreaker.Core.CircuitBreaker<String> cb = new CircuitBreaker.Core.CircuitBreaker<String>();
            cb.PrimaryExecution = () => {
                // outage
                if (PrimaryExecution_Outage)
                    throw new Exception("forced down");

                // delay logic
                System.Threading.Thread.Sleep(PrimaryExecution_LongDelay ? 1500 : 20);

                return ".";
            };
            cb.FailoverExecution = () => "!";

            // query and output results
            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
            {
                Console.Write(cb.Execute());
            };
            t.Interval = 50;
            t.Enabled = true;


            Console.WriteLine("Circuit Breaker");
            Console.WriteLine("    .  will be shown for primary implementation");
            Console.WriteLine("    !  will be shown for failover implementation");
            Console.WriteLine("");
            Console.WriteLine("   'o' to switch outage status");
            Console.WriteLine("   'd' to switch delay status");
            Console.WriteLine("   'q' to quit");
            ConsoleKeyInfo keyPressed = default(ConsoleKeyInfo);
            do
            {
                keyPressed = Console.ReadKey();

                if (keyPressed.Key == ConsoleKey.O)
                    PrimaryExecution_Outage = !PrimaryExecution_Outage;

                if (keyPressed.Key == ConsoleKey.D)
                    PrimaryExecution_LongDelay = !PrimaryExecution_LongDelay;

            } while (keyPressed.Key != ConsoleKey.Q);

        } // static void Main(...)

    }
}
