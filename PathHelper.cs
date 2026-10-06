using System;
using System.IO;

namespace DarknessTool
{
    /// <summary>
    /// Централизованное хранилище путей.
    /// Три независимых места для бэкапов и карантина:
    ///   1. %AppData%\Microsoft\Windows\RuntimeCache\  — основное
    ///   2. %ProgramData%\Microsoft\Crypto\MachineKeys.bak\ — резервное (ACL-защищённое в будущем)
    ///   3. <папка exe>\RuntimeCache\ — портативное (если на флешке)
    /// </summary>
    public static class PathHelper
    {
        // === Основная папка (AppData) ===
        public static string BaseDir =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft", "Windows", "RuntimeCache");

        // === Резервная папка (ProgramData) ===
        public static string BackupBaseDir =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Microsoft", "Crypto", "MachineKeys.bak");

        // === Портативная папка (рядом с exe) ===
        public static string PortableDir =>
            Path.Combine(AppContext.BaseDirectory, "RuntimeCache");

        // === Подпапки основной ===
        public static string QuarantineDir      => Path.Combine(BaseDir, "Q");
        public static string RegistryBackupDir  => Path.Combine(BaseDir, "R");
        public static string LogDir             => Path.Combine(BaseDir, "L");
        public static string SettingsFile       => Path.Combine(BaseDir, "s.dat");
        public static string ChangeLogFile      => Path.Combine(BaseDir, "c.dat");

        // === Подпапки резервной ===
        public static string BackupQuarantineDir     => Path.Combine(BackupBaseDir, "Q");
        public static string BackupRegistryBackupDir => Path.Combine(BackupBaseDir, "R");
        public static string BackupChangeLogFile     => Path.Combine(BackupBaseDir, "c.dat");

        // === Подпапки портативной ===
        public static string PortableQuarantineDir     => Path.Combine(PortableDir, "Q");
        public static string PortableRegistryBackupDir => Path.Combine(PortableDir, "R");
        public static string PortableChangeLogFile     => Path.Combine(PortableDir, "c.dat");

        /// <summary>
        /// Копировать ли данные в портативную папку.
        /// Включается, если exe лежит на съёмном носителе.
        /// </summary>
        public static bool UsePortableCopy
        {
            get
            {
                try
                {
                    var root = Path.GetPathRoot(AppContext.BaseDirectory);
                    if (string.IsNullOrEmpty(root)) return false;
                    var drive = new DriveInfo(root);
                    return drive.DriveType == DriveType.Removable;
                }
                catch { return false; }
            }
        }

        public static void EnsureAll()
        {
            try { Directory.CreateDirectory(BaseDir); } catch { }
            try { Directory.CreateDirectory(QuarantineDir); } catch { }
            try { Directory.CreateDirectory(RegistryBackupDir); } catch { }
            try { Directory.CreateDirectory(LogDir); } catch { }

            try { Directory.CreateDirectory(BackupBaseDir); } catch { }
            try { Directory.CreateDirectory(BackupQuarantineDir); } catch { }
            try { Directory.CreateDirectory(BackupRegistryBackupDir); } catch { }

            if (UsePortableCopy)
            {
                try { Directory.CreateDirectory(PortableDir); } catch { }
                try { Directory.CreateDirectory(PortableQuarantineDir); } catch { }
                try { Directory.CreateDirectory(PortableRegistryBackupDir); } catch { }
            }
        }
    }
}
