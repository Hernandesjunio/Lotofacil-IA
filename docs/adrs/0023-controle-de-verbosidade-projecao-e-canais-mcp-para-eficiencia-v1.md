# ADR 0023 — Controle de verbosidade, projeção de resposta e canais MCP (StructuredContent vs Content) para eficiência

**Navegação:** [← Brief (índice)](../brief.md) · [Contrato MCP](../mcp-tool-contract.md) · [ADR 0005 (transportes)](0005-transporte-mcp-e-superficie-tools-v1.md) · [ADR 0008 (descoberta)](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) · [ADR 0009 (help/resources)](0009-help-e-catalogo-de-templates-resources-v1.md)

## Status

Proposto.

**Data:** 2026-05-01

## Contexto

1. **Objetivo de produto (eficiência):** o MCP é consumido hoje via **Cursor chat** e também via **HTTP** (endpoints espelhados) e **stdio**. O custo relevante é:
   - tokens gerados pelo servidor nas respostas (principalmente no canal textual);
   - round-trips (tool calls) necessárias para obter um resultado útil.
2. **Duplicação de payload na resposta MCP:** o SDK MCP permite devolver payload estruturado e texto. Se o servidor repetir o JSON completo em ambos, o host que lê ambos pode pagar tokens duplicados.
3. **Escolha de detalhe deve ser do operador:** o nível de detalhe (resumo vs detalhado) deve ser escolhido por quem opera o chat (ou política do host/agente), não por defaults ocultos do servidor.
4. **Servidor é stateless por request:** o contrato proíbe que o servidor mantenha “preferências” de sessão; preferências devem ser mantidas no host/agente (ver [mcp-tool-contract.md](../mcp-tool-contract.md), invariantes; e [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md) sobre o papel do host no protocolo).
5. **Extensibilidade futura:** haverá um agente dedicado com LLM e API key própria, que deve preferir consumir **JSON estruturado** de forma estável e barata, sem depender de texto.
6. **Streaming:** no transporte MCP (tools/call) a resposta é um payload final; já em HTTP, streaming (SSE/chunked/NDJSON) pode existir, mas não deve ser a única forma de eficiência (projeção/paginação/verbosidade são mais simples, testáveis e determinísticos).

## Decisões

### D1 — Introduzir parâmetros transversais de economia (`verbosity`, `include_explanations`, `fields/response_projection`)

**Decisão:** Tools que retornam payloads potencialmente grandes (listas, matrizes, breakdowns) devem aceitar parâmetros consistentes:

- `verbosity`: `"minimal" | "standard" | "full"`
- `include_explanations`: `boolean` (quando aplicável)
- `fields` (ou `response_projection`): lista de campos a incluir na resposta (projeção server-side), quando a tool tiver resposta grande e parcialmente útil.

**Semântica normativa (resumo):**

- `minimal`: deve omitir texto explicativo e campos redundantes; deve priorizar “dados mínimos auditáveis”.
- `standard`: mantém comportamento atual (ou próximo), com explicações curtas.
- `full`: inclui detalhamento completo, podendo exigir paginação (D4).

**Consequência de produto (não-predição):** `full` (e projeção/paginação associada) existe para **completude** do contrato, explicabilidade, ensino e auditoria — **não** para sugerir que “mais dados” impliquem melhor previsão de sorteio, vantagem estatística no jogo ou recomendação de apostas. O posicionamento do repositório permanece [brief.md](../brief.md) (educacional, descritivo na janela).

**Regra de determinismo:** se `deterministic_hash` for hash do payload canônico, mudanças de `verbosity/fields` devem ser refletidas no input hashed **ou** o contrato deve definir que o hash é da semântica e não da apresentação. A decisão específica de hashing por tool deve ser fechada no contrato quando estes parâmetros forem implementados.

### D2 — Separar canais: `StructuredContent` como fonte canônica; `Content` como resumo humano útil (nunca duplicar JSON)

**Decisão:** Em respostas MCP (stdio e HTTP MCP), o servidor deve tratar os canais como complementares:

