using Spectre.Console;

namespace CzuProjekt;

public static class Logger
{
    private const string LogFileName = "log.txt";
    private const string LogFolderName = "CzuProjekt";

    private static readonly string LogFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LogFolderName);

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

    private static void LogToFile(string message)
    {
        try
        {
            if (!Directory.Exists(LogFilePath))
            {
                Directory.CreateDirectory(LogFilePath);
            }

            string logFullPath = Path.Combine(LogFilePath, LogFileName);

            // Write to log file - this will create it if it doesn't exist
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