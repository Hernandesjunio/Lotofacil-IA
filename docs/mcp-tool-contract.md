# Contrato das ferramentas MCP

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

## Objetivo

Definir um contrato explicável, testável e determinístico para um MCP que permita:

- consultar concursos e janelas;
- calcular métricas canônicas;
- analisar estabilidade, composição, associação e padrões históricos;
- gerar jogos candidatos por estratégias nomeadas ou perfil composto declarado;
- explicar resultados de forma reproduzível.

Este contrato não assume capacidade preditiva. O foco é análise descritiva, geração heurística e explicabilidade.

## Guia de leitura e validação

O contrato está organizado em camadas. Para validar uma implementação ou um PR contra o documento, percorra nesta ordem:

1. **Escopo V1** — confirma o que o servidor não faz (limites éticos e técnicos).
2. **Modelo conceitual** — define o vocabulário: o que é `Draw`, `MetricValue`, etc. Toda resposta deve poder ser mapeada a esses tipos.
3. **Invariantes globais** — regras transversais; violar qualquer uma é falha de contrato, mesmo que o JSON do request esteja “válido” no sentido de schema.
4. **Ferramentas** — cada tool tem finalidade, input de exemplo, regras e (abaixo) semântica dos campos para validação pontual.
5. **Estratégias e erros** — catálogo fechado de geração e de códigos de erro esperados.
6. **Persistência e testes mínimos** — requisitos de dados e checklist objetivo de aceite.

**Validação prática:** para cada tool, verifique (a) rejeição de inputs inválidos com o código de erro certo, (b) presença dos campos obrigatórios no output conforme invariantes, (c) reprodutibilidade quando o contrato exige `seed` ou quando `deterministic_hash` e `replay_guaranteed` fixam o comportamento esperado ([ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md) para geração).

7. **Lacunas de parâmetro em linguagem natural** — quando um pedido do usuário não puder ser mapeado sem ambiguidade para o JSON da tool, o fluxo deve obter dados faltantes por **perguntas específicas** (seção *Integração com agentes: lacunas de parâmetros e esclarecimento*, abaixo), nunca por inferência oculta no servidor.
8. **Inter-tool, disponibilidade e pipeline** — recorte de métricas por rota, padrão de cadeia de tools (fluidez), `stability_check` em associações e códigos de erro associados estão normatizados em [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md). O catálogo de métricas e a tabela *Disponibilidade normativa* em [metric-catalog.md](metric-catalog.md) desambiguam “nome canónico” vs. “aceite nesta tool nesta build”.
9. **Descoberta para consumidores e janela por extremos** — o modelo híbrido *superfície de instância* (tool ou erros com allowlist) vs *norma* (resources / `docs/`), a equivalência entre janela expressa por `start_contest_id`/`end_contest_id` e `window_size`+`end_contest_id`, o mapeamento do rótulo de export `HistoricoTop10MaisSorteados` para a métrica canónica `top10_mais_sorteados`, e a proibição de *defaults* “últimos N” herdados de UI legada, estão em [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) (D1–D4).

## Disponibilidade de métricas, pipeline mínimo e performance do fluxo

### Catálogo vs. `compute_window_metrics`

- O [metric-catalog.md](metric-catalog.md) define **nomes, fórmulas, formas e versões**; uma build pode, num recorte, implementar `compute_window_metrics` apenas para um **subconjunto** alinhado à [vertical-slice.md](vertical-slice.md) e extensões documentadas.  
- `UNKNOWN_METRIC` com `details.metric_name` preenchido indica, neste contexto, **nome conhecido no catálogo mas ainda não utilizável nesta rota ou nesta build**; o servidor **deve** preencher `details` com pistas auditáveis (p.ex. `allowed_metrics` fechado ou mensagem canónica) e manter o `tool_version` rastreável, conforme [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) D1.
- **Migração / sunset (nome deprecated):** quando a métrica existir no catálogo mas o nome foi removido por política de sunset, o servidor deve responder `UNKNOWN_METRIC` e incluir em `details` (quando aplicável):
  - `deprecated_metric_name` (o nome pedido);
  - `replacement_metric_name` (nome recomendado no catálogo);
  - `sunset_date` (ISO-8601, ex. `2026-05-25`);
  - `migration_note` (texto humano curto).
- Isto **não** contradiu o `UNKNOWN_METRIC` para strings que não estão no catálogo: aí a correção continua a ser ajustar o request ou o catálogo, não a implementação adivinhar o nome.

### Pipeline mínimo recomendado (reprodutível, sem defaults ocultos)

Padrão descriptivo para análise de janela, interação entre indicadores e candidatos, respeitando a mesma janela explícita (`window_size`, `end_contest_id`, `dataset_version`):

1. `get_draw_window` quando o cliente precisar do recorte bruto; caso contrário, as tools seguintes materializam a janela da mesma forma.  
2. `compute_window_metrics` com a lista `metrics` **explícita**; ou, conforme a pergunta, ir direto a análise.  
3. `analyze_indicator_stability`, `analyze_indicator_associations` e/ou `summarize_window_patterns` **conforme necessidade**, todos com a mesma janela.  
4. `generate_candidate_games` com `generation_mode`, `plan` e, **se** se pretender replay da lista de candidatos, `seed` (ver `replay_guaranteed` na resposta); em seguida `explain_candidate_games` para os mesmos parâmetros de janela e lista de jogos.  

A **fluidez** é obtida com **menos *round-trips** explícitos (lote de métricas no passo 2) e com documentação alinhada à [generation-strategies.md](generation-strategies.md), não com defaults não documentados no servidor.

**Validação:** testes de integração e de contrato devem referir-se a estes passos nos planos [contract-test-plan.md](contract-test-plan.md) e [test-plan.md](test-plan.md) quando validarem GAPS e cenários de correlação (ver ADR 0006 D3 e D5).

## Integração com agentes: lacunas de parâmetros e esclarecimento

### Problema

Uma conversa em linguagem natural pode ser insuficiente para o modelo montar todos os argumentos exigidos pelo schema da tool (janela, `seed`, agregações vetoriais, pesos que somam 1, estratégia fechada, etc.). O contrato **proíbe** que o servidor preencha lacunas com defaults semânticos não documentados (ver `MetricRequest` e invariante de composição explícita). Isso cria um gap entre intenção do usuário e chamada MCP válida.

### Obrigação do fluxo host/agente (validação dos `docs/`)

Antes de invocar uma tool com argumentos incompletos ou genéricos demais, o agente (cliente) deve:

1. **Identificar** quais campos obrigatórios ou enums fechados não foram fixados pelo texto do usuário.
2. **Perguntar de forma específica**, listando o que falta e, quando aplicável, as opções válidas do próprio contrato (ex.: `window_size`, `end_contest_id`, `aggregation` para séries vetoriais, `seed` para busca amostrada, nomes de métricas do catálogo).
3. **Só então** montar o JSON final e chamar a tool — uma requisição completa por chamada, mantendo o invariante **Stateless por request** (sem “memória” da conversa dentro do servidor).

Perguntas vagas (“quer dizer qual janela?”) **não** cumprem este requisito; a pergunta deve ser auditável (alinhada aos campos do schema e às tabelas deste documento).

### Papel do servidor MCP

O servidor pode permanecer estritamente validador: entradas inválidas ou incompletas → `INVALID_REQUEST` (e demais códigos da tabela de erros). Opcionalmente, a implementação pode enriquecer a resposta de erro com uma lista estruturada de faltas (ex.: `missing: ["window_size", "seed"]`, `hints` com enums permitidos) para o cliente exibir perguntas guiadas **sem** alterar a semântica stateless: nenhuma execução parcial com defaults inventados.

### Relação com prompts de teste

Famílias em [prompt-catalog.md](prompt-catalog.md) devem ser **suficientemente declarativas** para permitir o mapeamento direto. Prompts ambíguos pertencem a testes **negativos**: o resultado esperado é esclarecimento (ou recusa), não execução com suposições.

## Primitivas MCP opcionais: Prompts e Resources

