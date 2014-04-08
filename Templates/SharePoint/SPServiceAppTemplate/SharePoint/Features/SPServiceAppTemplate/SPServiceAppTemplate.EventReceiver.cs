using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SBrickey.SPServiceAppTemplate.SharePoint.Features.SPServiceAppTemplate
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("a5d0a91c-1f19-44ec-8ea7-951bd9be8ee7")]
    public class WinServiceEventReceiver : SPFeatureReceiver
    {
        #region Event Receiver - Install / Uninstall

        // Uncomment the method below to handle the event raised after a feature has been installed.
        public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        {
            //SP_LoggingService_Add();
            WinService_Add();
            SP_WinService_Add(); // Windows Service
            SP_WebService_Add(); // Service App Framework
        }


        // Uncomment the method below to handle the event raised before a feature is uninstalled.
        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            SP_WebService_Remove(); // Service App Framework
            SP_WinService_Remove(); // Windows Service
            WinService_Remove();
            //SP_LoggingService_Remove();
        }

        #endregion
        #region Event Receiver - Activation / Deactivation

        // Uncomment the method below to handle the event raised after a feature has been activated.

        //public override void FeatureActivated(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}

        #endregion
        #region Event Receiver - Upgrade

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}

        #endregion


        #region WinService

        private void WinService_Add()
        {
            // remove any previous evidence, just in case.
            WinService_Remove();

            // install windows service
            Common.WinService.Exec_Install();
        }

        private void WinService_Remove()
        {
            // remove from Windows Services
            Common.WinService.Exec_Uninstall();
        }

        #endregion

        #region SP - Windows Service

        private void SP_WinService_Add()
        {
            // register with SharePoint services
            var svc = SharePoint.WindowsService.FACTORY_Ensure();

            // set the process identity
            SP_WinService_SetProcessIdentity(svc);

            // register the instance with all SPServers
            SPFarm.Local
                  .Servers
                  .Where(s => s.Role == SPServerRole.Application ||
                              s.Role == SPServerRole.SingleServer)
                  .ToList()
                  .ForEach(s =>
                  {
                      var inst = s.ServiceInstances.GetValue<SharePoint.WindowsServiceInstance>();
                      if (inst == null)
                      {
                          SharePoint.WindowsServiceInstance.FACTORY_Ensure("", s);
                      }
                  });

        }

        private void SP_WinService_SetProcessIdentity(WindowsService svc)
        {

            // configure the windows service to use the SPFarm's managed account

            // get the managed account, per http://www.sbrickey.com/Tech/Blog/Post/Recovering_SharePoint_Managed_Accounts
            var farmAccount_LoginName = SPFarm.Local.DefaultServiceAccount.LookupName();
            var managedAccounts = new SPFarmManagedAccountCollection(SPFarm.Local);
            var farmAccount = managedAccounts[farmAccount_LoginName];
            // use the managed account, per http://social.technet.microsoft.com/Forums/sharepoint/en-US/0e89487d-5b1e-46ba-9956-f04ebcf8646c/sptrace-service-account-requirements?forum=sharepointgeneralprevious
            svc.ProcessIdentity.CurrentIdentityType = IdentityType.SpecificUser;
            svc.ProcessIdentity.ManagedAccount = farmAccount;

            // persist
            svc.ProcessIdentity.Update(true);   // persist the changes to SharePoint
            svc.ProcessIdentity.Provision();    // actual application of credentials to the windows service
        }

        private void SP_WinService_Remove()
        {
            // stop and delete all instances; otherwise the service won't be stopped, and the EXE may remain in use (file lock)
            SPFarm.Local.Servers
                        .ToList()
                        .ForEach(s =>
                        {
                            var inst = s.ServiceInstances.GetValue<SharePoint.WindowsServiceInstance>();
                            if (inst != null)
                            {
                                if (inst.Status == SPObjectStatus.Online)
                                    inst.Unprovision();

                                inst.Delete();
                            }
                        });

            // remove from SharePoint registration
            var svc = SPFarm.Local
                            .Services
                            .OfType<SharePoint.WindowsService>()
                            .SingleOrDefault();

            if (svc != null)
            {
                if (svc.Status == SPObjectStatus.Online)
                    svc.Unprovision();
                svc.Delete();
            }

        }

        #endregion

        #region SP - (Web) Service App

        private void SP_WebService_Add()
        {
            var svc = SPFarm.Local.Services.GetValue<ServiceApp.Provider.WebService>();
            if (svc == null)
            {
                svc = ServiceApp.Provider.WebService.FACTORY_Ensure();
                if (svc.Status != SPObjectStatus.Online)
                    svc.Provision();
            }

            var svcProxy = SPFarm.Local.Services.GetValue<ServiceApp.Consumer.WebServiceProxy>();
            if (svcProxy == null)
            {
                svcProxy = ServiceApp.Consumer.WebServiceProxy.FACTORY_Ensure();
                if (svcProxy.Status != SPObjectStatus.Online)
                    svcProxy.Provision();
            }

            SPFarm.Local.Servers
                        .Where(s => s.Role == SPServerRole.Application ||
                                    s.Role == SPServerRole.SingleServer)
                        .ToList()
                        .ForEach(s =>
                        {
                            var inst = s.ServiceInstances.GetValue<ServiceApp.Provider.WebServiceInstance>();
                            if (inst == null)
                            {
                                inst = ServiceApp.Provider.WebServiceInstance.FACTORY_Ensure(s, svc);

                            }
                        });

        }

        private void SP_WebService_Remove()
        {
            // remove all service app proxies
            SPFarm.Local.ServiceApplicationProxyGroups.ToList().ForEach(group =>
            {
                group.Proxies.OfType<ServiceApp.Consumer.WebServiceAppProxy>().ToList().ForEach(sap =>
                {
                    sap.Unprovision();
                    sap.Delete();
                });
            });

            // remove all service apps
            SPFarm.Local.Services.ToList().ForEach(s =>
            {
                s.Applications.OfType<ServiceApp.Provider.WebServiceApp>().ToList().ForEach(svcApp =>
                {
                    if (svcApp.Status == SPObjectStatus.Online)
                        svcApp.Unprovision();

                    svcApp.Delete();
                });
            });

            // remove all service instances
            SPFarm.Local.Servers
                        .ToList()
                        .ForEach(s =>
                        {
                            var inst = s.ServiceInstances.GetValue<ServiceApp.Provider.WebServiceInstance>();
                            if (inst != null)
                            {
                                if (inst.Status == SPObjectStatus.Online)
                                    inst.Unprovision();

                                inst.Delete();
                            }
                        });


            // remove the service proxy
            var svcProxy = SPFarm.Local.Services.GetValue<ServiceApp.Consumer.WebServiceProxy>();
            if (svcProxy != null)
            {
                if (svcProxy.Status == SPObjectStatus.Online)
                    svcProxy.Unprovision();
                svcProxy.Delete();
            }

            // remove the service
            var svc = SPFarm.Local.Services.GetValue<ServiceApp.Provider.WebService>();
            if (svc != null)
            {
                if (svc.Status == SPObjectStatus.Online)
                    svc.Unprovision();
                svc.Delete();
            }
        }
        
        #endregion

    } // class
} // namespace
