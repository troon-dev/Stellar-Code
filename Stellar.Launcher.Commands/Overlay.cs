using System;
using System.Runtime.InteropServices;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Native;

namespace Stellar.Launcher.Commands;

public static class Overlay
{
	private const double OVERLAY_WIDTH = 380.0;

	private const double OVERLAY_MARGIN = 16.0;

	private static readonly object cornerGate = new object();

	private static string corner = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dD();

	private static bool TargetMonitor(ManagedWindow window, out Win32.RECT area, out double scale)
	{
		area = default(Win32.RECT);
		scale = 1.0;
		nint num = Win32.MonitorFromWindow(window.Handle, 2u);
		if (num == IntPtr.Zero)
		{
			return false;
		}
		Win32.MONITORINFO mONITORINFO = default(Win32.MONITORINFO);
		mONITORINFO.cbSize = Marshal.SizeOf<Win32.MONITORINFO>();
		Win32.MONITORINFO lpmi = mONITORINFO;
		if (!Win32.GetMonitorInfo(num, ref lpmi))
		{
			return false;
		}
		area = lpmi.rcMonitor;
		if (Win32.GetDpiForMonitor(num, 0, out var dpiX, out var _) == 0 && dpiX != 0)
		{
			scale = (double)dpiX / 96.0;
		}
		return true;
	}

	private static void Place(ManagedWindow window, double? height)
	{
		if (TargetMonitor(window, out var area, out var scale))
		{
			int num = (int)Math.Round(16.0 * scale);
			int num2 = area.Right - area.Left;
			int num3 = area.Bottom - area.Top;
			(int, int) valueTuple;
			if (height.HasValue)
			{
				double valueOrDefault = height.GetValueOrDefault();
				valueTuple = ((int)Math.Round(380.0 * scale), (int)Math.Round(valueOrDefault * scale));
			}
			else
			{
				valueTuple = (window.Window.Width, window.Window.Height);
			}
			(int, int) tuple = valueTuple;
			string text;
			lock (cornerGate)
			{
				text = corner;
			}
			int x = (text.EndsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BN(), StringComparison.Ordinal) ? (area.Left + num2 - tuple.Item1 - num) : (area.Left + num));
			int y = (text.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dC(), StringComparison.Ordinal) ? (area.Top + num3 - tuple.Item2 - num) : (text.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dc(), StringComparison.Ordinal) ? (area.Top + (num3 - tuple.Item2) / 2) : (area.Top + num)));
			Win32.SetWindowPos(window.Handle, Win32.HWND_TOPMOST, x, y, tuple.Item1, tuple.Item2, 0x10u | ((!height.HasValue) ? 1u : 0u));
		}
	}

	public static void SetPosition(string position)
	{
		lock (cornerGate)
		{
			corner = position;
		}
		ManagedWindow overlay = AppWindows.Overlay;
		if (overlay != null)
		{
			Place(overlay, null);
		}
	}

	public static void Show()
	{
		ManagedWindow managedWindow = AppWindows.EnsureOverlay();
		if (managedWindow != null)
		{
			AppWindows.SetIgnoreCursorEvents(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax(), ignore: true);
			Place(managedWindow, null);
			AppWindows.ShowWithoutActivating(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax());
			Win32.SetWindowPos(managedWindow.Handle, Win32.HWND_TOPMOST, 0, 0, 0, 0, 19u);
		}
	}

	public static void Hide()
	{
		if (AppWindows.Overlay != null)
		{
			AppWindows.Hide(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax());
		}
	}

	public static void Resize(double height)
	{
		ManagedWindow overlay = AppWindows.Overlay;
		if (overlay != null)
		{
			double num = 720.0;
			if (TargetMonitor(overlay, out var area, out var scale))
			{
				num = (double)(area.Bottom - area.Top) / scale - 32.0;
			}
			double num2 = Math.Min(Math.Max(height, 1.0), num);
			Place(overlay, num2);
		}
	}

	public static void SetInteractive(bool interactive)
	{
		if (AppWindows.Overlay != null && (!interactive || AppWindows.IsVisible(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax())))
		{
			AppWindows.SetIgnoreCursorEvents(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax(), !interactive);
			if (interactive)
			{
				AppWindows.Focus(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax());
			}
		}
	}

	public static bool Visible()
	{
		if (AppWindows.Overlay != null)
		{
			return AppWindows.IsVisible(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax());
		}
		return false;
	}
}
