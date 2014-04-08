using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Provider
{
    /// <summary>
    /// Factory for the WCF service
    /// </summary>
    [System.Runtime.InteropServices.Guid("BF8A9DA6-DA94-4D83-B508-9BC0838A965E")]
    public sealed class ServiceHostFactory : System.ServiceModel.Activation.ServiceHostFactory
    {
        public override System.ServiceModel.ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            ServiceHost serviceHost = null;

            serviceHost = new ServiceHost(typeof(WebServiceApp), baseAddresses);
            serviceHost.Configure(SPServiceAuthenticationMode.Claims);

            return serviceHost;
        }
    } // class
}
