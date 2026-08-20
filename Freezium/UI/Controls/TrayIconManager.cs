using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Freezium.Services;

namespace Freezium.UI.Controls
{
    /// <summary>
    /// System tray icon and context menu management.
    /// An independent component separated from MainWindow.
    /// </summary>
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Window _ownerWindow;

        public event Action StartProxyRequested;
        public event Action StopProxyRequested;
        public event Action<bool> ManipulateWLChanged;

        public TrayIconManager(Window owner)
        {
            _ownerWindow = owner;

            _notifyIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text = "Freezium",
                Visible = true
            };

            _notifyIcon.MouseDoubleClick += OnTrayDoubleClick;
            SetupContextMenu();
        }

        private void SetupContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Renderer = new DarkContextMenuRenderer();
            menu.BackColor = Color.FromArgb(33, 29, 37);
            menu.ForeColor = Color.White;
            menu.ShowImageMargin = false;
            menu.ShowCheckMargin = false;
            menu.Padding = new Padding(4);
            menu.AutoSize = true;

            // Show/Hide
            var showItem = CreateMenuItem("Show", "showHide");
            showItem.Click += (s, e) => _ownerWindow.Dispatcher.Invoke(ToggleWindowVisibility);
            menu.Items.Add(showItem);

            menu.Items.Add(new ToolStripSeparator());

            // Start Proxy
            var startItem = CreateMenuItem("Start Proxy", "startProxy");
            startItem.Click += (s, e) => _ownerWindow.Dispatcher.Invoke(() => StartProxyRequested?.Invoke());
            menu.Items.Add(startItem);

            // Stop Proxy
            var stopItem = CreateMenuItem("Stop Proxy", "stopProxy");
            stopItem.Click += (s, e) => _ownerWindow.Dispatcher.Invoke(() => StopProxyRequested?.Invoke());
            menu.Items.Add(stopItem);

            menu.Items.Add(new ToolStripSeparator());

            // Manipulate toggle
            var manipulateItem = CreateMenuItem("Manipulate Watch List, Follow, Favorite", "manipulateWL");
            manipulateItem.CheckOnClick = true;
            manipulateItem.Click += (s, e) =>
                _ownerWindow.Dispatcher.Invoke(() => ManipulateWLChanged?.Invoke(manipulateItem.Checked));
            menu.Items.Add(manipulateItem);

            menu.Items.Add(new ToolStripSeparator());

            // Exit
            var exitItem = CreateMenuItem("Exit", "exit");
            exitItem.Click += (s, e) => _ownerWindow.Dispatcher.Invoke(() => _ownerWindow.Close());
            menu.Items.Add(exitItem);

            StyleAllItems(menu);
            menu.Opening += (s, e) => UpdateMenuState(menu);

            _notifyIcon.ContextMenuStrip = menu;
        }

        private ToolStripMenuItem CreateMenuItem(string text, string name)
        {
            return new ToolStripMenuItem(text)
            {
                Name = name,
                AutoSize = false,
                Size = new System.Drawing.Size(280, 28)
            };
        }

        private void StyleAllItems(ContextMenuStrip menu)
        {
            foreach (ToolStripItem item in menu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.ForeColor = Color.White;
                    menuItem.BackColor = Color.FromArgb(33, 29, 37);
                    menuItem.Font = new Font("Segoe UI", 9F);
                }
                else if (item is ToolStripSeparator separator)
                {
                    separator.BackColor = Color.FromArgb(33, 29, 37);
                    separator.ForeColor = Color.FromArgb(147, 143, 155);
                }
            }
        }

        private void UpdateMenuState(ContextMenuStrip menu)
        {
            bool isRunning = Fiddler.FiddlerApplication.IsStarted();

            var showHideItem = menu.Items["showHide"] as ToolStripMenuItem;
            var startItem = menu.Items["startProxy"] as ToolStripMenuItem;
            var stopItem = menu.Items["stopProxy"] as ToolStripMenuItem;
            var manipulateItem = menu.Items["manipulateWL"] as ToolStripMenuItem;

            if (showHideItem != null)
            {
                bool isVisible = _ownerWindow.IsVisible && _ownerWindow.WindowState != WindowState.Minimized;
                showHideItem.Text = isVisible ? "Hide to Tray" : "Show";
            }

            if (startItem != null) startItem.Enabled = !isRunning;
            if (stopItem != null) stopItem.Enabled = isRunning;
            if (manipulateItem != null) manipulateItem.Checked = AppSettingsService.Current.ManipulateWL;
        }

        private void ToggleWindowVisibility()
        {
            if (_ownerWindow.IsVisible && _ownerWindow.WindowState != WindowState.Minimized)
            {
                _ownerWindow.WindowState = WindowState.Minimized;
                _ownerWindow.ShowInTaskbar = false;
            }
            else
            {
                ShowWindow();
            }
        }

        private void OnTrayDoubleClick(object sender, MouseEventArgs e)
        {
            ShowWindow();
        }

        public void ShowWindow()
        {
            _ownerWindow.Show();
            _ownerWindow.WindowState = WindowState.Normal;
            _ownerWindow.ShowInTaskbar = true;
            _ownerWindow.Activate();
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        #region Dark Theme Renderer

        private class DarkContextMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkContextMenuRenderer() : base(new DarkColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var menuItem = e.Item as ToolStripMenuItem;

                if (menuItem != null && menuItem.Checked)
                    DrawCheckmark(e);

                if (e.Item.Selected)
                {
                    var rect = new Rectangle(4, 0, e.Item.Width - 8, e.Item.Height);
                    using (var brush = new SolidBrush(Color.FromArgb(40, 36, 46)))
                        e.Graphics.FillRectangle(brush, rect);
                    using (var pen = new Pen(Color.FromArgb(51, 176, 128, 255)))
                        e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

                    if (menuItem != null && menuItem.Checked)
                        DrawCheckmark(e);
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }

            private static void DrawCheckmark(ToolStripItemRenderEventArgs e)
            {
                var checkRect = new Rectangle(8, (e.Item.Height - 16) / 2, 16, 16);

                using (var brush = new SolidBrush(Color.FromArgb(176, 128, 255)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(brush, checkRect);
                }

                using (var pen = new Pen(Color.White, 2))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawLine(pen,
                        checkRect.Left + 4, checkRect.Top + 8,
                        checkRect.Left + 7, checkRect.Top + 11);
                    e.Graphics.DrawLine(pen,
                        checkRect.Left + 7, checkRect.Top + 11,
                        checkRect.Left + 12, checkRect.Top + 5);
                }
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) { }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                var rect = new Rectangle(12, e.Item.Height / 2, e.Item.Width - 24, 1);
                using (var brush = new SolidBrush(Color.FromArgb(26, 176, 128, 255)))
                    e.Graphics.FillRectangle(brush, rect);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                var menuItem = e.Item as ToolStripMenuItem;
                e.TextFormat |= TextFormatFlags.VerticalCenter;

                var textRect = e.TextRectangle;
                textRect.X = (menuItem != null && menuItem.Checked) ? 32 : 12;
                textRect.Y = 0;
                textRect.Height = e.Item.Height;
                textRect.Width = e.Item.Width - (menuItem != null && menuItem.Checked ? 40 : 20);
                e.TextRectangle = textRect;

                base.OnRenderItemText(e);
            }
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            private static readonly Color Surface = Color.FromArgb(33, 29, 37);
            private static readonly Color Hover = Color.FromArgb(40, 36, 46);
            private static readonly Color Border = Color.FromArgb(51, 176, 128, 255);
            private static readonly Color Sep = Color.FromArgb(26, 176, 128, 255);

            public override Color MenuItemSelected => Hover;
            public override Color MenuItemSelectedGradientBegin => Hover;
            public override Color MenuItemSelectedGradientEnd => Hover;
            public override Color MenuItemBorder => Border;
            public override Color MenuBorder => Border;
            public override Color ImageMarginGradientBegin => Surface;
            public override Color ImageMarginGradientMiddle => Surface;
            public override Color ImageMarginGradientEnd => Surface;
            public override Color SeparatorDark => Sep;
            public override Color SeparatorLight => Sep;
        }

        #endregion
    }
}
