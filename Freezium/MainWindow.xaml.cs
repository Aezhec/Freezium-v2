using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Freezium.Infrastructure.Proxy;
using Freezium.Services;
using Freezium.UI.Controls;

namespace Freezium
{
    /// <summary>
    /// Main window - handles UI interaction logic only.
    /// Proxy, TrayIcon, and business logic are delegated to separate classes.
    /// </summary>
    public partial class MainWindow : Window
    {
        private TrayIconManager _trayIcon;
        private ProxyService _proxyService;

        public MainWindow(ProxyService proxyService)
        {
            InitializeComponent();

            _proxyService = proxyService;
            _proxyService.LogMessage += AddLog;
            _proxyService.StatusChanged += UpdateStatus;

            SetupTrayIcon();
        }

        #region Initialization

        private void SetupTrayIcon()
        {
            _trayIcon = new TrayIconManager(this);
            _trayIcon.StartProxyRequested += async () => await _proxyService.StartAsync();
            _trayIcon.StopProxyRequested += () => _proxyService.Stop();
            _trayIcon.ManipulateWLChanged += (isChecked) =>
            {
                cbManipulateWL.IsChecked = isChecked;
                AppSettingsService.Current.ManipulateWL = isChecked;
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbManipulateWL.IsChecked = AppSettingsService.Current.ManipulateWL;
        }

        #endregion

        #region Title Bar

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Proxy Controls

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await _proxyService.StartAsync();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _proxyService.Stop();
        }

        #endregion

        #region Settings

        private void cbManipulateWL_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (cbManipulateWL.IsChecked.HasValue)
            {
                AppSettingsService.Current.ManipulateWL = cbManipulateWL.IsChecked.Value;
            }
        }

        #endregion

        #region UI Updates

        public void UpdateStatus(string status)
        {
            Dispatcher.Invoke(() =>
            {
                lblStatus.ToolTip = status;
                lblStatus.Text = "Status: " + status;

                if (status == "Running")
                    StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x50, 0xE0, 0x80));
                else if (status == "Stopped")
                    StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xE0, 0x50, 0x50));
                else
                    StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xB0, 0x80, 0xFF));
            });
        }

        public void AddLog(string text)
        {
            Dispatcher.Invoke(() =>
            {
                rtbLogs.Text = text + "\n" + rtbLogs.Text;
            });
        }

        #endregion

        #region Window Lifecycle

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _trayIcon?.Dispose();
            _proxyService.Shutdown();
        }

        #endregion
    }
}
