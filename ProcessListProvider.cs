using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace DarknessTool
{
    public static class ProcessListProvider
    {
        // Для CPU-счётчика: храним прошлый TotalProcessorTime
        private static readonly Dictionary<int, TimeSpan> _prevCpu = new();
        private static DateTime _prevSampleTime = DateTime.UtcNow;

        /// <summary>
        /// Получить список процессов с метриками.
        /// </summary>
        public static List<ProcessInfo> GetProcesses()
        {
            var list = new List<ProcessInfo>();
            int coreCount = Environment.ProcessorCount;
            var now = DateTime.UtcNow;
            var elapsed = (now - _prevSampleTime).TotalMilliseconds;
            if (elapsed <= 0) elapsed = 1;

            Process[] processes;
            try { processes = Process.GetProcesses(); }
            catch { return list; }

            foreach (var p in processes)
            {
                try
                {
                    var info = new ProcessInfo
                    {
                        Pid = p.Id,
                        Name = p.ProcessName,
                        ThreadsCount = TryGetThreads(p)
                    };

                    // CPU %
                    try
                    {
                        var cpuTime = p.TotalProcessorTime;
                        if (_prevCpu.TryGetValue(p.Id, out var prevCpu))
                        {
                            double deltaMs = (cpuTime - prevCpu).TotalMilliseconds;
                            if (deltaMs < 0) deltaMs = 0;
                            info.CpuPercent = (deltaMs / elapsed) * 100.0 / coreCount;
                        }
                        _prevCpu[p.Id] = cpuTime;
                    }
                    catch { info.CpuPercent = 0; }

                    // RAM
                    try { info.WorkingSetBytes = p.WorkingSet64; } catch { }

                    // Путь
                    try { info.FilePath = p.MainModule?.FileName ?? ""; } catch { }

                    // Пользователь (медленно, но надёжно)
                    info.UserName = GetProcessOwner(p.Id);

                    // Время старта
                    try { info.StartTime = p.StartTime; } catch { }

                    list.Add(info);
                }
                catch { }
                finally { p.Dispose(); }
            }

            _prevSampleTime = now;

            // Удаляем из кэша CPU те PID, которых больше нет
            var alive = new HashSet<int>();
            foreach (var item in list) alive.Add(item.Pid);
            var toRemove = new List<int>();
            foreach (var key in _prevCpu.Keys)
                if (!alive.Contains(key)) toRemove.Add(key);
            foreach (var key in toRemove) _prevCpu.Remove(key);

            return list;
        }

        private static int TryGetThreads(Process p)
        {
            try { return p.Threads.Count; }
            catch { return 0; }
        }

        /// <summary>
        /// Получить пользователя для процесса через WMI.
        /// </summary>
        public static string GetProcessOwner(int pid)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_Process WHERE ProcessId = {pid}");

                foreach (ManagementBaseObject baseObj in searcher.Get())
                {
                    if (baseObj is not ManagementObject obj) continue;

                    var outParams = obj.InvokeMethod("GetOwner", null, null);
                    if (outParams == null) continue;

                    var user = outParams["User"] as string ?? "";
                    var domain = outParams["Domain"] as string ?? "";
                    if (!string.IsNullOrEmpty(user))
                        return string.IsNullOrEmpty(domain) ? user : $"{domain}\\{user}";
                }
            }
            catch { }
            return "";
        }

        /// <summary>
        /// Завершить процесс.
        /// </summary>
        public static bool KillProcess(int pid, bool killTree = false)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                p.Kill(killTree);
                return true;
            }
            catch { return false; }
        }

        // ============ ЗАМОРОЗКА / РАЗМОРОЗКА ============

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSuspendProcess(IntPtr processHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtResumeProcess(IntPtr processHandle);

        public static bool SuspendProcess(int pid)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                int result = NtSuspendProcess(p.Handle);
                p.Dispose();
                return result == 0;
            }
            catch { return false; }
        }

        public static bool ResumeProcess(int pid)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                int result = NtResumeProcess(p.Handle);
                p.Dispose();
                return result == 0;
            }
            catch { return false; }
        }
    }
}