- `StructuredContent`: **JSON canônico** (contrato) — destinado a automação/SDK/agentes.
- `Content`: **texto humano curto e útil** — destinado a chat/UX e alinhado a `verbosity`.

**Proibição:** o servidor **não** deve repetir o JSON completo do `StructuredContent` dentro de `Content` por padrão.

**Hotfix de clarificação pós-implementação:** eficiência de tokens **não autoriza** esvaziar a resposta humana. O `Content` deve continuar semanticamente suficiente para responder consultas comuns da tool sem exigir leitura manual do `StructuredContent`.

**Regras normativas adicionais:**

- `minimal`: pode ser econômico, mas deve manter **o fato principal auditável** da resposta.
- `standard`: deve ser o modo **chat-safe**; precisa responder sozinho às consultas humanas mais comuns da tool.
- `full`: pode remeter ao `StructuredContent` para detalhes extensos, mas não pode degradar para um aviso genérico do tipo "see structured payload" sem expor o resultado principal.
- tools factuais (ex.: `get_draw_window`) devem trazer no `Content` os fatos principais do recorte pedido, como concurso final, data e dezenas quando o recorte for pequeno ou a pergunta for naturalmente factual.
- tools analíticas (ex.: métricas, rankings, agregados) devem trazer no `Content` ao menos nomes e resultados salientes, sem dump integral do JSON.
- o servidor **não** deve inferir intenção conversacional além dos knobs explicitamente recebidos; o hotfix melhora a utilidade do `Content`, mas não autoriza mudança implícita de janela, projeção ou paginação.
- `standard` deve permanecer compacto: quando a resposta completa for extensa, o `Content` deve expor apenas o subconjunto saliente e deterministicamente escolhido, deixando o restante para `StructuredContent` e mecanismos explícitos como `fields`/paginação.
- a seleção do que entra em `Content` deve ser **determinística e derivada do payload estruturado**; não deve depender de síntese livre, massa dinâmica ou enriquecimento gerado fora do contrato.

**Justificativa:** reduz tokens no consumo por chat, preserva extensibilidade para agentes que consomem structured JSON e evita regressão de utilidade em hosts que não materializam bem o `StructuredContent`.

### D3 — Preferência e escolha do modo pertencem ao host/agente; o servidor não persiste “modo”

**Decisão:** O servidor não mantém estado de “modo econômico” entre chamadas. O host/agente:

- interpreta intenção do usuário (“modo econômico”, “detalhado”, “só números”) e mapeia para `verbosity/include_explanations/fields`;
- mantém essa preferência na sessão (chat) e aplica nos próximos calls.

**Consequência:** `help` e `discover_capabilities` devem tornar essa escolha evidente para leigos (ver D5).

### D4 — Paginação determinística como mecanismo principal para respostas grandes; streaming em HTTP é opcional

**Decisão:** Para payloads grandes em `full`, a forma primária de controle é:

- **paginação determinística** (ex.: `page`/`page_size` ou `cursor` estável por ordenação canônica), e/ou
- **projeção** (`fields`) para reduzir payload na fonte.

**Streaming (HTTP):** pode ser suportado no futuro para reduzir latência percebida, mas:

- não substitui paginação/projeção;
- deve manter determinismo (ordem canônica, contagem total, e cursor reproduzível quando aplicável);
- não deve exigir que o consumidor implemente lógica complexa para obter o “resultado final”.

### D5 — Descoberta e UX para leigos: `help` e `discover_capabilities` declaram knobs, exemplos e expectativa de utilidade

**Decisão:** Para reduzir tentativa/erro e permitir uso por leigos:

