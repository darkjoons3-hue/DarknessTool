using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace DarknessTool
{
    public class AboutForm : Form
    {
        private const string RepoUrl = "https://github.com/darkjoons3-hue/DarknessTool1.0ver";

        private static readonly Color BgTop       = Color.FromArgb(0x0A, 0x14, 0x28);
        private static readonly Color BgBottom    = Color.FromArgb(0x05, 0x0A, 0x14);
        private static readonly Color TextPrimary = Color.FromArgb(0xE8, 0xE8, 0xE8);
        private static readonly Color TextSecond  = Color.FromArgb(0x88, 0x99, 0xAA);
        private static readonly Color Accent      = Color.FromArgb(0x4A, 0x9E, 0xFF);
        private static readonly Color BtnBg       = Color.FromArgb(0x14, 0x26, 0x42);
        private static readonly Color BtnHover    = Color.FromArgb(0x1E, 0x3A, 0x5F);

        public AboutForm()
        {
            Text = "О программе — DarknessTool";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            BackColor = BgTop;
            ForeColor = TextPrimary;
            Font = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
            ClientSize = new Size(520, 480);

            BuildUi();
        }

        private void BuildUi()
        {
            var logo = new LogoPanel
            {
                Dock = DockStyle.Top,
                Height = 140
            };

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            string verStr = version != null
                ? $"{version.Major}.{version.Minor}.{version.Build}"
                : "1.0.0";

            var title = new Label
            {
                Text = "DarknessTool " + verStr,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font.FontFamily, 15f, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            var subtitle = new Label
            {
                Text = "Восстановление Windows после заражения и снятия блокировок",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font(Font.FontFamily, 9f),
                ForeColor = TextSecond,
                BackColor = Color.Transparent
            };

            var author = new Label
            {
                Text = "Автор: darkjoons3-hue  ·  Лицензия: MIT",
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font.FontFamily, 8.5f),
                ForeColor = TextSecond,
                BackColor = Color.Transparent
            };

            var buttons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                Padding = new Padding(40, 12, 40, 12),
                BackColor = Color.Transparent
            };

            buttons.Controls.Add(MakeButton("🐞  Сообщить о баге", () => OpenUrl(RepoUrl + "/issues/new?title=Баг&body=**Описание:**%0A%0A**Шаги воспроизведения:**%0A%0A**Ожидалось:**%0A%0A**Получил:**%0A%0A" + GetSystemInfo())));
            buttons.Controls.Add(MakeButton("💡  Предложить идею", () => OpenUrl(RepoUrl + "/issues/new?title=Идея&body=**Что хочу:**%0A%0A**Зачем:**%0A%0A")));
            buttons.Controls.Add(MakeButton("📋  Скопировать инфо о системе", CopySystemInfo));
            buttons.Controls.Add(MakeButton("🔗  Открыть репозиторий", () => OpenUrl(RepoUrl)));

            var closeBtn = new Button
            {
                Text = "Закрыть",
                Dock = DockStyle.Bottom,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = BtnBg,
                ForeColor = TextPrimary,
                FlatAppearance = { BorderSize = 0 }
            };
            closeBtn.Click += (s, e) => Close();

            Controls.Add(buttons);
            Controls.Add(author);
            Controls.Add(subtitle);
            Controls.Add(title);
            Controls.Add(logo);
            Controls.Add(closeBtn);
        }

        private Button MakeButton(string text, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 40,
                Margin = new Padding(0, 4, 0, 4),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = BtnBg,
                ForeColor = TextPrimary,
                Font = new Font(Font.FontFamily, 10f),
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = BtnHover }
            };
            btn.Click += (s, e) =>
            {
                try { onClick(); }
                catch (Exception ex) { MessageBox.Show("Не удалось: " + ex.Message, "DarknessTool"); }
            };
            return btn;
        }

        private static void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        }

        private void CopySystemInfo()
        {
            try
            {
                Clipboard.SetText(GetSystemInfo());
                MessageBox.Show("Информация о системе скопирована в буфер обмена.\n\nМожешь приложить её к багрепорту.",
                    "DarknessTool", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось скопировать: " + ex.Message, "DarknessTool");
            }
        }

        private static string GetSystemInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== DarknessTool — информация о системе ===");
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            sb.AppendLine($"DarknessTool: {(ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0")}");
            sb.AppendLine($"ОС: {Environment.OSVersion}");
            sb.AppendLine($"Архитектура: {RuntimeInformation.OSArchitecture}");
            sb.AppendLine($".NET: {Environment.Version}");
            sb.AppendLine($"Права: {(IsAdmin() ? "Администратор" : "Пользователь")}");
            sb.AppendLine($"Режим: {(IsWinRE() ? "WinRE" : "Windows")}");
            sb.AppendLine($"Системный диск: {Environment.SystemDirectory}");
            sb.AppendLine("==========================================");
            return sb.ToString();
        }

        private static bool IsAdmin()
        {
            using var id = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool IsWinRE()
        {
            try
            {
                var sys = Environment.SystemDirectory ?? "";
                return sys.StartsWith("X:", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(
                ClientRectangle, BgTop, BgBottom, 90f);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        private class LogoPanel : Panel
        {
            public LogoPanel() { DoubleBuffered = true; BackColor = Color.Transparent; }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                float s = DeviceDpi / 96f;
                float size = 90 * s;
                float x = (Width - size) / 2f;
                float y = (Height - size) / 2f + 5 * s;

                using var path = new GraphicsPath();
                float r = 10 * s;
                float w = size, h = size * 0.9f;

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

                using (var p = new Pen(Color.FromArgb(0x4A, 0x9E, 0xFF), 2f * s))
                    g.DrawPath(p, path);

                var family = SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif;
                using var font = new Font(family, size / 2.4f, FontStyle.Bold);
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using var textBrush = new SolidBrush(Color.White);
                g.DrawString("DT", font, textBrush,
                    new RectangleF(x, y + h * 0.35f, w, h * 0.5f), sf);
            }
        }
    }
}
