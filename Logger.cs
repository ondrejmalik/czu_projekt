using Spectre.Console;

public static class Logger
{
    static string logFileName = "log.txt";
    static string logFolderName = "czu_projekt";
    static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), logFolderName);
    public static void LogError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
        LogToFile("Error - " + message);
    }

    public static void LogWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{message}[/]");
        LogToFile("Warning - " + message);
    }

    public static void LogSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
        LogToFile("Success - " + message);
    }

    public static void LogToFile(string message)
    {
        try
        {
            if (!Directory.Exists(logFilePath))
            {
                Directory.CreateDirectory(logFilePath);
            }
            string logFullPath = Path.Combine(logFilePath, logFileName);

            // Write to log file - this will create it if doesn't exist
            using (StreamWriter writer = File.AppendText(logFullPath))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }

            // Log the path for debugging
            Console.WriteLine($"Log file path: {logFullPath}");
        }
        catch (Exception ex)
        {
            // Log error to console instead of throwing an exception
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
}
