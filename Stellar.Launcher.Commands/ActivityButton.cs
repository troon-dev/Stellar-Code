using System.Text.Json.Serialization;

namespace Stellar.Launcher.Commands;

public sealed class ActivityButton
{
	[JsonPropertyName("label")]
	public string Label { get; set; } = string.Empty;


	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

}
