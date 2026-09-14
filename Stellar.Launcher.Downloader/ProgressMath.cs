using System;

namespace Stellar.Launcher.Downloader;

public static class ProgressMath
{
	public static double Percent(long done, long total)
	{
		if (total == 0L)
		{
			return 0.0;
		}
		return Math.Clamp((double)done / (double)total * 100.0, 0.0, 100.0);
	}
}
