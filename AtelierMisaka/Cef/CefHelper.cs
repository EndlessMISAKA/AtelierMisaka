using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AtelierMisaka
{
    public static class CefHelper
    {
        private static readonly string asesmblyDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        public static readonly string cefDirectory = Path.Combine(asesmblyDirectory, Environment.Is64BitProcess ? "x64" : "x86");
        private static bool initialized;

        public static string CachePath => Path.Combine(asesmblyDirectory, "ChromiumCache");

        public static void Initialize()
        {
            if (initialized) return;

            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

            var cefSettings = new CefSettings()
            {
                BrowserSubprocessPath = Path.Combine(cefDirectory, "CefSharp.BrowserSubprocess.exe"),
                CachePath = CachePath,
            };
            cefSettings.CefCommandLineArgs.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.129 Safari/537.36");

            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            Cef.Initialize(cefSettings);
            initialized = true;
        }

        public static Assembly ResolveCefSharpAssembly(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                var assemblyName = args.Name.Split(new[] { ',' }, 2).FirstOrDefault() + ".dll";
                var archSpecificPath = Path.Combine(cefDirectory, assemblyName);

                return File.Exists(archSpecificPath)
                    ? Assembly.LoadFile(archSpecificPath)
                    : null;
            }
            return null;
        }

        public static async Task<bool> SetProxy(ChromiumWebBrowser cwb, string Address)
        {
            return await CefSharp.Cef.UIThreadTaskFactory.StartNew(delegate
            {
                var rc = cwb.GetBrowser().GetHost().RequestContext;
                var v = new Dictionary<string, object>();
                v["mode"] = "fixed_servers";
                v["server"] = Address;
                return rc.SetPreference("proxy", v, out string error);
            });
        }
    }
}
