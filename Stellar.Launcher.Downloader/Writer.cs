using System;
using System.IO;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Microsoft.Win32.SafeHandles;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Downloader;

public static class Writer
{
	private const int FLUSH_SIZE = 8388608;

	public static void WriteAt(SafeFileHandle file, ReadOnlySpan<byte> buffer, long offset)
	{
		RandomAccess.Write(file, buffer, offset);
	}

	public static int ReadAt(SafeFileHandle file, Span<byte> buffer, long offset)
	{
		int i;
		int num;
		for (i = 0; i < buffer.Length; i += num)
		{
			num = RandomAccess.Read(file, buffer.Slice(i), offset + i);
			if (num == 0)
			{
				break;
			}
		}
		return i;
	}

	public static SafeFileHandle OpenWrite(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directoryName))
		{
			try
			{
				Directory.CreateDirectory(directoryName);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DC() + directoryName + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dc() + ex.Message);
			}
		}
		try
		{
			return File.OpenHandle(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.None, 0L);
		}
		catch (Exception ex2)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DD() + path + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dc() + ex2.Message);
		}
	}

	public static void Prepare(Plan plan)
	{
		foreach (string empty in plan.Empties)
		{
			string directoryName = Path.GetDirectoryName(empty);
			if (!string.IsNullOrEmpty(directoryName))
			{
				try
				{
					Directory.CreateDirectory(directoryName);
				}
				catch (Exception ex)
				{
					throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DC() + directoryName + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dc() + ex.Message);
				}
			}
			if (!File.Exists(empty))
			{
				try
				{
					File.Create(empty).Dispose();
				}
				catch (Exception ex2)
				{
					throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DC() + empty + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dc() + ex2.Message);
				}
			}
		}
		foreach (var (text, num) in plan.Files)
		{
			using SafeFileHandle safeFileHandle = OpenWrite(text);
			long length;
			try
			{
				length = RandomAccess.GetLength(safeFileHandle);
			}
			catch (Exception ex3)
			{
				throw new CommandFailure(ex3.Message);
			}
			if (length != num)
			{
				try
				{
					RandomAccess.SetLength(safeFileHandle, num);
				}
				catch (Exception ex4)
				{
					throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dd() + text + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dc() + ex4.Message);
				}
			}
		}
	}
}
