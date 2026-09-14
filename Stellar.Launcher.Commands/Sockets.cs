using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Ipc;

namespace Stellar.Launcher.Commands;

public sealed class Sockets
{
	private sealed class Connection
	{
		public readonly SemaphoreSlim SendGate = new SemaphoreSlim(1, 1);

		public required ClientWebSocket Socket { get; init; }

		public required CancellationTokenSource Cancellation { get; init; }
	}

	private readonly Bridge bridge;

	private readonly ConcurrentDictionary<long, Connection> connections = new ConcurrentDictionary<long, Connection>();

	private long sequence;

	public Sockets(Bridge bridge)
	{
		this.bridge = bridge;
	}

	public async Task<long> Connect(string url, Dictionary<string, string> headers)
	{
		ClientWebSocket socket = new ClientWebSocket();
		foreach (var (text3, text4) in headers)
		{
			if (string.Equals(text3, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fe(), StringComparison.OrdinalIgnoreCase))
			{
				socket.Options.SetRequestHeader(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fe(), text4);
			}
			else if (!string.Equals(text3, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FF(), StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					socket.Options.SetRequestHeader(text3, text4);
				}
				catch
				{
				}
			}
		}
		try
		{
			await socket.ConnectAsync(new Uri(url), CancellationToken.None).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			socket.Dispose();
			throw new CommandFailure(ex.Message);
		}
		long id = Interlocked.Increment(ref sequence);
		Connection connection = new Connection
		{
			Socket = socket,
			Cancellation = new CancellationTokenSource()
		};
		connections[id] = connection;
		Task.Run(() => Receive(id, connection));
		return id;
	}

	private async Task Receive(long id, Connection connection)
	{
		byte[] buffer = new byte[65536];
		MemoryStream accumulated = new MemoryStream();
		try
		{
			while (connection.Socket.State == WebSocketState.Open && !connection.Cancellation.IsCancellationRequested)
			{
				WebSocketReceiveResult webSocketReceiveResult;
				try
				{
					webSocketReceiveResult = await connection.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), connection.Cancellation.Token).ConfigureAwait(false);
				}
				catch
				{
					break;
				}
				if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
				{
					break;
				}
				accumulated.Write(buffer, 0, webSocketReceiveResult.Count);
				if (webSocketReceiveResult.EndOfMessage)
				{
					byte[] array = accumulated.ToArray();
					accumulated.SetLength(0L);
					if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text)
					{
						Bridge obj2 = bridge;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
						defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ff());
						defaultInterpolatedStringHandler.AppendFormatted(id);
						obj2.Emit(defaultInterpolatedStringHandler.ToStringAndClear(), new
						{
							type = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FG(),
							data = Encoding.UTF8.GetString(array)
						});
					}
					else
					{
						Bridge obj3 = bridge;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 1);
						defaultInterpolatedStringHandler2.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ff());
						defaultInterpolatedStringHandler2.AppendFormatted(id);
						obj3.Emit(defaultInterpolatedStringHandler2.ToStringAndClear(), new
						{
							type = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fg(),
							data = Convert.ToBase64String(array)
						});
					}
				}
			}
		}
		finally
		{
			Bridge obj4 = bridge;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(13, 1);
			defaultInterpolatedStringHandler3.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ff());
			defaultInterpolatedStringHandler3.AppendFormatted(id);
			obj4.Emit(defaultInterpolatedStringHandler3.ToStringAndClear(), new
			{
				type = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FH(),
				data = new
				{
					code = (int)connection.Socket.CloseStatus.GetValueOrDefault(WebSocketCloseStatus.NormalClosure),
					reason = (connection.Socket.CloseStatusDescription ?? string.Empty)
				}
			});
			Teardown(id);
		}
	}

	public async Task Send(long id, string type, byte[] binary, string text)
	{
		if (!connections.TryGetValue(id, out Connection connection))
		{
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fh());
		}
		await connection.SendGate.WaitAsync().ConfigureAwait(false);
		try
		{
			if (type == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FG())
			{
				await connection.Socket.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
			}
			else
			{
				await connection.Socket.SendAsync(binary, WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			throw new CommandFailure(ex.Message);
		}
		finally
		{
			connection.SendGate.Release();
		}
	}

	public async Task Disconnect(long id)
	{
		if (!connections.TryGetValue(id, out Connection connection))
		{
			return;
		}
		try
		{
			if (connection.Socket.State == WebSocketState.Open)
			{
				await connection.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
			}
		}
		catch
		{
		}
		Teardown(id);
	}

	private void Teardown(long id)
	{
		if (connections.TryRemove(id, out Connection connection))
		{
			connection.Cancellation.Cancel();
			connection.Cancellation.Dispose();
			connection.Socket.Dispose();
		}
	}
}
