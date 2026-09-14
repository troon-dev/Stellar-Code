using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Downloader;
using Stellar.Launcher.Ipc;
using Stellar.Launcher.Native;

namespace Stellar.Launcher.Commands;

public static class Updater
{
	private sealed class Platform
	{
		[JsonPropertyName("signature")]
		public string Signature { get; set; } = string.Empty;


		[JsonPropertyName("url")]
		public string Url { get; set; } = string.Empty;

	}

	private sealed class Release
	{
		[JsonPropertyName("version")]
		public string Version { get; set; } = string.Empty;


		[JsonPropertyName("notes")]
		public string Notes { get; set; } = string.Empty;


		[JsonPropertyName("pub_date")]
		public string PubDate { get; set; } = string.Empty;


		[JsonPropertyName("url")]
		public string Url { get; set; } = string.Empty;


		[JsonPropertyName("signature")]
		public string Signature { get; set; } = string.Empty;


		[JsonPropertyName("platforms")]
		public Dictionary<string, Platform> Platforms { get; set; } = new Dictionary<string, Platform>();

	}

	private const string ENDPOINT = "https://prod-api-v1.stellarfn.dev/stellar/launcher/v2/update/{target}/{arch}/{version}";

	private static Release? cached;

	public static string CurrentVersion()
	{
		System.Version version = typeof(Updater).Assembly.GetName().Version;
		if ((object)version != null)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
			defaultInterpolatedStringHandler.AppendFormatted(version.Major);
			defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bA());
			defaultInterpolatedStringHandler.AppendFormatted(version.Minor);
			defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bA());
			defaultInterpolatedStringHandler.AppendFormatted(version.Build);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
		return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dV();
	}

	private static string Target()
	{
		return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dv();
	}

	private static string Arch()
	{
		if (!Environment.Is64BitProcess)
		{
			return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dW();
		}
		return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dw();
	}

	private static int Compare(string left, string right)
	{
		string[] array = left.TrimStart('v').Split('.');
		string[] array2 = right.TrimStart('v').Split('.');
		for (int i = 0; i < Math.Max(array.Length, array2.Length); i++)
		{
			int num;
			int num2 = ((i < array.Length && int.TryParse(array[i], out num)) ? num : 0);
			int num3;
			int num4 = ((i < array2.Length && int.TryParse(array2[i], out num3)) ? num3 : 0);
			if (num2 != num4)
			{
				return num2.CompareTo(num4);
			}
		}
		return 0;
	}

	private static (string Url, string Signature)? Artifact(Release release)
	{
		if (!string.IsNullOrEmpty(release.Url))
		{
			return (release.Url, release.Signature);
		}
		string text = Target() + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bW() + Arch();
		if (release.Platforms.TryGetValue(text, out Platform platform) && !string.IsNullOrEmpty(platform.Url))
		{
			return (platform.Url, platform.Signature);
		}
		Platform platform2 = release.Platforms.Values.FirstOrDefault((Platform entry) => !string.IsNullOrEmpty(entry.Url));
		if (platform2 != null)
		{
			return (platform2.Url, platform2.Signature);
		}
		return null;
	}

	public static async Task<object?> Check()
	{
		string current = CurrentVersion();
		string url = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fk().Replace(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FL(), Target()).Replace(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fl(), Arch())
			.Replace(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FM(), current);
		string text;
		try
		{
			text = await Net.FetchText(Net.BuildClient(), url, 2).ConfigureAwait(false);
		}
		catch
		{
			return null;
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		Release release;
		try
		{
			release = JsonSerializer.Deserialize<Release>(text);
		}
		catch
		{
			return null;
		}
		if (release == null || string.IsNullOrEmpty(release.Version) || Compare(release.Version, current) <= 0)
		{
			return null;
		}
		cached = release;
		return new
		{
			available = true,
			version = release.Version,
			currentVersion = current,
			date = release.PubDate,
			body = release.Notes
		};
	}

	public static async Task Install()
	{
		Release release = cached ?? throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fm());
		var (url, signature) = Artifact(release) ?? throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FN());
		if (string.IsNullOrEmpty(signature))
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fn());
		}
		byte[] array;
		try
		{
			array = await Net.FetchBytesResilient(url).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FO() + ex.Message);
		}
		if (!Minisign.Verify(array, signature))
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fo());
		}
		string staging = Path.Combine(Path.GetTempPath(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FP() + release.Version);
		try
		{
			Directory.CreateDirectory(staging);
		}
		catch (Exception ex2)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fp() + ex2.Message);
		}
		string text = Path.GetFileName(new Uri(url).LocalPath);
		if (string.IsNullOrEmpty(text))
		{
			text = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FQ();
		}
		string artifact = Path.Combine(staging, text);
		try
		{
			await File.WriteAllBytesAsync(artifact, array).ConfigureAwait(false);
		}
		catch (Exception ex3)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fq() + ex3.Message);
		}
		if (artifact.EndsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FR(), StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				ZipFile.ExtractToDirectory(artifact, staging, true);
			}
			catch (Exception ex4)
			{
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fr() + ex4.Message);
			}
			artifact = Directory.EnumerateFiles(staging, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FS(), SearchOption.AllDirectories).FirstOrDefault((string file) => file.EndsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dX(), StringComparison.OrdinalIgnoreCase) || file.EndsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FK(), StringComparison.OrdinalIgnoreCase)) ?? throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fs());
		}
		Launch(artifact);
	}

	private static void Launch(string artifact)
	{
		string text = Environment.ProcessPath ?? string.Empty;
		try
		{
			if (artifact.EndsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dX(), StringComparison.OrdinalIgnoreCase))
			{
				string text2 = (string.IsNullOrEmpty(text) ? string.Empty : (_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dx() + text + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD()));
				string arguments = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dY() + artifact + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dy() + text2;
				System.Diagnostics.Process.Start(new ProcessStartInfo
				{
					FileName = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dZ(),
					Arguments = arguments,
					UseShellExecute = true,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					Verb = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dz()
				});
			}
			else
			{
				System.Diagnostics.Process.Start(new ProcessStartInfo
				{
					FileName = artifact,
					UseShellExecute = true
				});
			}
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EA() + ex.Message);
		}
	}
}
