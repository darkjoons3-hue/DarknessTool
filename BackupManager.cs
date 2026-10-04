using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DarknessTool
{
    public static class BackupManager
    {
        public static string BaseDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarknessTool");

        public static string BackupsRegistryDir => Path.Combine(BaseDir, "Backups", "registry");
        public static string QuarantineFilesDir  => Path.Combine(BaseDir, "Quarantine", "files");

        public static void EnsureDirs()
        {
            Directory.CreateDirectory(BackupsRegistryDir);
            Directory.CreateDirectory(QuarantineFilesDir);
        }

        // ============ РЕЕСТР ============

        /// <summary>
        /// Экспорт ветки реестра в .reg файл. Возвращает путь к бэкапу или пустую строку.
        /// </summary>
        public static string BackupRegistryKey(string hive, string subKey)
        {
            try
            {
                EnsureDirs();
                string safeName = SafeFileName(hive + "_" + subKey) + "_" +
                                  DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".reg";
                string path = Path.Combine(BackupsRegistryDir, safeName);

                string fullKey = hive + "\\" + subKey;
                if (!fullKey.StartsWith("HKEY", StringComparison.OrdinalIgnoreCase))
                    fullKey = HiveToFull(hive) + "\\" + subKey;

                var psi = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"export \"{fullKey}\" \"{path}\" /y",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using var p = Process.Start(psi);
                if (p == null) return "";
                p.WaitForExit(8000);

                if (p.ExitCode == 0 && File.Exists(path))
                    return path;

                return "";
            }
            catch { return ""; }
        }

        public static bool RestoreRegistryBackup(string regFilePath)
        {
            try
            {
                if (!File.Exists(regFilePath)) return false;

                var psi = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"import \"{regFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using var p = Process.Start(psi);
                if (p == null) return false;
                p.WaitForExit(10000);
                return p.ExitCode == 0;
            }
            catch { return false; }
        }

        private static string HiveToFull(string hive)
        {
            return hive.ToUpperInvariant() switch
            {
                "HKLM" or "HKEY_LOCAL_MACHINE" => "HKEY_LOCAL_MACHINE",
                "HKCU" or "HKEY_CURRENT_USER"  => "HKEY_CURRENT_USER",
                "HKCR" or "HKEY_CLASSES_ROOT"  => "HKEY_CLASSES_ROOT",
                "HKU"  or "HKEY_USERS"          => "HKEY_USERS",
                "HKCC" or "HKEY_CURRENT_CONFIG" => "HKEY_CURRENT_CONFIG",
                _                                => hive
            };
        }

        // ============ ФАЙЛЫ (карантин) ============

        /// <summary>
        /// Перемещает файл в карантин. Возвращает путь к .quar-файлу или пустую строку.
        /// </summary>
        public static string QuarantineFile(string originalPath)
        {
            try
            {
                EnsureDirs();
                if (!File.Exists(originalPath)) return "";

                string baseName = Path.GetFileName(originalPath);
                string safe = SafeFileName(baseName) + "_" +
                              DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".quar";
                string dest = Path.Combine(QuarantineFilesDir, safe);

                // Копируем, потом удаляем оригинал
                File.Copy(originalPath, dest, true);
                try { File.SetAttributes(originalPath, FileAttributes.Normal); } catch { }
                File.Delete(originalPath);

                return dest;
            }
            catch { return ""; }
        }

        /// <summary>
        /// Копирование файла в карантин без удаления (для бэкапа).
        /// </summary>
        public static string CopyToQuarantine(string originalPath)
        {
            try
            {
                EnsureDirs();
                if (!File.Exists(originalPath)) return "";

                string baseName = Path.GetFileName(originalPath);
                string safe = SafeFileName(baseName) + "_" +
                              DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".quar";
                string dest = Path.Combine(QuarantineFilesDir, safe);

                File.Copy(originalPath, dest, true);
                return dest;
            }
            catch { return ""; }
        }

        public static bool RestoreFileFromQuarantine(string quarPath, string originalPath)
        {
            try
            {
                if (!File.Exists(quarPath)) return false;
                string dir = Path.GetDirectoryName(originalPath) ?? "";
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(quarPath, originalPath, true);
                return true;
            }
            catch { return false; }
        }

        public static bool DeleteQuarantineItem(string quarPath)
        {
            try
            {
                if (File.Exists(quarPath)) { File.Delete(quarPath); return true; }
            }
            catch { }
            return false;
        }

        // ============ ОТКАТ ============

        public static bool Undo(ChangeEntry entry)
        {
            try
            {
                switch (entry.Type)
                {
                    case ChangeType.RegistrySet:
                    case ChangeType.RegistryDelete:
                        if (string.IsNullOrEmpty(entry.BackupPath) || !File.Exists(entry.BackupPath))
                            return false;
                        return RestoreRegistryBackup(entry.BackupPath);

                    case ChangeType.FileQuarantine:
                    case ChangeType.FileDelete:
                        if (string.IsNullOrEmpty(entry.BackupPath) || !File.Exists(entry.BackupPath))
                            return false;
                        if (string.IsNullOrEmpty(entry.OriginalPath)) return false;
                        return RestoreFileFromQuarantine(entry.BackupPath, entry.OriginalPath);

                    default:
                        return false;
                }
            }
            catch { return false; }
        }

        // ============ УТИЛИТЫ ============

        public static string SafeFileName(string name)
        {
            var sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
                    sb.Append(c);
                else
                    sb.Append('_');
            }
            var s = sb.ToString();
            if (s.Length > 80) s = s.Substring(0, 80);
            return s;
        }

        public static long GetQuarantineSize()
        {
            try
            {
                if (!Directory.Exists(QuarantineFilesDir)) return 0;
                long total = 0;
                foreach (var f in Directory.GetFiles(QuarantineFilesDir))
                    total += new FileInfo(f).Length;
                return total;
            }
            catch { return 0; }
        }

        public static int GetQuarantineCount()
        {
            try
            {
                if (!Directory.Exists(QuarantineFilesDir)) return 0;
                return Directory.GetFiles(QuarantineFilesDir).Length;
            }
            catch { return 0; }
        }
    }
}
