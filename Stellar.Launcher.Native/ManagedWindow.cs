using System;
using Photino.NET;

namespace Stellar.Launcher.Native;

public sealed class ManagedWindow
{
	public required string Label { get; init; }

	public required PhotinoWindow Window { get; init; }

	public nint Handle => Window.WindowHandle;

	public void Post(Action action)
	{
		Window.Invoke(action);
	}
}
