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

Issue consolidada no repositório (backlog, critérios de aceite e mapa para ADRs existentes): [issues/issue-mcp-relatorio-completo-metricas-fluidez.md](issues/issue-mcp-relatorio-completo-metricas-fluidez.md).

## Anexo A — especificação anti-ambiguidade (UI, agentes e contrato)

Este anexo explicita **representações**, **layouts**, **índices** e **agregações** para reduzir ambiguidade e evitar que um agente “invente” interpretações.

### A.1 Definições normativas (termos)

- **Janela**: sequência de concursos \([start_contest_id..end_contest_id]\) com tamanho `window_size`.
- **Série**: valor por concurso na janela (tamanho = `window_size`, ordem cronológica).
- **Agregado de janela**: estatística/operador aplicado sobre uma série para produzir um escalar/vetor “resumo” (ex.: `sum`, `mean`, `median`, `min`, `max`, `std`, `iqr`).
- **Representação**: como o valor é serializado em JSON (p.ex. `array` flat vs matriz 2D). Representação **não altera** a semântica quando é uma transformação reversível (reshape).

### A.2 Regra de segurança para agentes (evitar alucinação)

Um agente **não pode** inferir “dimensões”, “ordem” ou “layout” de um `array` apenas pelo nome do shape. Se `value` for um `array` plano e o shape indicar multi-dimensionalidade, o contrato deve fornecer:

- `dims` (ex.: `[25,15]`)
- `layout` (ex.: `"row_major(dezena,slot)"`)
- regra de indexação (fórmula)

Sem isso, o cliente deve tratar como **payload opaco** e só exibir com uma legenda explícita (“vetor achatado”) ou chamar uma tool de formatação/sumarização.

### A.3 Padrão recomendado de “representação para UI”

Para métricas que retornam séries/estruturas grandes, o servidor deve oferecer **dois modos**:

- **Semântico canônico**: preserva a métrica como definida no catálogo (frequentemente uma série).
- **UI summary**: retorna um agregado determinístico por janela, com estatísticas mínimas e **sem perder rastreabilidade**.

Isso evita “ensinar” o consumidor a reimplementar estatística (onde nascem bugs e divergências entre clientes).

---

### A.4 Métricas confirmadas com representação difícil (antes/depois + definição técnica)

As subseções abaixo descrevem **o cenário atual (antes)**, **o desejado (depois)** e a semântica precisa.

#### A.4.1 `matriz_numero_slot` (`count_matrix[25x15]`)

- **Semântica (normativa)**:
  - Para cada concurso da janela, ordenar as 15 dezenas em ordem crescente.
  - Definir `slot` como a posição nessa ordenação (1..15).
  - Definir matriz \(M[d,s]\) com `d=1..25`, `s=1..15`, onde \(M[d,s]\) é a contagem de vezes que a dezena `d` ocupou o slot `s` na janela.
- **Estado atual (antes)**:
  - `value` é `array` flat de tamanho 375, com indexação:
    - `dezenaIndex = d - 1` (0..24)
    - `slotIndex = s - 1` (0..14)
    - `flatIndex = (dezenaIndex * 15) + slotIndex`
    - `value[flatIndex] == M[d,s]`

Exemplo (antes, shape atual):

```json
{
  "metric_name": "matriz_numero_slot",
  "shape": "count_matrix[25x15]",
  "value": [/* 375 números */]
}
```

- **Desejado (depois)** (duas opções; escolher uma como norma):
  - **Opção 1 (preferida para UI)**: `value` como `number[25][15]`:

```json
{
  "metric_name": "matriz_numero_slot",
  "shape": "count_matrix[25x15]",
  "value": [[/* 15 */], [/* 15 */], /* ... 25 linhas ... */]
}
```

  - **Opção 2 (mantém flat mas remove ambiguidade)**:

```json
{
  "metric_name": "matriz_numero_slot",
  "shape": "count_matrix[25x15]",
  "dims": [25, 15],
  "layout": "row_major(dezena,slot)",
  "value": [/* 375 */]
}
```

