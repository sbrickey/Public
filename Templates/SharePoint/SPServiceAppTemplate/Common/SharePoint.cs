using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.SPServiceAppTemplate.Common
{
    public static class SharePoint
    {

        public static class SPWindowsService
        {
            /// <summary>
            /// name of the service as it appears in the Service list at
            ///   Central Administration | System Settings | Manage servers in this farm
            /// and
            ///   Central Administration | System Settings | Manage services on server
            /// and
            ///   Central Administration | Security | Configure Service Accounts
            /// </summary>
            public const string TypeName = "Custom Windows Service";

            public const string DisplayName = "Custom Windows Service DisplayName";
        }

        public static class SPWindowsServiceInstance
        {
            /// <summary>
            /// overrides the SPWindowsService.TypeName
            /// </summary>
            public const string TypeName = SPWindowsService.TypeName; // do not override

            public const string DisplayName = "Custom Windows Service Instance DisplayName";

            public const string Description = "Custom Windows Service Instance Description";
        }

        public static class SPWebService
        {
            /// <summary>
            /// name of the service as it appears in the Service list at
            ///   Central Administration | System Settings | Manage servers in this farm
            /// and
            ///   Central Administration | System Settings | Manage services on server
            /// </summary>
            public const string TypeName = "Custom Web service";

            public const string DisplayName = "Custom Web Service DisplayName";
        }

        public static class SPWebServiceInstance
        {
            /// <summary>
            /// overrides the SPWebService.TypeName
            /// </summary>
            public const string TypeName = SPWebService.TypeName; // do not override

            public const string DisplayName = "Custom Web Service Instance DisplayName";

            public const string Description = "Custom Web Service Instance Description";
        }

        public static class SPWebServiceProxy
        {
            /// <summary>
            /// name of the service application proxy as it appears in the CONNECT dropdown in the ribbon at
            ///   Central Administration | Application Management | Manage Service Applications page.
            /// </summary>
            public const string TypeName = "Custom Service Application Proxy";

            public const string Description = "Custom Service Application Proxy Description";
        }

        public static class SPWebServiceApplication
        {
            /// <summary>
            /// name of the service application (*NOT* the proxy) as it appears in the Type column of
            ///   Central Administration | Application Management | Manage Service Applications page.
            /// also shown in the Service Accounts list
            /// </summary>
            public const string TypeName = "Custom Service Application";

            public const string DisplayName = TypeName;
            public const string Description = "Custom Service Application Description";

            // TODO: Add a CREATE page to the solution.
            //       Use of the scenarioid is entirely optional, but would require the matching Scenario XML file
            public const string LINK_Create = "/_admin/SBrickey/SPServiceAppTemplate/Create.aspx?scenarioid=SBrickey_SPServiceAppTemplate";
        }

        public static class SPWebServiceApplicationProxy
        {
            /// <summary>
            /// name of the service application PROXY as it appears in the Type column of
            ///   Central Administration | Application Management | Manage Service Applications page.
            /// </summary>
            public const string TypeName = "Custom Service Application Proxy";
            public const string DisplayName = TypeName;
        }


    } // class
} // namespace
