using System;
using System.Diagnostics;
using System.Threading;

namespace DarknessTool
{
    /// <summary>
    /// Управление watchdog-процессом из основного приложения.
    /// </summary>
    public static class WatchdogHelper
    {
        private static EventWaitHandle? _stopEvent;
        private static Process? _watchdogProcess;
        private static bool _started = false;

        public static void StartWatchdog()
        {
            if (_started) return;
            _started = true;

            try
            {
                int myPid = Environment.ProcessId;

                // Событие, по которому watchdog поймёт, что можно завершаться
                _stopEvent = new EventWaitHandle(
                    false, EventResetMode.ManualReset,
                    WatchdogRunner.EventPrefix + myPid);

                string exePath = Environment.ProcessPath ?? "";
                if (string.IsNullOrEmpty(exePath)) return;

                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "--watchdog " + myPid,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                _watchdogProcess = Process.Start(psi);
            }
            catch
            {
                // Не удалось запустить watchdog — работаем без него.
            }
        }

        public static void StopWatchdog()
        {
            try
            {
                _stopEvent?.Set();
            }
            catch { }

            try
            {
                // Дадим watchdog 500 мс, чтобы он завершился сам
                Thread.Sleep(300);
                if (_watchdogProcess != null && !_watchdogProcess.HasExited)
                {
                    _watchdogProcess.Kill();
                }
            }
            catch { }

            try { _stopEvent?.Dispose(); } catch { }
            _stopEvent = null;
            _watchdogProcess = null;
        }
    }
}
