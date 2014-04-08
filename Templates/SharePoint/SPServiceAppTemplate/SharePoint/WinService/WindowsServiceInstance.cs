using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;

namespace SBrickey.SPServiceAppTemplate.SharePoint
{
    public class WindowsServiceInstance : SPWindowsServiceInstance
    {
        // default constructor required by SPPersistedObject
        public WindowsServiceInstance() : base() { }
        // private ctor - used by FACTORY
        private WindowsServiceInstance(string name, SPServer server, WindowsService service) : base(name, server, service) { }

        public static WindowsServiceInstance FACTORY_Get(string name = "", SPServer server = null)
        {
            if (server == null)
                return null;

            var WinSvc = WindowsService.FACTORY_Get(name);

            return WinSvc == null
                 ? null
                 : WinSvc.Instances
                         .Where(i => i.Server == server)
                         .DefaultIfEmpty(null)
                         .Cast<WindowsServiceInstance>()
                         .SingleOrDefault();
        }
        public static WindowsServiceInstance FACTORY_Ensure(string name = "", SPServer server = null)
        {
            if (server == null)
                throw new ArgumentException("server");

            var outval = FACTORY_Get(name: name, server: server);

            if (outval == null)
            {
                var svc = WindowsService.FACTORY_Get();
                outval = new WindowsServiceInstance(
                    name: name,
                    server: server,
                    service: svc
                );

                // persist
                outval.Update(true);
            }

            return outval;
        }

        /// <remarks>
        /// As shown in Central Admin | Security | Configure Service Accounts
        /// (overrides the WindowsService.TypeName implementation)
        /// </remarks>
        public override string TypeName { get { return Common.SharePoint.SPWindowsServiceInstance.TypeName; } }
        public override string DisplayName { get { return Common.SharePoint.SPWindowsServiceInstance.DisplayName; } }
        public override string Description { get { return Common.SharePoint.SPWindowsServiceInstance.Description; } }

    } // class
} // namespace
