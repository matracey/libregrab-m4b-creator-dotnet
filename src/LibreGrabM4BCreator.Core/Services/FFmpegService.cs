using System.Text;

using FFMpegCore;

using Instances;

using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Core.Services;

/// <summary>
/// Service for interacting with FFmpeg/FFprobe.
/// </summary>
public sealed class FFmpegService
{
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private string? _cachedEncoder;

    public FFmpegService()
    {
        _ffmpegPath = GlobalFFOptions.GetFFMpegBinaryPath();
        _ffprobePath = GlobalFFOptions.GetFFProbeBinaryPath();
    }

    /// <summary>
    /// Checks if FFmpeg and FFprobe are available.
    /// </summary>
    public (bool Available, string? ErrorMessage) CheckDependencies()
    {
        try
        {
            if (!File.Exists(_ffmpegPath) && !IsExecutableInPath("ffmpeg"))
            {
                return (false, "FFmpeg not found. Please install FFmpeg and ensure it's in your PATH.");
            }

            if (!File.Exists(_ffprobePath) && !IsExecutableInPath("ffprobe"))
            {
                return (false, "FFprobe not found. Please install FFprobe and ensure it's in your PATH.");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error checking FFmpeg dependencies: {ex.Message}");
        }
    }

    private static bool IsExecutableInPath(string executable)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var paths = pathEnv.Split(Path.PathSeparator);

        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, executable);
            if (File.Exists(fullPath))
            {
                return true;
            }

