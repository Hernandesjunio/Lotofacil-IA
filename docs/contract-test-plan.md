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
   - validação do enum fechado de `aggregate_type` e rejeição de tipos fora do catálogo;
   - validação de parâmetros obrigatórios por tipo, sem defaults semânticos ocultos;
   - validação de ordenação canônica e desempates determinísticos por tipo.

**Extensão planejada (ADR 0008):**

6. **Fase B.2 — Janela por extremos, mapeamento `top10` e coerência com legado de export** (ver [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) e a [Fase 23 do guia](spec-driven-execution-guide.md)). **Não** usar [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) como fonte de descoberta (D1) ou de mapeamento Top 10 — 0007 cobre `summarize_window_aggregates` (Fase B.1).
   - **Janela D2:** equivalência numérica entre o recorte com `start_contest_id` e `end_contest_id` (inclusivos) e a forma `window_size` + `end_contest_id` na **mesma fixture** densa, para pelo menos `frequencia_por_dezena@1.0.0` e `top10_mais_sorteados@1.0.0` **quando** o protocolo aceitar ambas as formas; caso contrário, documentar a forma única suportada e ainda testar D2 via `Window` materializado (extremos e tamanho no *output*).
   - **Ambiguidade / conflito:** request com `window_size` e `start`/`end` **incompatíveis** entre si, ou `start_contest_id > end_contest_id`, ou outra combinação não reduzível a um único recorte → `INVALID_REQUEST` (ou código fechado no [mcp-tool-contract.md](mcp-tool-contract.md)) **sem** aplicação de janela por *tie-break* oculto.
   - **D4 (sem N mágico de UI legada):** teste negativo explícito — o servidor **não** deriva `window_size` a partir de constantes de ecrã antigo (“últimos 10/20”); o cliente deve fornecer `window_size`+`end_contest_id` (ou par D2). Ausência de campos de janela continua a caminhar com `INVALID_REQUEST` / esclarecimento no host, não com default semântico no servidor.
   - **D3 / `top10_mais_sorteados`:** golden versionado (Tabela 2 de [metric-catalog.md](metric-catalog.md)), desempate dezena asc; fixture `tie_heavy.json` (ou equivalente) para stress de empates.
   - **Export `QtdFrequencia` → métrica canónica:** para uma janela e fixture fixas, o vector 1..25 alinha-se a `frequencia_por_dezena@1.0.0` (ver *Rótulo de export* no catálogo), não a `atraso_por_dezena` — asserção reprodutível (subconjunto estável do JSON ou golden dedicado).
   - **D1 descoberta (sem nome de tool fechado):** `compute_window_metrics` com `metrics` contendo nome **presente** no [metric-catalog.md](metric-catalog.md) mas **fora** da allowlist da build → `UNKNOWN_METRIC` com `details.metric_name` e, se existir, `details.allowed_metrics` (conjunto fechado), alinhado a [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md). Não exigir implementação de tool com identificador específico de “listar superfície” — só a semântica de prova de allowlist.

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
| `generate_candidate_games` | `generation_mode`, `replay_guaranteed`, política de `deterministic_hash` *vs.* `seed`, interseção de restrições, opt-in de `structural_exclusions`/critérios, orçamento (soma **≤ 1000**), e restrições flexíveis por `range`/`allowed_values`/`typical_range` (ADR 0019) quando o recorte as incluir. |
| `explain_candidate_games` | Breakdown e métricas declaradas. |

### Matriz mínima — Fase B.1 (`summarize_window_aggregates`)

| Caso | Esperado |
|------|----------|
| `aggregate_type` fora do enum fechado | Erro `UNSUPPORTED_AGGREGATE_TYPE`. |
| `histogram_scalar_series` com `bucket_values` explícitos | Sucesso com `buckets[]` ordenados por `x` asc e contagens determinísticas. |
| `histogram_scalar_series` com `bucket_spec` ausente/incompleto ou modo misto | Erro `INVALID_REQUEST` (sem inferência de bucketização). |
| `topk_patterns_count_vector5_series` com `top_k` válido | Sucesso com `items[]` ordenados por `count desc` e desempate lexicográfico de `pattern` asc. |
| `topk_patterns_count_vector5_series` sem `top_k` ou `top_k < 1` | Erro `INVALID_REQUEST`. |
| `histogram_count_vector5_series_per_position_matrix` com `value_min`/`value_max` válidos | Sucesso com `matrix[5][K]` cheia, linhas por posição asc e colunas por valor asc. |
| `histogram_count_vector5_series_per_position_matrix` sem limites ou com `value_min > value_max` | Erro `INVALID_REQUEST`. |
| `aggregate_type` incompatível com `shape`/`scope` da fonte | Erro `UNSUPPORTED_SHAPE`. |
| Request com múltiplos agregados | Resposta preserva a ordem de `aggregates[]` do request. |

