using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.Libraries.Win32API._Common_
{
    public class HANDLE
    {
        internal IntPtr ptr { get; set; }

        // convert from...
        public static implicit operator HANDLE(IntPtr o) { return new HANDLE() { ptr = o }; }

        // convert to...
        public static implicit operator IntPtr(HANDLE o) { return o.ptr; }
    }
}
