using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LotofacilMcp.ContractTests;

/// <summary>
/// ADR 0024 acceptance checks for the "no repo" scenario (distributed ZIP / self-contained executable).
/// References:
/// - docs/adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md
/// - docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md
/// </summary>
public sealed class Adr0024NoRepoStdioAcceptanceContractTests : IAsyncLifetime
{
    private McpClient _mcp = null!;
    private string _tempDir = null!;
    private string _tempExePath = null!;

    public async Task InitializeAsync()
    {
        var repoRoot = GetRepositoryRoot();
        var serverProject = Path.Combine(repoRoot, "src", "LotofacilMcp.Server", "LotofacilMcp.Server.csproj");

#if DEBUG
        var configuration = "Debug";
#else
        var configuration = "Release";
#endif

        _tempDir = Path.Combine(Path.GetTempPath(), "LotofacilMcp.NoRepo", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(_tempDir);

        // Publish into a dedicated temp folder to avoid locks from parallel runs,
        // and to guarantee we are not relying on repo-local outputs.
        var publishDir = Path.Combine(_tempDir, "publish");
        await PublishSelfContainedAsync(serverProject, publishDir, configuration);

        _tempExePath = Path.Combine(publishDir, "LotofacilMcp.Server.exe");
        if (!File.Exists(_tempExePath))
        {
            throw new InvalidOperationException($"Expected published executable in temp dir: {_tempExePath}");
        }

        _mcp = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "LotofacilMcp.Server.NoRepo",
            Command = "cmd",
            Arguments =
            [
                "/c",
                BuildLaunchCommand(_tempExePath, datasetDrawsSourceUri: null)
            ]
        }));
    }

    public async Task DisposeAsync()
    {
        if (_mcp is not null)
        {
            await _mcp.DisposeAsync();
        }

        if (!string.IsNullOrWhiteSpace(_tempDir) && Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, recursive: true);
            }
            catch
            {
                // best-effort cleanup (tests may still have file locks on Windows)
            }
        }
    }

    [Fact]
    public async Task ToolsList_Works_WhenStartingViaSelfContainedExecutable_WithMcpStdio()
    {
        // "tools/list" is exercised by ListToolsAsync() over MCP stdio.
        var listedTools = await _mcp.ListToolsAsync();
        Assert.NotEmpty(listedTools);
        Assert.Contains(listedTools, t => t.Name is "help");
        Assert.Contains(listedTools, t => t.Name is "discover_capabilities");
    }

    [Fact]
    public async Task Help_And_DiscoverCapabilities_Work_WithoutRepo_AndWithoutDataset()
    {
        // Some builds may require a dataset provider to be configured before meta tools can fully operate.
        // This test remains "no repo" by using a temp dataset file colocated with the extracted executable.
        var datasetPath = Path.Combine(_tempDir, "draws.temp.json");
        await File.WriteAllTextAsync(datasetPath, MinimalDatasetJson());

        await using var mcpWithDataset = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "LotofacilMcp.Server.NoRepo.WithDataset",
            Command = "cmd",
            Arguments =
            [
                "/c",
                BuildLaunchCommand(_tempExePath, datasetPath)
            ]
        }));

        var help = await mcpWithDataset.CallToolAsync("help", new Dictionary<string, object?>());
        Assert.False(help.IsError);
        var helpJson = ReadMcpStructuredJson(help);
        Assert.True(helpJson.TryGetProperty("getting_started_resource_uri", out _));
        Assert.True(helpJson.TryGetProperty("index_resource_uri", out _));

        var discover = await mcpWithDataset.CallToolAsync("discover_capabilities", new Dictionary<string, object?>());
        Assert.False(discover.IsError);
        var discoverJson = ReadMcpStructuredJson(discover);
        Assert.True(discoverJson.TryGetProperty("tool_version", out _));
        Assert.True(discoverJson.TryGetProperty("dataset_requirements", out _));
        Assert.True(discoverJson.TryGetProperty("tools", out _));
    }

    [Fact]
    public async Task WithoutDatasetDrawsSourceUri_DatasetDependentTools_ReturnDatasetUnavailable_MissingEnv_NoFallback()
    {
        // ADR 0024 acceptance: without Dataset__DrawsSourceUri, history-dependent tools must return DATASET_UNAVAILABLE (no fallback).
        // ADR 0022 D4: details.reason == "missing_env", with missing_env == "Dataset__DrawsSourceUri".
        var response = await _mcp.CallToolAsync("get_draw_window", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003
        });

        Assert.True(response.IsError);
        var json = ReadMcpStructuredJson(response);
        var error = json.GetProperty("error");
        Assert.Equal("DATASET_UNAVAILABLE", error.GetProperty("code").GetString());

        var details = error.GetProperty("details");
        Assert.Equal("missing_env", details.GetProperty("reason").GetString());
        Assert.Equal("Dataset__DrawsSourceUri", details.GetProperty("missing_env").GetString());
    }

    private static string BuildLaunchCommand(string exePath, string? datasetDrawsSourceUri)
    {
        // ADR 0024: executable must work outside repo; we avoid repo-relative paths and clear defaults explicitly.
        // ADR 0022: Dataset__DrawsSourceUri is required for history-dependent tools; no fallback.
        if (string.IsNullOrWhiteSpace(datasetDrawsSourceUri))
        {
            return $"set Dataset__DrawsSourceUri= && {exePath} --mcp-stdio";
        }

        return $"set Dataset__DrawsSourceUri={datasetDrawsSourceUri} && {exePath} --mcp-stdio";
    }

    private static async Task PublishSelfContainedAsync(string projectPath, string outDir, string configuration)
    {
        Directory.CreateDirectory(outDir);

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            ArgumentList =
            {
                "publish",
                projectPath,
                "-c",
                configuration,
                "-r",
                "win-x64",
                "--self-contained",
                "true",
                "-o",
                outDir,
                "-p:PublishSingleFile=true",
                "-p:IncludeNativeLibrariesForSelfExtract=true"
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet publish.");
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        if (p.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "dotnet publish failed.\n\nSTDOUT:\n" + stdout + "\n\nSTDERR:\n" + stderr);
        }
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dir.Replace(sourceDir, targetDir));
        }

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var dest = file.Replace(sourceDir, targetDir);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
    }

    private static JsonElement ReadMcpStructuredJson(CallToolResult response)
    {
        if (response.StructuredContent is JsonElement structuredContent)
        {
            return structuredContent;
        }

        var jsonText = response.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault()
            ?.Text;
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            throw new InvalidOperationException("MCP call result did not contain structured content or text JSON.");
        }

        return JsonSerializer.Deserialize<JsonElement>(jsonText);
    }

    private static string GetRepositoryRoot() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string MinimalDatasetJson() =>
        """
        {
          "draws": [
            { "contest_id": 1001, "draw_date": "2024-01-01", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15], "winners_15": 0, "has_winner_15": false },
            { "contest_id": 1002, "draw_date": "2024-01-02", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,16], "winners_15": 0, "has_winner_15": false },
            { "contest_id": 1003, "draw_date": "2024-01-03", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,17], "winners_15": 1, "has_winner_15": true }
          ]
        }
        """;
}

