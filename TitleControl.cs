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

        public TileControl(string icon, string title, string subtitle, bool dangerous)
        {
            _icon = icon;
            _title = title;
            _subtitle = subtitle;
            _dangerous = dangerous;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            Height = 90;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = RoundedRect(rect, 8);

            using (var bg = new SolidBrush(_hover ? TileHover : TileBg))
                g.FillPath(bg, path);

            if (_dangerous)
            {
                using var bar = new SolidBrush(Danger);
                using var barPath = RoundedRect(new Rectangle(0, 0, 4, Height - 1), 2);
                g.FillPath(bar, barPath);
            }

            using (var pen = new Pen(_hover ? BorderHover : Border, 1f))
                g.DrawPath(pen, path);

            var family = SystemFonts.MessageBoxFont?.FontFamily ?? FontFamily.GenericSansSerif;

            using (var iconFont = new Font(family, 20f))
            using (var iconBrush = new SolidBrush(TextPrimary))
                g.DrawString(_icon, iconFont, iconBrush, new PointF(14, Height / 2f - 22f));

            using (var titleFont = new Font(family, 11f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(TextPrimary))
                g.DrawString(_title, titleFont, titleBrush, new PointF(70, 14));

            using (var subFont = new Font(family, 8.5f))
            using (var subBrush = new SolidBrush(TextSecond))
                g.DrawString(_subtitle, subFont, subBrush,
                    new RectangleF(70, 42, Width - 80, Height - 50));
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
