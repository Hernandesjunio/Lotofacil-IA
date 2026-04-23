# Plano de testes de contrato e fixtures douradas (execução)

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Complementa [test-plan.md](test-plan.md) com **ordem de implementação**, **layout de fixtures** e **matriz mínima** para a fatia vertical e para a suíte completa. A especificação normativa continua sendo o contrato MCP e o catálogo de métricas.

## Ordem sugerida (dependências)

1. **Fase A — Fatia V0** (bloqueia o restante): fixture sintética mínima + `frequencia_por_dezena` + tools `get_draw_window` e `compute_window_metrics` + `deterministic_hash` + erro `UNKNOWN_METRIC`. Ver [vertical-slice.md](vertical-slice.md).
2. **Fase B — Testes de contrato por tool**: expandir validação de schema/códigos de erro conforme tabela “Cobertura por tool” em `test-plan.md`.
3. **Fase C — Golden por métrica**: para cada linha das tabelas de métricas em `test-plan.md`, associar fixture e valores esperados congelados.
4. **Fase D — Integração e E2E**: janela real curta/longa, prompts de `prompt-catalog.md`, determinismo repetido (seção “Cobertura de determinismo” em `test-plan.md`).

**Extensão planejada (após ADR 0007):**

5. **Fase B.1 — Agregados canônicos (tool `summarize_window_aggregates`)**: adicionar fixtures pequenas e goldens específicos para:
   - histogramas sobre séries escalares (`histogram_scalar_series`) com bucketização explícita;
   - top-k de padrões sobre séries de vetores `[5]` (`topk_patterns_count_vector5_series`);
   - matriz cheia por posição×valor (`histogram_count_vector5_series_per_position_matrix`) com limites explícitos.

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
| `summarize_window_aggregates` | Validação de `aggregate_type`, bucket spec/matriz, compatibilidade de `shape` e ordenação canônica. |
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

## Fase 11 — Evidências do recorte V1 entregue (MCP + tools implementadas)

Este fechamento cobre o recorte V1 efetivamente implementado no repositório: MCP `stdio` e MCP HTTP (`/mcp`, streamable) com paridade semântica contra HTTP REST para as tools já materializadas.

### Escopo entregue e transportes

| Tool do contrato | HTTP `/tools/*` | HTTP `/mcp/tools/*` (REST **deprecado**) | MCP `stdio` | MCP HTTP `/mcp` | Status no recorte |
|------------------|------------------|----------------------|-------------|------------------|-------------------|
| `get_draw_window` | Sim | Sim (deprecado) | Sim | Sim | Entregue (onda A) |
| `compute_window_metrics` | Sim | Sim (deprecado) | Sim | Sim | Entregue (onda A) |
| `analyze_indicator_stability` | Sim | Sim (deprecado) | Sim | Sim | Entregue (onda B, recorte inicial) |
| `compose_indicator_analysis` | Não | Não | Não | Não | Fora do recorte fechado nesta fase |
| `analyze_indicator_associations` | Não | Não | Não | Não | Fora do recorte fechado nesta fase |
| `summarize_window_patterns` | Não | Não | Não | Não | Fora do recorte fechado nesta fase |
| `generate_candidate_games` | Não | Não | Não | Não | Fora do recorte fechado nesta fase |
| `explain_candidate_games` | Não | Não | Não | Não | Fora do recorte fechado nesta fase |

Justificativa explícita para as tools não implementadas: permanecem planejadas para as próximas fatias da Fase 10 no [spec-driven-execution-guide.md](spec-driven-execution-guide.md), sem declaração de V1 completa.

### Suites de evidência para CI (gate do recorte)

Executar estas suites como evidência mínima de fechamento do recorte V1:

1. `dotnet test tests/LotofacilMcp.Domain.Tests/LotofacilMcp.Domain.Tests.csproj`
2. `dotnet test tests/LotofacilMcp.Infrastructure.Tests/LotofacilMcp.Infrastructure.Tests.csproj`
3. `dotnet test tests/LotofacilMcp.ContractTests/LotofacilMcp.ContractTests.csproj`

Rastreabilidade principal:

- Domínio e invariantes base: `tests/LotofacilMcp.Domain.Tests/`.
- Casos de uso e validação cross-field: `tests/LotofacilMcp.Infrastructure.Tests/`.
- Contrato e paridade MCP/HTTP (`tools/list` + `tools/call`) para stdio e `/mcp`: `tests/LotofacilMcp.ContractTests/McpTransportParityIntegrationTests.cs`.

## GAPS de contrato, coerência cruzada e interação pares–entropia (ADR 0006)

Bateria normativa a acrescentar **quando a implementação** acompanhar o [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) e o contrato atualizados na mesma linha.

| Cenário | Objetivo | Referência de payload / fixture |
|---------|----------|---------------------------------|
| **A — Erro congelado `UNKNOWN_METRIC`** | `compute_window_metrics` com métrica canónica ainda fechada na rota; assert `code`, `details.metric_name` e, se existir, `allowed_metrics`. | [mcp-tool-contract.md](mcp-tool-contract.md) secção *Disponibilidade em `compute_window_metrics`*. |
| **B — Coerência explain / compute** | Se `explain_candidate_games` ou geração referem uma métrica, e `compute_window_metrics` a rejeita na mesma build, teste de regressão documenta o comportamento até a promoção. | [metric-catalog.md](metric-catalog.md) *Disponibilidade normativa*; [generation-strategies.md](generation-strategies.md). |
| **C — `min_history` > janela** | `analyze_indicator_stability` com `min_history` maior que `window_size` resolvido; esperado `INSUFFICIENT_HISTORY` com `details` numéricos. | [mcp-tool-contract.md](mcp-tool-contract.md) `analyze_indicator_stability`. |
| **D — `stability_check` sem suporte** | Request a `analyze_indicator_associations` **com** `stability_check` em build **sem** implementação; esperado `UNSUPPORTED_STABILITY_CHECK` (e não sucesso vazio). | ADR 0006 D2. |
| **E — Pares × entropia de linha (Spearman)** | Mesma `window_size` e `end_contest_id` em `window_long_real.json` (ou `synthetic_min_window.json` equivalente); `items` = `pares_no_concurso` + `entropia_linha_por_concurso`; `method: spearman`; assert magnitude reprodutível. | [test-plan.md](test-plan.md) § Cenário canónico; ADR 0006 D5. |

A ordem de gravação dos goldens e a fixture escolhida seguem a mesma regra de *Atualização de golden* (secção *Fixtures douradas* acima).

### Checklist ADR 0005 para este recorte

| Critério do ADR 0005 | Evidência no recorte | Situação |
|----------------------|----------------------|----------|
| 1. Transporte MCP com testes de `tools/list` e `tools/call` para tools em escopo | `McpTransportParityIntegrationTests` valida descoberta e chamadas para as 3 tools entregues | Atendido |
| 2. Paridade semântica MCP vs HTTP em sucesso/erro de contrato | `McpTransportParityIntegrationTests` usa `JsonElement.DeepEquals` para os dois caminhos | Atendido |
| 3. Ondas B/C seguem execução spec-driven por tool | `analyze_indicator_stability` entrou com testes; demais tools de B/C permanecem fora deste fechamento | Parcial por escopo, sem declarar V1 completa |
