using System;

namespace Stellar.Launcher.Native;

internal sealed class Process
{
	public nint Handle { get; init; }

	public nint Thread { get; init; }

	public uint Id { get; init; }

	public void Kill()
	{
		if (Handle != IntPtr.Zero)
		{
			Win32.TerminateProcess(Handle, 1u);
		}
	}

	public void Release()
	{
		if (Thread != IntPtr.Zero)
		{
			Win32.CloseHandle(Thread);
		}
		if (Handle != IntPtr.Zero)
		{
			Win32.CloseHandle(Handle);
		}
	}
}