            // Check with common extensions on macOS/Linux
            if (File.Exists(fullPath + ".exe"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Detects the best available AAC encoder.
    /// Priority: aac_at (Apple AudioToolbox) > libfdk_aac > aac (native)
    /// </summary>
    /// <remarks>
    /// This method uses direct process invocation because FFMpegCore does not expose
    /// an API to query available encoders. The -encoders flag output format is not
    /// part of the library's parsed output model.
    /// </remarks>
    public async Task<string> GetBestAacEncoderAsync()
    {
        if (_cachedEncoder is not null)
        {
            return _cachedEncoder;
        }

        var encoders = new[] { "aac_at", "libfdk_aac", "aac" };

        foreach (var encoder in encoders)
        {
            if (!await IsEncoderAvailableAsync(encoder))
            {
                continue;
            }

            _cachedEncoder = encoder;
            return encoder;
        }

        // Fallback to native AAC (should always be available)
        _cachedEncoder = "aac";
        return _cachedEncoder;
    }

    /// <summary>
    /// Checks if a specific encoder is available in FFmpeg.
    /// </summary>
    /// <remarks>
    /// Uses direct process invocation because FFMpegCore does not expose encoder availability queries.
    /// </remarks>
    private async Task<bool> IsEncoderAvailableAsync(string encoderName)
    {
        try
        {
            var ffmpegPath = _ffmpegPath.Length > 0 ? _ffmpegPath : "ffmpeg";
            var processArguments = new ProcessArguments(ffmpegPath, "-hide_banner -encoders");
            // Use Instances namespace explicitly to avoid ambiguity with FFMpegCore extension
            var result = await Instances.ProcessArgumentsExtensions.StartAndWaitForExitAsync(processArguments);
            var output = string.Join("\n", result.OutputData);

            // Look for the encoder in the output
            // Format is like: " A..... aac              AAC (Advanced Audio Coding)"
            return output.Contains($" {encoderName} ") || output.Contains($" {encoderName}\t");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the audio bitrate in kbps from a file.
    /// </summary>
    public async Task<int> GetBitrateKbpsAsync(string filePath)
    {
        var analysis = await FFProbe.AnalyseAsync(filePath);
        var audioStream = analysis.AudioStreams.FirstOrDefault();

        if (audioStream is null)
        {
            return 128; // Default fallback
        }

        // FFMpegCore returns bitrate in bits per second
        var bitrate = audioStream.BitRate;
        if (bitrate > 0)
        {
            return (int)(bitrate / 1000);
        }

        // If audio stream bitrate is not available, use the overall bitrate
        if (analysis.Format.BitRate > 0)
        {
            return (int)(analysis.Format.BitRate / 1000);
        }

        return 128; // Default fallback
    }

    /// <summary>
    /// Gets the number of audio channels from a file.
    /// </summary>
    public async Task<int> GetChannelsAsync(string filePath)
    {
        var analysis = await FFProbe.AnalyseAsync(filePath);
        var audioStream = analysis.AudioStreams.FirstOrDefault();
        return audioStream?.Channels ?? 2;
    }

    /// <summary>
    /// Gets the duration of an audio file in seconds.
    /// </summary>
    public async Task<double> GetDurationSecondsAsync(string filePath)
    {
        var analysis = await FFProbe.AnalyseAsync(filePath);
        return analysis.Duration.TotalSeconds;
    }

    /// <summary>
    /// Creates an FFmpeg concat demuxer file list.
    /// </summary>
    public async Task<string> CreateConcatFileListAsync(IEnumerable<FileInfo> files, string tempDirectory)
    {
        var listPath = Path.Combine(tempDirectory, "concat_list.txt");
        var sb = new StringBuilder();

        foreach (var file in files)
        {
            // Normalize path to use forward slashes and escape single quotes
            var normalizedPath = file.FullName.Replace('\\', '/');
            var escapedPath = normalizedPath.Replace("'", "'\\''");
            sb.AppendLine($"file '{escapedPath}'");
        }

        await File.WriteAllTextAsync(listPath, sb.ToString());
        return listPath;
    }

    /// <summary>
    /// Creates an FFmpeg chapter metadata file.
    /// </summary>
    public async Task<string> CreateChapterMetadataFileAsync(
        IEnumerable<Models.Chapter> chapters,
        string? title,
        string? author,
        string tempDirectory)
    {
        var metadataPath = Path.Combine(tempDirectory, "chapters.txt");
        var sb = new StringBuilder();

        sb.AppendLine(";FFMETADATA1");

        if (!string.IsNullOrWhiteSpace(title))
        {
            sb.AppendLine($"title={EscapeMetadataValue(title)}");
            sb.AppendLine($"album={EscapeMetadataValue(title)}");
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            sb.AppendLine($"artist={EscapeMetadataValue(author)}");
        }

        sb.AppendLine($"genre=Audiobook");
        sb.AppendLine($"date={DateTime.Now.Year}");
        sb.AppendLine();

        foreach (var chapter in chapters)
        {
            sb.AppendLine("[CHAPTER]");
            sb.AppendLine("TIMEBASE=1/1");
            sb.AppendLine($"START={chapter.StartSecondsInt}");
            sb.AppendLine($"END={chapter.EndSecondsInt}");
            sb.AppendLine($"title={EscapeMetadataValue(chapter.Title)}");
            sb.AppendLine();
        }

        await File.WriteAllTextAsync(metadataPath, sb.ToString());
        return metadataPath;
    }

    private static string EscapeMetadataValue(string value)
    {
        // Escape special characters for FFmpeg metadata
        return value
            .Replace("\\", "\\\\")
            .Replace("=", "\\=")
            .Replace(";", "\\;")
            .Replace("#", "\\#")
            .Replace("\n", "\\\n");
    }

    /// <summary>
    /// Converts audio files to M4B format.
    /// </summary>
    /// <remarks>
    /// Uses FFMpegCore's fluent API for argument building and process management.
    /// Custom arguments are used for options not directly supported by FFMpegCore's typed API:
    /// -map_chapters, -brand, -aac_coder, -avoid_negative_ts, -fflags, -profile:a
    /// </remarks>
    public async Task<bool> ConvertToM4BAsync(
        string concatListPath,
        string chapterMetadataPath,
        string outputPath,
        AudiobookInfo audiobook,
        ConversionOptions options,
        Action<double>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        var encoder = await GetBestAacEncoderAsync();
        var customArgs = BuildCustomArguments(audiobook, options, encoder);
        var totalDuration = TimeSpan.FromSeconds(audiobook.TotalDurationSeconds);

        try
        {
            var result = await FFMpegArguments
                .FromFileInput(concatListPath, verifyExists: false, opts => opts
                    .ForceFormat("concat")
                    .WithCustomArgument("-safe 0"))
                .AddFileInput(chapterMetadataPath)
                .OutputToFile(outputPath, overwrite: true, opts => opts
                    .WithCustomArgument(customArgs))
                .NotifyOnProgress(progress => onProgress?.Invoke(Math.Min(progress, 100)), totalDuration)
                .CancellableThrough(cancellationToken)
                .ProcessAsynchronously(throwOnError: false);

            return result;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private static string BuildCustomArguments(
        AudiobookInfo audiobook,
        ConversionOptions options,
        string encoder)
    {
        var sb = new StringBuilder();

        // Map inputs
        sb.Append("-map 0:a -map_metadata 1 -map_chapters 1 ");

        // Audio codec settings
        if (options.TelegramMode)
        {
            sb.Append($"-c:a {encoder} ");
            sb.Append("-profile:a aac_low ");
        }
        else
        {
            if (encoder == "aac")
            {
                // Use TwoLoop coder for native AAC encoder
                sb.Append("-c:a aac -aac_coder twoloop ");
            }
            else
            {
                sb.Append($"-c:a {encoder} ");
            }
        }

        // Bitrate and channels
        sb.Append($"-b:a {audiobook.SourceBitrateKbps}k ");
        sb.Append($"-ac {audiobook.SourceChannels} ");

        // Container flags
        sb.Append(options.TelegramMode
            ? "-movflags +faststart -avoid_negative_ts make_zero -fflags +genpts"
            : "-brand isom -movflags +faststart");

        return sb.ToString();
    }

    /// <summary>
    /// Verifies that the output file has extradata_size: 2 for Telegram compatibility.
    /// </summary>
    /// <remarks>
    /// Uses direct process invocation because FFMpegCore's FFProbe.Analyse does not
    /// expose the extradata_size field in its parsed output model.
    /// </remarks>
    public async Task<bool> VerifyTelegramExtradataAsync(string filePath)
    {
        try
        {
            var ffprobePath = _ffprobePath.Length > 0 ? _ffprobePath : "ffprobe";
            var processArguments = new ProcessArguments(
                ffprobePath,
                $"-v error -show_streams -select_streams a:0 \"{filePath}\"");

            // Use Instances namespace explicitly to avoid ambiguity with FFMpegCore extension
            var result = await Instances.ProcessArgumentsExtensions.StartAndWaitForExitAsync(processArguments);
            var output = string.Join("\n", result.OutputData);

            // Check for extradata_size=2
            return output.Contains("extradata_size=2");
        }
        catch
        {
            return false;
        }
    }
}