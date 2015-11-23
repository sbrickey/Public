namespace SBrickey.Libraries.Win32API.User32
{
    using System;
    using System.Runtime.InteropServices;

    public static class Window
    {
        private static class _Unmanaged_
        {
            private const string ExternDLL_User32 = "user32.dll";

            [DllImport(ExternDLL_User32, SetLastError = true)]
            public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport(ExternDLL_User32, SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int GetWindowLong(IntPtr hWnd, GetWindowLong_Param_nIndex nIndex);

            [DllImport(ExternDLL_User32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindow(IntPtr hWnd);

            [DllImport(ExternDLL_User32, SetLastError = true)]
            public static extern IntPtr GetOpenClipboardWindow();

            [DllImport(ExternDLL_User32, SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        } // class Unmanaged
        
        public static IntPtr? FindWindow(IntPtr Parent, IntPtr? ChildAfter, WindowStyles WindowStyle)
        {
            var hWnd = _Unmanaged_.FindWindowEx(
                hwndParent: Parent,
                hwndChildAfter: ChildAfter.HasValue ? ChildAfter.Value : IntPtr.Zero,
                lpszClass: null,
                lpszWindow: null
            );

            // if null result, done
            if (hWnd == IntPtr.Zero)
                return null;

            // get window style
            var hWnd_WindowStyle = (WindowStyles)_Unmanaged_.GetWindowLong(hWnd, _Unmanaged_.GetWindowLong_Param_nIndex.GWL_STYLE);

            // if match, return
            if (hWnd_WindowStyle.HasFlag(WindowStyle))
                return hWnd;

            // recursive : depth (child handles) then breadth (childafter)
            return FindWindow(Parent: hWnd, ChildAfter: null, WindowStyle: WindowStyle)
                ?? FindWindow(Parent: Parent, ChildAfter: hWnd, WindowStyle: WindowStyle)
                ?? IntPtr.Zero;
        }

        public static bool IsWindow(IntPtr hwnd)
        {
            return _Unmanaged_.IsWindow(hwnd);
        }

        public static IntPtr GetOpenClipboardWindow()
        {
            return _Unmanaged_.GetOpenClipboardWindow();
        }

        public static System.Diagnostics.Process GetWindowThreadProcess(IntPtr hwnd)
        {
            uint ProcessID;
            var thread = _Unmanaged_.GetWindowThreadProcessId(hwnd, out ProcessID);
            
            return System.Diagnostics.Process.GetProcessById((int)ProcessID);
        }

    } // User32
} // namespace