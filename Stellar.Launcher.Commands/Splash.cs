using System;
using System.IO;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Downloader;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Commands;

public static class Splash
{
	private static readonly string[] SPLASH_DIR = new string[2]
	{
		_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.di(),
		_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.du()
	};

	private const string SPLASH_FILE = "Splash.png";

	private const string BACKUP_FILE = "Splash.original.png";

	private static string SplashDir(string build)
	{
		string text = build;
		string[] sPLASH_DIR = SPLASH_DIR;
		foreach (string text2 in sPLASH_DIR)
		{
			text = Path.Combine(text, text2);
		}
		return text;
	}

	private static void EnsureBackup(string dir)
	{
		string text = Path.Combine(dir, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dr());
		string text2 = Path.Combine(dir, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dS());
		if (File.Exists(text2) || !File.Exists(text))
		{
			return;
		}
		try
		{
			File.Copy(text, text2);
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ds() + ex.Message);
		}
	}

	public static object SplashStatus(string build)
	{
		string text = SplashDir(build);
		return new
		{
			supported = Directory.Exists(text),
			custom = File.Exists(Path.Combine(text, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dS()))
		};
	}

	public static async Task SplashApply(string build, string url)
	{
		string dir = SplashDir(build);
		if (!Directory.Exists(dir))
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FI());
		}
		EnsureBackup(dir);
		byte[] array;
		try
		{
			array = await Net.FetchBytesResilient(url).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fi() + ex.Message);
		}
		if (array.Length < 8 || array[0] != 137 || array[1] != 80 || array[2] != 78 || array[3] != 71 || array[4] != 13 || array[5] != 10 || array[6] != 26 || array[7] != 10)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FJ());
		}
		try
		{
			await File.WriteAllBytesAsync(Path.Combine(dir, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dr()), array).ConfigureAwait(false);
		}
		catch (Exception ex2)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fj() + ex2.Message);
		}
	}

	public static void SplashRestore(string build)
	{
		string text = SplashDir(build);
		string text2 = Path.Combine(text, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dS());
		if (!File.Exists(text2))
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dT());
		}
		try
		{
			File.Copy(text2, Path.Combine(text, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dr()), true);
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dt() + ex.Message);
		}
		try
		{
			File.Delete(text2);
		}
		catch (Exception ex2)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dU() + ex2.Message);
		}
	}
}
