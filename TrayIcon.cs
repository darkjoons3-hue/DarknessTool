using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DarknessTool
{
    public class TrayIcon : IDisposable
    {
        private readonly NotifyIcon _notify;
        private readonly MainForm _mainForm;
        private Icon? _generatedIcon;

        public TrayIcon(MainForm form)
        {
            _mainForm = form;

            _generatedIcon = GenerateIcon();

            var menu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(0x0F, 0x1E, 0x33),
                ForeColor = Color.FromArgb(0xE8, 0xE8, 0xE8),
                Font = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont,
                ShowImageMargin = false
            };

            var openItem = new ToolStripMenuItem("Развернуть");
            openItem.ForeColor = Color.FromArgb(0xE8, 0xE8, 0xE8);
            openItem.Click += (s, e) => RestoreWindow();

            var logItem = new ToolStripMenuItem("Журнал изменений");
            logItem.ForeColor = Color.FromArgb(0xE8, 0xE8, 0xE8);
            logItem.Click += (s, e) =>
            {
                RestoreWindow();
                new LogForm().ShowDialog(_mainForm);
            };

            var sep = new ToolStripSeparator();

            var exitItem = new ToolStripMenuItem("Выход");
            exitItem.ForeColor = Color.FromArgb(0xD1, 0x34, 0x38);
            exitItem.Click += (s, e) => ExitApp();

            menu.Items.Add(openItem);
            menu.Items.Add(logItem);
            menu.Items.Add(sep);
            menu.Items.Add(exitItem);

            _notify = new NotifyIcon
            {
                Icon = _generatedIcon,
                Text = "DarknessTool",
                Visible = true,
                ContextMenuStrip = menu
            };

            // Двойной клик — развернуть
            _notify.DoubleClick += (s, e) => RestoreWindow();
        }

        public void RestoreWindow()
        {
            if (_mainForm.WindowState == FormWindowState.Minimized)
                _mainForm.WindowState = FormWindowState.Normal;

            _mainForm.Show();
            _mainForm.Activate();
            _mainForm.BringToFront();
        }

        public void ShowBalloon(string title, string message)
        {
            try
            {
                _notify.BalloonTipTitle = title;
                _notify.BalloonTipText = message;
                _notify.BalloonTipIcon = ToolTipIcon.Info;
                _notify.ShowBalloonTip(3000);
            }
            catch { }
        }

        private void ExitApp()
        {
            _notify.Visible = false;
            _mainForm.ForceClose();
        }

        /// <summary>
        /// Генерирует иконку в трее программно: треугольник с DT.
        /// </summary>
        private static Icon GenerateIcon()
        {
            int size = 32;
            using var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                float pad = 2f;
                float w = size - pad * 2;
                float h = (size - pad * 2) * 0.9f;
                float x = pad;
                float y = pad;
                float r = 3f;

                using var path = new GraphicsPath();
                path.AddLine(x + w / 2, y, x + w / 2 + r, y + r * 0.5f);
                path.AddLine(x + w / 2 + r, y + r * 0.5f, x + w - r, y + h - r);
                path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
                path.AddLine(x + w - r, y + h - r, x + r, y + h - r);
                path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
                path.AddLine(x + r, y + h - r, x + w / 2 - r, y + r * 0.5f);
                path.CloseFigure();

                using (var b = new LinearGradientBrush(
                    new RectangleF(x, y, w, h),
                    Color.FromArgb(0x0E, 0x3A, 0x5C),
                    Color.FromArgb(0x07, 0x1E, 0x33), 90f))
                    g.FillPath(b, path);

                using (var p = new Pen(Color.FromArgb(0x4A, 0x9E, 0xFF), 1.5f))
                    g.DrawPath(p, path);

                var family = SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif;
                using var font = new Font(family, 11f, FontStyle.Bold);
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using var tb = new SolidBrush(Color.White);
                g.DrawString("DT", font, tb,
                    new RectangleF(x, y + h * 0.35f, w, h * 0.5f), sf);
            }

            // Получаем HICON из Bitmap
            IntPtr hIcon = bmp.GetHicon();
            var icon = Icon.FromHandle(hIcon);

            // Клонируем, чтобы можно было освободить handle
            var cloned = (Icon)icon.Clone();
            DestroyIcon(hIcon);
            return cloned;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public void Dispose()
        {
            try
            {
                _notify.Visible = false;
                _notify.Dispose();
                _generatedIcon?.Dispose();
            }
            catch { }
        }
    }
}
