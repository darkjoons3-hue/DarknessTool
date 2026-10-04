using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DarknessTool
{
    public class TileControl : Control
    {
        private bool _hover;
        private readonly bool _dangerous;
        private readonly string _icon;
        private readonly string _title;
        private readonly string _subtitle;

        private static readonly Color TileBg      = Color.FromArgb(230, 0x14, 0x26, 0x42);
        private static readonly Color TileHover   = Color.FromArgb(255, 0x1E, 0x3A, 0x5F);
        private static readonly Color Border      = Color.FromArgb(0x1A, 0x2A, 0x44);
        private static readonly Color BorderHover = Color.FromArgb(0x4A, 0x9E, 0xFF);
        private static readonly Color TextPrimary = Color.FromArgb(0xE8, 0xE8, 0xE8);
        private static readonly Color TextSecond  = Color.FromArgb(0x88, 0x99, 0xAA);
        private static readonly Color Danger      = Color.FromArgb(0xD1, 0x34, 0x38);

        // Базовая логическая высота плитки в единицах (при 96 DPI = 90 px)
        public const int LogicalHeight = 90;

        public TileControl(string icon, string title, string subtitle, bool dangerous)
        {
            _icon = icon;
            _title = title;
            _subtitle = subtitle;
            _dangerous = dangerous;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            // Высота будет пересчитана в OnHandleCreated
        }

        private float DpiScale => DeviceDpi / 96f;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyDpiScaling();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            ApplyDpiScaling();
            Invalidate();
        }

        private void ApplyDpiScaling()
        {
            Height = (int)Math.Round(LogicalHeight * DpiScale);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            float s = DpiScale;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = (int)Math.Round(8 * s);
            using var path = RoundedRect(rect, radius);

            using (var bg = new SolidBrush(_hover ? TileHover : TileBg))
                g.FillPath(bg, path);

            if (_dangerous)
            {
                int barW = (int)Math.Round(4 * s);
                using var bar = new SolidBrush(Danger);
                using var barPath = RoundedRect(new Rectangle(0, 0, barW, Height - 1), barW / 2);
                g.FillPath(bar, barPath);
            }

            using (var pen = new Pen(_hover ? BorderHover : Border, 1f))
                g.DrawPath(pen, path);

            var family = SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif;

            // Иконка
            float iconSize = 20f * s;
            float iconX = 14 * s;
            float iconY = Height / 2f - iconSize * 1.1f;
            using (var iconFont = new Font(family, iconSize))
            using (var iconBrush = new SolidBrush(TextPrimary))
                g.DrawString(_icon, iconFont, iconBrush, new PointF(iconX, iconY));

            // Заголовок
            float titleSize = 11f * s;
            float textX = 70 * s;
            float titleY = 14 * s;
            using (var titleFont = new Font(family, titleSize, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(TextPrimary))
                g.DrawString(_title, titleFont, titleBrush, new PointF(textX, titleY));

            // Подпись
            float subSize = 8.5f * s;
            float subY = 42 * s;
            float subW = Width - textX - 10 * s;
            float subH = Height - subY - 8 * s;
            if (subW > 0 && subH > 0)
            {
                using var subFont = new Font(family, subSize);
                using var subBrush = new SolidBrush(TextSecond);
                g.DrawString(_subtitle, subFont, subBrush,
                    new RectangleF(textX, subY, subW, subH));
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (d <= 0) { path.AddRectangle(r); return path; }
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
