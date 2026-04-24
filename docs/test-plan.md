# Plano de testes do domínio

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Este documento define o que significa cobertura de testes "100%" para o domínio documentado neste repositório.

## Princípio de cobertura total

Considera-se cobertura total quando todos os itens abaixo têm pelo menos:

1. um teste positivo determinístico;
2. um teste de borda ou negativo;
3. um vínculo rastreável com ao menos um prompt de [prompt-catalog.md](prompt-catalog.md).

Os itens obrigatórios são:

- todas as métricas do catálogo;
- todas as tools do MCP;
- todos os operadores de composição;
- todos os métodos de associação;
- todas as agregações permitidas;
- todas as transformações permitidas;
- todas as estratégias de geração;
- todos os filtros estruturais;
- todos os códigos de erro previstos;
- todas as famílias de prompt cobertas.

## Camadas de teste

### 1. Testes de fórmula

Validam o cálculo matemático puro em janelas sintéticas pequenas, com valores esperados calculados à mão.

### 2. Testes de contrato

Validam schema de input/output, `shape`, `scope`, `unit`, `version`, erros e determinismo do payload. A ordem de entrega e as matrizes **Fase B.1** (agregados) e **Fase B.2** (janela por extremos, `top10` e legado de export, [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)) estão em [contract-test-plan.md](contract-test-plan.md).

### 3. Testes de integração

Validam fluxo completo com histórico real ou fixture congelada.

### 4. Testes E2E guiados por prompt

Validam que os prompts documentados podem ser mapeados para as tools corretas e produzem resposta semanticamente correta.

### 5. Testes de propriedades

Validam invariantes como determinismo, ordenação, monotonicidade, ranges, finitude e não regressão semântica.

### 6. Integração real com API OpenAI (ChatGPT)

Valida fluxo **agente → tool MCP** com **modelo real** (tool calling), usando prompts naturais alinhados ao [prompt-catalog.md](prompt-catalog.md). Por **custo** e **não determinismo do LLM**, esta camada roda em **esteira separada** no GitHub Actions e **não** substitui testes de fórmula, contrato ou propriedades.

Requisitos e suíte mínima de **cinco** cenários (**L1–L5**): [live-openai-integration-pipeline.md](live-openai-integration-pipeline.md).

## Cobertura por métrica

### Base, por_transformacao e apoio por janela

| Métrica | Teste positivo obrigatório | Teste de borda / negativo |
|---|---|---|
| `frequencia_por_dezena` | contagem exata em janela sintética | soma dos 25 contadores = `15 × window_size` |
| `top10_mais_sorteados` | ranking correto do top 10 | ties estáveis |
| `top10_menos_sorteados` | ranking correto do bottom 10 | ties estáveis |
| `atraso_por_dezena` | atraso exato ao fim da janela | dezena nunca observada satura corretamente |
| `frequencia_blocos` | blocos de presença corretos | janela com bloco único |
| `ausencia_blocos` | blocos de ausência corretos | ausência contínua até o fim |
| `estado_atual_dezena` | mapeia saída recente para `0` | atraso coerente quando não sai |
| `matriz_numero_slot` | contagem correta em `dezena x slot` | ordenação prévia obrigatória |
| `assimetria_blocos` | razão correta por dezena e mediana da janela | divisão protegida em casos degenerados |
| `persistencia_atraso_extremo` | contagem correta acima do percentil de referência declarado | erro se `reference` ou `baseline_version` estiver ausente |

### Séries por concurso

| Métrica | Teste positivo obrigatório | Teste de borda / negativo |
|---|---|---|
| `repeticao_concurso_anterior` | interseção correta entre concursos consecutivos | fronteira `N` versus `N-1` |
| `intersecoes_multiplas` | interseção correta para lag declarado | lag inválido retorna erro |
| `pares_no_concurso` | contagem correta de pares por concurso | todos pares / todos ímpares |
| `quantidade_vizinhos_por_concurso` | adjacências corretas por concurso | concurso sem vizinhos |
| `sequencia_maxima_vizinhos_por_concurso` | run máxima correta | run unitária |
| `distribuicao_linha_por_concurso` | vetor correto por linha | soma dos componentes = 15 |
| `distribuicao_coluna_por_concurso` | vetor correto por coluna | soma dos componentes = 15 |
| `entropia_linha_por_concurso` | bits corretos em caso simples | entropia zero em concentração máxima |
| `entropia_coluna_por_concurso` | bits corretos em caso simples | entropia zero em concentração máxima |
| `hhi_linha_por_concurso` | HHI correto | limite máximo em concentração total |
| `hhi_coluna_por_concurso` | HHI correto | limite máximo em concentração total |

