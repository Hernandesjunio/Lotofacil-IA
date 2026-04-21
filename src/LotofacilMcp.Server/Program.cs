using LotofacilMcp.Server.DependencyInjection;
using LotofacilMcp.Server.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddV0Server(builder.Configuration, builder.Environment);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "LotofacilMcp.Server",
    phase = "v0"
}));

app.MapV0ToolEndpoints();

app.Run();

public partial class Program;
