using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Downloader;

public static class DownloadState
{
	private const string STATE_FILE = ".stellar-download.json";

	public static string StatePath(string root)
	{
		return Path.Combine(root, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DA());
	}

	public static SaveState Load(string root)
	{
		string text = StatePath(root);
		try
		{
			return JsonSerializer.Deserialize<SaveState>(File.ReadAllText(text), Bridge.Options) ?? new SaveState();
		}
		catch
		{
			return new SaveState();
		}
	}

	public static void Save(string root, string version, string manifest, HashSet<string> completed)
	{
		List<string> list = completed.ToList();
		list.Sort(StringComparer.Ordinal);
		SaveState saveState = new SaveState
		{
			Version = version,
			Manifest = manifest,
			Completed = list
		};
		string text;
		try
		{
			text = JsonSerializer.Serialize(saveState, Bridge.Options);
		}
		catch (Exception ex)
		{
			throw new CommandFailure(ex.Message);
		}
		string text2 = StatePath(root);
		string text3 = Path.ChangeExtension(text2, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Da());
		try
		{
			File.WriteAllText(text3, text);
		}
		catch (Exception ex2)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DB() + ex2.Message);
		}
		try
		{
			File.Move(text3, text2, true);
		}
		catch (Exception ex3)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DB() + ex3.Message);
		}
	}

	public static void Clear(string root)
	{
		try
		{
			File.Delete(StatePath(root));
		}
		catch
		{
		}
		try
		{
			File.Delete(Path.ChangeExtension(StatePath(root), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Da()));
		}
		catch
		{
		}
		try
		{
			File.Delete(Path.Combine(root, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Db()));
		}
		catch
		{
		}
	}
}
