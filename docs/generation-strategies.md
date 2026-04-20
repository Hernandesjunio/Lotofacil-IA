# Estratégias iniciais de geração

## Objetivo

Separar a semântica das estratégias de geração do contrato MCP e da implementação do motor. Cada estratégia é um **contrato formal** que o motor implementa.

## Contrato obrigatório de uma estratégia

Toda estratégia declara, sem exceção, os 11 campos abaixo. Uma estratégia sem todos os campos fechados **não é V1**.

| Campo | Obrigatório | Descrição |
|-------|:-:|-----------|
| `strategy_name` | sim | Identificador estável (`snake_case`) |
| `strategy_version` | sim | SemVer; bump de major em qualquer mudança de score ou filtro |
| `objetivo` | sim | Frase curta do que a estratégia tenta produzir |
| `metricas_principais` | sim | Lista de métricas canônicas consumidas, com versão |
| `filtros` | sim | Restrições hard (pré-seleção ou rejeição de candidatos) |
| `score` | sim | Fórmula fechada `score(game, window) → ℝ`, com range declarado |
| `search_method` | sim | `"exhaustive" \| "sampled" \| "greedy_topk"` (ADR 0001 D3) |
| `seed` | sim quando `search_method ≠ "exhaustive"` | `uint64` passada do input da tool |
| `tie_break_rule` | sim | Critério determinístico de desempate |
| `interpretacao_correta` | sim | Como ler a saída |
| `interpretacao_incorreta` | sim | Leitura enganosa mais comum |

Regras de evolução:

1. Fórmula de score documentada e testável.
2. Payload de explicação (campos `scores.*` e `rationale`) definido.
3. Teste determinístico mínimo: dado `{seed, window, dataset_version}`, a saída é idêntica entre execuções.
4. Sem termos ambíguos ("centro", "aderência", "equilíbrio") sem operacionalização formal.

## Estratégias V1

### `common_repetition_frequency`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `common_repetition_frequency` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Jogos alinhados ao comportamento central recente de frequência e repetição. |
| `metricas_principais` | `frequencia_por_dezena@1.0.0`, `top10_mais_sorteados@1.0.0`, `repeticao_concurso_anterior@1.0.0` |
| `search_method` | `greedy_topk` |
| `seed` | obrigatória (usada só em desempate de alternativas com mesmo score) |

**Filtros (hard):**
- Jogo tem exatamente 15 dezenas únicas em `[1..25]`.
- Repetição prevista contra o último concurso da janela `r* ∈ [Q1, Q3]` da série `repeticao_concurso_anterior` na janela (ADR 0001 D16). `Q1, Q3` calculados com método linear (tipo 7 do R / default do NumPy).
- Jogo contém no mínimo 6 dezenas do `top10_mais_sorteados` da janela.

**Score:**
```
freq_alignment = (1/15) · Σ_{d ∈ jogo} (frequencia_por_dezena[d] / max(frequencia_por_dezena))
repeat_alignment = 1 - |r*(jogo) - mediana(repeticao_concurso_anterior)| / 15
score = 0.6 · freq_alignment + 0.4 · repeat_alignment      # range [0, 1]
```
Pesos `0.6 / 0.4` são definição da estratégia (congelados em `1.0.0`), não parâmetro de entrada.

**`tie_break_rule`:** menor `hhi_concentracao.hhi_linha`; se empatar, ordem lexicográfica do jogo ordenado ascendente.

**`interpretacao_correta`:** "jogo alinhado ao perfil central de frequência e repetição da janela analisada".

**`interpretacao_incorreta`:** "jogo mais provável de sair" — leitura proibida (viola invariante 6 do contrato MCP).

---

### `row_entropy_balance`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `row_entropy_balance` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Jogos com distribuição equilibrada entre linhas do volante e dispersão preservada, sem colapsar em uma única configuração. |
| `metricas_principais` | `distribuicao_linha@1.0.0`, `entropia_linha@1.0.0`, `hhi_concentracao@1.0.0`, `frequencia_por_dezena@1.0.0`, `top10_mais_sorteados@1.0.0` |
| `search_method` | `sampled` |
| `seed` | obrigatória |

**Filtros (hard, ADR 0001 D17):**
- Jogo tem exatamente 15 dezenas únicas em `[1..25]`.
- `entropia_linha.H_norm(jogo) ≥ 0.95` (entropia é filtro, **não** objetivo).
- `hhi_concentracao.hhi_linha(jogo) ≤ 0.25`.

**Score (primário):**
```
score = freq_alignment(jogo, janela)                       # mesma fórmula da estratégia anterior, range [0, 1]
```

**Justificativa de desacoplar score e entropia:** maximizar `H_norm` isoladamente tem ótimo único em `3-3-3-3-3`, colapsando a diversidade do conjunto retornado. Usar entropia como filtro preserva diversidade; `freq_alignment` como score primário torna a estratégia discriminativa entre candidatos qualificados.