- **Transformação apenas na exibição?**
  - Sim, `flat -> 2D` é reversível (reshape). Não muda semântica.
- **Impacto em filtros / agente**
  - Não atrapalha. Para filtros, geralmente se usam features derivadas (`analise_slot`, `surpresa_slot`, concentração por slots).
  - Importante: agente deve tratar `slot` como posição **na ordenação crescente**, não “ordem do sorteio ao vivo”.

---

#### A.4.2 `distribuicao_linha_por_concurso` e `distribuicao_coluna_por_concurso` (`series_of_count_vector[5]`)

- **Semântica (normativa)**:
  - Para cada concurso da janela, produzir um vetor de 5 contagens, somando 15.
  - Linha: `rowIndex = floor((number-1)/5)` (0..4).
  - Coluna: `colIndex = (number-1) mod 5` (0..4).
- **Estado atual (antes)**:
  - `value` é `array` flat com tamanho `window_size * 5`, concatenando vetores por concurso.
  - Indexação:
    - `i` = índice do concurso (0..window_size-1)
    - `k` = componente da linha/coluna (0..4)
    - `value[(i*5)+k]` é a contagem da componente `k` no concurso `i`.

Exemplo (antes):

```json
{
  "metric_name": "distribuicao_linha_por_concurso",
  "shape": "series_of_count_vector[5]",
  "value": [2,3,3,4,3, 4,3,3,1,4 /* ... */]
}
```

- **Desejado (depois)**:
  - `value` como `number[window_size][5]` (lista de vetores por concurso), preservando ordem cronológica:

```json
{
  "metric_name": "distribuicao_linha_por_concurso",
  "shape": "series_of_count_vector[5]",
  "value": [
    [2,3,3,4,3],
    [4,3,3,1,4]
    /* ... window_size linhas ... */
  ]
}
```

  - Alternativa “flat sem ambiguidade”: adicionar `dims: [window_size, 5]` e `layout: "row_major(concurso,componente)"`.
- **Transformação apenas na exibição?**
  - Sim, reshape reversível.
- **Impacto em filtros / agente**
  - Não atrapalha; melhora. Agente tende a calcular agregados: soma por linha/coluna, médias, dispersão, etc.
- **Regra de UI para `count` em janela** (evitar “spam de série”):
  - Manter série como canônica, mas oferecer um “summary de janela”:
    - `sum_by_component[5]`, `mean_by_component[5]`, `min_by_component[5]`, `max_by_component[5]`.

---

#### A.4.3 `frequencia_blocos` e `ausencia_blocos` (`count_list_by_dezena`)

- **Semântica (normativa)**:
  - Para cada dezena `d`, produzir a lista de comprimentos dos blocos consecutivos:
    - `frequencia_blocos`: blocos de presença (`d` aparece).
    - `ausencia_blocos`: blocos de ausência (`d` não aparece).
- **Estado atual (antes)**:
  - `value` é uma lista codificada concatenando todas as dezenas:
    - `[dezena, block_count, block_1, ..., block_n]` repetido para `dezena=1..25`.

Exemplo (antes, trecho ilustrativo):

```json
{
  "metric_name": "frequencia_blocos",
  "shape": "count_list_by_dezena",
  "value": [1,2,3,1, 2,1,3 /* ... */]
}
```

- **Ambiguidade a remover**:
  - O consumidor precisa saber (e não “adivinhar”) que o encoding é exatamente:
    - `value[pos] = dezena`
    - `value[pos+1] = block_count`
    - `value[pos+2..pos+1+block_count] = blocks`
    - e que isso se repete para dezenas 1..25 em ordem crescente.
- **Desejado (depois)**:
  - Representação autoexplicativa, sem parsing posicional:

```json
{
  "metric_name": "frequencia_blocos",
  "shape": "count_list_by_dezena",
  "by_dezena": {
    "1": [3,1],
    "2": [3],
    "3": []
  }
}
```

  - Alternativa compacta e determinística para UI/SDK: `value` como `number[25][]` (array de 25 listas), com mapeamento explícito `index 0 => dezena 1`.
