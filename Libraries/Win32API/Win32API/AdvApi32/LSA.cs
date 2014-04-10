using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.ComponentModel;
using SBrickey.Libraries.Win32API._Common_;

namespace SBrickey.Libraries.Win32API.AdvApi32
{
    public static class LSA
    {
        sealed public class LSA_HANDLE : HANDLE, IDisposable
        {
            internal LSA_HANDLE(IntPtr ptr) { this.ptr = ptr; }
            public void Dispose() { LsaFreeMemory(this); }
        }



        public static LSA_HANDLE OpenPolicy(string SystemName, ACCESS_MASK RequiredAccess)
        {
            LSA_OBJECT_ATTRIBUTES lsaAttr = LSA_OBJECT_ATTRIBUTES_Default;
            
            // identify machine to query
            LSA_UNICODE_STRING[] system = null;
            if (SystemName != null)
            {
                system = new LSA_UNICODE_STRING[1];
                system[0] = Convert.LSA_UNICODE_STRING(SystemName);
            }

            IntPtr lsaHandle;

            // connect to LSA (least privileges required)
            uint ret = LsaOpenPolicy(system, ref lsaAttr, (int)(ACCESS_MASK.POLICY_LOOKUP_NAMES | ACCESS_MASK.POLICY_VIEW_LOCAL_INFORMATION), out lsaHandle);

            // success
            if (ret == 0)
                return new LSA_HANDLE(lsaHandle);

            // failure
            if (ret == STATUS_ACCESS_DENIED)
                throw new UnauthorizedAccessException();
            if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY))
                throw new OutOfMemoryException();
            throw new Win32Exception(LsaNtStatusToWinError((int)ret));
        }

        // Enumerate Rights

        public static IEnumerable<string> LsaEnumerateAccountRights(LSA_HANDLE lsaHandle, string account, string privilege)
        {
            byte[] sid = LsaLookupNames2(lsaHandle, account);

            // call output
            IntPtr hPrivileges;
            uint privileges_count;

            // call
            uint ret = LsaEnumerateAccountRights(lsaHandle, sid, out hPrivileges, out privileges_count);

            // failure
            if (ret == STATUS_ACCESS_DENIED)
                throw new UnauthorizedAccessException();
            if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY))
                throw new OutOfMemoryException();
            if (ret != 0)
                throw new Win32Exception(LsaNtStatusToWinError((int)ret));

            // return value
            List<string> privileges = new List<string>();

            // marshall the call outputs to the return value
            for (ulong i = 0; i < privileges_count; i++)
            {
                IntPtr itemAddr = new IntPtr(hPrivileges.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(typeof(LSA_UNICODE_STRING))));
                LSA_UNICODE_STRING LSA_privilege = (LSA_UNICODE_STRING)Marshal.PtrToStructure(itemAddr, typeof(LSA_UNICODE_STRING));
                privileges.Add(LSA_privilege.Buffer);
            }

            // free the memory
            LsaFreeMemory(hPrivileges);

            // return results
            return privileges;
        }

        public static IEnumerable<SID.PSID> LsaEnumerateAccountsWithUserRight(LSA_HANDLE lsaHandle, string privilege)
        {
            IntPtr buffer = IntPtr.Zero;

            try
            {
                // prepare input parameters
                LSA_UNICODE_STRING[] privileges = new LSA_UNICODE_STRING[] { Convert.LSA_UNICODE_STRING(privilege) };

                // prepare output parameters
                uint count = 0;

                // call
                uint ret = LsaEnumerateAccountsWithUserRight(lsaHandle, privileges, out buffer, out count);

                // successful call; no results
                if (ret == STATUS_NO_MORE_ENTRIES)
                    return new List<SID.PSID>();

                // errors and exceptions
                if (ret == STATUS_ACCESS_DENIED)
                    throw new UnauthorizedAccessException();
                if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY))
                    throw new OutOfMemoryException();
                if (ret != 0)
                    throw new Win32Exception(LsaNtStatusToWinError((int)ret));

                // prepare return data
                List<SID.PSID> outval = new List<SID.PSID>();

                // process and marshal output data into return object
                for (ulong i = 0; i < count; i++)
                {
                    IntPtr itemAddr = new IntPtr(buffer.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(typeof(LSA_ENUMERATION_INFORMATION))));
                    LSA_ENUMERATION_INFORMATION LsaInfo = (LSA_ENUMERATION_INFORMATION)Marshal.PtrToStructure(itemAddr, typeof(LSA_ENUMERATION_INFORMATION));
                    outval.Add(LsaInfo.PSid);
                }

                return outval;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    LsaFreeMemory(buffer);
            }
        }

        // Lookups (SID->Name, Name->SID)

        public static SID.PSID LsaLookupNames2(LSA_HANDLE lsaHandle, string account)
        {
            IntPtr tsids = IntPtr.Zero;
            IntPtr tdom = IntPtr.Zero;
            try
            {
                // call parameters
                LSA_UNICODE_STRING[] names = new LSA_UNICODE_STRING[] { Convert.LSA_UNICODE_STRING(account) };
                LSA_TRANSLATED_SID2 lts = new LSA_TRANSLATED_SID2() { Sid = IntPtr.Zero };

                // call
                int ret = LsaLookupNames2(lsaHandle, 0, 1, names, ref tdom, ref tsids);

                // errors and exceptions
                if (ret != 0)
                    throw new Win32Exception(LsaNtStatusToWinError(ret));

                // marshal output
                lts = (LSA_TRANSLATED_SID2)Marshal.PtrToStructure(tsids, typeof(LSA_TRANSLATED_SID2));
                return lts.Sid;
            }
            finally
            {
                if (tsids != IntPtr.Zero)
                    LsaFreeMemory(tsids);
                if (tdom != IntPtr.Zero)
                    LsaFreeMemory(tdom);
            }
        }

        public static string LsaLookupSids(LSA_HANDLE lsaHandle, SID.PSID sid)
        {
            IntPtr pNames = IntPtr.Zero;
            IntPtr pDomain = IntPtr.Zero;

            try
            {

                int ret = LsaLookupSids(lsaHandle, 1, sid, out pDomain, out pNames);
                if (ret != 0)
                {
                    var Win32Err = LsaNtStatusToWinError(ret);

                    // handle this exception (unable to map sid)
                    if (Win32Err == 1332)
                        return null;

                    throw new Win32Exception(Win32Err);
                }

                // Since this function only ever provides a single SID to the call, I don't need to worry about iterating over all returned domains/accounts
                //string domain = ((LSA_REFERENCED_DOMAIN_LIST)Marshal.PtrToStructure(pDomain, typeof(LSA_REFERENCED_DOMAIN_LIST))).Domains.Name.Buffer;
                return ((LSA_TRANSLATED_NAME)Marshal.PtrToStructure(pNames, typeof(LSA_TRANSLATED_NAME))).Name.Buffer;
            }
            finally
            {
                LsaFreeMemory(pDomain);
                LsaFreeMemory(pNames);
            }
        } // LsaLookupSids(...)


        #region #####   HELPERS   #####

        private static LSA_OBJECT_ATTRIBUTES LSA_OBJECT_ATTRIBUTES_Default
        {
            get
            {
                LSA_OBJECT_ATTRIBUTES lsaAttr;
                lsaAttr.RootDirectory = IntPtr.Zero;
                lsaAttr.ObjectName = IntPtr.Zero;
                lsaAttr.Attributes = 0;
                lsaAttr.SecurityDescriptor = IntPtr.Zero;
                lsaAttr.SecurityQualityOfService = IntPtr.Zero;
                lsaAttr.Length = Marshal.SizeOf(typeof(LSA_OBJECT_ATTRIBUTES));
                return lsaAttr;
            }
        }

        #endregion

        private static class Convert
        {
            public static LSA_UNICODE_STRING LSA_UNICODE_STRING(string s)
            {
                // Unicode strings max. 32KB
                if (s.Length > 0x7ffe)
                    throw new ArgumentException("String too long");

                LSA_UNICODE_STRING lus = new LSA_UNICODE_STRING();
                lus.Buffer = s;
                //removed due to unsafe context errors when using sizeof()
                // lus.Length = (ushort)(s.Length * sizeof(char));
                // lus.MaximumLength = (ushort)(lus.Length + sizeof(char));
                lus.Length = (ushort)(s.Length * 2); // Unicode char is 2 bytes
                lus.MaximumLength = (ushort)(lus.Length + 2);

                return lus;
            }
        }

        #region HELPER ATTRIBUTES
        private class LsaPrivilegeAttribute : Attribute
        {
            public string InternalTextName { get; set; }
            public string Description { get; set; }


            public static string GetInternalTextName(Authorizations Authorization)
            {
                // Get the type
                Type type = Authorization.GetType();
                // Get fieldinfo for this type
                System.Reflection.FieldInfo fieldInfo = type.GetField(Authorization.ToString());
                // Get the stringvalue attributes
                LsaPrivilegeAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(LsaPrivilegeAttribute), false) as LsaPrivilegeAttribute[];
                // Return the first if there was a match.
                return attribs.Length > 0 ? attribs[0].InternalTextName : null;
            }
        }
        #endregion



        #region #####   CONST   #####

        const uint STATUS_NO_MORE_ENTRIES        = 0x8000001A;
        const uint STATUS_NO_MEMORY              = 0xc0000017;
        const uint STATUS_ACCESS_DENIED          = 0xc0000022;
        const uint STATUS_INSUFFICIENT_RESOURCES = 0xc000009a;

        #endregion


        #region #####   ENUM   #####

        /// <summary>
        /// Source: http://msdn.microsoft.com/en-us/library/cc232779.aspx
        /// </summary>
        public enum Authorizations
        {
            // reference: http://msdn.microsoft.com/en-us/library/aa375728.aspx

            #region Account Rights
            // reference: http://msdn.microsoft.com/en-us/library/bb545671.aspx

            /// <summary>
            /// Log on as a service
            /// </summary>
            [LsaPrivilege(InternalTextName = "SeServiceLogonRight", Description = "Log on as a service")]
            SE_SERVICE_LOGON_NAME,

            /// <summary>
            /// DENY log on as a service
            /// </summary>
            [LsaPrivilege(InternalTextName = "SeDenyServiceLogonRight", Description = "Deny log on as a service")]
            [Description("Deny log on as a service")]
            SE_DENY_SERVICE_LOGON_NAME,

            #endregion

            #region Privileges
            // reference: http://msdn.microsoft.com/en-us/library/bb530716.aspx

            /// <summary>
            /// Impersonate a client after authetication
            /// </summary>
            [LsaPrivilege(InternalTextName = "SeImpersonatePrivilege", Description = "Impersonate a client after authentication")]
            SE_IMPERSONATE_NAME,

            /// <summary>
            /// Act as part of the operating system
            /// </summary>
            [LsaPrivilege(InternalTextName = "SeTcbPrivilege", Description = "Act as part of the operating system")]
            SE_TCB_NAME

            #endregion

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

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/aa374892(v=vs.85).aspx" />
        [Flags]
        public enum ACCESS_MASK : int
        {
            POLICY_READ = 0x20006,
            POLICY_ALL_ACCESS = 0x00F0FFF,
            POLICY_EXECUTE = 0x20801,
            POLICY_WRITE = 0x207F8,

            POLICY_VIEW_LOCAL_INFORMATION = 0x1,
            POLICY_LOOKUP_NAMES = 0x00000800
        }

        #endregion


        #region #####   STRUCT   #####

        /// <summary>
        /// API documentation: http://msdn.microsoft.com/en-us/library/ms721829.aspx
        /// </summary>
        /// <remarks>
        /// API indicates that LsaOpenPolicy(..) no longer uses this information. Initializers added to simply use.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_OBJECT_ATTRIBUTES
        {
            internal int Length;
            internal IntPtr RootDirectory;
            internal IntPtr ObjectName;
            internal int Attributes;
            internal IntPtr SecurityDescriptor;
            internal IntPtr SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct LSA_UNICODE_STRING
        {
            internal ushort Length;
            internal ushort MaximumLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRUST_INFORMATION
        {
            internal LSA_UNICODE_STRING Name;
            internal IntPtr Sid;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRANSLATED_SID2
        {
            internal SID_NAME_USE Use;
            internal IntPtr Sid;
            internal int DomainIndex;
            internal uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_TRANSLATED_NAME
        {
            internal SID_NAME_USE Use;
            internal LSA_UNICODE_STRING Name;
            internal long DomainIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LSA_REFERENCED_DOMAIN_LIST
        {
            internal uint Entries;
            internal LSA_TRUST_INFORMATION Domains;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct LSA_ENUMERATION_INFORMATION
        {
            internal IntPtr PSid;
        }

        #endregion


        #region #####   EXTERN   #####

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true),
         SuppressUnmanagedCodeSecurity]
        private static extern uint LsaOpenPolicy(LSA_UNICODE_STRING[] SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, int AccessMask, out IntPtr PolicyHandle);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true),
         SuppressUnmanagedCodeSecurity]
        private static extern uint LsaEnumerateAccountRights(IntPtr PolicyHandle, [MarshalAs(UnmanagedType.LPArray)] byte[] AccountSid, out IntPtr UserRights, out uint CountOfRights);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true),
         SuppressUnmanagedCodeSecurityAttribute]
        private static extern uint LsaEnumerateAccountsWithUserRight(IntPtr PolicyHandle, LSA_UNICODE_STRING[] UserRights, out IntPtr EnumerationBuffer, out uint CountReturned);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true),
         SuppressUnmanagedCodeSecurity]
        private static extern uint LsaAddAccountRights(IntPtr PolicyHandle, IntPtr pSID, LSA_UNICODE_STRING[] UserRights, int CountOfRights);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true),
         SuppressUnmanagedCodeSecurity]
        private static extern int LsaLookupNames2(IntPtr PolicyHandle, uint Flags, uint Count, LSA_UNICODE_STRING[] Names, ref IntPtr ReferencedDomains, ref IntPtr Sids);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true),
         SuppressUnmanagedCodeSecurity]
        private static extern int LsaLookupSids(IntPtr PolicyHandle, uint Count, IntPtr Sids, out IntPtr ReferencedDomains, out IntPtr Names);
        // LsaLookupSids2 is for Win8
        //public static extern int LsaLookupSids2(LSA_HANDLE PolicyHandle, uint LookupOptions, uint Count, ref IntPtr Sids, ref IntPtr ReferencedDomains, ref IntPtr Names);

        [DllImport("advapi32")]
        private static extern int LsaNtStatusToWinError(int NTSTATUS);

        [DllImport("advapi32")]
        private static extern int LsaClose(IntPtr PolicyHandle);

        [DllImport("advapi32")]
        private static extern int LsaFreeMemory(IntPtr Buffer);

        #endregion

    } // class LSA
} // namespace
