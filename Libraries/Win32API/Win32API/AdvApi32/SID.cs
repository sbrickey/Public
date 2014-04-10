using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SBrickey.Libraries.Win32API.AdvApi32
{
    public static class SID
    {
        sealed public class PSID
        {
            private IntPtr ptr { get; set; }

            // convert from...
            public static implicit operator PSID(byte[] sid)
            {
                // convert byte array to string, via pString
                IntPtr pstrSID;
                ConvertSidToStringSid(sid, out pstrSID);
                string strSID = Marshal.PtrToStringAuto(pstrSID);

                // convert string pointer to PSID
                IntPtr psid = IntPtr.Zero;
                ConvertStringSidToSid(strSID, out psid);

                // create LSAInfo struct and pointer, then marshal the structure
                AdvApi32.LSA.LSA_ENUMERATION_INFORMATION lsaInfo = new AdvApi32.LSA.LSA_ENUMERATION_INFORMATION { PSid = psid };
                IntPtr pLsaInfo = Marshal.AllocHGlobal(Marshal.SizeOf(lsaInfo));
                Marshal.StructureToPtr(lsaInfo, pLsaInfo, true);
                return pLsaInfo;
            }
            public static implicit operator PSID(IntPtr o) { return new PSID() { ptr = o }; }

            // convert to...
            public static implicit operator IntPtr(PSID o) { return o.ptr; }
            public static implicit operator byte[](PSID o)
            {
                int sidLen = (int)GetLengthSid(o);
                byte[] sid = new byte[sidLen];
                Marshal.Copy(o, sid, 0, sidLen);

                return sid;
            }
        }



        [DllImport("advapi32.dll")]
        private static extern uint GetLengthSid(IntPtr pSid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool ConvertStringSidToSid(string lbBuffer, out IntPtr ptrSid);

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ConvertSidToStringSid([MarshalAs(UnmanagedType.LPArray)] byte[] pSID, out IntPtr ptrSid);

    }
}
