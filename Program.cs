using System.Diagnostics;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace CzuProjekt;

public static class Program
{
    [STAThread]
    public static void Main() => ListFilesInDirectory(".");

    /// <summary>
    /// Displays a file selection menu and handles user actions.
    /// </summary>
    /// <param name="directoryChoice">The directory path to scan for files.</param>
    static void ListFilesInDirectory(string directoryChoice)
    {
        ConfigData config = Config.Load();
        while (true)
        {
            string[] files = Directory.GetFiles(directoryChoice)
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .ToArray();
            string[] configSection =
            [
                "Config",
                "----------------"
            ];
            string[] choices = configSection.Concat(files).Append("Exit").ToArray();

            var fileChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"> File\n\n[{config.Colors.HighlightA}]Choose a file[/]")
                    .PageSize(10)
                    .HighlightStyle($"{config.Colors.HighlightB}")
                    .AddChoices(choices));

            if (fileChoice == choices[0])
            {
                Config.List(config);
                continue;
            }

            if (fileChoice == choices[1])
            {
                // ----------------
                continue;
            }

            if (fileChoice == choices[^1])
            {
                return;
            }

            ListActionsOnFile(fileChoice, config);

            AnsiConsole.WriteLine();
        }
    }

    /// <summary>
    /// Displays a list of actions the user can perform on the selected file.
    /// </summary>
    /// <param name="fileChoice">The selected file name.</param>
    /// <param name="config">The loaded configuration data.</param>
    /// <returns>True if user chooses to go back, false otherwise.</returns>
    static void ListActionsOnFile(string fileChoice, ConfigData config)
    {
        List<string> matchValues = new List<string>();
        while (true)
        {
            string path = $"> {fileChoice} > Actions";

            string[] actions =
            [
                "View contents",
                "List",
                "Replace",
                "Back"
            ];
            if (matchValues != null)
            {
                actions = new[] { "dump matches" }.Concat(actions).ToArray();
            }

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{path}\n\nWhat would you like to do with [{config.Colors.HighlightA}]{fileChoice}[/]?")
                    .HighlightStyle($"{config.Colors.HighlightB}")
                    .AddChoices(actions));

            if (action == "dump matches")
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter("out.txt", false))
                    {
                        writer.WriteLine(string.Join("\n", matchValues!));
                    }

                    Logger.LogSuccess("Dumped matches to out.txt");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error writing to out.txt: " + ex.Message);
                }
            }
            else if (action == "View contents")
            {
                ViewContents(fileChoice);
            }
            else if (action == "List")
            {
                matchValues = ListOptions(fileChoice, config);
            }
            else if (action == "Replace")
            {
                ListOptions(fileChoice, config, true);
            }
            else if (action == "Back")
            {
                return;
            }
        }
    }


    /// <summary>
    /// Displays the full content of the selected file.
    /// </summary>
    static void ViewContents(string fileChoice)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        try
        {
            var contents = File.ReadAllText(fileChoice);

            AnsiConsole.WriteLine($"{contents}");
            AnsiConsole.WriteLine("Took " + sw.ElapsedMilliseconds + " ms");
        }
        catch (Exception ex)
        {
            Logger.LogError("Error reading file: " + ex.Message);
        }
    }

    /// <summary>
    /// Displays a list of regexes.
    /// </summary>
    static List<string> ListOptions(string fileChoice, ConfigData config, bool replace = false)
    {
        string path = $"> {fileChoice}> Actions > {(replace ? "Replace" : "List")}";
        string[] premadeActions =
        [
            "text in html tags",
            "function signature"
        ];
        string[] customActions =
        [
            "Custom regex 1",
            "Custom regex 2",
            "Custom regex 3",
            "Regex pattern",
            "Back"
        ];
        string[] actions = premadeActions.Concat(customActions).ToArray();

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(
                    $"{path}\n\nWhat would you like to do with [{config.Colors.HighlightA}]{fileChoice}[/]?")
                .HighlightStyle($"{config.Colors.HighlightB}")
                .AddChoices(actions));

        string pattern = "";
        if (action == actions[0])
        {
            pattern = @"(?<=>)[^<>\n]+(?=<)";
        }
        else if (action == actions[1])
        {
            pattern =
                @"(?<= *)((((public)|(static)|(private)|(protected)|(override)|(virtual)|(extern)|(internal)) )+(\w{1}\S*)(\w{1}\S*) \w{1}\S*\(.*\))";
        }

        if (action == actions[2])
        {
            pattern = config.CustomRegex.Custom1;
        }
        else if (action == actions[3])
        {
            pattern = config.CustomRegex.Custom2;
        }

        if (action == actions[4])
        {
            pattern = config.CustomRegex.Custom3;
        }

        else if (action == actions[^2])
        {
            pattern = AnsiConsole.Ask<string>($"Give me [{config.Colors.HighlightB}]regex pattern[/] to match");
        }
        else if (action == actions[^1])
        {
            return null!;
        }

        Stopwatch sw = new Stopwatch();
        List<string> matches = new List<string>();
        try
        {
            if (replace)
            {
                string replacement =
                    AnsiConsole.Ask<string>($"Give me the [{config.Colors.HighlightB}]replacement[/]");
                sw.Start();
                ReplaceTextByPattern(fileChoice, pattern, replacement, config);
            }
            else
            {
                sw.Start();
                matches = ListLinesByPattern(fileChoice, pattern, config);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"Took [yellow]{sw.ElapsedMilliseconds.ToString()}[/] ms");
            return matches;
        }
        catch (RegexMatchTimeoutException ex)
        {
            Logger.LogError("Regex match timeout: " + ex.Message);
            return null!;
        }
        catch (FileNotFoundException ex)
        {
            Logger.LogError("File not found: " + ex.Message);
            return null!;
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.LogError("Directory not found: " + ex.Message);
            return null!;
        }
        catch (Exception ex)
        {
            Logger.LogError("Error: " + ex.Message);
            return null!;
        }
    }

    /// <summary>
    /// Lists all lines that match the regex pattern.
    /// </summary>
    static List<string> ListLinesByPattern(string fileChoice, string pattern, ConfigData config)
    {
        List<string> matchValues = new List<string>();
        RegexOptions options = RegexOptions.Multiline;
        int matchCount = 0;
        var lines = File.ReadAllLines(fileChoice);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var matches = Regex.Matches(line, pattern, options);
            if (matches.Count > 0)
            {
                matchCount += matches.Count;

                line = line.Replace("[", "[[").Replace("]", "]]");


                var escaped = line.EscapeMarkup();
                var highlightedLine = Regex.Replace(
                    escaped,
                    pattern,
                    match =>
                    {
                        matchValues.Add(match.Value);
                        return $"[{config.Colors.HighlightC}]{match.Value.EscapeMarkup()}[/]";
                    },
                    options
                );

                try
                {
                    AnsiConsole.MarkupLine($"[{config.Colors.HighlightB}]Line {i + 1}[/]: {highlightedLine}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{highlightedLine} {e}");
                }
            }
        }

        AnsiConsole.Markup($"[{config.Colors.HighlightC}]Found {matchCount} matches[/]\n");
        return matchValues;
    }

    /// <summary>
    /// Replaces all matches of the regex pattern in the file with the given replacement.
    /// </summary>
    static void ReplaceTextByPattern(string fileChoice, string pattern, string replacement,
        ConfigData config)
    {
        var contents = File.ReadAllText(fileChoice);
        int replacementCount = 0;
        var newContents = Regex.Replace(contents, pattern, _ =>
        {
            replacementCount++;
            return replacement;
        });

        AnsiConsole.MarkupLine(
            $"[{config.Colors.HighlightC}]{replacementCount} replacement{(replacementCount != 1 ? "s" : "")} made[/] in [{config.Colors.HighlightA}]{fileChoice}[/]");

        File.WriteAllText(fileChoice, newContents);
    }
}