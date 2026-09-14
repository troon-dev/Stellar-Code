using System.Collections.Generic;

namespace Stellar.Launcher.Downloader;

public sealed class Report
{
	public int TotalChunks { get; set; }

	public int BadChunks { get; set; }

	public long CheckedBytes { get; set; }

	public bool Healthy { get; set; }

	public List<string> MissingFiles { get; set; } = new List<string>();

}
