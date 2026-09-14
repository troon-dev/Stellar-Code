using System;
using System.Runtime.CompilerServices;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace Stellar.Launcher.Native;

public static class Minisign
{
	private sealed record ParsedKey(byte[] KeyId, byte[] PublicKey)
	{
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EN());
			stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.En());
			if (PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EO());
			builder.Append(KeyId);
			builder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eo());
			builder.Append(PublicKey);
			return true;
		}
	}

	private const string PUBLIC_KEY = "dW50cnVzdGVkIGNvbW1lbnQ6IG1pbmlzaWduIHB1YmxpYyBrZXk6IDUyMTRGQzI0NTJFNEJCODYKUldTR3UrUlNKUHdVVXJvckw1VWs2bWFLK0k0ZWJSdXZUcWZYck5NdXg1VXdaR0ZRMXFqek4vdEwK";

	private static string[] SplitLines(string text)
	{
		return text.Replace(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aZ(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.az()).Split('\n');
	}

	private static ParsedKey ParsePublicKey(string encoded)
	{
		string[] array = SplitLines(Encoding.UTF8.GetString(Convert.FromBase64String(encoded)));
		if (array.Length < 2)
		{
			throw new FormatException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BA());
		}
		byte[] array2 = Convert.FromBase64String(array[1].Trim());
		if (array2.Length != 42)
		{
			throw new FormatException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ba());
		}
		return new ParsedKey(array2[2..10], array2[10..42]);
	}

	public static bool Verify(byte[] artifact, string signatureField)
	{
		return Verify(artifact, signatureField, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BB());
	}

	internal static bool Verify(byte[] artifact, string signatureField, string publicKey)
	{
		try
		{
			ParsedKey parsedKey = ParsePublicKey(publicKey);
			string[] array = SplitLines(Encoding.UTF8.GetString(Convert.FromBase64String(signatureField.Trim())));
			if (array.Length < 4)
			{
				return false;
			}
			byte[] array2 = Convert.FromBase64String(array[1].Trim());
			if (array2.Length != 74)
			{
				return false;
			}
			string @string = Encoding.ASCII.GetString(array2, 0, 2);
			byte[] subArray = array2[2..10];
			byte[] subArray2 = array2[10..74];
			if (!subArray.AsSpan().SequenceEqual(parsedKey.KeyId))
			{
				return false;
			}
			byte[] array3;
			if (@string == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bb())
			{
				Blake2bDigest blake2bDigest = new Blake2bDigest(512);
				blake2bDigest.BlockUpdate(artifact, 0, artifact.Length);
				array3 = new byte[64];
				blake2bDigest.DoFinal(array3, 0);
			}
			else
			{
				if (!(@string == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BC()))
				{
					return false;
				}
				array3 = artifact;
			}
			Ed25519PublicKeyParameters ed25519PublicKeyParameters = new Ed25519PublicKeyParameters(parsedKey.PublicKey, 0);
			Ed25519Signer ed25519Signer = new Ed25519Signer();
			ed25519Signer.Init(false, ed25519PublicKeyParameters);
			ed25519Signer.BlockUpdate(array3, 0, array3.Length);
			if (!ed25519Signer.VerifySignature(subArray2))
			{
				return false;
			}
			if (!array[2].StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bc(), StringComparison.Ordinal))
			{
				return false;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(array[2].Substring(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Bc().Length));
			byte[] array4 = Convert.FromBase64String(array[3].Trim());
			byte[] array5 = new byte[subArray2.Length + bytes.Length];
			Buffer.BlockCopy(subArray2, 0, array5, 0, subArray2.Length);
			Buffer.BlockCopy(bytes, 0, array5, subArray2.Length, bytes.Length);
			Ed25519Signer ed25519Signer2 = new Ed25519Signer();
			ed25519Signer2.Init(false, ed25519PublicKeyParameters);
			ed25519Signer2.BlockUpdate(array5, 0, array5.Length);
			return ed25519Signer2.VerifySignature(array4);
		}
		catch
		{
			return false;
		}
	}
}
