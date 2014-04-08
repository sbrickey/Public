using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using System.Runtime.InteropServices;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Provider
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true)]
    [IisWebServiceApplicationBackupBehavior]
    [Guid("42BAC033-5C82-402E-B3C1-3F9AED6370F6")]
    public sealed class WebServiceApp : SPIisWebServiceApplication, _Contract_.iServiceAppContract
    {
        // public default ctor - required by SPPersistedObject
        public WebServiceApp() : base() { }
        // private CTor used by FACTORY
        private WebServiceApp(string name, WebService service, SPIisWebServiceApplicationPool appPool) : base(name, service, appPool) { }

        public static WebServiceApp FACTORY_Get(string name, WebService parent)
        {
            #region validation
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (parent == null)
                throw new ArgumentNullException("parent");
            #endregion

            return SPFarm.Local.GetObject(name, parent.Id, typeof(WebServiceApp)) as WebServiceApp;
        }
        public static WebServiceApp FACTORY_Ensure(string name, WebService parent, SPIisWebServiceApplicationPool IisWebServiceApplicationPool)
        {
            #region validation
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (parent == null)
                throw new ArgumentNullException("parent");
            #endregion

            var outval = SPFarm.Local.GetObject(name, parent.Id, typeof(WebServiceApp)) as WebServiceApp;
            if (outval == null)
            {
                if (IisWebServiceApplicationPool == null)
                    throw new ArgumentNullException("IisWebServiceApplicationPool");

                outval = Provider.WebServiceApp.FACTORY_Create(
                        name,
                        parent,
                        null,
                        IisWebServiceApplicationPool,
                        true
                    );
            }

            return outval;
        }
        private static WebServiceApp FACTORY_Create(string name, WebService service, SPDatabaseParameters dbParams, SPIisWebServiceApplicationPool appPool, bool createAppProxy)
        {
            WebServiceApp svcApp = null;

            #region Validation

            // Validation : service proxy must be installed in the farm
            var svcProxy = SPFarm.Local.Services.GetValue<Consumer.WebServiceProxy>();
            if (svcProxy == null)
            {
                throw new Exception("Service Proxy is not installed");
            }

            // Validation : NetworkServiceAccount can ONLY be used in a single server install
            if (SPServer.LocalServerRole != SPServerRole.SingleServer &&
                appPool.GetSecurityIdentifier().IsWellKnown(System.Security.Principal.WellKnownSidType.NetworkServiceSid))
            {
                throw new Exception(String.Format("Network Service application pool [{0}] cannot be used except for Single Server farms", appPool.Name));
            }

            #endregion

            try
            {
                svcApp = new WebServiceApp(name, service, appPool);
                svcApp.Update();

                svcApp.AddServiceEndpoint("http", SPIisWebServiceBindingType.Http, "http");
                svcApp.AddServiceEndpoint("https", SPIisWebServiceBindingType.Https, "https");
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                if (createAppProxy)
                {
                    // Create the service application Proxy
                    var svcAppProxy = Consumer.WebServiceAppProxy.FACTORY_Ensure(name, svcProxy, svcApp.Uri);

                    // Assign to the default proxy group
                    svcAppProxy.FACTORY_AddToDefaultProxyGroup();

                } // if createAppProxy
            }
            catch (Exception)
            {

                throw;
            }

            return svcApp;

        } // CreateServiceApp(...)

        /// <summary>
        /// name of the service application (*NOT* the proxy) as it appears in the Type column of
        ///   Central Administration | Application Management | Manage Service Applications page.
        /// also shown in the Service Accounts list
        /// </summary>
        public override string TypeName { get { return Common.SharePoint.SPWebServiceApplication.TypeName; } }

        public override string DisplayName { get { return Common.SharePoint.SPWebServiceApplication.DisplayName; } }

        /**** WCF Contract ****/

        #region iContract

        // TODO: Actual backend implementation of WCF contract

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        public _Contract_.iServiceAppContract_Ping_Response Ping(_Contract_.iServiceAppContract_Ping_Request request)
        {
            try
            {
                // generate response object
                return new _Contract_.iServiceAppContract_Ping_Response()
                {
                    Success = true
                };

            }
            catch (Exception ex)
            {
                return new _Contract_.iServiceAppContract_Ping_Response()
                {
                    Success = false,
                    FailureMessage = ex.Message
                };
            }
        }

        #endregion

        #region SPIisWebServiceApplication

        /**** Service App : Provisioning/Unprovisioning ****/

        public override void Provision()
        {
            // update status to Provisioning
            this.Status = SPObjectStatus.Provisioning;
            this.Update();

            // Provision
            base.Provision();

            // update status to Online
            this.Status = SPObjectStatus.Online;
            this.Update();
        }

        public override void Unprovision(bool deleteData)
        {
            // update status to Unprovisioning
            this.Status = SPObjectStatus.Unprovisioning;
            this.Update();

            // Unprovision
            base.Unprovision(deleteData);

            // update status to Disabled
            this.Status = SPObjectStatus.Disabled;
            this.Update();
        }


        // WCF EndPoint settings

        protected override string DefaultEndpointName { get { return "https"; } }

        // TODO: confirm WCF settings (bindings, etc) in web.config
        protected override string InstallPath { get { return SPUtility.GetGenericSetupPath(@"WebServices\SBrickey\SPServiceAppTemplate"); } }

        protected override string VirtualPath { get { return "Service.svc"; } }

        #endregion




    } // class
} // namespace