- **Transformação apenas na exibição?**
  - É possível, mas é onde mais nascem bugs: parsing posicional + listas variáveis.
  - Preferível padronizar no servidor com estrutura explícita.
- **Impacto em filtros / agente**
  - Não atrapalha; facilita. O agente normalmente não quer a lista crua; quer features derivadas:
    - `block_count`, `max_block`, `mean_block`, `p90_block`, `current_block`.

---

### A.5 Métricas `count` que hoje retornam como **série** (UI deve apresentar agregados)

**Motivo**: para UI “painel de janela”, exibir `window_size` números é ruído. Para filtros, um agregado é mais útil.

Confirmadas como série por concurso:

- `pares_no_concurso` (`series`, `unit=count`)
- `quantidade_vizinhos_por_concurso` (`series`, `unit=count`)
- `sequencia_maxima_vizinhos_por_concurso` (`series`, `unit=count`)
- (idem para qualquer outra métrica `scope=series` com `unit=count`)

**Desejado para UI**:

- retornar (ou disponibilizar via tool de sumarização) um resumo determinístico por janela:
  - `sum`, `mean`, `median`, `min`, `max`, `std` (ou MAD/MADN), `iqr`

**Impacto em filtros / agente**:

- Não atrapalha: agente usa agregados para filtros (ex.: `mean(vizinhos) <= X`).
- Série deve continuar disponível para análises de “mudança de regime” (quando pedida).

---

### A.6 Entropia e HHI em janela (representação recomendada)

Confirmadas como séries por concurso:

- `entropia_linha_por_concurso` (bits)
- `entropia_coluna_por_concurso` (bits)
- `hhi_linha_por_concurso` (dimensionless)
- `hhi_coluna_por_concurso` (dimensionless)

**Objetivo principal**:

- Entropia: uniformidade vs concentração (quanto maior, mais “uniforme” a distribuição).
- HHI: concentração (quanto maior, mais concentrado).

**Ambiguidade a evitar**:

- Não inventar “entropia da janela” como uma nova fórmula sem especificação.
- Não confundir “agregar a série” com “recomputar a métrica em um agregado de contagens da janela”.

**Proposta (normativa para UI e filtros)**:

- Tratar entropia/HHI como série canônica e expor um **resumo estatístico de janela**:
  - `mean`, `median`, `min`, `max`, `std` (ou MAD/MADN), `iqr`.
  - opcional: histograma com bins fixos determinísticos.

**Justificativa técnica**:

- preserva o objetivo: “nível típico” e “variabilidade” no período
- é determinístico e explicável
- torna filtros naturais (ex.: `mean(entropia_linha) >= 2.22`, `max(hhi_coluna) <= 0.24`)

**Opção alternativa (apenas se especificada)**:

- Agregar contagens por linha/coluna ao longo da janela e calcular uma entropia/HHI “global da janela”.
  - Atenção: isso mede outra coisa (distribuição total no período), não a forma por concurso.
  - Se adotado, deve ser uma **métrica nova** com nome/versão própria (para evitar alucinação e confusão).

---

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

---

## Anexo B — eficiência de tokens e UX conversacional (brief/detailed/full)

Este anexo captura requisitos e propostas para reduzir **consumo total de tokens** (prompt do agente + respostas MCP + explicação), com foco em ganhos **≥ 10%** por fluxo típico.

### B.1 Princípios normativos (para evitar alucinação e custos)

- **Separar semântica de apresentação**: reshape e agregações descritivas não criam novas métricas.
- **Descrição em linguagem natural deve ser “descritiva”**: sem promessas/predição; sempre ancorada na janela e nos valores retornados.
- **Economia por default**: modos “verbosos” devem ser opt-in e com limites (p.ex. paginação/encodings).
- **UI-first**: para painéis de janela, preferir resumos estatísticos e agregados determinísticos em vez de séries completas.

### B.2 Proposta: parâmetros transversais de economia

