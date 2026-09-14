using System.Collections.Generic;

namespace Stellar.Launcher.Downloader;

public sealed class Chunk
{
	public required string Hash { get; init; }

	public required long Size { get; init; }

	public required List<Span> Spans { get; init; }
}
