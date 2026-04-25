using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpMetricAudit;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        const int windowSize = 20;
        const int endContestId = 3666;

        var repoRoot = FindRepoRoot();
        var serverProjectPath = Path.Combine(repoRoot, "src", "LotofacilMcp.Server", "LotofacilMcp.Server.csproj");

        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "LotofacilMcp.Server",
            Command = "dotnet",
            Arguments =
            [
                "run",
                "-c",
                "Debug",
                "--no-build",
                "--project",
                serverProjectPath,
                "--",
                "--mcp-stdio"
            ]
        }));

        var tools = await client.ListToolsAsync();
        var toolNames = tools.Select(t => t.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();

        var discover = await client.CallToolAsync("discover_capabilities", new Dictionary<string, object?>());
        if (discover.IsError == true)
        {
            Console.Error.WriteLine("discover_capabilities returned error.");
            Console.Error.WriteLine(ReadStructuredJson(discover).ToString());
            return 2;
        }

        var discoverJson = ReadStructuredJson(discover);
        var allowed = discoverJson
            .GetProperty("metrics")
            .GetProperty("compute_window_metrics_allowed")
            .EnumerateArray()
            .Select(e => e.GetString()!)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();

        var computeRequest = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["metrics"] = allowed.Select(m => new Dictionary<string, object?> { ["name"] = m }).ToArray()
        };

        var compute = await client.CallToolAsync("compute_window_metrics", computeRequest);
        if (compute.IsError == true)
        {
            Console.Error.WriteLine("compute_window_metrics returned error.");
            Console.Error.WriteLine(ReadStructuredJson(compute).ToString());
            return 3;
        }

        var computeJson = ReadStructuredJson(compute);
        var metrics = computeJson.GetProperty("metrics").EnumerateArray().ToArray();

        Console.WriteLine("## Tools (MCP STDIO)\n");
        foreach (var name in toolNames)
        {
            Console.WriteLine($"- `{name}`");
        }

        Console.WriteLine("\n## Métricas expostas (via `compute_window_metrics`) e verificação (janela=20)\n");
        Console.WriteLine("| Métrica | Descrição (campo `explanation`) | Validação (janela=20) | Evidência |");
        Console.WriteLine("|---|---|---|---|");

        var divergences = new List<Divergence>();

        foreach (var metricName in allowed)
        {
            var metric = metrics.FirstOrDefault(m =>
                string.Equals(m.GetProperty("metric_name").GetString(), metricName, StringComparison.Ordinal));

            if (metric.ValueKind == JsonValueKind.Undefined)
            {
                divergences.Add(new Divergence(metricName, "Tool expõe a métrica, mas não veio no payload.", "Sem resultado no array `metrics[]`."));
                Console.WriteLine($"| `{metricName}` | *(ausente na resposta)* | **DIVERGENTE** | não retornou |");
                continue;
            }

            var scope = metric.GetProperty("scope").GetString() ?? "";
            var shape = metric.GetProperty("shape").GetString() ?? "";
            var explanation = (metric.TryGetProperty("explanation", out var ex) ? ex.GetString() : null) ?? "";
            var value = metric.GetProperty("value").EnumerateArray().Select(v => v.GetDouble()).ToArray();

            var check = ValidateMetric(metricName, scope, shape, value, windowSize);
            var status = check.IsOk ? "OK" : "**DIVERGENTE**";
            var evidence = EscapePipe(check.Evidence);

            Console.WriteLine($"| `{metricName}` | {EscapePipe(explanation)} | {status} | {evidence} |");

            if (!check.IsOk)
            {
                divergences.Add(new Divergence(metricName, EscapePipe(explanation), check.ActualBehavior));
            }
        }

        Console.WriteLine("\n## Divergências encontradas\n");
        if (divergences.Count == 0)
        {
            Console.WriteLine("Nenhuma divergência detectada por regras determinísticas simples (forma, tamanho e invariantes).");
        }
        else
        {
            Console.WriteLine("| Métrica | Documentação (explanation) | Comportamento observado (motivo) |");
            Console.WriteLine("|---|---|---|");
            foreach (var d in divergences)
            {
                Console.WriteLine($"| `{d.MetricName}` | {d.Documented} | {EscapePipe(d.Observed)} |");
            }
        }

        return 0;
    }

    private static string EscapePipe(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("|", "\\|");

    private static MetricCheck ValidateMetric(string name, string scope, string shape, double[] value, int windowSize)
    {
        switch (name)
        {
            case "frequencia_por_dezena":
            case "total_de_presencas_na_janela_por_dezena":
            {
                var sum = value.Sum();
                var ok = value.Length == 25 && Math.Abs(sum - 15 * windowSize) < 1e-9 &&
                         value.All(v => v >= 0 && Math.Abs(v - Math.Round(v)) < 1e-9);
                return ok
                    ? new MetricCheck(true, $"len=25; soma={sum} (=15×{windowSize})", "")
                    : new MetricCheck(false, $"len={value.Length}; soma={sum}",
                        $"Esperado vector[25] de contagens inteiras somando 15×N; recebido len={value.Length}, soma={sum}.");
            }
            case "top10_mais_sorteados":
            case "top10_menos_sorteados":
            case "top10_maiores_totais_de_presencas_na_janela":
            case "top10_menores_totais_de_presencas_na_janela":
            {
                var ok = value.Length == 10 && value.All(v => v >= 1 && v <= 25 && Math.Abs(v - Math.Round(v)) < 1e-9);
                return ok
                    ? new MetricCheck(true, $"len=10; itens=[{string.Join(",", value.Select(v => ((int)Math.Round(v)).ToString()))}]", "")
                    : new MetricCheck(false, $"len={value.Length}; itens=[{string.Join(",", value)}]",
                        "Esperado lista de 10 dezenas (1..25) inteiras.");
            }
            case "sequencia_atual_de_presencas_por_dezena":
            case "atraso_por_dezena":
            case "assimetria_blocos":
            {
                var ok = value.Length == 25;
                return ok
                    ? new MetricCheck(true, "len=25", "")
                    : new MetricCheck(false, $"len={value.Length}", "Esperado vector[25].");
            }
            case "pares_no_concurso":
            case "quantidade_vizinhos_por_concurso":
            case "sequencia_maxima_vizinhos_por_concurso":
            case "entropia_linha_por_concurso":
            case "entropia_coluna_por_concurso":
            case "hhi_linha_por_concurso":
            case "hhi_coluna_por_concurso":
            {
                var ok = value.Length == windowSize;
                return ok
                    ? new MetricCheck(true, $"len={windowSize}", "")
                    : new MetricCheck(false, $"len={value.Length}", $"Esperado série com N={windowSize} pontos.");
            }
            case "repeticao_concurso_anterior":
            {
                var ok = value.Length == windowSize || value.Length == windowSize - 1;
                return ok
                    ? new MetricCheck(true, $"len={value.Length} (aceito N ou N-1)", "")
                    : new MetricCheck(false, $"len={value.Length}", "Esperado série com N ou N-1 pontos.");
            }
            case "distribuicao_linha_por_concurso":
            case "distribuicao_coluna_por_concurso":
            {
                var ok = value.Length == windowSize * 5;
                return ok
                    ? new MetricCheck(true, $"len={value.Length} (= {windowSize}×5, série flatten)", "")
                    : new MetricCheck(false, $"len={value.Length}",
                        $"Esperado série de vetores[5] flatten com len={windowSize * 5}.");
            }
            default:
                return new MetricCheck(true, $"sem regra de validação específica (scope={scope}, shape={shape}, len={value.Length})", "");
        }
    }

    private static JsonElement ReadStructuredJson(CallToolResult response)
    {
        if (response.StructuredContent is JsonElement structured)
        {
            return structured;
        }

        var text = response.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault()
            ?.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Tool response did not include structured JSON.");
        }

        return JsonSerializer.Deserialize<JsonElement>(text);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var i = 0; i < 20 && dir is not null; i++)
        {
            if (File.Exists(Path.Combine(dir.FullName, "LotofacilMcp.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root (LotofacilMcp.sln).");
    }

    private sealed record MetricCheck(bool IsOk, string Evidence, string ActualBehavior);
    private sealed record Divergence(string MetricName, string Documented, string Observed);
}