### Métricas de jogo candidato

| Métrica | Teste positivo obrigatório | Teste de borda / negativo |
|---|---|---|
| `pares_impares` | paridade correta | jogo inválido rejeitado |
| `quantidade_vizinhos` | adjacências corretas | nenhuma adjacência |
| `sequencia_maxima_vizinhos` | run máxima correta | run de tamanho 1 |
| `distribuicao_linha` | vetor correto por linha | soma = 15 |
| `distribuicao_coluna` | vetor correto por coluna | soma = 15 |
| `entropia_linha` | `H` e `H_norm` corretos | concentração total |
| `entropia_coluna` | `H` e `H_norm` corretos | concentração total |
| `hhi_concentracao` | par correto `(hhi_linha, hhi_coluna)` | limites esperados |
| `analise_slot` | score correto com matriz conhecida | range fechado em `[0,1]` |
| `surpresa_slot` | valor finito com smoothing | nunca retorna `NaN` |
| `intersecao_conjunto_referencia` | interseção correta | referência vazia |
| `estatistica_runs` | par correto `(run_max, neighbor_count)` | consistência com métricas base |
| `outlier_score` | distância correta em fixture controlada | regularização evita singularidade |

### Métricas de estabilidade

| Métrica | Teste positivo obrigatório | Teste de borda / negativo |
|---|---|---|
| `media_janela` | média correta | série constante |
| `desvio_padrao_janela` | desvio amostral correto | `N < 2` tratado corretamente |
| `coeficiente_variacao` | CV correto em série estritamente positiva com `μ > ε_cv` | erro `UNSUPPORTED_NORMALIZATION_METHOD` para série com valores `<= 0` ou `μ <= ε_cv` |
| `madn_janela` | MADN correto | fallback robusto quando mediana é zero |
| `mad_janela` | MAD correto | série constante |
| `tendencia_linear` | inclinação correta | série constante com slope zero |
| `divergencia_kl` | valor finito com smoothing | sem infinito para `N >= 5` |
| `zscore_repeticao` | z-score correto com referência declarada | erro se referência ausente |
| `estabilidade_ranking` | score esperado \(1.0\) quando rankings idênticos entre blocos; golden conforme catálogo | ranking oposto entre blocos consecutivos (\(\rho=-1 \Rightarrow\) score \(0.0\)); fixture com empates para average rank |

## Cobertura por tool

| Tool | Casos positivos | Casos negativos |
|---|---|---|
| `get_draw_window` | janela válida com e sem `end_contest_id` | `window_size <= 0`, `contest_id` inexistente |
| `compute_window_metrics` | múltiplas métricas com shapes distintos | `metrics` ausente, métrica desconhecida |
| `analyze_indicator_stability` | escalar, vetorial e série vetorial | sem agregação, método inválido |
| `compose_indicator_analysis` | `weighted_rank`, `threshold_filter`, `joint_profile`, `stability_rank` | pesos incorretos, target incompatível |
| `analyze_indicator_associations` | `spearman` e `pearson`; cenário pares×entropia (secção *Cenário canónico*) | séries incompatíveis, método inválido; com `stability_check` e build sem suporte → `UNSUPPORTED_STABILITY_CHECK` |
| `summarize_window_patterns` | cobertura, moda, IQR, percentis | feature incompatível ou sem agregação |
| `summarize_window_aggregates` | histograma escalar com `bucket_values` explícitos, top-k de padrões com desempate lexicográfico e matriz cheia `5xK` com eixos declarados | `aggregates` ausente/vazio, `aggregate_type` inválido, parâmetros obrigatórios ausentes por tipo, bucket spec ambígua, fonte incompatível (`UNSUPPORTED_SHAPE`), métrica desconhecida |
| `generate_candidate_games` | estratégias fixas e `declared_composite_profile` | seed ausente, orçamento excedido, exclusões conflitantes |
| `explain_candidate_games` | breakdown completo de score e exclusões | jogo fora do domínio ou payload inválido |
| `help` | retorna `index_markdown`, `index_resource_uri` e `templates[]` | erro `HELP_UNAVAILABLE` se o índice não puder ser carregado |

