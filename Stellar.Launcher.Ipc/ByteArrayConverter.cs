using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Ipc;

public sealed class ByteArrayConverter : JsonConverter<byte[]>
{
	public override byte[] Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			return reader.GetBytesFromBase64();
		}
		if (reader.TokenType != JsonTokenType.StartArray)
		{
			throw new JsonException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bJ());
		}
		List<byte> list = new List<byte>();
		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			list.Add(reader.GetByte());
		}
		return list.ToArray();
	}

	public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (byte b in value)
		{
			writer.WriteNumberValue(b);
		}
		writer.WriteEndArray();
	}
}
