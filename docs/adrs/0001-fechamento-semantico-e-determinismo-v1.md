# ADR 0001 — Fechamento semântico e determinismo da V1

**Navegação:** [← Brief (índice)](../brief.md) · [README](../../README.md)

- **Status:** Aceito
- **Data:** 2026-04-20
- **Escopo:** [brief.md](../brief.md), [project-guide.md](../project-guide.md), [metric-catalog.md](../metric-catalog.md), [generation-strategies.md](../generation-strategies.md), [mcp-tool-contract.md](../mcp-tool-contract.md)
- **Contexto de revisão:** análise crítica cruzada dos documentos-base antes da primeira implementação

> Nota: a ampliação da V1 para composições dinâmicas, associações entre indicadores, séries estruturais adicionais, filtros estruturais e catálogo de prompts foi registrada separadamente em [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md). Este ADR permanece como a base do fechamento semântico e do determinismo inicial.

## Contexto

Antes de escrever qualquer linha de código da V1, os documentos foram revisados com dois critérios:

1. **Determinismo operacional** — o brief promete "mesma entrada ⇒ mesma saída", mas nem todos os insumos para garantir isso estavam declarados.
2. **Rigor estatístico** — métricas com fórmula incompleta ou ambígua tornam o sistema dependente de escolhas implícitas de implementação, o que invalida o invariante principal do contrato MCP.

A revisão encontrou 3 classes de problemas:

- **Determinismo incompleto** (ausência de `seed`, `deterministic_hash` sem definição, `dataset_version` sem regra, `search_method` não declarado nas estratégias).
- **Fórmulas estatísticas em aberto** (`coeficiente_variacao` default instável, `divergencia_kl` sem smoothing, `tendencia_linear` sem eixo-x, `outlier_score` circular, entropias sem base, `zscore_repeticao` e `persistencia_atraso_extremo` sem referência declarada).
- **Contrato MCP permissivo com exclusão em runtime** (aceita indicadores vetoriais no input, exclui em runtime; não tem orçamento em `plan.count`; `MetricValue.shape` não existe; códigos de erro incompletos).

Este ADR consolida as decisões tomadas. Em versões futuras, decisões estruturais que surgirem individualmente devem ser registradas como ADRs novos (`0002`, `0003`, ...).

## Decisões

### D1 — Determinismo por contrato com `seed` e hash canônico

**Decisão:** toda ferramenta com componente estocástico ou de desempate aceita `seed: uint64` como entrada. Toda resposta retorna `deterministic_hash = SHA256(canonical_json({input, dataset_version, tool_version}))`, onde `canonical_json` segue RFC 8785 (JCS).

**Justificativa:** sem `seed`, `generate_candidate_games` com qualquer tie-break não-trivial pode divergir entre execuções, violando a promessa de reprodutibilidade do brief. Sem regra de hash, duas implementações corretas geram hashes diferentes para o mesmo output — o hash vira decorativo.

**Evidência de impacto:** `C(25,15) = 3.268.760` combinações; qualquer estratégia que amostre do espaço sem seed tem entropia de saída suficiente para gerar jogos distintos em execuções consecutivas.

**Arquivos alterados:** `mcp-tool-contract.md` (seção `generate_candidate_games`, invariantes, persistência), `generation-strategies.md` (campos obrigatórios), `brief.md` (premissas).

### D2 — `dataset_version` derivado determinístico

**Decisão:** `dataset_version = "cef-" + YYYY-MM-DD + "-sha" + primeiros 8 hex de SHA256 do arquivo fonte`. Exemplo: `cef-2026-04-20-sha7c3a91b2`.

**Justificativa:** rastreabilidade de qual snapshot de dados produziu qual resposta é pré-requisito para reprodutibilidade. Hash do arquivo garante que reingestão do mesmo conteúdo mantém a mesma versão; data humaniza.

