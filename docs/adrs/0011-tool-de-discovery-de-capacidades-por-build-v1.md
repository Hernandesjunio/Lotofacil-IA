# ADR 0011 — Tool de discovery de capacidades por build v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) · [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

`tools/list` do protocolo MCP lista tools e descrições, mas não resolve o problema central do consumidor:

- saber **quais métricas** são aceitas por `compute_window_metrics` **nesta build**;
- saber quais métricas são aceitas como `source_metric_name` em `summarize_window_aggregates`;
- saber quais `aggregate_type`, `range_method`, `normalization_method`, `operator`, `transform`, estratégias de geração e filtros são suportados nesta build;
- reduzir frustração por tentativa e erro.

Isso aparece como GAP B20 e afeta B21 (templates/resources sugerindo mais do que a build entrega).

## Decisão

Adicionar uma tool MCP **dedicada** (nome canônico recomendado: `discover_capabilities`) para publicar a superfície real da instância/build de forma determinística.

Essa tool não executa cálculo de métricas: ela publica **metadados de capacidade**.

## Especificação (v1)

### Tool

- **Name:** `discover_capabilities`
- **Input:** vazio (v1)
- **Output:** JSON estruturado com:
  - `tool_version`
  - `build_profile` (ex.: `v0`)
  - `dataset_requirements` (ex.: “requires fixture json path configured”)
  - `window_modes_supported` (por tool): `window_size+end_contest_id`, `start_contest_id+end_contest_id` quando aplicável
  - `tools[]` com:
    - `name`
    - `tool_version` (por tool, quando aplicável)
    - `supported_parameters` (enums e flags relevantes)
    - `capabilities` (resumo humano curto)
  - `metrics`:
    - `implemented_metric_names[]` (fonte de verdade do servidor)
    - `compute_window_metrics_allowed[]`
    - `summarize_window_aggregates_allowed_sources[]`
    - `association_allowed_indicators[]` (ou regra equivalente)
  - `generation`:
    - `strategies[]` (nome+versão)
    - `search_methods[]`
    - `supported_filters[]` (quando os filtros forem declarativos; ver ADR 0017)

### Invariantes

- Deve ser **determinística** para a mesma build/config (sem depender do dataset histórico).
- Deve refletir **o que o servidor realmente aceita**, não o que o catálogo normativo deseja.
- Deve ser suficiente para um cliente/host montar UI/validação sem trial-and-error.

## Consequências

### Positivas

- Reduz frustração: o consumidor vê o que funciona antes de chamar tools de cálculo.
- Alinha a prática ao ADR 0008 (descoberta build vs norma), mas com uma superfície explícita.
- Permite geração de templates/resources “a partir do que existe” (B21) sem mentir.

### Trade-offs

- Exige manutenção junto com mudanças de contrato/capacidade. Para minimizar drift, deve derivar de um registro único (ver ADR 0012).

## Critérios de verificação

1) A tool `discover_capabilities` retorna:
   - lista coerente de métricas implementadas e permitidas por rota;
   - listas de enums coerentes com validações do servidor (ex.: `range_method`, `search_method`, `normalization_method`).
2) A resposta muda apenas quando mudar a build/config relevante (ou bump de versão).
3) `help` continua sendo onboarding; `discover_capabilities` é discovery técnico (sem sobrepor).

## Referências internas

- [ADR 0010](0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md)
- [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)
- [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
