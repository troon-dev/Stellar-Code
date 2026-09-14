using System;
using System.Threading;

namespace Stellar.Launcher.Downloader;

public sealed class Counter
{
	private long value;

	public long Value => Interlocked.Read(ref value);

	public Counter(long initial)
	{
		value = initial;
	}

	public void Add(long amount)
	{
		Interlocked.Add(ref value, amount);
	}

	public void Subtract(long amount)
	{
		if (amount <= 0)
		{
			return;
		}
		long num = Interlocked.Read(ref value);
		while (true)
		{
			long num2 = Math.Max(num - amount, 0L);
			long num3 = Interlocked.CompareExchange(ref value, num2, num);
			if (num3 == num)
			{
				break;
			}
			num = num3;
		}
	}
}
