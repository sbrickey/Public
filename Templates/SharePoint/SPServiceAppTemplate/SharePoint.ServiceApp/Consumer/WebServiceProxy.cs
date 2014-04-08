using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Consumer
{
    /// <summary>
    /// Factory for the Service App Proxy
    /// </summary>
    /// <remarks>
    /// SupportedServiceApplication must match ServiceApp's [Guid] attribute
    /// </remarks>
    [System.Runtime.InteropServices.Guid("E16F2CE4-5E1C-401C-844A-2437BBB91E6D")]
    [SupportedServiceApplication(classId: "42BAC033-5C82-402E-B3C1-3F9AED6370F6", minimumVersion: "1.0.0.0", proxyType: typeof(WebServiceAppProxy))]
    public sealed class WebServiceProxy : SPIisWebServiceProxy, IServiceProxyAdministration
    {
        public WebServiceProxy() : base() { }

        // ctor - ONLY accessible to factory
        private WebServiceProxy(SPFarm farm) : base(farm) { }

        public static WebServiceProxy FACTORY_Get()
        {
            return SPFarm.Local.GetObject(string.Empty, SPFarm.Local.Id, typeof(WebServiceProxy)) as WebServiceProxy;
        }
        public static WebServiceProxy FACTORY_Ensure()
        {
            var serviceProxy = FACTORY_Get();
            if (serviceProxy == null)
            {
                serviceProxy = new Consumer.WebServiceProxy(SPFarm.Local);
                serviceProxy.Update();
                serviceProxy.Provision();
                serviceProxy.Update();
            }

            return serviceProxy;
        }

        #region IServiceProxyAdministration

        public SPServiceApplicationProxy CreateProxy(Type serviceApplicationProxyType, string name, Uri serviceApplicationUri, SPServiceProvisioningContext provisioningContext)
        {
            if (serviceApplicationProxyType != typeof(Consumer.WebServiceAppProxy))
                throw new NotSupportedException();

            return Consumer.WebServiceAppProxy.FACTORY_Ensure(name, this, serviceApplicationUri);
        }

        /// <summary>
        /// name of the service application proxy as it appears in the CONNECT dropdown in the ribbon at
        ///   Central Administration | Application Management | Manage Service Applications page.
        /// </summary>
        public SPPersistedTypeDescription GetProxyTypeDescription(Type serviceApplicationProxyType)
        {
            return new SPPersistedTypeDescription(
                Common.SharePoint.SPWebServiceProxy.TypeName,
                Common.SharePoint.SPWebServiceProxy.Description
            );
        }

        public Type[] GetProxyTypes()
        {
            return new Type[] { typeof(Consumer.WebServiceAppProxy) };
        }

        #endregion

    } // class
} // namespace
