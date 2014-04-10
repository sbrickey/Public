using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace SBrickey.Libraries.Win32API.NetApi32
{
    public static class NetApi32
    {

        // local group lookup
        public static List<string> GetUserLocalGroups(string Servername, string Username, bool IncludeIndirect)
        {
            IntPtr bufPtr = IntPtr.Zero;

            try
            {

                // input parameters
                NetUserGetLocalGroups_FLAGS flags = IncludeIndirect
                                                  ? NetUserGetLocalGroups_FLAGS.LG_INCLUDE_INDIRECT
                                                  : NetUserGetLocalGroups_FLAGS.NONE;

                // return parameters
                uint EntriesRead;
                uint TotalEntries;

                int err = NetUserGetLocalGroups(Servername, Username, 0, (int)flags, out bufPtr, 1024, out EntriesRead, out TotalEntries);
                if (err != 0)
                    throw new Exception("Username or computer not found");

                List<string> groups = new List<string>();

                // prepare an array to store the groups
                LPGROUP_USERS_INFO_0[] RetGroups = new LPGROUP_USERS_INFO_0[EntriesRead];

                // 64-bit safe way to marshal through the array
                for (ulong i = 0; i < EntriesRead; i++)
                {
                    IntPtr itemAddr = new IntPtr(bufPtr.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(typeof(LPGROUP_USERS_INFO_0))));
                    RetGroups[i] = (LPGROUP_USERS_INFO_0)Marshal.PtrToStructure(itemAddr, typeof(LPGROUP_USERS_INFO_0));
                    groups.Add(RetGroups[i].groupname);
                }

                return groups;
            }
            finally
            {
                if (bufPtr != IntPtr.Zero)
                    NetApiBufferFree(bufPtr);
            }
        }

        // domain group lookup
        public static List<string> GetUserNetGroups(string Servername, string Username)
        {
            IntPtr bufPtr = IntPtr.Zero;

            try
            {
                // output paramers
                uint EntriesRead;
                uint TotalEntries;

                int ErrorCode = NetUserGetGroups(Servername, Username, 0, out bufPtr, 1024, out EntriesRead, out TotalEntries);
                if (ErrorCode != 0)
                    throw new Exception("Username or computer not found");

                List<string> groups = new List<string>();

                // prepare an array to store the groups
                LPGROUP_USERS_INFO_0[] RetGroups = new LPGROUP_USERS_INFO_0[EntriesRead];

                // 64-bit safe way to marshal through the array
                for (ulong i = 0; i < EntriesRead; i++)
                {
                    IntPtr itemAddr = new IntPtr(bufPtr.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(typeof(LPGROUP_USERS_INFO_0))));
                    RetGroups[i] = (LPGROUP_USERS_INFO_0)Marshal.PtrToStructure(itemAddr, typeof(LPGROUP_USERS_INFO_0));
                    groups.Add(RetGroups[i].groupname);
                }

                return groups;
            }
            finally
            {
                if (bufPtr != IntPtr.Zero)
                    NetApiBufferFree(bufPtr);
            }
        }


        public static List<string> NetLocalGroupGetMembers(string Servername, string GroupName)
        {
            IntPtr pBuffer = IntPtr.Zero;
            IntPtr pResumeHandle = IntPtr.Zero;

            try
            {
                uint EntriesRead;
                uint TotalEntries;

                int ErrorCode = NetLocalGroupGetMembers(Servername, GroupName, 1, out pBuffer, 1024, out EntriesRead, out TotalEntries, ref pResumeHandle);
                if (ErrorCode != 0)
                    throw new Exception("Username or computer not found");

                List<string> groups = new List<string>();
                for (ulong i = 0; i < EntriesRead; i++)
                {
                    // identify the memory address for the current entry (64-bit safe)
                    IntPtr itemAddr = new IntPtr(pBuffer.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(typeof(LOCALGROUP_MEMBERS_INFO_1))));
                    // marshal the pointer to an object
                    LOCALGROUP_MEMBERS_INFO_1 RetGroup = (LOCALGROUP_MEMBERS_INFO_1)Marshal.PtrToStructure(itemAddr, typeof(LOCALGROUP_MEMBERS_INFO_1));

                    groups.Add(RetGroup.lgrmi1_name);
                }
                return groups;

            }
            finally
            {
                if (pBuffer == IntPtr.Zero)
                    NetApiBufferFree(pBuffer);
                if (pResumeHandle == IntPtr.Zero)
                    NetApiBufferFree(pResumeHandle);
            }
        }

        public static List<string> NetGroupGetUsers(string Servername, string GroupName)
        {

            IntPtr pBuffer = IntPtr.Zero;
            IntPtr pResumeHandle = IntPtr.Zero;

            try
            {
                // output parameters
                uint EntriesRead;
                uint TotalEntries;

                int ErrorCode = NetGroupGetUsers(Servername, GroupName, 0, out pBuffer, 1024, out EntriesRead, out TotalEntries, ref pResumeHandle);
                if (ErrorCode != 0)
                    throw new Exception("Username or computer not found");

                List<string> groups = new List<string>();

                // prepare an array to store the groups
                LPGROUP_USERS_INFO_0[] RetGroups = new LPGROUP_USERS_INFO_0[EntriesRead];

                // 64-bit safe way to marshal through the array
                for (ulong i = 0; i < EntriesRead; i++)
                {
                    IntPtr itemAddr = new IntPtr(pBuffer.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(typeof(LPGROUP_USERS_INFO_0))));
                    RetGroups[i] = (LPGROUP_USERS_INFO_0)Marshal.PtrToStructure(itemAddr, typeof(LPGROUP_USERS_INFO_0));
                    groups.Add(RetGroups[i].groupname);
                }

                return groups;

                // release memory
            }
            finally
            {
                if (pBuffer != IntPtr.Zero)
                    NetApiBufferFree(pBuffer);
                if (pResumeHandle != IntPtr.Zero)
                    NetApiBufferFree(pResumeHandle);
            }
        }

        public static WKSTA_INFO_100 NetWkstaGetInfo_100(string ComputerName)
        {
            IntPtr pBuffer = IntPtr.Zero;

            try
            {
                int retval = NetWkstaGetInfo(ComputerName, 100, out pBuffer);
                if (retval != 0)
                    throw new Win32Exception(retval);

                WKSTA_INFO_100 info = (WKSTA_INFO_100)Marshal.PtrToStructure(pBuffer, typeof(WKSTA_INFO_100));
                return info;
            }
            finally
            {
                if (pBuffer != IntPtr.Zero)
                    NetApiBufferFree(pBuffer);
            }
        }









        const int ERROR_MORE_DATA = 234;
        const int ERROR_NO_SUCH_ALIAS = 0x560;   // 1376

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LPGROUP_USERS_INFO_0
        {
            public string groupname;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LOCALGROUP_MEMBERS_INFO_0
        {
            public IntPtr lgrmi0_sid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LOCALGROUP_MEMBERS_INFO_1
        {
            IntPtr lgrmi1_sid;

            SID_NAME_USE lgrmi1_sidusage;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lgrmi1_name;
        }

        /// <see cref="http://msdn.microsoft.com/en-us/library/aa371402(v=vs.85).aspx" />
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class WKSTA_INFO_100
        {
            public wki100_platform_id wki100_platform_id;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string wki100_computername;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string wki100_langroup;
            public int wki100_ver_major;
            public int wki100_ver_minor;
        }

        public enum wki100_platform_id : int
        {
            PLATFORM_ID_DOS = 300,
            PLATFORM_ID_OS2 = 400,
            PLATFORM_ID_NT = 500,
            PLATFORM_ID_OSF = 600,
            PLATFORM_ID_VMS = 700
        }

        public enum SID_NAME_USE : int
        {
            User = 1,
            Group = 2,
            Domain = 3,
            Alias = 4,
            KnownGroup = 5,
            DeletedAccount = 6,
            Invalid = 7,
            Unknown = 8,
            Computer = 9
        }

        [Flags]
        public enum NetUserGetLocalGroups_FLAGS : int
        {
            NONE = 0x0000,
            LG_INCLUDE_INDIRECT = 0x0001
        }

        #region externs

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetUserGetLocalGroups
            ([MarshalAs(UnmanagedType.LPWStr)] string servername,
             [MarshalAs(UnmanagedType.LPWStr)] string username,
             int level,
             int flags,
             out IntPtr bufptr,
             int prefmaxlen,
             out uint entriesread,
             out uint totalentries);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetUserGetGroups
            ([MarshalAs(UnmanagedType.LPWStr)] string servername,
             [MarshalAs(UnmanagedType.LPWStr)] string username,
             int level,
             out IntPtr bufptr,
             int prefmaxlen,
             out uint entriesread,
             out uint totalentries);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetLocalGroupGetMembers
            ([MarshalAs(UnmanagedType.LPWStr)] string servername,
             [MarshalAs(UnmanagedType.LPWStr)] string groupname,
             int level,
             out IntPtr bufptr,
             int prefmaxlen,
             out uint entriesread,
             out uint totalentries,
             ref IntPtr ResumeHandle);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetGroupGetUsers
            ([MarshalAs(UnmanagedType.LPWStr)] string servername,
             [MarshalAs(UnmanagedType.LPWStr)] string groupname,
             int level,
             out IntPtr bufptr,
             int prefmaxlen,
             out uint entriesread,
             out uint totalentries,
             ref IntPtr ResumeHandle);

        [DllImport("netapi32.dll", CharSet = CharSet.Auto)]
        static extern int NetWkstaGetInfo(
            string server,
            int level,
            out IntPtr info);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetApiBufferFree(IntPtr Buffer);


        #endregion
    }
}
