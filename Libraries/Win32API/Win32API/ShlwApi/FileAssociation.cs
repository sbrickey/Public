namespace SBrickey.Libraries.Win32API.ShlwApi
{
    using System;
    using System.Text;
    using System.Runtime.InteropServices;

    public static class FileAssociation
    {

        // REFERENCE: http://msdn.microsoft.com/en-us/library/bb762475.aspx
        [Flags]
        public enum ASSOCF
        {
            ASSOCF_NONE = 0x00000000,
            ASSOCF_INIT_NOREMAPCLSID = 0x00000001,
            ASSOCF_INIT_BYEXENAME = 0x00000002,
            ASSOCF_OPEN_BYEXENAME = 0x00000002,
            ASSOCF_INIT_DEFAULTTOSTAR = 0x00000004,
            ASSOCF_INIT_DEFAULTTOFOLDER = 0x00000008,
            ASSOCF_NOUSERSETTINGS = 0x00000010,
            ASSOCF_NOTRUNCATE = 0x00000020,
            ASSOCF_VERIFY = 0x00000040,
            ASSOCF_REMAPRUNDLL = 0x00000080,
            ASSOCF_NOFIXUPS = 0x00000100,
            ASSOCF_IGNOREBASECLASS = 0x00000200,
            ASSOCF_INIT_IGNOREUNKNOWN = 0x00000400,
            ASSOCF_INIT_FIXED_PROGID = 0x00000800,
            ASSOCF_IS_PROTOCOL = 0x00001000,
            ASSOCF_INIT_FOR_FILE = 0x00002000
        } // enum ASSOCF

        // REFERENCE: http://msdn.microsoft.com/en-us/library/bb762475.aspx
        public enum ASSOCSTR
        {
            ASSOCSTR_COMMAND = 1,
            ASSOCSTR_EXECUTABLE,
            ASSOCSTR_FRIENDLYDOCNAME,
            ASSOCSTR_FRIENDLYAPPNAME,
            ASSOCSTR_NOOPEN,
            ASSOCSTR_SHELLNEWVALUE,
            ASSOCSTR_DDECOMMAND,
            ASSOCSTR_DDEIFEXEC,
            ASSOCSTR_DDEAPPLICATION,
            ASSOCSTR_DDETOPIC,
            ASSOCSTR_INFOTIP,
            ASSOCSTR_QUICKTIP,
            ASSOCSTR_TILEINFO,
            ASSOCSTR_CONTENTTYPE,
            ASSOCSTR_DEFAULTICON,
            ASSOCSTR_SHELLEXTENSION,
            ASSOCSTR_DROPTARGET,
            ASSOCSTR_DELEGATEEXECUTE,
            ASSOCSTR_SUPPORTED_URI_PROTOCOLS,
            ASSOCSTR_MAX
        } // enum ASSOCSTR

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern uint AssocQueryString(ASSOCF flags, ASSOCSTR str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

        /// <summary>
        /// Obtains information about the file associations registered within Windows.
        /// </summary>
        /// <param name="association"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <example>
        /// MyAssocQueryString ( COMMAND , ".txt" ) ==> "c:\path\to\some\app.exe %1"
        /// </example>
        public static string MyAssocQueryString(ASSOCSTR association, string extension)
        {
            const int S_OK = 0;
            const int S_FALSE = 1;

            uint length = 0;
            uint ret = AssocQueryString(ASSOCF.ASSOCF_NONE, association, extension, null, null, ref length);

            if (ret != S_FALSE)
            {
                throw new InvalidOperationException("Could not determine associated string");
            }

            StringBuilder sb = new StringBuilder((int)length); // (length-1) will probably work too as the marshaller adds null termination
            ret = AssocQueryString(ASSOCF.ASSOCF_NONE, association, extension, null, sb, ref length);
            if (ret != S_OK)
            {
                throw new InvalidOperationException("Could not determine associated string");
            }

            return sb.ToString();
        } // fn
    } // class
} // namespace