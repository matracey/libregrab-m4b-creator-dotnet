using System.Text.Json.Serialization;
using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Core.Serialization;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(LibreGrabMetadata))]
internal partial class LibreGrabJsonContext : JsonSerializerContext
{
}
