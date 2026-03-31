using System;
using System.Drawing;
using System.IO;

namespace SoulsDeathCounter.Core
{
    public class AppSettings
    {
        public Color FontColor { get; set; } = Color.White;
        public string FontFamily { get; set; } = "Arial";
        public float FontSize { get; set; } = 72f;
        public FontStyle FontStyle { get; set; } = FontStyle.Bold;
        public Color BackgroundColor { get; set; } = Color.FromArgb(64, 64, 64);
    }

    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
        private static readonly string DeathsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deaths.txt");

        public static AppSettings Load()
        {
            var settings = new AppSettings();

            if (!File.Exists(SettingsPath))
            {
                Save(settings);
                return settings;
            }

            try
            {
                var lines = File.ReadAllLines(SettingsPath);
                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length != 2)
                        continue;

                    var key = parts[0].Trim().ToLowerInvariant();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "font_color":
                            settings.FontColor = ParseColor(value, settings.FontColor);
                            break;
                        case "font_type":
                            settings.FontFamily = value;
                            break;
                        case "font_size":
                            if (float.TryParse(value, out var size))
                                settings.FontSize = size;
                            break;
                        case "font_style":
                            if (int.TryParse(value, out var style))
                                settings.FontStyle = (FontStyle)style;
                            break;
                        case "background_color":
                            settings.BackgroundColor = ParseColor(value, settings.BackgroundColor);
                            break;
                    }
                }
            }
            catch
            {
            }

            return settings;
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                var content = $@"Font_Color: {ColorToHex(settings.FontColor)}
Font_Type: {settings.FontFamily}
Font_Size: {settings.FontSize}
Font_Style: {(int)settings.FontStyle}
Background_Color: {ColorToHex(settings.BackgroundColor)}

# Font_Style values:
# 0 = Regular
# 1 = Bold
# 2 = Italic
# 3 = Bold + Italic

# Colors can be hex (#FFFFFF) or names (White, Red, etc.)
# Background_Color: Use RGB(64,64,64) or #404040 for chroma key in OBS/StreamLabs";

                File.WriteAllText(SettingsPath, content);
            }
            catch
            {
            }
        }

        public static void SaveDeathCount(int count)
        {
            try
            {
                File.WriteAllText(DeathsPath, count.ToString());
            }
            catch
            {
            }
        }

        private static Color ParseColor(string value, Color defaultColor)
        {
            try
            {
                if (value.StartsWith("#"))
                    return ColorTranslator.FromHtml(value);

                return Color.FromName(value);
            }
            catch
            {
                return defaultColor;
            }
        }

        private static string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
