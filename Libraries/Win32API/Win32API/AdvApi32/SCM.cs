using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SBrickey.Libraries.Win32API._Common_;
using System.ComponentModel;

namespace SBrickey.Libraries.Win32API.AdvApi32
{
    public static class SCM
    {
        sealed public class SCM_HANDLE : HANDLE, IDisposable
        {
            internal SCM_HANDLE(IntPtr ptr) { this.ptr = ptr; }
            public void Dispose()
            {
                if (this.ptr != IntPtr.Zero)
                {
                    CloseServiceHandle(this);
                    this.ptr = IntPtr.Zero;
                }
            }
        }
        sealed public class SERVICE_HANDLE : HANDLE, IDisposable
        {
            internal SERVICE_HANDLE(IntPtr ptr) { this.ptr = ptr; }
            public void Dispose()
            {
                if (this.ptr != IntPtr.Zero)
                {
                    CloseServiceHandle(this);
                    this.ptr = IntPtr.Zero;
                }
            }
        }


        public static SCM_HANDLE OpenSCManager(string Hostname, SCM_ACCESS rights)
        {
            IntPtr pHandle = OpenSCManager(Hostname, null, (uint)rights);

            if (pHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error()); //, "Unable to open handle to Service Control Manager");

            return new SCM_HANDLE(pHandle);
        }
        public static SERVICE_HANDLE OpenService(SCM_HANDLE scmHandle, string ServiceName, SERVICE_ACCESS rights)
        {
            IntPtr pService = OpenService(scmHandle, ServiceName, (uint)rights);
            
            // errors and exceptions
            if (pService == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return new SERVICE_HANDLE(pService);
        }

        public static QUERY_SERVICE_CONFIG QueryServiceConfig(SERVICE_HANDLE serviceHandle)
        {
            IntPtr qscPtr = IntPtr.Zero;
            try
            {
                // identify the bytes necessary for allocation
                uint bytesNeeded = 0;

                // call to determine amount of memory to allocate
                if (!QueryServiceConfig(serviceHandle, IntPtr.Zero, 0, out bytesNeeded) && bytesNeeded == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // allocate the memory
                qscPtr = Marshal.AllocCoTaskMem(Convert.ToInt32(bytesNeeded));

                // call with allocated memory
                if (!QueryServiceConfig(serviceHandle, qscPtr, bytesNeeded, out bytesNeeded))
                    throw new Win32Exception();

                // marshal the return object
                var sci = (QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(qscPtr, typeof(QUERY_SERVICE_CONFIG));

                return sci;
            }
            finally
            {
                Marshal.FreeCoTaskMem(qscPtr);
            }
        }


        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms685981.aspx" />
        /// </summary>
        [Flags]
        public enum SCM_ACCESS : uint
        {
            #region Service Control Manager : Access Rights

            /// <summary>
            /// Required to connect to the service control manager.
            /// </summary>
            SC_MANAGER_CONNECT = 0x00001,

            /// <summary>
            /// Required to call the CreateService function to create a service
            /// object and add it to the database.
            /// </summary>
            SC_MANAGER_CREATE_SERVICE = 0x00002,

            /// <summary>
            /// Required to call the EnumServicesStatusEx function to list the 
            /// services that are in the database.
            /// </summary>
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,

            /// <summary>
            /// Required to call the LockServiceDatabase function to acquire a 
            /// lock on the database.
            /// </summary>
            SC_MANAGER_LOCK = 0x00008,

            /// <summary>
            /// Required to call the QueryServiceLockStatus function to retrieve 
            /// the lock status information for the database.
            /// </summary>
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,

            /// <summary>
            /// Required to call the NotifyBootConfigStatus function.
            /// </summary>
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,

            #endregion

            /// <summary>
            /// Includes STANDARD_RIGHTS_REQUIRED, in addition to all access 
            /// rights in this table.
            /// </summary>
            SC_MANAGER_ALL_ACCESS =
                ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            #region Service Control Manager : Generic (aggregate) Access Rights

            GENERIC_READ =
                ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,

            GENERIC_WRITE =
                ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_EXECUTE =
                ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT |
                SC_MANAGER_LOCK,

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS

            #endregion

        }

        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms685981.aspx" />
        /// </summary>
        [Flags]
        public enum SERVICE_ACCESS : uint
        {

            #region Service : Access Rights

            /// <summary>
            /// Required to call the QueryServiceConfig and QueryServiceConfig2 functions to query the service configuration.
            /// </summary>
            SERVICE_QUERY_CONFIG = 0x0001,

            /// <summary>
            /// Required to call the ChangeServiceConfig or ChangeServiceConfig2 function to change the service configuration.
            ///  Because this grants the caller the right to change the executable file that the system runs, it should be granted only to administrators.
            /// </summary>
            SERVICE_CHANGE_CONFIG = 0x0002,

            /// <summary>
            /// Required to call the QueryServiceStatus or QueryServiceStatusEx function to ask the service control manager about the status of the service.
            /// Required to call the NotifyServiceStatusChange function to receive notification when a service changes status.
            /// </summary>
            SERVICE_QUERY_STATUS = 0x0004,

            /// <summary>
            /// Required to call the EnumDependentServices function to enumerate all the services dependent on the service.
            /// </summary>
            SERVICE_ENUMERATE_DEPENDENTS = 0x0008,

            /// <summary>
            /// Required to call the StartService function to start the service.
            /// </summary>
            SERVICE_START = 0x0010,

            /// <summary>
            /// Required to call the ControlService function to stop the service.
            /// </summary>
            SERVICE_STOP = 0x0020,

            /// <summary>
            /// Required to call the ControlService function to pause or continue the service.
            /// </summary>
            SERVICE_PAUSE_CONTINUE = 0x0040,

            /// <summary>
            /// Required to call the ControlService function to ask the service to report its status immediately.
            /// </summary>
            SERVICE_INTERROGATE = 0x0080,

            /// <summary>
            /// Required to call the ControlService function to specify a user-defined control code.
            /// </summary>
            SERVICE_USER_DEFINED_CONTROL = 0x0100,


            #endregion

            /// <summary>
            /// Includes STANDARD_RIGHTS_REQUIRED, in addition to all access rights in this table (enum).
            /// </summary>
            SERVICE_ALL_ACCESS =
                ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SERVICE_QUERY_CONFIG |
                SERVICE_CHANGE_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_INTERROGATE |
                SERVICE_USER_DEFINED_CONTROL,

            #region Service : Standard Access Rights

            /// <summary>
            /// Required to call the QueryServiceObjectSecurity or SetServiceObjectSecurity function to access the SACL.
            ///  The proper way to obtain this access is to enable the SE_SECURITY_NAMEprivilege in the caller's current access token,
            ///  open the handle for ACCESS_SYSTEM_SECURITY access, and then disable the privilege.
            /// </summary>
            ACCESS_SYSTEM_SECURITY = ACCESS_MASK.ACCESS_SYSTEM_SECURITY,

            /// <summary>
            /// Required to call the DeleteService function to delete the service.
            /// </summary>
            DELETE = ACCESS_MASK.DELETE,

            /// <summary>
            /// Required to call the QueryServiceObjectSecurity function to query the security descriptor of the service object.
            /// </summary>
            READ_CONTROL = ACCESS_MASK.READ_CONTROL,

            /// <summary>
            /// Required to call the SetServiceObjectSecurity function to modify the Dacl member of the service object's security descriptor.
            /// </summary>
            WRITE_DAC = ACCESS_MASK.ACCESS_SYSTEM_SECURITY,

            /// <summary>
            /// Required to call the SetServiceObjectSecurity function to modify the Owner and Group members of the service object's security descriptor.
            /// </summary>
            WRITE_OWNER = ACCESS_MASK.WRITE_OWNER,

            #endregion



            #region Service : Generic (aggregate) Access Rights

            GENERIC_READ =
                ACCESS_MASK.STANDARD_RIGHTS_READ |
                SERVICE_QUERY_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_INTERROGATE |
                SERVICE_ENUMERATE_DEPENDENTS,

            GENERIC_WRITE =
                ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SERVICE_CHANGE_CONFIG,

            GENERIC_EXECUTE =
                ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_USER_DEFINED_CONTROL

            #endregion


        }

        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/aa379607.aspx" />
        /// <seealso cref="http://pinvoke.net/default.aspx/Enums.ACCESS_MASK" />
        /// </summary>
        [Flags]
        public enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000f0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001f0000,

            SPECIFIC_RIGHTS_ALL = 0x0000ffff,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037f
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class QUERY_SERVICE_CONFIG
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwServiceType;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwStartType;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwErrorControl;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpBinaryPathName;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpLoadOrderGroup;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwTagID;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpDependencies;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpServiceStartName;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpDisplayName;
        }



        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenService(IntPtr hSCManager, String lpServiceName, UInt32 dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Boolean QueryServiceConfig(IntPtr hService, IntPtr intPtrQueryConfig, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

    }
}
