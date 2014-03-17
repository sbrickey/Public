using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitBreaker.Core
{
    public interface iQuery<TResult>
    {
        TResult Execute();
    }

    public class CircuitBreaker<TResult> : iQuery<TResult>
    {
        public class ExecutionLog
        {
            public DateTime StartTime { get; set; }
            public TimeSpan Duration { get; set; }

            public enum Responses { Success, Failure }
            public Responses Response { get; set; }
        }

        private bool PrimaryExecution_isDown = false;
        public Func<TResult> PrimaryExecution { get; set; }
        public Func<TResult> FailoverExecution { get; set; }
        private bool ExecuteConcurrently = false;

        private System.Collections.Concurrent.ConcurrentBag<ExecutionLog> ExecutionLogs =
            new System.Collections.Concurrent.ConcurrentBag<ExecutionLog>();

        private System.Timers.Timer ExecutionMonitor = new System.Timers.Timer();
        private void ExecutionMonitor_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var RecentLogs = ExecutionLogs.Where(l => l.StartTime > DateTime.Now.Subtract(TimeSpan.FromSeconds(5)));
            var Total = RecentLogs.Count();
            var Successful = RecentLogs.Where(l => l.Response == ExecutionLog.Responses.Success).Count();

            PrimaryExecution_isDown = Successful / Total < .8;
        }

        private System.Random Rand = new Random();

        // Constructor(s)
        public CircuitBreaker()
        {
            ExecutionMonitor.Elapsed += new System.Timers.ElapsedEventHandler(ExecutionMonitor_Elapsed);
            ExecutionMonitor.Interval = 200;
            ExecutionMonitor.Enabled = true;
        }





        public TResult Execute()
        {
            TResult outval = default(TResult);

            // if isDown, and not part of the 20% tests (to detect repairs)... just run failover
            if (PrimaryExecution_isDown && (Rand.Next(10) > 1))
            {
                outval = FailoverExecution.Invoke();
            }
            else
            {
                ExecutionLog tLog = new ExecutionLog() { StartTime = DateTime.Now };
                ExecutionLogs.Add(tLog);

                try
                {

                    outval = PrimaryExecution.Invoke();
                    tLog.Duration = DateTime.Now.Subtract(tLog.StartTime);
                    tLog.Response = ExecutionLog.Responses.Success;
                }
                catch
                {
                    tLog.Duration = DateTime.Now.Subtract(tLog.StartTime);
                    tLog.Response = ExecutionLog.Responses.Failure;
                    outval = FailoverExecution.Invoke();
                }
            }
            return outval;
        }
    }
}
