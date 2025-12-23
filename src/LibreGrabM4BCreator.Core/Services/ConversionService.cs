using System.Runtime.InteropServices;

using Instances;

using LibreGrabM4BCreator.Core.Abstractions;
using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Core.Services;

/// <summary>
/// Service for converting audiobooks to M4B format.
/// </summary>
public sealed class ConversionService
{
    private readonly FFmpegService _ffmpegService;
    private readonly AudiobookDiscoveryService _discoveryService;
    private readonly IProgressReporter _progressReporter;
    private readonly IUserInterface _userInterface;

    public ConversionService(
        FFmpegService ffmpegService,
        AudiobookDiscoveryService discoveryService,
        IProgressReporter progressReporter,
        IUserInterface userInterface)
    {
        _ffmpegService = ffmpegService;
        _discoveryService = discoveryService;
        _progressReporter = progressReporter;
        _userInterface = userInterface;
    }

    /// <summary>
    /// Converts an audiobook directory to M4B format.
    /// </summary>
    public async Task<ConversionResult> ConvertAsync(
        DirectoryInfo sourceDirectory,
        ConversionOptions options,
        CancellationToken cancellationToken = default)
    {
        string? tempDir = null;

        try
        {
            // Discover audiobook
            var audiobook = await _discoveryService.DiscoverAsync(sourceDirectory);

            if (audiobook is null)
            {
                return new ConversionResult
                {
                    Success = false,
                    Title = sourceDirectory.Name,
                    ErrorMessage = $"No MP3 files found in '{sourceDirectory.FullName}'"
                };
            }

            // Ensure output directory exists
            options.OutputDirectory.Create();

            // Create temporary directory for intermediate files
            tempDir = Path.Combine(Path.GetTempPath(), $"m4b-creator-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            // Create concat file list
            var concatListPath = await _ffmpegService.CreateConcatFileListAsync(
                audiobook.AudioFiles,
                tempDir);

            // Create chapter metadata file
            var chapterMetadataPath = await _ffmpegService.CreateChapterMetadataFileAsync(
                audiobook.Chapters,
                audiobook.Title,
                audiobook.Author,
                tempDir);

            // Determine output path
            var sanitizedTitle = SanitizeFileName(audiobook.Title);
            var outputPath = Path.Combine(
                options.OutputDirectory.FullName,
                $"{sanitizedTitle}.m4b");

            // Get the best encoder and report it
            var encoder = await _ffmpegService.GetBestAacEncoderAsync();

            _userInterface.DisplayInfo($"Using encoder: {encoder}");
            _userInterface.DisplayInfo(
                $"Source: {audiobook.SourceBitrateKbps} kbps, {audiobook.SourceChannels} channel(s)");
            _userInterface.DisplayInfo($"Chapters: {audiobook.Chapters.Count}");

            // Convert with progress
            var success = await _progressReporter.ExecuteWithProgressAsync(
                $"Converting {audiobook.Title}",
                async (progressCallback) => await _ffmpegService.ConvertToM4BAsync(
                    concatListPath,
                    chapterMetadataPath,
                    outputPath,
                    audiobook,
                    options,
                    progressCallback,
                    cancellationToken));

            if (!success)
            {
                return new ConversionResult
                {
                    Success = false,
                    Title = audiobook.Title,
                    ErrorMessage = "FFmpeg conversion failed. Check the console output for details."
                };
            }

            // Post-processing
            var outputFile = new FileInfo(outputPath);

            // Remove quarantine attribute on macOS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                await Instance.FinishAsync("xattr", "-d com.apple.quarantine " + outputPath);
            }

            // Verify Telegram extradata if in Telegram mode
            bool? telegramExtradataValid = null;
            string? warningMessage = null;

            if (options.TelegramMode)
            {
                telegramExtradataValid = await _ffmpegService.VerifyTelegramExtradataAsync(outputPath);
                if (!telegramExtradataValid.Value)
                {
                    warningMessage = "Telegram extradata verification failed (extradata_size != 2). " +
                                     "The file may not be fully compatible with Telegram.";
                }
            }

            return new ConversionResult
            {
                Success = true,
                OutputFile = outputFile,
                Title = audiobook.Title,
                DurationSeconds = audiobook.TotalDurationSeconds,
                FileSizeBytes = outputFile.Length,
                TelegramExtradataValid = telegramExtradataValid,
                WarningMessage = warningMessage
            };
        }
        catch (Exception ex)
        {
            return new ConversionResult
            {
                Success = false, Title = sourceDirectory.Name, ErrorMessage = $"Conversion failed: {ex.Message}"
            };
        }
        finally
        {
            // Clean up temporary files
            if (tempDir is not null && Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    /// <summary>
    /// Converts multiple audiobook directories to M4B format.
    /// </summary>
    public async Task<IReadOnlyList<ConversionResult>> ConvertBatchAsync(
        IEnumerable<DirectoryInfo> sourceDirectories,
        ConversionOptions options,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ConversionResult>();

        foreach (var directory in sourceDirectories)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            _userInterface.DisplayProcessingStatus($"\nProcessing: {directory.Name}");

            var result = await ConvertAsync(directory, options, cancellationToken);
            results.Add(result);

            // Display result summary
            DisplayResultSummary(result);
        }

        return results;
    }

    private void DisplayResultSummary(ConversionResult result)
    {
        if (result.Success)
        {
            _userInterface.DisplaySuccess($"✓ Completed: {result.Title}");
            _userInterface.DisplayInfo($"  Duration: {result.DurationFormatted}");
            _userInterface.DisplayInfo($"  Size: {result.FileSizeFormatted}");

            if (result.OutputFile is not null)
            {
                _userInterface.DisplayInfo($"  Output: {result.OutputFile.FullName}");
            }

            if (result.TelegramExtradataValid == true)
            {
                _userInterface.DisplaySuccess($"  Telegram extradata: Valid");
            }
            else if (result.TelegramExtradataValid == false)
            {
                _userInterface.DisplayWarning($"  ⚠ {result.WarningMessage}");
            }
        }
        else
        {
            _userInterface.DisplayError($"✗ Failed: {result.Title}");
            if (result.ErrorMessage is not null)
            {
                _userInterface.DisplayError($"  {result.ErrorMessage}");
            }
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        // Replace ':' with '_' and '"' with '\''
        fileName = fileName.Replace(":", "_").Replace("\"", "'");

        // Replace invalid chars with '_'
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName
            .Select(c => invalidChars.Contains(c) ? '_' : c)
            .ToArray());

        return sanitized.Trim();
    }
}