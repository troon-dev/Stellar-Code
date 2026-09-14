using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Microsoft.Win32;

namespace Stellar.Launcher.Native;

public static class SingleInstance
{
	private const string PIPE_NAME = "stellar-launcher-single-instance";

	private const string MUTEX_NAME = "Global\\dev.stellar.launcher";

	public const string SCHEME = "stellarv2";

	private static Mutex? guard;

	public static bool Claim()
	{
		guard = new Mutex(true, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bz(), out var result);
		return result;
	}

	public static void Forward(string[] args)
	{
		try
		{
			using NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bA(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ba(), PipeDirection.Out);
			namedPipeClientStream.Connect(2000);
			byte[] bytes = Encoding.UTF8.GetBytes(string.Join('\n', args));
			namedPipeClientStream.Write(bytes);
			namedPipeClientStream.Flush();
		}
		catch
		{
		}
	}

	public static void Listen(Action<string[]> handler)
	{
		Action<string[]> handler2 = handler;
		Thread thread = new Thread((ThreadStart)delegate
		{
			while (true)
			{
				try
				{
					using NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ba(), PipeDirection.In, 1, PipeTransmissionMode.Byte);
					namedPipeServerStream.WaitForConnection();
					using StreamReader streamReader = new StreamReader(namedPipeServerStream, Encoding.UTF8);
					string[] array = streamReader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
					if (array.Length != 0)
					{
						handler2(array);
					}
				}
				catch
				{
					Thread.Sleep(250);
				}
			}
		});
		thread.IsBackground = true;
		thread.Name = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bB();
		thread.Start();
	}

	public static void RegisterScheme(string scheme)
	{
		string processPath = Environment.ProcessPath;
		if (string.IsNullOrEmpty(processPath))
		{
			return;
		}
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bb() + scheme);
			registryKey.SetValue(string.Empty, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bC() + scheme);
			registryKey.SetValue(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bc(), string.Empty);
			using RegistryKey registryKey2 = registryKey.CreateSubKey(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bD());
			registryKey2.SetValue(string.Empty, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD() + processPath + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bd());
			using RegistryKey registryKey3 = registryKey.CreateSubKey(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bE());
			registryKey3.SetValue(string.Empty, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD() + processPath + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.be());
		}
		catch
		{
		}
	}

	public static List<string> DeepLinksFrom(string[] args)
	{
		return args.Where((string argument) => argument.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EP(), StringComparison.OrdinalIgnoreCase)).ToList();
	}
}
