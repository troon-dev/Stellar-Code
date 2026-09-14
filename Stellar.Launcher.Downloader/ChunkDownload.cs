using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Downloader;

public static class ChunkDownload
{
	private const int MAX_ATTEMPTS = 10;

	private const int MAX_RESTARTS = 3;

	public const string CANCELLED = "cancelled";

	private static void Rollback(Context context, long amount)
	{
		context.DoneBytes.Subtract(amount);
	}

	public static async Task Download(Context context, Chunk chunk)
	{
		string url = context.ChunksUrl + chunk.Hash + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ET();
		string text = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Et();
		for (int restart = 0; restart < 3; restart++)
		{
			if (context.Control.Cancelled)
			{
				throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU());
			}
			if (restart > 0)
			{
				await Task.Delay(600).ConfigureAwait(false);
			}
			try
			{
				await Run(context, chunk, url).ConfigureAwait(false);
				return;
			}
			catch (IOException ex) when (ex.Message == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU())
			{
				throw;
			}
			catch (Exception ex2)
			{
				text = ex2.Message;
			}
		}
		throw new IOException(text);
	}

	private static async Task Run(Context context, Chunk chunk, string url)
	{
		using IncrementalHash hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
		using ScatterWriter writer = new ScatterWriter(chunk.Spans);
		writer.Reset(0L);
		long position = 0L;
		long contributed = 0L;
		int attempts = 0;
		bool complete = false;
		string last = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eu();
		byte[] buffer = new byte[262144];
		while (attempts < 10)
		{
			if (context.Control.Cancelled)
			{
				Rollback(context, contributed);
				throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU());
			}
			await context.Control.WaitWhilePaused().ConfigureAwait(false);
			if (context.Control.Cancelled)
			{
				Rollback(context, contributed);
				throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU());
			}
			if (attempts > 0)
			{
				await Task.Delay((int)Math.Min(300L << Math.Min(attempts, 5), 8000L)).ConfigureAwait(false);
			}
			RangeStream rangeStream;
			try
			{
				rangeStream = await Net.OpenRange(context.Client, url, position, CancellationToken.None).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				last = ex.Message;
				attempts++;
				continue;
			}
			if (!rangeStream.Resumed)
			{
				Rollback(context, contributed);
				contributed = 0L;
				position = 0L;
				hasher.GetHashAndReset();
				writer.Reset(0L);
			}
			bool interrupted = false;
			bool paused = false;
			using (rangeStream.Response)
			{
				Stream body;
				try
				{
					body = await rangeStream.Response.Content.ReadAsStreamAsync().ConfigureAwait(false);
				}
				catch (Exception ex2)
				{
					last = ex2.Message;
					attempts++;
					goto end_IL_031e;
				}
				Stream stream = body;
				try
				{
					do
					{
						if (context.Control.Cancelled)
						{
							Rollback(context, contributed);
							throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU());
						}
						if (context.Control.IsPaused)
						{
							paused = true;
							break;
						}
						int num;
						try
						{
							using CancellationTokenSource timeout = new CancellationTokenSource(Net.STALL_TIMEOUT);
							num = await body.ReadAsync(buffer, timeout.Token).ConfigureAwait(false);
						}
						catch (OperationCanceledException)
						{
							last = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EV();
							interrupted = true;
							break;
						}
						catch (Exception ex4)
						{
							last = ex4.Message;
							interrupted = true;
							break;
						}
						if (num != 0)
						{
							long num2 = Math.Max(chunk.Size - position, 0L);
							if (num2 != 0L)
							{
								int num3 = (int)Math.Min(num, num2);
								hasher.AppendData(buffer.AsSpan(0, num3));
								writer.Write(buffer.AsSpan(0, num3));
								position += num3;
								contributed += num3;
								context.DoneBytes.Add(num3);
								continue;
							}
							break;
						}
						break;
					}
					while (position < chunk.Size);
				}
				finally
				{
					if (stream != null)
					{
						await stream.DisposeAsync();
					}
				}
				goto IL_06b4;
				end_IL_031e:;
			}
			continue;
			IL_06b4:
			if (position >= chunk.Size)
			{
				complete = true;
				break;
			}
			if (!paused)
			{
				if (!interrupted)
				{
					last = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ev();
				}
				attempts++;
			}
		}
		writer.Finish();
		if (!complete)
		{
			Rollback(context, contributed);
			throw new IOException(last);
		}
		if (Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant() != chunk.Hash)
		{
			Rollback(context, contributed);
			throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EW());
		}
	}
}
