using System;
using System.Collections.Generic;
using System.IO;

namespace DarknessTool
{
    public class IntegrityResult
    {
        public bool AllOk { get; set; } = true;
        public List<string> Problems { get; set; } = new();
        public List<string> Info { get; set; } = new();

        public string Summary =>
            AllOk
                ? "Все данные на месте"
                : $"Обнаружены проблемы: {Problems.Count}";
    }

    public static class IntegrityChecker
    {
        public static IntegrityResult Check()
        {
            var result = new IntegrityResult();

            try
            {
                if (!Directory.Exists(PathHelper.BaseDir))
                {
                    result.Info.Add("Папка данных будет создана при первом изменении.");
                }

                int logEntries = ChangeLog.Load().Count;
                if (logEntries > 0)
                {
                    result.Info.Add($"Журнал изменений: {logEntries} записей.");

                    if (!File.Exists(PathHelper.BackupChangeLogFile))
                    {
                        result.Problems.Add("Отсутствует резервная копия журнала изменений.");
                        result.AllOk = false;
                    }

                    if (PathHelper.UsePortableCopy && !File.Exists(PathHelper.PortableChangeLogFile))
                    {
                        result.Problems.Add("Отсутствует портативная копия журнала.");
                        result.AllOk = false;
                    }
                }

                int qCount = BackupManager.GetQuarantineCount();
                if (qCount > 0)
                {
                    result.Info.Add($"Карантин: {qCount} файлов.");

                    int backupCount = CountFiles(PathHelper.BackupQuarantineDir);
                    if (backupCount < qCount)
                    {
                        result.Problems.Add(
                            $"Карантин повреждён: в резерве {backupCount} из {qCount} файлов.");
                        result.AllOk = false;
                    }
                }

                int regBackups = CountFiles(PathHelper.RegistryBackupDir);
                if (regBackups > 0)
                {
                    result.Info.Add($"Бэкапов реестра: {regBackups}.");

                    int regBackupReserve = CountFiles(PathHelper.BackupRegistryBackupDir);
                    if (regBackupReserve < regBackups)
                    {
                        result.Problems.Add(
                            $"Бэкапы реестра повреждены: в резерве {regBackupReserve} из {regBackups}.");
                        result.AllOk = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Problems.Add("Ошибка проверки: " + ex.Message);
                result.AllOk = false;
            }

            return result;
        }

        private static int CountFiles(string dir)
        {
            try
            {
                if (!Directory.Exists(dir)) return 0;
                return Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
            }
            catch { return 0; }
        }

        public static void ShowIfProblems(IntegrityResult result)
        {
            if (result.AllOk) return;

            var text = string.Join("\n", result.Problems);
            System.Windows.Forms.MessageBox.Show(
                "⚠️ Обнаружены проблемы с данными DarknessTool:\n\n" + text +
                "\n\nВозможно, malware пытался повредить бэкапы или карантин. " +
                "Проверь журнал изменений и восстанови из резервной копии.",
                "Проверка целостности",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Warning);
        }
    }
}
