namespace SBrickey
{
    using System;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;

    // TODO : move the actual service implementation into its own file
    public class WinService : myServiceBase { }
    abstract public class myServiceBase : ServiceBase
    {
        // expose the OnStart method
        public void Start(string[] args) { this.OnStart(args); }
    }

    // TODO : adjust any argument parsing/etc
    class MainEntry
    {
        public static void Main(string[] args)
        {
            // delegates for making various arg based determinations
            Func<string[], bool> argFnHelp = p => p.Contains("/?");
            // debug parameter for REQUESTING a debug session from a non-development environment (remote debugger, etc)
            Func<string[], bool> argFnDebug = p => p.Contains("/d") || p.Contains("/debug");
            Func<string[], bool> argFnInteractive = p => p.Contains("/i") || p.Contains("/interactive");

            // execute delegates
            var arg_Help = argFnHelp(args);
            var arg_Debug = argFnDebug(args);
            var arg_Interactive = argFnInteractive(args);

            if (arg_Help)
            {
                DisplayHelp();
                return;
            }

            if (arg_Debug)
                DebugMode.Attach();

            // interactive mode if it was requested, or the debugger is attached
            if (arg_Interactive || System.Diagnostics.Debugger.IsAttached)
                InteractiveMode.Main(args);
            else
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                ServiceBase[] ServicesToRun = new ServiceBase[] { new WinService() };
                ServiceBase.Run(ServicesToRun);
            }
        }
        private static class DebugMode
        {
            public static void Attach()
            {
                // If a debugger is not attached, attempt to launch a registered debugger.
                if (!System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Launch();

                // in the event that a debugger isn't registered (thus the previous attempt to Launch will do nothing), wait for one to attach (remote debugger)
                if (!System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("Waiting for debugger to attach");

                    while (!System.Diagnostics.Debugger.IsAttached)
                        Thread.Sleep(100);
                }
            }
        }
        private static class InteractiveMode
        {
            // ManualResetEvents to signal among threads when the service has started, stop was requested, and when it was stopped
            private readonly static ManualResetEventSlim mutexSvcStarted = new ManualResetEventSlim(initialState: false);
            private readonly static ManualResetEventSlim mutexSvcStopReq = new ManualResetEventSlim(initialState: false);
            private readonly static ManualResetEventSlim mutexSvcStopped = new ManualResetEventSlim(initialState: false);

            // threads
            private readonly static Thread Thread_Svc = new Thread(new ThreadStart(Thread_Svc_Execute));
            private readonly static Thread Thread_UI = new Thread(new ThreadStart(Thread_UI_Execute));

            private static string[] Main_Args;
            public static void Main(string[] args)
            {
                // share the args with the rest of the class
                Main_Args = args;

                // register a console control handler, if the app is forcefully closed (CTRL+C, Close btn, Logoff, shutdown, etc)
                ConsoleExtensions.ControlHandler = Console_ControlHandler;

                // start the background service thread
                Thread_Svc.Start();

                // wait for the service to finish starting
                mutexSvcStarted.Wait();

                // start the UI thread (listens for the user to press ESC)
                Thread_UI.Start();

                // assume that there are other more relevant projects in the system, and get this window out of the way
                ConsoleExtensions.Minimize();

                // wait for the service to stop, whether caused by the UI thread, or console control handler
                mutexSvcStopped.Wait();
            }

            private static bool Console_ControlHandler(ConsoleExtensions.CtrlType sig)
            {
                // request the service to be stopped
                mutexSvcStopReq.Set();

                // try to wait until the service has stopped (Windows will kill the process after 10s)
                mutexSvcStopped.Wait();

                // as of Win7, Windows will close the process when true is returned (or after the 10s timeout).
                // ref: http://stackoverflow.com/a/5610042/961902
                return true;
            }

            private static void Thread_Svc_Execute()
            {
                // start
                var service = new WinService();
                service.Start(Main_Args);
                mutexSvcStarted.Set(); // signal to the other thread that the service has finished its startup

                // wait for external request to stop the service
                mutexSvcStopReq.Wait();

                // stop
                Console.WriteLine("Shutting down...");
                service.Stop();
                mutexSvcStopped.Set();
            }
            private static void Thread_UI_Execute()
            {
                Console.WriteLine("Press ESC to stop");
                ConsoleExtensions.WaitFor(ConsoleKey.Escape);
                mutexSvcStopReq.Set();
            }
        }

        // TODO: implement
        private static void DisplayHelp() { throw new NotImplementedException(); }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) { /* implement global error logging/handling */ }

    } // class MainEntry

    public static class ConsoleExtensions
    {
        public static void WaitFor(ConsoleKey key)
        {
            bool keyPressed = false;
            do
            {
                // KeyAvailability provides notification that ReadKey has a value, which makes this nonblocking
                if (Console.KeyAvailable)
                    keyPressed = Console.ReadKey(intercept: true).Key == key;

                Thread.Sleep(100); // wait .1s
            } while (!keyPressed);
        }


        #region Control Handler

        public enum CtrlType
        {
            CTRL_C_EVENT        = 0,
            CTRL_BREAK_EVENT    = 1,
            CTRL_CLOSE_EVENT    = 2,
            CTRL_LOGOFF_EVENT   = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        public delegate bool ControlHandlerDelegate(CtrlType sig);

        private static ControlHandlerDelegate _ControlHandler;
        public static ControlHandlerDelegate ControlHandler
        {
            get { return _ControlHandler; }
            set { _ControlHandler = value; DelegateHandler_Register(); }
        }

        private static bool _DelegateWrapper(CtrlType sig) { return ControlHandler?.Invoke(sig) ?? true; }
        private static ControlHandlerDelegate _DelegateWrapperDelegate = new ControlHandlerDelegate(_DelegateWrapper);
        private static bool _DelegateWrapperIsRegistered;
        private static void DelegateHandler_Register()
        {
            // only need to register once
            if (_DelegateWrapperIsRegistered)
                return;

            // call the interop API, then indicate that this has been done
            SetConsoleCtrlHandler(_DelegateWrapperDelegate, true);
            _DelegateWrapperIsRegistered = true;
        }

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ControlHandlerDelegate handler, bool add);

        #endregion


        #region Minimize

        const Int32 SW_MINIMIZE = 6;

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall, SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("User32.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool ShowWindow([System.Runtime.InteropServices.In] IntPtr hWnd, [System.Runtime.InteropServices.In] Int32 nCmdShow);

        public static void Minimize()
        {
            IntPtr hWndConsole = GetConsoleWindow();
            ShowWindow(hWndConsole, SW_MINIMIZE);
        }

        #endregion

    }
} // namespace