using System;

namespace Stellar.Launcher.Ipc;

public sealed class CommandFailure : Exception
{
	public CommandFailure(string message)
		: base(message)
	{
	}
}
