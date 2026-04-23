using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LotofacilMcp.ContractTests;

[Trait("Category", "LiveOpenAI")]
[Trait("Suite", "Extended")]
public sealed class LiveOpenAiIntegrationPipelineTests : IAsyncLifetime
{
    private const string L6Prompt = "Nos últimos 100 concursos, qual a magnitude da associação (Spearman) entre `pares_no_concurso` e `entropia_linha_por_concurso` na **mesma janela**? Descreva apenas co-movimento na janela, sem inferir que um *causa* o outro.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly WebApplicationFactory<Program> _serverFactory = new();

    private HttpClient _serverClient = default!;
    private HttpClient? _openAiClient;
    private string _openAiModel = string.Empty;
    private int _maxRounds;

    public Task InitializeAsync()
    {
        _serverClient = _serverFactory.CreateClient();

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Task.CompletedTask;
        }

        _openAiModel = Environment.GetEnvironmentVariable("OPENAI_MODEL")?.Trim() switch
        {
            { Length: > 0 } value => value,
            _ => "gpt-4o-mini"
        };

        _maxRounds = ResolveMaxRounds();

        var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL")?.Trim();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://api.openai.com/v1/";
        }

        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            baseUrl += "/";
        }

        _openAiClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(90)
        };
        _openAiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _openAiClient?.Dispose();
        _serverClient.Dispose();
        await _serverFactory.DisposeAsync();
    }

    [Fact]
    [Trait("Scenario", "L6")]
    public async Task L6_AssociationPrompt_RoutesToAnalyzeIndicatorAssociations_WithExplicitWindow()
    {
        if (!IsL6Enabled() || _openAiClient is null)
        {
            return;
        }

        var messages = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["role"] = "system",
                ["content"] =
                    "Você é um assistente para análise descritiva da Lotofácil. Use tool calling quando necessário. " +
                    "Não invente parâmetros obrigatórios e nunca escreva linguagem preditiva."
            },
            new()
            {
                ["role"] = "user",
                ["content"] = L6Prompt
            }
        };

        var toolCallsObserved = new List<(string Name, JsonElement Arguments)>();
        JsonElement? lastToolResponsePayload = null;

        for (var round = 0; round < _maxRounds; round++)
        {
            var completion = await CreateChatCompletionAsync(messages);
            var assistantMessage = completion.GetProperty("choices")[0].GetProperty("message");

            if (!assistantMessage.TryGetProperty("tool_calls", out var toolCallsElement) ||
                toolCallsElement.ValueKind != JsonValueKind.Array ||
                toolCallsElement.GetArrayLength() == 0)
            {
                break;
            }

            messages.Add(new Dictionary<string, object?>
            {
                ["role"] = "assistant",
                ["content"] = assistantMessage.TryGetProperty("content", out var assistantContent)
                    ? assistantContent.GetString()
                    : null,
                ["tool_calls"] = toolCallsElement
            });

            foreach (var toolCall in toolCallsElement.EnumerateArray())
            {
                var toolCallId = toolCall.GetProperty("id").GetString();
                var functionNode = toolCall.GetProperty("function");
                var toolName = functionNode.GetProperty("name").GetString() ?? string.Empty;
                var rawArguments = functionNode.GetProperty("arguments").GetString() ?? "{}";

                JsonElement arguments;
                try
                {
                    arguments = JsonSerializer.Deserialize<JsonElement>(rawArguments);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"OpenAI returned invalid function arguments JSON for tool '{toolName}': {ex.Message}");
                }

                toolCallsObserved.Add((toolName, arguments));
                var toolPayload = await ExecuteToolAsync(toolName, arguments);
                lastToolResponsePayload = toolPayload;

                messages.Add(new Dictionary<string, object?>
                {
                    ["role"] = "tool",
                    ["tool_call_id"] = toolCallId,
                    ["content"] = toolPayload.GetRawText()
                });
            }
        }

        Assert.Contains(toolCallsObserved, call => call.Name == "analyze_indicator_associations");
        var associationCall = toolCallsObserved.Last(call => call.Name == "analyze_indicator_associations");

        Assert.True(
            associationCall.Arguments.TryGetProperty("window_size", out var windowSizeElement),
            "L6 must include explicit window_size in tool arguments.");
        Assert.Equal(100, windowSizeElement.GetInt32());

        Assert.True(
            associationCall.Arguments.TryGetProperty("method", out var methodElement),
            "L6 must include explicit association method.");
        Assert.Equal("spearman", methodElement.GetString(), StringComparer.OrdinalIgnoreCase);

        Assert.True(
            associationCall.Arguments.TryGetProperty("items", out var itemsElement) &&
            itemsElement.ValueKind == JsonValueKind.Array &&
            itemsElement.GetArrayLength() >= 2,
            "L6 must include at least two indicators for association.");
        var itemNames = itemsElement
            .EnumerateArray()
            .Where(item => item.TryGetProperty("name", out _))
            .Select(item => item.GetProperty("name").GetString())
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("pares_no_concurso", itemNames);
        Assert.Contains("entropia_linha_por_concurso", itemNames);

        Assert.NotNull(lastToolResponsePayload);
        AssertServerContractShape(lastToolResponsePayload!.Value);
    }

    private static bool IsL6Enabled()
    {
        var raw = Environment.GetEnvironmentVariable("LIVE_OPENAI_ENABLE_L6");
        return string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static int ResolveMaxRounds()
    {
        var primary = Environment.GetEnvironmentVariable("OPENAI_MAX_ROUNDS");
        var legacy = Environment.GetEnvironmentVariable("LIVE_OPENAI_MAX_ROUNDS");
        var raw = string.IsNullOrWhiteSpace(primary) ? legacy : primary;

        if (!int.TryParse(raw, out var parsed))
        {
            return 6;
        }

        return Math.Clamp(parsed, 1, 12);
    }

    private async Task<JsonElement> CreateChatCompletionAsync(IReadOnlyList<Dictionary<string, object?>> messages)
    {
        var openAiClient = _openAiClient ?? throw new InvalidOperationException("OpenAI client is not initialized.");

        var payload = new Dictionary<string, object?>
        {
            ["model"] = _openAiModel,
            ["temperature"] = 0.0,
            ["max_completion_tokens"] = 400,
            ["tool_choice"] = "auto",
            ["messages"] = messages,
            ["tools"] = BuildToolDefinitions()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };

        using var response = await openAiClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI request failed ({(int)response.StatusCode}): {body}");
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"OpenAI response is not valid JSON: {ex.Message}");
        }
    }

    private async Task<JsonElement> ExecuteToolAsync(string toolName, JsonElement arguments)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            throw new InvalidOperationException("OpenAI called an empty tool name.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/tools/{toolName}")
        {
            Content = new StringContent(arguments.GetRawText(), Encoding.UTF8, "application/json")
        };

        using var response = await _serverClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        JsonElement payload;
        try
        {
            payload = JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Server tool response is not valid JSON for '{toolName}': {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            AssertStructuredErrorEnvelope(payload);
            return payload;
        }

        return payload;
    }

    private static object[] BuildToolDefinitions()
    {
        return
        [
            new Dictionary<string, object?>
            {
                ["type"] = "function",
                ["function"] = new Dictionary<string, object?>
                {
                    ["name"] = "analyze_indicator_associations",
                    ["description"] = "Measure Spearman or Pearson associations between indicator series in a window.",
                    ["parameters"] = new Dictionary<string, object?>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object?>
                        {
                            ["window_size"] = new Dictionary<string, object?>
                            {
                                ["type"] = "integer",
                                ["minimum"] = 1
                            },
                            ["end_contest_id"] = new Dictionary<string, object?>
                            {
                                ["type"] = "integer"
                            },
                            ["method"] = new Dictionary<string, object?>
                            {
                                ["type"] = "string",
                                ["enum"] = new[] { "spearman", "pearson" }
                            },
                            ["top_k"] = new Dictionary<string, object?>
                            {
                                ["type"] = "integer",
                                ["minimum"] = 1
                            },
                            ["items"] = new Dictionary<string, object?>
                            {
                                ["type"] = "array",
                                ["minItems"] = 2,
                                ["items"] = new Dictionary<string, object?>
                                {
                                    ["type"] = "object",
                                    ["properties"] = new Dictionary<string, object?>
                                    {
                                        ["name"] = new Dictionary<string, object?> { ["type"] = "string" },
                                        ["aggregation"] = new Dictionary<string, object?> { ["type"] = "string" }
                                    },
                                    ["required"] = new[] { "name" }
                                }
                            }
                        },
                        ["required"] = new[] { "window_size", "method", "items" }
                    }
                }
            }
        ];
    }

    private static void AssertServerContractShape(JsonElement payload)
    {
        if (payload.TryGetProperty("error", out var errorElement))
        {
            AssertStructuredErrorEnvelope(payload);
            return;
        }

        Assert.True(payload.TryGetProperty("dataset_version", out var datasetVersion));
        Assert.False(string.IsNullOrWhiteSpace(datasetVersion.GetString()));

        Assert.True(payload.TryGetProperty("tool_version", out var toolVersion));
        Assert.False(string.IsNullOrWhiteSpace(toolVersion.GetString()));

        Assert.True(payload.TryGetProperty("deterministic_hash", out var deterministicHash));
        Assert.False(string.IsNullOrWhiteSpace(deterministicHash.GetString()));

        Assert.True(payload.TryGetProperty("window", out var window));
        Assert.True(window.TryGetProperty("size", out var _));
        Assert.True(window.TryGetProperty("start_contest_id", out var _));
        Assert.True(window.TryGetProperty("end_contest_id", out var _));
    }

    private static void AssertStructuredErrorEnvelope(JsonElement payload)
    {
        Assert.True(payload.TryGetProperty("error", out var error), "Error payload must include 'error' object.");
        Assert.True(error.TryGetProperty("code", out var _), "Error payload must include 'error.code'.");
        Assert.True(error.TryGetProperty("message", out var _), "Error payload must include 'error.message'.");
        Assert.True(error.TryGetProperty("details", out var _), "Error payload must include 'error.details'.");
    }
}
