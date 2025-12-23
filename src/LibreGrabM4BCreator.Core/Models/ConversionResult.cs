namespace LibreGrabM4BCreator.Core.Models;

/// <summary>
/// Result of an M4B conversion.
/// </summary>
public sealed record ConversionResult
{
    /// <summary>
    /// Whether the conversion was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The output M4B file (if successful).
    /// </summary>
    public FileInfo? OutputFile { get; init; }

    /// <summary>
    /// The audiobook title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Total duration in seconds.
    /// </summary>
    public double DurationSeconds { get; init; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// Error message (if failed).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Warning message (if any).
    /// </summary>
    public string? WarningMessage { get; init; }

    /// <summary>
    /// Whether the Telegram extradata check passed.
    /// </summary>
    public bool? TelegramExtradataValid { get; init; }

    /// <summary>
    /// Formatted file size string.
    /// </summary>
    public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

    /// <summary>
    /// Formatted duration string.
    /// </summary>
    public string DurationFormatted => TimeSpan.FromSeconds(DurationSeconds).ToString(@"hh\:mm\:ss");

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