**Observação de contrato:** os testes da Fase B.1 devem sempre explicitar bucketização (`bucket_values` ou `min/max/width`) e dimensões de matriz (`value_min/value_max`) no request; ausência não pode ser compensada por default semântico no servidor.

### Matriz mínima — Fase B.2 (janela por extremos, `top10` e legado de export, ADR 0008)

Bateria a acrescentar **quando a implementação** acompanhar o [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) e o [mcp-tool-contract.md](mcp-tool-contract.md) (entidade `Window`, *Prompts e Resources*). A matriz **não** conflitua com [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) (allowlist); descoberta *instância* vs. *norma* reforçada no contrato, secção *Primitivas MCP opcionais* e ADR 0008 D1.

| Caso | O que validar (mínimo reprodutível) | Referência cruzada |
|------|-----------------------------------|--------------------|
| D2 — Par equivalente | Dois recortes equivalentes: (a) `start_contest_id`+`end_contest_id` inclusivos; (b) `window_size`+`end_contest_id` com `window_size = end - start + 1` — *ou* apenas uma forma suportada, com extremos coerentes no `Window` de saída. **Esperado:** mesmos `frequencia_por_dezena@1.0.0` e `top10_mais_sorteados@1.0.0` (ou nota de escopo se a segunda ainda não estiver na rota). | [mcp-tool-contract.md](mcp-tool-contract.md) *Window*; [ADR 0008 D2](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) |
| Ambiguidade de janela | `window_size` incompatível com `start`/`end` *juntos*; `start_contest_id > end_contest_id`; combinação que não reduz a um intervalo contíguo único. **Esperado:** `INVALID_REQUEST` (ou código fechado no contrato); **nunca** recorte silencioso. | *Window* e parâmetros comuns; ADR 0008 D2 |
| D4 — Sem “últimos N” do legado | Pedido sem janela completa ou intenção de replicar *default* de ecrã antigo (“últimos 10/20”). **Esperado:** `INVALID_REQUEST` / esclarecimento no host; **não** `window_size` derivado de N fixo de UI. | [ADR 0008 D4](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) |
| `top10_mais_sorteados@1.0.0` + empates | Fixture `tie_heavy.json` (ou equivalente) com muitos empates em frequência. **Esperado:** `dezena_list[10]`, desempate dezena asc; golden versionado. | [metric-catalog.md](metric-catalog.md) Tabela 2 |
| D3 — `HistoricoTop10MaisSorteados` *vs.* canónico | Intenção de substituição de export legado. **Esperado:** só `top10_mais_sorteados` na janela **declarada**; não tratar *rolling* implícito como esta métrica. | [ADR 0008 D3](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) |
| `QtdFrequencia` (export) | Vector 1..25 = contagens na janela. **Esperado:** alinha a `frequencia_por_dezena@1.0.0` na mesma janela/fixture; distinto de `atraso_por_dezena`. | *Rótulo* `QtdFrequencia` no [metric-catalog.md](metric-catalog.md); ADR 0008 Anexo A |
| D1 — Allowlist (sem nome de tool) | `compute_window_metrics` com nome **no catálogo** e **fora** da allowlist da build. **Esperado:** `UNKNOWN_METRIC` + `details.metric_name` e, se existir, `details.allowed_metrics` fechado; **não** exigir tool concreta de “listar superfície”. | [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md); [ADR 0008 D1](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) |
| Transparência de janela | Resposta de métrica ancorada em janela. **Esperado:** `MetricValue.window` (ou equivalente) com `start`/`end`/`size` coerentes com o pedido resolvido. | [mcp-tool-contract.md](mcp-tool-contract.md) invariante 2; entidade `Window` |

**Observação de contrato:** a Fase B.2 **não** substitui a bateria A–E (GAPS) do plano/ADR 0006; reutiliza D1 de 0006 quando o caso for *allowlist*. A Fase B.1 (`summarize_window_aggregates`, ADR 0007) continua **ortogonal**: agregados canónicos ≠ descoberta nem mapeamento Top 10.

