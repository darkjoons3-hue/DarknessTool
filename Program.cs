using System;
using System.IO;
using System.Windows.Forms;

namespace DarknessTool
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Проверка единственного экземпляра — ДО всего остального
            if (!SingleInstance.TryAcquire())
            {
                MessageBox.Show(
                    "DarknessTool уже запущен.\n\nПроверь панель задач и трей.",
                    "DarknessTool",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (var splash = new SplashForm())
                {
                    splash.ShowDialog();
                }

                Application.Run(new MainForm());
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
                SingleInstance.Release();
            }
        }
    }
}
