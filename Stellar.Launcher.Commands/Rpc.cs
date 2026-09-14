using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Commands;

public sealed class Rpc
{
	private readonly object gate = new object();

	private NamedPipeClientStream? pipe;

	private string clientId = string.Empty;

	public async Task Start(string id)
	{
		NamedPipeClientStream namedPipeClientStream;
		lock (gate)
		{
			namedPipeClientStream = pipe;
			clientId = id;
		}
		if (namedPipeClientStream != null && namedPipeClientStream.IsConnected)
		{
			return;
		}
		for (int index = 0; index < 10; index++)
		{
			string text = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bA();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
			defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fc());
			defaultInterpolatedStringHandler.AppendFormatted(index);
			NamedPipeClientStream stream = new NamedPipeClientStream(text, defaultInterpolatedStringHandler.ToStringAndClear(), PipeDirection.InOut, PipeOptions.Asynchronous);
			try
			{
				await stream.ConnectAsync(500).ConfigureAwait(false);
			}
			catch
			{
				await stream.DisposeAsync().ConfigureAwait(false);
				continue;
			}
			try
			{
				await Frame(stream, 0, JsonSerializer.Serialize(new
				{
					v = 1,
					client_id = id
				})).ConfigureAwait(false);
			}
			catch
			{
				await stream.DisposeAsync().ConfigureAwait(false);
				continue;
			}
			lock (gate)
			{
				pipe?.Dispose();
				pipe = stream;
			}
			Task.Run(() => Drain(stream));
			return;
		}
		throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FD());
	}

	private static async Task Frame(Stream stream, int opcode, string payload)
	{
		byte[] body = Encoding.UTF8.GetBytes(payload);
		byte[] array = new byte[8];
		BinaryPrimitives.WriteInt32LittleEndian(array.AsSpan(0, 4), opcode);
		BinaryPrimitives.WriteInt32LittleEndian(array.AsSpan(4, 4), body.Length);
		await stream.WriteAsync(array).ConfigureAwait(false);
		await stream.WriteAsync(body).ConfigureAwait(false);
		await stream.FlushAsync().ConfigureAwait(false);
	}

	private static async Task Drain(NamedPipeClientStream stream)
	{
		byte[] header = new byte[8];
		try
		{
			while (stream.IsConnected && await stream.ReadAsync(header).ConfigureAwait(false) >= 8)
			{
				int length = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4, 4));
				if (length <= 0)
				{
					continue;
				}
				byte[] body = new byte[length];
				int num;
				for (int taken = 0; taken < length; taken += num)
				{
					num = await stream.ReadAsync(body.AsMemory(taken)).ConfigureAwait(false);
					if (num == 0)
					{
						return;
					}
				}
			}
		}
		catch
		{
		}
	}

	public async Task SetActivity(ActivityPayload activity)
	{
		NamedPipeClientStream namedPipeClientStream;
		lock (gate)
		{
			namedPipeClientStream = pipe;
		}
		if (namedPipeClientStream == null || !namedPipeClientStream.IsConnected)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (!string.IsNullOrEmpty(activity.State))
		{
			dictionary[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eX()] = activity.State;
		}
		if (!string.IsNullOrEmpty(activity.Details))
		{
			dictionary[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ex()] = activity.Details;
		}
		int? type = activity.Type;
		if (type.HasValue)
		{
			int valueOrDefault = type.GetValueOrDefault();
			dictionary[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eY()] = valueOrDefault;
		}
		ActivityTimestamps timestamps = activity.Timestamps;
		if (timestamps != null)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			long? start = timestamps.Start;
			if (start.HasValue)
			{
				long valueOrDefault2 = start.GetValueOrDefault();
				dictionary2[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ey()] = valueOrDefault2;
			}
			start = timestamps.End;
			if (start.HasValue)
			{
				long valueOrDefault3 = start.GetValueOrDefault();
				dictionary2[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Br()] = valueOrDefault3;
			}
			if (dictionary2.Count > 0)
			{
				dictionary[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eZ()] = dictionary2;
			}
		}
		ActivityAssets assets = activity.Assets;
		if (assets != null)
		{
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			if (!string.IsNullOrEmpty(assets.LargeImage))
			{
				dictionary3[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ez()] = assets.LargeImage;
			}
			if (!string.IsNullOrEmpty(assets.LargeText))
			{
				dictionary3[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FA()] = assets.LargeText;
			}
			if (!string.IsNullOrEmpty(assets.SmallImage))
			{
				dictionary3[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fa()] = assets.SmallImage;
			}
			if (!string.IsNullOrEmpty(assets.SmallText))
			{
				dictionary3[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FB()] = assets.SmallText;
			}
			if (dictionary3.Count > 0)
			{
				dictionary[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fb()] = dictionary3;
			}
		}
		if (activity.Buttons.Count > 0)
		{
			dictionary[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FC()] = activity.Buttons.Select((ActivityButton button) => new Dictionary<string, string>
			{
				[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh()] = button.Label,
				[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eW()] = button.Url
			}).ToList();
		}
		string payload = JsonSerializer.Serialize(new
		{
			cmd = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ew(),
			nonce = Guid.NewGuid().ToString(),
			args = new
			{
				pid = Environment.ProcessId,
				activity = dictionary
			}
		});
		try
		{
			await Frame(namedPipeClientStream, 1, payload).ConfigureAwait(false);
		}
		catch
		{
		}
	}

	public async Task ClearActivity()
	{
		NamedPipeClientStream namedPipeClientStream;
		lock (gate)
		{
			namedPipeClientStream = pipe;
		}
		if (namedPipeClientStream == null || !namedPipeClientStream.IsConnected)
		{
			return;
		}
		string payload = JsonSerializer.Serialize(new
		{
			cmd = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ew(),
			nonce = Guid.NewGuid().ToString(),
			args = new
			{
				pid = Environment.ProcessId,
				activity = (object)null
			}
		});
		try
		{
			await Frame(namedPipeClientStream, 1, payload).ConfigureAwait(false);
		}
		catch
		{
		}
	}

	public void Stop()
	{
		lock (gate)
		{
			pipe?.Dispose();
			pipe = null;
		}
	}
}