**Arquivos alterados:** `mcp-tool-contract.md` (seção de persistência e exemplo de output).

### D3 — `search_method` explícito em cada estratégia

**Decisão:** todo contrato de estratégia declara `search_method ∈ { "exhaustive", "sampled", "greedy_topk" }`. Para `sampled`, declarar `n_samples` e consumir `seed`. `exhaustive` é preferido sempre que o score for O(1) por jogo candidato e couber em orçamento de latência.

**Reclassificação em revisão (pós-ADR 0001):** `slot_weighted` foi revisto de `greedy_topk` para `exhaustive`. Como o score `analise_slot = (1/15) · Σ_s p_smooth(g_s, s)` é separável por slot e a única restrição é `g_1 < g_2 < … < g_15` com `g_i ∈ [1..25]`, o ótimo global é computável por programação dinâmica em `O(25² · 15) ≈ 9.375` operações — trivialmente dentro do orçamento. Fica dentro da preferência declarada nesta decisão. `seed` deixa de ser obrigatória para a estratégia (ADR 0001 D3 não exige `seed` em `exhaustive`); determinismo é garantido pela DP + `tie_break_rule` lexicográfico.

**Justificativa:** duas implementações corretas da mesma estratégia, mas com métodos diferentes de busca, produzem saídas diferentes com o mesmo `seed`. Fixar o método é condição necessária para o invariante 1.

**Arquivos alterados:** `generation-strategies.md`, `mcp-tool-contract.md` (exemplo de `generate_candidate_games.summary.strategies`).

### D4 — CV sai como default de estabilidade

**Decisão:** `normalization_method` default em `analyze_indicator_stability` passa de `coefficient_of_variation` para `madn` (MAD normalizado pela mediana: `MAD / mediana`). CV continua disponível como `opt-in` e é **rejeitado** quando a série admite valores ≤ 0 (ex.: `tendencia_linear`), com erro `UNSUPPORTED_NORMALIZATION_METHOD`.

**Justificativa estatística:**

- CV só tem interpretação válida em variáveis positivas com escala de razão (Everitt, *Cambridge Dictionary of Statistics*).
- Quando `μ ≈ 0`, CV amplifica ruído numérico: uma série oscilando em torno de zero recebe CV gigantesco, dando falso sinal de "alta volatilidade".
- MADN é robusto a outliers e finito sempre que a mediana da série for > 0; quando a mediana é 0, o servidor usa `IQR / |mediana + ε|` como fallback robusto declarado.

**Arquivos alterados:** `mcp-tool-contract.md` (`analyze_indicator_stability`), `metric-catalog.md` (observações críticas).

### D5 — Smoothing obrigatório em log-probabilidades (KL, surpresa_slot)

**Decisão:** toda métrica baseada em `log p` aplica **add-α smoothing com α = 1/|support|** (Laplace ajustado). Suporte é declarado na métrica e descreve a cardinalidade do espaço amostral da distribuição sobre a qual o `log` é aplicado:
- `divergencia_kl` entre distribuições de frequência por dezena: `|support| = 25`, `α = 1/25`.
- `surpresa_slot`: há **15 distribuições independentes, uma por slot**, cada uma com espaço amostral de 25 dezenas. Cada distribuição aplica `α = 1/25` isoladamente (`p_smooth(d,s) = (M[d,s] + α) / (Σ_d M[d,s] + 25α)`). Não há suporte conjunto `25 × 15`; o produto `Π_s p_smooth(g_s, s)` é a composição de 15 probabilidades condicionadas ao slot.

**Justificativa estatística:** em janela típica de `N=20`, a probabilidade de pelo menos uma dezena ter frequência 0 é praticamente 1 (só seria zero se as 15 dezenas sorteadas fossem exatamente as mesmas 15 em todos os 20 concursos, o que equivale a repetição total — cenário de probabilidade desprezível). Sem smoothing, `D_KL = +∞` é regra, não exceção.

