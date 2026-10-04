using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DarknessTool
{
    public class ConfirmDialog : Form
    {
        private static readonly Color BgTop       = Color.FromArgb(0x0A, 0x14, 0x28);
        private static readonly Color BgBottom    = Color.FromArgb(0x05, 0x0A, 0x14);
        private static readonly Color TextPrimary = Color.FromArgb(0xE8, 0xE8, 0xE8);
        private static readonly Color TextSecond  = Color.FromArgb(0x88, 0x99, 0xAA);
        private static readonly Color BtnBg       = Color.FromArgb(0x14, 0x26, 0x42);
        private static readonly Color BtnHover    = Color.FromArgb(0x1E, 0x3A, 0x5F);
        private static readonly Color Accent      = Color.FromArgb(0x4A, 0x9E, 0xFF);
        private static readonly Color Danger      = Color.FromArgb(0xD1, 0x34, 0x38);
        private static readonly Color DangerHover = Color.FromArgb(0xE8, 0x48, 0x4C);
        private static readonly Color BorderCol   = Color.FromArgb(0x1A, 0x2A, 0x44);

        public static bool Ask(IWin32Window? owner, string title, string message,
                               bool dangerous = false,
                               string okText = "Продолжить",
                               string cancelText = "Отмена")
        {
            using var dlg = new ConfirmDialog(title, message, dangerous, okText, cancelText);
            return dlg.ShowDialog(owner) == DialogResult.OK;
        }

        private ConfirmDialog(string title, string message, bool dangerous,
                              string okText, string cancelText)
        {
            Text = "DarknessTool";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            BackColor = BgTop;
            ForeColor = TextPrimary;
            Font = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
            ClientSize = new Size(480, 220);
            Padding = new Padding(1);

            var titleLbl = new Label
            {
                Text = (dangerous ? "⚠  " : "") + title,
                Dock = DockStyle.Top,
                Height = 44,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 20, 0),
                Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
                ForeColor = dangerous ? Danger : TextPrimary,
                BackColor = Color.Transparent
            };

            var msgLbl = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 8, 20, 8),
                Font = new Font(Font.FontFamily, 9.5f),
                ForeColor = TextSecond,
                BackColor = Color.Transparent
            };

            var btnPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.Transparent
            };

            var okBtn = new Button
            {
                Text = okText,
                Size = new Size(140, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = dangerous ? Danger : Accent,
                ForeColor = Color.White,
                Font = new Font(Font.FontFamily, 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            okBtn.FlatAppearance.BorderSize = 0;
            okBtn.FlatAppearance.MouseOverBackColor = dangerous ? DangerHover : Color.FromArgb(0x5A, 0xAE, 0xFF);
            okBtn.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            var cancelBtn = new Button
            {
                Text = cancelText,
                Size = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = BtnBg,
                ForeColor = TextPrimary,
                Font = new Font(Font.FontFamily, 9.5f),
                Cursor = Cursors.Hand
            };
            cancelBtn.FlatAppearance.BorderSize = 0;
            cancelBtn.FlatAppearance.MouseOverBackColor = BtnHover;
            cancelBtn.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            btnPanel.Controls.Add(okBtn);
            btnPanel.Controls.Add(cancelBtn);

            void LayoutButtons()
            {
                int right = btnPanel.Width - 20;
                okBtn.Location = new Point(right - okBtn.Width, (btnPanel.Height - okBtn.Height) / 2);
                cancelBtn.Location = new Point(okBtn.Left - cancelBtn.Width - 10, (btnPanel.Height - cancelBtn.Height) / 2);
            }
            btnPanel.Resize += (s, e) => LayoutButtons();

            Controls.Add(msgLbl);
            Controls.Add(btnPanel);
            Controls.Add(titleLbl);

            // Ресайз под контент
            btnPanel.PerformLayout();
            btnPanel.Width = ClientSize.Width;
            LayoutButtons();

            AcceptButton = okBtn;
            CancelButton = cancelBtn;

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(BorderCol, 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(
                ClientRectangle, BgTop, BgBottom, 90f);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }
}
