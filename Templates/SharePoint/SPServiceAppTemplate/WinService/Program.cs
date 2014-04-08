using System.Linq;
using System.ServiceProcess;

namespace SBrickey.SPServiceAppTemplate.WinService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the EXE.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Contains("/i"))
            {
                Install();
            }
            else if (args.Contains("/u"))
            {
                Uninstall();
            }
            else
            {
                Main_RunService();
            }
        } // Main(...)

        private static void Main_RunService()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new WinServiceApp() 
            };
            ServiceBase.Run(ServicesToRun);
        } // Main_RunService()


        private static void Install()
        {
            WinService.WinServiceInstaller._Install();
        } // Install()

        private static void Uninstall()
        {
            WinService.WinServiceInstaller._Uninstall();
        } // Uninstall()

    } // class
} // namespace
