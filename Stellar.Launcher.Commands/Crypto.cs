using System;
using System.Runtime.InteropServices;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Commands;

public static class Crypto
{
	[DllImport("stellar_native", CallingConvention = CallingConvention.Cdecl)]
	private static extern nint StellarSocketEncrypt(byte[] plaintext, nuint plaintextLen, byte[] sessionKey, nuint sessionKeyLen, out nuint outLen);

	[DllImport("stellar_native", CallingConvention = CallingConvention.Cdecl)]
	private static extern nint StellarSocketDecrypt(byte[] packet, nuint packetLen, byte[] sessionKey, nuint sessionKeyLen, out nuint outLen);

	[DllImport("stellar_native", CallingConvention = CallingConvention.Cdecl)]
	private static extern void StellarFree(nint ptr, nuint len);

	public static byte[] SocketEncrypt(string plaintext, byte[] sessionKey)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
		nuint outLen;
		nint num = StellarSocketEncrypt(bytes, (nuint)bytes.Length, sessionKey, (nuint)sessionKey.Length, out outLen);
		if (num == IntPtr.Zero)
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Df());
		}
		try
		{
			byte[] array = new byte[outLen];
			Marshal.Copy(num, array, 0, (int)outLen);
			return array;
		}
		finally
		{
			StellarFree(num, outLen);
		}
	}

	public static string? SocketDecrypt(byte[] packet, byte[] sessionKey)
	{
		nuint outLen;
		nint num = StellarSocketDecrypt(packet, (nuint)packet.Length, sessionKey, (nuint)sessionKey.Length, out outLen);
		if (num == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			byte[] array = new byte[outLen];
			Marshal.Copy(num, array, 0, (int)outLen);
			return Encoding.UTF8.GetString(array);
		}
		finally
		{
			StellarFree(num, outLen);
		}
	}
}
