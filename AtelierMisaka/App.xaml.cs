using AtelierMisaka.Cef;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AtelierMisaka
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CefHelper.ResolveCefSharpAssembly;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            SetDllDirectory(CefHelper.cefDirectory);
            
            Control.IsTabStopProperty.OverrideMetadata(typeof(Control), new FrameworkPropertyMetadata(false));

            GlobalLanguage.SetCulture(CultureInfo.CurrentCulture.Name);

            base.OnStartup(e);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);
    }
}
