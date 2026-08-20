using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Freezium.Infrastructure.Proxy
{
    /// <summary>
    /// Manages Windows native PAC (Proxy Auto-Configuration) settings using WinINET API and Registry.
    /// Allows selective proxying of target domains without impacting global internet speed or requiring browser extensions.
    /// </summary>
    public static class SystemProxyHelper
    {
        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;
        private const string RegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

        /// <summary>
        /// Enables Windows automatic proxy configuration pointing to the local PAC script URL.
        /// </summary>
        /// <param name="pacUrl">The URL serving the PAC script (e.g. http://127.0.0.1:8888/proxy.pac)</param>
        public static void EnablePac(string pacUrl = "http://127.0.0.1:8888/proxy.pac")
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue("AutoConfigURL", pacUrl);
                        key.SetValue("ProxyEnable", 0); // Disable blanket proxy so only PAC governs routing
                    }
                }
                RefreshWinInet();
            }
            catch
            {
                // Silent fail if registry write fails
            }
        }

        /// <summary>
        /// Disables PAC configuration and restores normal direct internet connectivity.
        /// </summary>
        public static void DisablePac()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("AutoConfigURL", false);
                        key.SetValue("ProxyEnable", 0);
                    }
                }
                RefreshWinInet();
            }
            catch
            {
                // Silent fail
            }
        }

        /// <summary>
        /// Flushes WinINET proxy cache and notifies all running applications (browsers, OS) of proxy changes.
        /// </summary>
        private static void RefreshWinInet()
        {
            try
            {
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            }
            catch
            {
            }
        }
    }
}