**Arquivos alterados:** `metric-catalog.md` (fórmulas), `mcp-tool-contract.md` (observações).

### D6 — Entropia em bits normalizada

**Decisão:** entropias usam **base 2 (bits)** e são reportadas também na forma normalizada `H_norm = H / log₂(K)`, onde `K` é a cardinalidade do suporte (5 para linhas/colunas).

**Justificativa:** `H_norm ∈ [0, 1]` é diretamente comparável entre implementações, invariante a escolha de base, e facilita scoring em estratégias. Mantém `H` em bits para auditoria.

**Arquivos alterados:** `metric-catalog.md`.

### D7 — `tendencia_linear` com eixo-x canônico

**Decisão:** `tendencia_linear` usa `x = 0..N-1` (offset dentro da janela), `y = valor do indicador`. Retorna `slope` em "variação por concurso".

**Justificativa:** sem fixar o eixo, uma implementação com `x = contest_id` e outra com `x = datetime` devolvem inclinações distintas em magnitude. Offset local torna a métrica comparável entre janelas de tamanhos diferentes e independente de gaps temporais do calendário.

**Arquivos alterados:** `metric-catalog.md`.

### D8 — `zscore_repeticao` e `persistencia_atraso_extremo` com referência explícita

**Decisão:** ambas as métricas passam a requerer os campos `reference: "window" | "historical" | "declared_baseline"` e `baseline_version`. Default é `historical` com `baseline_version` atrelado ao `dataset_version`.

**Justificativa:** sem referência declarada, a métrica alterna silenciosamente entre "outlier relativo à janela" e "outlier histórico" conforme a implementação. São fenômenos semanticamente distintos.

**Arquivos alterados:** `metric-catalog.md`.

### D9 — `outlier_score` via distância de Mahalanobis regularizada

**Decisão:** `outlier_score(x) = sqrt((x - μ)ᵀ (Σ + λI)⁻¹ (x - μ))`, onde:
- `x` é o vetor de features do jogo candidato: `[freq_alignment, slot_alignment, row_entropy_norm, pairs_ratio, repeticao_anterior]`
- `μ, Σ` são estimados sobre os jogos da janela (após transformação das features)
- `λ = 1e-3 × traço(Σ)/dim` (regularização adaptativa, evita singularidade)

**Justificativa:**
- Sai da circularidade "métrica composta com pesos arbitrários definidos na estratégia que a usa".
- Referência estatística consagrada (Mahalanobis, 1936), bem suportada em bibliotecas.
- A matriz `Σ` carrega as correlações entre features — pesos implícitos, não arbitrários.
- Testável com valores esperados para jogos sintéticos.

**Status:** `canonica (v1.0)` a partir deste ADR. Permite promover `outlier_candidate` de `preview` para V1 após teste determinístico mínimo.

**Arquivos alterados:** `metric-catalog.md`, `generation-strategies.md`.

### D10 — `MetricValue.shape` explícito e validação de entrada

**Decisão:** `MetricValue` ganha `shape ∈ { "scalar", "series", "vector_by_dezena", "count_vector[5]", "count_matrix[25x15]", "count_pair", "dezena_list[10]", "count_list_by_dezena", "dimensionless_pair" }`. `value` é tipado conforme `shape`. Em `analyze_indicator_stability`, se o indicador é vetorial ou matricial (`vector_by_dezena`, `count_vector[5]`, `count_matrix[25x15]`, `count_list_by_dezena`, `count_pair`, `dezena_list[10]`, `dimensionless_pair`), o input **deve** declarar `aggregation ∈ { "mean", "max", "l2_norm", "per_component" }`; caso contrário, erro `UNSUPPORTED_AGGREGATION` antes de calcular.

