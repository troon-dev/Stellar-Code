using System;
using System.Collections.Generic;
using System.Threading;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Native;

public sealed class Shortcuts
{
	private const uint WM_HOTKEY = 786u;

	private const uint WM_APP_REGISTER = 32768u;

	private const uint WM_APP_UNREGISTER = 32769u;

	private const uint MOD_ALT = 1u;

	private const uint MOD_CONTROL = 2u;

	private const uint MOD_SHIFT = 4u;

	private const uint MOD_WIN = 8u;

	private const uint MOD_NOREPEAT = 16384u;

	private readonly Bridge bridge;

	private readonly object gate = new object();

	private readonly Dictionary<string, int> registered = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<int, string> byId = new Dictionary<int, string>();

	private readonly Queue<(uint Message, string Shortcut)> queue = new Queue<(uint, string)>();

	private uint threadId;

	private int nextId = 1;

	public Shortcuts(Bridge bridge)
	{
		this.bridge = bridge;
		Thread thread = new Thread(Pump);
		thread.IsBackground = true;
		thread.Name = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bd();
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
	}

	private void Pump()
	{
		threadId = Win32.GetCurrentThreadId();
		Win32.MSG lpMsg;
		while (Win32.GetMessage(out lpMsg, IntPtr.Zero, 0u, 0u) > 0)
		{
			(uint, string) tuple;
			if (lpMsg.message == 786)
			{
				string text;
				lock (gate)
				{
					byId.TryGetValue(((IntPtr)lpMsg.wParam).ToInt32(), out text);
				}
				if (text != null)
				{
					bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BE() + text, new
					{
						shortcut = text,
						id = 0,
						state = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Be()
					});
				}
			}
			else
			{
				if (lpMsg.message != 32768 && lpMsg.message != 32769)
				{
					continue;
				}
				lock (gate)
				{
					if (queue.Count == 0)
					{
						continue;
					}
					tuple = queue.Dequeue();
					goto IL_00e6;
				}
			}
			continue;
			IL_00e6:
			if (tuple.Item1 == 32768)
			{
				Bind(tuple.Item2);
			}
			else
			{
				Unbind(tuple.Item2);
			}
		}
	}

	private static bool Parse(string shortcut, out uint modifiers, out uint key)
	{
		modifiers = 16384u;
		key = 0u;
		string[] array = shortcut.Split('+', StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			string text2 = text.ToLowerInvariant();
			if (text2 != null)
			{
				switch (text2.Length)
				{
				case 4:
				{
					char c = text2[0];
					if (c != 'c')
					{
						if (c != 'm' || !(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bf()))
						{
							break;
						}
						goto IL_01bb;
					}
					if (!(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BF()))
					{
						break;
					}
					goto IL_01a3;
				}
				case 7:
				{
					char c = text2[2];
					if (c != 'm')
					{
						if (c != 'n' || !(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BG()))
						{
							break;
						}
						goto IL_01a3;
					}
					if (!(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bg()))
					{
						break;
					}
					goto IL_01bb;
				}
				case 3:
				{
					char c = text2[0];
					if (c != 'a')
					{
						if (c != 'w' || !(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bi()))
						{
							break;
						}
						goto IL_01bb;
					}
					if (!(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BI()))
					{
						break;
					}
					goto IL_01ab;
				}
				case 5:
				{
					char c = text2[1];
					if (c != 'h')
					{
						if (c != 'u' || !(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BK()))
						{
							break;
						}
						goto IL_01bb;
					}
					if (!(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bj()))
					{
						break;
					}
					modifiers |= 4u;
					continue;
				}
				case 16:
					if (!(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BH()))
					{
						break;
					}
					goto IL_01a3;
				case 9:
					if (!(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bh()))
					{
						break;
					}
					goto IL_01a3;
				case 6:
					{
						if (!(text2 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BJ()))
						{
							break;
						}
						goto IL_01ab;
					}
					IL_01bb:
					modifiers |= 8u;
					continue;
					IL_01ab:
					modifiers |= 1u;
					continue;
					IL_01a3:
					modifiers |= 2u;
					continue;
				}
			}
			if (!Code(text, out key))
			{
				return false;
			}
		}
		return key != 0;
	}

	private static bool Code(string part, out uint key)
	{
		key = 0u;
		if (part.Length == 1)
		{
			char c = char.ToUpperInvariant(part[0]);
			if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
			{
				key = c;
				return true;
			}
		}
		if (part.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bk(), StringComparison.OrdinalIgnoreCase) && part.Length == 4)
		{
			key = char.ToUpperInvariant(part[3]);
			return true;
		}
		if (part.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BL(), StringComparison.OrdinalIgnoreCase) && part.Length == 6)
		{
			key = part[5];
			return true;
		}
		if (part.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bl(), StringComparison.OrdinalIgnoreCase) && part.Length >= 2 && int.TryParse(part.Substring(1), out var num) && num >= 1 && num <= 24)
		{
			key = (uint)(112 + num - 1);
			return true;
		}
		string text = part.ToLowerInvariant();
		uint num2;
		if (text != null)
		{
			switch (text.Length)
			{
			case 5:
				break;
			case 6:
				goto IL_0131;
			case 3:
				goto IL_0177;
			case 9:
				goto IL_01a0;
			case 4:
				goto IL_01c9;
			case 8:
				goto IL_01f2;
			case 7:
				goto IL_0212;
			case 10:
				goto IL_0232;
			case 2:
				goto IL_03f6;
			case 11:
				goto IL_045f;
			default:
				goto IL_04d7;
			}
			switch (text[0])
			{
			case 's':
				break;
			case 'e':
				goto IL_0267;
			case 'r':
				goto IL_027c;
			case 'p':
				goto IL_0291;
			default:
				goto IL_04d7;
			}
			if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BM())
			{
				num2 = 32u;
				goto IL_04d9;
			}
		}
		goto IL_04d7;
		IL_04a9:
		num2 = 40u;
		goto IL_04d9;
		IL_01c9:
		char c2 = text[0];
		if (c2 != 'd')
		{
			if (c2 != 'h')
			{
				if (c2 == 'l' && text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bu())
				{
					goto IL_04ae;
				}
			}
			else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bt())
			{
				num2 = 36u;
				goto IL_04d9;
			}
		}
		else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BU())
		{
			goto IL_04a9;
		}
		goto IL_04d7;
		IL_0212:
		c2 = text[0];
		if (c2 != 'a')
		{
			if (c2 == 'n' && text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BX())
			{
				num2 = 144u;
				goto IL_04d9;
			}
		}
		else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bw())
		{
			goto IL_04a4;
		}
		goto IL_04d7;
		IL_045f:
		if (!(text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.By()))
		{
			goto IL_04d7;
		}
		num2 = 44u;
		goto IL_04d9;
		IL_04b3:
		num2 = 39u;
		goto IL_04d9;
		IL_01f2:
		c2 = text[0];
		if (c2 != 'c')
		{
			if (c2 != 'p' || !(text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BV()))
			{
				goto IL_04d7;
			}
			num2 = 34u;
		}
		else
		{
			if (!(text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bv()))
			{
				goto IL_04d7;
			}
			num2 = 20u;
		}
		goto IL_04d9;
		IL_04d9:
		key = num2;
		return key != 0;
		IL_047d:
		num2 = 27u;
		goto IL_04d9;
		IL_04a4:
		num2 = 38u;
		goto IL_04d9;
		IL_0291:
		if (!(text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bn()))
		{
			goto IL_04d7;
		}
		num2 = 19u;
		goto IL_04d9;
		IL_04ae:
		num2 = 37u;
		goto IL_04d9;
		IL_027c:
		if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BN())
		{
			goto IL_04b3;
		}
		goto IL_04d7;
		IL_04d7:
		num2 = 0u;
		goto IL_04d9;
		IL_0267:
		if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bm())
		{
			goto IL_0473;
		}
		goto IL_04d7;
		IL_0131:
		c2 = text[0];
		if ((uint)c2 <= 101u)
		{
			if (c2 != 'd')
			{
				if (c2 == 'e' && text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bo())
				{
					goto IL_047d;
				}
			}
			else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BP())
			{
				num2 = 46u;
				goto IL_04d9;
			}
		}
		else if (c2 != 'i')
		{
			if (c2 != 'p')
			{
				if (c2 == 'r' && text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BO())
				{
					goto IL_0473;
				}
			}
			else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BQ())
			{
				num2 = 33u;
				goto IL_04d9;
			}
		}
		else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bp())
		{
			num2 = 45u;
			goto IL_04d9;
		}
		goto IL_04d7;
		IL_03f6:
		if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BW())
		{
			goto IL_04a4;
		}
		goto IL_04d7;
		IL_0177:
		c2 = text[1];
		if (c2 != 'a')
		{
			if (c2 != 'n')
			{
				if (c2 == 's' && text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BR())
				{
					goto IL_047d;
				}
			}
			else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Br())
			{
				num2 = 35u;
				goto IL_04d9;
			}
		}
		else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bq())
		{
			num2 = 9u;
			goto IL_04d9;
		}
		goto IL_04d7;
		IL_01a0:
		c2 = text[5];
		if (c2 != 'd')
		{
			if (c2 != 'l')
			{
				if (c2 == 'p' && text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BS())
				{
					num2 = 8u;
					goto IL_04d9;
				}
			}
			else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BT())
			{
				goto IL_04ae;
			}
		}
		else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bs())
		{
			goto IL_04a9;
		}
		goto IL_04d7;
		IL_0232:
		c2 = text[0];
		if (c2 != 'a')
		{
			if (c2 == 's' && text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BY())
			{
				num2 = 145u;
				goto IL_04d9;
			}
		}
		else if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bx())
		{
			goto IL_04b3;
		}
		goto IL_04d7;
		IL_0473:
		num2 = 13u;
		goto IL_04d9;
	}

	private void Bind(string shortcut)
	{
		if (!Parse(shortcut, out var modifiers, out var key))
		{
			return;
		}
		int num;
		lock (gate)
		{
			if (registered.ContainsKey(shortcut))
			{
				return;
			}
			num = nextId++;
		}
		if (!Win32.RegisterHotKey(IntPtr.Zero, num, modifiers, key))
		{
			return;
		}
		lock (gate)
		{
			registered[shortcut] = num;
			byId[num] = shortcut;
		}
	}

	private void Unbind(string shortcut)
	{
		int num;
		lock (gate)
		{
			if (!registered.TryGetValue(shortcut, out num))
			{
				return;
			}
			registered.Remove(shortcut);
			byId.Remove(num);
		}
		Win32.UnregisterHotKey(IntPtr.Zero, num);
	}

	private void Post(uint message, string shortcut)
	{
		lock (gate)
		{
			queue.Enqueue((message, shortcut));
		}
		uint num = threadId;
		if (num != 0)
		{
			Win32.PostThreadMessage(num, message, IntPtr.Zero, IntPtr.Zero);
		}
	}

	public void Register(string shortcut)
	{
		if (!Parse(shortcut, out var _, out var _))
		{
			throw new CommandFailure(shortcut + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BZ());
		}
		Post(32768u, shortcut);
	}

	public void Unregister(string shortcut)
	{
		Post(32769u, shortcut);
	}

	public bool IsRegistered(string shortcut)
	{
		lock (gate)
		{
			return registered.ContainsKey(shortcut);
		}
	}
}
