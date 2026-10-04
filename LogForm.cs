using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DarknessTool
{
    public class LogForm : Form
    {
        private DataGridView _grid = null!;
        private Label _summary = null!;
        private List<ChangeEntry> _entries = new();

        private static readonly Color BgTop       = Color.FromArgb(0x0A, 0x14, 0x28);
        private static readonly Color BgBottom    = Color.FromArgb(0x05, 0x0A, 0x14);
        private static readonly Color PanelBg     = Color.FromArgb(0x0F, 0x1E, 0x33);
        private static readonly Color GridBg      = Color.FromArgb(0x14, 0x26, 0x42);
        private static readonly Color GridAltBg   = Color.FromArgb(0x10, 0x1F, 0x38);
        private static readonly Color GridSelBg   = Color.FromArgb(0x1E, 0x3A, 0x5F);
        private static readonly Color TextPrimary = Color.FromArgb(0xE8, 0xE8, 0xE8);
        private static readonly Color TextSecond  = Color.FromArgb(0x88, 0x99, 0xAA);
        private static readonly Color Accent      = Color.FromArgb(0x4A, 0x9E, 0xFF);
        private static readonly Color BtnBg       = Color.FromArgb(0x14, 0x26, 0x42);
        private static readonly Color BtnHover    = Color.FromArgb(0x1E, 0x3A, 0x5F);
        private static readonly Color Danger      = Color.FromArgb(0xD1, 0x34, 0x38);
        private static readonly Color Success     = Color.FromArgb(0x4C, 0xAF, 0x50);
        private static readonly Color BorderCol   = Color.FromArgb(0x1A, 0x2A, 0x44);

        public LogForm()
        {
            Text = "Журнал изменений — DarknessTool";
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            BackColor = BgTop;
            ForeColor = TextPrimary;
            Font = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
            ClientSize = new Size(880, 520);
            MinimumSize = new Size(700, 420);

            BuildUi();
            LoadEntries();
        }

        private void BuildUi()
        {
            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = PanelBg,
                Padding = new Padding(12, 8, 12, 8)
            };

            var undoBtn = MakeButton("↶  Отменить последнее", Accent, Color.White);
            undoBtn.Width = 200;
            undoBtn.Click += (s, e) => UndoLast();

            var restoreBtn = MakeButton("↺  Восстановить выбранное", BtnBg, TextPrimary);
            restoreBtn.Width = 220;
            restoreBtn.Margin = new Padding(8, 0, 0, 0);
            restoreBtn.Click += (s, e) => RestoreSelected();

            var refreshBtn = MakeButton("⟳", BtnBg, TextPrimary);
            refreshBtn.Width = 40;
            refreshBtn.Margin = new Padding(8, 0, 0, 0);
            refreshBtn.Click += (s, e) => LoadEntries();

            var clearBtn = MakeButton("🗑 Очистить журнал", BtnBg, TextPrimary);
            clearBtn.Width = 160;
            clearBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            clearBtn.Margin = new Padding(8, 0, 0, 0);
            clearBtn.Click += (s, e) => ClearLog();

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                WrapContents = false,
                AutoScroll = true
            };
            flow.Controls.Add(undoBtn);
            flow.Controls.Add(restoreBtn);
            flow.Controls.Add(refreshBtn);
            flow.Controls.Add(clearBtn);

            top.Controls.Add(flow);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = GridBg,
                BorderStyle = BorderStyle.None,
                GridColor = BorderCol,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 34,
                EnableHeadersVisualStyles = false,
                Font = new Font(Font.FontFamily, 9f)
            };

            _grid.ColumnHeadersDefaultCellStyle.BackColor = PanelBg;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = PanelBg;
            _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
            _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);

            _grid.DefaultCellStyle.BackColor = GridBg;
            _grid.DefaultCellStyle.ForeColor = TextPrimary;
            _grid.DefaultCellStyle.SelectionBackColor = GridSelBg;
            _grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            _grid.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);

            _grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltBg;
            _grid.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
            _grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = GridSelBg;

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time",  HeaderText = "Время",       FillWeight = 14 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type",  HeaderText = "Тип",         FillWeight = 18 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Desc",  HeaderText = "Описание",    FillWeight = 36 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Target",HeaderText = "Объект",      FillWeight = 26 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "State", HeaderText = "Статус",      FillWeight = 10 });

            _summary = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                ForeColor = TextSecond,
                BackColor = PanelBg,
                Font = new Font(Font.FontFamily, 8.5f)
            };

            Controls.Add(_grid);
            Controls.Add(_summary);
            Controls.Add(top);
        }

        private Button MakeButton(string text, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text = text,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font(Font.FontFamily, 9.5f),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = BtnHover;
            return btn;
        }

        private void LoadEntries()
        {
            _entries = ChangeLog.Load()
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            _grid.Rows.Clear();
            foreach (var e in _entries)
            {
                string state = e.Undone ? "отменено" : (e.CanUndo ? "можно откатить" : "—");
                int idx = _grid.Rows.Add(
                    e.Timestamp.ToString("dd.MM HH:mm:ss"),
                    e.TypeDisplay,
                    e.Description,
                    ShortTarget(e.Target),
                    state);

                if (e.Undone)
                    _grid.Rows[idx].DefaultCellStyle.ForeColor = TextSecond;
                else if (!e.CanUndo)
                    _grid.Rows[idx].DefaultCellStyle.ForeColor = TextSecond;
                else
                    _grid.Rows[idx].DefaultCellStyle.ForeColor = Success;
            }

            int total = _entries.Count;
            int undoable = _entries.Count(x => x.CanUndo && !x.Undone);
            long qsize = BackupManager.GetQuarantineSize();
            _summary.Text = $"  Всего записей: {total}   ·   Можно откатить: {undoable}   ·   Карантин: {BackupManager.GetQuarantineCount()} файлов, {FormatSize(qsize)}";
        }

        private static string ShortTarget(string target)
        {
            if (string.IsNullOrEmpty(target)) return "";
            if (target.Length <= 60) return target;
            return "…" + target.Substring(target.Length - 57);
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " Б";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.0") + " КБ";
            if (bytes < 1024L * 1024 * 1024) return (bytes / 1024.0 / 1024).ToString("0.0") + " МБ";
            return (bytes / 1024.0 / 1024 / 1024).ToString("0.00") + " ГБ";
        }

        private void UndoLast()
        {
            var last = ChangeLog.GetLastUndoable();
            if (last == null)
            {
                MessageBox.Show("Нет действий, которые можно отменить.", "DarknessTool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ok = ConfirmDialog.Ask(this,
                "Отменить последнее действие?",
                $"Тип: {last.TypeDisplay}\nОбъект: {last.Target}\n\nБудет выполнен откат к предыдущему состоянию.",
                dangerous: false,
                okText: "Отменить действие");

            if (!ok) return;

            bool success = BackupManager.Undo(last);
            if (success)
            {
                ChangeLog.MarkUndone(last.Id);
                MessageBox.Show("Действие отменено.", "DarknessTool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Не удалось отменить действие.\nПроверь, что бэкап существует и доступен.",
                    "DarknessTool", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            LoadEntries();
        }

        private void RestoreSelected()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выбери запись для восстановления.", "DarknessTool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idx = _grid.SelectedRows[0].Index;
            if (idx < 0 || idx >= _entries.Count) return;
            var entry = _entries[idx];

            if (entry.Undone)
            {
                MessageBox.Show("Эта запись уже отменена.", "DarknessTool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!entry.CanUndo)
            {
                MessageBox.Show("Эта запись не поддерживает откат.", "DarknessTool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ok = ConfirmDialog.Ask(this,
                "Восстановить выбранное?",
                $"{entry.TypeDisplay}\n{entry.Description}\n\nОбъект: {entry.Target}",
                dangerous: false,
                okText: "Восстановить");

            if (!ok) return;

            bool success = BackupManager.Undo(entry);
            if (success)
            {
                ChangeLog.MarkUndone(entry.Id);
                MessageBox.Show("Восстановлено.", "DarknessTool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Не удалось восстановить.", "DarknessTool",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            LoadEntries();
        }

        private void ClearLog()
        {
            var ok = ConfirmDialog.Ask(this,
                "Очистить журнал?",
                "Все записи будут удалены. Файлы из карантина и бэкапы реестра останутся на диске.\n\nПродолжить?",
                dangerous: true,
                okText: "Очистить");
            if (!ok) return;
            ChangeLog.Clear();
            LoadEntries();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(
                ClientRectangle, BgTop, BgBottom, 90f);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }
}