Adicionar parâmetros consistentes (onde fizer sentido) nas tools principais (`get_draw_window`, `compute_window_metrics`, `discover_capabilities`, `generate_candidate_games`, `explain_candidate_games`):

- `verbosity`: `"minimal" | "standard" | "full"`
  - `minimal`: sem explicações textuais, sem campos redundantes, payload compacto
  - `standard`: comportamento atual (ou próximo), com explicações curtas
  - `full`: inclui detalhes completos (podendo exigir paginação/encoding)
- `include_explanations`: `boolean` (default depende de `verbosity`; em `minimal` deve ser `false`)
- `fields` / `response_projection`: lista de campos a incluir (projeção server-side)
- `value_encoding`: `"flat" | "2d" | "sparse_triplets" | "rle"` (por shape)

Critérios de aceitação (gerais):

- `minimal` reduz o tamanho do payload e o consumo total de tokens em **≥ 10%** em cenários de referência.
- defaults não quebram compatibilidade (quando houver mudança, versionar tool ou parâmetro).

#### B.2.0 Diretriz de UX (leigos): “modo econômico” e “modo detalhado”

Objetivo: tornar o consumo intuitivo no chat (Cursor hoje; agente dedicado no futuro) sem exigir que o usuário saiba MCP/JSON.

**Regras normativas:**

- O nível de detalhe é sempre escolha de quem opera o chat: o host/agente deve mapear frases naturais do usuário para `verbosity` e parâmetros correlatos.
- O servidor permanece stateless; “preferência de modo” não é persistida no servidor, apenas no host (sessão do chat/agente).
- `verbosity` deve afetar principalmente:
  - presença/tamanho de texto explicativo (campos `explanation`, descrições extensas);
  - tamanho de listas grandes (via paginação determinística quando aplicável);
  - inclusão de campos redundantes.

**Mini‑guia de UX (frases prontas):**

- “**Use modo econômico a partir de agora**” → `verbosity="minimal"` (e `include_explanations=false`).
- “**Me dê só o essencial**” → `verbosity="minimal"` + `fields` com os campos mínimos (quando suportado).
- “**Agora quero detalhado**” → `verbosity="full"` (e `include_explanations=true`), podendo ativar paginação se necessário.
- “**Volte ao padrão**” → `verbosity="standard"`.
- “**Quero os números, sem texto**” → `verbosity="minimal"` + `include_explanations=false`.

**Descoberta e ajuda (para reduzir tentativa/erro):**

- `help` deve listar, em linguagem simples, o que muda em `minimal|standard|full` e exemplos de pedidos acima.
- `discover_capabilities` deve declarar explicitamente:
  - `supported_parameters.verbosity = ["minimal","standard","full"]`
  - `supported_parameters.include_explanations = ["false","true"]` (quando aplicável)
  - (quando implementado) `supported_parameters.fields` / `response_projection`
  - `recommended_defaults` (p.ex. `standard`) e recomendações de economia (“use minimal para respostas curtas no chat”).

**Nota de eficiência para MCP STDIO (Cursor chat):**

- Para evitar duplicação de tokens, a resposta deve privilegiar JSON em `StructuredContent` e manter `Content` como texto **curto** (resumo humano) conforme `verbosity` — nunca repetir o JSON completo em ambos os canais.

### B.2.1 Forma mais eficiente: tool direta para “últimos X concursos” (1 tool call)

**Motivação**: no contrato atual, `get_draw_window` exige `end_contest_id` (ou `start+end`). Para obter “último”/“últimos X”, o consumidor precisa primeiro descobrir o `latest_contest_id` do dataset (o que hoje não é exposto de forma pequena), gerando **mais tool calls** e mais tokens.

**Proposta (recomendada para velocidade)**: nova tool `get_latest_draws`.

- **Request (mínimo)**:
  - `count`: `integer` (1..N, com limite definido em spec; p.ex. 1..200)
  - `verbosity`: `"minimal" | "standard" | "full"` (default `"minimal"`)
  - `fields`: lista opcional para projeção (p.ex. `["draws.contest_id","draws.draw_date","draws.numbers","window"]`)
