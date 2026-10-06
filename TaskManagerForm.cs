using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DarknessTool
{
    public class TaskManagerForm : Form
    {
        private DataGridView _grid = null!;
        private Label _status = null!;
        private Timer _autoTimer = null!;
        private bool _autoUpdate = false;
        private List<ProcessInfo> _processes = new();

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
        private static readonly Color BorderCol   = Color.FromArgb(0x1A, 0x2A, 0x44);

        public TaskManagerForm()
        {
            Text = "Диспетчер задач — " + WindowMasker.CurrentTitle;
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            BackColor = BgTop;
            ForeColor = TextPrimary;
            Font = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
            ClientSize = new Size(980, 600);
            MinimumSize = new Size(700, 420);

            BuildUi();
            RefreshList();

            _autoTimer = new Timer { Interval = 2000 };
            _autoTimer.Tick += (s, e) => { if (_autoUpdate) RefreshList(); };
        }

        private void BuildUi()
        {
            // Верхняя панель
            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = PanelBg,
                Padding = new Padding(12, 10, 12, 10)
            };

            var refreshBtn = MakeButton("⟳  Обновить", BtnBg, TextPrimary);
            refreshBtn.Width = 130;
            refreshBtn.Click += (s, e) => RefreshList();

            var autoBtn = MakeButton("Авто", BtnBg, TextPrimary);
            autoBtn.Width = 80;
            autoBtn.Click += (s, e) =>
            {
                _autoUpdate = !_autoUpdate;
                autoBtn.BackColor = _autoUpdate ? Accent : BtnBg;
                autoBtn.ForeColor = _autoUpdate ? Color.White : TextPrimary;
                if (_autoUpdate) _autoTimer.Start(); else _autoTimer.Stop();
            };

            var killBtn = MakeButton("✖  Завершить", Danger, Color.White);
            killBtn.Width = 150;
            killBtn.Click += (s, e) => KillSelected();

            var suspendBtn = MakeButton("❚❚  Заморозить", BtnBg, TextPrimary);
            suspendBtn.Width = 150;
            suspendBtn.Click += (s, e) => SuspendSelected();

            var resumeBtn = MakeButton("▶  Разморозить", BtnBg, TextPrimary);
            resumeBtn.Width = 150;
            resumeBtn.Click += (s, e) => ResumeSelected();

            var propsBtn = MakeButton("ⓘ  Свойства", BtnBg, TextPrimary);
            propsBtn.Width = 130;
            propsBtn.Click += (s, e) => ShowProperties();

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                WrapContents = false,
                AutoScroll = true
            };
            flow.Controls.Add(refreshBtn);
            flow.Controls.Add(autoBtn);
            flow.Controls.Add(killBtn);
            flow.Controls.Add(suspendBtn);
            flow.Controls.Add(resumeBtn);
            flow.Controls.Add(propsBtn);

            top.Controls.Add(flow);

            // Таблица
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
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);

            _grid.DefaultCellStyle.BackColor = GridBg;
            _grid.DefaultCellStyle.ForeColor = TextPrimary;
            _grid.DefaultCellStyle.SelectionBackColor = GridSelBg;
            _grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            _grid.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);

            _grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltBg;
            _grid.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
            _grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = GridSelBg;

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pid",     HeaderText = "PID",          FillWeight = 8 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name",    HeaderText = "Имя",          FillWeight = 22 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "User",    HeaderText = "Пользователь", FillWeight = 15 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cpu",     HeaderText = "CPU %",        FillWeight = 8 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ram",     HeaderText = "RAM",          FillWeight = 10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Threads", HeaderText = "Потоки",       FillWeight = 7 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Path",    HeaderText = "Путь",         FillWeight = 30 });

            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ShowProperties(); };

            // Нижняя строка статуса
            _status = new Label
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
            Controls.Add(_status);
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
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 8, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = BtnHover;
            return btn;
        }

        private void RefreshList()
        {
            try
            {
                _processes = ProcessListProvider.GetProcesses()
                    .OrderByDescending(p => p.CpuPercent)
                    .ThenBy(p => p.Name)
                    .ToList();

                int? selectedPid = GetSelectedPid();

                _grid.SuspendLayout();
                _grid.Rows.Clear();

                foreach (var p in _processes)
                {
                    _grid.Rows.Add(
                        p.Pid,
                        p.Name,
                        p.UserName,
                        p.CpuDisplay,
                        p.MemoryDisplay,
                        p.ThreadsCount,
                        p.FilePath);
                }

                _grid.ResumeLayout();

                if (selectedPid.HasValue)
                {
                    foreach (DataGridViewRow row in _grid.Rows)
                    {
                        if (row.Cells["Pid"].Value?.ToString() == selectedPid.Value.ToString())
                        {
                            row.Selected = true;
                            break;
                        }
                    }
                }

                int total = _processes.Count;
                double totalRam = _processes.Sum(p => p.WorkingSetBytes) / 1024.0 / 1024;
                _status.Text = $"  Процессов: {total}   ·   Всего RAM: {totalRam:0} МБ   ·   Последнее обновление: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                _status.Text = "  Ошибка обновления: " + ex.Message;
            }
        }

        private int? GetSelectedPid()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            var val = _grid.SelectedRows[0].Cells["Pid"].Value?.ToString();
            return int.TryParse(val, out int pid) ? pid : (int?)null;
        }

        private ProcessInfo? GetSelectedProcess()
        {
            var pid = GetSelectedPid();
            if (!pid.HasValue) return null;
            return _processes.FirstOrDefault(p => p.Pid == pid.Value);
        }

        private void KillSelected()
        {
            var p = GetSelectedProcess();
            if (p == null)
            {
                MessageBox.Show("Выбери процесс в таблице.", "Диспетчер задач",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ok = ConfirmDialog.Ask(this,
                "Завершить процесс?",
                $"Процесс: {p.Name} (PID {p.Pid})\nПуть: {p.FilePath}\n\n" +
                "Если это системный процесс — система может стать нестабильной.",
                dangerous: true,
                okText: "Завершить");
            if (!ok) return;

            bool success = ProcessListProvider.KillProcess(p.Pid);
            if (success)
            {
                ChangeLog.Append(new ChangeEntry
                {
                    Type = ChangeType.Other,
                    Target = $"{p.Name} (PID {p.Pid})",
                    Description = "Процесс завершён",
                    CanUndo = false
                });
                RefreshList();
            }
            else
            {
                MessageBox.Show("Не удалось завершить процесс.\nВозможно, он защищён системой или антивирусом.",
                    "Диспетчер задач", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SuspendSelected()
        {
            var p = GetSelectedProcess();
            if (p == null) return;
            if (ProcessListProvider.SuspendProcess(p.Pid))
                _status.Text = $"  Процесс {p.Name} заморожен.";
            else
                MessageBox.Show("Не удалось заморозить процесс.", "Диспетчер задач",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ResumeSelected()
        {
            var p = GetSelectedProcess();
            if (p == null) return;
            if (ProcessListProvider.ResumeProcess(p.Pid))
                _status.Text = $"  Процесс {p.Name} разморожен.";
            else
                MessageBox.Show("Не удалось разморозить процесс.", "Диспетчер задач",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowProperties()
        {
            var p = GetSelectedProcess();
            if (p == null) return;

            var msg =
                $"Имя: {p.Name}\n" +
                $"PID: {p.Pid}\n" +
                $"Пользователь: {(string.IsNullOrEmpty(p.UserName) ? "—" : p.UserName)}\n" +
                $"CPU: {p.CpuDisplay} %\n" +
                $"RAM: {p.MemoryDisplay}\n" +
                $"Потоки: {p.ThreadsCount}\n" +
                $"Старт: {p.StartDisplay}\n\n" +
                $"Путь:\n{p.FilePath}";

            MessageBox.Show(msg, "Свойства процесса — " + p.Name,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(
                ClientRectangle, BgTop, BgBottom, 90f);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }
}
