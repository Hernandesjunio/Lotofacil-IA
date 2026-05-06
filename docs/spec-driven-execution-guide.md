# Guia de Execução Spec-Driven — ADRs 0023–0025 (eficiência, distribuição, deploy)

**Navegação:** [← Brief (índice)](brief.md) · [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md) · [ADR 0024](adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md) · [ADR 0025](adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md) · **Guia completo (arquivado):** `docs/archived/spec-driven-execution-guide.md`

Este arquivo foi **reduzido** para ganhar eficiência no consumo de tokens e focar no recorte das ADRs 0023–0025. As fases antigas (V0/V1 e ADRs anteriores) permanecem no guia arquivado.

## Objetivo

Materializar:

- a [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md) (eficiência / knobs / canais),
- a [ADR 0024](adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md) (distribuição ZIP self-contained para MCP STDIO sem repo),
- a [ADR 0025](adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md) (deploy MCP HTTP via Docker/IIS/cloud),

sem quebrar invariantes do contrato e do dataset.

- reduzir tokens (evitar duplicação de JSON no canal textual),
- permitir controle explícito de detalhe (`verbosity`, `include_explanations`),
- permitir **projeção** (`fields`) e paginação determinística quando necessário,
- tornar knobs descobríveis via `help` e `discover_capabilities`,
- manter determinismo e regra clara para `deterministic_hash` quando a apresentação variar.

## Referências normativas mínimas

