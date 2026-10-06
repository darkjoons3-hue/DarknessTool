using System;
using System.IO;

namespace DarknessTool
{
    /// <summary>
    /// Централизованное хранилище путей.
    /// Все пути — нейтральные, не содержат "DarknessTool", "Antivirus", "Cleaner".
    /// </summary>
    public static class PathHelper
    {
        // Основная папка данных (нейтральное имя)
        public static string BaseDir =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft", "Windows", "RuntimeCache");

        // Резервная папка в ProgramData
        public static string BackupBaseDir =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Microsoft", "Crypto", "MachineKeys.bak");

        // Рядом с exe (для портативного режима)
        public static string PortableDir =>
            Path.Combine(AppContext.BaseDirectory, "RuntimeCache");

        public static string QuarantineDir => Path.Combine(BaseDir, "Q");
        public static string RegistryBackupDir => Path.Combine(BaseDir, "R");
        public static string LogDir => Path.Combine(BaseDir, "L");
        public static string SettingsFile => Path.Combine(BaseDir, "s.dat");
        public static string ChangeLogFile => Path.Combine(BaseDir, "c.dat");

        public static void EnsureAll()
        {
            try { Directory.CreateDirectory(BaseDir); } catch { }
            try { Directory.CreateDirectory(BackupBaseDir); } catch { }
            try { Directory.CreateDirectory(QuarantineDir); } catch { }
            try { Directory.CreateDirectory(RegistryBackupDir); } catch { }
            try { Directory.CreateDirectory(LogDir); } catch { }
        }
    }
}
