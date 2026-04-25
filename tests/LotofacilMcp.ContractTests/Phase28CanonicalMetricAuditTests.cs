using System.Text.RegularExpressions;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Server.Tools;
using Xunit.Abstractions;

namespace LotofacilMcp.ContractTests;

public sealed class Phase28CanonicalMetricAuditTests
{
    private readonly ITestOutputHelper _output;

    public Phase28CanonicalMetricAuditTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CanonicalMetrics_FromMetricCatalogTable1_AreTrackedByBuildRegistry_And_AuditShowsRouteAllowlistGaps()
    {
        // Spec-driven reference:
        // - docs/metric-catalog.md (Tabela 1 — Identificação e tipagem; Status=canonica)
        // - docs/mcp-tool-contract.md (rota/tool compute_window_metrics tem allowlist por build)
        // - docs/spec-driven-execution-guide.md (Fase 28.1 — auditoria de lacunas: catálogo → superfície real)
        //
        // IMPORTANT: this audit does NOT infer "implemented" by file/class existence.
        // It treats a metric as "available in this build" only if it is:
        // - registered in the build capability registry (MetricAvailabilityCatalog), AND/OR
        // - dispatchable / allowed in the relevant tool route allowlist.

        var canonicalFromDocs = LoadCanonicalMetricNamesFromTable1();

        var knownByRegistry = MetricAvailabilityCatalog.GetKnownMetricNames().ToHashSet(StringComparer.Ordinal);
        var implementedByRegistry = MetricAvailabilityCatalog.GetImplementedMetricNames().ToHashSet(StringComparer.Ordinal);
        var computeWindowAllowed = MetricAvailabilityCatalog
            .GetComputeWindowMetricsAllowedMetrics(allowPending: false)
            .ToHashSet(StringComparer.Ordinal);

        // Explicit reference to the repo discovery mechanism:
        // V0Tools.DiscoverCapabilities is the deterministic build-surface publisher, and should match the registry.
        var surface = Assert.IsType<DiscoverCapabilitiesResponse>(new V0Tools().DiscoverCapabilities(new DiscoverCapabilitiesRequest()));
        Assert.Equal(
            MetricAvailabilityCatalog.GetComputeWindowMetricsAllowedMetrics(allowPending: false),
            surface.Metrics.ComputeWindowMetricsAllowed);

        var canonicalNotTracked = canonicalFromDocs
            .Where(name => !knownByRegistry.Contains(name))
            .OrderBy(static n => n, StringComparer.Ordinal)
            .ToArray();

        // This repository evolves in waves; for the current build profile ("v0"), the audit is expected to
        // surface canonical metrics that exist in the normative catalog but are not yet tracked by the
        // build registry/discovery. We keep this list explicit and deterministic to avoid silent drift.
        //
        // If you implement/register any of these, update this expected snapshot in the same PR.
        var expectedNotTrackedInThisBuild =
            new[]
            {
                "sequencia_atual_de_presencas_por_dezena",
                "top10_maiores_totais_de_presencas_na_janela",
                "top10_menores_totais_de_presencas_na_janela",
                "total_de_presencas_na_janela_por_dezena"
            };

        Assert.Equal(expectedNotTrackedInThisBuild, canonicalNotTracked);

        // Deterministic gap report for this build profile (route/tool allowlist matters).
        // This does not fail the build by default; it is evidence for the current build surface.
        var canonicalImplemented = canonicalFromDocs
            .Where(name => implementedByRegistry.Contains(name))
            .OrderBy(static n => n, StringComparer.Ordinal)
            .ToArray();

        var canonicalMissingImplementation = canonicalFromDocs
            .Where(name => !implementedByRegistry.Contains(name))
            .OrderBy(static n => n, StringComparer.Ordinal)
            .ToArray();

        var canonicalMissingFromComputeWindowRoute = canonicalFromDocs
            .Where(name => !computeWindowAllowed.Contains(name))
            .OrderBy(static n => n, StringComparer.Ordinal)
            .ToArray();

        _output.WriteLine("=== Phase 28.1 audit (docs → registry → route allowlist) ===");
        _output.WriteLine($"Canonical metrics in Table 1: {canonicalFromDocs.Count}");
        _output.WriteLine($"Canonical metrics NOT tracked by registry/discovery yet: {canonicalNotTracked.Length}");
        _output.WriteLine($"Implemented (registry): {canonicalImplemented.Length}");
        _output.WriteLine($"Missing implementation (registry): {canonicalMissingImplementation.Length}");
        _output.WriteLine($"Not exposed by compute_window_metrics allowlist: {canonicalMissingFromComputeWindowRoute.Length}");
        _output.WriteLine("");
        _output.WriteLine("Not tracked by registry/discovery (yet):");
        _output.WriteLine(string.Join(Environment.NewLine, canonicalNotTracked.Select(static n => $"- {n}")));
        _output.WriteLine("");
        _output.WriteLine("Implemented (registry):");
        _output.WriteLine(string.Join(Environment.NewLine, canonicalImplemented.Select(static n => $"- {n}")));
        _output.WriteLine("");
        _output.WriteLine("Missing (registry Implemented=false):");
        _output.WriteLine(string.Join(Environment.NewLine, canonicalMissingImplementation.Select(static n => $"- {n}")));
        _output.WriteLine("");
        _output.WriteLine("Missing from compute_window_metrics allowlist (route exposure):");
        _output.WriteLine(string.Join(Environment.NewLine, canonicalMissingFromComputeWindowRoute.Select(static n => $"- {n}")));
    }

    private static IReadOnlyList<string> LoadCanonicalMetricNamesFromTable1()
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "docs", "metric-catalog.md");

        var markdown = File.ReadAllText(path);

        // Narrowly parse only the markdown table of "Tabela 1 — Identificação e tipagem".
        // We only accept rows that contain:
        // - first cell: `metric_name` in backticks
        // - last cell: canonica
        var startAnchor = "<a id=\"tabela-1-identificacao-e-tipagem\"></a>";
        var startIdx = markdown.IndexOf(startAnchor, StringComparison.Ordinal);
        if (startIdx < 0)
        {
            throw new InvalidOperationException($"Could not find Table 1 anchor in {path}: {startAnchor}");
        }

        var slice = markdown[startIdx..];
        var lines = slice.Split('\n');

        var inTable = false;
        var names = new List<string>();

        // Example row:
        // | `frequencia_por_dezena` | base | configurável | `window` | `count` | `vector_by_dezena` | 1.0.0 | canonica |
        var rowRegex = new Regex(
            @"^\|\s*`(?<name>[a-z0-9_]+)`\s*\|.*\|\s*(?<status>canonica)\s*\|\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');
            if (!inTable)
            {
                if (line.StartsWith("| Nome | Categoria | Janela |", StringComparison.Ordinal))
                {
                    inTable = true;
                }

                continue;
            }

            // Table ends when we hit a non-table line after we've started.
            if (!line.StartsWith("|", StringComparison.Ordinal))
            {
                break;
            }

            // Skip header separator row.
            if (line.StartsWith("|------", StringComparison.Ordinal))
            {
                continue;
            }

            var match = rowRegex.Match(line);
            if (!match.Success)
            {
                continue;
            }

            var name = match.Groups["name"].Value;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            names.Add(name);
        }

        return names
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static n => n, StringComparer.Ordinal)
            .ToArray();
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

        throw new InvalidOperationException("Could not locate repo root (LotofacilMcp.sln) from current directory.");
    }
}

