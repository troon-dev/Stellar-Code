using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Downloader;

public sealed class Session
{
	public readonly object Gate = new object();

	public required Control Control { get; init; }

	public Snapshot Snapshot { get; set; } = Stellar.Launcher.Downloader.Snapshot.Simple(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cx(), string.Empty);

}