- **Semântica (normativa)**:
  - resolve internamente `latest_contest_id` do dataset carregado
  - retorna a janela canônica \([latest-count+1..latest]\), com concursos em ordem cronológica
  - em `verbosity="minimal"`, retorna apenas metadados e o essencial do draw (sem textos de explicação)
- **Response (mínimo)**:
  - `dataset_version`, `tool_version`, `deterministic_hash`
  - `window`: `{ size, start_contest_id, end_contest_id }`
  - `draws[]`: `{ contest_id, draw_date, numbers }`

**Por que é a mais eficiente**:

- reduz para **1** tool call o fluxo “último concurso / últimos X” (sem etapa de descoberta)
- elimina tentativa/erro com `end_contest_id`
- permite respostas curtas previsíveis (`verbosity="minimal"` + `fields`)

**Teste de contrato sugerido**:

- `get_latest_draws({count:10, verbosity:"minimal"})`:
  - `window.size == 10`
  - `draws.length == 10`
  - `draws[9].contest_id == window.end_contest_id`
  - determinismo: mesma fixture ⇒ mesma saída e `deterministic_hash`

### B.3 Tool: descrições das métricas do último concurso (brief/detailed/full)

#### B.3.1 Motivação

Fluxos naturais de chat:

- “Descreva brevemente as métricas do último jogo.”
- “Descreva de forma mais detalhada as métricas do último jogo.”
- “Descreva e traga as métricas detalhadas do último jogo.”

Objetivo: reduzir idas-e-voltas e evitar que o agente reescreva explicações longas.

#### B.3.2 Proposta A (recomendada): tool dedicada `describe_latest_metrics`

**Nova tool**: `describe_latest_metrics`

- **Entrada (mínimo)**:
  - `metrics`: lista opcional (se omitida, usa um “default brief set” definido em spec)
  - `detail_level`: `"brief" | "detailed" | "full"`
  - `include_values`: `boolean` (default `true` em `brief`, `true` em `detailed`, configurável em `full`)
  - `include_explanations`: `boolean` (default `false` em `brief`, `true` em `detailed/full`)
- **Saída (mínimo)**:
  - `window` resolvida (size=1) + metadados determinísticos
  - `items[]`: `{ metric_name, shape, scope, value?, description }`

**Contrato anti-alucinação**:

- `brief` e `detailed` devem usar descrições **templateadas/determinísticas** (sem LLM no servidor).
- `full` pode incluir descrições extensas, mas deve impor limites:
  - `max_chars_per_metric` (determinístico) e/ou paginação de conteúdo textual.

**Ganhos esperados**:

- ≥ 10% no total de tokens por reduzir chamadas (“último concurso” + “métricas” + “explicar”) para 1 tool, e por eliminar explicações repetidas.

#### B.3.3 Proposta B (alternativa): manter tools existentes e criar “modo econômico”

Sem tool nova, padronizar:

- `get_draw_window(window_size=1, verbosity=minimal)`
- `compute_window_metrics(window_size=1, metrics=[...], verbosity=minimal)`
- O agente produz o texto `brief/detailed` com base em templates locais (cliente), usando apenas valores.

Risco: descrições espalhadas por clientes → divergências. Preferir Proposta A se o objetivo for padronização.

### B.4 Alerta de alto consumo (pergunta direta ao usuário antes de continuar)

#### B.4.1 No agente/cliente (recomendado)

Quando a intenção do usuário solicitar muitos itens (métricas * grandes janelas * full details), o agente deve:

- estimar custo usando heurísticas determinísticas:
  - `numbers_count` esperado (p.ex. `window_size * 5` em distribuições)
  - matriz: 375 números (ou mais, dependendo do encoding)
  - séries: `window_size` pontos por métrica
- perguntar: “isso pode consumir muitos tokens; deseja continuar em modo completo, ou prefere resumo?”

**Nota importante (limite do servidor MCP):**

