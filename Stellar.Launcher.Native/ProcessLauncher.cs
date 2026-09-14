using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Native;

internal static class ProcessLauncher
{
	[StructLayout(0, CharSet = CharSet.Unicode)]
	private struct STARTUPINFO
	{
		public int cb;

		public nint lpReserved;

		public nint lpDesktop;

		public nint lpTitle;

		public int dwX;

		public int dwY;

		public int dwXSize;

		public int dwYSize;

		public int dwXCountChars;

		public int dwYCountChars;

		public int dwFillAttribute;

		public uint dwFlags;

		public short wShowWindow;

		public short cbReserved2;

		public nint lpReserved2;

		public nint hStdInput;

		public nint hStdOutput;

		public nint hStdError;
	}

	private struct PROCESS_INFORMATION
	{
		public nint hProcess;

		public nint hThread;

		public uint dwProcessId;

		public uint dwThreadId;
	}

	private const uint STARTF_USESTDHANDLES = 256u;

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool CreateProcessW(string? lpApplicationName, StringBuilder lpCommandLine, nint lpProcessAttributes, nint lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, nint lpEnvironment, string? lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

	public static string Quote(string value)
	{
		if (value.Length > 0 && value.IndexOfAny(new char[3] { ' ', '\t', '"' }) < 0)
		{
			return value;
		}
		StringBuilder stringBuilder = new StringBuilder(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD());
		int num = 0;
		foreach (char c in value)
		{
			switch (c)
			{
			case '\\':
				num++;
				break;
			case '"':
				stringBuilder.Append('\\', num * 2 + 1);
				num = 0;
				stringBuilder.Append('"');
				break;
			default:
				stringBuilder.Append('\\', num);
				num = 0;
				stringBuilder.Append(c);
				break;
			}
		}
		stringBuilder.Append('\\', num * 2);
		stringBuilder.Append('"');
		return stringBuilder.ToString();
	}

	public static Process Spawn(string executable, IEnumerable<string> arguments, string workingDirectory, uint flags)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Quote(executable));
		foreach (string argument in arguments)
		{
			if (argument.Length != 0)
			{
				stringBuilder.Append(' ').Append(Quote(argument));
			}
		}
		STARTUPINFO sTARTUPINFO = default(STARTUPINFO);
		sTARTUPINFO.cb = Marshal.SizeOf<STARTUPINFO>();
		sTARTUPINFO.dwFlags = 256u;
		sTARTUPINFO.hStdInput = IntPtr.Zero;
		sTARTUPINFO.hStdOutput = IntPtr.Zero;
		sTARTUPINFO.hStdError = IntPtr.Zero;
		STARTUPINFO lpStartupInfo = sTARTUPINFO;
		if (!CreateProcessW(executable, stringBuilder, IntPtr.Zero, IntPtr.Zero, bInheritHandles: false, flags, IntPtr.Zero, workingDirectory, ref lpStartupInfo, out var lpProcessInformation))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		return new Process
		{
			Handle = lpProcessInformation.hProcess,
			Thread = lpProcessInformation.hThread,
			Id = lpProcessInformation.dwProcessId
		};
	}

	public static List<uint> FindByName(params string[] names)
	{
		List<uint> list = new List<uint>();
		nint num = Win32.CreateToolhelp32Snapshot(2u, 0u);
		if (num == IntPtr.Zero || num == new IntPtr(-1))
		{
			return list;
		}
		Win32.PROCESSENTRY32 pROCESSENTRY = default(Win32.PROCESSENTRY32);
		pROCESSENTRY.dwSize = (uint)Marshal.SizeOf<Win32.PROCESSENTRY32>();
		pROCESSENTRY.szExeFile = string.Empty;
		Win32.PROCESSENTRY32 lppe = pROCESSENTRY;
		if (Win32.Process32FirstW(num, ref lppe))
		{
			do
			{
				foreach (string text in names)
				{
					if (string.Equals(lppe.szExeFile, text, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(lppe.th32ProcessID);
						break;
					}
				}
			}
			while (Win32.Process32NextW(num, ref lppe));
		}
		Win32.CloseHandle(num);
		return list;
	}

	public static void KillPid(uint pid)
	{
		nint num = Win32.OpenProcess(1u, bInheritHandle: false, pid);
		if (num != IntPtr.Zero)
		{
			Win32.TerminateProcess(num, 1u);
			Win32.CloseHandle(num);
		}
	}
}
