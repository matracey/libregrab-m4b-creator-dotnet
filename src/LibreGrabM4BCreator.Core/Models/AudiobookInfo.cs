namespace LibreGrabM4BCreator.Core.Models;

/// <summary>
/// Contains all information needed to create an M4B audiobook.
/// </summary>
public sealed record AudiobookInfo
{
    /// <summary>
    /// The source directory containing MP3 files.
    /// </summary>
    public required DirectoryInfo SourceDirectory { get; init; }

    /// <summary>
    /// The title of the audiobook.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The author of the audiobook.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Ordered list of MP3 files to concatenate.
    /// </summary>
    public required IReadOnlyList<FileInfo> AudioFiles { get; init; }

    /// <summary>
    /// List of chapters with timing information.
    /// </summary>
    public required IReadOnlyList<Chapter> Chapters { get; init; }

    /// <summary>
    /// Total duration in seconds.
    /// </summary>
    public required double TotalDurationSeconds { get; init; }

    /// <summary>
    /// Bitrate of the source audio in kbps (e.g., 128, 192, 320).
    /// </summary>
    public required int SourceBitrateKbps { get; init; }

    /// <summary>
    /// Number of audio channels in the source.
    /// </summary>
    public required int SourceChannels { get; init; }
}
