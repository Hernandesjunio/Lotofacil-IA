# Estratégias de geração

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

## Objetivo

Separar a semântica das estratégias de geração do contrato MCP e da implementação do motor. Cada estratégia é um contrato formal testável.

## Contrato obrigatório de uma estratégia

Toda estratégia declara:

| Campo | Obrigatório | Descrição |
|-------|:-:|-----------|
| `strategy_name` | sim | Identificador estável |
| `strategy_version` | sim | SemVer |
| `objetivo` | sim | O que a estratégia tenta produzir |
| `metricas_principais` | sim | Métricas canônicas consumidas |
| `filtros` | sim | Restrições hard |
| `score` | sim | Fórmula fechada `score(game, window) -> R` |
| `search_method` | sim | `exhaustive | sampled | greedy_topk` |
| `seed` | sim quando necessário | `uint64` |
| `tie_break_rule` | sim | Desempate determinístico |
| `interpretacao_correta` | sim | Como ler a saída |
| `interpretacao_incorreta` | sim | Leitura proibida |

Regras gerais:

1. Fórmula de score documentada e testável.
2. Payload de explicação definido.
3. Mesmo `{seed, window, dataset_version}` implica mesma saída.
4. Termos como "equilíbrio", "aderência" e "central" precisam de definição operacional.

## Guardrails estruturais canônicos

Toda estratégia V1 pode receber filtros adicionais por `structural_exclusions`, desde que o payload os declare explicitamente. Os filtros canônicos são:

- `max_consecutive_run`: rejeita jogos com `sequencia_maxima_vizinhos > limite`.
- `max_neighbor_count`: rejeita jogos com `quantidade_vizinhos > limite`.
- `min_row_entropy_norm`: rejeita jogos com `entropia_linha.H_norm < limite`.
- `min_column_entropy_norm`: rejeita jogos com `entropia_coluna.H_norm < limite`.
- `max_hhi_linha`: rejeita jogos com `hhi_concentracao.hhi_linha > limite`.
- `max_hhi_coluna`: rejeita jogos com `hhi_concentracao.hhi_coluna > limite`.
- `repeat_range`: rejeita jogos cuja repetição contra o último concurso esteja fora de `[min, max]`.
- `min_slot_alignment`: rejeita jogos com `analise_slot < limite`.
- `max_outlier_score`: rejeita jogos estruturalmente distantes demais do centro da janela.

Esses filtros não afirmam impossibilidade matemática; apenas removem regiões consideradas pouco úteis para a geração orientada a padrão histórico.

## Estratégias V1

### `common_repetition_frequency`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `common_repetition_frequency` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Jogos alinhados ao comportamento central recente de frequência e repetição |
| `metricas_principais` | `frequencia_por_dezena@1.0.0`, `top10_mais_sorteados@1.0.0`, `repeticao_concurso_anterior@1.0.0` |
| `search_method` | `greedy_topk` |
| `seed` | obrigatória |

**Filtros:**
- 15 dezenas únicas em `[1..25]`;
- repetição prevista `r* ∈ [Q1, Q3]` da série `repeticao_concurso_anterior`;
- ao menos 6 dezenas no `top10_mais_sorteados`.

**Score:**

```text
freq_alignment = (1/15) * Σ_{d ∈ jogo} (frequencia_por_dezena[d] / max(frequencia_por_dezena))
repeat_alignment = 1 - |r*(jogo) - mediana(repeticao_concurso_anterior)| / 15
score = 0.6 * freq_alignment + 0.4 * repeat_alignment
```

**`tie_break_rule`:** menor `hhi_concentracao.hhi_linha`; depois ordem lexicográfica ascendente.

**`interpretacao_correta`:** jogo alinhado ao perfil central de frequência e repetição da janela.

**`interpretacao_incorreta`:** jogo mais provável de sair.

---

### `row_entropy_balance`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `row_entropy_balance` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Jogos com boa dispersão por linha, sem colapsar em uma forma única |
| `metricas_principais` | `distribuicao_linha@1.0.0`, `entropia_linha@1.0.0`, `hhi_concentracao@1.0.0`, `frequencia_por_dezena@1.0.0` |
| `search_method` | `sampled` |
| `seed` | obrigatória |

**Filtros:**
- 15 dezenas únicas em `[1..25]`;
- `entropia_linha.H_norm >= 0.95`;
- `hhi_concentracao.hhi_linha <= 0.25`.

**Score:**

```text
score = freq_alignment(jogo, janela)
```

**Justificativa:** a entropia fica como filtro, não como objetivo, para evitar colapso da diversidade em `3-3-3-3-3`.

**`tie_break_rule`:** menor `hhi_concentracao.hhi_coluna`; depois ordem lexicográfica ascendente.

**`interpretacao_correta`:** jogo espacialmente balanceado nas linhas e discriminado por aderência à frequência.

**`interpretacao_incorreta`:** jogo estatisticamente superior ou com maior chance.

---

