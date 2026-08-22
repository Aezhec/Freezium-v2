using System;
using System.Windows;
using Freezium.Core.Interfaces;
using Freezium.Infrastructure.Api;
using Freezium.Infrastructure.Data;
using Freezium.Infrastructure.Proxy;
using Freezium.Services;

namespace Freezium
{
    /// <summary>
    /// Application entry point - dependency wiring is done here.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // --- Dependency Wiring (Poor Man's DI) ---
            IAnimeApiClient apiClient = new AniziumApiClient();
            using (var repository = new LiteDbRepository(apiClient))
            {
                // Initialize Settings
                AppSettingsService.Initialize(repository);

                // Create Proxy components
                var requestInterceptor = new RequestInterceptor(repository);
                var responseInterceptor = new ResponseInterceptor(repository);
                var proxyService = new ProxyService(requestInterceptor, responseInterceptor);

                // Start UI
                var app = new App();
                app.InitializeComponent();
                app.Run(new MainWindow(proxyService));
            }
        }
    }
}
