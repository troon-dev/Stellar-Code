using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Stellar.Launcher.Native;

internal static class Chrome
{
	private delegate nint WindowProc(nint hWnd, uint message, nint wParam, nint lParam);

	private struct MARGINS
	{
		public int Left;

		public int Right;

		public int Top;

		public int Bottom;
	}

	private const int GWL_STYLE = -16;

	private const int WS_CAPTION = 12582912;

	private const int WS_THICKFRAME = 262144;

	private const int WS_SYSMENU = 524288;

	private const int WS_MINIMIZEBOX = 131072;

	private const int GWLP_WNDPROC = -4;

	private const int WM_NCCALCSIZE = 131;

	private const int WM_NCHITTEST = 132;

	private const int WM_NCACTIVATE = 134;

	private const int HTCLIENT = 1;

	private const int HTCAPTION = 2;

	private const int HTLEFT = 10;

	private const int HTRIGHT = 11;

	private const int HTTOP = 12;

	private const int HTTOPLEFT = 13;

	private const int HTTOPRIGHT = 14;

	private const int HTBOTTOM = 15;

	private const int HTBOTTOMLEFT = 16;

	private const int HTBOTTOMRIGHT = 17;

	private const uint SWP_FRAMECHANGED = 32u;

	private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

	private const int DWMWCP_ROUND = 2;

	private const int GRIP = 6;

	private static readonly Dictionary<nint, nint> originals = new Dictionary<nint, nint>();

	private static readonly List<WindowProc> pinned = new List<WindowProc>();

	public static int Caption => 2;

	[DllImport("dwmapi.dll")]
	private static extern int DwmExtendFrameIntoClientArea(nint hWnd, ref MARGINS margins);

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hWnd, int attribute, ref int value, int size);

	[DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
	private static extern nint SetWindowLongPtr(nint hWnd, int index, nint value);

	[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
	private static extern nint GetWindowLongPtr(nint hWnd, int index);

	[DllImport("user32.dll")]
	private static extern nint CallWindowProcW(nint previous, nint hWnd, uint message, nint wParam, nint lParam);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(nint hWnd, out Win32.RECT rect);

	public static void Apply(nint handle, bool resizable)
	{
		if (handle == IntPtr.Zero)
		{
			return;
		}
		int windowLong = Win32.GetWindowLong(handle, -16);
		windowLong |= 0xCA0000;
		if (resizable)
		{
			windowLong |= 0x40000;
		}
		Win32.SetWindowLong(handle, -16, windowLong);
		int value = 2;
		DwmSetWindowAttribute(handle, 33, ref value, 4);
		MARGINS mARGINS = default(MARGINS);
		mARGINS.Left = 0;
		mARGINS.Right = 0;
		mARGINS.Top = 1;
		mARGINS.Bottom = 0;
		MARGINS margins = mARGINS;
		DwmExtendFrameIntoClientArea(handle, ref margins);
		WindowProc windowProc = (nint hWnd, uint message, nint wParam, nint lParam) => Dispatch(hWnd, message, wParam, lParam, resizable);
		lock (originals)
		{
			if (originals.ContainsKey(handle))
			{
				return;
			}
			pinned.Add(windowProc);
			nint num = SetWindowLongPtr(handle, -4, Marshal.GetFunctionPointerForDelegate(windowProc));
			originals[handle] = num;
		}
		Win32.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 51u);
	}

	private static nint Dispatch(nint handle, uint message, nint wParam, nint lParam, bool resizable)
	{
		nint zero;
		lock (originals)
		{
			if (!originals.TryGetValue(handle, out zero))
			{
				zero = IntPtr.Zero;
			}
		}
		switch (message)
		{
		case 131u:
			if (wParam != IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			break;
		case 134u:
			return CallWindowProcW(zero, handle, message, wParam, new IntPtr(-1));
		case 132u:
			if (resizable)
			{
				return new IntPtr(HitTest(handle, lParam));
			}
			break;
		}
		if (zero != IntPtr.Zero)
		{
			return CallWindowProcW(zero, handle, message, wParam, lParam);
		}
		return IntPtr.Zero;
	}

	private static int HitTest(nint handle, nint lParam)
	{
		if (!GetWindowRect(handle, out var rect))
		{
			return 1;
		}
		short num = (short)(((IntPtr)lParam).ToInt64() & 0xFFFF);
		short num2 = (short)((((IntPtr)lParam).ToInt64() >> 16) & 0xFFFF);
		bool flag = num < rect.Left + 6;
		bool flag2 = num >= rect.Right - 6;
		bool flag3 = num2 < rect.Top + 6;
		bool flag4 = num2 >= rect.Bottom - 6;
		if (flag3 && flag)
		{
			return 13;
		}
		if (flag3 && flag2)
		{
			return 14;
		}
		if (flag4 && flag)
		{
			return 16;
		}
		if (flag4 && flag2)
		{
			return 17;
		}
		if (flag)
		{
			return 10;
		}
		if (flag2)
		{
			return 11;
		}
		if (flag3)
		{
			return 12;
		}
		if (flag4)
		{
			return 15;
		}
		return 1;
	}
}
