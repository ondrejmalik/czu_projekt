using Spectre.Console;
using System.Diagnostics;
using System.Text.RegularExpressions;


ListFilesInDirectory(".");

static void ListFilesInDirectory(string directory_choice)
{
    // COnfigure
    ConfigData config = Config.Load();
    while (true)
    {
        string[] files = Directory.GetFiles(".")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>()
            .ToArray();
        string[] config_section = new string[] {
            "Config",
            "----------------",
        };
        // combine the two arrays
        string[] choices = config_section.Concat(files).Append("Exit").ToArray();

        var file_choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{config.colors.highlightA}]Choose a file[/]")
                .PageSize(10)
                .HighlightStyle($"{config.colors.highlightB}")
                .AddChoices(choices));

        if (file_choice == choices[0])
        {
            // Config
            Config.List(config);
            continue;
        }
        else if (file_choice == choices[1])
        {
            // ----------------
            continue;
        }
        else if (file_choice == choices[^1])
        {
            // Exit
            return;
        }


        AnsiConsole.MarkupLine($"[{config.colors.highlightB}]Selected file: {file_choice}[/]");
        ListActionsOnFile(file_choice, config);

        AnsiConsole.WriteLine();
    }
}

static bool ListActionsOnFile(string file_choice, ConfigData config)
{
    while (true)
    {
        string[] actions = new string[] {
                "View contents",
                "List",
                "Replace",
                "Back"
        };

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"What would you like to do with [{config.colors.highlightA}]{file_choice}[/]?")
                .HighlightStyle($"{config.colors.highlightB}")
                .AddChoices(actions));

        if (action == actions[0])
        {
            ViewContents(file_choice);
        }
        else if (action == actions[1])
        {
            ListOptions(file_choice, config);
        }
        else if (action == actions[2])
        {
            ListOptions(file_choice, config, true);
        }
        else if (action == actions[3])
        {
            return true;
        }
    }
}

static void ViewContents(string file_choice)
{
    Stopwatch sw = new Stopwatch();
    sw.Start();
    var contents = File.ReadAllText(file_choice);
    AnsiConsole.WriteLine($"{contents}");
    AnsiConsole.WriteLine("Took " + sw.ElapsedMilliseconds.ToString() + " ms");
}

static bool ListOptions(string file_choice, ConfigData config, bool replace = false)
{
    string[] actions = new string[] {
            "Premade 1",
            "Premade 2",
            "Custom regex",
            "Back"
    };
    var action = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"What would you like to do with [{config.colors.highlightA}]{file_choice}[/]?")
            .HighlightStyle($"{config.colors.highlightB}")
            .AddChoices(actions));

    string pattern = "";
    if (action == actions[0])
    {
        pattern = "premade pattern";
    }
    else if (action == actions[1])
    {
        pattern = "premade pattern 2";
    }
    else if (action == actions[2])
    {
        pattern = AnsiConsole.Ask<string>($"Give me [{config.colors.highlightB}]regex pattern[/] to match");
    }
    else if (action == actions[3])
    {
        return true;
    }

    Stopwatch sw = new Stopwatch();

    if (replace)
    {
        string replacement = AnsiConsole.Ask<string>($"Give me the [{config.colors.highlightB}]replacement[/]");
        sw.Start();
        ReplaceTextByPattern(file_choice, pattern, replacement, config);
    }
    else
    {
        sw.Start();
        ListLinesByPattern(file_choice, pattern, config);
    }
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"Took [yellow]{sw.ElapsedMilliseconds.ToString()}[/] ms");
    return false;
}

static void ListLinesByPattern(string file_choice, string pattern, ConfigData config)
{
    var contents = File.ReadAllText(file_choice);
    RegexOptions options = RegexOptions.Multiline;
    int matchCount = 0;
    var lines = File.ReadAllLines(file_choice);
    for (int i = 0; i < lines.Length; i++)
    {
        var line = lines[i];
        var matches = Regex.Matches(line, pattern, options);
        if (matches.Count > 0)
        {
            matchCount += matches.Count;

            line = line.Replace("[", "[[").Replace("]", "]]");

            var highlightedLine = Regex.Replace(line, pattern, match => $"[{config.colors.highlightC}]{match.Value}[/]", options);

            AnsiConsole.Markup($"[{config.colors.highlightB}]Line {i + 1}[/]: {highlightedLine} \n");
        }
    }

    AnsiConsole.Markup($"[{config.colors.highlightC}]Found {matchCount} matches[/]\n");
}

static void ReplaceTextByPattern(string file_choice, string pattern, string replacement, ConfigData config)
{
    var contents = File.ReadAllText(file_choice);
    int replacementCount = 0;
    var newContents = Regex.Replace(contents, pattern, match =>
    {
        replacementCount++;
        return replacement;
    });

    AnsiConsole.MarkupLine($"[{config.colors.highlightC}]{replacementCount} replacement{(replacementCount != 1 ? "s" : "")} made[/] in [{config.colors.highlightA}]{file_choice}[/]");

    File.WriteAllText(file_choice, newContents);
}

