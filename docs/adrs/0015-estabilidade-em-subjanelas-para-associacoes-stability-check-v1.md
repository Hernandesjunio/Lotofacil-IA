# ADR 0015 — Estabilidade em subjanelas para associações (`stability_check`) v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) · [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

O contrato expõe `stability_check` em `analyze_indicator_associations`, mas a build atual rejeita o campo e retorna apenas magnitude global.

O brief pede análise de associação com estabilidade/persistência. ADR 0006 (D2) já fixa semântica de erro para `stability_check` não suportado.

Este ADR define como implementar `stability_check` de forma determinística e rastreável.

## Decisão

Implementar `stability_check` como cálculo de robustez da associação via **subjanelas determinísticas**, retornando um bloco `association_stability` não nulo quando solicitado.

## Especificação (v1)

### Entrada (`stability_check`)

Parâmetros normativos (proposta):

- `subwindow_size` (int, > 1)
- `stride` (int, >= 1)
- `min_subwindows` (int, >= 2)

### Saída (`association_stability`)

Estrutura mínima:

- `subwindow_size`, `stride`, `subwindows_count`
- `method` (ex.: `spearman`)
- estatísticas sobre a distribuição das correlações por subjanela:
  - `mean`, `median`, `p10`, `p90`, `min`, `max`, `stddev`
- opcional: `sign_consistency_ratio` (fração de subjanelas com mesmo sinal do global)

### Invariantes

- Mesma janela e mesmos parâmetros ⇒ mesmo resultado.
- O cálculo não deve introduzir defaults ocultos: todos os parâmetros de `stability_check` devem ser explícitos (ou ter default documentado no contrato e refletido em `resolved_defaults`).

## Consequências

### Positivas

- Fecha o GAP B09 sem remover contrato.
- Ajuda a interpretar “associação” como co-movimento robusto vs efeito espúrio na janela.

### Trade-offs

- Custo computacional maior (mais correlações). Exigir budget/limites no contrato pode ser necessário.

## Critérios de verificação

1) Com `stability_check` presente, `association_stability` deve ser retornado e não nulo.
2) Com `stability_check` ausente, a resposta pode omitir estabilidade ou retornar `null` com semântica clara.
3) Testes de contrato devem cobrir:
   - caso feliz com estabilidade,
   - erro para parâmetros inválidos,
   - determinismo (hash canônico).

## Referências internas

- [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) (D2)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
