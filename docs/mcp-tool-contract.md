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

**Validação prática:** para cada tool, verifique (a) rejeição de inputs inválidos com o código de erro certo, (b) presença dos campos obrigatórios no output conforme invariantes, (c) reprodutibilidade quando o contrato exige `seed` ou `deterministic_hash`.

7. **Lacunas de parâmetro em linguagem natural** — quando um pedido do usuário não puder ser mapeado sem ambiguidade para o JSON da tool, o fluxo deve obter dados faltantes por **perguntas específicas** (seção *Integração com agentes: lacunas de parâmetros e esclarecimento*, abaixo), nunca por inferência oculta no servidor.

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
| **Prompts** (`prompts/list`, `prompts/get`) | Templates com argumentos nomeados; mensagens estruturadas para o LLM ([especificação](https://modelcontextprotocol.io/specification/2025-06-18/server/prompts)). | Fluxos recorrentes (ex.: “estabilidade na janela N”, “composição com pesos declarados”) onde os **argumentos do template espelham campos do schema** da tool, reduzindo omissão de parâmetros sem violar a proibição de pesos implícitos. |
| **Resources** (`resources/read`, templates) | Dados identificados por URI, fornecendo contexto à aplicação/modelo ([especificação](https://modelcontextprotocol.io/specification/2025-06-18/server/resources)). | Trechos versionados do glossário, definições de métricas ou metadados de `dataset_version` — conteúdo **read-only** e referenciável em análises sem duplicar lógica nas tools. |

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
- `sampled` — amostra do espaço; exige `seed` no request e `seed_used` + `n_samples_used` no output.
- `greedy_topk` — exploração gulosa/top-k; exige `seed` quando houver desempate/aleatoriedade na seleção do topo.

##### Catálogo fechado de `tie_break_rule`

Valores aceitos:

- `lexicographic_numbers_asc` — empate resolve pelo jogo ordenado crescente, comparado lexicograficamente.
- `stable_rank_then_lexicographic` — mantém ordem estável do ranking/score e desempata por `lexicographic_numbers_asc`.

## Invariantes globais

Cada item é uma obrigação de comportamento; testes de contrato devem assertar explicitamente o item correspondente.

1. **Reprodutibilidade por versão de dados** — Mesmo input canônico + mesmo `dataset_version` deve produzir o mesmo output.  
   *Validação:* duas chamadas idênticas após ingestão estável; comparar payload ou hash conforme política do projeto.

2. **Transparência de janela** — Toda resposta analítica declara a janela usada.  
   *Validação:* presença de `Window` ou campos equivalentes (`window_size`, `start_contest_id`, `end_contest_id`, lista de ids) no corpo da resposta.

3. **Transparência de geração** — Toda resposta de geração declara estratégia, versão, filtros, pesos, `search_method`, `tie_break_rule` e `seed_used` quando aplicável.  
   *Validação:* nenhum jogo candidato sem linhagem de estratégia; estratégias amostradas sempre com `seed_used` resolvido.

4. **Stateless por request** — Nenhuma tool depende de contexto oculto de conversa.  
   *Validação:* mesma requisição HTTP/MCP isolada produz mesmo resultado; não há “memória” implícita entre chamadas além do dataset versionado.

5. **Linguagem definida** — Termos como `slot`, `outlier`, `persistência`, `equilíbrio`, `faixa típica` e `correlação` devem ter definição explícita no payload ou na documentação.  
   *Validação:* glossário ou campo `explanation`/`definitions` referenciado; respostas não usam jargão solto.

6. **Proibição de predição comercial** — Ferramentas não devem concluir "mais provável de sair"; devem concluir "mais estável", "mais aderente", "mais persistente no histórico declarado" ou "mais raro".  
   *Validação:* revisão de strings em `rationale` e resumos; testes de regressão com prompts que pedem “probabilidade”.

7. **Hash determinístico** — `deterministic_hash = SHA256(canonical_json({input, dataset_version, tool_version}))`.  
   *Validação:* `canonical_json` segue JSON canônico (ver seção “Canonização e metadados comuns”); hash estável para fixture fixa.

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

#### Semântica e validação

- **Validação:** testes de contrato devem asserir presença de todos os campos acima em todas as tools.

## Ferramentas propostas

### Parâmetros comuns de janela

Várias tools recebem um recorte temporal sobre o histórico canônico. Campos recorrentes:

| Campo | O que representa | Como validar |
|--------|------------------|--------------|
| `window_size` | Quantidade de concursos consecutivos a considerar, ancorada no fim da janela. | Inteiro `>= 1`; zero ou negativo → `INVALID_WINDOW_SIZE`. |
| `end_contest_id` | Último concurso inclusivo da janela. | Se omitido, o servidor usa o mais recente disponível; id inexistente → `INVALID_CONTEST_ID`. |
| `include_metadata` | Quando presente (ex.: `get_draw_window`), inclui campos auxiliares de `Draw` além dos números. | Resposta contém `source` / `ingested_at` etc. somente quando `true`, salvo política explícita documentada. |

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
- Métricas `pendente de detalhamento` exigem `allow_pending: true`.
- Parâmetros de métrica devem ser explícitos em `params`; o servidor não infere defaults semânticos escondidos.

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
- **`top_k` / `min_history`:** limitam o ranking e exigem histórico mínimo para estatística estável; `min_history` maior que janela disponível → falha clara (`INSUFFICIENT_HISTORY` ou regra documentada no catálogo).

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

### 7. `generate_candidate_games`

#### Finalidade

Gerar jogos candidatos a partir de estratégias nomeadas e filtros declarados.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
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

#### Regras

- `seed` é obrigatória sempre que houver qualquer estratégia `sampled` ou `greedy_topk`.
- `MAX_COUNT_PER_STRATEGY = 100`; `MAX_TOTAL_COUNT = 250`.
- Estratégia desconhecida retorna `UNKNOWN_STRATEGY`.
- Quando a estratégia admitir mais de um `search_method` no contrato da própria estratégia, o item correspondente em `plan` deve declarar `search_method` explicitamente.
- `declared_composite_profile` só aceita componentes listados em [generation-strategies.md](generation-strategies.md).
- `structural_exclusions` são opcionais, mas quando presentes tornam-se parte do determinismo do request.
- O servidor continua não aceitando "pesos soltos" fora de um schema explícito.

#### Semântica e validação

- **`plan`:** fila de estratégias com `count` cada; a soma dos `count` está limitada por `MAX_TOTAL_COUNT`; cada estratégia por `MAX_COUNT_PER_STRATEGY`.
- **`plan[].search_method`:** só aparece no request quando o contrato da estratégia permite múltiplos métodos de busca. Em `declared_composite_profile`, é obrigatório e deve ser `sampled` ou `greedy_topk`; nas estratégias com método fixo em [generation-strategies.md](generation-strategies.md), o request não repete esse campo.
- **`seed`:** obrigatória se qualquer passo for estocástico ou `greedy_topk`; validar reprodutibilidade com a mesma semente e dataset.
- **`global_constraints`:** `unique_games` evita duplicatas no lote; `sorted_numbers` padroniza representação dos jogos.
- **`structural_exclusions`:** filtros duros no espaço de jogos; quando presentes, fazem parte do input canônico do hash determinístico e devem aparecer no output (metadados ou explicação).
- **`declared_composite_profile`:** subscores e pesos só podem usar nomes permitidos em [generation-strategies.md](generation-strategies.md); soma dos pesos do perfil deve seguir a mesma regra de tolerância que composições (`1.0 ± 1e-9`) salvo especificação divergente no doc de estratégias.

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

Os códigos abaixo são parte do contrato público: clientes e testes devem poder asserções sobre eles. O campo `message` é texto humano (pode variar); `code` e `details` são o contrato estável para automação.

**Validação geral:** para cada código, existir pelo menos um teste negativo que provoque o código e um caminho feliz que não o emita; `details` deve carregar pistas estruturadas (`missing_field`, `metric_name`, etc.) sem vazar segredos.

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
| `UNSUPPORTED_PATTERN_FEATURE` | feature não suportada em resumo de padrões | padrões |
| `INCOMPATIBLE_INDICATOR_FOR_STABILITY` | indicador sem shape compatível para ranking | estabilidade |
| `INCOMPATIBLE_COMPOSITION` | componentes incompatíveis entre si ou com o target | composição, geração |
| `STRUCTURAL_EXCLUSION_CONFLICT` | exclusões tornam o plano inviável ou contraditório | geração |
| `INSUFFICIENT_HISTORY` | histórico insuficiente para a janela pedida | tools com janela |
| `PLAN_BUDGET_EXCEEDED` | `count` acima do orçamento permitido | geração |
| `NON_DETERMINISTIC_CONFIGURATION` | configuração impede reprodução | geração |
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
- **Catálogo fechado:** `UNKNOWN_METRIC`, `UNKNOWN_STRATEGY`, `UNSUPPORTED_*` — o cliente pediu algo fora do que a V1 implementa; a correção é ajustar o request ou estender o catálogo na documentação, não “adivinhar” no servidor.
- **Compatibilidade dimensional:** `INCOMPATIBLE_INDICATOR_FOR_STABILITY`, `INCOMPATIBLE_COMPOSITION` — combinação de métricas, `target` ou agregações não faz sentido matematicamente para aquela tool.
- **Geração e orçamento:** `STRUCTURAL_EXCLUSION_CONFLICT`, `PLAN_BUDGET_EXCEEDED`, `NON_DETERMINISTIC_CONFIGURATION` — plano impossível, excesso de `count` ou falta de `seed` onde o contrato exige determinismo.
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

1. Mesmo input + mesmo `dataset_version` retornam o mesmo `deterministic_hash`.
2. `compute_window_metrics` retorna valores idênticos em execuções repetidas, com `shape` explícito.
3. `analyze_indicator_stability` rejeita vetoriais sem `aggregation` e usa `madn` por default.
4. `compose_indicator_analysis` rejeita pesos que não somam 1 e componentes incompatíveis.
5. `analyze_indicator_associations` rejeita associação sem redução explícita de série vetorial.
6. `summarize_window_patterns` calcula cobertura, moda e faixa típica de forma determinística.
7. `generate_candidate_games` respeita orçamento, seed, filtros estruturais e estratégia composta declarada.
8. `explain_candidate_games` retorna ranking de estratégias e detalhamento de exclusões.
9. `divergencia_kl` nunca retorna `+∞` ou `NaN` para janelas `N >= 5`.
10. Toda família de prompt documentada em [prompt-catalog.md](prompt-catalog.md) deve ter ao menos um teste positivo e um negativo em [test-plan.md](test-plan.md).
11. Em pelo menos um fluxo de integração (E2E ou teste de agente), um pedido **não** mapeável sem lacunas deve resultar em esclarecimento com campos explícitos ou em `INVALID_REQUEST` **sem** execução com parâmetros supostos pelo modelo.

## Avaliação de viabilidade

### Viável em V1

- `get_draw_window`
- `compute_window_metrics`
- `analyze_indicator_stability`
- `compose_indicator_analysis`
- `analyze_indicator_associations`
- `summarize_window_patterns`
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
2. as 8 tools acima;
3. catálogo de métricas fechado;
4. estratégias e filtros estruturais fechados;
5. catálogo de prompts de teste;
6. plano de testes cobrindo métricas, tools, composições, filtros, erros e prompts.
