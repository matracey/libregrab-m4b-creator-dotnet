using ConsoleAppFramework;

using Spectre.Console;

using FFMpegCore;

// Use ConsoleAppFramework for argument parsing
var app = ConsoleApp.Create();

app.Add("transcode", async (string input, string output) =>
{
    await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[green]Transcoding...[/]");

            // Example FFMpegCore usage
            var mediaInfo = await FFProbe.AnalyseAsync(input);
            var totalDuration = mediaInfo.Duration;

            await FFMpegArguments
                .FromFileInput(input)
                .OutputToFile(output, true, options => options
                    .WithVideoCodec("libx264")
                    .WithAudioCodec("aac"))
                .NotifyOnProgress(time => task.Value = time.TotalSeconds / totalDuration.TotalSeconds * 100)
                .ProcessAsynchronously();
        });

    AnsiConsole.MarkupLine("[bold blue]Done![/]");
});

app.Run(args);