## Cobertura de agregados canônicos (`summarize_window_aggregates`)

Cada `aggregate_type` do enum fechado deve ter:

- 1 teste positivo com validação de forma, ordenação canônica e determinismo;
- 1 teste negativo de parâmetros obrigatórios ausentes/inválidos;
- 1 teste negativo de compatibilidade de shape da métrica fonte.

Tipos cobertos:

- `histogram_scalar_series`
  - Positivo: buckets ordenados por `x` asc com bucketização explícita.
  - Negativo: `bucket_spec` ausente, misto (discreto + contínuo) ou incompleto.
- `topk_patterns_count_vector5_series`
  - Positivo: `items` ordenados por `count desc` e desempate por `pattern` lexicográfico asc.
  - Negativo: `top_k` ausente, não inteiro ou `< 1`.
- `histogram_count_vector5_series_per_position_matrix`
  - Positivo: matriz cheia `matrix[5][K]` com eixo de posição `1..5` e eixo de valor `value_min..value_max` em ordem ascendente.
  - Negativo: `value_min`/`value_max` ausentes, não inteiros ou `value_min > value_max`.

## Cobertura de agregações

Cada agregação aceita pelo contrato deve ter:

- 1 teste positivo em `analyze_indicator_stability`;
- 1 teste positivo em `analyze_indicator_associations` quando aplicável;
- 1 teste negativo por uso incompatível.

Agregações:

- `mean`
- `max`
- `l2_norm`
- `per_component`
- `mode_vector` (somente em `summarize_window_patterns`)

## Cobertura de transformações de composição

| Transformação | Caso positivo | Caso negativo |
|---|---|---|
| `normalize_max` | vetor positivo normalizado em `[0,1]` | máximo zero tratado corretamente |
| `invert_normalize_max` | atraso menor gera score maior | divisão inválida tratada |
| `rank_percentile` | ranking determinístico | empates estáveis |
| `identity_unit_interval` | valor preservado em `[0,1]` | valor fora da faixa rejeitado |
| `one_minus_unit_interval` | complementar correto | valor fora da faixa rejeitado |
| `shift_scale_unit_interval` | escala correta para domínio conhecido | parâmetros implícitos proibidos |

## Cobertura de associações

| Método | Positivo | Negativo |
|---|---|---|
| `spearman` | correlação monotônica detectada corretamente | série constante tratada |
| `pearson` | correlação linear detectada corretamente | série constante ou incompatível |
| `rolling_window` | estabilidade da associação por subjanela | subjanela maior que a janela |

## Cenário canónico: pares e entropia de linha (interação, não causalidade)

Pergunta descritiva: *quando, na janela, a quantidade de pares por concurso varia, como co-move a entropia de linha nesses mesmos concursos?* — sem afirmar causalidade.

| Elemento | Valor |
|----------|--------|
| Tool | `analyze_indicator_associations` |
| Itens (séries alinhadas) | `pares_no_concurso`, `entropia_linha_por_concurso` (ambas Tabela 1) |
| Método canónico (este cenário) | `spearman` (robusto; ver contrato) |
| Teste positivo | Mesmo `(fixture, window_size, end_contest_id, dataset_version)` produz a mesma magnitude (e `deterministic_hash` conforme a tool) |
| Leitura permitida | co-movimento / correlação na janela |
| Leitura proibida | “um causa o outro” fora de experiência controlada (ver [metric-glossary.md](metric-glossary.md), glossário “correlação” e [brief.md](brief.md)) |

O mesmo padrão pode ser replicado para outras pares (ex. `entropia_coluna_por_concurso`) em testes adicionais; o [prompt-catalog.md](prompt-catalog.md) inclui prompt 42 para roteamento.

**Referência normativa:** [ADR 0006 D5](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) e tabela de GAPS em [contract-test-plan.md](contract-test-plan.md).

## GAPS e parâmetros de janela (disponibilidade, estabilidade)

