using System.Collections.ObjectModel;

namespace LotofacilMcp.Server.Prompting;

public static class PromptCatalog
{
    public const string IndexId = "index@1.0.0";
    public const string IndexUri = "lotofacil-ia://prompts/index@1.0.0";
    public const string IndexFileName = "index@1.0.0.md";

    public static readonly IReadOnlyList<PromptTemplateInfo> Templates = new ReadOnlyCollection<PromptTemplateInfo>(
        new[]
        {
            new PromptTemplateInfo(
                Id: "dashboard-essentials@1.0.0",
                Uri: "lotofacil-ia://prompts/dashboard-essentials@1.0.0",
                FileName: "dashboard-essentials@1.0.0.md",
                Title: "Dashboard essencial (20/100/500)",
                Description: "Painel geral (recente + ranking por dezena) com rastreabilidade e sem linguagem preditiva.",
                SuggestedWindows: "20/100/(300–500)"),

            new PromptTemplateInfo(
                Id: "repetition-overlap@1.0.0",
                Uri: "lotofacil-ia://prompts/repetition-overlap@1.0.0",
                FileName: "repetition-overlap@1.0.0.md",
                Title: "Repetição / sobreposição entre concursos",
                Description: "Repetição contra o concurso anterior e comportamento repetitivo no curto prazo.",
                SuggestedWindows: "20/(100)"),

            new PromptTemplateInfo(
                Id: "neighbors-runs@1.0.0",
                Uri: "lotofacil-ia://prompts/neighbors-runs@1.0.0",
                FileName: "neighbors-runs@1.0.0.md",
                Title: "Vizinhos e runs no tempo",
                Description: "Adjacências (vizinhos) e tamanho de sequências ao longo do tempo na janela.",
                SuggestedWindows: "20/(200–500)"),

            new PromptTemplateInfo(
                Id: "shape-lines-columns@1.0.0",
                Uri: "lotofacil-ia://prompts/shape-lines-columns@1.0.0",
                FileName: "shape-lines-columns@1.0.0.md",
                Title: "Forma no volante (linhas/colunas)",
                Description: "Distribuição por linha/coluna, entropia (dispersão) e HHI (concentração) na janela.",
                SuggestedWindows: "20/(200–500)"),

            new PromptTemplateInfo(
                Id: "frequency-vs-delay@1.0.0",
                Uri: "lotofacil-ia://prompts/frequency-vs-delay@1.0.0",
                FileName: "frequency-vs-delay@1.0.0.md",
                Title: "Frequência vs atraso (por dezena)",
                Description: "Painel por dezena com frequência, top10, atraso e estado atual.",
                SuggestedWindows: "100/(20)"),

            new PromptTemplateInfo(
                Id: "blocks-presence-absence@1.0.0",
                Uri: "lotofacil-ia://prompts/blocks-presence-absence@1.0.0",
                FileName: "blocks-presence-absence@1.0.0.md",
                Title: "Blocos de presença/ausência (satélite)",
                Description: "Blocos de presença/ausência e assimetria para padrões repetitivos por dezena.",
                SuggestedWindows: "100/(300–500)"),

            new PromptTemplateInfo(
                Id: "ranking-stability@1.0.0",
                Uri: "lotofacil-ia://prompts/ranking-stability@1.0.0",
                FileName: "ranking-stability@1.0.0.md",
                Title: "Estabilidade do ranking (janela média)",
                Description: "Persistência de ordem relativa do ranking por dezena entre sub-janelas.",
                SuggestedWindows: "100/200"),

            new PromptTemplateInfo(
                Id: "regime-shift@1.0.0",
                Uri: "lotofacil-ia://prompts/regime-shift@1.0.0",
                FileName: "regime-shift@1.0.0.md",
                Title: "Mudança de regime (comparar janelas)",
                Description: "Comparação de duas janelas para descrever deslocamentos e mudanças.",
                SuggestedWindows: "20 vs 20 / 20 vs 100 / 100 vs 300–500"),

            new PromptTemplateInfo(
                Id: "associations-sanity@1.0.0",
                Uri: "lotofacil-ia://prompts/associations-sanity@1.0.0",
                FileName: "associations-sanity@1.0.0.md",
                Title: "Associações (Spearman) — sanity checks",
                Description: "Co-movimento descritivo entre séries alinhadas (sem causalidade).",
                SuggestedWindows: "100/(20)"),

            new PromptTemplateInfo(
                Id: "candidate-vs-history-screening@1.0.0",
                Uri: "lotofacil-ia://prompts/candidate-vs-history-screening@1.0.0",
                FileName: "candidate-vs-history-screening@1.0.0.md",
                Title: "Triagem candidato × histórico",
                Description: "Explicar/ranquear jogos candidatos com métricas estruturais + slot + outlier.",
                SuggestedWindows: "100/(20)"),
        });

    public static PromptTemplateInfo? TryGetByUri(string uri)
    {
        if (string.Equals(uri, IndexUri, StringComparison.OrdinalIgnoreCase))
        {
            return new PromptTemplateInfo(IndexId, IndexUri, IndexFileName, "Índice de templates", "Lista e guia rápido de templates disponíveis.", "-");
        }

        return Templates.FirstOrDefault(t => string.Equals(t.Uri, uri, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record PromptTemplateInfo(
    string Id,
    string Uri,
    string FileName,
    string Title,
    string Description,
    string SuggestedWindows);

