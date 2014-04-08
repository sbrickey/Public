using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Provider
{
    /// <summary>
    /// Instance of the service app, running on a specific SPServer.
    /// 
    /// This includes provisioning any files/folders to the server, and creating
    /// the necessary IIS components (app pool, virtual directory, etc)
    /// </summary>
    /// <remarks>
    /// SPIisWebServiceInstance defines a running instance of the service
    ///  as managed in Central Administration | System Settings | Manage Services on Server
    /// </remarks>
    [System.Runtime.InteropServices.Guid("835B5711-AC7B-4A35-900D-54189533BF84")]
    public sealed class WebServiceInstance : SPIisWebServiceInstance
    {
        // default constructor required by SPPersistedObject
        public WebServiceInstance() { }

        // private constructor, used by FACTORY
        private WebServiceInstance(SPServer server, Provider.WebService service) : base(server, service) { }

        public static WebServiceInstance FACTORY_Get(SPServer server)
        {
            return server.ServiceInstances
                         .GetValue<WebServiceInstance>();
        }
        public static WebServiceInstance FACTORY_Ensure(SPServer server, Provider.WebService service)
        {
            var instance = server.ServiceInstances.GetValue<WebServiceInstance>();
            if (instance == null)
            {
                instance = new WebServiceInstance(server, service);
                instance.Update();
            }
            return instance;
        }

        #region Names and Descriptions

        // via SPServiceInstance
        public override string Description { get { return Common.SharePoint.SPWebServiceInstance.Description; } }

        // via SPPersistedObject
        public override string DisplayName { get { return Common.SharePoint.SPWebServiceInstance.DisplayName; } }

        /// <summary>
        /// name of the service as it appears in the Service list at
        ///   Central Administration | System Settings | Manage servers in this farm
        /// and
        ///   Central Administration | System Settings | Manage services on server
        /// </summary>
        /// <remarks>
        ///   overrides the WebService.TypeName
        /// </remarks>
        public override string TypeName { get { return Common.SharePoint.SPWebServiceInstance.TypeName; } }

        #endregion

        #region SPIisWebServiceInstance methods

        public override void Provision()
        {
            base.Provision();
        }

        public override void Unprovision()
        {
            base.Unprovision();
        }

        #endregion

    } // class
} // namespace
