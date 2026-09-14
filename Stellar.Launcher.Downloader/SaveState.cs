using System.Collections.Generic;

namespace Stellar.Launcher.Downloader;

public sealed class SaveState
{
	public string Version { get; set; } = string.Empty;


	public string Manifest { get; set; } = string.Empty;


	public List<string> Completed { get; set; } = new List<string>();

}
