using System;
using System.Windows;
using System.Windows.Media;

namespace NotionNote.Helpers
{
    public static class ThemeManager
    {
        public static event EventHandler<bool>? ThemeChanged;

        public static void ApplyTheme(bool isDarkMode)
        {
            var app = Application.Current;
            if (app == null) return;

            // Update color resources directly
            if (isDarkMode)
            {
                ApplyDarkTheme(app);
            }
            else
            {
                ApplyLightTheme(app);
            }

            // Force UI refresh by updating all windows
            foreach (Window window in app.Windows)
            {
                UpdateWindowTheme(window, isDarkMode);
                InvalidateVisualTree(window);
            }

            // Notify subscribers
            ThemeChanged?.Invoke(null, isDarkMode);
        }

        private static void InvalidateVisualTree(DependencyObject obj)
        {
            if (obj == null) return;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                if (child is FrameworkElement fe)
                {
                    fe.InvalidateVisual();
                    fe.InvalidateArrange();
                    fe.InvalidateMeasure();
                }
                InvalidateVisualTree(child);
            }
        }

        private static void UpdateWindowTheme(Window window, bool isDarkMode)
        {
            if (window == null) return;

            // Update window background
            if (window.Resources.Contains("BackgroundColor"))
            {
                window.Background = (SolidColorBrush)window.Resources["BackgroundColor"];
            }
            else
            {
                window.Background = isDarkMode 
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            }
        }

        private static void ApplyLightTheme(Application app)
        {
            UpdateResource(app, "PrimaryColor", "#2563EB");
            UpdateResource(app, "PrimaryLightColor", "#3B82F6");
            UpdateResource(app, "PrimaryDarkColor", "#1D4ED8");
            UpdateResource(app, "SecondaryColor", "#64748B");
            UpdateResource(app, "AccentColor", "#10B981");
            UpdateResource(app, "BackgroundColor", "#FFFFFF");
            UpdateResource(app, "SurfaceColor", "#F8FAFC");
            UpdateResource(app, "BorderColor", "#E2E8F0");
            UpdateResource(app, "TextPrimaryColor", "#1E293B");
            UpdateResource(app, "TextSecondaryColor", "#64748B");
            UpdateResource(app, "HoverColor", "#F1F5F9");
            UpdateResource(app, "SelectedColor", "#E0F2FE");
        }

        private static void ApplyDarkTheme(Application app)
        {
            UpdateResource(app, "PrimaryColor", "#3B82F6");
            UpdateResource(app, "PrimaryLightColor", "#60A5FA");
            UpdateResource(app, "PrimaryDarkColor", "#2563EB");
            UpdateResource(app, "SecondaryColor", "#94A3B8");
            UpdateResource(app, "AccentColor", "#10B981");
            UpdateResource(app, "BackgroundColor", "#0F172A");
            UpdateResource(app, "SurfaceColor", "#1E293B");
            UpdateResource(app, "BorderColor", "#334155");
            UpdateResource(app, "TextPrimaryColor", "#F1F5F9");
            UpdateResource(app, "TextSecondaryColor", "#CBD5E1");
            UpdateResource(app, "HoverColor", "#334155");
            UpdateResource(app, "SelectedColor", "#1E3A8A");
        }

        private static void UpdateResource(Application app, string key, string colorHex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                var brush = new SolidColorBrush(color);
                brush.Freeze(); // Freeze for better performance
                
                // Update in main resources
                if (app.Resources.Contains(key))
                {
                    app.Resources[key] = brush;
                }
                else
                {
                    app.Resources.Add(key, brush);
                }

                // Also update in merged dictionaries (especially Styles.xaml)
                foreach (var dict in app.Resources.MergedDictionaries)
                {
                    if (dict.Contains(key))
                    {
                        dict[key] = brush;
                    }
                }
            }
            catch
            {
                // Ignore errors during theme switching
            }
        }
    }
}

