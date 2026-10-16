using System;
using System.IO;
using System.Text;

namespace DarknessTool
{
    public static class StartupLog
    {
        public static string LogPath =>
            Path.Combine(Path.GetTempPath(), "WinRtCache_start.log");

        private static readonly object _lock = new object();

        public static void Write(string message)
        {
            lock (_lock)
            {
                try
                {
                    var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\r\n";
                    File.AppendAllText(LogPath, line, Encoding.UTF8);
                }
                catch { }
            }
        }

        public static void WriteHeader()
        {
            lock (_lock)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine("============================================");
                    sb.AppendLine($"DarknessTool startup — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine("============================================");

                    try { sb.AppendLine($"OS: {Environment.OSVersion}"); } catch { }
                    try { sb.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}"); } catch { }
                    try { sb.AppendLine($"64-bit process: {Environment.Is64BitProcess}"); } catch { }
                    try { sb.AppendLine($".NET: {Environment.Version}"); } catch { }
                    try { sb.AppendLine($"CPU cores: {Environment.ProcessorCount}"); } catch { }
                    try { sb.AppendLine($"System dir: {Environment.SystemDirectory}"); } catch { }
                    try { sb.AppendLine($"Process path: {Environment.ProcessPath}"); } catch { }

                    try
                    {
                        sb.AppendLine($"Base dir: {PathHelper.BaseDir}");
                        sb.AppendLine($"Base exists: {Directory.Exists(PathHelper.BaseDir)}");
                    }
                    catch { }

                    try
                    {
                        using var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                        sb.AppendLine($"Screen DPI: {g.DpiX} x {g.DpiY}");
                    }
                    catch { }

                    try
                    {
                        using var f = new System.Drawing.Font("Segoe UI", 9f);
                        sb.AppendLine("Segoe UI available: yes");
                    }
                    catch
                    {
                        sb.AppendLine("Segoe UI available: NO");
                    }

                    try
                    {
                        using var f = new System.Drawing.Font("Segoe UI Emoji", 9f);
                        sb.AppendLine("Segoe UI Emoji available: yes");
                    }
                    catch
                    {
                        sb.AppendLine("Segoe UI Emoji available: NO");
                    }

                    sb.AppendLine("============================================");
                    sb.AppendLine();

                    File.AppendAllText(LogPath, sb.ToString(), Encoding.UTF8);
                }
                catch { }
            }
        }

        public static void WriteException(string context, Exception ex)
        {
            Write($"ERROR in {context}: {ex.GetType().Name}: {ex.Message}");
            try
            {
                File.AppendAllText(LogPath, ex.StackTrace + "\r\n\r\n", Encoding.UTF8);
            }
            catch { }
        }
    }
}
