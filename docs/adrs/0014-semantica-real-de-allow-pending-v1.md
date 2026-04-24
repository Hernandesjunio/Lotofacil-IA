# ADR 0014 — Semântica real de `allow_pending` v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

O parâmetro `allow_pending` existe no contrato/superfície para `compute_window_metrics`, entra no hash determinístico, mas hoje não altera o comportamento.

Isso cria frustração e inconsistência semântica (GAP B17).

## Decisão

Definir e implementar uma semântica **observável e testável** para `allow_pending`.

`allow_pending` deve controlar apenas uma coisa: **habilitar métricas explicitamente marcadas como “pendentes”** na build, mantendo:

- determinismo,
- rastreabilidade,
- discovery da superfície real (ADR 0011/0012).

## Especificação (v1)

- Métricas podem ter status:
  - `stable` (default)
  - `pending` (implementada mas ainda em fase de validação/cobertura)
- Quando `allow_pending = false`:
  - requisições para métricas `pending` retornam erro canônico de “indisponível nesta rota/build” com detalhe `reason = pending_requires_opt_in`.
- Quando `allow_pending = true`:
  - métricas `pending` passam a ser aceitas onde o registro/rota indicar.

## Consequências

### Positivas

- Remove “parâmetro inerte” do ponto de vista do consumidor sem remover o contrato.
- Permite rollout incremental de métricas sem enganar o usuário.

### Trade-offs

- Exige governança: declarar quais métricas são `pending` e manter discovery coerente.

## Critérios de verificação

1) Com `allow_pending=false`, métricas pendentes falham com erro canônico e mensagem consistente.
2) Com `allow_pending=true`, as mesmas métricas passam a funcionar (onde suportadas).
3) `discover_capabilities` declara explicitamente quais métricas são `pending`.

## Referências internas

- [ADR 0011](0011-tool-de-discovery-de-capacidades-por-build-v1.md)
- [ADR 0012](0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
