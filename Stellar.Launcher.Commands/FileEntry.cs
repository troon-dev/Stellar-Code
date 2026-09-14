using System.Text.Json.Serialization;

namespace Stellar.Launcher.Commands;

public sealed class FileEntry
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;


	[JsonPropertyName("path")]
	public string Path { get; set; } = string.Empty;


	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

}
