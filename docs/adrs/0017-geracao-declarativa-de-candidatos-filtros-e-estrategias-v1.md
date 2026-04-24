# ADR 0017 — Geração declarativa de candidatos: critérios, filtros, estratégias e rastreabilidade v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) · [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

O brief pede geração determinística de jogos candidatos com:

- critérios escolhidos,
- filtros estruturais declarativos,
- comparação entre estratégias,
- rastreabilidade completa (critérios, pesos, filtros, janela, `seed`, `search_method`, `tie_break_rule`, versões).

A build atual entrega uma geração mínima com uma estratégia pública e filtros mais presentes na explicação do que como configuração de geração (GAPs B11–B14).

## Decisão

Evoluir `generate_candidate_games` para um contrato declarativo, mantendo `explain_candidate_games` como auditoria/justificativa.

Em particular:

1) `generate_candidate_games` deve aceitar `criteria`, `weights` e `filters` explícitos por item de plano.
2) O resultado deve ecoar a configuração aplicada (`applied_configuration`) e as versões efetivas usadas.
3) Deve existir pluralidade real de estratégias públicas e comparáveis.

## Especificação (v1)

### Request (proposta)

Para cada item do `plan[]`:

- `strategy_name`
- `strategy_version` (opcional; default documentado ou erro se não houver versão única)
- `count`
- `search_method`
- `tie_break_rule` (quando aplicável)
- `criteria` (objeto livre versionado por estratégia, mas com schema documentado)
- `weights` (quando aplicável)
- `filters[]` (lista de filtros estruturais com parâmetros e versão)

### Response (mínimo)

Para cada candidato:

- `numbers`
- `strategy_name`, `strategy_version`
- `search_method`, `tie_break_rule`
- `seed_used`
- `applied_configuration` (eco do efetivamente usado, inclusive defaults resolvidos)

### Invariantes

- Sem defaults ocultos: qualquer default aplicado deve ser documentado e refletido em `applied_configuration.resolved_defaults`.
- Determinismo: o mesmo request + dataset ⇒ mesmos candidatos e mesmas explicações.

## Consequências

### Positivas

- Fecha a lacuna “explico filtros mas não consigo configurá-los”.
- Permite comparação entre estratégias de forma real e rastreável.

### Trade-offs

- Aumenta superfície de contrato e validação; exige bom plano de testes.

## Critérios de verificação

1) É possível gerar candidatos aplicando filtros declarativos (não só explicar depois).
2) Pelo menos 2 estratégias públicas produzem resultados comparáveis na mesma janela.
3) A resposta inclui `applied_configuration` e permite auditoria completa.

## Referências internas

- [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md)
- [generation-strategies.md](../generation-strategies.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
