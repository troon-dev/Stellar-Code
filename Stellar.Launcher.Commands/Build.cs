using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Commands;

public static class Build
{
	public static void DeleteFiles(List<string> files)
	{
		foreach (string file in files)
		{
			try
			{
				File.Delete(file);
			}
			catch (FileNotFoundException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (Exception ex3)
			{
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.De() + file + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DF() + ex3.Message);
			}
		}
	}

	public static bool CheckFileExistsAndSize(string path, long? size)
	{
		if (!File.Exists(path))
		{
			return false;
		}
		if (!size.HasValue)
		{
			return true;
		}
		long length;
		try
		{
			length = new FileInfo(path).Length;
		}
		catch (Exception ex)
		{
			throw new CommandFailure(ex.Message);
		}
		return length == size.Value;
	}

	public static bool CheckFileExists(string path)
	{
		return File.Exists(path);
	}

	public static List<string> SearchForVersion(string path)
	{
		byte[] array;
		try
		{
			array = File.ReadAllBytes(path);
		}
		catch (Exception ex)
		{
			throw new CommandFailure(ex.Message);
		}
		byte[] array2 = new byte[22]
		{
			43, 0, 43, 0, 70, 0, 111, 0, 114, 0,
			116, 0, 110, 0, 105, 0, 116, 0, 101, 0,
			43, 0
		};
		List<string> list = new List<string>();
		for (int i = 0; i + array2.Length <= array.Length; i++)
		{
			if (Matches(array, i, array2))
			{
				int to = Math.Min(i + array2.Length + 64, array.Length);
				int? num = FindEnd(array, i + array2.Length, to);
				if (num.HasValue)
				{
					string @string = Encoding.Unicode.GetString(array, i, (array2.Length + num.Value) / 2 * 2);
					list.Add(@string.TrimEnd('\0'));
				}
			}
		}
		return list;
	}

	private static bool Matches(byte[] buffer, int offset, byte[] pattern)
	{
		for (int i = 0; i < pattern.Length; i++)
		{
			if (buffer[offset + i] != pattern[i])
			{
				return false;
			}
		}
		return true;
	}

	private static int? FindEnd(byte[] data, int from, int to)
	{
		for (int i = 0; from + i + 1 < to; i += 2)
		{
			if (data[from + i] == 0 && data[from + i + 1] == 0)
			{
				return i;
			}
		}
		return null;
	}
}