**Nota de rastreabilidade:** a revisão do ADR manteve o enum alinhado ao contrato MCP ([mcp-tool-contract.md](../mcp-tool-contract.md)). A versão inicial deste ADR listava apenas `{ scalar, series, vector_by_dezena, matrix_dezena_slot }`; foi expandida para refletir todos os shapes reais emitidos por métricas canônicas do catálogo (`count_vector[5]` para distribuição linha/coluna, `count_pair` para pares/ímpares e runs, `dezena_list[10]` para os tops, `count_list_by_dezena` para blocos, `dimensionless_pair` para `hhi_concentracao`). `matrix_dezena_slot` foi renomeado para `count_matrix[25x15]` para eliminar o alias informal.

**Justificativa:** o contrato aceitava indicadores vetoriais e os excluía em runtime — isso é contrato permissivo com descoberta tardia. IA consumidora paga um round-trip para aprender a restrição. Validação em schema elimina a classe inteira.

**Arquivos alterados:** `mcp-tool-contract.md` (modelo `MetricValue`, `analyze_indicator_stability`, códigos de erro).

### D11 — Orçamento obrigatório em `generate_candidate_games`

**Decisão:** `plan.count` é limitado por estratégia a `MAX_COUNT_PER_STRATEGY = 100` (ajustável em `appsettings.json`). Soma total do plano é limitada a `MAX_TOTAL_COUNT = 250`. Violação retorna `PLAN_BUDGET_EXCEEDED` com detalhes.

**Justificativa:** sem orçamento por requisição, um cliente pode pedir 10⁴ jogos numa estratégia que exige cálculo de KL por jogo candidato, travando o host. `Access/` previne excesso de *requisições*, não de *custo por requisição*.

**Arquivos alterados:** `mcp-tool-contract.md` (`generate_candidate_games`, códigos de erro).

### D12 — Lista de códigos de erro completa

**Decisão:** adiciona `UNAUTHORIZED`, `RATE_LIMITED`, `QUOTA_EXCEEDED`, `DATASET_UNAVAILABLE`, `PLAN_BUDGET_EXCEEDED`, `UNSUPPORTED_AGGREGATION`, `INTERNAL_ERROR`. Cada ferramenta declara explicitamente o subconjunto de códigos que pode emitir.

**Justificativa:** [project-guide.md](../project-guide.md) já previa autenticação e throttling. O contrato não listava nenhum código correspondente — inconsistência entre guide e contrato.

**Arquivos alterados:** `mcp-tool-contract.md`.

### D13 — Remover `scope: draw` do `MetricValue` até existir métrica com esse escopo

**Decisão:** `MetricValue.scope` passa a aceitar apenas `{ "window", "candidate_game", "series" }`. `draw` é reservado para evolução futura e só volta ao enum quando existir ao menos uma métrica canônica com esse escopo no `metric-catalog.md`.

**Justificativa:** campo presente no contrato sem uso declarado é armadilha para IA consumidora. Melhor cortar até ter exemplo concreto.

**Arquivos alterados:** `mcp-tool-contract.md`.

### D14 — Default explícito de `metrics` em `compute_window_metrics`

**Decisão:** `metrics` é **obrigatório** no input. Omissão retorna `INVALID_REQUEST` com `details.missing_field = "metrics"`. Não existe "conjunto default implícito" nem código dedicado `MISSING_METRICS`; a validação de schema é tratada uniformemente por `INVALID_REQUEST`, conforme tabela de códigos em [mcp-tool-contract.md](../mcp-tool-contract.md).

**Justificativa:** default implícito obriga IA a inferir quais métricas "sempre fazem sentido" — fonte típica de drift. Forçar a escolha torna cada chamada autodocumentada.

**Arquivos alterados:** `mcp-tool-contract.md`.

### D15 — `explain_candidate_games` retorna `candidate_strategies` ranqueadas

**Decisão:** substituir `dominant_strategy` (string única) por `candidate_strategies: [{ strategy_name, strategy_version, score }]` ordenado decrescente por score. O consumidor decide se usa o topo, limiar mínimo ou mostra todos.

