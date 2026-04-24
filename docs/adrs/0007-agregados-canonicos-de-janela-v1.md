# ADR 0007 — Agregados canônicos de janela (histogramas, padrões e matrizes) como tool MCP (V1)

**Navegação:** [← Brief (índice)](../brief.md) · [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) · [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto — pronto para aceitação.

## Contexto

O MCP expõe métricas canônicas por janela e ferramentas analíticas. Há uma necessidade recorrente de **estruturas agregadas** (histogramas, “top padrões”, matrizes) que:

- são **derivações determinísticas** de métricas canônicas (séries escalares ou séries de vetores);
- são úteis para consumo interativo (chat/UI) e consistência entre clientes, evitando reimplementações divergentes;
- **não** devem ser retornadas em formato de visualização (p.ex. `labels/datasets`) para evitar acoplamento com UI e drift contratual;
- exigem **ordenação canônica**, desempates e regras explícitas para preservar reprodutibilidade e `deterministic_hash`.

Além disso, o repositório distingue **catálogo semântico** de métricas vs. **disponibilidade por rota/build** (ver ADR 0006 D1): nem toda métrica listada no catálogo precisa estar exposta por todas as tools em uma build.

## Decisão

### D1 — Introduzir a tool `summarize_window_aggregates`

Criar uma tool MCP (com endpoint HTTP espelhado) chamada `summarize_window_aggregates` para produzir **agregados canônicos** a partir de métricas canônicas na mesma janela.

- Request inclui `window_size`, `end_contest_id` e uma lista `aggregates[]` (batch).
- Cada item em `aggregates[]` declara explicitamente:
  - `source_metric_name` (nome canônico)
  - `aggregate_type` (enum fechado)
  - parâmetros do agregado (sem defaults semânticos implícitos)

**Justificativa técnica**

- Reduz round-trips: múltiplos agregados em um único request (fluidez; ver ADR 0006 D3).
- Padroniza agregação com semântica e ordenação canônicas (reprodutibilidade + testes dourados).
- Mantém o contrato independente de UI, sem “modelos de gráfico” no payload.

### D2 — Agregados suportados (recorte inicial, enum fechado)

`aggregate_type` (enum fechado) no recorte inicial:

1) `histogram_scalar_series`

- **Fonte:** métrica com `scope="series"` e `shape="series"`.
- **Bucketização:** sempre explícita no request:
  - `bucket_values: [...]` (discreto), ou
  - `min`, `max`, `width` (contínuo/discretizado)
- **Ordenação canônica:** buckets ordenados por `x` ascendente.

2) `topk_patterns_count_vector5_series`

- **Fonte:** métrica com `shape="series_of_count_vector[5]"`.
- **Saída:** itens `{pattern:[5], count, ratio?}`.
- **Ordenação canônica:** `count desc`, desempate por `pattern` lexicográfico asc.

3) `histogram_count_vector5_series_per_position_matrix` (matriz cheia)

- **Fonte:** métrica com `shape="series_of_count_vector[5]"`.
- **Saída:** matriz cheia `matrix[5][K]` (contagens por posição 1..5 × valor `value_min..value_max`).
- **Parâmetros obrigatórios:** `value_min`, `value_max` definem `K = value_max - value_min + 1`.

### D3 — Regras normativas de determinismo e forma

- Toda resposta inclui `dataset_version`, `tool_version`, `deterministic_hash`, e declara a janela (`window`).
- A lista `aggregates[]` na resposta preserva **a ordem do request**.
- Proibição de defaults ocultos: bucketização e dimensões de matriz são sempre declaradas no request.

## Alternativas consideradas

1. **Retornar formatos prontos de gráfico** — rejeitado por acoplamento UI/contrato e drift de visualização.
2. **Derivar tudo no cliente** — rejeitado por inconsistências entre clientes e enfraquecimento de evidências reprodutíveis.
3. **Expandir `summarize_window_patterns` para tudo** — rejeitado por misturar semânticas (IQR/quantis vs agregados estruturais).

## Consequências

### Positivas

- Cobertura de estruturas agregadas sem poluir métricas canônicas.
- Contrato padroniza desempates e ordenação, facilitando goldens.

### Custos

- Nova tool: novo schema, novos testes de contrato e fixtures/goldens.

## Critérios de verificação

1. `summarize_window_aggregates` existe em MCP e no HTTP espelhado.
2. Testes de contrato validam: validação de request, determinismo, ordenação canônica, compatibilidade de shapes e códigos de erro.
3. `mcp-tool-contract.md`, `test-plan.md` e `contract-test-plan.md` são atualizados na mesma linha.

## Referências internas

- [mcp-tool-contract.md](../mcp-tool-contract.md)
- [metric-catalog.md](../metric-catalog.md)
- [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)
- [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) (descoberta, janela por extremos e mapeamento Top 10; **não** substitui D1–D3 desta ADR)
