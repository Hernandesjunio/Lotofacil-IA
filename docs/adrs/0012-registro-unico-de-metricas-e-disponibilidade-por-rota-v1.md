# ADR 0012 — Registro único de métricas e disponibilidade por rota v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) · [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

Hoje há sinais de drift entre:

- “métricas conhecidas no catálogo” vs “métricas realmente implementadas”;
- validações por rota vs dispatcher/cálculo;
- templates/resources citando famílias que não estão expostas na build.

Isso está no núcleo dos GAPs B01/B03 e influencia diretamente B21.

## Decisão

Introduzir um **registro único** (fonte de verdade) para métricas canônicas **na instância/build**, capaz de derivar:

- allowlist de `compute_window_metrics`;
- allowlist de `summarize_window_aggregates` (por `source_metric_name`);
- regras de compatibilidade para associações e composição;
- dados para `discover_capabilities` (ADR 0011);
- mensagens de erro determinísticas e informativas.

Esse registro é “instância/build”, não substitui o `metric-catalog.md` (norma). Ele responde: **o que esta build realmente entrega**.

## Especificação (v1)

O registro deve manter para cada métrica:

- `metric_name`
- `version`
- `scope` e `shape` (quando relevante)
- `implemented` (bool)
- `routes`:
  - `compute_window_metrics` (bool)
  - `summarize_window_aggregates_source` (bool)
  - `associations` (bool + regras de agregação)
  - `compose_indicator_analysis_component` (bool; compatibilidade com target)

## Consequências

### Positivas

- Reduz “catálogo maior do que execução”.
- Permite que o erro `UNKNOWN_METRIC` seja semanticamente estável: desconhecida vs conhecida porém não disponível na rota/build.
- Facilita evolução incremental sem quebrar o contrato: o consumidor entende “não suportado nesta build” com discovery + erro canônico.

### Trade-offs

- Exige governança para manter o registro consistente com código e docs.

## Critérios de verificação

1) O servidor não deve ter múltiplas listas divergentes de métricas por rota.
2) `discover_capabilities` deve ser derivado do registro.
3) `help` e templates devem referenciar apenas capacidades registradas como suportadas, ou marcar explicitamente quando for “planejado”.

## Referências internas

- [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)
- [ADR 0011](0011-tool-de-discovery-de-capacidades-por-build-v1.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
