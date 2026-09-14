using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Stellar.Launcher.Ipc;

public sealed class Request
{
	public required string Label { get; init; }

	public required JsonElement Args { get; init; }

	private bool Lookup(string name, out JsonElement value)
	{
		value = default(JsonElement);
		if (Args.ValueKind != JsonValueKind.Object)
		{
			return false;
		}
		if (Args.TryGetProperty(name, out value))
		{
			return value.ValueKind != JsonValueKind.Null;
		}
		return false;
	}

	public string String(string name, string fallback = "")
	{
		if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.String)
		{
			return fallback;
		}
		return value.GetString() ?? fallback;
	}

	public string? OptionalString(string name)
	{
		if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.String)
		{
			return null;
		}
		return value.GetString();
	}

	public bool Bool(string name, bool fallback = false)
	{
		if (!Lookup(name, out var value))
		{
			return fallback;
		}
		return value.ValueKind switch
		{
			JsonValueKind.True => true, 
			JsonValueKind.False => false, 
			_ => fallback, 
		};
	}

	public long Number(string name, long fallback = 0L)
	{
		if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Number)
		{
			return fallback;
		}
		return value.GetInt64();
	}

	public long? OptionalNumber(string name)
	{
		if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Number)
		{
			return null;
		}
		return value.GetInt64();
	}

	public double Double(string name, double fallback = 0.0)
	{
		if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Number)
		{
			return fallback;
		}
		return value.GetDouble();
	}

	public List<string> Strings(string name)
	{
		List<string> list = new List<string>();
		if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Array)
		{
			return list;
		}
		foreach (JsonElement item in value.EnumerateArray())
		{
			if (item.ValueKind == JsonValueKind.String)
			{
				list.Add(item.GetString() ?? string.Empty);
			}
		}
		return list;
	}

	public byte[] Bytes(string name)
	{
		if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Array)
		{
			return Array.Empty<byte>();
		}
		byte[] array = new byte[value.GetArrayLength()];
		int num = 0;
		foreach (JsonElement item in value.EnumerateArray())
		{
			array[num++] = (byte)item.GetInt32();
		}
		return array;
	}

	public T? Object<T>(string name)
	{
		if (!Lookup(name, out var value))
		{
			return default(T);
		}
		return value.Deserialize<T>(Bridge.Options);
	}

	public T? Payload<T>()
	{
		return Args.Deserialize<T>(Bridge.Options);
	}
}
