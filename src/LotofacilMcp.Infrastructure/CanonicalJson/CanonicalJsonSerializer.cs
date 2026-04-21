using System.Text.Json;

namespace LotofacilMcp.Infrastructure.CanonicalJson;

public sealed class CanonicalJsonSerializer
{
    public string Serialize(object value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var element = JsonSerializer.SerializeToElement(value);
        return Serialize(element);
    }

    public string Serialize(JsonElement element)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        WriteCanonicalElement(writer, element);
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteCanonicalElement(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonicalElement(writer, property.Value);
                }

                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteCanonicalElement(writer, item);
                }

                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                WriteCanonicalNumber(writer, element);
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                writer.WriteBooleanValue(element.GetBoolean());
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;

            default:
                throw new InvalidOperationException($"Unsupported JSON value kind: {element.ValueKind}");
        }
    }

    private static void WriteCanonicalNumber(Utf8JsonWriter writer, JsonElement numberElement)
    {
        if (numberElement.TryGetInt64(out var int64Value))
        {
            writer.WriteNumberValue(int64Value);
            return;
        }

        if (numberElement.TryGetUInt64(out var uint64Value))
        {
            writer.WriteNumberValue(uint64Value);
            return;
        }

        if (numberElement.TryGetDecimal(out var decimalValue))
        {
            writer.WriteNumberValue(decimalValue);
            return;
        }

        if (numberElement.TryGetDouble(out var doubleValue))
        {
            writer.WriteNumberValue(doubleValue);
            return;
        }

        writer.WriteRawValue(numberElement.GetRawText());
    }
}
