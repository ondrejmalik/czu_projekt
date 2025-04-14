using System.Text;
using Spectre.Console;
using Tomlyn;

public static class Config
{
    public static ConfigData Load()
    {
        var path_directory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "/czu_projekt/");
        var path_file = Path.Join(path_directory, "config.toml");

        if (File.Exists(path_file))
        {
            string file_data = File.ReadAllText(path_file);
            var model = Toml.ToModel<ConfigData>(file_data);

            return model;
        }
        FileStream file = File.Create(path_file);
        string data = "[colors]";
        data += "\n";
        data += "highlight_a = \"blue\"";
        data += "\n";
        data += "highlight_b = \"green\"";
        data += "\n";
        data += "highlight_c = \"red\"";
        data += "\n";
        data += "[custom_regex]";
        data += "\n";
        data += "custom1 = \"d.\"";
        data += "\n";
        data += "custom2 = \"d*\"";
        data += "\n";
        data += "custom3 = \"d+\"";
        data += "\n";
        file.Write(Encoding.UTF8.GetBytes(data));
        file.Close();

        return new ConfigData();
    }

    public static void Save(ConfigData config)
    {
        var pathDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "/czu_projekt/");
        var pathFile = Path.Join(pathDirectory, "config.toml");

        if (File.Exists(pathFile))
        {
            string tomlOut = Toml.FromModel(config);
            File.WriteAllText(pathFile, tomlOut);
        }
        else
        {
            SaveDefault(pathFile);
        }

    }

    public static void SaveDefault(string pathFile)
    {
        FileStream file = File.Create(pathFile);
        string data = "[colors]";
        data += "\n";
        data += "highlight_a = \"red\"";
        data += "\n";
        data += "highlight_b = \"blue\"";
        data += "\n";
        data += "highlight_c = \"green\"";
        data += "\n";
        file.Write(Encoding.UTF8.GetBytes(data));
        file.Close();
    }


    public static bool List(ConfigData config)
    {
        while (true)
        {
            string[] settings = new string[] {
                  "Colors",
                  "Custom regex",
                  "Exit"
                };

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{config.colors.highlightA}]Choose settings[/]")
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
            string[] settings = new string[] {
                  "Highlight A - " + config.colors.highlightA,
                  "Highlight B - " + config.colors.highlightB,
                  "Highlight C - " + config.colors.highlightC,
                  "Exit"
                };

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{config.colors.highlightA}]Choose settings[/]")
                    .HighlightStyle($"{config.colors.highlightB}")
                    .AddChoices(settings));

            if (setting == settings[0])
            {
                config.colors.highlightA = AnsiConsole.Ask<string>("Enter color: ");
            }
            else if (setting == settings[1])
            {
                config.colors.highlightB = AnsiConsole.Ask<string>("Enter color: ");
            }
            else if (setting == settings[2])
            {
                config.colors.highlightC = AnsiConsole.Ask<string>("Enter color: ");
            }
            else if (setting == settings[^1])
            {
                Save(config);
                return true;
            }
        }

    }
    public static bool ListCustomRegex(ConfigData config)
    {
        while (true)
        {
            string[] settings = new string[] {
                  "Custom 1 - " + config.custom_regex.custom_1,
                  "Custom 2 - " + config.custom_regex.custom_2,
                  "Custom 3 - " + config.custom_regex.custom_3,
                  "Exit"
                };

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{config.colors.highlightA}]Choose settings[/]")
                    .HighlightStyle($"{config.colors.highlightB}")
                    .AddChoices(settings));

            if (setting == settings[0])
            {
                config.custom_regex.custom_1 = AnsiConsole.Ask<string>("Enter regex: ");
            }
            else if (setting == settings[1])
            {
                config.custom_regex.custom_1 = AnsiConsole.Ask<string>("Enter regex: ");
            }
            else if (setting == settings[2])
            {
                config.colors.highlightB = AnsiConsole.Ask<string>("Enter regex: ");
            }
            else if (setting == settings[^1])
            {
                Save(config);
                return true;
            }
        }

    }

}
