using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Ipc;

public static class Maintenance
{
	private static readonly TimeSpan TRIM_INTERVAL = TimeSpan.FromSeconds(1.0);

	private static readonly TimeSpan GC_INTERVAL = TimeSpan.FromSeconds(10.0);

	private static long lastGc;

	[DllImport("psapi.dll")]
	private static extern bool EmptyWorkingSet(nint hProcess);

	[DllImport("kernel32.dll")]
	private static extern nint GetCurrentProcess();

	[DllImport("user32.dll")]
	private static extern nint GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern int GetWindowThreadProcessId(nint hWnd, out int processId);

	public static void Start()
	{
		Thread thread = new Thread(Loop);
		thread.IsBackground = true;
		thread.Name = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CT();
		thread.Start();
	}

	private static bool IsForeground()
	{
		nint foregroundWindow = GetForegroundWindow();
		if (foregroundWindow == IntPtr.Zero)
		{
			return false;
		}
		GetWindowThreadProcessId(foregroundWindow, out var processId);
		return processId == Environment.ProcessId;
	}

	private static void Loop()
	{
		while (true)
		{
			Thread.Sleep(TRIM_INTERVAL);
			long tickCount = Environment.TickCount64;
			if (!IsForeground() && (double)(tickCount - Volatile.Read(ref lastGc)) >= GC_INTERVAL.TotalMilliseconds)
			{
				GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
				GC.Collect(2, GCCollectionMode.Forced, true, true);
				GC.WaitForPendingFinalizers();
				Volatile.Write(ref lastGc, tickCount);
			}
			try
			{
				EmptyWorkingSet(GetCurrentProcess());
			}
			catch
			{
			}
		}
	}
}
