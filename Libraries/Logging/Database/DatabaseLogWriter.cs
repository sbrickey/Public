namespace SBrickey.Libraries.Logging.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class DatabaseLogWriter : IDatabaseLogWriter<int>
    {

        public int WriteScope(DatabaseWriterLogScope<int> scope)
        {
            return WriteImpl(
                timeStamp: scope.TimeStamp,
                computername: scope.ComputerName,
                username: scope.UserName,
                processName: scope.ProcessName,
                processID: scope.ProcessId,
                threadID: scope.ThreadId,

                /* this causes recursive persistence of scope IDs */
                parentID: scope.Parent?.PersistedId.Value,

                logLevel: LogLevel.Trace,
                category: scope.Category,
                eventId: 0,
                Message: String.Format("Enter scope : {0}", scope),
                exception: null
            );
        }

        public void Write(LogEntry entry)
        {
            var scope = DatabaseWriterLogScope<int>.Current;
            Task.Factory.StartNew(() =>
            {
                this.WriteImpl(
                    timeStamp: entry.TimeStamp,
                    computername: entry.ComputerName,
                    username: entry.UserName,
                    processName: entry.ProcessName,
                    processID: entry.ProcessId,
                    threadID: entry.ThreadId,

                    /* this causes recursive persistence of scope IDs */
                    parentID: scope?.PersistedId.Value,

                    category: entry.Category,
                    eventId: entry.EventId,
                    logLevel: entry.Level,
                    Message: entry.Message,
                    exception: entry.Exception
                );
            });
        }
        

        private int WriteImpl( DateTimeOffset timeStamp, string computername, string username, string processName, int processID, int threadID
                             , int? parentID, string category, EventId eventId, LogLevel logLevel, string Message, string exception )
        {

            using (var tConn = new SqlConnection(.ConnectionString))
            {
                using (var tCmd = new SqlCommand(cmdText: "[Log].[Log_Insert]", connection: tConn))
                {
                    tCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    
                    tCmd.Parameters.AddWithValue("@Parent_LogID", (object)parentID ?? DBNull.Value);

                    tCmd.Parameters.AddWithValue("@TimeStamp", timeStamp);
                    tCmd.Parameters.AddWithValue("@MachineName", computername);
                    tCmd.Parameters.AddWithValue("@UserName", username);
                    tCmd.Parameters.AddWithValue("@ProcessName", processName);
                    tCmd.Parameters.AddWithValue("@ProcessID", processID);
                    tCmd.Parameters.AddWithValue("@ThreadID", threadID);

                    tCmd.Parameters.AddWithValue("@Category", category);
                    tCmd.Parameters.AddWithValue("@EventId", eventId.Id);
                    tCmd.Parameters.AddWithValue("@Level", logLevel.ToString());
                    tCmd.Parameters.AddWithValue("@Message", Message);
                    tCmd.Parameters.AddWithValue("@Exception", exception);

                    tConn.Open();
                    var id = tCmd.ExecuteScalar();

                    return (int)id;
                } // using tCmd
            } // using tConn

        }
    } // class
} // namespace