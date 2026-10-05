using System;

namespace DarknessTool
{
    public static class WindowMasker
    {
        // Список реальных системных имён Windows — malware их пропускает
        private static readonly string[] SystemNames = new[]
        {
            "Runtime Broker",
            "Windows Update Helper",
            "Security Health Service",
            "Host Process",
            "System Configuration",
            "Windows Explorer Helper",
            "Antimalware Service",
            "Task Scheduler Service",
            "Device Setup Manager",
            "Print Spooler Service"
        };

        private static readonly Random _rnd = new Random();

        private static string _currentTitle = "DarknessTool";
        private static string _currentClassName = "";

        public static string CurrentTitle => _currentTitle;
        public static string CurrentClassName => _currentClassName;

        /// <summary>
        /// Генерирует случайный заголовок окна.
        /// </summary>
        public static string GenerateTitle(bool masked)
        {
            if (!masked)
            {
                _currentTitle = "DarknessTool";
                return _currentTitle;
            }

            var name = SystemNames[_rnd.Next(SystemNames.Length)];
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 4).ToLowerInvariant();
            _currentTitle = $"{name} · {suffix}";
            return _currentTitle;
        }

        /// <summary>
        /// Генерирует случайное имя класса окна (Win32 class name).
        /// </summary>
        public static string GenerateClassName()
        {
            _currentClassName = "WinCls" + Guid.NewGuid().ToString("N").Substring(0, 10);
            return _currentClassName;
        }
    }
}
