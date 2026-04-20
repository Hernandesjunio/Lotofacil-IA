# Plano de testes do domínio

Este documento define o que significa cobertura de testes "100%" para o domínio documentado neste repositório.

## Princípio de cobertura total

Considera-se cobertura total quando todos os itens abaixo têm pelo menos:

1. um teste positivo determinístico;
2. um teste de borda ou negativo;
3. um vínculo rastreável com ao menos um prompt de `docs/prompt-catalog.md`.

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

Validam schema de input/output, `shape`, `scope`, `unit`, `version`, erros e determinismo do payload.

### 3. Testes de integração

Validam fluxo completo com histórico real ou fixture congelada.

### 4. Testes E2E guiados por prompt

Validam que os prompts documentados podem ser mapeados para as tools corretas e produzem resposta semanticamente correta.

### 5. Testes de propriedades

Validam invariantes como determinismo, ordenação, monotonicidade, ranges, finitude e não regressão semântica.

## Cobertura por métrica

### Base e derivadas por janela

| Métrica | Teste positivo obrigatório | Teste de borda / negativo |
|---|---|---|
| `frequencia_por_dezena` | contagem exata em janela sintética | empate resolvido por dezena ascendente |
| `top10_mais_sorteados` | ranking correto do top 10 | ties estáveis |
| `top10_menos_sorteados` | ranking correto do bottom 10 | ties estáveis |
| `atraso_por_dezena` | atraso exato ao fim da janela | dezena nunca observada satura corretamente |
| `frequencia_blocos` | blocos de presença corretos | janela com bloco único |
| `ausencia_blocos` | blocos de ausência corretos | ausência contínua até o fim |
| `estado_atual_dezena` | mapeia saída recente para `0` | atraso coerente quando não sai |
| `matriz_numero_slot` | contagem correta em `dezena x slot` | ordenação prévia obrigatória |
| `assimetria_blocos` | razão correta por dezena e mediana da janela | divisão protegida em casos degenerados |
| `persistencia_atraso_extremo` | contagem correta acima do percentil de referência | warning ou fallback para referência circular |

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
| `coeficiente_variacao` | CV correto em série positiva | fallback em média próxima de zero |
| `madn_janela` | MADN correto | fallback robusto quando mediana é zero |
| `mad_janela` | MAD correto | série constante |
| `tendencia_linear` | inclinação correta | série constante com slope zero |
| `divergencia_kl` | valor finito com smoothing | sem infinito para `N >= 5` |
| `zscore_repeticao` | z-score correto com referência declarada | erro se referência ausente |
| `estabilidade_ranking` | teste bloqueado por status pendente | exige `allow_pending: true` |

## Cobertura por tool

| Tool | Casos positivos | Casos negativos |
|---|---|---|
| `get_draw_window` | janela válida com e sem `end_contest_id` | `window_size <= 0`, `contest_id` inexistente |
| `compute_window_metrics` | múltiplas métricas com shapes distintos | `metrics` ausente, métrica desconhecida |
| `analyze_indicator_stability` | escalar, vetorial e série vetorial | sem agregação, método inválido |
| `compose_indicator_analysis` | `weighted_rank`, `threshold_filter`, `joint_profile`, `stability_rank` | pesos incorretos, target incompatível |
| `analyze_indicator_associations` | `spearman` e `pearson` | séries incompatíveis, método inválido |
| `summarize_window_patterns` | cobertura, moda, IQR, percentis | feature incompatível ou sem agregação |
| `generate_candidate_games` | estratégias fixas e `declared_composite_profile` | seed ausente, orçamento excedido, exclusões conflitantes |
| `explain_candidate_games` | breakdown completo de score e exclusões | jogo fora do domínio ou payload inválido |

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

Todos os erros documentados em `docs/mcp-tool-contract.md` precisam ter pelo menos um teste que:

1. provoque o erro;
2. valide `code`, `message` e `details`;
3. valide que o código veio da tool correta.

## Cobertura E2E por prompt

Todos os prompts de `docs/prompt-catalog.md` devem ter:

- 1 teste E2E de roteamento correto para a tool ou combinação de tools;
- 1 teste semântico validando que a resposta não usa linguagem preditiva indevida;
- 1 assertiva de determinismo quando houver payload equivalente.

Prompts 35 a 38 do catálogo são obrigatórios como testes negativos.

## Cobertura de determinismo

Os seguintes cenários precisam ser repetidos múltiplas vezes sobre a mesma fixture:

1. `get_draw_window`
2. `compute_window_metrics`
3. `analyze_indicator_stability`
4. `compose_indicator_analysis`
5. `analyze_indicator_associations`
6. `summarize_window_patterns`
7. `generate_candidate_games`
8. `explain_candidate_games`

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
