using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Abstraction
{
    public class NetworkConnectivity
    {
        public interface iAmInternetConnected
        {
            bool Check();
        }

        public class Microsoft_NCSI : iAmInternetConnected
        {
            private string _NCSI_DNS = @"dns.msftncsi.com";
            private System.Net.IPAddress _NCSI_DNS_Expected =  new System.Net.IPAddress(new byte[] { 131, 107, 255, 255 });
            private string _NCSI_HTTP_Request_Uri = @"http://www.msftncsi.com/ncsi.txt";
            private string _NCSI_HTTP_Response = @"Microsoft NCSI";

            private string _GPO_RegKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\NlaSvc\Parameters\Internet";
            private string _GPO_RegName =  @"EnableActiveProbing";

            
            public bool IgnorePolicy { get; set; }

            public bool Check()
            {
                // VALIDATION : check against group policy
                if (!IgnorePolicy && DisabledInRegistry())
                    throw new SBrickey.Exceptions.RegistryPolicyException()
                    {
                        RegKey = _GPO_RegKey,
                        RegName = _GPO_RegName,
                        ReferenceUrls = String.Join(
                            System.Environment.NewLine,
                            new string[]
                            {
                                "http://technet.microsoft.com/en-us/library/cc766017.aspx",
                                "http://blogs.technet.com/b/networking/archive/2012/12/20/the-network-connection-status-icon.aspx"
                            }
                        )
                    };

                var nsLookup = System.Net.Dns.GetHostEntry(_NCSI_DNS);
                if (nsLookup.AddressList.Count() != 1 ||
                    nsLookup.AddressList.Single() != _NCSI_DNS_Expected)
                    return false;
            
                var wc = new System.Net.WebClient();
                if (wc.DownloadString(_NCSI_HTTP_Request_Uri) != _NCSI_HTTP_Response)
                    return false;

                return true;
            }

            private bool DisabledInRegistry() { return (Int32) Microsoft.Win32.Registry.GetValue(_GPO_RegKey, _GPO_RegName, 1) == 0; }
        }

        public static iAmInternetConnected Instance { get; set; }

        static NetworkConnectivity()
        {
            // default implementation
            NetworkConnectivity.Instance = new Microsoft_NCSI();
        }

    }

}
