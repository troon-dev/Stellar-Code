using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Microsoft.Win32;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Commands;

public static class Security
{
	private static string ReadSacState()
	{
		try
		{
			using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dd());
			if (registryKey == null)
			{
				return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dE();
			}
			object value = registryKey.GetValue(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.de());
			if (!(value is int))
			{
				return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dE();
			}
			return (int)value switch
			{
				0 => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dF(), 
				1 => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.df(), 
				2 => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dG(), 
				_ => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dg(), 
			};
		}
		catch
		{
			return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dE();
		}
	}

	private static bool RunningAsAdmin()
	{
		try
		{
			using WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
			return new WindowsPrincipal(windowsIdentity).IsInRole(WindowsBuiltInRole.Administrator);
		}
		catch
		{
			return false;
		}
	}

	private static string? CurrentExe()
	{
		return Environment.ProcessPath;
	}

	private static string? LauncherDir()
	{
		string text = CurrentExe();
		if (text != null)
		{
			return Path.GetDirectoryName(text);
		}
		return null;
	}

	private static List<SecurityTarget> CollectTargets(List<string> buildPaths)
	{
		List<SecurityTarget> list = new List<SecurityTarget>();
		string text = CurrentExe();
		if (text != null)
		{
			list.Add(new SecurityTarget
			{
				Label = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dH(),
				Path = text,
				Exists = File.Exists(text)
			});
		}
		string text2 = LauncherDir();
		if (text2 != null)
		{
			list.Add(new SecurityTarget
			{
				Label = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dh(),
				Path = text2,
				Exists = Directory.Exists(text2)
			});
		}
		foreach (string buildPath in buildPaths)
		{
			string fileName = Path.GetFileName(buildPath.TrimEnd('\\', '/'));
			list.Add(new SecurityTarget
			{
				Label = (string.IsNullOrEmpty(fileName) ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dI() : fileName),
				Path = buildPath,
				Exists = File.Exists(Path.Combine(buildPath, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.di(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.db()))
			});
		}
		return list;
	}

	public static SecurityStatus Status(List<string> buildPaths)
	{
		return new SecurityStatus
		{
			SmartAppControl = ReadSacState(),
			IsAdmin = RunningAsAdmin(),
			Targets = CollectTargets(buildPaths)
		};
	}

	public static List<string> ApplyExclusions(List<string> buildPaths)
	{
		List<string> list = (from target in CollectTargets(buildPaths)
			where target.Exists
			select target.Path).ToList();
		if (list.Count == 0)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dJ());
		}
		List<string> list2 = list.Select((string path) => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fd() + path.Replace(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fd(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FE()) + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fd()).ToList();
		string text = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dj() + string.Join(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dK(), list2);
		ProcessStartInfo processStartInfo = new ProcessStartInfo
		{
			FileName = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dk(),
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		processStartInfo.ArgumentList.Add(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dL());
		processStartInfo.ArgumentList.Add(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dl());
		processStartInfo.ArgumentList.Add(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dM());
		processStartInfo.ArgumentList.Add(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dm());
		processStartInfo.ArgumentList.Add(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dN());
		processStartInfo.ArgumentList.Add(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dn() + text.Replace(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dO()) + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD());
		Process process;
		try
		{
			process = Process.Start(processStartInfo) ?? throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.@do());
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dP() + ex.Message);
		}
		using (process)
		{
			string text2 = process.StandardError.ReadToEnd();
			process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				string text3 = text2.Trim();
				throw new CommandFailure((text3.Length == 0) ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dQ() : (_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dp() + text3));
			}
			return list;
		}
	}

	public static void OpenSmartAppControl()
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dq(),
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dR() + ex.Message);
		}
	}
}
