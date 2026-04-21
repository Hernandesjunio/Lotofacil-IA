# Plano de testes de contrato e fixtures douradas (execução)

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Complementa [test-plan.md](test-plan.md) com **ordem de implementação**, **layout de fixtures** e **matriz mínima** para a fatia vertical e para a suíte completa. A especificação normativa continua sendo o contrato MCP e o catálogo de métricas.

## Ordem sugerida (dependências)

1. **Fase A — Fatia V0** (bloqueia o restante): fixture sintética mínima + `frequencia_por_dezena` + tools `get_draw_window` e `compute_window_metrics` + `deterministic_hash` + erro `UNKNOWN_METRIC`. Ver [vertical-slice.md](vertical-slice.md).
2. **Fase B — Testes de contrato por tool**: expandir validação de schema/códigos de erro conforme tabela “Cobertura por tool” em `test-plan.md`.
3. **Fase C — Golden por métrica**: para cada linha das tabelas de métricas em `test-plan.md`, associar fixture e valores esperados congelados.
4. **Fase D — Integração e E2E**: janela real curta/longa, prompts de `prompt-catalog.md`, determinismo repetido (seção “Cobertura de determinismo” em `test-plan.md`).

## Layout de fixtures (convênio)

Armazenar sob `tests/fixtures/` (ou equivalente na linguagem escolhida):

| Arquivo | Conteúdo |
|---------|-----------|
| `synthetic_min_window.json` | Poucos concursos com contagens manuais triviais; usado na V0 e em testes de fórmula. |
| `window_short_real.json` | Recorte de histórico real pequeno; `dataset_version` = hash do conteúdo. |
| `window_long_real.json` | Recorte longo para estabilidade e correlações. |
| `pathological_runs.json` | Runs longos e concentração extrema. |
| `tie_heavy.json` | Empates frequentes para rankings e top 10. |

Cada arquivo deve listar `draws` no formato canônico do contrato (`numbers` ordenados). Metadado opcional no root: `dataset_version` explícito ou regra de hash documentada.

## Fixtures douradas (golden)

**Definição:** para um par `(fixture, window_size, end_contest_id, metric_request)`, o teste compara o **JSON canônico serializado** da resposta (ou subconjunto estável) com um arquivo `.golden.json` versionado no repositório.

**Regras:**

- Atualização de golden só em PR que também altere contrato, catálogo de métricas ou semântica intencional (revisão humana obrigatória).
- Incluir nos testes a asserção de `metric_version` / versão da métrica conforme [metric-catalog.md](metric-catalog.md).
- Para `deterministic_hash`, o golden pode armazenar apenas o hash esperado (string) ou a resposta completa — preferir hash + amostra parcial se o payload for grande.

## Matriz mínima — testes de contrato (tool × invariante)

Documentação de referência: [mcp-tool-contract.md](mcp-tool-contract.md).

| Tool | Invariante de contrato a validar no teste |
|------|---------------------------------------------|
| `get_draw_window` | Ordem crescente de `contest_id`; rejeição de `window_size` inválido. |
| `compute_window_metrics` | Cada `MetricValue` com `scope`, `shape`, `unit`, `version`; erro `UNKNOWN_METRIC`. |
| `analyze_indicator_stability` | Exigência de `aggregation` quando aplicável; método de normalização. |
| `compose_indicator_analysis` | Componentes, pesos e operador explícitos; erros de composição. |
| `analyze_indicator_associations` | Método permitido; séries compatíveis. |
| `summarize_window_patterns` | Agregações e features compatíveis. |
| `generate_candidate_games` | `seed`, estratégia, versão, exclusões reportadas. |
| `explain_candidate_games` | Breakdown e métricas declaradas. |

## Matriz — Fase C (métrica × tipo de teste)

Para cada métrica do catálogo, um caso deve indicar:

- **Fórmula**: [`test-plan.md` § Cobertura por métrica](test-plan.md) (colunas positivo/negativo).
- **Contrato**: `shape`, `scope`, `unit` em [`metric-catalog.md`](metric-catalog.md) Tabela 1.
- **Golden**: apontar para arquivo em `tests/fixtures/golden/<metric_name>/<case_id>.golden.json` após a primeira implementação.

`estabilidade_ranking@1.0.0` é canônica: exige teste de fórmula (positivo e negativo) e golden versionado após a primeira implementação, com as mesmas regras de revisão dos demais goldens do repositório.

## Ligação com prompts

Cada família de [prompt-catalog.md](prompt-catalog.md) deve mapear para pelo menos um teste E2E (já exigido em [test-plan.md](test-plan.md)). A Fase D não inicia antes da Fase B estar verde para as tools usadas naquele prompt.

## Sumário

| Documento | Papel |
|-----------|--------|
| `test-plan.md` | O *quê* deve ser coberto (matriz completa do domínio). |
| Este arquivo | *Como* organizar fixtures, goldens e ordem de entrega. |
| `vertical-slice.md` | Primeiro incremento executável obrigatório. |
