namespace SBrickey.Libraries.Win32API.User32
{
    using System;
    using System.Runtime.InteropServices;

    public static class Scroll
    {
        private static class _Unmanaged_
        {
            private const string ExternDLL_User32 = "user32.dll";

            /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms632600(v=vs.85).aspx"/>
            [Flags]
            public enum WindowStyles : long
            {
                WS_HSCROLL = 0x00100000L,
                WS_VSCROLL = 0x00200000L,
            }
            public enum SBOrientation : int
            {
                SB_HORZ = 0x0,
                SB_VERT = 0x1,
                SB_CTL = 0x2,
                SB_BOTH = 0x3
            }
            public enum GetWindowLong_Param_nIndex : int
            {
                GWL_USERDATA = -21,
                GWL_EXSTYLE = -20,
                GWL_STYLE = -16,
                GWL_ID = -12,
                GWL_HWNDPARENT = -8,
                GWL_HINSTANCE = -6,
                GWL_WNDPROC = -4,
                DWL_MSGRESULT = 0,
                DWL_DLGPROC = 4,
                DWL_USER = 8,
            }

 
            [StructLayout(LayoutKind.Sequential)]
            public struct Win32Rect
            {
                internal int left;
                internal int top;
                internal int right;
                internal int bottom;
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct ScrollBarInfo
            {
                internal int cbSize;
                internal Win32Rect rcScrollBar;
                internal int dxyLineButton;
                internal int xyThumbTop;
                internal int xyThumbBottom;
                internal int reserved;
                internal int scrollBarInfo;
                internal int upArrowInfo;
                internal int largeDecrementInfo;
                internal int thumbnfo;
                internal int largeIncrementInfo;
                internal int downArrowInfo;
            }

            [DllImport(ExternDLL_User32, SetLastError = true)]
            internal static extern bool GetScrollBarInfo(IntPtr hwnd, int fnBar, [In, Out] ref ScrollBarInfo lpsi);


            #region ScrollInfo consts and struct
            public const int SIF_RANGE    = 0x0001;
            public const int SIF_PAGE     = 0x0002;
            public const int SIF_POS      = 0x0004;
            public const int SIF_TRACKPOS = 0x0010;
            public const int SIF_ALL      = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS);

            [StructLayout(LayoutKind.Sequential)]
            public struct ScrollInfo
            {
                public int cbSize;
                public int fMask;
                public int nMin;
                public int nMax;
                public int nPage;
                public int nPos;
                public int nTrackPos;

                public static ScrollInfo Create()
                {
                    return new ScrollInfo()
                    {
                        cbSize = Marshal.SizeOf(typeof(ScrollInfo)),
                        fMask = SIF_ALL
                    };
                }
            }

            [DllImport(ExternDLL_User32, SetLastError = true)]
            internal static extern bool GetScrollInfo(IntPtr hwnd, SBOrientation fnBar, [In, Out] ref ScrollInfo lpsi);
            #endregion
            
        } // class Unmanaged

        public class GetScrollInfo_Result {
            public int Min { get; set; }
            public int Max { get; set; }
            public int Page { get; set; }
            public int Pos { get; set; }
            public int TrackPos { get; set; }
        }
        public static GetScrollInfo_Result GetScrollInfo(IntPtr Handle, System.Windows.Forms.Orientation ScrollBar)
        {
            var si = _Unmanaged_.ScrollInfo.Create();
            var Success = _Unmanaged_.GetScrollInfo(
                hwnd: Handle,
                fnBar: (_Unmanaged_.SBOrientation)ScrollBar,
                lpsi: ref si
            );

            if (!Success)
            {
                var Win32Error = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(Win32Error, "Error attempting to execute GetScrollInfo via user32.dll");
            }

            return new GetScrollInfo_Result() { Min = si.nMin, Max = si.nMax, Page = si.nPage, Pos = si.nPos, TrackPos = si.nTrackPos };
        }

        [Flags]
        public enum WindowStyles : long
        {
            ScrollBar_Horizontal = 0x00100000L,
            ScrollBar_Vertical   = 0x00200000L,
        }

    } // User32
} // namespace