- O **servidor MCP** (tools) não possui um “canal de conversa” para **perguntar e aguardar resposta** no meio de uma execução. A decisão “continuar vs resumir” precisa ocorrer **no host/agente/cliente** antes da chamada da tool.
- Logo, a melhoria “pergunta direta ao usuário antes de continuar” é **implementável apenas no lado cliente/host**, não como comportamento interativo do servidor.

#### B.4.2 No servidor (limitado mas útil)

O servidor não conhece tokenização do modelo, mas pode retornar:

- `payload_estimate`: `{ numbers_count, series_points, approx_bytes }`
- `warnings`: `[{ code: "HIGH_PAYLOAD", threshold, observed }]`

Critério de aceitação:

- consumidores conseguem detectar “alto payload” sem parse ad hoc do JSON.

### B.5 Geração de jogos baseada nos últimos 20 concursos (pipeline econômico)

#### B.5.1 Problema

Fluxo em chat tende a ser:

1) “pegar janela”
2) “computar métricas”
3) “interpretar”
4) “gerar jogos”

Isso repete payloads e aumenta tokens.

#### B.5.2 Proposta: tool composta/pipeline

Criar (ou estender) uma tool que aceite `window_size` e encapsule o pipeline:

- **Opção**: estender `generate_candidate_games` para aceitar `window_size` (ancorado em latest) e selecionar o mínimo de métricas necessárias internamente.
- **Ou**: tool nova `generate_candidate_games_from_window`

Parâmetros econômicos:

- `verbosity=minimal` por default
- `explain=false` por default (explicação fica sob demanda em `explain_candidate_games`)

Critérios de aceitação:

- para `count` em {1,2,10,50,100}, payload cresce aproximadamente linear no número de jogos, sem anexar métricas redundantes.
- replay/auditoria (seed/hash/versões) preservados conforme contrato.

### B.6 Regras para séries `count` e “modo painel” (agregados por janela)

Requisito de UI: quando `unit="count"` e `scope="series"`, exibir por default agregados (sum/mean/min/max/iqr), não a série completa.

Proposta:

- Nova tool (ou modo em `compute_window_metrics`) `compute_window_metrics_summary`:
  - para métricas `scope=series`, retorna agregados determinísticos
  - a série completa fica disponível por opt-in (`include_series=true`)

Nota: isso reduz tokens muito mais que 10% quando `window_size` é 100.

### B.7 Entropia e HHI na janela (representação recomendada para painéis e filtros)

Reforço (ver Anexo A.6):

- manter métricas canônicas como série por concurso
- para “janela” em painel/filtros, usar resumo estatístico (mean/median/min/max/iqr/std ou MAD/MADN)
- se desejar “global da janela por contagens agregadas”, criar **métrica nova versionada** (para evitar ambiguidade semântica)

### B.8 Classificação por dificuldade (para planejamento)

- **Baixa**
  - `fields/response_projection` em `discover_capabilities`
  - `include_explanations=false` e/ou `verbosity=minimal` (quando não quebrar compatibilidade)
  - adicionar `payload_estimate` / `warnings` (estrutura simples e determinística)
- **Média**
  - `compute_window_metrics_summary` (agregados para séries, padrão “painel”)
  - `value_encoding` (p.ex. `sparse_triplets`) para `matriz_numero_slot`
  - `describe_latest_metrics` com templates determinísticos (`brief/detailed`)
- **Alta**
  - paginação para `full` detalhado (matrizes/séries grandes)
  - tool composta “pipeline” de geração baseada em janela com seleção mínima de métricas e auditoria completa

**Nota (limite do transporte atual):**

- “Streaming” de resultados dentro do **mesmo** call de tool não é um alvo realista no transporte atual (tool retorna um único payload JSON por execução).
- Para obter o mesmo benefício (economia e latência), preferir:
  - **paginação determinística** (ex.: `page`/`page_size`/`cursor` estável por ordenação canônica), e/ou
  - **projeção de campos** (`fields` / `response_projection`) + modos (`verbosity` / `response_format`).

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

