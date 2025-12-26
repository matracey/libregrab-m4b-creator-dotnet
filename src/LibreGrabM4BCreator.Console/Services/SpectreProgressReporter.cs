using Spectre.Console;

using LibreGrabM4BCreator.Core.Abstractions;

namespace LibreGrabM4BCreator.Console.Services;

/// <summary>
/// Spectre.Console implementation of IProgressReporter.
/// </summary>
public sealed class SpectreProgressReporter : IProgressReporter
{
    public async Task<bool> ExecuteWithProgressAsync(string taskDescription, Func<Action<double>, Task<bool>> operation)
    {
        var success = false;
        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[green]{taskDescription}[/]");
                task.MaxValue = 100;

                success = await operation(progress => task.Value = progress);

                task.Value = 100;
            });
            
        return success;
    }
}