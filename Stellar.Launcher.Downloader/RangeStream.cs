using System.Net.Http;

namespace Stellar.Launcher.Downloader;

public sealed class RangeStream
{
	public required HttpResponseMessage Response { get; init; }

	public required bool Resumed { get; init; }
}