Esta seção **não** substitui as tools: o cálculo determinístico continua em chamadas com JSON explícito. Ela trata de recursos **adicionais** do [Model Context Protocol](https://modelcontextprotocol.io/) para reduzir falhas de mapeamento e fornecer contexto estável ao modelo.

| Primitiva | O que é no MCP (resumo) | Uso neste projeto (quando faz sentido) |
|-----------|-------------------------|----------------------------------------|
| **Prompts** (`prompts/list`, `prompts/get`) | Templates com argumentos nomeados; mensagens estruturadas para o LLM ([especificação](https://modelcontextprotocol.io/specification/2025-06-18/server/prompts)). | **Camada C (ADR 0008 D1):** ergonomia de orquestração — fluxos recorrentes onde os **argumentos do template espelham campos do schema** da tool, reduzindo omissão sem violar a proibição de pesos implícitos. |
| **Resources** (`resources/read`, templates) | Dados identificados por URI, fornecendo contexto à aplicação/modelo ([especificação](https://modelcontextprotocol.io/specification/2025-06-18/server/resources)). | **Camada B (ADR 0008 D1):** *norma* estável — trechos do [metric-glossary.md](metric-glossary.md), [metric-catalog.md](metric-catalog.md) e referências a ADRs, **read-only**, sem lógica de cálculo. |

**Descoberta “o que posso pedir a esta instância” (camada A, ADR 0008 D1):** a *allowlist* concreta desta build (`allowed_metrics` alinhado a [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md), enums como `aggregate_type` em `summarize_window_aggregates`, *tool_version*) deve ser obtida por **respostas de tool** ou **`details` em erros** (p.ex. `UNKNOWN_METRIC` com pistas fechadas), de forma **falsificável** por teste de contrato. O catálogo em `docs/` permanece a *norma semântica* (camada B); **não** se exige neste contrato um nome fechado de tool de listagem — só a semântica acima. O modelo híbrido A/B/C está fechado em [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) D1.

**Disambiguação:** “Prompt” no protocolo MCP ≠ “prompt do usuário” no chat cotidiano. O primeiro é recurso **servidor** com descoberta e parâmetros; o segundo é entrada livre, sujeita às regras de esclarecimento desta seção.

### Fundamentação para uso (evidências)

1. **Prompts MCP expõem argumentos declarados** — A especificação define `arguments` por prompt (nome, descrição, obrigatoriedade) e validação antes do processamento; isso alinha-se ao requisito de parâmetros explícitos do domínio Lotofácil sem conflitar com o invariante de stateless, desde que o resultado final das tools seja JSON completo por requisição. Ver [Prompts — Data Types / arguments](https://modelcontextprotocol.io/specification/2025-06-18/server/prompts).
2. **Resources formalizam contexto anexável** — O protocolo descreve resources como meio padronizado de compartilhar “files, database schemas, or application-specific information” com o modelo; encaixa glossário/catálogo como dados de referência estáveis. Ver [Resources — introdução](https://modelcontextprotocol.io/specification/2025-06-18/server/resources).
3. **Composição Prompt + Resource na spec** — Mensagens de prompt podem incluir **embedded resources** (documentação, exemplos) diretamente no fluxo, o que sustenta análises com insumos textuais canônicos sem inflar o system prompt do cliente. Ver [Prompts — Embedded resources](https://modelcontextprotocol.io/specification/2025-06-18/server/prompts).
4. **Application-driven** — Resources são “application-driven”; o host decide inclusão no contexto. Isso é compatível com políticas de auditoria e limite de tokens no cliente.

### Quando **não** introduzir Prompts/Resources na implementação

- Se o cliente já injeta [metric-glossary.md](metric-glossary.md) / [metric-catalog.md](metric-catalog.md) por outros meios e os testes E2E cobrem o mapeamento NL→JSON, as primitivas extras são **opcionais** (otimização e padronização, não pré-requisito de correção).
- Evitar duplicar documentação mutável em duas superfícies sem processo de sincronização: ou resources gerados a partir dos mesmos fontes do repositório, ou apenas referência por URI estável.

## Decisão de escopo para V1

A V1 expandida deve operar sobre um histórico canônico da Lotofácil e expor poucas tools de alto valor, cada uma com semântica fechada e payload estável.

### Fora da V1

- chat livre dentro do servidor;
- recomendação comercial de apostas;
- linguagem de "jogo provável" ou "chance de sair";
- pesos implícitos inferidos por prompt;
- escrita concorrente em múltiplas fontes sem reconciliação;
- inferência preditiva não validada.

## Modelo conceitual

As entidades abaixo são o “tipo de dados mental” do MCP: implementação e testes devem conseguir serializar e desserializar resultados usando esses nomes e relações. Campos marcados com `?` são opcionais no request ou no payload conforme a tool.

### Entidades canônicas

#### `Draw`

Um sorteio único da Lotofácil no histórico canônico.

- `contest_id` — identificador estável do concurso (chave de junção com janelas e métricas).
- `draw_date` — data do sorteio no calendário (auditoria e ordenação humana).
- `numbers` — array ordenado crescente com 15 dezenas válidas entre 1 e 25 (regra do jogo).
- `source` — origem do registro (arquivo, API, migração) para rastreabilidade.
- `ingested_at` — quando o registro entrou no dataset; útil para versionamento e depuração.

**Validação:** 15 dezenas distintas, intervalo [1, 25], ordem crescente; duplicidade de `contest_id` no dataset é erro de dados.

#### `Window`

Recorte contínuo de concursos usado como referência temporal para métricas e análises.

- `size` — quantidade de concursos na janela (comprimento).
- `start_contest_id` / `end_contest_id` — limites inclusivos da janela após resolução no dataset.
- `draws` — lista de `Draw` em ordem crescente de concurso.

**Validação:** `len(draws) == size` (salvo `INSUFFICIENT_HISTORY`); extremos coerentes com o pedido (`end_contest_id` ancorado quando omitido no request).

**Proibição de N mágico (legado UI):** tamanhos do tipo “últimos 10/20 concursos” que existiam como *default* implícito em ecrãs antigos **não** são aplicados pelo servidor no MCP. O recorte é sempre o que o **chamador declara** via `window_size` + `end_contest_id` (ou par equivalente por D2) — [ADR 0008 D4](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md).

**Resolução de janela no *request* (equivalência):** quando o cliente declara `start_contest_id` e `end_contest_id` **inclusivos** e a janela for contígua no dataset canónico, a resolução é equivalente a `end_contest_id` (extremo mais recente) e `window_size = end_contest_id - start_contest_id + 1`, conforme [ADR 0008 D2](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md). A implementação pode aceitar só `window_size` + `end_contest_id` no protocolo, desde que esta equivalência fique documentada e testada.

**Conflito ou redundância no request:** se o JSON incluir `window_size` *e* `start_contest_id` / `end_contest_id` de formas **mutuamente incompatíveis** (não reduzíveis a uma única janela), ou combinação **não interpretável de forma única**, a tool **deve** recusar com `INVALID_REQUEST` (ou código documentado no mesmo espírito), **sem** escolher silenciosamente um recorte. Ordem e exemplos mínimos para testes: [contract-test-plan.md](contract-test-plan.md) Fase B.2.

#### `MetricRequest`

Pedido de cálculo de uma métrica nomeada, possivelmente com redução agregada.

- `name` — identificador da métrica no catálogo fechado.
- `params?` — parâmetros somente explícitos; o servidor não inventa defaults não documentados.
- `aggregation?` — obrigatório quando o `shape` da métrica não for escalar e a tool exigir um único valor ou ranking escalar.
- `component_index?` — quando o cliente escolhe um componente fixo de um vetor/série vetorial.

**Validação:** `UNKNOWN_METRIC` fora do catálogo; vetorial sem agregação onde a tool exige → `UNSUPPORTED_AGGREGATION` ou equivalente.

#### `MetricValue`

Resultado tipado de uma métrica: liga nome, forma do valor, janela e texto explicável.

- `metric_name` — eco do pedido ou nome canônico resolvido.
- `scope` — `window` (agregado na janela), `series` (por concurso ao longo da janela), `candidate_game` (avaliação sobre um jogo candidato).
- `shape` — formato de `value` (enum fechado; ver abaixo); evita ambiguidade no cliente.
- `window` — qual janela foi usada (transparência obrigatória).
- `value` — payload numérico ou estruturado conforme `shape`.
- `unit` — unidade semântica (ex.: contagem, proporção, bits) para interpretação correta.
- `explanation` — texto ou estrutura explicativa alinhada ao invariante de termos definidos.
- `version` — versão da definição da métrica (reprodutibilidade).

**Validação:** `shape` consistente com `value`; `scope` coerente com a tool; V1 não usa `scope = draw` (ver seção de viabilidade).

##### Catálogo fechado de `shape`

`shape` é um identificador fechado para tipar `value`. Valores aceitos na V1 expandida (alinhados ao catálogo de métricas):

- `scalar`
- `series`
- `vector_by_dezena` (25 posições)
- `count_vector[5]`
- `series_of_count_vector[5]`
- `count_matrix[25x15]`
- `count_pair`
- `dezena_list[10]`
- `count_list_by_dezena`
- `dimensionless_pair`

**Semântica e validação:** cada `shape` impõe uma forma validável de `value` (ex.: `count_vector[5]` é um vetor de 5 contagens inteiras não negativas; `count_matrix[25x15]` é matriz 25x15 de contagens inteiras não negativas; `series` tem comprimento igual à janela resolvida, salvo regras documentadas de fronteira).

#### `CandidateGame`

Um jogo de 15 dezenas produzido por heurística, com rastreio completo da decisão.

- `numbers` — dezenas do candidato (normalmente ordenadas se `global_constraints.sorted_numbers`).
- `strategy_name` / `strategy_version` — qual estratégia nominal gerou o jogo.
- `seed_used` — semente efetiva após derivação (determinismo).
- `search_method` — como o espaço foi explorado (enum fechado; ver abaixo).
- `n_samples_used?` — amostras avaliadas quando aplicável.
- `scores` — mapa ou lista de subscores usados no ranking ou na composição.
- `constraints_applied` — restrições globais e estruturais efetivamente aplicadas a este jogo.
- `tie_break_rule` — regra determinística quando empates ocorrem (enum fechado; ver abaixo).
- `rationale` — justificativa legível, sem linguagem preditiva proibida.

**Validação:** todo campo exigido pelos invariantes de geração deve estar presente; exclusões estruturais usadas devem aparecer em output agregado ou em `explain_candidate_games`.

##### Catálogo fechado de `search_method`

Valores aceitos:

- `exhaustive` — varre o espaço definido pela estratégia (ou usa algoritmo exato equivalente) e escolhe o ótimo determinístico por `tie_break_rule`.
- `sampled` — amostra do espaço; com **`replay_guaranteed: true`** em `generate_candidate_games`, exige `seed` no request e `seed_used` + `n_samples_used` no output. Com **`replay_guaranteed: false`**, `seed` é opcional; vide [ADR 0020 D7](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md) e secção *`deterministic_hash` e `replay_guaranteed`*.
- `greedy_topk` — exploração gulosa/top-k; com **`replay_guaranteed: true`**, exige `seed` quando houver desempate/aleatoriedade na seleção do topo; com **`replay_guaranteed: false`**, mesma qualificação que `sampled`.

##### Catálogo fechado de `tie_break_rule`

Valores aceitos:

- `lexicographic_numbers_asc` — empate resolve pelo jogo ordenado crescente, comparado lexicograficamente.
- `stable_rank_then_lexicographic` — mantém ordem estável do ranking/score e desempata por `lexicographic_numbers_asc`.

## Invariantes globais

Cada item é uma obrigação de comportamento; testes de contrato devem assertar explicitamente o item correspondente.

1. **Reprodutibilidade por versão de dados** — Mesmo input canônico + mesmo `dataset_version` deve produzir o mesmo output **canónico** da tool.  
   *Qualificação (`generate_candidate_games`, [ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)):* quando a resposta declara `replay_guaranteed: false`, a lista ordenada de candidatos **pode** diferir entre chamadas com o mesmo JSON; o invariante aplica-se ao restante do payload conforme a política de `deterministic_hash` descrita na secção *Canonização* (subsecção *`deterministic_hash` e `replay_guaranteed`*).  
   *Validação:* duas chamadas idênticas após ingestão estável; comparar payload ou hash conforme política do projeto e o valor de `replay_guaranteed`.

2. **Transparência de janela** — Toda resposta analítica declara a janela usada.  
   *Validação:* presença de `Window` ou campos equivalentes (`window_size`, `start_contest_id`, `end_contest_id`, lista de ids) no corpo da resposta.

3. **Transparência de geração** — Toda resposta de sucesso de `generate_candidate_games` declara `replay_guaranteed`, estratégia, versão, filtros e critérios efetivos, `search_method`, `tie_break_rule` e `seed_used` **quando** o episódio for replayável (`replay_guaranteed: true`) ou quando o servidor tiver materializado uma semente para auditoria.  
   *Validação:* nenhum jogo candidato sem linhagem de estratégia; com `replay_guaranteed: true`, `seed_used` está presente e estável entre execuções para o mesmo pedido canónico.

4. **Stateless por request** — Nenhuma tool depende de contexto oculto de conversa.  
   *Validação:* mesma requisição HTTP/MCP isolada produz mesmo resultado; não há “memória” implícita entre chamadas além do dataset versionado.

5. **Linguagem definida** — Termos como `slot`, `outlier`, `persistência`, `equilíbrio`, `faixa típica` e `correlação` devem ter definição explícita no payload ou na documentação.  
   *Validação:* glossário ou campo `explanation`/`definitions` referenciado; respostas não usam jargão solto.

6. **Proibição de predição comercial** — Ferramentas não devem concluir "mais provável de sair"; devem concluir "mais estável", "mais aderente", "mais persistente no histórico declarado" ou "mais raro".  
   *Validação:* revisão de strings em `rationale` e resumos; testes de regressão com prompts que pedem “probabilidade”.

7. **Hash determinístico** — Regra por defeito: `deterministic_hash = SHA256(canonical_json({input, dataset_version, tool_version}))`, com `input` o pedido canónico da tool (ver *Canonização*).  
   *Qualificação (`generate_candidate_games`):* quando `replay_guaranteed: false`, o objeto hashed **exclui** a sequência concreta de candidatos gerados e outras parcelas puramente estocásticas da saída; inclui insumos não aleatórios (janela, `generation_mode`, plano e restrições **resolvidas**, versões) de forma a permanecer **idêntico** entre duas execuções com o mesmo request **mesmo** que as listas de dezenas diferirem. Quando `replay_guaranteed: true`, o `input` hashed **deve** incluir tudo o que fixa o episódio reprodutível (incluindo `seed` e parâmetros relevantes), de modo que o mesmo pedido ⇒ mesmo hash **e** mesma lista ordenada de candidatos.  
   *Validação:* `canonical_json` segue JSON canônico (RFC 8785); hash estável para fixture fixa segundo a política de `replay_guaranteed`. Ver [ADR 0001 D1](adrs/0001-fechamento-semantico-e-determinismo-v1.md) (núcleo de determinismo) e [ADR 0020 D7](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md) (qualificação da rota de geração).

8. **Composição totalmente declarada** — Toda composição dinâmica deve declarar componentes, transformações, agregações, pesos e operador.  
   *Validação:* `compose_indicator_analysis` e perfis de geração sem campos omitidos “por convenção”.

9. **Exclusões auditáveis** — Toda exclusão estrutural usada na geração deve ser reportada no output.  
   *Validação:* `structural_exclusions` refletidos em metadados da resposta ou em `explain_candidate_games` com `include_exclusion_breakdown`.

## Canonização e metadados comuns

Esta seção fecha os termos usados pelos invariantes (`dataset_version`, `tool_version`, `canonical_json` e o envelope mínimo de metadados). Implementações podem ter campos adicionais, mas os itens abaixo são o contrato estável.

### `dataset_version`

#### Finalidade

Identificar qual snapshot do histórico canônico foi consumido na execução.

#### Input

Não é input do cliente. Campo de output em todas as ferramentas.

#### Regras

- Deve ser estável para o mesmo snapshot de dados.
- Deve mudar quando o dataset canônico mudar (correção/ingestão).

#### Observações

- Serve para auditoria, cache e reprodutibilidade do `deterministic_hash`.

#### Semântica e validação

- **Formato recomendado:** string rastreável e determinística, por exemplo `cef-YYYY-MM-DD-shaXXXXXXXX` (prefixo + data humana + hash curto do snapshot).
- **Validação:** presente e não vazio; duas chamadas idênticas com o mesmo snapshot retornam o mesmo valor.

### `tool_version`

#### Finalidade

Versionar a semântica/implementação da tool (ou do servidor) consumida pelo cliente, para auditoria e reprodutibilidade.

#### Input

Não é input do cliente. Campo de output em todas as ferramentas.

#### Regras

- Deve existir em todas as respostas.
- Deve mudar quando a semântica de output mudar (mesmo que o dataset esteja estável).

#### Observações

- Evita que o `deterministic_hash` pareça “quebrado” quando uma mudança de implementação altera resultados.

#### Semântica e validação

- **Formato recomendado:** SemVer em string (ex.: `"1.2.0"`).
- **Validação:** presente e usado na composição do `deterministic_hash`.

### `canonical_json(...)`

#### Finalidade

Definir a serialização canônica do input + versões para gerar o `deterministic_hash` de forma idêntica entre implementações.

#### Input

Uso interno do servidor. Não é exposto como campo obrigatório, mas sua regra é parte do contrato.

#### Regras

- A canonização não pode depender de ordem de chaves, whitespace ou representação equivalente de números.
- Deve produzir bytes estáveis para o mesmo objeto lógico.

#### Observações

- Sem canonização fechada, duas implementações corretas podem divergir no hash mesmo com output equivalente.

#### Semântica e validação

- **Recomendação normativa:** `canonical_json` deve seguir RFC 8785 (JSON Canonicalization Scheme / JCS).
- **Validação:** fixtures douradas (input conhecido) têm hash idêntico entre runs e entre stacks.

### `deterministic_hash` e `replay_guaranteed` (`generate_candidate_games`)

#### Finalidade

Harmonizar o invariante global de hash com a política de **`seed` opcional** e de **replay não garantido** na geração, sem contradizer o núcleo de determinismo do projeto ([ADR 0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md)); qualificação normativa em [ADR 0020 D7](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md).

#### `replay_guaranteed` (resposta, nome canónico)

- Campo **booleano obrigatório** em toda resposta **de sucesso** de `generate_candidate_games` (nível superior do payload ou `meta` equivalente, junto de `dataset_version`, `tool_version`, `deterministic_hash`).
- **`true`:** o servidor garante que, com o mesmo pedido canónico (incl. `seed` quando a estocástica depender dele), o mesmo `dataset_version` e o mesmo `tool_version`, a **lista ordenada de candidatos** devolvida é idêntica entre invocações.
- **`false`:** o servidor **não** garante igualdade da lista de candidatos em reinvocações com o mesmo JSON (caso típico: `seed` ausente **e** existir componente estocástica na estratégia ou no método de busca). Isto **não** implica predição de resultado; apenas deixa explícito que o episódio é **não replayável** para efeito de regressão ou golden de candidatos.

#### `deterministic_hash` *vs.* presença de `seed`

| Situação | `replay_guaranteed` | Significado normativo de `deterministic_hash` |
|----------|----------------------|-----------------------------------------------|
| `seed` **presente** e restrições de replay satisfeitas | `true` | Hash do conjunto de insumos que **fixam** o episódio reprodutível (inclui `seed` e configuração canónica relevante) + `dataset_version` + `tool_version`; duas execuções com o mesmo pedido ⇒ **mesmo** hash **e** mesma lista de candidatos. |
| `seed` **ausente** e há estocástica na geração | `false` | Hash dos insumos **não aleatórios** (janela, `generation_mode`, critérios/filtros/`structural_exclusions` **declarados e resolvidos**, orçamentos, versões) **sem** incluir a sequência concreta de candidatos; duas execuções podem divergir nas dezenas **mantendo** o mesmo hash. |
| Geração **puramente determinística** na rota (sem entropia; ex.: busca exaustiva sem amostragem) | `true` | Igual à primeira linha em espírito: o pedido canónico fixa a saída; `seed` pode ser omitido quando o contrato da estratégia o declarar. |

Qualquer **default** aplicado pelo servidor (incl. modo efetivo se no futuro o request permitir omissão com resolução documentada) continua a ser ecoado em `applied_configuration.resolved_defaults` quando a implementação existir ([ADR 0017](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md), [ADR 0020 D1](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)).

### Envelope de metadados comum de resposta

#### Finalidade

Padronizar os metadados mínimos exigidos pelos invariantes de reprodutibilidade e auditoria.

#### Input

N/A (output).

#### Regras

- Toda resposta deve incluir, no nível superior do payload (ou em um campo `meta` equivalente), no mínimo:
  - `dataset_version`
  - `tool_version`
  - `deterministic_hash`

#### Observações

- Ferramentas que usam janela devem também declarar a janela efetiva usada (ver invariante 2).
- `generate_candidate_games` acrescenta **`replay_guaranteed`** ao conjunto mínimo da resposta de sucesso (ver secção dessa tool e *Canonização*).

#### Semântica e validação

- **Validação:** testes de contrato devem asserir presença de todos os campos acima em todas as tools; em `generate_candidate_games`, incluir `replay_guaranteed` e validar `deterministic_hash` segundo `true`/`false`.

## Ferramentas propostas

### Parâmetros comuns de janela

Várias tools recebem um recorte temporal sobre o histórico canônico. Campos recorrentes:

| Campo | O que representa | Como validar |
|--------|------------------|--------------|
| `window_size` | Quantidade de concursos consecutivos a considerar, ancorada no fim da janela. | Inteiro `>= 1`; zero ou negativo → `INVALID_WINDOW_SIZE`. |
| `end_contest_id` | Último concurso inclusivo da janela. | Se omitido, o servidor usa o mais recente disponível; id inexistente → `INVALID_CONTEST_ID`. |
| `start_contest_id` | (Opcional) Primeiro concurso inclusivo, **se** a tool/transporte estender o schema conforme [ADR 0008 D2](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md). | Com `end_contest_id`, define o mesmo recorte que `window_size = end - start + 1` e o mesmo `end_contest_id`; `start > end` ou intervalo inválido → `INVALID_REQUEST` (ou `INVALID_CONTEST_ID` conforme política fechada na implementação). |
| Incompatibilidade entre `window_size` e par `start`/`end` | Redundância que não fecha numa única janela. | `INVALID_REQUEST` com `details` coerentes — sem *snap* silencioso. |
| `include_metadata` | Quando presente (ex.: `get_draw_window`), inclui campos auxiliares de `Draw` além dos números. | Resposta contém `source` / `ingested_at` etc. somente quando `true`, salvo política explícita documentada. |

**Legado “últimos N” na UI (ADR 0008 D4):** o servidor **não** mapeia automaticamente *N* de gráficos antigos para `window_size`; o cliente declara a janela.

Outras tools usam o mesmo ancoramento temporal implicitamente: a janela é sempre “os últimos `window_size` sorteios até `end_contest_id`”, salvo erro de histórico insuficiente.

### 1. `get_draw_window`

#### Finalidade

Retornar um recorte canônico de concursos.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "include_metadata": true
}
```

#### Regras

- `window_size` deve ser inteiro positivo.
- Se `end_contest_id` for omitido, usar o concurso mais recente.
- Os concursos devem ser retornados em ordem crescente.

#### Semântica e validação

- **Output esperado:** um `Window` (ou estrutura equivalente) com `draws` ordenados por `contest_id` crescente.
- **Caso limite:** se não houver `window_size` concursos até o fim ancorado → `INSUFFICIENT_HISTORY`.
- **Uso:** base para todas as análises; não calcula métricas, apenas materializa o recorte de dados.

### 2. `compute_window_metrics`

#### Finalidade

Calcular métricas canônicas para uma janela.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "allow_pending": false,
  "metrics": [
    { "name": "frequencia_por_dezena" },
    { "name": "repeticao_concurso_anterior" },
    { "name": "distribuicao_linha_por_concurso" },
    { "name": "entropia_linha_por_concurso" }
  ]
}
```

#### Regras

- `metrics` é obrigatório.
- Cada item de `metrics` é um objeto; nomes soltos deixam de ser aceitos na V1 expandida.
- Métrica desconhecida retorna `UNKNOWN_METRIC`.
- Métricas com **Status** `pendente de detalhamento` no [metric-catalog.md](metric-catalog.md) exigem `allow_pending: true`.
- Parâmetros de métrica devem ser explícitos em `params`; o servidor não infere defaults semânticos escondidos.

#### Disponibilidade em `compute_window_metrics` (D1, ADR 0006)

- A tool pode recusar uma métrica cujo **nome** está no [metric-catalog.md](metric-catalog.md) com `UNKNOWN_METRIC` quando a **implementação** da build ainda não expuser essa métrica nesta rota; nesse caso `details.metric_name` identifica o pedido e, quando possível, `details` inclui a lista de nomes **efetivamente** aceites.  
- O recorte mínimo documentado em [vertical-slice.md](vertical-slice.md) exige, para fechamento da V0, apenas `frequencia_por_dezena@1.0.0` em sucesso; outras entradas seguem a matriz em [metric-catalog.md](metric-catalog.md) e a decisão [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md).  
- **Coesão com geração/explicar:** se `explain_candidate_games` ou estratégias referem internamente `repeticao_concurso_anterior` (ou outra), mas `compute_window_metrics` a rejeita nesta build, o teste de coerência cruzada em [test-plan.md](test-plan.md) aplica-se até a métrica ser promovida (ver *GAPS*, [contract-test-plan.md](contract-test-plan.md)).
- **Deprecação (compatibilidade temporária):** durante a janela de migração, uma build pode aceitar nomes equivalentes (ex.: `frequencia_por_dezena` e `total_de_presencas_na_janela_por_dezena`) e retornar ambos normalmente. Após o sunset definido no catálogo, a build pode recusar o nome antigo com `UNKNOWN_METRIC` e `details.replacement_metric_name` (ver “Catálogo vs. `compute_window_metrics`” acima).

#### Observações

- `scope`, `shape`, `unit` e `version` são sempre explícitos.
- A tool cobre tanto métricas clássicas quanto séries estruturais por concurso.

#### Semântica e validação

- **`metrics`:** lista de `MetricRequest`; cada elemento deve ser objeto com pelo menos `name`. Strings soltas na lista violam o contrato V1 expandida.
- **`allow_pending`:** opt-in explícito para métricas ainda em especificação no catálogo; sem isso, métricas incompletas devem falhar de forma controlada (código alinhado ao catálogo).
- **Output:** cada métrica pedida vira um ou mais `MetricValue`; validar paridade pedido/resposta e tipos de `shape` conforme catálogo de métricas.
- **Explícito vs implícito:** ausência de `params` significa “sem parâmetros adicionais”, não “use default mágico”.

### 3. `analyze_indicator_stability`

#### Finalidade

Comparar indicadores em uma janela e identificar quais apresentam menor volatilidade relativa.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "indicators": [
    { "name": "repeticao_concurso_anterior" },
    { "name": "frequencia_por_dezena", "aggregation": "mean" },
    { "name": "distribuicao_linha_por_concurso", "aggregation": "per_component" }
  ],
  "normalization_method": "madn",
  "top_k": 5,
  "min_history": 20
}
```

#### Regras

- Vetores e séries vetoriais exigem `aggregation`.
- Agregações aceitas: `mean | max | l2_norm | per_component`.
- `per_component` retorna múltiplas entradas no ranking, uma por componente.
- `normalization_method` default: `madn`.
- `coefficient_of_variation` só é aceito para séries positivas.

#### Semântica e validação

- **Objetivo:** comparar *estabilidade relativa* entre indicadores na mesma janela, não “qual indicador é melhor para apostar”.
- **`indicators`:** cada item precisa de `aggregation` quando a série subjacente for vetorial ou multivalor; sem isso → `UNSUPPORTED_AGGREGATION` ou `INCOMPATIBLE_INDICATOR_FOR_STABILITY`.
- **`normalization_method`:** `madn` (mediana absoluta normalizada) reduz sensibilidade a outliers extremos na comparação de volatilidade; outro método só se suportado e compatível com os sinais dos dados.
- **`top_k` / `min_history`:** limitam o ranking e exigem histórico mínimo para estatística estável. Se `min_history` for **estritamente maior** que a quantidade de concursos resolvida na janela, a tool **deve** falhar com `INSUFFICIENT_HISTORY` e `details` que incluam, quando possível, `min_history` requisitado e tamanho efetivo da janela; não devolver ranking “meio vazio” sem indicação de regra, conforme [ADR 0006 D4](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md). O default exemplificado de `20` no contrato é **ilustrativo**; o cliente ajusta à janela.

##### Catálogo fechado de `normalization_method`

Valores aceitos:

- `madn` — `MAD / mediana` (robusto). Quando a mediana for 0, o servidor deve usar fallback robusto declarado (ex.: `IQR / |mediana + ε|`) para evitar divisão por zero.
- `coefficient_of_variation` — `σ/μ` apenas para séries estritamente positivas; caso contrário → `UNSUPPORTED_NORMALIZATION_METHOD`.

### 4. `compose_indicator_analysis`

#### Finalidade

Executar composições dinâmicas e declarativas entre indicadores para produzir ranking, filtragem, score composto ou perfil conjunto.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "target": "dezena",
  "operator": "weighted_rank",
  "components": [
    {
      "metric_name": "frequencia_por_dezena",
      "transform": "normalize_max",
      "weight": 0.4
    },
    {
      "metric_name": "atraso_por_dezena",
      "transform": "invert_normalize_max",
      "weight": 0.3
    },
    {
      "metric_name": "assimetria_blocos",
      "transform": "shift_scale_unit_interval",
      "weight": 0.3
    }
  ],
  "top_k": 10
}
```

#### Regras

- `target` aceito: `dezena | candidate_game | indicator`.
- `operator` aceito: `weighted_rank | threshold_filter | joint_profile | stability_rank`.
- Pesos são obrigatórios em `weighted_rank` e devem somar `1.0 ± 1e-9`.
- Transformações aceitas: `normalize_max`, `invert_normalize_max`, `rank_percentile`, `identity_unit_interval`, `one_minus_unit_interval`, `shift_scale_unit_interval`.
- Componentes incompatíveis com o `target` retornam `INCOMPATIBLE_COMPOSITION`.
- A tool não aceita fórmulas livres em texto.

#### Uso esperado

- cruzar frequência com ausência e blocos;
- ranquear dezenas persistentes;
- combinar slot, frequência e equilíbrio estrutural;
- produzir score composto reprodutível.

#### Semântica e validação

- **`target`:** unidade sobre a qual o operador atua (ex.: `dezena` ranqueia dezenas 1–25); componentes devem ser dimensionalmente compatíveis com esse alvo.
- **`operator`:** `weighted_rank` exige pesos explícitos que somam 1; `threshold_filter` e `joint_profile` seguem regras no schema da tool (sem texto livre).
- **`transform`:** mapeia cada componente para uma escala comparável antes de pesar; validar que só entram transformações listadas em **Regras**. Ver “Semântica das transformações” abaixo.
- **Erros esperados:** pesos fora da tolerância, targets incompatíveis ou métricas desconhecidas → códigos da tabela de erros (`INCOMPATIBLE_COMPOSITION`, `UNKNOWN_METRIC`, etc.).

##### Semântica das transformações (`transform`)

Transformações são funções determinísticas que recebem um vetor/série/score e produzem valores comparáveis (geralmente em \([0,1]\)).

- `normalize_max`: para valores não negativos, `x' = x / max(x)`. Se `max(x) = 0`, retornar 0 para todos os itens.
- `invert_normalize_max`: `x' = 1 - normalize_max(x)`. Usada para “quanto menor melhor” (ex.: atraso).
- `rank_percentile`: converte para percentil por ranking (0..1). Empates usam ranking estável: ordenar por valor e desempatar por id canônico (ex.: dezena asc).
- `identity_unit_interval`: exige que o input já esteja em \([0,1]\); fora do intervalo → `INVALID_REQUEST` (ou erro específico equivalente).
- `one_minus_unit_interval`: `x' = 1 - x` com input em \([0,1]\).
- `shift_scale_unit_interval`: reescala linearmente para \([0,1]\) usando min/max observados: `x' = (x - min) / (max - min)`. Se `max = min`, retornar 0.5 para todos os itens (caso constante) para evitar enviesar seleção.

### 5. `analyze_indicator_associations`

#### Finalidade

Medir associações entre séries de indicadores compatíveis.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "items": [
    { "name": "repeticao_concurso_anterior" },
    { "name": "quantidade_vizinhos_por_concurso" },
    { "name": "pares_no_concurso" },
    { "name": "entropia_linha_por_concurso" }
  ],
  "method": "spearman",
  "top_k": 5,
  "stability_check": {
    "method": "rolling_window",
    "subwindow_size": 20
  }
}
```

#### Regras

- Métodos aceitos: `spearman | pearson`.
- Séries vetoriais exigem `aggregation` antes da associação.
- O output deve separar magnitude da associação e estabilidade da associação.
- Interpretação jamais deve afirmar causalidade.

#### Semântica e validação

- **`items`:** séries temporais (por concurso) alinhadas na mesma janela; vetores por concurso precisam de `aggregation` no item antes de correlacionar.
- **`method`:** `spearman` é monotônica e robusta a outliers leves; `pearson` mede linearidade — escolha afeta interpretação, não “verdade causal”.
- **`stability_check`:** mede se a associação se mantém em subjanelas (ex.: rolante); validar que o output distingue “força da correlação” de “consistência ao longo do tempo”.
- **Invariante de linguagem:** textos explicativos descrevem co-movimento estatístico, não “uma métrica causa a outra”.

##### Semântica de `stability_check`

- `stability_check.method` (enum fechado): `rolling_window`.
- `stability_check.subwindow_size`: tamanho da subjanela (inteiro \(\ge 2\) e \(\le window_size\)).

**Semântica:** para `rolling_window`, o servidor calcula a associação em cada subjanela rolante (passo 1) e retorna:

- uma estatística de estabilidade (ex.: MADN dos valores de correlação nas subjanelas, ou proporção de subjanelas com mesmo sinal), declarada no payload;
- contagem de subjanelas avaliadas;
- separação explícita entre “magnitude global na janela” e “estabilidade em subjanelas”.

**`stability_check` não implementado nesta build:** se o request inclui `stability_check` e a build **não** implementa a camada de estabilidade, a resposta **deve** ser erro `UNSUPPORTED_STABILITY_CHECK` (ver tabela de erros), e não sucesso com campos de estabilidade nulos/omitidos *sem* sinal claro. Se o request **omitir** `stability_check`, a resposta fornece a magnitude global da associação; campos de estabilidade de subjanela permanecem omitidos ou, se o schema reservar campo, o contrato de tipagem é o da versão de tool. Ver [ADR 0006 D2](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md).

### 6. `summarize_window_patterns`

#### Finalidade

Resumir padrões dominantes, faixas típicas, cobertura percentual e eventos raros em uma janela.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "features": [
    { "metric_name": "quantidade_vizinhos_por_concurso" },
    { "metric_name": "sequencia_maxima_vizinhos_por_concurso" },
    { "metric_name": "pares_no_concurso" },
    { "metric_name": "entropia_linha_por_concurso" }
  ],
  "coverage_threshold": 0.8,
  "range_method": "iqr"
}
```

#### Regras

- A tool produz moda, percentis, cobertura, faixa típica, outliers e texto explicativo.
- Para `distribuicao_linha_por_concurso` e `distribuicao_coluna_por_concurso`, o payload deve declarar `aggregation = per_component` ou `aggregation = mode_vector`.
- Pode responder perguntas do tipo "80% dos sorteios tiveram qual característica?".

#### Semântica e validação

- **`features`:** métricas por concurso (ou agregáveis) cuja distribuição na janela será resumida; devem estar no catálogo de features suportadas para esta tool.
- **`coverage_threshold`:** ex.: `0.8` liga respostas ao tipo “em pelo menos 80% dos concursos da janela…”; validar contagem explícita no output.
- **`range_method`:** define “faixa típica” (ex.: IQR); deve constar no payload ou anexo para cumprir o invariante de termos definidos. Ver “Semântica de faixa típica” abaixo.
- **Vetores de linha/coluna:** exigem `aggregation` declarada para não haver ambiguidade na moda/percentis agregados.

##### Semântica de faixa típica (`range_method`)

`range_method` é um enum fechado.

- `iqr`: define a faixa típica como \([Q1, Q3]\), com `IQR = Q3 - Q1`. Outliers podem ser reportados como fora de \([Q1 - 1.5·IQR, Q3 + 1.5·IQR]\); se o servidor reportar outliers, deve declarar o limiar usado no payload.

**Validação:** o output deve declarar explicitamente `Q1`, `median`, `Q3`, `IQR`, cobertura observada e contagens (total e outliers quando aplicável).

### 6.5. `summarize_window_aggregates`

#### Finalidade

Produzir **agregados canônicos** (histogramas, padrões e matrizes) a partir de métricas canônicas na mesma janela, sem retornar payload acoplado a UI.

Esta tool é normatizada em [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md).

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "aggregates": [
    {
      "id": "pairs_histogram",
      "source_metric_name": "pares_no_concurso",
      "aggregate_type": "histogram_scalar_series",
      "params": {
        "bucket_spec": { "bucket_values": [0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15] },
        "include_ratios": true
      }
    },
    {
      "id": "row_pattern_topk",
      "source_metric_name": "distribuicao_linha_por_concurso",
      "aggregate_type": "topk_patterns_count_vector5_series",
      "params": { "top_k": 10, "include_ratios": true }
    }
  ]
}
```

#### Regras

- `aggregates` é obrigatório e deve ser não vazio.
- `aggregate_type` é enum fechado, com apenas:
  - `histogram_scalar_series`
  - `topk_patterns_count_vector5_series`
  - `histogram_count_vector5_series_per_position_matrix`
  Valores fora da lista → `UNSUPPORTED_AGGREGATE_TYPE`.
- A tool opera em **batch**: múltiplos agregados no mesmo request para reduzir round-trips.
- Proibição de defaults semânticos ocultos: bucketização e dimensões de matriz devem ser declaradas no request.
- Cada item de `aggregates[]` deve declarar `source_metric_name`, `aggregate_type` e `params` (objeto). O servidor não infere `params` ausentes.

#### Semântica e validação

- **`source_metric_name`:** nome canônico no catálogo. Se o nome não existir → `UNKNOWN_METRIC`.  
  Se o nome existir no catálogo, mas a build ainda não disponibilizar a métrica necessária para compor o agregado nesta rota, a tool pode responder `UNKNOWN_METRIC` com `details` coerentes com a política do projeto (ver ADR 0006 D1).
- **Compatibilidade de shape:** o servidor valida se o `aggregate_type` é compatível com o `shape`/`scope` da métrica fonte; incompatibilidade → `UNSUPPORTED_SHAPE` (ou erro equivalente documentado na tabela de erros).
- **Ordem canônica:** a resposta preserva a ordem de `aggregates` do request; dentro de cada agregado aplica a ordenação canônica do tipo.

##### `aggregate_type = histogram_scalar_series`

- **Fonte:** métrica `scope="series"` e `shape="series"`.
- **Parâmetros:** `params.bucket_spec` é obrigatório:
  - `{ "bucket_values": [...] }` (discreto), ou
  - `{ "min": <number>, "max": <number>, "width": <number> }` (contínuo/discretizado).
- **Validação de parâmetros:** `bucket_spec` deve usar **exatamente um** modo (discreto ou contínuo). Mistura de modos ou ausência de campos obrigatórios → `INVALID_REQUEST`.
- **Output:** `buckets[]` ordenados por `x` ascendente, cada um com `{ x, count, ratio? }`.

##### `aggregate_type = topk_patterns_count_vector5_series`

- **Fonte:** métrica `shape="series_of_count_vector[5]"`.
- **Parâmetros:** `params.top_k` obrigatório.
- **Validação de parâmetros:** `top_k` deve ser inteiro `>= 1`; caso contrário → `INVALID_REQUEST`.
- **Output:** `items[]` com `{ pattern:[5], count, ratio? }`, ordenados por:
  1) `count desc`
  2) `pattern` lexicográfico asc (desempate determinístico)

##### `aggregate_type = histogram_count_vector5_series_per_position_matrix`

- **Fonte:** métrica `shape="series_of_count_vector[5]"`.
- **Parâmetros:** `params.value_min` e `params.value_max` obrigatórios; definem o eixo de valores e as dimensões.
- **Validação de parâmetros:** `value_min` e `value_max` devem ser inteiros com `value_min <= value_max`; caso contrário → `INVALID_REQUEST`.
- **Output:** `matrix[5][K]` (matriz cheia) com contagens por posição `1..5` × valor `value_min..value_max`, onde `K = value_max - value_min + 1`.
- **Ordenação canônica dos eixos:** linhas em `position` asc (`1..5`) e colunas em `value` asc (`value_min..value_max`).

### 7. `generate_candidate_games`

#### Finalidade

Gerar jogos candidatos a partir de estratégias nomeadas, com **modo de geração explícito** (aleatório sem guardrails não declarados *vs.* condicionado a comportamentos declarados na janela). Restrições combinam-se por **interseção** (ver abaixo). O sistema permanece **descritivo** relativamente ao histórico analisado; não há promessa de maior probabilidade de acerto no sorteio oficial ([ADR 0020 D5](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)).

#### `generation_mode` (obrigatório no request)

Enum fechado no topo do pedido (irmão de `window_size`, `plan`, etc.):

| Valor | Intenção | Comportamento normativo |
|-------|----------|-------------------------|
| `random_unrestricted` | Candidatos no espaço válido da Lotofácil (15 dezenas distintas em `[1..25]`, ordenação canónica) **sem** guardrails estruturais nem critérios de aderência a padrões históricos **não declarados** | **Proibido** aplicar `structural_exclusions`, critérios em `plan[].criteria` ou filtros em `plan[].filters` que **não** constem explicitamente no JSON. **Não** aplicar defaults conservadores “de oficina” (vizinhos, entropia mínima, alinhamento de slot, etc.) só porque o campo foi omitido. |
| `behavior_filtered` | Candidatos **condicionados** ao que o cliente declarar sobre comportamentos na janela | Aplica-se **somente** o declarado: critérios, `plan[].filters`, faixas (`range`, `typical_range`, `allowed_values` conforme [ADR 0019](adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md)), pesos, estratégias e `structural_exclusions` enviados; **proibido** introduzir guardrails não solicitados. Defaults numéricos **só** quando documentados e ecoados em `applied_configuration.resolved_defaults` ([ADR 0017](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md), [ADR 0020 D2](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)). |

**Transparência:** o modo efetivo (se no futuro puder ser inferido de um subconjunto mínimo de campos) deve sempre aparecer em `applied_configuration.resolved_defaults` quando a implementação materializar defaults.

#### Opt-in de `structural_exclusions`, critérios e filtros ([ADR 0020 D2](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md))

- **`structural_exclusions`:** objeto opcional; cada chave/limiar só tem efeito se **explicitamente** presente no request (ou resolvido de um valor declarado e ecoado — **nunca** por suposição silenciosa).
- **`plan[].criteria` / `plan[].filters`:** **opt-in** por item; omissão significa ausência daquela restrição, não “modo conservador” implícito.
- Em `random_unrestricted`, listas vazias ou omissão de blocos de restrição equivalem a **nenhuma** restrição além das regras do jogo e `global_constraints` (ex.: `unique_games`).

#### Interseção de restrições ([ADR 0020 D4](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md))

- Para cada restrição declarada \(i\), seja \(S_i\) o conjunto de jogos válidos que a satisfaz (critério *hard*, filtro estrutural, faixa em métrica de perfil, `structural_exclusions`, etc., conforme o schema da tool e [generation-strategies.md](generation-strategies.md)).
- Salvo modo alternativo **explicitamente** nomeado noutro documento normativo (ex.: estratégia ou perfil cuja semântica própria seja união ou agregação distinta — hoje **fora** do recorte padrão), o conjunto admissível é \(S = \bigcap_i S_i\).
- Um candidato só é aceite se pertencer a **\(S\)**. Com `global_constraints.unique_games: true`, o `count` do plano refere-se a candidatos **distintos** em \(S\) (salvo modo documentado de amostragem com reposição).
- Compor várias faixas ou critérios **restringe** o espaço a uma região **descritiva** do histórico; **não** implica maior “chance” no sorteio futuro.

#### `seed` e `replay_guaranteed` (resposta)

- Ver subsecção *`deterministic_hash` e `replay_guaranteed`* em *Canonização e metadados comuns*. O nome canónico do campo de “replay não garantido” é **`replay_guaranteed`** (`false` = episódio não replayável para a lista de candidatos).

#### Orçamento por pedido ([ADR 0020 D6](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md))

- A **soma** dos `count` em todos os itens de `plan[]` num único request **não pode exceder 1000**; violação → `INVALID_REQUEST` (ou código dedicado quando existir), com `details` que orientem a **dividir em várias rodadas** se for necessário mais volume.
- Mantém-se, além disso, `MAX_COUNT_PER_STRATEGY` por item conforme [generation-strategies.md](generation-strategies.md) até harmonização explícita.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "generation_mode": "behavior_filtered",
  "seed": 424242,
  "plan": [
    { "strategy_name": "common_repetition_frequency", "count": 3 },
    {
      "strategy_name": "declared_composite_profile",
      "count": 3,
      "search_method": "sampled",
      "profile": {
        "components": [
          { "name": "freq_alignment", "weight": 0.35 },
          { "name": "slot_alignment", "weight": 0.25 },
          { "name": "row_entropy_norm", "weight": 0.15 },
          { "name": "neighbors_balance_score", "weight": 0.15 },
          { "name": "pairs_balance_score", "weight": 0.10 }
        ]
      }
    }
  ],
  "global_constraints": {
    "unique_games": true,
    "sorted_numbers": true
  },
  "structural_exclusions": {
    "max_consecutive_run": 8,
    "max_neighbor_count": 7,
    "min_row_entropy_norm": 0.82,
    "max_hhi_linha": 0.30,
    "min_slot_alignment": 0.08
  }
}
```

Exemplo (ADR 0019) — **restrições flexíveis** por faixa (`range`), conjunto discreto (`allowed_values`), `mode`, `typical_range` e orçamento:

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "generation_mode": "behavior_filtered",
  "seed": 424242,
  "generation_budget": {
    "max_attempts": 20000
  },
  "plan": [
    {
      "strategy_name": "declared_composite_profile",
      "count": 10,
      "search_method": "sampled",
      "tie_break_rule": "outlier_score_asc_then_hhi_linha_asc_then_lexicographic_numbers_asc",
      "criteria": [
        { "name": "pairs_count", "range": { "min": 7, "max": 10 }, "mode": "hard" },
        { "name": "neighbors_count", "allowed_values": { "values": [8, 9, 10] }, "mode": "hard" },
        {
          "name": "repeat_count",
          "typical_range": { "metric_name": "repeticao_concurso_anterior", "method": "iqr", "coverage": 0.8, "params": {} },
          "mode": "hard"
        },
        { "name": "top10_overlap_count", "range": { "min": 5, "max": 9 }, "mode": "soft" }
      ],
      "weights": [
        { "name": "freq_alignment", "weight": 0.4 },
        { "name": "repeat_alignment", "weight": 0.2 },
        { "name": "slot_alignment", "weight": 0.2 },
        { "name": "row_entropy_norm", "weight": 0.1 },
        { "name": "hhi_linha_inverse", "weight": 0.1 }
      ],
      "filters": [
        { "name": "max_consecutive_run", "value": 8 },
        { "name": "max_neighbor_count", "value": 11 },
        { "name": "min_row_entropy_norm", "value": 0.82 }
      ]
    }
  ],
  "global_constraints": {
    "unique_games": true,
    "sorted_numbers": true
  }
}
```

Exemplo mínimo — **`random_unrestricted`** (sem `structural_exclusions` nem `plan[].criteria` / `filters`; **sem** `seed` ⇒ esperar `replay_guaranteed: false` se a estratégia for estocástica). O modo **não** remove filtros **intrínsecos** da estratégia em [generation-strategies.md](generation-strategies.md); remove apenas guardrails transversais **não declarados** em `structural_exclusions` / critérios globais ([ADR 0020 D1–D2](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)).

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "generation_mode": "random_unrestricted",
  "plan": [{ "strategy_name": "row_entropy_balance", "count": 5 }],
  "global_constraints": { "unique_games": true, "sorted_numbers": true }
}
```

#### Regras

- **`generation_mode`:** obrigatório; valores `random_unrestricted` \| `behavior_filtered` (semântica na tabela acima; norma completa em [ADR 0020 D1](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)).
- **`seed`:** **obrigatória** se o cliente quiser `replay_guaranteed: true` sempre que a cadeia de geração incluir estocástica (`sampled`, `greedy_topk` com aleatoriedade documentada, etc.). Com `seed` ausente nesses caminhos, a resposta **deve** declarar `replay_guaranteed: false` quando aplicável; o servidor **não** promete a mesma lista de candidatos em reinvocações (ver *Canonização*, *`deterministic_hash` e `replay_guaranteed`*).
- **`MAX_COUNT_PER_STRATEGY`:** conforme [generation-strategies.md](generation-strategies.md). **Soma de `plan[].count`:** máximo **1000** por pedido ([ADR 0020 D6](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)); exceder → `INVALID_REQUEST` (ou código dedicado documentado) com orientação para várias rodadas.
- Estratégia desconhecida retorna `UNKNOWN_STRATEGY`.
- Quando a estratégia admitir mais de um `search_method` no contrato da própria estratégia, o item correspondente em `plan` deve declarar `search_method` explicitamente.
- `declared_composite_profile` só aceita componentes listados em [generation-strategies.md](generation-strategies.md).
- **`structural_exclusions`:** opcionais e **opt-in** por campo; quando presentes, entram na interseção \(S\) e no material de hash conforme `replay_guaranteed` (ver *Canonização*).
- O servidor continua não aceitando "pesos soltos" fora de um schema explícito.
- **Ranges e multi-valores (ADR 0019):** quando o request declarar restrições por `range` e/ou `allowed_values`, isso define um **conjunto válido** de candidatos; o servidor deve retornar **qualquer** lote que satisfaça as restrições dentro da política de `replay_guaranteed` (ou falhar com `STRUCTURAL_EXCLUSION_CONFLICT` com `available_count`).
- **Sem inferência:** o servidor não “descobre” faixas por conta própria. Se o cliente quiser faixa típica por método estatístico, deve declarar explicitamente via `typical_range` (ADR 0019) e o servidor deve ecoar `resolved_range`/`coverage_observed` em `applied_configuration.resolved_defaults`.

#### Semântica e validação

- **`plan`:** fila de estratégias com `count` cada; a soma dos `count` **≤ 1000** por pedido e cada item **≤ `MAX_COUNT_PER_STRATEGY`**.
- **`plan[].search_method`:** só aparece no request quando o contrato da estratégia permite múltiplos métodos de busca. Em `declared_composite_profile`, é obrigatório e deve ser `sampled` ou `greedy_topk`; nas estratégias com método fixo em [generation-strategies.md](generation-strategies.md), o request não repete esse campo.
- **`seed`:** necessária para golden tests e demos que exijam lista idêntica de candidatos; sem `seed` em trajectória estocástica, validar `replay_guaranteed: false` e semântica de `deterministic_hash` **sem** igualdade da lista.
- **`global_constraints`:** `unique_games` evita duplicatas no lote; `sorted_numbers` padroniza representação dos jogos.
- **`structural_exclusions`:** filtros duros **somente se declarados**; compõem \(S\) por interseção; quando presentes, fazem parte do input canônico relevante para hash/replay conforme política acima e devem aparecer no output (metadados ou explicação).
- **`declared_composite_profile`:** subscores e pesos só podem usar nomes permitidos em [generation-strategies.md](generation-strategies.md); soma dos pesos do perfil deve seguir a mesma regra de tolerância que composições (`1.0 ± 1e-9`) salvo especificação divergente no doc de estratégias.
- **Restrições flexíveis (ADR 0019):** critérios/filtros podem ser declarados como:
  - valor absoluto (`value`, `min`, `max`);
  - `range: {min,max,inclusive?}`;
  - `allowed_values: {values:[...]}`;
  - `typical_range: {metric_name, method, coverage, params...}` (faixa típica calculada na janela).
  Mistura de modos no mesmo item deve retornar `INVALID_REQUEST`.
- **`mode`:** quando suportado pela estratégia/validador, `mode="hard"` rejeita candidatos fora do conjunto; `mode="soft"` mantém o candidato e aplica penalidade determinística (ADR 0019). O default, se existir, deve aparecer em `applied_configuration.resolved_defaults`.
- **Orçamento de geração (ADR 0019):** quando exposto no request (ex.: `generation_budget.max_attempts`), o servidor deve ecoar `attempts_used`, `accepted_count` e uma forma agregada determinística de rejeições por motivo em `applied_configuration.resolved_defaults`.

### 8. `explain_candidate_games`

#### Finalidade

Explicar por que os jogos foram gerados e por que outros perfis foram descartados.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "games": [
    [1,3,4,5,7,8,10,11,13,15,17,18,20,22,24]
  ],
  "include_metric_breakdown": true,
  "include_exclusion_breakdown": true
}
```

#### Regras

- `candidate_strategies` é ordenado por score decrescente.
- Quando houver exclusões estruturais, a tool deve informar quais filtros o jogo respeitou.
- `metric_breakdown` sempre traz `metric_version`.

#### Semântica e validação

- **Entrada `games`:** jogos já formados (ex.: saída de `generate_candidate_games` ou lista manual); a tool não altera dezenas, apenas explica aderência e exclusões.
- **`include_metric_breakdown`:** liga o detalhamento por métrica/subscore com versões para auditoria e regressão.
- **`include_exclusion_breakdown`:** para cada exclusão estrutural relevante, informa se o jogo passou ou falhou e por quê (suporte ao invariante 9).
- **`candidate_strategies`:** ordenação por score decrescente define a interpretação “estratégias mais aderentes a este cartão primeiro”, sem implicar probabilidade de sorteio.

## Estratégias V1

Definição canônica em [generation-strategies.md](generation-strategies.md).

Cada nome listado abaixo é um **identificador fechado**: o servidor só gera jogos por estratégias documentadas, com versão semver (`@x.y.z`). Isso evita “estratégia inventada pelo prompt”. Para validar: (1) `generate_candidate_games` com nome fora da lista → `UNKNOWN_STRATEGY`; (2) parâmetros da estratégia batem com o schema em `generation-strategies.md`; (3) o par `@version` retornado em `CandidateGame` coincide com a versão implementada.

Estratégias V1:

- `common_repetition_frequency@1.0.0`
- `row_entropy_balance@1.0.0`
- `slot_weighted@1.0.0`
- `outlier_candidate@1.0.0`
- `declared_composite_profile@1.0.0`

## Erros de contrato

Os códigos abaixo são parte do contrato público: clientes e testes devem poder fazer asserções sobre eles. O campo `message` é texto humano (pode variar); `code` e `details` são o contrato estável para automação.

**Nota operacional — feature toggles de acesso:** `UNAUTHORIZED`, `RATE_LIMITED` e `QUOTA_EXCEEDED` permanecem no contrato mesmo quando autenticação, throttling e quotas estiverem desabilitados na V0/V1 inicial. Nessa fase, esses códigos são **reservados**; a obrigatoriedade de testes negativos para eles passa a valer quando o respectivo mecanismo estiver habilitado por configuração.

**Validação geral:** para cada código **ativo na configuração do ambiente**, existir pelo menos um teste negativo que provoque o código e um caminho feliz que não o emita; `details` deve carregar pistas estruturadas (`missing_field`, `metric_name`, etc.) sem vazar segredos. Para códigos reservados por `feature toggle`, a documentação e a configuração devem deixar explícito que o mecanismo está desligado.

Formato sugerido:

```json
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Campo obrigatório ausente.",
    "details": {
      "missing_field": "metrics"
    }
  }
}
```

| Código | Descrição | Ferramentas que podem emitir |
|--------|-----------|------------------------------|
| `INVALID_REQUEST` | Schema inválido, campo ausente ou tipo errado | todas |
| `INVALID_WINDOW_SIZE` | `window_size` não positivo | tools com janela |
| `INVALID_CONTEST_ID` | `end_contest_id` ausente do dataset | tools com janela |
| `INVALID_REFERENCE_WINDOW` | janela de referência incompatível | composição, padrões, associações |
| `UNKNOWN_METRIC` | métrica não listada no catálogo | métricas, estabilidade, composição, associações |
| `UNKNOWN_STRATEGY` | estratégia não listada em [generation-strategies.md](generation-strategies.md) | geração |
| `UNSUPPORTED_AGGREGATION` | agregação obrigatória ausente ou inválida | estabilidade, composição, associações, padrões |
| `UNSUPPORTED_TRANSFORM` | transformação de composição não suportada | composição, geração |
| `UNSUPPORTED_NORMALIZATION_METHOD` | método incompatível com a série | estabilidade |
| `UNSUPPORTED_ASSOCIATION_METHOD` | método de associação não suportado | associações |
| `UNSUPPORTED_STABILITY_CHECK` | cálculo de `stability_check` em subjanelas ainda não disponível nesta build, embora o request o tenha declarado; magnitude global é obrigatória no caminho de sucesso quando a estabilidade não for pedida | associações |
| `UNSUPPORTED_PATTERN_FEATURE` | feature não suportada em resumo de padrões | padrões |
| `UNSUPPORTED_AGGREGATE_TYPE` | tipo de agregado fora do enum fechado da tool de agregados | agregados |
| `UNSUPPORTED_SHAPE` | shape/scope da métrica fonte incompatível com o agregado solicitado | agregados |
| `INCOMPATIBLE_INDICATOR_FOR_STABILITY` | indicador sem shape compatível para ranking | estabilidade |
| `INCOMPATIBLE_COMPOSITION` | componentes incompatíveis entre si ou com o target | composição, geração |
| `STRUCTURAL_EXCLUSION_CONFLICT` | exclusões tornam o plano inviável ou contraditório | geração |
| `INSUFFICIENT_HISTORY` | histórico insuficiente para a janela pedida | tools com janela |
| `PLAN_BUDGET_EXCEEDED` | `count` acima do orçamento permitido | geração |
| `NON_DETERMINISTIC_CONFIGURATION` | pedido logicamente inconsistente com replay prometido (ex.: exigência implícita ou explícita de `replay_guaranteed: true` sem `seed` onde a estratégia exige semente) | geração |
| `UNAUTHORIZED` | credencial ausente ou inválida | todas |
| `RATE_LIMITED` | throttling | todas |
| `QUOTA_EXCEEDED` | quota excedida | todas |
| `DATASET_UNAVAILABLE` | dataset indisponível | todas |
| `INTERNAL_ERROR` | erro raro e rastreável por hash | todas |

## Glossário mínimo (termos de linguagem definida)

Esta seção fecha os termos citados no invariante 5. Respostas em `explanation`/`rationale` devem usar esses termos apenas com o significado abaixo (ou declarar definição alternativa no próprio payload).

### Finalidade

Evitar jargão ambíguo e permitir validação e auditoria por cliente/testes.

### Input

N/A (documentação). Opcionalmente, ferramentas podem retornar um campo `definitions` no output para ecoar as definições relevantes do request.

### Regras

- Definições são descritivas; não podem implicar previsão.
- Quando o termo se refere a uma métrica, deve haver referência ao nome canônico da métrica.

### Observações

- Este glossário não substitui o catálogo de métricas; ele define o vocabulário mínimo usado no texto explicativo.

### Semântica e validação

- `slot`: posição ordenada \(s=1..15\) após ordenar ascendente as dezenas de um concurso; não é ordem física do sorteio nem posição no volante.
- `outlier`: item/jogo/valor distante do comportamento típico na janela declarada, definido por uma regra estatística explícita (ex.: alto `outlier_score` ou fora da faixa típica por `range_method`).
- `persistência`: regularidade observada no histórico declarado (janela/histórico), sem qualquer promessa de ocorrência futura.
- `equilíbrio`: aderência a uma faixa típica declarada (ex.: pares/ímpares, entropia, HHI, vizinhos), não “melhor chance”.
- `faixa típica`: intervalo definido por `range_method` (ex.: `iqr` ⇒ \([Q1,Q3]\)).
- `correlação`: associação estatística (Spearman/Pearson) entre séries alinhadas; não implica causalidade.

### Grupos de erros (leitura rápida para validação)

- **Request e dados de entrada:** `INVALID_REQUEST`, `INVALID_WINDOW_SIZE`, `INVALID_CONTEST_ID`, `INVALID_REFERENCE_WINDOW` — falha antes ou semântica inválida do recorte; validar JSON Schema e existência de concursos.
- **Catálogo fechado e rotas parciais:** `UNKNOWN_METRIC` numa rota, `UNKNOWN_STRATEGY` na geração, `UNSUPPORTED_STABILITY_CHECK` (associações) — ajustar o request, estender o **catálogo/ADR** e alinhar a build; o servidor não adivinha nomes, nem responde sucesso com `stability_check` faltando quando o cliente o pediu, conforme [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md).
- **Compatibilidade dimensional:** `INCOMPATIBLE_INDICATOR_FOR_STABILITY`, `INCOMPATIBLE_COMPOSITION` — combinação de métricas, `target` ou agregações não faz sentido matematicamente para aquela tool.
- **Geração e orçamento:** `STRUCTURAL_EXCLUSION_CONFLICT`, `PLAN_BUDGET_EXCEEDED`, `NON_DETERMINISTIC_CONFIGURATION` — plano impossível, violação de orçamento de `count`, ou configuração inconsistente com **replay** prometido (não confundir com `seed` opcional quando `replay_guaranteed: false` é válido, [ADR 0020 D7](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)).
- **Infraestrutura e limites:** `INSUFFICIENT_HISTORY`, `DATASET_UNAVAILABLE`, `UNAUTHORIZED`, `RATE_LIMITED`, `QUOTA_EXCEEDED`, `INTERNAL_ERROR` — dados, credenciais, política de uso ou bug encapsulado; `INTERNAL_ERROR` deve ser raro e com identificador de suporte (hash) quando aplicável.

## Requisitos de persistência e cache

O contrato exige:

1. leitura canônica consistente por concurso e por janela;
2. capacidade de recalcular métricas a partir do histórico bruto;
3. rastreabilidade da versão de dados usada em cada resposta;
4. invalidação ou versionamento explícito ao mudar o dataset.

**Para validar:** (1) duas leituras do mesmo `contest_id` retornam o mesmo `Draw`; (2) após correção de dados, `dataset_version` incrementa ou invalida cache conforme política; (3) toda resposta de tool expõe a versão efetiva consumida, alinhada ao invariante de reprodutibilidade.

## Testes mínimos para considerar o contrato viável

Cada item abaixo é um critério de aceite binário: sem ele, a implementação não cumpre o espírito do contrato, mesmo que compile.

1. Mesmo input + mesmo `dataset_version` retornam o mesmo `deterministic_hash` **salvo** a política específica de `generate_candidate_games` com `replay_guaranteed: false` (hash estável **sem** exigir igualdade da lista de candidatos).
2. `compute_window_metrics` retorna valores idênticos em execuções repetidas, com `shape` explícito.
3. `analyze_indicator_stability` rejeita vetoriais sem `aggregation` e usa `madn` por default.
4. `compose_indicator_analysis` rejeita pesos que não somam 1 e componentes incompatíveis.
5. `analyze_indicator_associations` rejeita associação sem redução explícita de série vetorial; com `stability_check` no request e build sem suporte à estabilidade em subjanelas, emite `UNSUPPORTED_STABILITY_CHECK` (ADR 0006 D2).
6. `summarize_window_patterns` calcula cobertura, moda e faixa típica de forma determinística.
7. `summarize_window_aggregates` valida enum fechado de `aggregate_type`, parâmetros obrigatórios por tipo, compatibilidade de shape e ordenação/desempates canônicos de forma determinística.
8. `generate_candidate_games` respeita `generation_mode`, interseção de restrições, orçamento (soma **≤ 1000**), `replay_guaranteed`, política de `seed`/`deterministic_hash`, e estratégia composta declarada.
9. `explain_candidate_games` retorna ranking de estratégias e detalhamento de exclusões.
10. `divergencia_kl` nunca retorna `+∞` ou `NaN` para janelas `N >= 5`.
11. Toda família de prompt documentada em [prompt-catalog.md](prompt-catalog.md) deve ter ao menos um teste positivo e um negativo em [test-plan.md](test-plan.md).
12. Em pelo menos um fluxo de integração (E2E ou teste de agente), um pedido **não** mapeável sem lacunas deve resultar em esclarecimento com campos explícitos ou em `INVALID_REQUEST` **sem** execução com parâmetros supostos pelo modelo.

## Avaliação de viabilidade

### Viável em V1

- `get_draw_window`
- `compute_window_metrics`
- `analyze_indicator_stability`
- `compose_indicator_analysis`
- `analyze_indicator_associations`
- `summarize_window_patterns`
- `summarize_window_aggregates`
- `generate_candidate_games`
- `explain_candidate_games`

### Não entra na V1

- pesos textuais livres sem schema;
- `scope = draw` em `MetricValue`;
- narrativa preditiva não validada;
- fórmulas de composição não declaradas no payload.

## Recomendação técnica

A V1 completa e testável fica composta por:

1. histórico canônico local com `dataset_version` derivado;
2. as 9 tools acima;
3. catálogo de métricas fechado;
4. estratégias e filtros estruturais fechados;
5. catálogo de prompts de teste;
6. plano de testes cobrindo métricas, tools, composições, filtros, erros e prompts.
