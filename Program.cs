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
            // === Режим watchdog ===
            if (args.Length >= 2 && args[0] == "--watchdog")
            {
                if (int.TryParse(args[1], out int parentPid))
                {
                    WatchdogRunner.Run(parentPid);
                }
                return;
            }

            bool restarted = args.Contains("--restarted");

            // Проверка единственного экземпляра
            if (!SingleInstance.TryAcquire())
            {
                if (!restarted)
                {
                    MessageBox.Show(
                        "DarknessTool уже запущен.\n\nПроверь панель задач и трей.",
                        "Runtime Host",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                return;
            }

            try
            {
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Запускаем watchdog (не блокирует)
                WatchdogHelper.StartWatchdog();

                // Сплэш
                using (var splash = new SplashForm())
                {
                    splash.ShowDialog();
                }

                // Главное окно
                Application.Run(new MainForm(restarted));
            }
            catch (Exception ex)
            {
                try
                {
                    var log = Path.Combine(Path.GetTempPath(), "WinRtCache_error.log");
                    File.AppendAllText(log, $"[{DateTime.Now:O}] {ex}\n\n");
                }
                catch { }

                MessageBox.Show(
                    "Произошла ошибка:\n\n" + ex.Message,
                    "Runtime Host",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                WatchdogHelper.StopWatchdog();
                SingleInstance.Release();
            }
        }
    }
}
