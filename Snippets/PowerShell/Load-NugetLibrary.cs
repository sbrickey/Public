/*
Usage:
Add-Type -ReferencedAssemblies @( "System.Core","System.Xml","System.IO.Compression","System.IO.Compression.FileSystem" ) `
         -TypeDefinition ( [System.IO.File]::ReadAllText("C:\path\to\Nuget.cs") )
*/

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Xml;

namespace SBrickey {
    public static class Nuget {

        public static Func<string,byte[]> DownloadFunc { private get; set; }

        // ctor
        static Nuget() {
            DownloadFunc = url => { return new WebClient().DownloadData(url); };
        }

        public static void Load(string packageName) {
            var dlUri   = String.Format("https://www.nuget.org/api/v2/package/{0}", packageName);
            var dlBytes = DownloadFunc(dlUri);
            using (var ms = new MemoryStream(dlBytes)) {
                var pkgZip    = new ZipArchive(ms);
                var specEntry_FN = String.Format("{0}.nuspec", packageName);
                var specEntry = pkgZip.Entries.Where(e => e.FullName.ToLower() == specEntry_FN.ToLower()).Single();
                // var specEntry = pkgZip.GetEntry();

                // first load dependencies
                var pkgDeps = GetPackageDependencies(specEntry);
                foreach(var pkgDep in pkgDeps)
                    Load(pkgDep);

                // now load the included assemblies
                var pkgDlls = GetPackageAssemblies(pkgZip);
                foreach(var dll in pkgDlls)
                    System.Reflection.Assembly.Load(dll);
            } // using ms
        }
        public static void Load(string packageName, Action loadTest) {
            Load(packageName);

            // now instantiate test object
            // var curResolver = System.AppDomain.CurrentDomain.AssemblyResolve;
            System.AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveInMemory;
            loadTest.Invoke();
            System.AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolveInMemory;
        }

        private static IEnumerable<string> GetPackageDependencies(ZipArchiveEntry specZip) {
            //if (specZip == null) throw new ArgumentNullException(nameof(specZip));
            if (specZip == null) throw new ArgumentNullException("specZip");

            var specXml = new XmlDocument();
            using (var sr = specZip.Open())
                specXml.Load(sr);

            var nsMgr = new XmlNamespaceManager(specXml.NameTable);
            var nsN   = specXml.DocumentElement.NamespaceURI;
            nsMgr.AddNamespace("n", nsN);

            var deps = specXml.SelectNodes( "/n:package/n:metadata/n:dependencies"
                                          + "/n:group[@targetFramework='.NETFramework4.5']/n:dependency/@id"
                                          , nsMgr)
                              .Cast<XmlAttribute>()
                              .Select(x => x.Value);
            return deps;
        }

        private static IEnumerable<byte[]> GetPackageAssemblies(ZipArchive zip) {
            var dllFiles = zip.Entries
                              .Where(e => Path.GetExtension(e.Name).ToLower() == ".dll")
                              .Where(e => e.FullName.ToLower().StartsWith("lib/net45")
                                       || e.FullName.ToLower().StartsWith("lib/netstandard1.")
                                    );
            if (!dllFiles.Any()) {
                dllFiles = zip.Entries
                              .Where(e => Path.GetExtension(e.Name).ToLower() == ".dll")
                              .Where(e => e.FullName.ToLower().StartsWith("lib/net40"));
            }

            foreach(var dll in dllFiles) {
                var bufSize = Convert.ToInt32(dll.Length);
                var buf = new byte[bufSize];
                using (var sr = dll.Open())
                    sr.Read(buf, 0, bufSize);
                yield return buf;
            }
        }

        private static System.Reflection.Assembly AssemblyResolveInMemory(object sender, ResolveEventArgs e) {
            var asm = new System.Reflection.AssemblyName(e.Name);
            var matches = System.AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == asm.Name
                                                                                 && a.GetName().Version >= asm.Version);
            if (matches.Any())
                return matches.First();

            return null;
        }

    } // class
} // namespace
