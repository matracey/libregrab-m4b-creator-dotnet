using ConsoleAppFramework;

using Humanizer;

using LibreGrabM4BCreator.Core.Abstractions;
using LibreGrabM4BCreator.Core.Models;
using LibreGrabM4BCreator.Core.Services;
using LibreGrabM4BCreator.Console.Services;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;

// Use ConsoleAppFramework for argument parsing
var app = ConsoleApp.Create();

// Configure DI services
app.ConfigureServices(services =>
{
    services.AddSingleton<FFmpegService>();
    services.AddSingleton<ConversionService>();
    services.AddSingleton<AudiobookDiscoveryService>();
    services.AddSingleton<IProgressReporter, SpectreProgressReporter>();
    services.AddSingleton<IUserInterface, SpectreUserInterface>();
});

app.Add("", Commands.Convert);
app.Run(args);

static class Commands
{
    /// <summary>
    /// Converts directories of MP3 files to M4B audiobooks. Most commonly used with audiobooks downloaded with LibreGrab.
    /// </summary>
    /// <param name="ffmpegService">The <see cref="FFmpegService"/> which handles FFmpeg operations.</param>
    /// <param name="conversionService">The <see cref="ConversionService"/> which handles the conversion logic.</param>
    /// <param name="directories">The input directories containing MP3 files.</param>
    /// <param name="outputDir">The output directory for M4B files. Defaults to the current directory.</param>
    /// <param name="telegram">Whether to use Telegram-compatible encoding settings.</param>
    public static async Task Convert(
        [FromServices] FFmpegService ffmpegService,
        [FromServices] ConversionService conversionService,
        [Argument] string[] directories,
        string? outputDir = null,
        bool telegram = false
    )

    {
        // Display banner
        AnsiConsole.Write(new FigletText("M4B Creator")
            .Color(Color.Blue));
        AnsiConsole.MarkupLine("[grey]LibreGrab M4B Audiobook Creator[/]\n");

        // Check FFmpeg dependencies
        var (available, errorMessage) = ffmpegService.CheckDependencies();
        if (!available)
        {
            AnsiConsole.MarkupLine($"[red]Error: {errorMessage}[/]");
            AnsiConsole.MarkupLine("[yellow]Please install FFmpeg:[/]");
            AnsiConsole.MarkupLine("  macOS: [blue]brew install ffmpeg[/]");
            AnsiConsole.MarkupLine("  Linux: [blue]sudo apt install ffmpeg[/]");
            AnsiConsole.MarkupLine("  Windows: [blue]winget install ffmpeg[/]");
            return;
        }

        // Validate input directories
        if (directories.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Error: At least one input directory is required.[/]");
            AnsiConsole.MarkupLine(
                "[grey]Usage: m4b-creator [--output-dir <path>] [--telegram] <directory1> [directory2] ...[/]");
            return;
        }

        var inputDirectories = new List<DirectoryInfo>();
        foreach (var dir in directories)
        {
            var dirInfo = new DirectoryInfo(dir);
            if (!dirInfo.Exists)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning: Directory not found: {dir}[/]");
                continue;
            }

            inputDirectories.Add(dirInfo);
        }

        if (inputDirectories.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Error: No valid input directories found.[/]");
            return;
        }

        // Determine output directory
        var outputDirectory = outputDir is not null
            ? new DirectoryInfo(outputDir)
            : new DirectoryInfo(Environment.CurrentDirectory);

        // Create conversion options
        var options = new ConversionOptions { OutputDirectory = outputDirectory, TelegramMode = telegram };

        // Display mode
        if (telegram)
        {
            AnsiConsole.MarkupLine("[cyan]Mode: Telegram-compatible encoding[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[cyan]Mode: Standard encoding[/]");
        }

        AnsiConsole.MarkupLine($"[grey]Output directory: {outputDirectory.FullName}[/]");
        AnsiConsole.MarkupLine($"[grey]Processing {inputDirectories.Count} directory(ies)...[/]\n");

        // Process all directories
        var results = await conversionService.ConvertBatchAsync(
            inputDirectories,
            options);

        // Display final summary
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Conversion Summary[/]").RuleStyle("blue"));

        var successCount = Enumerable.Count(results, r => r.Success);
        var failCount = Enumerable.Count(results, r => !r.Success);

        if (successCount > 0)
        {
            AnsiConsole.MarkupLine($"[green]✓ Successful: {successCount}[/]");
        }

        if (failCount > 0)
        {
            AnsiConsole.MarkupLine($"[red]✗ Failed: {failCount}[/]");
        }

        var totalSize = Enumerable.Where(results, r => r.Success).Sum(r => r.FileSizeBytes);
        var totalDuration = Enumerable.Where(results, r => r.Success).Sum(r => r.DurationSeconds);

        if (successCount > 0)
        {
            AnsiConsole.MarkupLine($"[grey]Total size: {FormatFileSize(totalSize)}[/]");
            AnsiConsole.MarkupLine($"[grey]Total duration: {TimeSpan.FromSeconds(totalDuration):hh\\:mm\\:ss}[/]");
        }
    }

    /// <summary>
    /// Formats file size in bytes to a human-readable string.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <returns>A formatted file size string.</returns>
    private static string FormatFileSize(long bytes)
    {
        ReadOnlySpan<Func<double, ByteSize>> converters =
        [
            ByteSize.FromBytes, ByteSize.FromKilobytes, ByteSize.FromMegabytes, ByteSize.FromGigabytes,
            ByteSize.FromTerabytes
        ];
        var len = bytes;
        var idx = 0;
        while (len >= 1024 && idx < converters.Length - 1)
        {
            idx++;
            len /= 1024;
        }

        return converters[idx](len).Humanize();
    }
}