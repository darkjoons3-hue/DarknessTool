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
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Сначала — сплэш-скрин (блокирует ~1.8 сек)
                using (var splash = new SplashForm())
                {
                    splash.ShowDialog();
                }

                // Потом — главное окно
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                try
                {
                    var log = Path.Combine(Path.GetTempPath(), "DarknessTool_error.log");
                    File.AppendAllText(log, $"[{DateTime.Now:O}] {ex}\n\n");
                }
                catch { }

                MessageBox.Show(
                    "Произошла ошибка:\n\n" + ex.Message,
                    "DarknessTool",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
