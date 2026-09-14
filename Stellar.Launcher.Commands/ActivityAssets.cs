using System.Text.Json.Serialization;

namespace Stellar.Launcher.Commands;

public sealed class ActivityAssets
{
	[JsonPropertyName("largeImage")]
	public string? LargeImage { get; set; }

	[JsonPropertyName("largeText")]
	public string? LargeText { get; set; }

	[JsonPropertyName("smallImage")]
	public string? SmallImage { get; set; }

	[JsonPropertyName("smallText")]
	public string? SmallText { get; set; }
}
