using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.SPServiceAppTemplate.Common
{
    public class WinService
    {
        public const string ServiceName = "WinService";
        public const string DisplayName = "Windows Service";
        public const string ServiceDescription = "My Custom Windows Service";


        public struct ProcessExecutionResults
        {
            public string ConsoleOutput { get; set; }
            public int ExitCode { get; set; }
        }

        public static ProcessExecutionResults Exec_Install()
        {
            // declare and initialize the installation process
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = Paths.WinServiceExe.Path,
                    Arguments = "/i",

                    // Required for RedirectStandardOutput
                    LoadUserProfile = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            // capture the console output
            var proc_Output = new System.Text.StringBuilder();
            process.OutputDataReceived += (sender, outputLine) => { proc_Output.Append(outputLine.Data); };

            // start the process, wait for completion
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            var Results = new ProcessExecutionResults()
            {
                ConsoleOutput = proc_Output.ToString(),
                ExitCode = process.ExitCode
            };
            return Results;
        } // Exec_Install()

        public static ProcessExecutionResults Exec_Uninstall()
        {
            // declare and initialize the installation process
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = Paths.WinServiceExe.Path,
                    Arguments = "/u",

                    // Required for RedirectStandardOutput
                    LoadUserProfile = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            // capture the console output
            var proc_Output = new System.Text.StringBuilder();
            process.OutputDataReceived += (sender, outputLine) => { proc_Output.Append(outputLine.Data); };

            // start the process, wait for completion
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            var Results = new ProcessExecutionResults()
            {
                ConsoleOutput = proc_Output.ToString(),
                ExitCode = process.ExitCode
            };
            return Results;
        } // void Exec_Uninstall()

    } // class
} // namespace
