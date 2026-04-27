# Melhorias na superfície MCP (tools) — plano spec-driven

Data: 2026-04-27  
Status: rascunho (a validar e executar por fases)  
Alvo: MCP STDIO (`user-lotofacil-ia`) — qualidade de contrato, DX e previsibilidade

## Contexto e objetivo

Durante o consumo do MCP via STDIO, a invocação funcionou, mas foram observados pontos de melhoria **de contrato** (schema vs runtime), **descoberta** (o que existe / o que é permitido), e **ergonomia** (payloads grandes sem knobs).

Objetivo: alinhar a superfície de tools com a estratégia **spec-driven** do repositório:

- contrato e semântica documentados antes do código
- comportamento determinístico e rastreável preservado
- testes de contrato cobrindo os casos e prevenindo regressões

## Escopo (o que muda / o que não muda)

- **Muda**: formato/validações de requests, adição de tools auxiliares de descoberta, opções de redução de payload.
- **Não muda**: determinismo, `dataset_version`, `tool_version`, `deterministic_hash`, e as métricas já existentes (salvo quando explicitamente versionadas).

## Problemas observados (do consumo real)

### P1 — Divergência: schema indica opcional, runtime exige obrigatório

**Sinal**: o descritor JSON de `compute_window_metrics` define `metrics` com `default: null`, mas o servidor retornou erro `metrics is required`.

**Impacto**:

- cliente precisa de tentativa/erro para aprender o contrato real
- quebra automação (geradores a partir do schema, validação em SDK, etc.)

**Proposta (escolher uma e especificar)**:

- **Opção A (recomendada)**: `metrics` **opcional**. Quando ausente, calcular **todas as métricas permitidas** para `compute_window_metrics` nesta build.
- **Opção B**: `metrics` **obrigatório**. Atualizar o schema para marcar como required e ajustar mensagens/ajuda.

**Critérios de aceitação**:

- o schema e o comportamento runtime estão alinhados
- testes de contrato cobrem request com `metrics` ausente e/ou presente conforme opção escolhida
- erro estruturado e consistente quando inválido (`code`, `message`, `details`)

**Testes sugeridos**:

- Contract test: `compute_window_metrics` sem `metrics`
  - Opção A: retorna `metrics[]` não-vazio e determinístico
  - Opção B: retorna erro determinístico com `missing_field=metrics`

---

### P2 — Descoberta: falta uma tool pequena para listar métricas disponíveis

**Sinal**: para responder “quais métricas estão disponíveis”, hoje é necessário interpretar `discover_capabilities` (payload grande) ou conhecer a lista previamente.

**Impacto**:

- integração mais difícil para clientes simples
- aumenta custo cognitivo para automação e validação

**Proposta**: nova tool `list_metrics` (ou `get_metric_catalog`).

**Especificação mínima**:

- Request: vazio ou com filtros opcionais (ex.: `tool_name`, `status`, `scope`, `shape`).
- Response: lista de entradas com:
  - `metric_name`
  - `version`
  - `scope`
  - `shape`
  - `unit`
  - `status` (`implemented|pending|deprecated`)
  - `available_in_tools` (ex.: `compute_window_metrics`, `summarize_window_aggregates`, etc.)

**Critérios de aceitação**:

- resposta determinística e estável
- não exige executar cálculo de métricas
- cobre 100% do catálogo desta build (sem divergências com `discover_capabilities`)

**Testes sugeridos**:

- Contract test: `list_metrics` retorna lista não-vazia
- Invariante: todo `metric_name` retornado em `compute_window_metrics_allowed` aparece com `available_in_tools` incluindo `compute_window_metrics`

---

### P3 — Janela implícita ao informar apenas `window_size`

**Sinal**: chamadas com somente `window_size` funcionam “ancoradas” no último concurso disponível, mas isso é implícito.

**Impacto**:

- consumidores não sabem se “último concurso” é regra normativa ou coincidência da build
- dificulta reproduzir resultados sem explicitar âncora

**Proposta**:

- Tornar explícito no contrato:
  - **Regra**: se `end_contest_id` é omitido, o sistema usa `end_contest_id = latest_contest_id` do dataset carregado.
  - ou exigir `end_contest_id` para todas as tools de janela (mais estrito).
- Opcional: adicionar parâmetro `anchor` com valores `latest|contest_id`.

**Critérios de aceitação**:

- request e response sempre incluem `window.start_contest_id/end_contest_id` (já ocorre hoje)
- documentação define a regra de resolução de janela sem ambiguidades

**Testes sugeridos**:

- Contract test: `get_draw_window(window_size=N)` resolve `end_contest_id` determinístico para o dataset fixture

---

### P4 — Payload grande sem “knobs” (séries/matrizes)

**Sinal**: algumas métricas retornam arrays longos (`series_of_count_vector[5]`, `count_matrix[25x15]`, etc.).

**Impacto**:

- custo/latência maiores
- dificulta uso em clientes com limites de tamanho

**Proposta**: adicionar parâmetros opcionais de formatação (sem mudar semântica).

**Exemplos (especificar um subset mínimo)**:

- `include_explanations: boolean` (default `true`)
- `response_format: "full" | "summary"` (default `full`)
- `value_encoding: "flat" | "rle"` (apenas para shapes matriciais/listas grandes)
- `max_series_points` (para tools que retornam séries; quando menor que a janela, aplicar recorte determinístico: últimos \(k\) pontos)

**Critérios de aceitação**:

- defaults preservam o comportamento atual
- formatos alternativos são determinísticos e testados

**Testes sugeridos**:

- Contract test: mesma request com `response_format=summary` produz payload menor e inclui metadados mínimos
- Invariante: `deterministic_hash` muda quando formato muda (se o hash refletir payload) **ou** é definido como hash do resultado semântico (definir em spec)

---

### P5 — `discover_capabilities` grande (mistura “índices” e detalhes)

**Sinal**: a tool é útil, mas serve a múltiplos propósitos numa única resposta.

**Impacto**:

- clientes que querem só “uma parte” pagam pelo payload completo

**Proposta**:

- Adicionar parâmetro `fields` (projeção), ex.: `["tools","metrics.compute_window_metrics_allowed"]`
  - ou quebrar em tools menores (`get_tool_index`, `get_build_info`, `get_metric_index`)

**Critérios de aceitação**:

- consumidores conseguem obter apenas o necessário com baixo custo
- conteúdo completo ainda permanece disponível

## Plano de execução por fases (spec-driven)

### Fase 1 — Alinhamento schema vs runtime (`compute_window_metrics.metrics`)

- Definir Opção A ou B e atualizar spec/descritor.
- Implementar comportamento.
- Adicionar/ajustar testes de contrato.

### Fase 2 — `list_metrics` (descoberta mínima)

- Especificar request/response e invariantes com `discover_capabilities`.
- Implementar e testar.

### Fase 3 — Controles de payload (formatos)

- Definir conjunto mínimo de parâmetros e efeito determinístico.
- Implementar e testar com fixtures focadas.

## Checklist de validação pós-implementação

- `discover_capabilities` e `list_metrics` coerentes (sem divergências)
- `compute_window_metrics` aceita o fluxo especificado (sem tentativa/erro)
- respostas continuam determinísticas (hash, versões, janela resolvida)
- testes de contrato passam em CI

