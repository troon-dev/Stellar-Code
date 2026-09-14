using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;

namespace Stellar.Launcher.Downloader;

public static class Verify
{
	private const int READ_SIZE = 1048576;

	public static List<string> CheckSizes(List<(string Path, long Size)> files)
	{
		List<string> list = new List<string>();
		foreach (var (text, num) in files)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(text);
				if (fileInfo.Exists && fileInfo.Length == num)
				{
					continue;
				}
			}
			catch
			{
			}
			list.Add(text);
		}
		return list;
	}

	public static bool VerifyChunk(Chunk chunk)
	{
		using SHA1 sHA = SHA1.Create();
		byte[] array = new byte[1048576];
		long num = 0L;
		foreach (Span span in chunk.Spans)
		{
			if (span.Start != num)
			{
				return false;
			}
			SafeFileHandle safeFileHandle;
			try
			{
				safeFileHandle = File.OpenHandle(span.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.None, 0L);
			}
			catch
			{
				return false;
			}
			using (safeFileHandle)
			{
				long num2 = span.End - span.Start;
				int num4;
				for (long num3 = 0L; num3 < num2; num3 += num4)
				{
					num4 = (int)Math.Min(num2 - num3, array.Length);
					int num5;
					try
					{
						num5 = Writer.ReadAt(safeFileHandle, array.AsSpan(0, num4), span.Offset + num3);
					}
					catch
					{
						return false;
					}
					if (num5 != num4)
					{
						return false;
					}
					sHA.TransformBlock(array, 0, num4, null, 0);
				}
			}
			num = span.End;
		}
		if (num != chunk.Size)
		{
			return false;
		}
		sHA.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		return Convert.ToHexString(sHA.Hash).ToLowerInvariant() == chunk.Hash;
	}
}