### `slot_weighted`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `slot_weighted` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Jogos aderentes ao padrão observado na matriz `dezena x slot` |
| `metricas_principais` | `matriz_numero_slot@1.0.0`, `analise_slot@1.0.0`, `surpresa_slot@1.0.0` |
| `search_method` | `exhaustive` |
| `seed` | não se aplica |

**Filtros:**
- 15 dezenas únicas em `[1..25]`;
- cada slot recebe dezenas com suporte observado na janela, salvo relaxação explicitamente reportada.

**Score:**

```text
score = analise_slot(jogo, janela)
```

**`tie_break_rule`:** menor `surpresa_slot`; depois ordem lexicográfica ascendente.

**`interpretacao_correta`:** jogo com maior aderência ao perfil histórico de slots.

**`interpretacao_incorreta`:** jogo com maior probabilidade de sair.

---

### `outlier_candidate`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `outlier_candidate` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Jogo deliberadamente distante do comportamento típico da janela |
| `metricas_principais` | `outlier_score@1.0.0`, `surpresa_slot@1.0.0`, `entropia_linha@1.0.0` |
| `search_method` | `sampled` |
| `seed` | obrigatória |

**Filtros:**
- 15 dezenas únicas em `[1..25]`;
- repetição contra o último concurso entre `0` e `15`;
- `entropia_linha.H_norm >= 0.50`.

**Score:**

```text
score = outlier_score(jogo, janela)
```

**`tie_break_rule`:** maior `surpresa_slot`; depois ordem lexicográfica ascendente.

**`interpretacao_correta`:** jogo distante do comportamento típico da janela.

**`interpretacao_incorreta`:** jogo com vantagem matemática.

---

### `declared_composite_profile`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `declared_composite_profile` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Permitir combinação dinâmica de componentes canônicos de score para geração determinística |
| `metricas_principais` | subconjunto explícito dos componentes permitidos abaixo |
| `search_method` | `sampled` ou `greedy_topk`, declarado no request |
| `seed` | obrigatória |

**Componentes de score permitidos:**

- `freq_alignment`
- `repeat_alignment`
- `slot_alignment`
- `row_entropy_norm`
- `column_entropy_norm`
- `pairs_balance_score`
- `neighbors_balance_score`
- `top10_overlap_ratio`
- `outlier_centrality`

**Definições canônicas dos componentes:**

```text
freq_alignment = (1/15) * Σ_{d ∈ jogo} (frequencia_por_dezena[d] / max(frequencia_por_dezena))
repeat_alignment = 1 - |repeticao_prevista(jogo) - mediana(repeticao_concurso_anterior)| / 15
slot_alignment = analise_slot(jogo, janela)
row_entropy_norm = entropia_linha.H_norm(jogo)
column_entropy_norm = entropia_coluna.H_norm(jogo)
pairs_balance_score = 1 - |pares(jogo) - mediana(pares_no_concurso)| / 15
neighbors_balance_score = 1 - |quantidade_vizinhos(jogo) - mediana(quantidade_vizinhos_por_concurso)| / 14
top10_overlap_ratio = |jogo ∩ top10_mais_sorteados| / 10
outlier_centrality = 1 / (1 + outlier_score(jogo, janela))
```

Todos os componentes são truncados para `[0, 1]` após a fórmula.

**Filtros:**
- 15 dezenas únicas em `[1..25]`;
- todos os componentes usados devem estar declarados no request com pesos explícitos;
- os pesos devem somar `1.0 ± 1e-9`;
- `structural_exclusions`, quando presentes, são aplicados antes do score.

**Score:**

```text
score = Σ_i w_i * component_i(jogo, janela)
```

**`tie_break_rule`:**
- primeiro menor `outlier_score`, salvo se `outlier_centrality` não estiver entre os componentes;
- depois menor `hhi_concentracao.hhi_linha`;
- depois ordem lexicográfica ascendente.

**`interpretacao_correta`:** jogo aderente ao perfil composto explicitamente declarado pelo request.

**`interpretacao_incorreta`:** jogo otimizado livremente por prompt ou jogo com chance futura aumentada.

## Orçamento e contrato com o motor

- `MAX_COUNT_PER_STRATEGY = 100`
- `MAX_TOTAL_COUNT = 250`
- estratégias `sampled` declaram `n_samples_used`
- a saída sempre inclui `strategy_name`, `strategy_version`, `seed_used`, `search_method`, `tie_break_rule` e filtros aplicados

## Matriz de prontidão para V1

| Estratégia | Score fechado? | Busca declarada? | Tie-break fechado? | Testável determinístico? | V1? |
|------------|:--------------:|:----------------:|:------------------:|:-------------------------:|:---:|
| `common_repetition_frequency` | sim | sim | sim | sim | **sim** |
| `row_entropy_balance` | sim | sim | sim | sim | **sim** |
| `slot_weighted` | sim | sim | sim | sim | **sim** |
| `outlier_candidate` | sim | sim | sim | sim | **sim** |
| `declared_composite_profile` | sim | sim | sim | sim | **sim** |