| Código / tema | O que o teste cobre |
|---------------|---------------------|
| `UNKNOWN_METRIC` com `details` em `compute_window_metrics` | Rota ainda com allowlist; payload congelado quando a política ainda restringe nomes (ver [mcp-tool-contract.md](mcp-tool-contract.md)). |
| Coerência geração/explicar vs. `compute` | Estratégia ou `explain` referem métrica; `compute` rejeita; comportamento rastreável até promoção. |
| `min_history` vs. `window_size` em `analyze_indicator_stability` | `min_history` &gt; tamanho resolvido da janela → `INSUFFICIENT_HISTORY` com `details` (ver ADR 0006 D4). |
| `UNSUPPORTED_STABILITY_CHECK` | Request a `analyze_indicator_associations` com `stability_check` e build sem a camada de estabilidade. |

## Cobertura das estratégias de geração

| Estratégia | Casos positivos obrigatórios | Casos negativos obrigatórios |
|---|---|---|
| `common_repetition_frequency` | gera jogos no intervalo `[Q1,Q3]` e com overlap mínimo do top 10 | sem `seed`, orçamento excedido |
| `row_entropy_balance` | respeita `H_norm >= 0.95` e `hhi_linha <= 0.25` | jogo colapsado em poucas linhas |
| `slot_weighted` | score ótimo por DP, desempate correto | suporte de slot inviável sem relaxação |
| `outlier_candidate` | score alto e filtro mínimo de entropia | exclusão por entropia muito baixa |
| `declared_composite_profile` | score ponderado correto e pesos somando 1 | componente não permitido, peso ausente |

## Cobertura dos filtros estruturais

Cada filtro precisa de:

- 1 caso aceito exatamente no limiar;
- 1 caso rejeitado logo acima ou abaixo do limiar;
- 1 caso em que o filtro aparece no `constraints_applied`.

Filtros:

- `max_consecutive_run`
- `max_neighbor_count`
- `min_row_entropy_norm`
- `min_column_entropy_norm`
- `max_hhi_linha`
- `max_hhi_coluna`
- `repeat_range`
- `min_slot_alignment`
- `max_outlier_score`

## Cobertura dos erros

Todos os erros documentados em [mcp-tool-contract.md](mcp-tool-contract.md) precisam ter pelo menos um teste que:

1. provoque o erro;
2. valide `code`, `message` e `details`;
3. valide que o código veio da tool correta.

Exemplos de erros cujo acréscimo ou aperto de semântica acompanha [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md): `INSUFFICIENT_HISTORY` com `min_history` explícito; `UNKNOWN_METRIC` com `details.metric_name` em `compute_window_metrics` quando a allowlist ainda restringe o catálogo; `UNSUPPORTED_STABILITY_CHECK` em `analyze_indicator_associations`.

## Cobertura E2E por prompt

Todos os prompts de [prompt-catalog.md](prompt-catalog.md) devem ter:

- 1 teste E2E de roteamento correto para a tool ou combinação de tools;
- 1 teste semântico validando que a resposta não usa linguagem preditiva indevida;
- 1 assertiva de determinismo quando houver payload equivalente.

Prompts 37 a 41 do catálogo são obrigatórios como testes negativos.

## Cobertura de determinismo

Os seguintes cenários precisam ser repetidos múltiplas vezes sobre a mesma fixture:

1. `get_draw_window`
2. `compute_window_metrics`
3. `analyze_indicator_stability`
4. `compose_indicator_analysis`
5. `analyze_indicator_associations`
6. `summarize_window_patterns`
7. `summarize_window_aggregates`
8. `generate_candidate_games`
9. `explain_candidate_games`

Critério de aceitação: mesma entrada gera mesmo `deterministic_hash` em 100% das execuções.

## Cobertura mínima de fixtures

O projeto deve manter pelo menos estas fixtures:

1. janela pequena sintética de fácil cálculo manual;
2. janela real curta com padrões variados;
3. janela real longa para estabilidade e correlações;
4. fixture patológica com runs longos e concentração extrema;
5. fixture com ties frequentes para validar desempates.

## Critério final de aceite

O domínio só deve ser considerado coberto quando:

- todas as métricas tiverem testes positivos e negativos;
- todas as tools tiverem testes positivos e negativos;
- todas as estratégias e filtros tiverem testes determinísticos;
- todos os prompts documentados tiverem cobertura E2E;
- todos os códigos de erro tiverem cobertura explícita;
- nenhum teste aceite linguagem preditiva onde o contrato só permite leitura descritiva.
