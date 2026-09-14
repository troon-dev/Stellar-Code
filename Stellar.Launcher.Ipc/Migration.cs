using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Ipc;

public static class Migration
{
	private const string ORIGIN = "http://tauri.localhost";

	private static Dictionary<string, string>? pending;

	public static void Run(string dataDir)
	{
		try
		{
			string profile = Path.Combine(dataDir, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ct());
			string text = Path.Combine(profile, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cu(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CV());
			if (!Directory.Exists(text) || !HasLegacy(text))
			{
				return;
			}
			byte[] prefix = Encoding.ASCII.GetBytes(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cv());
			Dictionary<string, (ulong Seq, bool Del, string Val)> best = new Dictionary<string, (ulong, bool, string)>();
			string[] files = Directory.GetFiles(text, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CW());
			foreach (string file2 in files)
			{
				Safe(delegate
				{
					ReadSst(File.ReadAllBytes(file2), prefix, best);
				});
			}
			files = Directory.GetFiles(text, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cw());
			foreach (string file in files)
			{
				Safe(delegate
				{
					ReadLog(File.ReadAllBytes(file), prefix, best);
				});
			}
			Dictionary<string, string> dictionary = best.Where<KeyValuePair<string, (ulong, bool, string)>>((KeyValuePair<string, (ulong Seq, bool Del, string Val)> p) => !p.Value.Del).ToDictionary<KeyValuePair<string, (ulong, bool, string)>, string, string>((KeyValuePair<string, (ulong Seq, bool Del, string Val)> p) => p.Key, (KeyValuePair<string, (ulong Seq, bool Del, string Val)> p) => p.Value.Val);
			if (dictionary.Count > 0)
			{
				pending = dictionary;
			}
			Safe(delegate
			{
				Directory.Delete(profile, true);
			});
		}
		catch
		{
		}
	}

	public static Dictionary<string, string>? Pull()
	{
		Dictionary<string, string>? result = pending;
		pending = null;
		return result;
	}

	private static void Safe(Action action)
	{
		try
		{
			action();
		}
		catch
		{
		}
	}

	private static bool HasLegacy(string leveldb)
	{
		byte[] array = "tauri.localhost"u8.ToArray();
		foreach (string item in Directory.EnumerateFiles(leveldb))
		{
			try
			{
				if (File.ReadAllBytes(item).AsSpan().IndexOf(array) >= 0)
				{
					return true;
				}
			}
			catch
			{
			}
		}
		return false;
	}

	private static (ulong Value, int Next) Varint(ReadOnlySpan<byte> data, int i)
	{
		ulong num = 0uL;
		int num2 = 0;
		while (true)
		{
			byte b = data[i++];
			num |= (ulong)((long)(b & 0x7F) << num2);
			if ((b & 0x80) == 0)
			{
				break;
			}
			num2 += 7;
		}
		return (Value: num, Next: i);
	}

	private static string Decode(ReadOnlySpan<byte> d)
	{
		if (d.Length == 0)
		{
			return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au();
		}
		return d[0] switch
		{
			1 => Encoding.Latin1.GetString(d.Slice(1)), 
			0 => Encoding.Unicode.GetString(d.Slice(1)), 
			_ => Encoding.Latin1.GetString(d), 
		};
	}

	private static byte[] Snappy(ReadOnlySpan<byte> data)
	{
		List<byte> list = new List<byte>(data.Length * 2);
		int num = Varint(data, 0).Next;
		while (num < data.Length)
		{
			int num2 = data[num++];
			if ((num2 & 3) == 0)
			{
				int num3 = num2 >> 2;
				if (num3 >= 60)
				{
					int num4 = num3 - 59;
					num3 = 0;
					for (int i = 0; i < num4; i++)
					{
						num3 |= data[num + i] << 8 * i;
					}
					num += num4;
				}
				num3++;
				for (int j = 0; j < num3; j++)
				{
					list.Add(data[num + j]);
				}
				num += num3;
				continue;
			}
			int num5;
			int num6;
			switch (num2 & 3)
			{
			case 1:
				num5 = 4 + ((num2 >> 2) & 7);
				num6 = (num2 >> 5 << 8) | data[num++];
				break;
			case 2:
				num5 = 1 + (num2 >> 2);
				num6 = data[num] | (data[num + 1] << 8);
				num += 2;
				break;
			default:
				num5 = 1 + (num2 >> 2);
				num6 = data[num] | (data[num + 1] << 8) | (data[num + 2] << 16) | (data[num + 3] << 24);
				num += 4;
				break;
			}
			int k = 0;
			int num7 = list.Count - num6;
			for (; k < num5; k++)
			{
				list.Add(list[num7 + k]);
			}
		}
		return list.ToArray();
	}

	private static byte[] Block(byte[] file, int offset, int size)
	{
		return file[offset + size] switch
		{
			0 => file.AsSpan(offset, size).ToArray(), 
			1 => Snappy(file.AsSpan(offset, size)), 
			_ => throw new InvalidDataException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CX()), 
		};
	}