**`tie_break_rule`:** menor `hhi_concentracao.hhi_coluna`; se empatar, ordem lexicográfica do jogo ordenado ascendente.

**`interpretacao_correta`:** "jogo com composição espacial balanceada entre as 5 linhas do volante, discriminado por aderência à frequência recente".

**`interpretacao_incorreta`:** "jogo estatisticamente superior" ou "jogo com maior chance".

---

### `slot_weighted`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `slot_weighted` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Jogos aderentes ao padrão observado na matriz `dezena × slot` da janela. |
| `metricas_principais` | `matriz_numero_slot@1.0.0`, `analise_slot@1.0.0`, `surpresa_slot@1.0.0` |
| `search_method` | `greedy_topk` (enumeração viável do vetor de slots via programação dinâmica; amostragem de empates com seed) |
| `seed` | obrigatória |

**Filtros (hard):**
- Jogo tem exatamente 15 dezenas únicas em `[1..25]`.
- Quando ordenado ascendente, cada slot `s` recebe apenas dezenas `d` com `M[d, s] > 0` na janela (ou seja, a dezena já ocupou aquele slot em algum concurso da janela). Caso relaxação seja necessária (janela muito curta), o servidor sinaliza `relaxed_slot_support=true` no payload.

**Score:**
```
score = analise_slot(jogo, janela)                         # range [0, 1]
```

**`tie_break_rule`:** menor `surpresa_slot` (menos raro); se empatar, ordem lexicográfica.

**`interpretacao_correta`:** "jogo cuja composição, avaliada posição-a-posição após ordenação crescente, tem maior aderência ao perfil histórico de slots da janela".

**`interpretacao_incorreta`:** "jogo com maior probabilidade de sair" ou "jogo que 'respeita' o sorteio".

**Dependência crítica:** `slot = posição ordenada no resultado` (ver `metric-catalog.md`, observações críticas).

---

### `outlier_candidate`

| Campo | Valor |
|-------|-------|
| `strategy_name` | `outlier_candidate` |
| `strategy_version` | `1.0.0` |
| `objetivo` | Jogo deliberadamente distante do comportamento típico da janela, ainda válido pelas restrições básicas. |
| `metricas_principais` | `outlier_score@1.0.0` (com dependência em `surpresa_slot`, `entropia_linha`, `frequencia_por_dezena`, `repeticao_concurso_anterior`) |
| `search_method` | `sampled` |
| `seed` | obrigatória |

**Filtros (hard):**
- Jogo tem exatamente 15 dezenas únicas em `[1..25]`.
- Repetição contra o último concurso `∈ [0, 15]` (aceita até 0 repetições — único filtro; a estratégia é deliberadamente permissiva nessa dimensão).
- `entropia_linha.H_norm(jogo) ≥ 0.50` (evita jogos patologicamente concentrados em uma linha só, que são triviais mas não interessantes).

**Score:**
```
score = outlier_score(jogo, janela)                        # range [0, +∞), tipicamente [0, ~5]
```

**`tie_break_rule`:** maior `surpresa_slot`; se empatar, ordem lexicográfica do jogo ordenado ascendente.

**`interpretacao_correta`:** "jogo distante do comportamento típico da janela, segundo distância de Mahalanobis sobre 5 features canônicas".

**`interpretacao_incorreta`:** "jogo com vantagem matemática" ou "jogo que 'quebra o padrão' com significado preditivo".

---

## Orçamento e contrato com o motor

- `MAX_COUNT_PER_STRATEGY = 100` (ADR 0001 D11). O input de `generate_candidate_games` é rejeitado com `PLAN_BUDGET_EXCEEDED` se `count` de alguma estratégia exceder.
- `MAX_TOTAL_COUNT = 250` soma todas as estratégias do `plan`.
- Para `search_method = "sampled"`, o motor declara `n_samples_used` no payload de cada estratégia.
- A saída de `generate_candidate_games` inclui, por estratégia: `strategy_name`, `strategy_version`, `seed_used`, `search_method`, `n_samples_used` (se aplicável), e `tie_break_rule`.

## Matriz de prontidão para V1

| Estratégia | Score fechado? | Busca declarada? | Tie-break fechado? | Testável determinístico? | V1? |
|------------|:--------------:|:----------------:|:------------------:|:-------------------------:|:---:|
| `common_repetition_frequency` | sim | sim | sim | sim | **sim** |
| `row_entropy_balance` | sim | sim | sim | sim | **sim** |
| `slot_weighted` | sim | sim | sim | sim | **sim** |
| `outlier_candidate` | sim (via `outlier_score@1.0.0`) | sim | sim | sim | **sim** |

Antes do fechamento semântico (pré-ADR 0001), `slot_weighted` e `outlier_candidate` estavam em `preview`; após a formalização do `outlier_score` por Mahalanobis e do `analise_slot` como score de aderência por slot, ambas entram na V1.