**Justificativa:** "estratégia dominante" obriga tie-break arbitrário no servidor. Entregar o ranking completo é mais informativo, elimina o tie-break do servidor e permite que a IA consumidora tome decisão contextual.

**Arquivos alterados:** `mcp-tool-contract.md`.

### D16 — `common_repetition_frequency` com faixa central formal

**Decisão:** "faixa central" na estratégia passa a ser **`[Q1, Q3]`** da série `repeticao_concurso_anterior` na janela. Filtro: jogo candidato deve ter repetição prevista `r* ∈ [Q1, Q3]` contra o último concurso.

**Justificativa:** elimina ambiguidade entre "mediana ± σ", "média ± σ" e "quartis". Quartis são robustos e interpretáveis.

**Arquivos alterados:** `generation-strategies.md`.

### D17 — `row_entropy_balance` com entropia como filtro, não objetivo

**Decisão:** entropia normalizada `H_norm(linhas) ≥ 0.95` vira filtro de qualificação; score primário passa a ser `freq_alignment` entre o jogo e o `top10_mais_sorteados` da janela. Tie-break: menor `hhi_concentracao.hhi_coluna` (fonte canônica do desempate: [generation-strategies.md](../generation-strategies.md), estratégia `row_entropy_balance`); se empatar, ordem lexicográfica do jogo ordenado ascendente.

**Justificativa:** maximizar `H_norm` de 5 bins tem ótimo único em `3-3-3-3-3`, o que **colapsa** a diversidade do conjunto retornado — problema contrário ao objetivo da estratégia. Transformar em filtro preserva diversidade; score primário torna a estratégia discriminativa.

**Arquivos alterados:** `generation-strategies.md`.

### D18 — Fronteira da janela em `repeticao_concurso_anterior`

**Decisão:** quando `end_contest_id - window_size + 1 > 1` (janela não toca o primeiro concurso do histórico), o primeiro elemento da série usa o concurso imediatamente anterior **fora** da janela como `t-1`. Caso contrário, a série tem comprimento `N - 1`.

**Justificativa:** sem regra, `mean` e `std` da série ficam dependentes da implementação (comprimento `N` vs `N-1`). Regra fixa torna teste determinístico trivial.

**Arquivos alterados:** `metric-catalog.md`.

### D19 — Catálogo ganha colunas `Unidade` e `Versão`

**Decisão:** tabela principal de `metric-catalog.md` ganha duas colunas. `Versão` segue SemVer por métrica e é propagada para `MetricValue.version` no output. Mudança de fórmula exige bump de major; mudança de apresentação, minor.

**Justificativa:** `MetricValue.unit` e `MetricValue.version` já eram campos mínimos no contrato; não estavam no catálogo. Alinha contrato e catálogo.

**Arquivos alterados:** `metric-catalog.md`.

### D20 — Ajustes em `brief.md` e `project-guide.md`

- Remover qualificador ambíguo em [brief.md](../brief.md) sobre validação explícita.
- Ajustar [project-guide.md](../project-guide.md) referenciando [metric-catalog.md](../metric-catalog.md) (nome correto) e `decisions.md` movido para [adrs/](../adrs/).
- Adicionar nota em `project-guide.md` sobre responsabilidade do `Core/` de normalizar (ordenar) entrada canônica — fecha a fronteira com `Providers/` e sustenta a invariante do `slot`.

## Alternativas consideradas e rejeitadas

- **Manter CV como default** por familiaridade — rejeitado: problema matemático real em séries com média próxima de zero, visível em `tendencia_linear`.
- **`outlier_score` como combinação ponderada com pesos declarados na estratégia** — rejeitado: introduz circularidade (estratégia declara pesos que a métrica consome, métrica define espaço que a estratégia navega), e torna a métrica 4 vezes (uma por estratégia que a usar) em vez de uma só.
- **Entropia em base `e`** — rejeitado: convenção em ML/IA é bits; nats facilita prova de teoremas mas não facilita comparação numérica entre implementações.
- **`seed` opcional com default `0`** — rejeitado: default invisível é pior que ausência. Exigir explícito força a IA consumidora a decidir e documentar.

