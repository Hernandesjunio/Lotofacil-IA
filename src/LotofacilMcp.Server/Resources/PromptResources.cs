using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using LotofacilMcp.Server.Prompting;

namespace LotofacilMcp.Server.Resources;

[McpServerResourceType]
public sealed class PromptResources
{
    private const string MimeMarkdown = "text/markdown";

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/index@1.0.0",
        Name = "Prompt templates index",
        MimeType = MimeMarkdown)]
    [Description("Índice de templates Markdown (resources) para copiar/colar no chat ao usar o MCP lotofacil-ia.")]
    public static ResourceContents Index()
        => ReadMarkdownResource(PromptCatalog.IndexUri, PromptCatalog.IndexFileName);

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/dashboard-essentials@1.0.0",
        Name = "Dashboard essencial (20/100/500)",
        MimeType = MimeMarkdown)]
    [Description("Template para painel geral (recente + ranking por dezena) com rastreabilidade.")]
    public static ResourceContents DashboardEssentials()
        => ReadMarkdownResource("lotofacil-ia://prompts/dashboard-essentials@1.0.0", "dashboard-essentials@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/repetition-overlap@1.0.0",
        Name = "Repetição / sobreposição entre concursos",
        MimeType = MimeMarkdown)]
    [Description("Template focado em repetição contra o concurso anterior e comportamento repetitivo na janela.")]
    public static ResourceContents RepetitionOverlap()
        => ReadMarkdownResource("lotofacil-ia://prompts/repetition-overlap@1.0.0", "repetition-overlap@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/neighbors-runs@1.0.0",
        Name = "Vizinhos e runs no tempo",
        MimeType = MimeMarkdown)]
    [Description("Template para analisar adjacências (vizinhos) e runs ao longo do tempo.")]
    public static ResourceContents NeighborsRuns()
        => ReadMarkdownResource("lotofacil-ia://prompts/neighbors-runs@1.0.0", "neighbors-runs@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/shape-lines-columns@1.0.0",
        Name = "Forma no volante (linhas/colunas)",
        MimeType = MimeMarkdown)]
    [Description("Template para repetição de forma: linhas/colunas, entropia e HHI.")]
    public static ResourceContents ShapeLinesColumns()
        => ReadMarkdownResource("lotofacil-ia://prompts/shape-lines-columns@1.0.0", "shape-lines-columns@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/frequency-vs-delay@1.0.0",
        Name = "Frequência vs atraso (por dezena)",
        MimeType = MimeMarkdown)]
    [Description("Template para painel por dezena com frequência, top10, atraso e estado atual.")]
    public static ResourceContents FrequencyVsDelay()
        => ReadMarkdownResource("lotofacil-ia://prompts/frequency-vs-delay@1.0.0", "frequency-vs-delay@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/blocks-presence-absence@1.0.0",
        Name = "Blocos de presença/ausência (satélite)",
        MimeType = MimeMarkdown)]
    [Description("Template para blocos de presença/ausência e assimetria (métricas satélite).")]
    public static ResourceContents BlocksPresenceAbsence()
        => ReadMarkdownResource("lotofacil-ia://prompts/blocks-presence-absence@1.0.0", "blocks-presence-absence@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/ranking-stability@1.0.0",
        Name = "Estabilidade do ranking (janela média)",
        MimeType = MimeMarkdown)]
    [Description("Template para medir estabilidade do ranking de frequência por dezena.")]
    public static ResourceContents RankingStability()
        => ReadMarkdownResource("lotofacil-ia://prompts/ranking-stability@1.0.0", "ranking-stability@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/regime-shift@1.0.0",
        Name = "Mudança de regime (comparar janelas)",
        MimeType = MimeMarkdown)]
    [Description("Template para comparar duas janelas e descrever deslocamentos.")]
    public static ResourceContents RegimeShift()
        => ReadMarkdownResource("lotofacil-ia://prompts/regime-shift@1.0.0", "regime-shift@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/associations-sanity@1.0.0",
        Name = "Associações (Spearman) — sanity checks",
        MimeType = MimeMarkdown)]
    [Description("Template para associações Spearman entre séries alinhadas (sem causalidade).")]
    public static ResourceContents AssociationsSanity()
        => ReadMarkdownResource("lotofacil-ia://prompts/associations-sanity@1.0.0", "associations-sanity@1.0.0.md");

    [McpServerResource(
        UriTemplate = "lotofacil-ia://prompts/candidate-vs-history-screening@1.0.0",
        Name = "Triagem candidato × histórico",
        MimeType = MimeMarkdown)]
    [Description("Template para explicar/ranquear jogos candidatos com base no histórico.")]
    public static ResourceContents CandidateVsHistoryScreening()
        => ReadMarkdownResource("lotofacil-ia://prompts/candidate-vs-history-screening@1.0.0", "candidate-vs-history-screening@1.0.0.md");

    private static ResourceContents ReadMarkdownResource(string uri, string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "resources", "prompts", fileName);
        var text = File.ReadAllText(path);

        return new TextResourceContents
        {
            Uri = uri,
            MimeType = MimeMarkdown,
            Text = text
        };
    }
}

