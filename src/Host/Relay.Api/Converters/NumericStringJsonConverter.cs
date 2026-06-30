using System.Text.Json;
using System.Text.Json.Serialization;

namespace Relay.Api.Converters;

/// <summary>
/// Accepts either a JSON string ("5,11") or a JSON number (5) and deserializes to string?.
/// Needed for the QueueId field while clients transition from sending a single int to a
/// comma-separated string.
/// </summary>
internal sealed class NumericStringJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();

            case JsonTokenType.Number:
                return reader.TryGetInt64(out var n) ? n.ToString() : reader.GetDouble().ToString();

            case JsonTokenType.Null:
                return null;

            case JsonTokenType.StartArray:
                var parts = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    parts.Add(reader.TokenType switch
                    {
                        JsonTokenType.Number => reader.TryGetInt64(out var i) ? i.ToString() : reader.GetDouble().ToString(),
                        JsonTokenType.String => reader.GetString() ?? string.Empty,
                        _ => throw new JsonException($"Unexpected token {reader.TokenType} inside QueueId array.")
                    });
                }
                return string.Join(',', parts);

            default:
                throw new JsonException($"Cannot convert token {reader.TokenType} to string.");
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value);
    }
}
