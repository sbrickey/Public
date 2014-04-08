using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp._Contract_
{
    // TODO: Define WCF contract for calls made using the service app framework

    [ServiceContract]
    public interface iServiceAppContract
    {

        [OperationContract]
        iServiceAppContract_Ping_Response Ping(iServiceAppContract_Ping_Request request);

    } // interface

    public class iServiceAppContract_Ping_Request
    {
    } // class
    public class iServiceAppContract_Ping_Response
    {
        public bool Success { get; set; }
        public string FailureMessage { get; set; }
    } // class

} // namespace