- `help` deve explicar, em linguagem simples, como pedir `minimal/standard/full` e quando usar.
- `help` deve também resolver o **primeiro uso operacional** sem exigir `verbosity="full"` nem leitura imediata do `StructuredContent`: como obter o último concurso, como começar com métricas básicas e quais campos mínimos de rastreabilidade preservar.
- `discover_capabilities` deve declarar:
  - quais tools suportam `verbosity/include_explanations/fields`;
  - valores aceitos (enums) e defaults recomendados;
  - avisos de custo (“para respostas curtas no chat, use minimal”);
  - aviso de utilidade: `standard` é o default recomendado para chat e não deve ocultar o resultado principal.
  - constraints operacionais relevantes para reduzir tentativa/erro, incluindo regras de janela por tool quando a build as expuser (ex.: `window_size=1` para ancorar no concurso mais recente, exigência de `end_contest_id` quando `start_contest_id` vier e coerência entre `window_size` e `start/end`).

**Hotfix de clarificação pós-implementação:** no caso de `help`, a utilidade mínima não é satisfeita por um simples índice de URIs/templates; o conteúdo deve incluir um quickstart textual auditável. No caso de `discover_capabilities`, a utilidade mínima não é satisfeita apenas por enums genéricos; a resposta deve conter as constraints operacionais necessárias para montar requests válidos sem tentativa/erro evitável.

**Alinhamento:** esta decisão complementa o modelo híbrido de descoberta do [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) (instância/build via tool + norma via docs/resources).

### D6 — Verificabilidade do hotfix: fixtures e saídas esperadas devem ser estáticas e revisáveis

**Decisão:** a validação do hotfix da ADR 0023 deve usar **massa de teste estática, pequena e auditável**, armazenada no repositório.

**Regras normativas:**

- testes do hotfix **não** devem depender de “último concurso disponível”, dataset vivo, rede externa ou qualquer fonte mutável fora do controle do repositório;
- a IA/agente **não** deve ficar responsável por inventar massa dinâmica no momento da execução para provar utilidade do `Content`;
- cada cenário de teste deve declarar explicitamente:
  - fixture de entrada;
  - request canônico;
  - expectativa de `StructuredContent` (por campos obrigatórios ou golden);
  - expectativa de `Content` (por trechos obrigatórios, ordem e ausência de anti-padrões);
- quando a comparação literal do `Content` for frágil, o teste deve preferir **asserções fechadas de presença/ausência** sobre fatos obrigatórios, mantendo o payload estruturado congelado via golden quando apropriado;
- para cenários de ranking/lista, a massa deve ser pequena o suficiente para revisão humana em PR antes da execução da suíte.

**Massa mínima recomendada para o hotfix:**

1. `tests/fixtures/synthetic_min_window.json` para consultas factuais curtas e métricas básicas.
2. `tests/fixtures/tie_heavy.json` para rankings/listas com empates e escolha determinística de itens salientes.
3. `tests/fixtures/golden/phase23/` para snapshots/goldens específicos do hotfix, revisáveis em PR.

**Matriz mínima de cenários revisáveis antes da execução:**

