using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SBrickey.SPServiceAppTemplate.WinService
{
    /// <summary>
    /// Provides the installation and uninstallation of the Windows Service by inheriting from the
    /// System.Configuration.Install.Installer class.
    /// </summary>
    [RunInstaller(true)]
    public class WinServiceInstaller : Installer
    {

        public WinServiceInstaller()
        {
            Installers.Add(new System.ServiceProcess.ServiceProcessInstaller()
            {
                Account = ServiceAccount.LocalSystem
            });
            Installers.Add(new System.ServiceProcess.ServiceInstaller()
            {
                ServiceName = Common.WinService.ServiceName,
                DisplayName = Common.WinService.DisplayName,
                Description = Common.WinService.ServiceDescription,
                StartType = ServiceStartMode.Disabled
            });
        } // default ctor


        public static void _Install()
        {
            ManagedInstallerClass.InstallHelper(new string[] { Common.Paths.WinServiceExe.Path });
        }

        public static void _Uninstall()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", Common.Paths.WinServiceExe.Path });
            }
            catch (System.Configuration.Install.InstallException ex)
            {
                var win32ex = ex.InnerException as System.ComponentModel.Win32Exception;
                if (win32ex != null &&
                    win32ex.ErrorCode == -2147467259 &&
                    win32ex.NativeErrorCode == 1060 &&
                    win32ex.Message == "The specified service does not exist as an installed service")
                    return;

                throw;
            }
        }


    } // namespace
} // class
