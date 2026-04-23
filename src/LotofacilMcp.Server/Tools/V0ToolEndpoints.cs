namespace LotofacilMcp.Server.Tools;

public static class V0ToolEndpoints
{
    public static IEndpointRouteBuilder MapV0ToolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        MapToolRoute(endpoints, "/tools/get_draw_window", HandleGetDrawWindowAsync);
        MapToolRoute(endpoints, "/tools/compute_window_metrics", HandleComputeWindowMetricsAsync);
        MapToolRoute(endpoints, "/tools/analyze_indicator_stability", HandleAnalyzeIndicatorStabilityAsync);
        MapToolRoute(endpoints, "/tools/compose_indicator_analysis", HandleComposeIndicatorAnalysisAsync);
        MapToolRoute(endpoints, "/tools/analyze_indicator_associations", HandleAnalyzeIndicatorAssociationsAsync);
        MapToolRoute(endpoints, "/tools/summarize_window_patterns", HandleSummarizeWindowPatternsAsync);
        MapToolRoute(endpoints, "/tools/summarize_window_aggregates", HandleSummarizeWindowAggregatesAsync);
        MapToolRoute(endpoints, "/tools/generate_candidate_games", HandleGenerateCandidateGamesAsync);
        MapToolRoute(endpoints, "/tools/explain_candidate_games", HandleExplainCandidateGamesAsync);

        // Alias REST (deprecado): manter compatibilidade sem sugerir que isso é MCP/HTTP.
        MapToolRoute(endpoints, "/mcp/tools/get_draw_window", HandleGetDrawWindowAsync);
        MapToolRoute(endpoints, "/mcp/tools/compute_window_metrics", HandleComputeWindowMetricsAsync);
        MapToolRoute(endpoints, "/mcp/tools/analyze_indicator_stability", HandleAnalyzeIndicatorStabilityAsync);
        MapToolRoute(endpoints, "/mcp/tools/compose_indicator_analysis", HandleComposeIndicatorAnalysisAsync);
        MapToolRoute(endpoints, "/mcp/tools/analyze_indicator_associations", HandleAnalyzeIndicatorAssociationsAsync);
        MapToolRoute(endpoints, "/mcp/tools/summarize_window_patterns", HandleSummarizeWindowPatternsAsync);
        MapToolRoute(endpoints, "/mcp/tools/summarize_window_aggregates", HandleSummarizeWindowAggregatesAsync);
        MapToolRoute(endpoints, "/mcp/tools/generate_candidate_games", HandleGenerateCandidateGamesAsync);
        MapToolRoute(endpoints, "/mcp/tools/explain_candidate_games", HandleExplainCandidateGamesAsync);

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

    private static async Task<IResult> HandleComposeIndicatorAnalysisAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<ComposeIndicatorAnalysisRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.ComposeIndicatorAnalysis(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }

    private static async Task<IResult> HandleAnalyzeIndicatorAssociationsAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<AnalyzeIndicatorAssociationsRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.AnalyzeIndicatorAssociations(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }

    private static async Task<IResult> HandleSummarizeWindowPatternsAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<SummarizeWindowPatternsRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.SummarizeWindowPatterns(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }

    private static async Task<IResult> HandleSummarizeWindowAggregatesAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<SummarizeWindowAggregatesRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.SummarizeWindowAggregates(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }

    private static async Task<IResult> HandleGenerateCandidateGamesAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<GenerateCandidateGamesRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.GenerateCandidateGames(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }

    private static async Task<IResult> HandleExplainCandidateGamesAsync(HttpRequest request, V0Tools tools)
    {
        var (toolRequest, bindingError) = await ToolRequestBinding.BindAsync<ExplainCandidateGamesRequest>(request);
        if (bindingError is not null)
        {
            return Results.BadRequest(bindingError);
        }

        var response = tools.ExplainCandidateGames(toolRequest!);
        return response is ContractErrorEnvelope errorEnvelope
            ? Results.BadRequest(errorEnvelope)
            : Results.Ok(response);
    }
}