| ID | Tool / objetivo | Fixture estática | Request canônico | Expectativa mínima de `StructuredContent` | Expectativa mínima de `Content` |
|----|------------------|------------------|------------------|-------------------------------------------|---------------------------------|
| `HF23-M01` | `get_draw_window` factual curto: último concurso da janela | `tests/fixtures/synthetic_min_window.json` | `{ "window_size": 1, "end_contest_id": 3, "verbosity": "standard" }` | `window.size = 1`, `window.start_contest_id = 3`, `window.end_contest_id = 3`, `draws[0].contest_id = 3`, `draws[0].draw_date = "2003-10-13"`, `draws[0].numbers = [1,4,6,7,8,9,10,11,12,14,16,17,20,23,24]` | Deve conter concurso `3`, data `2003-10-13` e as 15 dezenas do concurso; não pode começar com JSON nem delegar para `"see structured payload"`. |
| `HF23-M02` | `get_draw_window` janela curta auditável | `tests/fixtures/synthetic_min_window.json` | `{ "window_size": 3, "end_contest_id": 3, "verbosity": "standard" }` | `window = { size: 3, start_contest_id: 1, end_contest_id: 3 }`, `draws.length = 3`, último draw igual ao cenário `HF23-M01` | Deve mencionar que a janela cobre `1..3` (ou 3 concursos) e expor o concurso final `3` com seus dados principais; não pode ser apenas contagem administrativa de draws. |
| `HF23-M03` | `compute_window_metrics` com lista curta saliente | `tests/fixtures/tie_heavy.json` | `{ "window_size": 5, "end_contest_id": 5005, "metrics": [{ "name": "top10_mais_sorteados" }], "verbosity": "standard" }` | `metrics[0].metric_name = "top10_mais_sorteados"`, `metrics[0].value = [11,12,13,14,15,16,17,18,19,20]`, alinhado ao golden `tests/fixtures/golden/phaseB2/tie-heavy-top10-mais-sorteados.golden.json` | Deve conter o nome da métrica e o top 10 `[11..20]` em ordem determinística; não pode responder só `"Computed 1 metric"` ou equivalente. |
| `HF23-M04` | `summarize_window_patterns` com saída estatística curta e verificável | `tests/fixtures/synthetic_min_window.json` | `{ "window_size": 5, "end_contest_id": 1005, "features": [{ "name": "pares_no_concurso" }], "coverage_threshold": 0.8, "range_method": "iqr", "verbosity": "standard" }` | `summaries[0].metric_name = "pares_no_concurso"`, `mode = 8`, `q1 = 8`, `median = 8`, `q3 = 9`, `iqr = 1`, `coverage_observed = 0.6`, `coverage_threshold_met = false` | Deve conter o nome da feature e pelo menos os resultados salientes `mode/median = 8`, `iqr = 1` e/ou `coverage = 0.6`; não pode ocultar todos os números relevantes. |
| `HF23-M05` | `summarize_window_aggregates` com agregados pequenos e revisáveis | `tests/fixtures/golden/phase22/summarize-window-aggregates.canonical-small-window.golden.json` como referência de saída; fixture do cenário conforme `phase22` | Request canônico igual ao cenário pequeno já coberto em `Phase22SummarizeWindowAggregatesContractRedTests` | `aggregates[0].id = "z_hist_pairs"`, `aggregates[1].id = "a_topk_rows"`, `aggregates[2].id = "m_matrix_rows"`; payload estruturado alinhado ao golden de `phase22` | Deve expor no `Content` ao menos os IDs/tipos salientes dos agregados ou um resumo factual equivalente; não pode responder apenas quantidade de agregados. |
| `HF23-M06` | Anti-esvaziamento em `full` | Reutilizar `HF23-M01`, `HF23-M03` e `HF23-M04` | Mesmo request dos cenários acima, trocando `verbosity` para `"full"` | `StructuredContent` segue completo/canônico | `Content` pode ser mais detalhado, mas não pode se reduzir a texto genérico do tipo `"See structured payload for ..."` sem expor o resultado principal do cenário. |
| `HF23-M07` | `help` com quickstart operacional auditável | N/A (meta-tool; sem fixture específica além da instância de teste) | `{}` | `getting_started_resource_uri`, `index_resource_uri`, `quick_start_markdown`, `templates[]` presentes e consistentes | Deve conter `get_draw_window(window_size=1)`, uma chamada inicial de `compute_window_metrics` e os campos `dataset_version`, `tool_version`, `deterministic_hash` e `window`; não pode se reduzir a “veja o índice” ou equivalente. |
| `HF23-M08` | `discover_capabilities` com constraints operacionais de janela | N/A (meta-tool) | `{}` ou `{ "verbosity": "standard" }` | `tools[]` inclui `get_draw_window` com `supported_parameters` suficientes para montar a janela | Deve declarar `window_size > 0`, o atalho `window_size=1`, a exigência de `end_contest_id` quando `start_contest_id` vier e a constraint de coerência entre `window_size` e `start/end`; não pode expor apenas os nomes dos parâmetros sem as regras de uso. |

**Convenção recomendada para artefatos de teste do hotfix:**

- `tests/fixtures/golden/phase23/get-draw-window.window1-end3.standard.golden.json`
- `tests/fixtures/golden/phase23/get-draw-window.window3-end3.standard.golden.json`
- `tests/fixtures/golden/phase23/compute-window-metrics.top10-mais-sorteados.tie-heavy.standard.golden.json`
- `tests/fixtures/golden/phase23/summarize-window-patterns.pares-iqr.standard.golden.json`

