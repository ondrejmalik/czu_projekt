using System.Reflection;
using System.Text;
using Spectre.Console;
using Tomlyn;
public static class Config
{
    public static string defaultData = "[colors]" +
                                       "\n" +
                                       "highlight_a = \"blue\"" +
                                       "\n" +
                                       "highlight_b = \"green\"" +
                                       "\n" +
                                       "highlight_c = \"red\"" +
                                       "\n" +
                                       "[custom_regex]" +
                                       "\n" +
                                       "custom1 = \"d.\"" +
                                       "\n" +
                                       "custom2 = \"d*\"" +
                                       "\n" +
                                       "custom3 = \"d+\"" +
                                       "\n";

    public static ConfigData Load()
    {

        var path_directory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "/czu_projekt/");
        var path_file = Path.Join(path_directory, "config.toml");
        try
        {
            if (File.Exists(path_file))
            {
                string file_data = File.ReadAllText(path_file);
                var model = Toml.ToModel<ConfigData>(file_data);

                return model;
            }

            return DefaultConfig(path_file);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading config file {ex.Message}");
            DefaultConfig(path_file);
            return new ConfigData();
        }
    }
    public static ConfigData DefaultConfig(string path_file)
    {
        try
        {
            Logger.LogWarning("Config file not found, creating default config file.");
            FileStream file = File.Create(path_file);
            file.Write(Encoding.UTF8.GetBytes(defaultData));
            file.Close();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error creating config file {ex.Message}");
        }
        return new ConfigData();
    }
    public static void Save(ConfigData config)
    {
        try
        {
            var pathDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "/czu_projekt/");
            var pathFile = Path.Join(pathDirectory, "config.toml");

            if (File.Exists(pathFile))
            {
                Logger.LogSuccess("Config file found, saving changes.");
                string tomlOut = Toml.FromModel(config);
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

    public static void SaveDefault(string pathFile)
    {
        try
        {
            FileStream file = File.Create(pathFile);
            file.Write(Encoding.UTF8.GetBytes(defaultData));
            file.Close();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error creating config file{ex.Message}");
        }
    }


    public static bool List(ConfigData config)
    {
        while (true)
        {
            string path = $"> File > Config";
            string[] settings = new string[]
            {
                "Colors",
                "Custom regex",
                "Exit"
            };

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{path}\n\n[{config.colors.highlightA}]Choose settings[/]")
                    .HighlightStyle($"{config.colors.highlightB}")
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
                return true;
            }
        }
    }

    public static bool ListColors(ConfigData config)
    {
        while (true)
        {
            string path = $"> File > Config > Colors";
            string[] settings = new string[]
            {
                "Highlight A - " + config.colors.highlightA,
                "Highlight B - " + config.colors.highlightB,
                "Highlight C - " + config.colors.highlightC,
                "Exit"
            };
            try
            {
                var setting = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"{path}\n\n[{config.colors.highlightA}]Choose settings[/]")
                        .HighlightStyle($"{config.colors.highlightB}")
                        .AddChoices(settings));

                if (setting == settings[^1])
                {
                    Save(config);
                    return true;
                }
                string color = AnsiConsole.Ask<string>("Enter color: ");
                if (!typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public).Any(prop =>
                        string.Equals(prop.Name, color, StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.LogError("Invalid color format.");
                    return true;
                }

                if (setting == settings[0])
                {
                    config.colors.highlightA = color;
                }
                else if (setting == settings[1])
                {
                    config.colors.highlightB = color;
                }
                else if (setting == settings[2])
                {
                    config.colors.highlightC = color;
                }

            }
            catch (Exception)
            {

                Logger.LogError("Invalid color format.");
                return true;
            }
        }
    }

    public static bool ListCustomRegex(ConfigData config)
    {
        while (true)
        {
            string path = $"> File > Config > Custom regex";
            string[] settings = new string[]
            {
                "Custom 1 - " + config.custom_regex.custom_1,
                "Custom 2 - " + config.custom_regex.custom_2,
                "Custom 3 - " + config.custom_regex.custom_3,
                "Exit"
            };

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{path}\n\n[{config.colors.highlightA}]Choose settings[/]")
                    .HighlightStyle($"{config.colors.highlightB}")
                    .AddChoices(settings));

            if (setting == settings[0])
            {
                config.custom_regex.custom_1 = AnsiConsole.Ask<string>("Enter regex: ");
            }
            else if (setting == settings[1])
            {
                config.custom_regex.custom_2 = AnsiConsole.Ask<string>("Enter regex: ");
            }

            else if (setting == settings[2])
            {
                config.custom_regex.custom_3 = AnsiConsole.Ask<string>("Enter regex: ");
            }
            else if (setting == settings[^1])
            {
                Save(config);
                return true;
            }
        }
    }
}
