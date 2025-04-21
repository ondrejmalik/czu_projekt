namespace CzuProjekt;

/// <summary>
/// Represents the root configuration data loaded from or saved to a TOML config file.
/// Contains color highlight settings and custom regex patterns.
/// </summary>
public class ConfigData
{
    /// <summary>
    /// Gets or sets the color highlight configuration.
    /// </summary>
    public Colors Colors { get; set; } = new Colors();

    /// <summary>
    /// Gets or sets the custom regular expression configuration.
    /// </summary>
    public CustomRegex CustomRegex { get; set; } = new CustomRegex();
}

/// <summary>
/// Represents color settings used for highlighting UI elements.
/// </summary>
public class Colors
{
    /// <summary>
    /// Gets or sets the primary highlight color (highlight A).
    /// </summary>
    public string HighlightA { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secondary highlight color (highlight B).
    /// </summary>
    public string HighlightB { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tertiary highlight color (highlight C).
    /// </summary>
    public string HighlightC { get; set; } = string.Empty;
}

/// <summary>
/// Represents user-defined regular expression patterns.
/// </summary>
public class CustomRegex
{
    /// <summary>
    /// Gets or sets the first custom regular expression.
    /// </summary>
    ///
    public string Custom1 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the second custom regular expression.
    /// </summary>
    public string Custom2 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the third custom regular expression.
    /// </summary>
    public string Custom3 { get; set; } = string.Empty;
}