using System;
using System.Diagnostics;

namespace DarknessTool
{
    public class ProcessInfo
    {
        public int Pid { get; set; }
        public string Name { get; set; } = "";
        public string UserName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string CommandLine { get; set; } = "";
        public long WorkingSetBytes { get; set; }
        public double CpuPercent { get; set; }
        public DateTime? StartTime { get; set; }
        public int ThreadsCount { get; set; }

        public string MemoryDisplay => FormatSize(WorkingSetBytes);
        public string CpuDisplay => CpuPercent.ToString("0.0");
        public string StartDisplay => StartTime?.ToString("dd.MM HH:mm:ss") ?? "—";

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " Б";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.0") + " КБ";
            if (bytes < 1024L * 1024 * 1024) return (bytes / 1024.0 / 1024).ToString("0.0") + " МБ";
            return (bytes / 1024.0 / 1024 / 1024).ToString("0.00") + " ГБ";
        }
    }
}