### Matriz mínima — Extensão de geração com ranges (ADR 0019)

Adicionar esta bateria quando o recorte de `generate_candidate_games` incluir `range`/`allowed_values`/`typical_range` e/ou orçamento explícito.

| Caso | Esperado |
|------|----------|
| Critério com modos mistos (`value` + `range`, ou `range` + `allowed_values`) | Erro `INVALID_REQUEST` (sem inferência). |
| `allowed_values.values` vazio | Erro `INVALID_REQUEST`. |
| `range.min > range.max` | Erro `INVALID_REQUEST`. |
| `mode` inválido (quando presente) | Erro `INVALID_REQUEST` (ou código fechado no contrato). |
| `typical_range` com `method=percentile` sem `p_low/p_high` | Erro `INVALID_REQUEST`. |
| `typical_range.metric_name` desconhecida | Erro `UNKNOWN_METRIC`. |
| `typical_range` válido | Sucesso e eco de `resolved_range` + `coverage_observed` em `applied_configuration.resolved_defaults` (semântica auditável). |
| `count` alto com orçamento pequeno | `STRUCTURAL_EXCLUSION_CONFLICT` com `available_count` e pistas estáveis em `details` (quando aplicável). |
| Determinismo (replay) | Com `seed` e `replay_guaranteed: true`: mesmo request + dataset ⇒ mesmo `deterministic_hash` e mesma lista de candidatos (ordem canônica). Com `replay_guaranteed: false`: mesmo hash **sem** exigir mesma lista; ver [mcp-tool-contract.md](mcp-tool-contract.md) (*Canonização*). |

### Matriz mínima — ADR 0020 (modos, opt-in, interseção, `replay_guaranteed`)

Acrescentar na **próxima fatia** que materializar [ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md) no servidor/validator:

| Caso | Esperado |
|------|----------|
| `generation_mode: random_unrestricted` sem `structural_exclusions` | Sucesso **sem** aplicar exclusões estruturais não declaradas; `applied_configuration.resolved_defaults` não inventa guardrails. |
| `generation_mode: behavior_filtered` com dois critérios *hard* | Candidatos satisfazem **ambos** (interseção); falha com `STRUCTURAL_EXCLUSION_CONFLICT` / `available_count` quando o conjunto admissível for vazio. |
| `seed` ausente + trajectória estocástica | `replay_guaranteed: false`; duas chamadas podem divergir nas dezenas; `deterministic_hash` estável se o request não aleatório for idêntico. |
| `seed` presente + mesmo request | `replay_guaranteed: true` (quando aplicável); lista e hash conforme contrato. |
| Soma `plan[].count` > 1000 | `INVALID_REQUEST` (ou código dedicado) com `details` útil. |
| Omissão de `generation_mode` | `INVALID_REQUEST` (`missing_field` ou equivalente). |

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

### Evidência específica — Fase B.1 (`summarize_window_aggregates`)

Este recorte fecha apenas a validação final de paridade e evidências da tool de agregados, sem ampliar `aggregate_type` nem semântica:

- Paridade MCP/HTTP em sucesso e erro para `summarize_window_aggregates` em `tests/LotofacilMcp.ContractTests/McpTransportParityIntegrationTests.cs`.
- Determinismo (`deterministic_hash`) para request repetido da mesma tool na suíte de paridade e nos testes de contrato da fase.
- Golden auditável em `tests/fixtures/golden/phase22/summarize-window-aggregates.canonical-small-window.golden.json`, ancorado na fixture `tests/fixtures/aggregates_canonical_small_window.json`.

### Checklist ADR 0005 para este recorte

| Critério do ADR 0005 | Evidência no recorte | Situação |
|----------------------|----------------------|----------|
| 1. Transporte MCP com testes de `tools/list` e `tools/call` para tools em escopo | `McpTransportParityIntegrationTests` valida descoberta e chamadas para as 3 tools entregues | Atendido |
| 2. Paridade semântica MCP vs HTTP em sucesso/erro de contrato | `McpTransportParityIntegrationTests` usa `JsonElement.DeepEquals` para os dois caminhos | Atendido |
| 3. Ondas B/C seguem execução spec-driven por tool | `analyze_indicator_stability` entrou com testes; demais tools de B/C permanecem fora deste fechamento | Parcial por escopo, sem declarar V1 completa |
