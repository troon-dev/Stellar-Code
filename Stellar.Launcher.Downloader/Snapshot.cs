namespace Stellar.Launcher.Downloader;

public sealed class Snapshot
{
	public string State { get; set; } = string.Empty;


	public string Version { get; set; } = string.Empty;


	public string Path { get; set; } = string.Empty;


	public long DoneBytes { get; set; }

	public long TotalBytes { get; set; }

	public int DoneChunks { get; set; }

	public int TotalChunks { get; set; }

	public double Percent { get; set; }

	public double Speed { get; set; }

	public long Eta { get; set; }

	public string Message { get; set; } = string.Empty;


	public static Snapshot Simple(string state, string message)
	{
		return new Snapshot
		{
			State = state,
			Version = string.Empty,
			Path = string.Empty,
			DoneBytes = 0L,
			TotalBytes = 0L,
			DoneChunks = 0,
			TotalChunks = 0,
			Percent = 0.0,
			Speed = 0.0,
			Eta = 0L,
			Message = message
		};
	}

	public Snapshot Clone()
	{
		return (Snapshot)MemberwiseClone();
	}
}
