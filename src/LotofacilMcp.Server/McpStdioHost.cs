using LotofacilMcp.Server.DependencyInjection;
using LotofacilMcp.Server.Tools;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace LotofacilMcp.Server;

internal static class McpStdioHost
{
    private const string StdioSwitch = "--mcp-stdio";

    public static bool IsStdioMode(string[] args)
    {
        return args.Any(arg => string.Equals(arg, StdioSwitch, StringComparison.OrdinalIgnoreCase));
    }

    public static async Task RunAsync(string[] args)
    {
        var filteredArgs = args
            .Where(arg => !string.Equals(arg, StdioSwitch, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var builder = Host.CreateApplicationBuilder(filteredArgs);
        builder.Logging.AddConsole(consoleLogOptions =>
        {
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Services.AddV0Server(builder.Configuration, builder.Environment);
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithTools<V0McpTools>();

        await builder.Build().RunAsync();
    }
}
