using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace SBrickey.SPServiceAppTemplate.SharePoint._MAPPED_Template_ADMIN.SBrickey.SPServiceAppTemplate
{
    /// <summary>
    /// Sample code for creating the service application, as would be used in the Create admin application page
    /// </summary>
    class Create : System.Web.UI.Page
    {
        #region Page Controls
        protected TextBox txtServiceAppName;
        protected IisWebServiceApplicationPoolSection ApplicationPoolSection;
        #endregion

        private void OKButton_Click()
        {

            using (var spLongOp = new SPLongOperation(this))
            {
                try
                {
                    spLongOp.Begin();

                    var svc = SharePoint.ServiceApp.Provider.WebService.FACTORY_Get();
                    if (svc == null)
                        throw new Exception("Web Service is not installed");

                    var svcApp = SharePoint.ServiceApp.Provider.WebServiceApp.FACTORY_Ensure(
                        this.txtServiceAppName.Text.Trim(),
                        svc,
                        this.ApplicationPoolSection.GetOrCreateApplicationPool()
                    );

                    if (svcApp == null)
                        throw new Exception("Failed to create service application");

                    svcApp.EndProvision(svcApp.BeginProvision(null, null));
                }
                catch { }

                EndSPLongOperation(spLongOp);

            } // using spLongOp
        } // OKButton_Click


        /// <summary>
        /// correctly handles the closing of long operations
        ///   - addresses potential need to close IsDlg modal dialog
        /// </summary>
        protected void EndSPLongOperation(SPLongOperation operation)
        {
            HttpContext context = HttpContext.Current;
            if (context.Request.QueryString["IsDlg"] != null)
            {
                context.Response.Write("<script type='text/javascript'>window.frameElement.commitPopup();</script>");
                context.Response.Flush();
                context.Response.End();
            }
            else
            {
                string url = SPContext.Current.Web.Url;
                operation.End(url, Microsoft.SharePoint.Utilities.SPRedirectFlags.CheckUrl, context, string.Empty);
            }
        }

    } // class
} // namespace
