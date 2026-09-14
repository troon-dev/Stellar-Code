using System.Collections.Generic;

namespace Stellar.Launcher.Commands;

public sealed class SecurityStatus
{
	public string SmartAppControl { get; set; } = string.Empty;


	public bool IsAdmin { get; set; }

	public List<SecurityTarget> Targets { get; set; } = new List<SecurityTarget>();

}
