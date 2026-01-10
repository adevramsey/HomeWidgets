using MudBlazor;

namespace HomeWidgets.App.Theme;

/// <summary>
/// Centralized MudBlazor theme configuration.
/// Keep this as the single source of truth for colors/typography.
/// </summary>
public static class AppTheme
{
    /// <summary>
    /// Sleek, enterprise-friendly dark theme: high contrast, low chroma, muted surfaces.
    /// </summary>
    public static MudTheme Dark { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#4F8CFF",      // crisp enterprise blue
            Secondary = "#8B5CF6",    // subtle violet accent
            Tertiary = "#22C55E",     // success/positive accent

            Background = "#0B1220",   // deep navy
            Surface = "#0F1A2E",      // slightly lifted surface
            AppbarBackground = "#0F1A2E",
            DrawerBackground = "#0F1A2E",

            TextPrimary = "#E7EEF8",
            TextSecondary = "#A9B7CF",
            LinesDefault = "#1E2A44",
            Divider = "#1E2A44",

            Error = "#EF4444",
            Warning = "#F59E0B",
            Info = "#38BDF8",
            Success = "#22C55E"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "Segoe UI", "Helvetica Neue", "Arial", "sans-serif" },
                FontSize = ".95rem",
                FontWeight = "400"
            },
            H4 = new H4Typography { FontWeight = "600", LetterSpacing = "-0.02em" },
            H5 = new H5Typography { FontWeight = "600", LetterSpacing = "-0.01em" },
            H6 = new H6Typography { FontWeight = "600" }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px"
        }
    };
}
