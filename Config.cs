using System.Text;
using Spectre.Console;
using Tomlyn;

public class ConfigData
{
    public Colors colors { get; set; }

    public class Colors
    {
        public string highlightA { get; set; }
        public string highlightB { get; set; }
        public string highlightC { get; set; }
    }
}
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
        data += "highlight_a = \"red\"";
        data += "\n";
        data += "highlight_b = \"blue\"";
        data += "\n";
        data += "highlight_c = \"green\"";
        data += "\n";
        file.Write(Encoding.UTF8.GetBytes(data));
        file.Close();
        return new ConfigData();
    }



    public static bool List()
    {
        Load();
        while (true)
        {
            string[] settings = new string[] {
                  "Colors",
                  "Exit"
                };

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Choose settings")
                    .AddChoices(settings));

            if (setting == settings[0])
            {
                ListColors();
            }
            else if (setting == settings[^1])
            {
                return true;
            }
        }
    }

    public static bool ListColors()
    {
        while (true)
        {
            string[] settings = new string[] {
                  "Highlight A",
                  "Highlight B",
                  "Highlight C",
                  "Exit"
                };

            var setting = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Choose settings")
                    .AddChoices(settings));

            if (setting == settings[0])
            {
            }
            else if (setting == settings[1])
            {
            }
            else if (setting == settings[2])
            {
            }
            else if (setting == settings[^1])
            {
                return true;
            }
        }
    }
}
