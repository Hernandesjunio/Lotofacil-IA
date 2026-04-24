# ADR 0018 — Pacote de métricas prioritárias (slots, pares/ímpares, blocos, estabilidade, divergência, outliers) v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) · [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

O brief e os templates citam famílias de métricas que hoje não estão materializadas como métricas canônicas consumíveis pelo MCP (ou aparecem apenas como heurísticas internas).

Isso cria a frustração mais visível: “o MCP promete, mas não funciona”.

Este ADR define um pacote de métricas priorizadas para fechar os GAPs:

- B02 (slots e pares/ímpares),
- B08 (estabilidade/divergência),
- B10 (slots em exemplos),
- e parte de B21 (templates executáveis).

## Decisão

Implementar e expor um pacote mínimo de métricas canônicas, com versões e explicações, priorizando:

### Família A — Pares/ímpares

- `pares_impares` (forma canônica e escopo definido no catálogo)

### Família B — Slots

- `matriz_numero_slot`
- `analise_slot`
- `surpresa_slot`

### Família C — Blocos

- `frequencia_blocos`
- `ausencia_blocos`
- `estado_atual_dezena`

### Família D — Estabilidade e divergência

- `estabilidade_ranking`
- `divergencia_kl`
- `persistencia_atraso_extremo`

### Família E — Runs e outliers

- `estatistica_runs`
- `outlier_score`

## Especificação (v1)

1) Cada métrica deve entrar no `metric-catalog.md` (nome, versão, fórmula, shape/scope, unidade e exemplos).
2) Cada métrica deve:
   - ser implementada no domínio,
   - ser exposta via `compute_window_metrics` quando compatível,
   - ser declarada no registro de capacidades (ADR 0012) e visível via discovery (ADR 0011).
3) Templates que citarem essas famílias devem ser revisados para não pedir o que a build não entrega.

## Consequências

### Positivas

- Fecha a maior parte dos “missing metrics” percebidos por consumidores.
- Permite que templates mais usados sejam realmente executáveis.

### Trade-offs

- É um pacote grande: deve ser implementado em etapas, mantendo discovery sempre coerente.

## Critérios de verificação

1) Cada métrica acima tem:
   - entrada no catálogo,
   - implementação,
   - teste(s) de contrato,
   - exposição e discovery.
2) `compute_window_metrics` aceita as métricas quando marcadas como expostas.
3) `summarize_window_aggregates` aceita essas métricas como fonte quando aplicável.

## Referências internas

- [metric-catalog.md](../metric-catalog.md)
- [ADR 0011](0011-tool-de-discovery-de-capacidades-por-build-v1.md)
- [ADR 0012](0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
