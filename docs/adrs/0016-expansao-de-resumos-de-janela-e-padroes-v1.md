# ADR 0016 — Expansão de resumos de janela e padrões v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) · [ADR 0007](0007-agregados-canonicos-de-janela-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

O brief pede resumos de padrões típicos, faixas descritivas e eventos raros.

Hoje, `summarize_window_patterns` está num recorte mínimo (uma feature escalar e método IQR) e não cobre as famílias mais citadas em templates/exemplos (GAPs B04/B05/B22).

Este ADR define uma evolução incremental mantendo:

- determinismo,
- inputs explícitos,
- coerência com o catálogo de agregados (ADR 0007).

## Decisão

Evoluir `summarize_window_patterns` em duas dimensões:

1) **Cobertura de features**: permitir múltiplas séries escalares já disponíveis como métricas canônicas.
2) **Tipos de resumo**: além de IQR, incluir “raridade/outlier” descritivo quando aplicável (sem linguagem preditiva).

## Especificação (v1)

### Features suportadas (primeira expansão)

Adicionar suporte para séries escalares já existentes, por exemplo:

- `pares_no_concurso`
- `repeticao_concurso_anterior`
- `quantidade_vizinhos_por_concurso`
- `sequencia_maxima_vizinhos_por_concurso`
- `entropia_linha_por_concurso`
- `entropia_coluna_por_concurso`
- `hhi_linha_por_concurso`
- `hhi_coluna_por_concurso`

### Resumos retornados (mínimo)

Para cada feature:

- faixa típica por quantis (IQR ou percentis declarados),
- cobertura observada,
- contagem de outliers (descritiva),
- fences/limiares usados (explícitos),
- explicação curta sem linguagem preditiva.

### Integração com agregados (ADR 0007)

Quando a feature não for escalar ou exigir agregação:

- o caminho recomendado é `summarize_window_aggregates` (ADR 0007),
- `summarize_window_patterns` deve recusar de forma determinística com erro canônico.

## Consequências

### Positivas

- Aproxima o produto dos exemplos do brief sem exigir “montagem manual” no cliente.
- Reduz diferença entre templates e superfície real.

### Trade-offs

- Maior superfície de validação e testes.

## Critérios de verificação

1) As novas features são aceitas e retornam resumo determinístico.
2) Para features incompatíveis, erro canônico com detalhes do motivo e alternativa sugerida (`summarize_window_aggregates`).
3) Templates que dependem dessas features passam a ser executáveis sem contornos.

## Referências internas

- [ADR 0007](0007-agregados-canonicos-de-janela-v1.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
