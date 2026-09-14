using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stellar.Launcher.Downloader;

public sealed class Control
{
	private int cancel;

	private int paused;

	private readonly SemaphoreSlim notify = new SemaphoreSlim(0);

	public bool Cancelled => Volatile.Read(ref cancel) != 0;

	public bool IsPaused => Volatile.Read(ref paused) != 0;

	public void Pause()
	{
		Volatile.Write(ref paused, 1);
	}

	public void Resume()
	{
		Volatile.Write(ref paused, 0);
		Wake();
	}

	public void Stop()
	{
		Volatile.Write(ref cancel, 1);
		Volatile.Write(ref paused, 0);
		Wake();
	}

	private void Wake()
	{
		try
		{
			notify.Release();
		}
		catch (SemaphoreFullException)
		{
		}
	}

	public async Task WaitWhilePaused()
	{
		while (IsPaused && !Cancelled)
		{
			await notify.WaitAsync(TimeSpan.FromMilliseconds(200.0)).ConfigureAwait(false);
		}
	}
}
