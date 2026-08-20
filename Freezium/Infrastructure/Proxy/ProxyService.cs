using System;
using System.IO;
using System.Threading.Tasks;
using Fiddler;
using Freezium.Core;

namespace Freezium.Infrastructure.Proxy
{
    /// <summary>
    /// Manages the FiddlerCore proxy: certificate checks, start and stop processes.
    /// </summary>
    public class ProxyService
    {
        private readonly RequestInterceptor _requestInterceptor;
        private readonly ResponseInterceptor _responseInterceptor;

        public event Action<string> LogMessage;
        public event Action<string> StatusChanged;

        public bool IsRunning => FiddlerApplication.IsStarted();

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyService"/> with the given request and
        /// response interceptors.
        /// </summary>
        public ProxyService(RequestInterceptor requestInterceptor, ResponseInterceptor responseInterceptor)
        {
            _requestInterceptor = requestInterceptor;
            _responseInterceptor = responseInterceptor;
        }

        /// <summary>
        /// Checks the Root CA certificate, creates it if not found, requests trust if not trusted.
        /// </summary>
        public bool EnsureCertificate()
        {
            var certDir = Path.GetDirectoryName(Constants.CertLocation);
            if (!Directory.Exists(certDir))
            {
                Directory.CreateDirectory(certDir);
            }

            var certMaker = new BCCertMaker.BCCertMaker();
            CertMaker.oCertProvider = certMaker;

            if (!File.Exists(Constants.CertLocation))
            {
                certMaker.CreateRootCertificate();
                certMaker.WriteRootCertificateAndPrivateKeyToPkcs12File(
                    Constants.CertLocation, Constants.CertPassword);
            }
            else
            {
                certMaker.ReadRootCertificateAndPrivateKeyFromPkcs12File(
                    Constants.CertLocation, Constants.CertPassword);
            }

            if (!CertMaker.rootCertIsTrusted())
            {
                CertMaker.trustRootCert();
            }

            return CertMaker.rootCertIsTrusted();
        }

        /// <summary>
        /// Starts the proxy with automatic Windows PAC routing for target domains only.
        /// </summary>
        public async Task StartAsync()
        {
            if (IsRunning) return;

            LogMessage?.Invoke("Creating Internet Proxy...");

            while (!EnsureCertificate())
            {
                StatusChanged?.Invoke("Waiting for you to trust certificate");
                await Task.Delay(3000);
            }

            var settings = new FiddlerCoreStartupSettingsBuilder()
                .ListenOnPort(Constants.ProxyPort)
                .ChainToUpstreamGateway()
                .DecryptSSL()
                .OptimizeThreadPool()
                .Build();

#pragma warning disable CS0618
            CONFIG.sHostsThatBypassFiddler = Constants.BypassHost;

            // Performance: Enable socket reuse
            CONFIG.bReuseServerSockets = true;
            CONFIG.bReuseClientSockets = true;

            FiddlerApplication.BeforeRequest += _requestInterceptor.Handle;
            FiddlerApplication.BeforeResponse += _responseInterceptor.Handle;
            FiddlerApplication.Startup(settings);

            // Automatically configure Windows PAC so only target domains route through proxy
            SystemProxyHelper.EnablePac($"http://127.0.0.1:{Constants.ProxyPort}/proxy.pac");

            LogMessage?.Invoke($"Internet Proxy Enabled ({FiddlerApplication.oProxy.ListenPort})");
            LogMessage?.Invoke("Smart PAC routing active: Only target anime traffic is proxied. Zero internet speed loss.");
            StatusChanged?.Invoke("Running");
        }

        /// <summary>
        /// Gracefully stops the running proxy, cleans up Windows PAC settings, and releases all resources.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning) return;

            try
            {
                // Disable Windows PAC routing first
                SystemProxyHelper.DisablePac();

                // Detach event handlers
                FiddlerApplication.BeforeRequest -= _requestInterceptor.Handle;
                FiddlerApplication.BeforeResponse -= _responseInterceptor.Handle;
                
                // Shutdown FiddlerCore
                FiddlerApplication.Shutdown();
                
                // Small delay to ensure port is released
                System.Threading.Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Stop error: {ex.Message}");
            }

            LogMessage?.Invoke("Proxy Stopped and System Settings Restored.");
            StatusChanged?.Invoke("Stopped");
        }

        /// <summary>
        /// Forces an immediate shutdown of the FiddlerCore engine and cleans up proxy settings.
        /// </summary>
        public void Shutdown()
        {
            SystemProxyHelper.DisablePac();
            Stop();
        }
    }
}
