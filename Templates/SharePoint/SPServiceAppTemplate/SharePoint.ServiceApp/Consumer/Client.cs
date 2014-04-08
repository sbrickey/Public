using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.SPServiceAppTemplate.SharePoint.ServiceApp.Consumer
{
    /// <summary>
    /// Wrapper class to consume the WCF contract via SharePoint service application
    /// </summary>
    public sealed class Client : _Contract_.iServiceAppContract
    {
        private SPServiceContext _serviceContext;
        private WebServiceAppProxy _serviceAppProxy;

        public Client(SPSite site)
        {
            _serviceContext = SPServiceContext.GetContext(site);
            _serviceAppProxy = _serviceContext.GetDefaultProxy(typeof(WebServiceAppProxy)) as WebServiceAppProxy;
        }
        public Client(SPServiceContext serviceContext)
        {
            _serviceContext = serviceContext;
            _serviceAppProxy = _serviceContext.GetDefaultProxy(typeof(WebServiceAppProxy)) as WebServiceAppProxy;
        }

        public _Contract_.iServiceAppContract_Ping_Response Ping(_Contract_.iServiceAppContract_Ping_Request request)
        {
            // validation
            if (_serviceAppProxy == null)
            {
                return new _Contract_.iServiceAppContract_Ping_Response()
                {
                    Success = false,
                    FailureMessage = "No service application proxy exists for the current context"
                };
                //throw new NullReferenceException("No service application proxy exists for the current context");
            }

            // output prep
            _Contract_.iServiceAppContract_Ping_Response outval = null;

            // processing / execution
            outval = _serviceAppProxy.Ping(request);

            // return
            return outval;
        }
    } // class
} // namespace