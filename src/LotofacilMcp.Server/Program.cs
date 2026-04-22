using LotofacilMcp.Server;
using LotofacilMcp.Server.DependencyInjection;
using LotofacilMcp.Server.Tools;

if (McpStdioHost.IsStdioMode(args))
{
    await McpStdioHost.RunAsync(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddV0Server(builder.Configuration, builder.Environment);
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<V0McpTools>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "LotofacilMcp.Server",
    phase = "v0"
}));

app.MapV0ToolEndpoints();
app.MapMcp("/mcp");

app.Run();

public partial class Program;
