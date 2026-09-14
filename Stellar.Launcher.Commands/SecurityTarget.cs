namespace Stellar.Launcher.Commands;

public sealed class SecurityTarget
{
	public string Label { get; set; } = string.Empty;


	public string Path { get; set; } = string.Empty;


	public bool Exists { get; set; }
}
