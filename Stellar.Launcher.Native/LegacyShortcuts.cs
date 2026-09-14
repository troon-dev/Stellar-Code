using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Native;

public static class LegacyShortcuts
{
	private const string OLD_EXE = "stellar.exe";

	public static void Clean()
	{
		foreach (string item in Candidates())
		{
			try
			{
				File.Delete(item);
			}
			catch
			{
			}
		}
	}

	public static List<string> Candidates()
	{
		return Candidates(Environment.ProcessPath);
	}

	public static List<string> Candidates(string? current)
	{
		List<string> list = new List<string>();
		try
		{
			if (string.IsNullOrEmpty(current))
			{
				return list;
			}
			foreach (string item in new string[4]
			{
				Environment.GetFolderPath(Environment.SpecialFolder.Programs),
				Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
				Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
			}.Distinct())
			{
				if (string.IsNullOrEmpty(item) || !Directory.Exists(item))
				{
					continue;
				}
				IEnumerable<string> enumerable;
				try
				{
					enumerable = Directory.EnumerateFiles(item, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aY(), SearchOption.AllDirectories);
				}
				catch
				{
					continue;
				}
				foreach (string item2 in enumerable)
				{
					string text = ResolveTarget(item2);
					if (text != null && Path.GetFileName(text).Equals(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ay(), StringComparison.OrdinalIgnoreCase) && !text.Equals(current, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(item2);
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	private static string? ResolveTarget(string link)
	{
		try
		{
			byte[] array = File.ReadAllBytes(link);
			if (array.Length < 76 || BitConverter.ToUInt32(array, 0) != 76)
			{
				return null;
			}
			uint num = BitConverter.ToUInt32(array, 20);
			bool unicode = (num & 0x80) != 0;
			int num2 = 76;
			if ((num & (true ? 1u : 0u)) != 0)
			{
				if (num2 + 2 > array.Length)
				{
					return null;
				}
				num2 += 2 + BitConverter.ToUInt16(array, num2);
			}
			if ((num & 2u) != 0 && num2 + 28 <= array.Length)
			{
				string text = FromLinkInfo(array, num2);
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
				num2 += (int)BitConverter.ToUInt32(array, num2);
			}
			if ((num & 4u) != 0)
			{
				num2 = SkipString(array, num2, unicode);
			}
			if ((num & 8u) != 0)
			{
				string text2 = ReadString(array, num2, unicode);
				if (!string.IsNullOrEmpty(text2))
				{
					string directoryName = Path.GetDirectoryName(link);
					return (directoryName == null) ? null : Path.GetFullPath(Path.Combine(directoryName, text2));
				}
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	private static string? FromLinkInfo(byte[] bytes, int position)
	{
		uint num = BitConverter.ToUInt32(bytes, position + 4);
		if ((BitConverter.ToUInt32(bytes, position + 8) & 1) == 0)
		{
			return null;
		}
		string text = null;
		if (num >= 36 && position + 36 <= bytes.Length)
		{
			uint num2 = BitConverter.ToUInt32(bytes, position + 28);
			if (num2 != 0)
			{
				text = ReadAnsiZ(bytes, position + (int)num2, unicode: true);
			}
		}
		if (text == null)
		{
			text = ReadAnsiZ(bytes, position + (int)BitConverter.ToUInt32(bytes, position + 16), unicode: false);
		}
		uint num3 = ((num >= 36 && position + 36 <= bytes.Length) ? BitConverter.ToUInt32(bytes, position + 32) : 0u);
		string text2 = ((num3 != 0) ? ReadAnsiZ(bytes, position + (int)num3, unicode: true) : ReadAnsiZ(bytes, position + (int)BitConverter.ToUInt32(bytes, position + 24), unicode: false));
		string text3 = text + text2;
		if (!string.IsNullOrEmpty(text3))
		{
			return text3;
		}
		return null;
	}

	private static int SkipString(byte[] bytes, int position, bool unicode)
	{
		ushort num = BitConverter.ToUInt16(bytes, position);
		return position + 2 + (unicode ? (num * 2) : num);
	}

	private static string ReadString(byte[] bytes, int position, bool unicode)
	{
		ushort num = BitConverter.ToUInt16(bytes, position);
		position += 2;
		if (!unicode)
		{
			return Encoding.Latin1.GetString(bytes, position, num);
		}
		return Encoding.Unicode.GetString(bytes, position, num * 2);
	}

	private static string ReadAnsiZ(byte[] data, int offset, bool unicode)
	{
		int i = offset;
		if (unicode)
		{
			for (; i + 1 < data.Length && (data[i] != 0 || data[i + 1] != 0); i += 2)
			{
			}
			return Encoding.Unicode.GetString(data, offset, i - offset);
		}
		for (; i < data.Length && data[i] != 0; i++)
		{
		}
		return Encoding.Latin1.GetString(data, offset, i - offset);
	}
}
