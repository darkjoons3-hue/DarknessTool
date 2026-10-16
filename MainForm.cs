using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace DarknessTool
{
    public class MainForm : Form
    {
        private Bitmap? _backgroundCache;
        private TableLayoutPanel _tiles = null!;
        private Label _title = null!;
        private Label _subtitle = null!;
        private Label _status = null!;

        private Rectangle _btnSettings, _btnAbout, _btnLog;
        private int _hoverBtn = -1;

        private TrayIcon? _tray;
        private bool _forceClose = false;

        private static readonly Color BgTop       = Color.FromArgb(0x0A, 0x14, 0x28);
        private static readonly Color BgBottom    = Color.FromArgb(0x05, 0x0A, 0x14);
        private static readonly Color GridColor   = Color.FromArgb(13, 0x1A, 0x2A, 0x44);
        private static readonly Color TriColor    = Color.FromArgb(0x1E, 0x3A, 0x5F);
        private static readonly Color TriAccent   = Color.FromArgb(0x0E, 0x63, 0x9C);
        private static readonly Color TextPrimary = Color.FromArgb(0xE8, 0xE8, 0xE8);
        private static readonly Color TextSecond  = Color.FromArgb(0x88, 0x99, 0xAA);
        private static readonly Color StatusBg    = Color.FromArgb(0x0F, 0x1E, 0x33);
        private static readonly Color BtnTopBg    = Color.FromArgb(0x14, 0x26, 0x42);
        private static readonly Color BtnTopHover = Color.FromArgb(0x1E, 0x3A, 0x5F);

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                try
                {
                    if (!string.IsNullOrEmpty(WindowMasker.CurrentClassName))
                        cp.ClassName = WindowMasker.CurrentClassName;
                }
                catch { }
                return cp;
            }
        }

        public MainForm() : this(false) { }

        public MainForm(bool restarted)
        {
            try { WindowMasker.GenerateClassName(); } catch { }

            try { Text = WindowMasker.GenerateTitle(true); }
            catch { Text = "Runtime Host"; }

            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            BackColor = BgTop;
            ForeColor = TextPrimary;
            Font = SafeFont.Make(9f);
            MinimumSize = new Size(600, 480);

            try { BackupManager.EnsureDirs(); } catch { }

            BuildUi();
            ApplyDpiScaling();

            try
            {
                var wa = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1024, 768);
                int targetW = Math.Min(LogicalToDeviceUnits(920), Math.Max(600, wa.Width - 20));
                int targetH = Math.Min(LogicalToDeviceUnits(760), Math.Max(480, wa.Height - 20));
                Size = new Size(targetW, targetH);
            }
            catch { Size = new Size(920, 760); }

            Load += (s, e) =>
            {
                try { _tray = new TrayIcon(this); }
                catch (Exception ex) { StartupLog.WriteException("TrayIcon", ex); }

                try
                {
                    var integrity = IntegrityChecker.Check();
                    IntegrityChecker.ShowIfProblems(integrity);
                }
                catch (Exception ex) { StartupLog.WriteException("IntegrityChecker", ex); }

                if (restarted)
                {
                    try
                    {
                        _tray?.ShowBalloon(
                            "⚠️ Программа была закрыта неожиданно",
                            "Возможно, malware пытался её отключить. DarknessTool перезапущен.");
                    }
                    catch { }
                }
            };
        }

        public void ForceClose()
        {
            _forceClose = true;
            try { _tray?.Dispose(); } catch { }
            _tray = null;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_forceClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                try
                {
                    _tray?.ShowBalloon(
                        "DarknessTool продолжает работать",
                        "Программа свёрнута в трей. Двойной клик по иконке — развернуть.");
                }
                catch { }
                return;
            }
            base.OnFormClosing(e);
        }

        private float DpiScale
        {
            get
            {
                try { return DeviceDpi / 96f; }
                catch { return 1f; }
            }
        }

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            _backgroundCache?.Dispose();
            _backgroundCache = null;
            ApplyDpiScaling();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateButtonRects();
            Invalidate();
        }

        private void UpdateButtonRects()
        {
            try
            {
                float s = DpiScale;
                int size = (int)Math.Round(30 * s);
                int pad  = (int)Math.Round(8 * s);
                int y    = (int)Math.Round(10 * s);

                _btnLog      = new Rectangle(Width - pad - size, y, size, size);
                _btnAbout    = new Rectangle(_btnLog.X - pad - size, y, size, size);
                _btnSettings = new Rectangle(_btnAbout.X - pad - size, y, size, size);
            }
            catch { }
        }

        private void ApplyDpiScaling()
        {
            try
            {
                float s = DpiScale;

                _title.Font = SafeFont.Make(20f * s, FontStyle.Bold);
                _title.Height = (int)Math.Round(70 * s);

                _subtitle.Font = SafeFont.Make(9f * s);
                _subtitle.Height = (int)Math.Round(26 * s);

                _status.Font = SafeFont.Make(8.5f * s);
                _status.Height = (int)Math.Round(28 * s);
                _status.Padding = new Padding((int)Math.Round(14 * s), 0, 0, 0);

                _tiles.Padding = new Padding(
                    (int)Math.Round(20 * s), (int)Math.Round(6 * s),
                    (int)Math.Round(20 * s), (int)Math.Round(6 * s));

                foreach (Control c in _tiles.Controls)
                    c.Margin = new Padding((int)Math.Round(6 * s));

                UpdateButtonRects();
            }
            catch { }
        }

        private void BuildUi()
        {
            _title = new Label
            {
                Text = "D A R K N E S S T O O L",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextPrimary,
                BackColor = BgTop
            };

            _subtitle = new Label
            {
                Text = "версия " + GetVersionString(),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextSecond,
                BackColor = BgTop
            };

            _tiles = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = BgTop,
                AutoScroll = true,
                GrowStyle = TableLayoutPanelGrowStyle.AddRows
            };
            _tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            _tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            AddTile("🧩", "Диспетчер задач",   "Процессы, службы, автозагрузка", false);
            AddTile("🗝", "Редактор реестра",   "Просмотр, правка, оффлайн-кусты", false);
            AddTile("🗂", "Оффлайн-доступы",    "Службы, драйверы, задачи, профили", false);
            AddTile("🔍", "Сканер закрепа",    "Проверка типовых мест malware", true);
            AddTile("🔓", "Мастер снятия",     "Разблокировка ограничений Windows", true);
            AddTile("📊", "Сравнение реестра", "Diff до/после заражения", false);
            AddTile("🔤", "Шрифты системы",    "Сброс и восстановление", false);
            AddTile("🔑", "Замена Utilman",    "Точка входа для восстановления", false);
            AddTile("🗄", "Карантин",           "Безопасное хранилище", false);
            AddTile("🧪", "Скрипты",           "Правила PowerShell", false);
            AddTile("📖", "Справка",           "Документация и руководства", false);
            AddTile("⚙", "Настройки",         "Тема, VT, пути", false);

            _status = new Label
            {
                Dock = DockStyle.Bottom,
                Text = BuildStatusText(),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = TextSecond,
                BackColor = StatusBg,
                AutoEllipsis = true
            };

            Controls.Add(_tiles);
            Controls.Add(_subtitle);
            Controls.Add(_title);
            Controls.Add(_status);

            MouseMove += MainForm_MouseMove;
            MouseClick += MainForm_MouseClick;
        }

        private static string GetVersionString()
        {
            try
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                if (v == null) return "1.2.0";
                return $"{v.Major}.{v.Minor}.{v.Build}";
            }
            catch { return "1.2.0"; }
        }

        private void AddTile(string icon, string title, string subtitle, bool dangerous)
        {
            try
            {
                var tile = new TileControl(icon, title, subtitle, dangerous)
                {
                    Dock = DockStyle.Fill
                };
                tile.Click += (s, e) =>
                {
                    if (title == "Диспетчер задач")
                    {
                        try { new TaskManagerForm().ShowDialog(this); }
                        catch (Exception ex)
                        {
                            StartupLog.WriteException("Open TaskManager", ex);
                            MessageBox.Show("Не удалось открыть диспетчер: " + ex.Message,
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        try
                        {
                            MessageBox.Show(
                                $"Раздел «{title}» пока в разработке.",
                                "DarknessTool",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        catch { }
                    }
                };

                _tiles.Controls.Add(tile);
            }
            catch (Exception ex)
            {
                StartupLog.WriteException("AddTile " + title, ex);
            }
        }

        private static string BuildStatusText()
        {
            try
            {
                bool admin = IsAdmin();
                string adminMark = admin ? "● да" : "● нет";
                string env = IsWinRE() ? "WinRE" : "Windows";
                int qCount = 0;
                try { qCount = BackupManager.GetQuarantineCount(); } catch { }
                return $"  Админ: {adminMark}      Среда: {env}      VT: не настроен      Карантин: {qCount} файл(ов)      v{GetVersionString()}";
            }
            catch { return "  Готов"; }
        }

        private static bool IsAdmin()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
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

        private void MainForm_MouseMove(object? sender, MouseEventArgs e)
        {
            try
            {
                int prev = _hoverBtn;
                _hoverBtn = -1;
                if (_btnSettings.Contains(e.Location)) _hoverBtn = 0;
                else if (_btnAbout.Contains(e.Location)) _hoverBtn = 1;
                else if (_btnLog.Contains(e.Location)) _hoverBtn = 2;

                Cursor = _hoverBtn >= 0 ? Cursors.Hand : Cursors.Default;

                if (prev != _hoverBtn)
                    Invalidate(new Rectangle(_btnSettings.X - 4, _btnSettings.Y - 4,
                        (_btnLog.Right - _btnSettings.X) + 8, _btnSettings.Height + 8));
            }
            catch { }
        }

        private void MainForm_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            try
            {
                if (_btnSettings.Contains(e.Location))
                {
                    MessageBox.Show("Настройки пока в разработке.", "DarknessTool");
                }
                else if (_btnAbout.Contains(e.Location))
                {
                    try { new AboutForm().ShowDialog(this); } catch (Exception ex) { StartupLog.WriteException("AboutForm", ex); }
                }
                else if (_btnLog.Contains(e.Location))
                {
                    try
                    {
                        new LogForm().ShowDialog(this);
                        _status.Text = BuildStatusText();
                    }
                    catch (Exception ex) { StartupLog.WriteException("LogForm", ex); }
                }
            }
            catch (Exception ex)
            {
                StartupLog.WriteException("MainForm_MouseClick", ex);
            }
        }

        private void DrawCornerButtons(Graphics g)
        {
            try
            {
                float s = DpiScale;
                using var font = SafeFont.Make(11f * s);

                DrawCornerButton(g, _btnSettings, "⚙", _hoverBtn == 0, font, s);
                DrawCornerButton(g, _btnAbout,    "ℹ", _hoverBtn == 1, font, s);
                DrawCornerButton(g, _btnLog,      "📜", _hoverBtn == 2, font, s);
            }
            catch { }
        }

        private void DrawCornerButton(Graphics g, Rectangle r, string glyph, bool hover, Font font, float s)
        {
            try
            {
                int radius = (int)Math.Round(6 * s);
                using var path = RoundedRect(r, radius);

                using (var b = new SolidBrush(hover ? BtnTopHover : BtnTopBg))
                    g.FillPath(b, path);

                using (var pen = new Pen(hover
                    ? Color.FromArgb(0x4A, 0x9E, 0xFF)
                    : Color.FromArgb(0x1A, 0x2A, 0x44), 1f))
                    g.DrawPath(pen, path);

                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using var tb = new SolidBrush(TextPrimary);
                try
                {
                    g.DrawString(glyph, font, tb, r, sf);
                }
                catch
                {
                    try { g.FillEllipse(tb, r.X + r.Width / 2 - 3, r.Y + r.Height / 2 - 3, 6, 6); } catch { }
                }
            }
            catch { }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (d <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                if (_backgroundCache == null ||
                    _backgroundCache.Width != Width ||
                    _backgroundCache.Height != Height)
                {
                    _backgroundCache?.Dispose();
                    _backgroundCache = RenderBackground(Width, Height);
                }

                e.Graphics.DrawImageUnscaled(_backgroundCache, 0, 0);
            }
            catch
            {
                try { e.Graphics.Clear(BgTop); } catch { }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);
                DrawCornerButtons(e.Graphics);
            }
            catch { }
        }

        private static Bitmap RenderBackground(int w, int h)
        {
            if (w <= 0) w = 1;
            if (h <= 0) h = 1;

            var bmp = new Bitmap(w, h);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, w, h), BgTop, BgBottom, 90f))
                g.FillRectangle(brush, 0, 0, w, h);

            try
            {
                using var pen = new Pen(GridColor, 1f);
                for (int x = 0; x < w; x += 60) g.DrawLine(pen, x, 0, x, h);
                for (int y = 0; y < h; y += 60) g.DrawLine(pen, 0, y, w, y);
            }
            catch { }

            try
            {
                var rnd = new Random(20251004);
                for (int i = 0; i < 18; i++)
                {
                    int size = rnd.Next(40, 90);
                    int x = rnd.Next(0, Math.Max(1, w));
                    int y = rnd.Next(0, Math.Max(1, h));
                    int alpha = rnd.Next(8, 18);

                    var color = (i % 5 == 0)
                        ? Color.FromArgb(alpha, TriAccent.R, TriAccent.G, TriAccent.B)
                        : Color.FromArgb(alpha, TriColor.R, TriColor.G, TriColor.B);

                    using var brush = new SolidBrush(color);
                    var pts = new[]
                    {
                        new Point(x, y),
                        new Point(x + size, y + size / 3),
                        new Point(x + size / 3, y + size)
                    };
                    g.FillPolygon(brush, pts);
                }
            }
            catch { }

            return bmp;
        }
    }
}
