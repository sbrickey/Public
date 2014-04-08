using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Utilities;

namespace SBrickey.SPServiceAppTemplate.Common
{
    public static class Paths
    {

        public static string SPInstallPath = SPUtility.GetGenericSetupPath(String.Empty);

        public static class WinServiceExe
        {
            public static string Folder = System.IO.Path.Combine(SPInstallPath, @"BIN\SBrickey\SPServiceAppTemplate");
            public static string File = "SBrickey.SPServiceAppTemplate.WinService.exe";
            public static string Path = System.IO.Path.Combine(Folder, File);
        }
    }
}
