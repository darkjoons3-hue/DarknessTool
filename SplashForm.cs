using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Principal;
using System.Windows.Forms;

namespace DarknessTool
{
    public class SplashForm : Form
    {
        private readonly Timer _timer;
        private float _opacityStep = 0.08f;
        private bool _fadingOut = false;
        private readonly DateTime _startedAt = DateTime.UtcNow;

        private static readonly Color BgTop       = Color.FromArgb(0x0A, 0x14, 0x28);
        private static readonly Color BgBottom    = Color.FromArgb(0x05, 0x0A, 0x14);
        private static readonly Color ShellTop    = Color.FromArgb(0x0E, 0x3A, 0x5C);
        private static readonly Color ShellBottom = Color.FromArgb(0x07, 0x1E, 0x33);
        private static readonly Color ShellEdge   = Color.FromArgb(0x4A, 0x9E, 0xFF);
        private static readonly Color TextPrimary = Color.FromArgb(0xE8, 0xE8, 0xE8);
        private static readonly Color TextSecond  = Color.FromArgb(0x88, 0x99, 0xAA);
        private static readonly Color Accent      = Color.FromArgb(0x4A, 0x9E, 0xFF);

        private string _statusText = "Проверка прав…";

        public SplashForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            BackColor = BgTop;
            Opacity = 0.0;

            Size = new Size(420, 300);

            _timer = new Timer { Interval = 40 };
            _timer.Tick += Timer_Tick;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            float s = DeviceDpi / 96f;
            Size = new Size((int)Math.Round(420 * s), (int)Math.Round(300 * s));
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Начинаем затемнение через 1.4 сек
            if (!_fadingOut && (DateTime.UtcNow - _startedAt).TotalMilliseconds > 1400)
            {
                _fadingOut = true;
                _opacityStep = -0.06f;

                bool admin = IsAdmin();
                _statusText = admin ? "Запуск…" : "Запуск без прав администратора…";
                Invalidate();
            }

            double next = Opacity + _opacityStep;

            if (next >= 1.0)
            {
                Opacity = 1.0;
                _opacityStep = 0f;
            }
            else if (next <= 0.0)
            {
                Opacity = 0.0;
                _timer.Stop();
                Close();
            }
            else
            {
                Opacity = next;
            }
        }

        private static bool IsAdmin()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            float s = DeviceDpi / 96f;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(
                ClientRectangle, BgTop, BgBottom, 90f))
                g.FillRectangle(brush, ClientRectangle);

            // Рамка вокруг сплэша
            using (var pen = new Pen(Color.FromArgb(0x1A, 0x2A, 0x44), 1f * s))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            float s = DeviceDpi / 96f;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Треугольник с DT
            float triSize = 120 * s;
            float triX = (Width - triSize) / 2f;
            float triY = 30 * s;
            DrawTriangle(g, triX, triY, triSize, s);

            var family = SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif;

            // Название
            using (var f = new Font(family, 16f * s, FontStyle.Bold))
            using (var b = new SolidBrush(TextPrimary))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("D A R K N E S S T O O L", f, b,
                    new RectangleF(0, triY + triSize + 12 * s, Width, 30 * s), fmt);
            }

            // Версия
            using (var f = new Font(family, 9f * s))
            using (var b = new SolidBrush(TextSecond))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("версия 1.0", f, b,
                    new RectangleF(0, triY + triSize + 42 * s, Width, 20 * s), fmt);
            }

            // Статус
            using (var f = new Font(family, 9.5f * s))
            using (var b = new SolidBrush(Accent))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(_statusText, f, b,
                    new RectangleF(0, Height - 50 * s, Width, 24 * s), fmt);
            }
        }

        private void DrawTriangle(Graphics g, float x, float y, float size, float scale)
        {
            float w = size;
            float h = size * 0.9f;

            using var path = new GraphicsPath();
            float r = 10 * scale;

            // Треугольник со скруглением
            path.AddLine(x + w / 2, y, x + w / 2 + r, y + r * 0.5f);
            path.AddLine(x + w / 2 + r, y + r * 0.5f, x + w - r, y + h - r);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddLine(x + w - r, y + h - r, x + r, y + h - r);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.AddLine(x + r, y + h - r, x + w / 2 - r, y + r * 0.5f);
            path.CloseFigure();

            using (var brush = new LinearGradientBrush(
                new RectangleF(x, y, w, h), ShellTop, ShellBottom, 90f))
                g.FillPath(brush, path);

            using (var pen = new Pen(ShellEdge, 2f * scale))
                g.DrawPath(pen, path);

            // Буквы DT
            var family = SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif;
            using var font = new Font(family, size / 2.4f, FontStyle.Bold);
            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using var textBrush = new SolidBrush(Color.White);

            var textRect = new RectangleF(x, y + h * 0.35f, w, h * 0.5f);
            g.DrawString("DT", font, textBrush, textRect, sf);
        }
    }
}