	private static List<(byte[] Key, byte[] Value)> Entries(byte[] block)
	{
		int num = block.Length - 4 - 4 * BitConverter.ToInt32(block, block.Length - 4);
		List<(byte[], byte[])> list = new List<(byte[], byte[])>();
		byte[] array = Array.Empty<byte>();
		int num2 = 0;
		while (num2 < num)
		{
			(ulong Value, int Next) tuple = Varint(block, num2);
			ulong item = tuple.Value;
			int item2 = tuple.Next;
			(ulong Value, int Next) tuple2 = Varint(block, item2);
			ulong item3 = tuple2.Value;
			int item4 = tuple2.Next;
			(ulong Value, int Next) tuple3 = Varint(block, item4);
			ulong item5 = tuple3.Value;
			int item6 = tuple3.Next;
			byte[] array2 = new byte[(int)item + (int)item3];
			Array.Copy(array, 0, array2, 0, (int)item);
			Array.Copy(block, item6, array2, (int)item, (int)item3);
			num2 = item6 + (int)item3;
			list.Add((array2, block.AsSpan(num2, (int)item5).ToArray()));
			num2 += (int)item5;
			array = array2;
		}
		return list;
	}

	private static void ReadSst(byte[] file, byte[] prefix, Dictionary<string, (ulong, bool, string)> best)
	{
		Span<byte> span = file.AsSpan(file.Length - 48);
		(ulong Value, int Next) obj = Varint(i: Varint(i: Varint(span, 0).Next, data: span).Next, data: span);
		ulong item3 = obj.Value;
		ulong item5 = Varint(i: obj.Next, data: span).Value;
		foreach (var item10 in Entries(Block(file, (int)item3, (int)item5)))
		{
			byte[] item6 = item10.Value;
			(ulong Value, int Next) tuple = Varint(item6, 0);
			ulong item7 = tuple.Value;
			ulong item9 = Varint(i: tuple.Next, data: item6).Value;
			List<(byte[], byte[])> list;
			try
			{
				list = Entries(Block(file, (int)item7, (int)item9));
			}
			catch
			{
				continue;
			}
			foreach (var (array, value) in list)
			{
				if (array.Length >= 8)
				{
					ulong num = BitConverter.ToUInt64(array, array.Length - 8);
					Accept(array.AsSpan(0, array.Length - 8), num >> 8, (byte)(num & 0xFF), value, prefix, best);
				}
			}
		}
	}

	private static void ReadLog(byte[] file, byte[] prefix, Dictionary<string, (ulong, bool, string)> best)
	{
		List<byte> list = new List<byte>();
		int num = 0;
		while (num + 7 <= file.Length)
		{
			int num2 = num % 32768;
			if (32768 - num2 < 7)
			{
				num += 32768 - num2;
				continue;
			}
			int num3 = file[num + 4] | (file[num + 5] << 8);
			byte b = file[num + 6];
			int num4 = num + 7;
			if (num4 + num3 <= file.Length)
			{
				byte[] array = file.AsSpan(num4, num3).ToArray();
				num = num4 + num3;
				switch (b)
				{
				case 1:
					Batch(array, prefix, best);
					break;
				case 2:
					list.Clear();
					list.AddRange(array);
					break;
				case 3:
					list.AddRange(array);
					break;
				case 4:
					list.AddRange(array);
					Batch(list.ToArray(), prefix, best);
					list.Clear();
					break;
				}
				continue;
			}
			break;
		}
	}

	private static void Batch(byte[] batch, byte[] prefix, Dictionary<string, (ulong, bool, string)> best)
	{
		if (batch.Length < 12)
		{
			return;
		}
		ulong num = BitConverter.ToUInt64(batch, 0);
		int num2 = BitConverter.ToInt32(batch, 8);
		int i = 0;
		int num3 = 12;
		for (; i < num2; i++)
		{
			if (num3 >= batch.Length)
			{
				break;
			}
			byte b = batch[num3++];
			(ulong Value, int Next) tuple = Varint(batch, num3);
			ulong item = tuple.Value;
			int item2 = tuple.Next;
			byte[] array = batch.AsSpan(item2, (int)item).ToArray();
			num3 = item2 + (int)item;
			byte[] value = Array.Empty<byte>();
			if (b == 1)
			{
				(ulong Value, int Next) tuple2 = Varint(batch, num3);
				ulong item3 = tuple2.Value;
				int item4 = tuple2.Next;
				value = batch.AsSpan(item4, (int)item3).ToArray();
				num3 = item4 + (int)item3;
			}
			Accept(array, num + (ulong)i, (b != 0) ? ((byte)1) : ((byte)0), value, prefix, best);
		}
	}

	private static void Accept(ReadOnlySpan<byte> key, ulong sequence, byte type, byte[] value, byte[] prefix, Dictionary<string, (ulong Seq, bool Del, string Val)> best)
	{
		if (key.StartsWith(prefix))
		{
			string text = Decode(key.Slice(prefix.Length));
			if (!best.TryGetValue(text, out (ulong, bool, string) tuple) || tuple.Item1 < sequence)
			{
				best[text] = ((type == 0) ? (sequence, true, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()) : (sequence, false, Decode(value)));
			}
		}
	}
}
