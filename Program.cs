using Spectre.Console;
using System.Diagnostics;
using System.Text.RegularExpressions;
ListFilesInDirectory(".");

static void ListFilesInDirectory(string directory_choice)
{
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
                .Title("Choose a file")
                .AddChoices(choices));
        if (file_choice == choices[0])
        {
            // Config
            Config.List();
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


        AnsiConsole.MarkupLine($"[green]Selected file: {file_choice}[/]");
        ListActionsOnFile(file_choice);

        AnsiConsole.WriteLine();
    }
}

static bool ListActionsOnFile(string file_choice)
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
                .Title($"What would you like to do with [blue]{file_choice}[/]?")
                .AddChoices(actions));

        if (action == actions[0])
        {
            ViewContents(file_choice);
        }
        else if (action == actions[1])
        {
            ListOptions(file_choice);
        }
        else if (action == actions[2])
        {
            ListOptions(file_choice, true);
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
    AnsiConsole.MarkupLine($"[yellow]{contents}[/]");
    AnsiConsole.WriteLine("Took " + sw.ElapsedMilliseconds.ToString() + " ms");
}
static bool ListOptions(string file_choice, bool replace = false)
{
    string[] actions = new string[] {
            "Premade 1",
            "Premade 2",
            "Custom regex",
            "Back"
    };
    var action = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"What would you like to do with [blue]{file_choice}[/]?")
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
        pattern = AnsiConsole.Ask<string>("Give me [green]regex pattern[/] to match");
    }
    else if (action == actions[3])
    {
        return true;
    }

    Stopwatch sw = new Stopwatch();

    if (replace)
    {
        string replacement = AnsiConsole.Ask<string>("Give me the [green]replacement[/]");
        sw.Start();
        ReplaceTextByPattern(file_choice, pattern, replacement);
    }
    else
    {
        sw.Start();
        ListLinesByPattern(file_choice, pattern);
    }
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"Took [yellow]{sw.ElapsedMilliseconds.ToString()}[/] ms");
    return false;
}

static void ListLinesByPattern(string file_choice, string pattern)
{
    var contents = File.ReadAllText(file_choice);
    RegexOptions options = RegexOptions.Multiline;

    var lines = File.ReadAllLines(file_choice);
    for (int i = 0; i < lines.Length; i++)
    {
        var line = lines[i];
        if (Regex.IsMatch(line, pattern, options))
        {
            var highlightedLine = Regex.Replace(line, pattern, match => $"[red]{match.Value}[/]", options);
            AnsiConsole.Markup($"[green]Line {i + 1}[/]: {highlightedLine} \n");
        }
    }

}

static void ReplaceTextByPattern(string file_choice, string pattern, string replacement)
{
    var contents = File.ReadAllText(file_choice);
    int replacementCount = 0;
    var newContents = Regex.Replace(contents, pattern, match =>
    {
        replacementCount++;
        return replacement;
    });

    AnsiConsole.MarkupLine($"[green]{replacementCount} replacement{(replacementCount != 1 ? "s" : "")} made[/] in [blue]{file_choice}[/]");

    File.WriteAllText(file_choice, newContents);
}

