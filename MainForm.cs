using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        private static readonly Color BgTop       = Color.FromArgb(0x0A, 0x14, 0x28);
        private static readonly Color BgBottom    = Color.FromArgb(0x05, 0x0A, 0x14);
        private static readonly Color GridColor   = Color.FromArgb(13, 0x1A, 0x2A, 0x44);
        private static readonly Color TriColor    = Color.FromArgb(0x1E, 0x3A, 0x5F);
        private static readonly Color TriAccent   = Color.FromArgb(0x0E, 0x63, 0x9C);
        private static readonly Color TextPrimary = Color.FromArgb(0xE8, 0xE8, 0xE8);
        private static readonly Color TextSecond  = Color.FromArgb(0x88, 0x99, 0xAA);
        private static readonly Color StatusBg    = Color.FromArgb(0x0F, 0x1E, 0x33);

        public MainForm()
        {
            Text = "DarknessTool 1.0";
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            BackColor = BgTop;
            ForeColor = TextPrimary;
            Font = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
            MinimumSize = new Size(600, 480);

            BuildUi();
            ApplyDpiScaling();

            // Начальный размер — с учётом DPI, но не больше рабочего стола
            var wa = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1024, 768);
            int targetW = Math.Min(LogicalToDeviceUnits(920), wa.Width - 20);
            int targetH = Math.Min(LogicalToDeviceUnits(760), wa.Height - 20);
            Size = new Size(targetW, targetH);
        }

        private float DpiScale => DeviceDpi / 96f;

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            _backgroundCache?.Dispose();
            _backgroundCache = null;
            ApplyDpiScaling();
            Invalidate();
        }

        private void ApplyDpiScaling()
        {
            float s = DpiScale;

            _title.Font = new Font(Font.FontFamily, 20f * s, FontStyle.Bold);
            _title.Height = (int)Math.Round(70 * s);

            _subtitle.Font = new Font(Font.FontFamily, 9f * s);
            _subtitle.Height = (int)Math.Round(26 * s);

            _status.Font = new Font(Font.FontFamily, 8.5f * s);
            _status.Height = (int)Math.Round(28 * s);
            _status.Padding = new Padding((int)Math.Round(14 * s), 0, 0, 0);

            _tiles.Padding = new Padding(
                (int)Math.Round(20 * s), (int)Math.Round(6 * s),
                (int)Math.Round(20 * s), (int)Math.Round(6 * s));

            foreach (Control c in _tiles.Controls)
            {
                c.Margin = new Padding((int)Math.Round(6 * s));
            }
        }

        private void BuildUi()
        {
            _title = new Label
            {
                Text = "D A R K N E S S T O O L",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            _subtitle = new Label
            {
                Text = "версия 1.0",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = TextSecond,
                BackColor = Color.Transparent
            };

            _tiles = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Color.Transparent,
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
        }

        private void AddTile(string icon, string title, string subtitle, bool dangerous)
        {
            var tile = new TileControl(icon, title, subtitle, dangerous)
            {
                Dock = DockStyle.Fill
            };
            tile.Click += (s, e) => MessageBox.Show(
                $"Раздел «{title}» пока в разработке.",
                "DarknessTool",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            _tiles.Controls.Add(tile);
        }

        private static string BuildStatusText()
        {
            bool admin = IsAdmin();
            string adminMark = admin ? "● да" : "● нет";
            return $"  Админ: {adminMark}      Среда: Windows      VT: не настроен      v1.0.0";
        }

        private static bool IsAdmin()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
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

        private static Bitmap RenderBackground(int w, int h)
        {
            if (w <= 0 || h <= 0) w = Math.Max(1, w);
            if (h <= 0) h = Math.Max(1, h);

            var bmp = new Bitmap(Math.Max(1, w), Math.Max(1, h));
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, w, h), BgTop, BgBottom, 90f))
                g.FillRectangle(brush, 0, 0, w, h);

            using (var pen = new Pen(GridColor, 1f))
            {
                for (int x = 0; x < w; x += 60) g.DrawLine(pen, x, 0, x, h);
                for (int y = 0; y < h; y += 60) g.DrawLine(pen, 0, y, w, y);
            }

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

            return bmp;
        }
    }
}
