using System.Text.Json;

namespace LotofacilMcp.Server.Tools;

internal static class ResponseTransforms
{
    private static readonly StringComparer Ord = StringComparer.Ordinal;

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

        var syntaxError = ValidateFieldsSyntax(toolName, requestedFields);
        if (syntaxError is not null)
        {
            return syntaxError;
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
                    if (string.Equals(property.Name, "explanation", StringComparison.Ordinal) ||
                        string.Equals(property.Name, "rationale", StringComparison.Ordinal) ||
                        string.Equals(property.Name, "definitions", StringComparison.Ordinal))
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

    public static JsonElement Project(JsonElement element, IReadOnlyCollection<string> keepPaths)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return element;
        }

        var tree = ProjectionTree.Build(keepPaths);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteProjected(writer, element, tree);
        }

        using var doc = JsonDocument.Parse(stream.ToArray());
        return doc.RootElement.Clone();
    }

    private static void WriteProjected(Utf8JsonWriter writer, JsonElement element, ProjectionTree tree)
    {
        // Só projetamos objetos; outros tipos são escritos como estão.
        if (element.ValueKind != JsonValueKind.Object)
        {
            element.WriteTo(writer);
            return;
        }

        writer.WriteStartObject();

        foreach (var prop in element.EnumerateObject().OrderBy(p => p.Name, Ord))
        {
            if (!tree.Children.TryGetValue(prop.Name, out var child))
            {
                continue;
            }

            writer.WritePropertyName(prop.Name);
            if (child.IncludeAll)
            {
                prop.Value.WriteTo(writer);
                continue;
            }

            WriteProjectedValue(writer, prop.Value, child);
        }

        writer.WriteEndObject();
    }

    private static void WriteProjectedValue(Utf8JsonWriter writer, JsonElement value, ProjectionTree tree)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                WriteProjected(writer, value, tree);
                return;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in value.EnumerateArray())
                {
                    // Regra: para arrays, aplica a mesma projeção aos elementos quando houver paths abaixo.
                    if (tree.Children.Count == 0)
                    {
                        // Nada mais abaixo para projetar, mas IncludeAll já teria capturado esse caso.
                        item.WriteTo(writer);
                        continue;
                    }
                    WriteProjectedValue(writer, item, tree);
                }
                writer.WriteEndArray();
                return;

            default:
                // Se o path pediu algo abaixo de um escalar, isso deveria ter sido bloqueado por allowlist.
                value.WriteTo(writer);
                return;
        }
    }

    private static ContractErrorEnvelope? ValidateFieldsSyntax(string toolName, IReadOnlyList<string> fields)
    {
        // Sintaxe normativa (mcp-tool-contract / ADR 0023):
        // - path por nomes de campos separados por "."
        // - sem índices numéricos (ex.: "metrics.0.metric_name" é proibido)
        // - sem curingas (*)
        // - sem segmentos vazios
        var invalidSyntax = fields
            .Where(f => !IsValidFieldSelector(f))
            .Distinct(Ord)
            .OrderBy(s => s, Ord)
            .ToArray();

        if (invalidSyntax.Length == 0)
        {
            return null;
        }

        return new ContractErrorEnvelope(new ContractError(
            Code: "INVALID_REQUEST",
            Message: $"Invalid fields syntax for tool '{toolName}'.",
            Details: new Dictionary<string, object?>
            {
                ["field"] = "fields",
                ["tool"] = toolName,
                ["invalid_fields"] = invalidSyntax,
                ["constraint"] = "fields must be dot-path selectors; no array indices; no wildcards"
            }));
    }

    private static bool IsValidFieldSelector(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
        {
            return false;
        }

        if (selector.Contains('*', StringComparison.Ordinal))
        {
            return false;
        }

        var parts = selector.Split('.', StringSplitOptions.None);
        if (parts.Length == 0)
        {
            return false;
        }

        foreach (var p in parts)
        {
            if (string.IsNullOrWhiteSpace(p))
            {
                return false;
            }

            // Proibir segmentos puramente numéricos (índices de array).
            if (p.All(ch => ch is >= '0' and <= '9'))
            {
                return false;
            }
        }

        return true;
    }

    private sealed class ProjectionTree
    {
        public bool IncludeAll { get; private set; }
        public Dictionary<string, ProjectionTree> Children { get; } = new(Ord);

        public static ProjectionTree Build(IReadOnlyCollection<string> paths)
        {
            var root = new ProjectionTree();
            foreach (var path in paths)
            {
                root.Add(path);
            }
            return root;
        }

        private void Add(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            Add(parts, 0);
        }

        private void Add(string[] parts, int idx)
        {
            if (idx >= parts.Length)
            {
                IncludeAll = true;
                Children.Clear();
                return;
            }

            if (IncludeAll)
            {
                return;
            }

            var key = parts[idx];
            if (!Children.TryGetValue(key, out var child))
            {
                child = new ProjectionTree();
                Children[key] = child;
            }

            child.Add(parts, idx + 1);
        }
    }
}

