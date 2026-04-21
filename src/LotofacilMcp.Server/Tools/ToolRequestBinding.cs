using System.Text.Json;

namespace LotofacilMcp.Server.Tools;

internal static class ToolRequestBinding
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<(TRequest? Request, ContractErrorEnvelope? Error)> BindAsync<TRequest>(HttpRequest httpRequest)
    {
        if (httpRequest.ContentLength is 0)
        {
            return (default, InvalidRequest("request body is required.", "body"));
        }

        try
        {
            var request = await JsonSerializer.DeserializeAsync<TRequest>(httpRequest.Body, JsonOptions, httpRequest.HttpContext.RequestAborted);
            if (request is null)
            {
                return (default, InvalidRequest("request body is required.", "body"));
            }

            return (request, null);
        }
        catch (JsonException)
        {
            return (default, InvalidRequest("request body must be valid JSON.", "body"));
        }
    }

    private static ContractErrorEnvelope InvalidRequest(string message, string field)
    {
        return new ContractErrorEnvelope(new ContractError(
            Code: "INVALID_REQUEST",
            Message: message,
            Details: new Dictionary<string, object?>
            {
                ["field"] = field
            }));
    }
}
