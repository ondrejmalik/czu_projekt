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
                .Title($"> File\n\n[{config.colors.highlightA}]Choose a file[/]")
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


        //AnsiConsole.MarkupLine($"[{config.colors.highlightB}]Selected file: {file_choice}[/]");
        ListActionsOnFile(file_choice, config);

        AnsiConsole.WriteLine();
    }
}

static bool ListActionsOnFile(string file_choice, ConfigData config)
{
    List<string> matchValues = new List<string>();
    while (true)
    {
        string path = $"> {file_choice} > Actions";

        string[] actions = new string[] {
                "View contents",
                "List",
                "Replace",
                "Back"
        };
        if (matchValues != null)
        {
            actions = new[] { "dump matches" }.Concat(actions).ToArray();
        }
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"{path}\n\nWhat would you like to do with [{config.colors.highlightA}]{file_choice}[/]?")
                .HighlightStyle($"{config.colors.highlightB}")
                .AddChoices(actions));

        if (action == "dump matches")
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("out.txt", false))
                {
                    writer.WriteLine(string.Join("\n", matchValues));

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
            ViewContents(file_choice);
        }
        else if (action == "List")
        {
            matchValues = ListOptions(file_choice, config);
        }
        else if (action == "Replace")
        {
            ListOptions(file_choice, config, true);
        }
        else if (action == "Back")
        {
            return true;
        }

        static void ViewContents(string file_choice)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var contents = File.ReadAllText(file_choice);
            AnsiConsole.WriteLine($"{contents}");
            AnsiConsole.WriteLine("Took " + sw.ElapsedMilliseconds.ToString() + " ms");
        }

        static List<string> ListOptions(string file_choice, ConfigData config, bool replace = false)
        {
            string path = $"> {file_choice}> Actions > {(replace ? "Replace" : "List")}";
            string[] premadeActions = new string[] {
            "text in html tags",
            "Premade 2",

            };
            string[] customActions = new string[] {
                "Custom regex 1",
                "Custom regex 2",
                "Custom regex 3",
                "Regex pattern",
                "Back"
            };
            string[] actions = premadeActions.Concat(customActions).ToArray();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{path}\n\nWhat would you like to do with [{config.colors.highlightA}]{file_choice}[/]?")
                    .HighlightStyle($"{config.colors.highlightB}")
                    .AddChoices(actions));

            string pattern = "";
            if (action == actions[0])
            {
                pattern = "(?<=>)[^<>\n]+(?=<)";
            }
            else if (action == actions[1])
            {
                pattern = "premade pattern 2";
            }
            if (action == actions[2])
            {
                pattern = config.custom_regex.custom_1;
            }
            else if (action == actions[3])
            {
                pattern = config.custom_regex.custom_2;
            }
            if (action == actions[4])
            {
                pattern = config.custom_regex.custom_3;
            }

            else if (action == actions[^2])
            {
                pattern = AnsiConsole.Ask<string>($"Give me [{config.colors.highlightB}]regex pattern[/] to match");
            }
            else if (action == actions[^1])
            {
                return null;
            }

            Stopwatch sw = new Stopwatch();
            List<string> matches = new List<string>();

            if (replace)
            {
                string replacement = AnsiConsole.Ask<string>($"Give me the [{config.colors.highlightB}]replacement[/]");
                sw.Start();
                ReplaceTextByPattern(file_choice, pattern, replacement, config);
            }
            else
            {
                sw.Start();
                matches = ListLinesByPattern(file_choice, pattern, config);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"Took [yellow]{sw.ElapsedMilliseconds.ToString()}[/] ms");
            return matches;
        }

        static List<string> ListLinesByPattern(string file_choice, string pattern, ConfigData config)
        {
            List<string> matchValues = new List<string>();
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

                    var highlightedLine = Regex.Replace(line, pattern, match =>
                        {
                            matchValues.Add(match.Value);
                            return $"[{config.colors.highlightC}]{match.Value}[/]";
                        }, options);

                    AnsiConsole.Markup($"[{config.colors.highlightB}]Line {i + 1}[/]: {highlightedLine} \n");
                }
            }

            AnsiConsole.Markup($"[{config.colors.highlightC}]Found {matchCount} matches[/]\n");
            return matchValues;
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
    }
}
