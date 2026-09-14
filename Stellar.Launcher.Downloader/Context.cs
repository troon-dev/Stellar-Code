using System.Net.Http;

namespace Stellar.Launcher.Downloader;

public sealed class Context
{
	public required HttpClient Client { get; init; }

	public required string ChunksUrl { get; init; }

	public required Control Control { get; init; }

	public required Counter DoneBytes { get; init; }
}
