using System.Collections.Generic;

namespace Stellar.Launcher.Downloader;

public sealed class Plan
{
	public required List<Chunk> Chunks { get; init; }

	public required List<string> Empties { get; init; }

	public required List<(string Path, long Size)> Files { get; init; }

	public required long TotalBytes { get; init; }
}