## Consequências

**Positivas:**
- V1 fica testável por reprodutibilidade: dado `{input, seed, dataset_version}`, o `deterministic_hash` é verificável.
- `analyze_indicator_stability` deixa de depender de CV frágil.
- `outlier_candidate` sai de `preview` e entra na V1 com base matemática auditável.
- IA consumidora recebe contrato estrito: shape tipado, erros mapeados, sem descoberta em runtime.

**Negativas / custos:**
- Cada estratégia ganha 3 campos obrigatórios (`seed`, `search_method`, `tie_break_rule`) — mais verbosidade nos inputs.
- Implementar Mahalanobis exige dependência de álgebra linear (aceitável em .NET via `MathNet.Numerics`).
- Smoothing muda valores de KL em relação a implementações ingênuas — exige documentação clara da convenção.

**Neutras:**
- Decisões futuras podem revisar `MAX_COUNT_PER_STRATEGY` conforme observabilidade mostrar uso real.
- Default de `reference` em `zscore_repeticao` pode ser revisto se `historical` mostrar-se custoso; é decisão operacional, não semântica.

## Testes mínimos que este ADR habilita

1. **Determinismo cross-run:** `get_draw_window → compute_window_metrics → analyze_indicator_stability` com mesmo `seed` e mesmo `dataset_version` produz `deterministic_hash` idêntico em 100% das execuções.
2. **Smoothing ativo:** `divergencia_kl` nunca retorna `+∞` nem `NaN` para janelas `N ≥ 5`.
3. **Validação de shape:** `analyze_indicator_stability` com `frequencia_por_dezena` sem `aggregation` retorna `UNSUPPORTED_AGGREGATION`, não falha silenciosa.
4. **Orçamento:** `generate_candidate_games` com `plan.count = 101` em qualquer estratégia retorna `PLAN_BUDGET_EXCEEDED`.
5. **Explicação ranqueada:** `explain_candidate_games` retorna `candidate_strategies` com ao menos 2 entradas em ordem decrescente por score.
6. **`row_entropy_balance` diversificado:** 10 execuções produzem ≥ 5 jogos distintos (antes da mudança: risco de colapso para `3-3-3-3-3`).

## Rastreabilidade

| ID | Documento afetado | Seções tocadas |
|----|-------------------|----------------|
| D1 | contract, strategies, brief | `generate_candidate_games`, invariantes, premissas |
| D2 | contract | persistência, output de `get_draw_window` |
| D3 | strategies | cabeçalho, todas as 4 estratégias-base |
| D4 | contract, catalog | `analyze_indicator_stability`, observações |
| D5 | catalog, contract | `divergencia_kl`, `surpresa_slot`, observações |
| D6 | catalog | `entropia_linha`, `entropia_coluna` |
| D7 | catalog | `tendencia_linear` |
| D8 | catalog | `zscore_repeticao`, `persistencia_atraso_extremo` |
| D9 | catalog, strategies | `outlier_score`, `outlier_candidate` |
| D10 | contract | `MetricValue`, `analyze_indicator_stability`, erros |
| D11 | contract | `generate_candidate_games`, erros |
| D12 | contract | erros |
| D13 | contract | `MetricValue.scope` |
| D14 | contract | `compute_window_metrics` |
| D15 | contract | `explain_candidate_games` |
| D16 | strategies | `common_repetition_frequency` |
| D17 | strategies | `row_entropy_balance` |
| D18 | catalog | `repeticao_concurso_anterior` |
| D19 | catalog | tabela principal |
| D20 | brief, project-guide | escopo, nomes, fronteira Core/Providers |
