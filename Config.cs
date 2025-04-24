using System.Reflection;
using System.Text;
using Spectre.Console;
using Tomlyn;

namespace CzuProjekt;

/// <summary>
/// Provides functionality for loading, saving, and editing a configuration file (TOML) for the application.
/// </summary>
public static class Config
{
    private const string DefaultData = "[colors]" + "\n"
                                                  + "highlight_a = 'blue'" + "\n"
                                                  + "highlight_b = 'green'" + "\n"
                                                  + "highlight_c = 'red'" + "\n"
                                                  + "[custom_regex]" + "\n"
                                                  + "custom1 = 'd.'" + "\n"
                                                  + "custom2 = 'd*'" + "\n"
                                                  + "custom3 = 'd+'" + "\n";

    const string ConfigFileName = "config.toml";

    private static readonly string PathDirectory = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "/CzuProjekt/");

    /// <summary>
    /// Loads the configuration from a TOML file. If the file doesn't exist, creates and returns a default configuration.
    /// </summary>
    /// <returns>The loaded or default <see cref="ConfigData"/> instance.</returns>
    public static ConfigData Load()
    {
        var pathFile = Path.Join(PathDirectory, ConfigFileName);
        try
        {
            if (File.Exists(pathFile))
            {
                string fileData = File.ReadAllText(pathFile);
                if (!string.IsNullOrWhiteSpace(fileData) &&
                    Toml.TryToModel<ConfigData>(fileData, out var model, out _))
                {
                    return CheckColorsValid(model);
                }

                Logger.LogError($"Invalid TOML format or empty file: ");
                return DefaultConfig();
            }

            return DefaultConfig();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading config file {ex.Message}");
            DefaultConfig();
            return new ConfigData();
        }
    }

    /// <summary>
    ///  Returns Default config data if one of colors is invalid
    /// <param name="model">model to check</param>
    /// </summary>
    private static ConfigData CheckColorsValid(ConfigData model)
    {
        if (!typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public).Any(prop =>
                string.Equals(prop.Name, model.Colors.HighlightA, StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogError("Invalid color HighlightA.");
            return DefaultConfig();
        }

        if (!typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public).Any(prop =>
                string.Equals(prop.Name, model.Colors.HighlightB, StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogError("Invalid color HighlightB.");
            return DefaultConfig();
        }

        if (!typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public).Any(prop =>
                string.Equals(prop.Name, model.Colors.HighlightC, StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogError("Invalid color HighlightC.");
            return DefaultConfig();
        }

        return model;
    }

    /// <summary>
    /// Creates a default configuration file with predefined data and returns a new config object.
    /// </summary>
    /// <returns>A new default <see cref="ConfigData"/> object.</returns>
    public static ConfigData DefaultConfig()
    {
        try
        {
            Logger.LogWarning("Config file not found, creating default config file.");
            FileStream file = File.Create(PathDirectory + ConfigFileName);
            file.Write(Encoding.UTF8.GetBytes(DefaultData));
            file.Close();
            return Toml.ToModel<ConfigData>(DefaultData);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error creating config file {ex.Message}");
        }

        return new ConfigData();
    }

    /// <summary>
    /// Saves the current configuration to a TOML file. If the file doesn't exist, creates a default file first.
    /// </summary>
    /// <param name="config">The configuration to save.</param>
    private static void Save(ConfigData config)
    {
        try
        {
            var pathFile = Path.Join(PathDirectory, ConfigFileName);

            if (File.Exists(pathFile))
            {
                Logger.LogSuccess("Config file found, saving changes.");
                string tomlOut = Toml.FromModel(config);
                //make sure toml is in string literals and remove escaping
                tomlOut = tomlOut.Replace("\"", "'").Replace(@"\\", @"\");
                File.WriteAllText(pathFile, tomlOut);
            }
            else
            {
                Logger.LogWarning("Config file not found, creating default config file.");
                SaveDefault(pathFile);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error saving config file {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new config file with default settings.
    /// </summary>
    /// <param name="pathFile">The file path where the default config will be written.</param>
    private static void SaveDefault(string pathFile)
    {
        try
        {
            FileStream file = File.Create(pathFile);
            file.Write(Encoding.UTF8.GetBytes(DefaultData));
            file.Close();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error creating config file{ex.Message}");
        }
    }

    /// <summary>
    /// Displays a menu to allow users to choose between editing colors, custom regex settings, or exiting.
    /// </summary>
    /// <param name="config">The configuration to display and potentially modify.</param>
    public static void List(ConfigData config)
    {
        while (true)
        {
            string path = $"> File > Config";
            string[] settings =
            [
                "Colors",
                "Custom regex",
                "Exit"
            ];

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{path}\n\n[{config.Colors.HighlightA}]Choose settings[/]")
                    .HighlightStyle($"{config.Colors.HighlightB}")
                    .AddChoices(settings));

            if (setting == settings[0])
            {
                ListColors(config);
            }

            if (setting == settings[1])
            {
                ListCustomRegex(config);
            }
            else if (setting == settings[^1])
            {
                return;
            }
        }
    }

    /// <summary>
    /// Allows the user to view and change the highlight color settings interactively.
    /// </summary>
    /// <param name="config">The configuration containing color values.</param>
    private static void ListColors(ConfigData config)
    {
        while (true)
        {
            string path = $"> File > Config > Colors";
            string[] settings =
            [
                "Highlight A - " + config.Colors.HighlightA,
                "Highlight B - " + config.Colors.HighlightB,
                "Highlight C - " + config.Colors.HighlightC,
                "Exit"
            ];
            try
            {
                var setting = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"{path}\n\n[{config.Colors.HighlightA}]Choose settings[/]")
                        .HighlightStyle($"{config.Colors.HighlightB}")
                        .AddChoices(settings));

                if (setting == settings[^1])
                {
                    Save(config);
                    return;
                }

                string color = AnsiConsole.Ask<string>("Enter color: ");
                if (!typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public).Any(prop =>
                        string.Equals(prop.Name, color, StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.LogError("Invalid color.");
                    return;
                }

                if (setting == settings[0])
                {
                    config.Colors.HighlightA = color;
                }
                else if (setting == settings[1])
                {
                    config.Colors.HighlightB = color;
                }
                else if (setting == settings[2])
                {
                    config.Colors.HighlightC = color;
                }
            }
            catch (Exception)
            {
                Logger.LogError("Invalid color.");
                return;
            }
        }
    }

    /// <summary>
    /// Allows the user to view and modify custom regular expression patterns in the configuration.
    /// </summary>
    /// <param name="config">The configuration containing custom regex values.</param>
    /// <returns>True when the user exits the regex configuration menu.</returns>
    private static void ListCustomRegex(ConfigData config)
    {
        while (true)
        {
            string path = $"> File > Config > Custom regex";
            string[] settings =
            [
                "Custom 1 - " + Markup.Escape(config.CustomRegex.Custom1),
                "Custom 2 - " + Markup.Escape(config.CustomRegex.Custom2),
                "Custom 3 - " + Markup.Escape(config.CustomRegex.Custom3),
                "Exit"
            ];

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{path}\n\n[{config.Colors.HighlightA}]Choose settings[/]")
                    .HighlightStyle($"{config.Colors.HighlightB}")
                    .AddChoices(settings));

            if (setting == settings[0])
            {
                config.CustomRegex.Custom1 = AnsiConsole.Ask<string>("Enter regex: ");
            }
            else if (setting == settings[1])
            {
                config.CustomRegex.Custom2 = AnsiConsole.Ask<string>("Enter regex: ");
            }
            else if (setting == settings[2])
            {
                config.CustomRegex.Custom3 = AnsiConsole.Ask<string>("Enter regex: ");
            }
            else if (setting == settings[^1])
            {
                Save(config);
                return;
            }
        }
    }
}