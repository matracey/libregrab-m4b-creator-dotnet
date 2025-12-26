using Spectre.Console;

using LibreGrabM4BCreator.Core.Abstractions;

namespace LibreGrabM4BCreator.Console.Services;

/// <summary>
/// Spectre.Console implementation of IUserInterface.
/// </summary>
public sealed class SpectreUserInterface : IUserInterface
{
    public void DisplayInfo(string message)
    {
        AnsiConsole.MarkupLine($"[grey]{message}[/]");
    }

    public void DisplayProcessingStatus(string message)
    {
        AnsiConsole.MarkupLine($"[bold blue]{message}[/]");
    }

    public void DisplaySuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }

    public void DisplayWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{message}[/]");
    }

    public void DisplayError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }
}