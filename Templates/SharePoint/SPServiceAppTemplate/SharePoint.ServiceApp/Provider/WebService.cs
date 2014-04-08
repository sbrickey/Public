using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Provider
{
    /// <summary>
    /// Factory for the SPServiceApplication and SPServiceApplicationProxy
    /// </summary>
    /// <remarks>
    /// SPIisWebService defines an actual background SP service
    /// 
    /// IServiceAdministration provides support to CREATE the application via
    ///   Central Administration | Application Management | Manage Service Applications
    /// </remarks>
    [System.Runtime.InteropServices.Guid("5606186D-0AF9-4194-82CE-60B9E3FCBF04")]
    public sealed class WebService : SPIisWebService, IServiceAdministration
    {
        public WebService() { }
        private WebService(SPFarm farm) : base(farm) { }

        public static WebService FACTORY_Get()
        {
            return SPFarm.Local.Services.GetValue<WebService>() as WebService;
        }
        public static WebService FACTORY_Ensure()
        {
            var svc = FACTORY_Get();
            if (svc == null)
            {
                svc = new WebService(SPFarm.Local);
                svc.Update();
                svc.Provision();
            }
            return svc;
        }


        #region IServiceAdministration

        public SPServiceApplication CreateApplication(string name, Type serviceApplicationType, SPServiceProvisioningContext provisioningContext)
        {
            #region Validation
            if (serviceApplicationType != typeof(Provider.WebServiceApp))
                throw new ArgumentException("Type serviceApplicationType must be WebServiceApp", "serviceApplicationType");

            if (provisioningContext == null ||
                provisioningContext.IisWebServiceApplicationPool == null)
                throw new ArgumentNullException("provisioningContext");
            #endregion

            return Provider.WebServiceApp.FACTORY_Ensure(
                name,
                this,
                provisioningContext.IisWebServiceApplicationPool
            );
        } // public SPServiceApplication CreateApplication(...)

        public SPServiceApplicationProxy CreateProxy(string name, SPServiceApplication serviceApplication, SPServiceProvisioningContext provisioningContext)
        {
            #region validation
            // validate inputs
            if (serviceApplication == null)
                throw new ArgumentNullException("serviceApplication");

            if (serviceApplication.GetType() != typeof(Provider.WebServiceApp))
                throw new NotSupportedException();
            #endregion

            // Ensure Service Proxy
            var serviceProxy = Consumer.WebServiceProxy.FACTORY_Ensure();

            // Ensure App Proxy
            var applicationProxy = Consumer.WebServiceAppProxy.FACTORY_Ensure(name, serviceProxy, ((Provider.WebServiceApp)serviceApplication).Uri);

            return applicationProxy;
        }

        // name of the service application as it appears in the NEW dropdown in the ribbon at
        //   Central Administration | Application Management | Manage Service Applications page.
        public SPPersistedTypeDescription GetApplicationTypeDescription(Type serviceApplicationType)
        {
            // validate inputs
            if (serviceApplicationType != typeof(Provider.WebServiceApp))
                throw new NotSupportedException();

            return new SPPersistedTypeDescription(
                Common.SharePoint.SPWebServiceApplication.TypeName,
                Common.SharePoint.SPWebServiceApplication.Description
                );
        }

        public Type[] GetApplicationTypes()
        {
            return new Type[] { typeof(Provider.WebServiceApp) };
        }

        #endregion

        #region SPIisWebService

        /// <summary>
        /// Provides the link to the page used to create a new instance of the service application.
        /// 
        /// If not provided, no menu item will be created in the NEW menu on the MANAGE SERVICE APPLICATIONS page.
        /// </summary>
        public override SPAdministrationLink GetCreateApplicationLink(Type serviceApplicationType)
        {
            if (serviceApplicationType == null)
                throw new ArgumentNullException("serviceApplicationType");
            if (serviceApplicationType != typeof(Provider.WebServiceApp))
                throw new NotSupportedException();

            return new SPAdministrationLink(Common.SharePoint.SPWebServiceApplication.LINK_Create);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Useful for PowerShell.
        /// When using SPFarm.Services, DisplayName provides a descriptive name for the service.
        /// </remarks>
        public override string DisplayName { get { return Common.SharePoint.SPWebService.DisplayName; } }

        /// <summary>
        /// name of the service as it appears in the Service list at
        ///   Central Administration | System Settings | Manage servers in this farm
        /// and
        ///   Central Administration | System Settings | Manage services on server
        /// </summary>
        public override string TypeName { get { return Common.SharePoint.SPWebService.TypeName; } }

        #endregion

    } // class
} // namespace
