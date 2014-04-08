using Microsoft.SharePoint.Administration;
using SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp._Helper_;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Consumer
{    /// <summary>
    /// Proxy class for the WFE's to communicate with the service applications on the app server(s)
    /// </summary>
    [System.Runtime.InteropServices.Guid("4CFEB0E6-C862-4FB6-B0AA-7EBB633C8835")]
    public sealed class WebServiceAppProxy : mySPIisWebServiceApplicationProxy<_Contract_.iServiceAppContract>,
                                             _Contract_.iServiceAppContract
    {
        // public ctor - required for SPPersistedObject
        public WebServiceAppProxy() : base() { }
        // ctor - for FACTORY use ONLY
        private WebServiceAppProxy(string name, Consumer.WebServiceProxy proxy, Uri serviceAddress) : base(name, proxy, serviceAddress) { }

        public static WebServiceAppProxy FACTORY_Get(string name, WebServiceProxy proxy)
        {
            #region validation
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (proxy == null)
                throw new ArgumentNullException("proxy");
            #endregion

            return proxy.ApplicationProxies.GetValue<WebServiceAppProxy>(name);
        }
        public static WebServiceAppProxy FACTORY_Ensure(string name, WebServiceProxy proxy, Uri serviceAddress)
        {
            #region validation
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (proxy == null)
                throw new ArgumentNullException("proxy");
            if (serviceAddress == null)
                throw new ArgumentNullException("serviceAddress");
            #endregion

            var applicationProxy = FACTORY_Get(name, proxy);
            if (applicationProxy == null)
            {
                applicationProxy = new Consumer.WebServiceAppProxy(name, proxy, serviceAddress);
                applicationProxy.Update();
                applicationProxy.Provision();
            }
            return applicationProxy;
        }

        public void FACTORY_AddToDefaultProxyGroup()
        {
            var appProxyGroup = SPServiceApplicationProxyGroup.Default;
            appProxyGroup.Add(this);
            appProxyGroup.Update();
        }

        /// <summary>
        /// name of the service application PROXY as it appears in the Type column of
        ///   Central Administration | Application Management | Manage Service Applications page.
        /// </summary>
        public override string TypeName { get { return SPServiceAppTemplate.Common.SharePoint.SPWebServiceApplicationProxy.TypeName; } }
        public override string DisplayName { get { return SPServiceAppTemplate.Common.SharePoint.SPWebServiceApplicationProxy.DisplayName; } }

        // TODO: confirm WCF settings (bindings, etc) in client.config
        protected override string WebClientsConfigPath { get { return @"WebClients\SBrickey\SPServiceAppTemplate"; } }


        #region Exposed Methods
        // TODO: update to match WCF contract definition

        public _Contract_.iServiceAppContract_Ping_Response Ping(_Contract_.iServiceAppContract_Ping_Request request)
        {
            // prepare the response
            _Contract_.iServiceAppContract_Ping_Response outval = null;

            // execute the WCF call using the Service App Framework, assign response
            outval = ExecuteOnChannel(channel => channel.Ping(request));

            // return the response
            return outval;
        }

        #endregion


    }
}
