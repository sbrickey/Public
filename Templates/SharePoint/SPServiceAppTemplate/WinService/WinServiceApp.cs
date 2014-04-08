using System.ServiceProcess;

namespace SBrickey.SPServiceAppTemplate.WinService
{
    public partial class WinServiceApp : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            // TODO: switch release modes! Using the Debug build will cause the JIT
            //       debugger to launch whenever the service is started, whether by
            //       using Windows Services, or SharePoint!
            #if DEBUG
            // make sure a debugger is attached
            if (!System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Launch();

            // provide an immediate breakpoint for debugging
            System.Diagnostics.Debugger.Break();
            #endif

            // TODO: insert Windows Service startup logic
            base.OnStart(args);
        } // OnStart(...)

        protected override void OnStop()
        {
            // TODO: insert Windows Service shutdown logic (optional)
            base.OnStop();
        } // OnStop()

    } // class
} // namespace
