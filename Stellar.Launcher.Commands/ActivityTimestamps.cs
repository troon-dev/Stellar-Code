using System.Text.Json.Serialization;

namespace Stellar.Launcher.Commands;

public sealed class ActivityTimestamps
{
	[JsonPropertyName("start")]
	public long? Start { get; set; }

	[JsonPropertyName("end")]
	public long? End { get; set; }
}
