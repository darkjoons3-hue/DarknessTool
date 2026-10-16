using System;
using System.Drawing;

namespace DarknessTool
{
    public static class SafeFont
    {
        private static FontFamily? _cachedFamily;

        public static FontFamily UiFamily
        {
            get
            {
                if (_cachedFamily != null) return _cachedFamily;

                try
                {
                    var msgFont = SystemFonts.MessageBoxFont;
                    if (msgFont != null && msgFont.FontFamily != null)
                    {
                        _cachedFamily = msgFont.FontFamily;
                        return _cachedFamily;
                    }
                }
                catch { }

                try
                {
                    _cachedFamily = new FontFamily("Segoe UI");
                    return _cachedFamily;
                }
                catch { }

                try
                {
                    _cachedFamily = new FontFamily("Tahoma");
                    return _cachedFamily;
                }
                catch { }

                _cachedFamily = FontFamily.GenericSansSerif;
                return _cachedFamily;
            }
        }

        public static Font Make(float size, FontStyle style = FontStyle.Regular)
        {
            try
            {
                if (size < 1f) size = 1f;
                if (size > 100f) size = 100f;
                return new Font(UiFamily, size, style);
            }
            catch
            {
                try { return new Font(SystemFonts.DefaultFont, style); }
                catch { return SystemFonts.DefaultFont; }
            }
        }
    }
}
