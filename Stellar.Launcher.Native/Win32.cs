using System;
using System.Runtime.InteropServices;

namespace Stellar.Launcher.Native;

internal static class Win32
{
	public struct MSG
	{
		public nint hwnd;

		public uint message;

		public nint wParam;

		public nint lParam;

		public uint time;

		public int ptX;

		public int ptY;
	}

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public struct MONITORINFO
	{
		public int cbSize;

		public RECT rcMonitor;

		public RECT rcWork;

		public uint dwFlags;
	}

	[StructLayout(0, CharSet = CharSet.Unicode)]
	public struct PROCESSENTRY32
	{
		public uint dwSize;

		public uint cntUsage;

		public uint th32ProcessID;

		public nint th32DefaultHeapID;

		public uint th32ModuleID;

		public uint cntThreads;

		public uint th32ParentProcessID;

		public int pcPriClassBase;

		public uint dwFlags;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szExeFile;
	}

	public const int SW_HIDE = 0;

	public const int SW_SHOWNOACTIVATE = 4;

	public const int SW_SHOW = 5;

	public const int SW_MINIMIZE = 6;

	public const int SW_RESTORE = 9;

	public const int SW_MAXIMIZE = 3;

	public const int GWL_EXSTYLE = -20;

	public const int WS_EX_LAYERED = 524288;

	public const int WS_EX_TRANSPARENT = 32;

	public const int WS_EX_TOOLWINDOW = 128;

	public const int WS_EX_NOACTIVATE = 134217728;

	public const uint SWP_NOSIZE = 1u;

	public const uint SWP_NOMOVE = 2u;

	public const uint SWP_NOACTIVATE = 16u;

	public const uint SWP_SHOWWINDOW = 64u;

	public const int WM_NCLBUTTONDOWN = 161;

	public const int HTCAPTION = 2;

	public const uint LWA_ALPHA = 2u;

	public const uint PROCESS_TERMINATE = 1u;

	public const uint TH32CS_SNAPPROCESS = 2u;

	public const uint CREATE_NO_WINDOW = 134217728u;

	public const uint CREATE_SUSPENDED = 4u;

	public static readonly nint HWND_TOPMOST = new IntPtr(-1);

	[DllImport("user32.dll")]
	public static extern bool ShowWindow(nint hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	public static extern bool IsWindowVisible(nint hWnd);

	[DllImport("user32.dll")]
	public static extern bool IsIconic(nint hWnd);

	[DllImport("user32.dll")]
	public static extern bool SetForegroundWindow(nint hWnd);

	[DllImport("user32.dll")]
	public static extern nint SetFocus(nint hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetWindowLong(nint hWnd, int nIndex);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll")]
	public static extern bool SetLayeredWindowAttributes(nint hWnd, uint crKey, byte bAlpha, uint dwFlags);

	[DllImport("user32.dll")]
	public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

	[DllImport("user32.dll")]
	public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

	[DllImport("user32.dll")]
	public static extern bool UnregisterHotKey(nint hWnd, int id);

	[DllImport("user32.dll")]
	public static extern int GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	public static extern bool PostThreadMessage(uint threadId, uint msg, nint wParam, nint lParam);

	[DllImport("kernel32.dll")]
	public static extern uint GetCurrentThreadId();

	[DllImport("user32.dll")]
	public static extern nint MonitorFromWindow(nint hWnd, uint dwFlags);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

	[DllImport("shcore.dll")]
	public static extern int GetDpiForMonitor(nint hMonitor, int dpiType, out uint dpiX, out uint dpiY);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern bool Process32FirstW(nint hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern bool Process32NextW(nint hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool CloseHandle(nint hObject);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool TerminateProcess(nint hProcess, uint uExitCode);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool GetDiskFreeSpaceExW(string lpDirectoryName, out ulong lpFreeBytesAvailableToCaller, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
}
