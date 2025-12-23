namespace LibreGrabM4BCreator.Core.Models;

/// <summary>
/// Represents a chapter in the audiobook with timing information.
/// </summary>
public sealed record Chapter
{
    /// <summary>
    /// The chapter title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Start time in seconds.
    /// </summary>
    public required double StartSeconds { get; init; }

    /// <summary>
    /// End time in seconds.
    /// </summary>
    public required double EndSeconds { get; init; }

    /// <summary>
    /// Start time as integer (for FFmpeg metadata).
    /// </summary>
    public long StartSecondsInt => (long)Math.Floor(StartSeconds);

    /// <summary>
    /// End time as integer (for FFmpeg metadata).
    /// </summary>
    public long EndSecondsInt => (long)Math.Floor(EndSeconds);
}
