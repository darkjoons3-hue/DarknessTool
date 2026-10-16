using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DarknessTool
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try { StartupLog.WriteHeader(); } catch { }

            // Режим watchdog
            if (args.Length >= 2 && args[0] == "--watchdog")
            {
                if (int.TryParse(args[1], out int parentPid))
                {
                    try { StartupLog.Write($"Starting watchdog for PID {parentPid}"); } catch { }
                    try { WatchdogRunner.Run(parentPid); } catch { }
                }
                return;
            }

            bool restarted = args.Contains("--restarted");

            if (!SingleInstance.TryAcquire())
            {
                try { StartupLog.Write("Single instance check: already running"); } catch { }
                if (!restarted)
                {
                    try
                    {
                        MessageBox.Show(
                            "DarknessTool уже запущен.\n\nПроверь панель задач и трей.",
                            "Runtime Host",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch { }
                }
                return;
            }

            try
            {
                try { Application.SetHighDpiMode(HighDpiMode.PerMonitorV2); } catch { }
                try { Application.EnableVisualStyles(); } catch { }
                try { Application.SetCompatibleTextRenderingDefault(false); } catch { }

                try { WatchdogHelper.StartWatchdog(); } catch { }

                try
                {
                    using (var splash = new SplashForm())
                    {
                        splash.ShowDialog();
                    }
                }
                catch (Exception ex) { StartupLog.WriteException("Splash", ex); }

                MainForm mainForm;
                try
                {
                    mainForm = new MainForm(restarted);
                }
                catch (Exception ex)
                {
                    StartupLog.WriteException("MainForm ctor", ex);
                    MessageBox.Show(
                        "Не удалось создать главное окно:\n\n" + ex.Message +
                        "\n\nЛог: " + StartupLog.LogPath,
                        "Ошибка запуска",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                StartupLog.WriteException("Main", ex);
                try
                {
                    MessageBox.Show(
                        "Произошла ошибка:\n\n" + ex.Message +
                        "\n\nЛог: " + StartupLog.LogPath,
                        "Runtime Host",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch { }
            }
            finally
            {
                try { WatchdogHelper.StopWatchdog(); } catch { }
                try { SingleInstance.Release(); } catch { }
                try { StartupLog.Write("Application exited"); } catch { }
            }
        }
    }
}
