using System.Text.Json;
using System.Text.RegularExpressions;

using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Core.Services;

/// <summary>
/// Service for discovering and analyzing audiobook directories.
/// </summary>
public sealed partial class AudiobookDiscoveryService
{
    private readonly FFmpegService _ffmpegService;

    public AudiobookDiscoveryService(FFmpegService ffmpegService)
    {
        _ffmpegService = ffmpegService;
    }

    /// <summary>
    /// Discovers audiobook information from a directory.
    /// </summary>
    public async Task<AudiobookInfo?> DiscoverAsync(DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            return null;
        }

        // Find MP3 files in the root (non-recursive)
        var mp3Files = directory
            .GetFiles("*.mp3", SearchOption.TopDirectoryOnly)
            .ToList();

        if (mp3Files.Count == 0)
        {
            return null;
        }

        // Sort files with natural ordering (handles non-zero-padded numbers)
        mp3Files = SortFilesNaturally(mp3Files);

        // Get audio properties from the first file
        var firstFile = mp3Files[0];
        var bitrate = await _ffmpegService.GetBitrateKbpsAsync(firstFile.FullName);
        var channels = await _ffmpegService.GetChannelsAsync(firstFile.FullName);

        // Try to load metadata.json
        var metadataPath = Path.Combine(directory.FullName, "metadata", "metadata.json");
        LibreGrabMetadata? metadata = null;

        if (File.Exists(metadataPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(metadataPath);
                metadata = JsonSerializer.Deserialize<LibreGrabMetadata>(json);
            }
            catch
            {
                // Ignore parsing errors, fall back to file-based chapters
            }
        }

        // Get file durations for chapter calculation
        var fileDurations = new List<double>();
        foreach (var file in mp3Files)
        {
            var duration = await _ffmpegService.GetDurationSecondsAsync(file.FullName);
            fileDurations.Add(duration);
        }

        var totalDuration = fileDurations.Sum();

        // Generate chapters
        IReadOnlyList<Chapter> chapters;
        string title;
        string? author;

        if (metadata?.Chapters is not null && metadata.Spine is not null)
        {
            // Use metadata-based chapters
            chapters = GenerateChaptersFromMetadata(metadata, totalDuration);
            title = metadata.Title ?? directory.Name;
            author = metadata.GetAuthor();
        }
        else
        {
            // Use file-based chapters
            chapters = GenerateChaptersFromFiles(mp3Files, fileDurations);
            title = directory.Name;
            author = null;
        }

        return new AudiobookInfo
        {
            SourceDirectory = directory,
            Title = title,
            Author = author,
            AudioFiles = mp3Files,
            Chapters = chapters,
            TotalDurationSeconds = totalDuration,
            SourceBitrateKbps = bitrate,
            SourceChannels = channels
        };
    }

    /// <summary>
    /// Sorts files using natural ordering (handles numbers without leading zeros).
    /// </summary>
    private static List<FileInfo> SortFilesNaturally(List<FileInfo> files)
    {
        return files
            .OrderBy(f => NaturalSortKey(f.Name))
            .ToList();
    }

    /// <summary>
    /// Creates a sort key that handles natural number ordering.
    /// </summary>
    private static string NaturalSortKey(string filename)
    {
        // Replace sequences of digits with zero-padded versions for correct sorting
        return NaturalSortRegex().Replace(filename, match =>
        {
            var number = match.Value;
            return number.PadLeft(20, '0');
        });
    }

    /// <summary>
    /// Generates chapters from LibreGrab metadata.
    /// </summary>
    private static IReadOnlyList<Chapter> GenerateChaptersFromMetadata(
        LibreGrabMetadata metadata,
        double totalDuration)
    {
        var chapters = new List<Chapter>();
        var spines = metadata.Spine!;
        var chapterInfos = metadata.Chapters!;

        // Pre-calculate cumulative spine durations
        var cumulativeSpineDurations = new double[spines.Count + 1];
        for (int i = 0; i < spines.Count; i++)
        {
            cumulativeSpineDurations[i + 1] = cumulativeSpineDurations[i] + spines[i].Duration;
        }

        for (int i = 0; i < chapterInfos.Count; i++)
        {
            var chapterInfo = chapterInfos[i];
            
            // Calculate start time: sum of spine durations before this spine + offset
            var startSeconds = 0.0;
            if (chapterInfo.Spine >= 0 && chapterInfo.Spine < cumulativeSpineDurations.Length)
            {
                startSeconds = cumulativeSpineDurations[chapterInfo.Spine] + chapterInfo.Offset;
            }

            // Calculate end time: either next chapter's start or total duration
            double endSeconds;
            if (i + 1 < chapterInfos.Count)
            {
                var nextChapter = chapterInfos[i + 1];
                endSeconds = cumulativeSpineDurations[nextChapter.Spine] + nextChapter.Offset;
            }
            else
            {
                endSeconds = totalDuration;
            }

            chapters.Add(new Chapter
            {
                Title = chapterInfo.Title ?? $"Chapter {i + 1}",
                StartSeconds = startSeconds,
                EndSeconds = endSeconds
            });
        }

        return chapters;
    }

    /// <summary>
    /// Generates chapters from MP3 file names and durations.
    /// </summary>
    private static IReadOnlyList<Chapter> GenerateChaptersFromFiles(
        List<FileInfo> files,
        List<double> durations)
    {
        var chapters = new List<Chapter>();
        var currentTime = 0.0;

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            var duration = durations[i];
            var title = Path.GetFileNameWithoutExtension(file.Name);

            chapters.Add(new Chapter
            {
                Title = title,
                StartSeconds = currentTime,
                EndSeconds = currentTime + duration
            });

            currentTime += duration;
        }

        return chapters;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NaturalSortRegex();
}
