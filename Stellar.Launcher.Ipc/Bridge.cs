using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Photino.NET;

namespace Stellar.Launcher.Ipc;

public sealed class Bridge
{
	public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.Never,
		Converters = { (JsonConverter)new ByteArrayConverter() }
	};

	private readonly ConcurrentDictionary<string, Func<Request, Task<object?>>> handlers = new ConcurrentDictionary<string, Func<Request, Task<object>>>();

	private readonly List<(string Label, PhotinoWindow Window)> windows = new List<(string, PhotinoWindow)>();

	private long eventSequence;

	public void Register(string name, Func<Request, Task<object?>> handler)
	{
		handlers[name] = handler;
	}

	public void Register(string name, Func<Request, object?> handler)
	{
		Func<Request, object?> handler2 = handler;
		handlers[name] = (Request request) => Task.FromResult(handler2(request));
	}

	public void Register(string name, Action<Request> handler)
	{
		Action<Request> handler2 = handler;
		handlers[name] = delegate(Request request)
		{
			handler2(request);
			return Task.FromResult<object>(null);
		};
	}

	public void Attach(string label, PhotinoWindow window)
	{
		string label2 = label;
		lock (windows)
		{
			windows.Add((label2, window));
		}
		window.RegisterWebMessageReceivedHandler(delegate(object? _, string message)
		{
			Receive(label2, message);
		});
	}

	public void Emit(string name, object? payload = null)
	{
		long id = Interlocked.Increment(ref eventSequence);
		string encoded = JsonSerializer.Serialize(new
		{
			t = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bF(),
			id = id,
			@event = name,
			payload = payload
		}, Options);
		Send(encoded);
	}

	public void EmitTo(string label, string name, object? payload = null)
	{
		long id = Interlocked.Increment(ref eventSequence);
		string encoded = JsonSerializer.Serialize(new
		{
			t = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bF(),
			id = id,
			@event = name,
			payload = payload
		}, Options);
		Send(encoded, label);
	}

	private void Send(string encoded, string? only = null)
	{
		List<(string, PhotinoWindow)> list;
		lock (windows)
		{
			list = windows.ToList();
		}
		foreach (var (text, photinoWindow) in list)
		{
			if (only == null || !(text != only))
			{
				try
				{
					photinoWindow.SendWebMessage(encoded);
				}
				catch
				{
				}
			}
		}
	}

	private void Receive(string label, string raw)
	{
		JsonDocument jsonDocument;
		try
		{
			jsonDocument = JsonDocument.Parse(raw);
		}
		catch
		{
			return;
		}
		JsonElement rootElement = jsonDocument.RootElement;
		if (!rootElement.TryGetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bf(), out var jsonElement) || jsonElement.ValueKind != JsonValueKind.String)
		{
			jsonDocument.Dispose();
			return;
		}
		string @string = jsonElement.GetString();
		if (!(@string == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bG()))
		{
			if (@string == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bg())
			{
				long @int = rootElement.GetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bh()).GetInt64();
				string command = rootElement.GetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bI()).GetString() ?? string.Empty;
				JsonElement jsonElement2;
				JsonElement args = (rootElement.TryGetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bi(), out jsonElement2) ? jsonElement2.Clone() : default(JsonElement));
				jsonDocument.Dispose();
				Dispatch(label, @int, command, args);
			}
			else
			{
				jsonDocument.Dispose();
			}
		}
		else
		{
			string name = rootElement.GetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bF()).GetString() ?? string.Empty;
			JsonElement jsonElement3;
			object payload = (rootElement.TryGetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bH(), out jsonElement3) ? JsonSerializer.Deserialize<object>(jsonElement3.GetRawText(), Options) : null);
			jsonDocument.Dispose();
			Emit(name, payload);
		}
	}

	private async Task Dispatch(string label, long id, string command, JsonElement args)
	{
		object value = null;
		string error = null;
		try
		{
			if (!handlers.TryGetValue(command, out Func<Request, Task<object>> func))
			{
				throw new CommandFailure(command + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ep());
			}
			value = await func(new Request
			{
				Label = label,
				Args = args
			}).ConfigureAwait(false);
		}
		catch (CommandFailure commandFailure)
		{
			error = commandFailure.Message;
		}
		catch (Exception ex)
		{
			error = ex.Message;
		}
		string encoded;
		try
		{
			encoded = JsonSerializer.Serialize(new
			{
				t = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EQ(),
				id = id,
				ok = (error == null),
				value = value,
				error = error
			}, Options);
		}
		catch (Exception ex2)
		{
			encoded = JsonSerializer.Serialize(new
			{
				t = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EQ(),
				id = id,
				ok = false,
				value = (object)null,
				error = ex2.Message
			}, Options);
		}
		Send(encoded, label);
	}
}
