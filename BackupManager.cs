using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DarknessTool
{
    public static class BackupManager
    {
        public static string BaseDir => PathHelper.BaseDir;

        public static string BackupsRegistryDir => PathHelper.RegistryBackupDir;
        public static string QuarantineFilesDir  => PathHelper.QuarantineDir;

        public static void EnsureDirs()
        {
            PathHelper.EnsureAll();
        }

        // ============ РЕЕСТР ============

        public static string BackupRegistryKey(string hive, string subKey)
        {
            try
            {
                EnsureDirs();

                string safeName = SafeFileName(hive + "_" + subKey) + "_" +
                                  DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".reg";

                string mainPath = Path.Combine(BackupsRegistryDir, safeName);

                if (File.Exists(mainPath))
                {
                    safeName = Path.GetFileNameWithoutExtension(safeName) + "_" +
                               Guid.NewGuid().ToString("N").Substring(0, 4) + ".reg";
                    mainPath = Path.Combine(BackupsRegistryDir, safeName);
                }

                string fullKey = hive + "\\" + subKey;
                if (!fullKey.StartsWith("HKEY", StringComparison.OrdinalIgnoreCase))
                    fullKey = HiveToFull(hive) + "\\" + subKey;

                var psi = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"export \"{fullKey}\" \"{mainPath}\" /y",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using var p = Process.Start(psi);
                if (p == null) return "";
                p.WaitForExit(8000);

                if (p.ExitCode != 0 || !File.Exists(mainPath))
                    return "";

                TryCopyToBackup(mainPath, Path.Combine(PathHelper.BackupRegistryBackupDir, safeName));

                if (PathHelper.UsePortableCopy)
                    TryCopyToBackup(mainPath, Path.Combine(PathHelper.PortableRegistryBackupDir, safeName));

                return mainPath;
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

                File.Copy(originalPath, dest, true);

                TryCopyToBackup(dest, Path.Combine(PathHelper.BackupQuarantineDir, safe));
                if (PathHelper.UsePortableCopy)
                    TryCopyToBackup(dest, Path.Combine(PathHelper.PortableQuarantineDir, safe));

                try { File.SetAttributes(originalPath, FileAttributes.Normal); } catch { }
                File.Delete(originalPath);

                return dest;
            }
            catch { return ""; }
        }

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

                TryCopyToBackup(dest, Path.Combine(PathHelper.BackupQuarantineDir, safe));
                if (PathHelper.UsePortableCopy)
                    TryCopyToBackup(dest, Path.Combine(PathHelper.PortableQuarantineDir, safe));

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

        private static void TryCopyToBackup(string source, string dest)
        {
            try
            {
                var dir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(source, dest, true);
            }
            catch { }
        }

        public static string? FindBackupCopy(string fileName)
        {
            try
            {
                var candidates = new[]
                {
                    Path.Combine(QuarantineFilesDir, fileName),
                    Path.Combine(PathHelper.BackupQuarantineDir, fileName),
                    Path.Combine(PathHelper.PortableQuarantineDir, fileName)
                };

                foreach (var c in candidates)
                {
                    if (File.Exists(c)) return c;
                }
            }
            catch { }
            return null;
        }

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
