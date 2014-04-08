using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;

namespace SBrickey.SPServiceAppTemplate.SharePoint
{
    public class WindowsService : Microsoft.SharePoint.Administration.SPWindowsService
    {
        // default constructor required by SPPersistedObject
        public WindowsService() : base() { }
        // private ctor - used by FACTORY
        private WindowsService(string name, Microsoft.SharePoint.Administration.SPFarm farm) : base(name, farm) { }

        public static WindowsService FACTORY_Get(string name = "")
        {
            // if no name is specified (default behavior), use the default service name
            var ServiceName = String.IsNullOrEmpty(name)
                            ? Common.WinService.ServiceName
                            : name;

            var outval = SPFarm.Local
                               .Services
                               .GetValue<WindowsService>(ServiceName);

            return outval;
        }
        public static WindowsService FACTORY_Ensure(string name = "")
        {
            var outval = FACTORY_Get(name);

            if (outval == null)
            {
                // if no name is specified (default behavior), use the default service name
                var ServiceName = String.IsNullOrEmpty(name)
                                ? Common.WinService.ServiceName
                                : name;

                // register with SharePoint services
                outval = new SharePoint.WindowsService(
                    name: ServiceName,
                    farm: SPFarm.Local
                );

                // update/save
                outval.Update(true);
            }

            return outval;
        }

        /// <summary>
        /// Override the TypeName (default is the fully qualified class name)
        /// </summary>
        /// <remarks>
        /// name of the service as it appears in the Service list at
        ///   Central Administration | System Settings | Manage servers in this farm
        /// and
        ///   Central Administration | System Settings | Manage services on server
        /// and
        ///   Central Administration | Security | Configure Service Accounts
        /// </remarks>
        public override string TypeName { get { return Common.SharePoint.SPWindowsService.TypeName; } }
    } // class
} // namespace
