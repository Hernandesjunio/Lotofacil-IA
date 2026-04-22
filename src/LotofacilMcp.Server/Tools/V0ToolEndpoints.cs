namespace LotofacilMcp.Server.Tools;

public static class V0ToolEndpoints
{
    public static IEndpointRouteBuilder MapV0ToolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        MapToolRoute(endpoints, "/tools/get_draw_window", HandleGetDrawWindowAsync);
        MapToolRoute(endpoints, "/tools/compute_window_metrics", HandleComputeWindowMetricsAsync);
        MapToolRoute(endpoints, "/tools/analyze_indicator_stability", HandleAnalyzeIndicatorStabilityAsync);

        // Alias REST (deprecado): manter compatibilidade sem sugerir que isso é MCP/HTTP.
        MapToolRoute(endpoints, "/mcp/tools/get_draw_window", HandleGetDrawWindowAsync);
        MapToolRoute(endpoints, "/mcp/tools/compute_window_metrics", HandleComputeWindowMetricsAsync);
        MapToolRoute(endpoints, "/mcp/tools/analyze_indicator_stability", HandleAnalyzeIndicatorStabilityAsync);

        return endpoints;
    }

    private static void MapToolRoute(
        IEndpointRouteBuilder endpoints,
        string route,
        Delegate handler)
    {
        endpoints.MapPost(route, handler);
    }

    private static async Task<IResult> HandleGetDrawWindowAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<GetDrawWindowRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.GetDrawWindow(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }

    private static async Task<IResult> HandleComputeWindowMetricsAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<ComputeWindowMetricsRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.ComputeWindowMetrics(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }

    private static async Task<IResult> HandleAnalyzeIndicatorStabilityAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<AnalyzeIndicatorStabilityRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.AnalyzeIndicatorStability(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }
}