- [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)
- [ADR 0024](adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md)
- [ADR 0025](adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [ADR 0005 (transportes)](adrs/0005-transporte-mcp-e-superficie-tools-v1.md)
- (quando aplicável) [ADR 0022 (dataset)](adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md)
- (quando aplicável) [ADR 0008 (descoberta)](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md), [ADR 0009 (help/resources)](adrs/0009-help-e-catalogo-de-templates-resources-v1.md)

---

## Fases (ADR 0023)

### Fase 23.1 — Fechar contrato dos knobs de economia

**Objetivo:** padronizar parâmetros transversais onde aplicável.

- Definir `verbosity`: `minimal | standard | full`.
- Definir `include_explanations` onde aplicável.
- Definir `fields` (ou `response_projection`) para projeção server-side quando a resposta for grande.
- Fechar a regra de `deterministic_hash` quando `verbosity/include_explanations/fields` alterarem a apresentação.

**Pronto quando:** o contrato documenta knobs + hashing sem ambiguidade.

### Fase 23.2 — Separar canais (StructuredContent vs Content) sem duplicar JSON

**Objetivo:** `StructuredContent` é a fonte canônica; `Content` é resumo humano curto e útil.

- Garantir que `Content` não repita o JSON completo do `StructuredContent` por padrão.
- Mapear `verbosity` → tamanho/estilo do resumo humano em `Content`.

**Subetapas Hotfix:**

1. **Hotfix 23.2.1 — Fechar utilidade mínima do `Content`:** definir que eficiência não permite respostas genéricas sem o fato principal.
2. **Hotfix 23.2.2 — Definir comportamento chat-safe de `standard`:** exigir que `standard` responda consultas humanas comuns sem inspeção manual do `StructuredContent`.
3. **Hotfix 23.2.3 — Diferenciar classes de tool:** tools factuais devem expor fatos principais; tools analíticas devem expor nomes e resultados salientes.
4. **Hotfix 23.2.4 — Conter efeitos colaterais:** garantir que o hotfix não introduza inferência implícita de intenção, nem aumente `Content` para dump disfarçado em janelas grandes.
5. **Hotfix 23.2.5 — Anti-esvaziamento em multi-métrica (`compute_window_metrics`):** garantir que `verbosity="standard"` preserve um resumo mínimo no `Content` mesmo quando `metrics[]` contém 2+ itens (incluindo `include_explanations=false`), evitando respostas “administrativas”/vazias ou dependência de inspeção manual do `StructuredContent` para o caso comum de chat.

**Pronto quando:** em `minimal`, `Content` é curto e não contém JSON completo; em `standard`, continua útil por si só para chat.

### Fase 23.2.5 — Fix de utilidade do `Content` em `compute_window_metrics` (multi-métrica + `standard`)

**Objetivo:** eliminar o caso em que o `Content` deixa de ser útil em `verbosity="standard"` quando `compute_window_metrics` é chamado com 2+ métricas.

- O comportamento deve ser **determinístico** e **chat-safe**:
  - `verbosity="standard"`: `Content` menciona a janela e lista as métricas pedidas com um “highlight” determinístico por métrica.
  - `include_explanations=false` remove apenas `explanation`, não o resumo mínimo em `Content`.
- O ajuste **não** autoriza duplicar JSON completo no `Content`; permanece válido o princípio da Fase 23.2.
- Critério de evidência: adicionar teste(s) de contrato que travem o comportamento.

**Referências obrigatórias:**

- [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- `docs/issues/issue-mcp-compute-window-metrics-standard-multi-metric-jsonelement.md`

**Pronto quando:** para `compute_window_metrics`, `verbosity="standard"` com 2+ métricas não resulta em `Content` vazio/genérico; e regressões quebram testes.

### Fase 23.3 — Projeção (`fields`) e explicações opt-in

**Objetivo:** reduzir payload na fonte.

- Definir por tool quais campos suportam projeção e como validar `fields`.
- Definir por tool quando `include_explanations` é aceito e o que muda.

**Pronto quando:** projeção é determinística e validada (erro claro para campos inválidos).

### Fase 23.4 — Paginação determinística (quando aplicável)

**Objetivo:** controlar respostas grandes em `full` de forma reprodutível.

- Definir esquema de paginação determinística (ex.: `page/page_size` ou `cursor`).
- Fixar ordenação canônica e estabilidade do cursor (quando existir).

**Pronto quando:** mesmas entradas + mesmos knobs retornam o mesmo subconjunto paginado.

### Fase 23.5 — Descoberta/UX: `help` e `discover_capabilities`

**Objetivo:** reduzir tentativa/erro e tornar knobs visíveis.

- `help` explica “modo econômico” vs detalhado (mapeando para knobs).
- `discover_capabilities` declara suporte por tool (knobs, enums aceitos, defaults recomendados) sem payload excessivo.

**Subetapas Hotfix:**

1. **Hotfix 23.5.1 — Tornar `standard` explícito como default de chat:** discovery deve dizer que `standard` é recomendado para uso humano interativo.
2. **Hotfix 23.5.2 — Declarar expectativa de utilidade:** `help` e `discover_capabilities` devem deixar claro que o resultado principal não será omitido no canal `Content`.
3. **Hotfix 23.5.3 — Reduzir ambiguidade operacional:** exemplos devem mostrar quando usar `minimal` para economia e quando usar `standard/full` para obter resposta humana suficiente.
4. **Hotfix 23.5.4 — Declarar limites do hotfix:** discovery deve deixar claro que utilidade adicional no `Content` não substitui `fields`/paginação em respostas extensas.
5. **Hotfix 23.5.5 — Fechar quickstart do `help`:** `help` deve orientar explicitamente como obter o último concurso com `get_draw_window(window_size=1)`, como começar com `compute_window_metrics` e quais campos mínimos de rastreabilidade preservar.
6. **Hotfix 23.5.6 — Fechar constraints operacionais em `discover_capabilities`:** para tools de janela, discovery deve declarar constraints suficientes para montar o request sem tentativa/erro evitável (ex.: `window_size > 0`, `start_contest_id` exige `end_contest_id`, coerência entre `window_size` e `start/end`).

**Pronto quando:** um leigo consegue pedir `minimal/full` com poucas tentativas e entende que `standard` preserva o resultado principal.

### Fase 23.6 — Evidências e testes (contrato + determinismo + custo)

**Objetivo:** travar eficiência, utilidade mínima e determinismo por testes.

- Testes cobrindo ao menos `minimal` e `full` nas tools alvo.
- Testes garantindo: `Content` não duplica JSON; `StructuredContent` permanece canônico.
- Testes cobrindo a regra de `deterministic_hash` definida na Fase 23.1.

**Subetapas Hotfix:**

1. **Hotfix 23.6.1 — Testes de utilidade factual:** tools como `get_draw_window` devem provar que `Content` expõe concurso final e dados principais do recorte curto.
2. **Hotfix 23.6.2 — Testes de utilidade analítica:** tools de métricas/ranking devem provar que `Content` expõe pelo menos nomes e resultados salientes.
3. **Hotfix 23.6.3 — Testes anti-esvaziamento:** frases genéricas do tipo "see structured payload" não bastam como aceite quando omitirem o resultado principal.
4. **Hotfix 23.6.4 — Massa estática e revisável:** todos os cenários do hotfix devem usar fixtures/goldens versionados no repositório; sem dataset vivo, sem “último concurso” dinâmico e sem massa sintetizada em tempo de execução pela IA.
5. **Hotfix 23.6.5 — Insumo explícito por cenário:** cada teste do hotfix deve declarar request de entrada, fixture usada e saída esperada suficiente para revisão humana antes da execução.
6. **Hotfix 23.6.6 — Anti-fragilidade textual:** para `Content`, preferir asserções de presença/ausência de fatos obrigatórios e anti-padrões; usar golden textual literal apenas quando o wording fizer parte da decisão semântica.
7. **Hotfix 23.6.7 — Meta-tools com onboarding verificável:** `help` deve provar por teste que não é só índice administrativo; o `Content` precisa conter o quickstart mínimo auditável.
8. **Hotfix 23.6.8 — Discovery com constraints verificáveis:** `discover_capabilities` deve provar por teste que expõe constraints operacionais relevantes de janela, não apenas nomes de parâmetros.

**Pronto quando:** regressões de duplicação, utilidade mínima ou knobs/hashing quebram testes.

**Massa mínima recomendada para os testes do hotfix:**

1. `tests/fixtures/synthetic_min_window.json` para:
   - `get_draw_window` com `window_size=1` e `window_size=3`;
   - `compute_window_metrics` com 1 métrica escalar ou lista curta.
2. `tests/fixtures/tie_heavy.json` para:
   - rankings/listas com empates e escolha determinística do subconjunto saliente no `Content`.
3. `tests/fixtures/golden/phase23/` para:
   - goldens de payload estruturado e snapshots pequenos de cenários específicos do hotfix.

**Matriz mínima de cenários do hotfix (para revisão antes da execução):**

1. `HF23-M01` — `get_draw_window`, factual curto.
   - Fixture: `tests/fixtures/synthetic_min_window.json`
   - Request: `{ "window_size": 1, "end_contest_id": 3, "verbosity": "standard" }`
   - Structured esperado: concurso `3`, data `2003-10-13`, dezenas `[1,4,6,7,8,9,10,11,12,14,16,17,20,23,24]`
   - Content esperado: concurso, data e 15 dezenas; proibido responder só com contagem de janela ou remeter ao payload estruturado.
2. `HF23-M02` — `get_draw_window`, janela curta auditável.
   - Fixture: `tests/fixtures/synthetic_min_window.json`
   - Request: `{ "window_size": 3, "end_contest_id": 3, "verbosity": "standard" }`
   - Structured esperado: `window = 1..3`, `draws.length = 3`
   - Content esperado: deve mencionar a janela curta e expor os dados principais do concurso final.
3. `HF23-M03` — `compute_window_metrics`, lista curta determinística.
   - Fixture: `tests/fixtures/tie_heavy.json`
   - Request: `{ "window_size": 5, "end_contest_id": 5005, "metrics": [{ "name": "top10_mais_sorteados" }], "verbosity": "standard" }`
   - Structured esperado: top 10 `[11,12,13,14,15,16,17,18,19,20]`, alinhado ao golden já existente de `phaseB2`
   - Content esperado: nome da métrica + top 10; proibido `"Computed 1 metric"` sem o resultado.
4. `HF23-M04` — `summarize_window_patterns`, resumo estatístico verificável.
   - Fixture: `tests/fixtures/synthetic_min_window.json`
   - Request: `{ "window_size": 5, "end_contest_id": 1005, "features": [{ "name": "pares_no_concurso" }], "coverage_threshold": 0.8, "range_method": "iqr", "verbosity": "standard" }`
   - Structured esperado: `mode = 8`, `median = 8`, `iqr = 1`, `coverage_observed = 0.6`
   - Content esperado: feature + resultados salientes; proibido ocultar todos os números relevantes.
5. `HF23-M05` — `summarize_window_aggregates`, agregados pequenos.
   - Fixture/golden: cenário pequeno já coberto em `phase22`
   - Structured esperado: IDs `z_hist_pairs`, `a_topk_rows`, `m_matrix_rows`
   - Content esperado: ao menos os agregados salientes ou resumo factual equivalente; proibido responder apenas a quantidade.
6. `HF23-M06` — anti-esvaziamento em `full`.
   - Reaproveita `HF23-M01`, `HF23-M03` e `HF23-M04` com `verbosity = "full"`
   - Content esperado: mais detalhado, mas ainda contendo o resultado principal; proibido `"See structured payload for ..."` como resposta suficiente.
7. `HF23-M07` — `help`, quickstart operacional.
   - Request: `{}`
   - Structured esperado: `getting_started_resource_uri`, `index_resource_uri`, `quick_start_markdown`, `templates[]`
   - Content esperado: deve conter `get_draw_window(window_size=1)`, uma chamada inicial de `compute_window_metrics` e os campos `dataset_version`, `tool_version`, `deterministic_hash` e `window`.
8. `HF23-M08` — `discover_capabilities`, constraints de janela.
   - Request: `{}` ou `{ "verbosity": "standard" }`
   - Structured esperado: `tools[]` inclui `get_draw_window` com constraints operacionais explícitas
   - Content/shape esperado: não basta listar `window_size`/`start_contest_id`/`end_contest_id`; deve declarar o atalho `window_size=1`, a exigência de `end_contest_id` com `start_contest_id` e a coerência entre `window_size` e `start/end`.

**Artefatos recomendados para materialização da matriz:**

1. `tests/fixtures/golden/phase23/get-draw-window.window1-end3.standard.golden.json`
2. `tests/fixtures/golden/phase23/get-draw-window.window3-end3.standard.golden.json`
3. `tests/fixtures/golden/phase23/compute-window-metrics.top10-mais-sorteados.tie-heavy.standard.golden.json`
4. `tests/fixtures/golden/phase23/summarize-window-patterns.pares-iqr.standard.golden.json`

**Gate adicional de execução:** a implementação do hotfix só deve começar depois que a matriz acima estiver revisada e aceita por humano responsável.

---

### Fase 23.7 — Descoberta sem tentativa-erro para métricas de janela

**Objetivo:** tornar `discover_capabilities` suficiente para um cliente distinguir, sem leitura de código nem trial-and-error, o que `compute_window_metrics` aceita nesta build.

- Publicar classificação **estruturada e determinística** para métricas conhecidas do catálogo, distinguindo ao menos:
  - aceite agora em `compute_window_metrics` com `allow_pending=false`;
  - aceite apenas com opt-in (`allow_pending=true`);
  - conhecida, mas não exposta nesta rota/build;
  - fora do escopo desta rota por desenho (ex.: requer candidato ou outra tool).
- Garantir que a classificação deriva do registro único (`MetricAvailabilityCatalog`) e não de listas paralelas mantidas manualmente.
- Alinhar a discovery com os erros de validação/runtime (`UNKNOWN_METRIC`, `allowed_metrics`, `reason=pending_requires_opt_in`) para evitar drift semântico entre “o que discovery diz” e “o que a tool rejeita”.
- Não introduzir nesta fase atalho implícito para “todas as métricas”; o pedido de `compute_window_metrics` continua declarativo via `metrics[]` até contrato explícito em fase própria.

**Pronto quando:** em até 2 chamadas (`discover_capabilities` + `compute_window_metrics`), um consumidor consegue montar um request válido para métricas de janela sem ler `MetricAvailabilityCatalog.cs`.

### Fase 23.8 — Qualidade e evidências da discovery/relatório de janela

**Objetivo:** travar por testes a discovery de métricas, o comportamento de `allow_pending` e a ausência de fallback implícito de dataset.

- Adicionar fixture/golden multi-métrica para request único em `compute_window_metrics`.
- Cobrir por teste:
  - classificação de discovery para métricas suportadas / pendentes / fora da rota / fora do escopo;
  - `allow_pending=false` vs `allow_pending=true` para métricas `pending`;
  - dataset ausente/inválido sem `Dataset__DrawsSourceUri`, com erro canônico/documentado e sem fallback implícito.
- Verificar que `discover_capabilities` e `compute_window_metrics` permanecem coerentes entre si para a mesma build.
- Materializar cenários de regressão com massa estática e auditável (fixtures/goldens), sem depender de dataset vivo.

**Pronto quando:** regressões de discovery, `allow_pending` ou dataset quebram testes de contrato/integracão antes de chegar ao utilizador.

---

## Fases (ADR 0024 — Distribuição ZIP self-contained para MCP STDIO)

### Fase 24.1 — Fechar o contrato operacional de distribuição (sem repo)

**Objetivo:** tornar a distribuição ZIP um caminho “primeira-classe”, sem dependência do workspace/editor.

- Fixar o modo CLI mínimo do binário distribuído: `--mcp-stdio`.
- Declarar comportamento padrão do executável (sem flags) no `README.md` para evitar ambiguidade.
- Reafirmar a regra: discovery operacional vem de `tools/list` + tools meta (`help`, `discover_capabilities`) — não de descritores externos.
- Reafirmar a regra do dataset: `Dataset__DrawsSourceUri` é obrigatório; sem fallback/fixtures.

**Pronto quando:** documentação fecha “como executar via ZIP” com parâmetros, env vars e discovery, sem depender do repo.

### Fase 24.2 — Publicação self-contained por plataforma (build/publish)

**Objetivo:** gerar um executável que rode sem SDK .NET e sem código fonte.

- Definir a matriz v1 de suporte (mínimo Windows x64; demais como planejado).
- Definir o processo de publish self-contained por plataforma (RID, configuração, output).
- Garantir que o produto publicado não dependa de paths do workspace/editor.

**Pronto quando:** existe um artefato publish self-contained executável por plataforma alvo.

### Fase 24.3 — Empacotamento ZIP v1 (conteúdo e convenções)

**Objetivo:** produzir o ZIP versionado e “instalável” por extração.

- Definir conteúdo mínimo do ZIP: executável + README curto (ou link) com:
  - configuração do host MCP (ex.: Cursor) para STDIO,
  - execução em modo HTTP (quando suportado),
  - env vars obrigatórias (dataset) e exemplos.
- Definir convenção de nome (plataforma + versão).
- Definir política de dataset no ZIP (não incluir por padrão; sample apenas opt-in).

**Pronto quando:** o ZIP é gerado seguindo convenções e sem defaults ocultos.

### Fase 24.4 — Verificação “máquina limpa” (aceite da ADR 0024)

**Objetivo:** validar o cenário real do usuário final sem repositório.

- Validar que `tools/list` funciona iniciando o servidor via executável com `--mcp-stdio`.
- Validar que `help` e `discover_capabilities` funcionam sem repo.
- Validar que o resource `lotofacil-ia://help/getting-started@1.0.0` permanece acessível sem repo quando o host suportar resources.
- Validar que, sem `Dataset__DrawsSourceUri`, tools dependentes do histórico retornam `DATASET_UNAVAILABLE` (sem fallback).

**Pronto quando:** critérios de aceite da ADR 0024 são atendidos no cenário sem repo.

---

## Fases (ADR 0025 — Deploy HTTP para MCP HTTP)

### Fase 25.1 — Fechar endpoint MCP HTTP mínimo e modo de execução

**Objetivo:** remover ambiguidade do “HTTP mode” e do endpoint MCP.

- Definir no `README.md` o endpoint MCP HTTP mínimo (ex.: `/mcp`) e como iniciar o servidor em modo HTTP.
- Garantir alinhamento com a ADR 0005: MCP HTTP é protocolo MCP real (não “REST espelhado”).
- Reforçar: `Dataset__DrawsSourceUri` é obrigatório e não há fallback/fixtures.

**Pronto quando:** documentação fecha endpoint mínimo e forma de execução HTTP sem ambiguidade.

### Fase 25.2 — Artefato para deploy HTTP: Docker (quando aplicável)

**Objetivo:** padronizar deploy em ambientes com container.

- Definir (e documentar) o build de imagem Docker para rodar o servidor em modo HTTP.
- Documentar variáveis de ambiente necessárias (dataset) e o mapeamento de portas/URL do endpoint MCP.

**Pronto quando:** existe um caminho documentado e reprodutível para executar via Docker.

### Fase 25.3 — Hosting HTTP via IIS (quando aplicável)

**Objetivo:** suportar Windows Server/IIS como reverse proxy sem desviar do protocolo MCP.

- Documentar publicação/hospedagem ASP.NET Core atrás do IIS (reverse proxy).
- Garantir que o endpoint exposto é MCP real e permanece compatível com host MCP (conexão e `tools/list`/`tools/call`).

**Pronto quando:** existe um guia mínimo de IIS e a rota MCP permanece clara e testável.

### Fase 25.4 — Cloud e restrições de streaming (serverless vs container)

**Objetivo:** evitar “deploy possível” mas operacionalmente inadequado.

- Documentar critérios: quando serverless não é adequado a conexões longas/streaming do transporte MCP HTTP, preferir container/app service.
- Documentar que `Dataset__DrawsSourceUri` deve vir de config/secret do ambiente.

**Pronto quando:** documentação deixa claro o caminho preferencial por classe de ambiente e limitações.

### Fase 25.5 — Verificação de aceite (host MCP via HTTP)

**Objetivo:** validar que o deploy HTTP atende o mínimo.

- Validar conexão de um host MCP via endpoint HTTP mínimo (definido no README) com `tools/list` e `tools/call`.
- Validar que `help` e `discover_capabilities` preservam no HTTP a mesma semântica operacional esperada no stdio.
- Validar que o onboarding versionado (`lotofacil-ia://help/getting-started@1.0.0`) continua acessível quando o host suportar resources.
- Validar que, sem `Dataset__DrawsSourceUri`, tools dependentes do histórico retornam `DATASET_UNAVAILABLE`.

**Pronto quando:** critérios de aceite da ADR 0025 são atendidos no(s) alvo(s) escolhidos (container/IIS/cloud).

