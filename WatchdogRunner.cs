using System;
using System.Diagnostics;
using System.Threading;

namespace DarknessTool
{
    /// <summary>
    /// Watchdog-режим: следит за родительским процессом.
    /// Если родитель убит (некорректно завершён) — перезапускает его.
    /// </summary>
    public static class WatchdogRunner
    {
        public const string EventPrefix = @"Global\WinRtCache_";

        public static void Run(int parentPid)
        {
            try
            {
                string eventName = EventPrefix + parentPid;
                using var stopEvent = new EventWaitHandle(
                    false, EventResetMode.ManualReset, eventName);

                string exePath = Environment.ProcessPath ?? "";
                if (string.IsNullOrEmpty(exePath)) return;

                while (true)
                {
                    // Ждём сигнала о нормальном завершении — 2 секунды
                    if (stopEvent.WaitOne(2000))
                    {
                        // Родитель корректно закрылся
                        return;
                    }

                    // Проверяем, жив ли родитель
                    bool alive = false;
                    try
                    {
                        using var p = Process.GetProcessById(parentPid);
                        alive = !p.HasExited;
                    }
                    catch
                    {
                        alive = false;
                    }

                    if (!alive)
                    {
                        // Родитель убит — перезапускаем
                        try
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = exePath,
                                Arguments = "--restarted",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            };
                            Process.Start(psi);
                        }
                        catch { }
                        return;
                    }
                }
            }
            catch
            {
                // Если что-то пошло не так — просто выходим.
                // Лучше не сломать основной процесс, чем показать ошибку.
            }
        }
    }
}
