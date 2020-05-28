using System;
using System.Globalization;
using System.Runtime.InteropServices;
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

            GlobalRegex.Initialize();

            GlobalLanguage.InitializeCulture();

            base.OnStartup(e);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);
    }
}
