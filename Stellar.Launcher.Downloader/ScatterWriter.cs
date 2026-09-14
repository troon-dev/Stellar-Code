using System;
using System.Collections.Generic;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Microsoft.Win32.SafeHandles;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Downloader;

public sealed class ScatterWriter : IDisposable
{
	private const int FLUSH_SIZE = 8388608;

	private readonly List<Span> spans;

	private readonly Dictionary<string, SafeFileHandle> handles = new Dictionary<string, SafeFileHandle>(StringComparer.OrdinalIgnoreCase);

	private byte[] staging = new byte[8388608];

	private int stagingLength;

	private long stagingAt;

	private int spanIndex;

	public ScatterWriter(List<Span> spans)
	{
		this.spans = spans;
	}

	public void Reset(long position)
	{
		stagingLength = 0;
		stagingAt = position;
		spanIndex = spans.FindIndex((Span span) => span.End > position);
		if (spanIndex < 0)
		{
			spanIndex = spans.Count;
		}
	}

	public void Write(ReadOnlySpan<byte> data)
	{
		if (stagingLength + data.Length > staging.Length)
		{
			Array.Resize(ref staging, Math.Max(staging.Length * 2, stagingLength + data.Length));
		}
		data.CopyTo(staging.AsSpan(stagingLength));
		stagingLength += data.Length;
		if (stagingLength >= 8388608)
		{
			Flush();
		}
	}

	public void Flush()
	{
		if (stagingLength == 0)
		{
			return;
		}
		int num = stagingLength;
		int i = spanIndex;
		int num2 = 0;
		long num3 = stagingAt;
		while (num2 < num)
		{
			long num4;
			for (num4 = num3 + num2; i < spans.Count && num4 >= spans[i].End; i++)
			{
			}
			if (i >= spans.Count)
			{
				break;
			}
			Span span = spans[i];
			if (num4 < span.Start)
			{
				num2 += (int)(span.Start - num4);
				continue;
			}
			int num5 = (int)Math.Min(span.End - num4, num - num2);
			if (!handles.TryGetValue(span.Path, out SafeFileHandle safeFileHandle))
			{
				safeFileHandle = Writer.OpenWrite(span.Path);
				handles[span.Path] = safeFileHandle;
			}
			long offset = span.Offset + (num4 - span.Start);
			try
			{
				Writer.WriteAt(safeFileHandle, staging.AsSpan(num2, num5), offset);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DE() + span.Path + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dc() + ex.Message);
			}
			num2 += num5;
		}
		spanIndex = i;
		stagingAt = num3 + num;
		stagingLength = 0;
	}

	public void Finish()
	{
		Flush();
		Dispose();
	}

	public void Dispose()
	{
		foreach (SafeFileHandle value in handles.Values)
		{
			value.Dispose();
		}
		handles.Clear();
	}
}
