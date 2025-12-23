namespace LibreGrabM4BCreator.Core.Models;

/// <summary>
/// Options for M4B conversion.
/// </summary>
public sealed record ConversionOptions
{
    /// <summary>
    /// Output directory for the M4B file.
    /// </summary>
    public required DirectoryInfo OutputDirectory { get; init; }

    /// <summary>
    /// Whether to use Telegram-compatible encoding settings.
    /// </summary>
    public bool TelegramMode { get; init; }
}
