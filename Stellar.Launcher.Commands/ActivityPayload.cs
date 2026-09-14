using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Stellar.Launcher.Commands;

public sealed class ActivityPayload
{
	[JsonPropertyName("state")]
	public string? State { get; set; }

	[JsonPropertyName("details")]
	public string? Details { get; set; }

	[JsonPropertyName("type")]
	public int? Type { get; set; }

	[JsonPropertyName("timestamps")]
	public ActivityTimestamps? Timestamps { get; set; }

	[JsonPropertyName("assets")]
	public ActivityAssets? Assets { get; set; }

	[JsonPropertyName("buttons")]
	public List<ActivityButton> Buttons { get; set; } = new List<ActivityButton>();

}
