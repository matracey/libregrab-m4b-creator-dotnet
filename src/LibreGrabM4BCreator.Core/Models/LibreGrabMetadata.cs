using System.Text.Json.Serialization;

namespace LibreGrabM4BCreator.Core.Models;

/// <summary>
/// Represents the LibreGrab metadata.json format.
/// </summary>
public sealed record LibreGrabMetadata
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("creator")]
    public List<Creator>? Creator { get; init; }

    [JsonPropertyName("spine")]
    public List<SpineItem>? Spine { get; init; }

    [JsonPropertyName("chapters")]
    public List<ChapterInfo>? Chapters { get; init; }

    /// <summary>
    /// Gets the primary author from the creator list.
    /// </summary>
    public string? GetAuthor()
    {
        return Creator?.FirstOrDefault(c =>
            string.Equals(c.Role, "author", StringComparison.OrdinalIgnoreCase))?.Name
            ?? Creator?.FirstOrDefault()?.Name;
    }
}

public sealed record Creator
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("role")]
    public string? Role { get; init; }
}

public sealed record SpineItem
{
    [JsonPropertyName("duration")]
    public double Duration { get; init; }
}

public sealed record ChapterInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("spine")]
    public int Spine { get; init; }

    [JsonPropertyName("offset")]
    public double Offset { get; init; }
}