**Regra de revisão humana pré-execução:** nenhum cenário do hotfix entra na suíte sem que request, fixture e expectativa tenham sido materializados em arquivo revisável ou em asserts fechados explicitamente documentados no PR.

## Consequências

### Positivas

- Redução imediata de tokens no Cursor chat ao evitar duplicação de JSON no canal textual.
- Menos tool calls por permitir `fields` e `minimal` em tools “meta” e de payload grande.
- Extensibilidade: agentes futuros podem consumir `StructuredContent` de forma estável.
- Melhor comportamento em hosts MCP/chat que não materializam o `StructuredContent` de forma visível ao operador.
- Melhor auditabilidade do hotfix por usar fixtures/goldens pequenos e revisáveis.

### Custos

- Mais combinatória de testes: cada tool que suportar `verbosity/fields` precisa de testes de contrato cobrindo ao menos `minimal` e `full` (e paginação quando aplicável).
- Decisão explícita de `deterministic_hash` por tool quando apresentação variar (contrato precisa ser claro para evitar ambiguidade).
- Exige critérios objetivos de "utilidade mínima" por classe de tool para evitar resumos administrativos sem valor ao usuário.
- Exige curadoria explícita de massa de teste e saídas esperadas para evitar dependência de dataset mutável ou avaliação subjetiva ad hoc.

## Critérios de verificação (para outra IA planejar os ajustes)

1. **Contrato:** [mcp-tool-contract.md](../mcp-tool-contract.md) documenta `verbosity/include_explanations/fields` onde suportado e define a regra de `deterministic_hash` quando esses knobs alteram a apresentação.
2. **Ferramentas:** `discover_capabilities` expõe suporte e valores aceitos, sem payload excessivo (preferir projeção).
3. **Tokens:** em `verbosity="minimal"`, `Content` não contém JSON completo; é um resumo curto **e ainda útil**. O JSON permanece em `StructuredContent`.
4. **Determinismo:** duas chamadas idênticas (mesmo dataset + mesmos knobs) produzem respostas equivalentes e hash conforme política definida.
5. **Leigo:** `help` contém exemplos de frases (“modo econômico”, “agora detalhado”) e também um quickstart operacional explícito.
6. **Discovery operacional:** `discover_capabilities` expõe knobs e constraints relevantes de montagem de request, não apenas enums soltos.
7. **Chat-safe:** em `verbosity="standard"`, consultas humanas comuns da tool podem ser respondidas a partir de `Content` sem inspeção manual do `StructuredContent`.
8. **Tools factuais, analíticas e meta-tools:** cada classe de tool documenta quais fatos/resultados principais devem aparecer no `Content` para não haver perda de utilidade.
9. **Sem efeitos colaterais implícitos:** o hotfix não introduz defaults ocultos de janela, paginação, projeção ou “intenção” inferida no servidor.
10. **Massa estática:** os testes do hotfix usam fixtures/goldens estáticos do repositório, com entradas e expectativas revisáveis antes da execução.

## Alternativas consideradas

1. **Remover `StructuredContent` agora para economizar tokens** — rejeitado: quebra extensibilidade e automação futura; o problema é duplicação, não a existência do canal.
2. **Forçar `minimal` como default global** — rejeitado por risco de UX e quebra de expectativas; preferir defaults compatíveis e knobs explícitos.
3. **Apostar apenas em streaming HTTP** — rejeitado como solução principal: aumenta complexidade e não resolve eficiência no canal MCP stdio; paginação/projeção é mais simples e testável.

## Referências internas

- [mcp-tool-contract.md](../mcp-tool-contract.md)
- [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md)
- [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)
- [ADR 0009](0009-help-e-catalogo-de-templates-resources-v1.md)
- [docs/mcp-tools-melhorias-planejamento.md](../mcp-tools-melhorias-planejamento.md) (Anexo B — eficiência e UX)

