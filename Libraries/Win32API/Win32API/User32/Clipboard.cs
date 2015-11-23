using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBrickey.Libraries.Win32API.User32
{
    public static class Clipboard
    {
        public class ClipboardInUseException : Exception
        {
            public System.Diagnostics.Process Process { get; set; }

            public ClipboardInUseException(System.Diagnostics.Process process, Exception innerException) 
                : base("Clipboard is currently in use", innerException)
            {
                this.Process = process;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// See: http://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid
        /// </remarks>
        public static void SetText(string Text, bool KeepAfterExit, int RetryCount = 0, int RetryDelay = 0)
        {
            if (Text == null)
                throw new ArgumentNullException("Text");

            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(Text, KeepAfterExit, RetryCount, RetryDelay);
            }
            catch (Exception ex)
            {
                var ClipboardLock = Interop.User32.GetOpenClipboardWindow();
                if (ClipboardLock == IntPtr.Zero)
                    throw new System.ComponentModel.Win32Exception();

                var ClipboardLockProc = Interop.User32.GetWindowThreadProcess(ClipboardLock);
                if (ClipboardLockProc == null)
                    throw new System.ComponentModel.Win32Exception();

                throw new ClipboardInUseException(ClipboardLockProc, ex);
            }

        } // void SetText(...)



    } // class
} // namespace