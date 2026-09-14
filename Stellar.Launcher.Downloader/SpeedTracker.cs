using System;
using System.Diagnostics;

namespace Stellar.Launcher.Downloader;

public sealed class SpeedTracker
{
	private long lastBytes;

	private long lastAt;

	private double speed;

	public SpeedTracker(long bytes)
	{
		lastBytes = bytes;
		lastAt = Stopwatch.GetTimestamp();
		speed = 0.0;
	}

	public double Sample(long bytes)
	{
		long timestamp = Stopwatch.GetTimestamp();
		double num = (double)(timestamp - lastAt) / (double)Stopwatch.Frequency;
		if (num < 0.05)
		{
			return speed;
		}
		double num2 = (double)Math.Max(bytes - lastBytes, 0L) / num;
		speed = ((speed <= 0.0) ? num2 : (speed * 0.7 + num2 * 0.3));
		lastBytes = bytes;
		lastAt = timestamp;
		return speed;
	}

	public long Eta(long remaining)
	{
		if (speed < 1024.0)
		{
			return 0L;
		}
		return (long)Math.Max(Math.Round((double)remaining / speed), 0.0);
	}
}
