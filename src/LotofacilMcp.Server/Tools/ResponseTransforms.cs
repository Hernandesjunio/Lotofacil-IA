using System.Text.Json;

namespace LotofacilMcp.Server.Tools;

internal static class ResponseTransforms
{
    public static string[]? NormalizeFields(IReadOnlyList<string>? fields)
    {
        if (fields is null)
        {
            return null;
        }

        var normalized = fields
            .Select(s => (s ?? string.Empty).Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();

        return normalized.Length == 0 ? Array.Empty<string>() : normalized;
    }

    public static ContractErrorEnvelope? ValidateFields(string toolName, string[]? requestedFields, IReadOnlySet<string> allowedFields)
    {
        if (requestedFields is null)
        {
            return null;
        }

        var invalid = requestedFields
            .Where(f => !allowedFields.Contains(f))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();

        if (invalid.Length == 0)
        {
            return null;
        }

        var allowed = allowedFields.OrderBy(s => s, StringComparer.Ordinal).ToArray();

        return new ContractErrorEnvelope(new ContractError(
            Code: "INVALID_REQUEST",
            Message: $"Invalid fields for tool '{toolName}'.",
            Details: new Dictionary<string, object?>
            {
                ["field"] = "fields",
                ["tool"] = toolName,
                ["invalid_fields"] = invalid,
                ["allowed_fields"] = allowed
            }));
    }

    public static JsonElement StripExplanations(JsonElement element)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteWithoutExplanations(writer, element);
        }

        using var doc = JsonDocument.Parse(stream.ToArray());
        return doc.RootElement.Clone();
    }

    private static void WriteWithoutExplanations(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    // ADR 0023 / mcp-tool-contract: include_explanations=false omite campos explicativos.
                    if (string.Equals(property.Name, "explanation", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    writer.WritePropertyName(property.Name);
                    WriteWithoutExplanations(writer, property.Value);
                }
                writer.WriteEndObject();
                return;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteWithoutExplanations(writer, item);
                }
                writer.WriteEndArray();
                return;

            default:
                element.WriteTo(writer);
                return;
        }
    }

    public static JsonElement ProjectTopLevel(JsonElement element, IReadOnlyCollection<string> keepFields)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return element;
        }

        var keep = new HashSet<string>(keepFields, StringComparer.Ordinal);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();

            // Determinismo: ordenar as chaves mantidas.
            foreach (var property in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                if (!keep.Contains(property.Name))
                {
                    continue;
                }

                writer.WritePropertyName(property.Name);
                property.Value.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        using var doc = JsonDocument.Parse(stream.ToArray());
        return doc.RootElement.Clone();
    }
}

