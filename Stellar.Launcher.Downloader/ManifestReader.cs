using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Downloader;

public static class ManifestReader
{
	private sealed class VersionEntry
	{
		[JsonPropertyName("manifest")]
		public string? Manifest { get; set; }

		[JsonPropertyName("chunks")]
		public string? Chunks { get; set; }
	}

	private sealed class RawInfo
	{
		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;


		[JsonPropertyName("offset")]
		public long Offset { get; set; }

		[JsonPropertyName("start")]
		public long Start { get; set; }

		[JsonPropertyName("end")]
		public long End { get; set; }
	}

	private sealed class RawManifest
	{
		[JsonPropertyName("empty")]
		public List<string> Empty { get; set; } = new List<string>();


		[JsonPropertyName("chunks")]
		public Dictionary<string, List<RawInfo>> Chunks { get; set; } = new Dictionary<string, List<RawInfo>>();

	}

	private static string? Between(string text, string open, string close)
	{
		int num = text.IndexOf(open, StringComparison.Ordinal);
		if (num < 0)
		{
			return null;
		}
		string text2 = text.Substring(num + open.Length);
		int num2 = text2.IndexOf(close, StringComparison.Ordinal);
		if (num2 >= 0)
		{
			return text2.Substring(0, num2);
		}
		return text2;
	}

	private static string GameOf(string name)
	{
		return Between(name, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cE(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ce()) ?? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CZ();
	}

	private static string ReleaseOf(string name)
	{
		return Between(name, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cF(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cf()) ?? string.Empty;
	}

	private static (uint Major, uint Minor) ReleaseValue(string release)
	{
		string[] array = release.Split('.');
		uint num;
		uint num2 = ((array.Length != 0 && uint.TryParse(array[0].Trim(), out num)) ? num : 0);
		uint num3;
		uint num4 = ((array.Length > 1 && uint.TryParse(array[1].Trim(), out num3)) ? num3 : 0u);
		return (Major: num2, Minor: num4);
	}

	public static List<Version> ParseVersions(string body)
	{
		Dictionary<string, VersionEntry> dictionary;
		try
		{
			dictionary = JsonSerializer.Deserialize<Dictionary<string, VersionEntry>>(body);
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cG() + ex.Message);
		}
		List<Version> list = new List<Version>();
		foreach (string item in (dictionary ?? new Dictionary<string, VersionEntry>()).Keys.OrderBy<string, string>((string key) => key, StringComparer.Ordinal))
		{
			VersionEntry versionEntry = dictionary[item];
			if (!string.IsNullOrEmpty(versionEntry.Manifest) && !string.IsNullOrEmpty(versionEntry.Chunks))
			{
				string text = GameOf(item);
				string text2 = ReleaseOf(item);
				list.Add(new Version
				{
					Label = (text + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cD() + text2).Trim(),
					Game = text,
					Release = text2,
					Name = item,
					Manifest = versionEntry.Manifest,
					Chunks = versionEntry.Chunks
				});
			}
		}
		list.Sort(delegate(Version a, Version b)
		{
			(uint, uint) tuple = ReleaseValue(b.Release);
			(uint, uint) tuple2 = ReleaseValue(a.Release);
			return (tuple.Item1 == tuple2.Item1) ? tuple.Item2.CompareTo(tuple2.Item2) : tuple.Item1.CompareTo(tuple2.Item1);
		});
		if (list.Count == 0)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cg());
		}
		return list;
	}

	private static string SafeJoin(string root, string raw)
	{
		string text = root;
		bool flag = false;
		string[] array = raw.Replace('\\', '/').Split('/');
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i].Trim();
			if (text2.Length != 0 && !(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bA()))
			{
				if (text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cH() || text2.Contains(':'))
				{
					throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ch() + raw);
				}
				text = Path.Combine(text, text2);
				flag = true;
			}
		}
		if (!flag || !text.StartsWith(root, StringComparison.OrdinalIgnoreCase))
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ch() + raw);
		}
		return text;
	}

	public static Plan BuildPlan(string body, string root)
	{
		RawManifest rawManifest;
		try
		{
			rawManifest = JsonSerializer.Deserialize<RawManifest>(body);
		}
		catch (Exception ex)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cI() + ex.Message);
		}
		if (rawManifest == null)
		{
			rawManifest = new RawManifest();
		}
		List<Chunk> list = new List<Chunk>(rawManifest.Chunks.Count);
		Dictionary<string, long> dictionary = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
		long num = 0L;
		foreach (string item in rawManifest.Chunks.Keys.OrderBy<string, string>((string key) => key, StringComparer.Ordinal))
		{
			List<RawInfo> list2 = rawManifest.Chunks[item];
			if (item.Length != 40 || !item.All(Uri.IsHexDigit))
			{
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ci() + item);
			}
			if (list2.Count == 0)
			{
				continue;
			}
			List<Span> list3 = new List<Span>(list2.Count);
			foreach (RawInfo item2 in list2)
			{
				if (item2.End < item2.Start)
				{
					throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cJ() + item + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cj());
				}
				string text = SafeJoin(root, item2.Name);
				long num2 = item2.End - item2.Start;
				long num3 = item2.Offset + num2;
				if (dictionary.TryGetValue(text, out var num4))
				{
					if (num3 > num4)
					{
						dictionary[text] = num3;
					}
				}
				else
				{
					dictionary[text] = num3;
				}
				num += num2;
				list3.Add(new Span
				{
					Path = text,
					Offset = item2.Offset,
					Start = item2.Start,
					End = item2.End
				});
			}
			list3.Sort((Span a, Span b) => a.Start.CompareTo(b.Start));
			long size = ((list3.Count == 0) ? 0 : list3.Max((Span span) => span.End));
			list.Add(new Chunk
			{
				Hash = item,
				Size = size,
				Spans = list3
			});
		}
		if (list.Count == 0)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cK());
		}
		List<string> list4 = new List<string>(rawManifest.Empty.Count);
		foreach (string item3 in rawManifest.Empty)
		{
			list4.Add(SafeJoin(root, item3));
		}
		List<(string, long)> files = dictionary.Select((KeyValuePair<string, long> pair) => (Path: pair.Key, Size: pair.Value)).OrderBy<(string, long), string>(((string Path, long Size) pair) => pair.Path, StringComparer.Ordinal).ToList();
		return new Plan
		{
			Chunks = list,
			Empties = list4,
			Files = files,
			TotalBytes = num
		};
	}
}
