# ADR 0013 — Janela uniforme por extremos em todas as tools v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) · [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

O ADR 0008 (D2) define equivalência normativa para janela por extremos (`start_contest_id` e `end_contest_id` inclusivos).

Na build atual, essa forma aparece apenas em parte da superfície, criando inconsistência operacional (GAP B15).

## Decisão

Padronizar a semântica de janela por extremos em **todas as tools orientadas a janela**:

- `compute_window_metrics`
- `get_draw_window`
- `analyze_indicator_stability`
- `compose_indicator_analysis`
- `analyze_indicator_associations`
- `summarize_window_patterns`
- `summarize_window_aggregates`
- `generate_candidate_games`
- `explain_candidate_games`

Mantendo as regras de rejeição de combinações ambíguas, e mantendo a janela sempre explícita.

## Especificação (v1)

Para cada tool acima:

- adicionar `start_contest_id` opcional;
- manter `end_contest_id` (obrigatório quando `start_contest_id` existir);
- resolver janela por regra do ADR 0008 D2;
- rejeitar combinações incompatíveis (ex.: `window_size` conflitante com extremos).

## Consequências

### Positivas

- Um único modelo mental para o consumidor/host.
- Menos risco de janelas divergentes entre tools numa mesma conversa/pipeline.

### Trade-offs

- Mais campos no contrato; exige atualização coordenada de docs e testes.

## Critérios de verificação

1) Todas as tools orientadas a janela aceitam a forma por extremos, com mesma semântica.
2) Casos inválidos retornam `INVALID_REQUEST` (ou equivalente) com detalhes do conflito.
3) A janela resolvida aparece no `window` do payload, consistente entre tools.

## Referências internas

- [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) (D2)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
