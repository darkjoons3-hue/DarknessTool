using System;
using System.Threading;

namespace DarknessTool
{
    /// <summary>
    /// Защита от запуска второго экземпляра программы.
    /// Использует глобальный мьютекс с нейтральным именем (не содержит "Darkness").
    /// </summary>
    public static class SingleInstance
    {
        // Нейтральное имя — malware не ищет по "Darkness"
        private const string MutexName = @"Global\WinRuntimeCache_7a3f9b21";

        private static Mutex? _mutex;

        /// <summary>
        /// Пытается захватить мьютекс. Возвращает true, если это единственный экземпляр.
        /// </summary>
        public static bool TryAcquire()
        {
            try
            {
                _mutex = new Mutex(initiallyOwned: true, MutexName, out bool createdNew);
                return createdNew;
            }
            catch
            {
                // Если не удалось создать мьютекс — считаем, что экземпляр единственный
                // (лучше запуститься, чем не запуститься вообще)
                return true;
            }
        }

        public static void Release()
        {
            try
            {
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
                _mutex = null;
            }
            catch { }
        }
    }
}
