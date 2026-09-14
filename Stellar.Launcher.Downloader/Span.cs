namespace Stellar.Launcher.Downloader;

public sealed class Span
{
	public required string Path { get; init; }

	public required long Offset { get; init; }

	public required long Start { get; init; }

	public required long End { get; init; }
}
