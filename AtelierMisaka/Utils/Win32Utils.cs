using System.Runtime.InteropServices;

namespace AtelierMisaka
{
    internal static class Win32Utils
    {
        [DllImport("winhttp.dll", SetLastError = true)]
        internal static extern bool WinHttpGetIEProxyConfigForCurrentUser(ref WinHttpCurrentUserIEProxyConfig pProxyConfig);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WinHttpCurrentUserIEProxyConfig
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool AutoDetect;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string AutoConfigUrl;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string Proxy;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string ProxyBypass;

    }
}
