# Catálogo de métricas

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Fonte de verdade semântica das métricas da V1 ampliada. Alinha-se a [mcp-tool-contract.md](mcp-tool-contract.md), [generation-strategies.md](generation-strategies.md) e aos ADRs [0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md) e [0002](adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md).

Para **interpretação em linguagem simples**, **o que cada métrica observa** e **exemplos de uso** por nome, ver [metric-glossary.md](metric-glossary.md) (complemento pedagógico; fórmulas e versões permanecem aqui).

## Convenções

| Campo | Descrição |
|-------|-----------|
| **Nome** | Identificador estável (`snake_case`). |
| **Categoria** | `base`, `derivada`, `apoio`, `geração`, `estabilidade` ou `composição`. |
| **Janela** | `configurável`, `não se aplica` ou regra específica. |
| **Scope** | `window`, `series` ou `candidate_game`. |
| **Unidade** | `count`, `ratio`, `bits`, `dimensionless` ou `herda do indicador`. |
| **Shape** | `scalar`, `series`, `vector_by_dezena`, `count_vector[5]`, `series_of_count_vector[5]`, `count_matrix[25x15]`, `count_pair`, `dezena_list[10]`, `count_list_by_dezena` ou `dimensionless_pair`. |
| **Versão** | SemVer por métrica. |
| **Status** | `canonica`, `derivada` ou `pendente de detalhamento`. |

## Regras de uso

- Janela nunca é implícita.
- Toda composição dinâmica deve declarar agregação, transformação e referência quando a métrica não for escalar.
- Estabilidade descritiva usa `madn_janela` como default; `coeficiente_variacao` é opt-in e restrito a séries positivas.
- Toda distribuição em `log` usa add-α smoothing com `α = 1 / |support|`.
- Entropias são reportadas em bits e, quando aplicável, com `H_norm`.
- "Persistência" significa regularidade observada na janela ou no histórico declarado; não implica previsão.
- Séries derivadas de concursos continuam com `scope = "series"`; o catálogo não reintroduz `scope = "draw"`.

## Tabela 1 — Identificação e tipagem

| Nome | Categoria | Janela | Scope | Unidade | Shape | Versão | Status |
|------|-----------|--------|-------|---------|-------|--------|--------|
| `frequencia_por_dezena` | base | configurável | `window` | `count` | `vector_by_dezena` | 1.0.0 | canonica |
| `top10_mais_sorteados` | derivada | configurável | `window` | `dimensionless` | `dezena_list[10]` | 1.0.0 | canonica |
| `top10_menos_sorteados` | derivada | configurável | `window` | `dimensionless` | `dezena_list[10]` | 1.0.0 | canonica |
| `repeticao_concurso_anterior` | base | configurável | `series` | `count` | `series` | 1.0.0 | canonica |
| `intersecoes_multiplas` | apoio | configurável | `series` | `count` | `series` | 1.0.0 | derivada |
| `atraso_por_dezena` | base | histórico acumulado ou janela declarada | `window` | `count` | `vector_by_dezena` | 1.0.0 | canonica |
| `frequencia_blocos` | apoio | configurável | `window` | `count` | `count_list_by_dezena` | 1.0.0 | derivada |
| `ausencia_blocos` | apoio | configurável | `window` | `count` | `count_list_by_dezena` | 1.0.0 | derivada |
| `estado_atual_dezena` | apoio | configurável | `window` | `count` | `vector_by_dezena` | 1.0.0 | derivada |
| `pares_impares` | geração | não se aplica | `candidate_game` | `count` | `count_pair` | 1.0.0 | canonica |
| `pares_no_concurso` | apoio | configurável | `series` | `count` | `series` | 1.0.0 | canonica |
| `quantidade_vizinhos` | geração | não se aplica | `candidate_game` | `count` | `scalar` | 1.0.0 | canonica |
| `quantidade_vizinhos_por_concurso` | apoio | configurável | `series` | `count` | `series` | 1.0.0 | canonica |
| `sequencia_maxima_vizinhos` | geração | não se aplica | `candidate_game` | `count` | `scalar` | 1.0.0 | canonica |
| `sequencia_maxima_vizinhos_por_concurso` | apoio | configurável | `series` | `count` | `series` | 1.0.0 | canonica |
| `distribuicao_linha` | geração | não se aplica | `candidate_game` | `count` | `count_vector[5]` | 1.0.0 | canonica |
| `distribuicao_linha_por_concurso` | apoio | configurável | `series` | `count` | `series_of_count_vector[5]` | 1.0.0 | canonica |
| `distribuicao_coluna` | geração | não se aplica | `candidate_game` | `count` | `count_vector[5]` | 1.0.0 | canonica |
| `distribuicao_coluna_por_concurso` | apoio | configurável | `series` | `count` | `series_of_count_vector[5]` | 1.0.0 | canonica |
| `entropia_linha` | geração | não se aplica | `candidate_game` | `bits` | `scalar` | 1.0.0 | canonica |
| `entropia_linha_por_concurso` | apoio | configurável | `series` | `bits` | `series` | 1.0.0 | canonica |
| `entropia_coluna` | geração | não se aplica | `candidate_game` | `bits` | `scalar` | 1.0.0 | canonica |
| `entropia_coluna_por_concurso` | apoio | configurável | `series` | `bits` | `series` | 1.0.0 | canonica |
| `hhi_concentracao` | geração | não se aplica | `candidate_game` | `dimensionless` | `dimensionless_pair` | 1.0.0 | canonica |
| `hhi_linha_por_concurso` | apoio | configurável | `series` | `dimensionless` | `series` | 1.0.0 | canonica |
| `hhi_coluna_por_concurso` | apoio | configurável | `series` | `dimensionless` | `series` | 1.0.0 | canonica |
| `matriz_numero_slot` | base | configurável | `window` | `count` | `count_matrix[25x15]` | 1.0.0 | canonica |
| `analise_slot` | geração | configurável | `candidate_game` | `dimensionless` | `scalar` | 1.0.0 | canonica |
| `surpresa_slot` | geração | configurável | `candidate_game` | `bits` | `scalar` | 1.0.0 | canonica |
| `intersecao_conjunto_referencia` | apoio | não se aplica | `candidate_game` | `count` | `scalar` | 1.0.0 | canonica |
| `media_janela` | estabilidade | configurável | `window` | `herda do indicador` | `scalar` | 1.0.0 | canonica |
| `desvio_padrao_janela` | estabilidade | configurável | `window` | `herda do indicador` | `scalar` | 1.0.0 | canonica |
| `coeficiente_variacao` | estabilidade | configurável | `window` | `dimensionless` | `scalar` | 2.0.0 | canonica |
| `madn_janela` | estabilidade | configurável | `window` | `dimensionless` | `scalar` | 1.0.0 | canonica |
| `mad_janela` | estabilidade | configurável | `window` | `herda do indicador` | `scalar` | 1.0.0 | canonica |
| `tendencia_linear` | estabilidade | configurável | `window` | `herda do indicador / concurso` | `scalar` | 1.0.0 | canonica |
| `estabilidade_ranking` | estabilidade | configurável | `window` | `dimensionless` | `scalar` | 0.1.0 | pendente de detalhamento |
| `divergencia_kl` | estabilidade | comparação entre 2 janelas | `window` | `bits` | `scalar` | 1.0.0 | canonica |
| `zscore_repeticao` | estabilidade | configurável | `window` | `dimensionless` | `scalar` | 2.0.0 | canonica |
| `persistencia_atraso_extremo` | estabilidade | configurável | `window` | `count` | `scalar` | 2.0.0 | canonica |
| `assimetria_blocos` | estabilidade | configurável | `window` | `dimensionless` | `scalar` | 1.0.0 | derivada |
| `estatistica_runs` | apoio | não se aplica | `candidate_game` | `count` | `count_pair` | 1.0.0 | canonica |
| `outlier_score` | geração | configurável | `candidate_game` | `dimensionless` | `scalar` | 1.0.0 | canonica |

## Tabela 2 — Semântica

| Nome | Definição | Fórmula / regra | Fonte | Consumidor |
|------|-----------|-----------------|-------|------------|
| `frequencia_por_dezena` | Frequência absoluta de cada dezena na janela. | `freq[d] = contagem(d em concursos da janela)`. | Histórico | MCP / composição / geração |
| `top10_mais_sorteados` | 10 dezenas mais frequentes da janela. | Ordenar `frequencia_por_dezena` desc; empate por dezena asc; top 10. | Histórico | MCP / composição / geração |
| `top10_menos_sorteados` | 10 dezenas menos frequentes da janela. | Ordenar `frequencia_por_dezena` asc; empate por dezena asc; top 10. | Histórico | MCP / composição |
| `repeticao_concurso_anterior` | Interseção entre concursos consecutivos. | `r_t = |J_t ∩ J_{t-1}|`; comprimento `N` ou `N-1` conforme ADR 0001 D18. | Histórico | MCP / estabilidade / geração |
| `intersecoes_multiplas` | Interseção entre concursos separados por defasagem `l`. | `|J_t ∩ J_{t-l}|`, com `l` declarado no request. | Histórico | MCP / estabilidade |
| `atraso_por_dezena` | Distância desde a última ocorrência de cada dezena. | `atraso[d] = concursos desde último sorteio contendo d`; saturação explícita se nunca ocorreu. | Histórico | MCP / composição |
| `frequencia_blocos` | Blocos consecutivos de presença por dezena. | Lista de comprimentos de sequências consecutivas de presença. | Histórico | MCP / composição |
| `ausencia_blocos` | Blocos consecutivos de ausência por dezena. | Lista de comprimentos de sequências consecutivas de ausência. | Histórico | MCP / composição |
| `estado_atual_dezena` | Estado corrente da dezena ao fim da janela. | Saiu => `0`; não saiu => atraso atual. | Histórico | MCP / composição |
| `pares_impares` | Pares e ímpares de um jogo candidato. | `pares = count(n % 2 == 0)`; `impares = 15 - pares`. | Jogo candidato | MCP / geração |
| `pares_no_concurso` | Quantidade de pares em cada concurso da janela. | Série `p_t = count(n ∈ J_t : n % 2 == 0)`. | Histórico | MCP / padrões / associações |
| `quantidade_vizinhos` | Número de adjacências consecutivas em um jogo candidato. | Contar pares ordenados com diferença `1`. | Jogo candidato | MCP / geração |
| `quantidade_vizinhos_por_concurso` | Número de adjacências consecutivas em cada concurso. | Aplicar `quantidade_vizinhos` a cada `J_t` da janela. | Histórico | MCP / padrões / estabilidade |
| `sequencia_maxima_vizinhos` | Maior sequência consecutiva em um jogo candidato. | Maior comprimento de bloco com diferença `1`. | Jogo candidato | MCP / geração |
| `sequencia_maxima_vizinhos_por_concurso` | Maior sequência consecutiva em cada concurso. | Aplicar `sequencia_maxima_vizinhos` a cada `J_t`. | Histórico | MCP / padrões / estabilidade |
| `distribuicao_linha` | Quantidade de dezenas por linha no volante 5x5. | `c[i] = #dezenas na linha i`; `linha(n) = ceil(n/5)`. | Jogo candidato | MCP / geração |
| `distribuicao_linha_por_concurso` | Série da distribuição por linha. | Para cada `J_t`, vetor `c_t[1..5]`. | Histórico | MCP / padrões / associações |
| `distribuicao_coluna` | Quantidade de dezenas por coluna no volante 5x5. | `c[i] = #dezenas na coluna i`; `coluna(n) = ((n-1) mod 5) + 1`. | Jogo candidato | MCP / geração |
| `distribuicao_coluna_por_concurso` | Série da distribuição por coluna. | Para cada `J_t`, vetor `c_t[1..5]`. | Histórico | MCP / padrões / associações |
| `entropia_linha` | Dispersão do jogo pelas linhas. | `H = -Σ p_i log2(p_i)` com `p_i = distribuicao_linha[i]/15`; também reporta `H_norm = H / log2(5)`. | Jogo candidato | MCP / geração |
| `entropia_linha_por_concurso` | Entropia de linha de cada concurso. | Aplicar `entropia_linha` a cada `J_t`. | Histórico | MCP / padrões / estabilidade |
| `entropia_coluna` | Dispersão do jogo pelas colunas. | Análogo a `entropia_linha` usando colunas. | Jogo candidato | MCP / geração |
| `entropia_coluna_por_concurso` | Entropia de coluna de cada concurso. | Aplicar `entropia_coluna` a cada `J_t`. | Histórico | MCP / padrões / estabilidade |
| `hhi_concentracao` | Concentração em linhas e colunas de um jogo. | `hhi_linha = Σ(distribuicao_linha[i]/15)^2`; `hhi_coluna = Σ(distribuicao_coluna[i]/15)^2`. | Jogo candidato | MCP / geração |
| `hhi_linha_por_concurso` | HHI de linha de cada concurso. | Aplicar `hhi_linha` a cada `J_t`. | Histórico | MCP / padrões / estabilidade |
| `hhi_coluna_por_concurso` | HHI de coluna de cada concurso. | Aplicar `hhi_coluna` a cada `J_t`. | Histórico | MCP / padrões / estabilidade |
| `matriz_numero_slot` | Frequência de cada dezena em cada slot ordenado do concurso. | Matriz `M[d,s]` com `d = 1..25`, `s = 1..15`, após ordenação ascendente das dezenas. | Histórico | MCP / geração / composição |
| `analise_slot` | Aderência do jogo ao perfil histórico de slots. | `score = (1/15) Σ_s p_smooth(g_s, s)`; range `[0,1]`. | Histórico + jogo candidato | MCP / geração |
| `surpresa_slot` | Raridade do jogo no perfil de slots. | `-Σ_s log2 p_smooth(g_s,s)` com smoothing obrigatório. | Histórico + jogo candidato | MCP / geração |
| `intersecao_conjunto_referencia` | Interseção com conjunto externo declarado. | `|jogo ∩ referencia|`. | Jogo + conjunto | MCP / composição / geração |
| `media_janela` | Média temporal de uma série. | `μ = (1/N) Σ x_i`. | Série | MCP / estabilidade |
| `desvio_padrao_janela` | Desvio padrão amostral da série. | `σ = sqrt((1/(N-1)) Σ(x_i-μ)^2)`. | Série | MCP / estabilidade |
| `coeficiente_variacao` | Variação relativa. | `CV = σ / μ` apenas se `μ > ε_cv` e série positiva; caso contrário, fallback documentado. | Série | MCP / estabilidade |
| `madn_janela` | Dispersão robusta normalizada pela mediana. | `MADN = median(|x_i - median(x)|) / median(x)` com fallback robusto quando necessário. | Série | MCP / estabilidade |
| `mad_janela` | Dispersão robusta absoluta. | `MAD = median(|x_i - median(x)|)`. | Série | MCP / estabilidade |
| `tendencia_linear` | Inclinação linear canônica da série. | `x = 0..N-1`; `slope = Σ(x-x̄)(y-ȳ) / Σ(x-x̄)^2`. | Série | MCP / estabilidade |
| `estabilidade_ranking` | Persistência de posição relativa entre sub-janelas. | Pendente de detalhamento. | Séries | MCP / estabilidade |
| `divergencia_kl` | Mudança entre distribuições observadas em janelas distintas. | `D_KL(p||q)` com add-α smoothing. | Distribuições derivadas | MCP / estabilidade |
| `zscore_repeticao` | Distância padronizada da repetição observada. | `Z = (R - μ_ref) / σ_ref`, com `reference` e `baseline_version` explícitos. | Histórico | MCP / estabilidade |
| `persistencia_atraso_extremo` | Quantas dezenas estão acima do atraso extremo de referência. | `Σ_d I(atraso[d] > P95_ref)`. | Histórico | MCP / estabilidade |
| `assimetria_blocos` | Desequilíbrio entre presença e ausência. | Por dezena: `(pres-aus)/(pres+aus)`; agregação padrão: mediana das dezenas. | Histórico | MCP / estabilidade / composição |
| `estatistica_runs` | Resumo dos runs do jogo. | `runs = (sequencia_maxima_vizinhos, quantidade_vizinhos)`. | Jogo candidato | MCP / geração |
| `outlier_score` | Distância do jogo ao centroide da janela. | Distância de Mahalanobis regularizada sobre 5 features canônicas. | Histórico + jogo | MCP / geração |

## Features do `outlier_score`

1. `freq_alignment = (1/15) · Σ_{d ∈ jogo} (frequencia_por_dezena[d] / max(frequencia_por_dezena))`.
2. `slot_alignment = analise_slot(jogo, janela)`.
3. `row_entropy_norm = entropia_linha.H_norm(jogo)`.
4. `pairs_ratio = pares / 15`.
5. `repeticao_anterior = |jogo ∩ último_concurso_da_janela| / 15`.

## Usos práticos recomendados

### Para identificar comportamento padrão

- `repeticao_concurso_anterior`, `quantidade_vizinhos_por_concurso`, `pares_no_concurso`, `entropia_linha_por_concurso` e `entropia_coluna_por_concurso` ajudam a descrever a forma típica dos concursos.
- `distribuicao_linha_por_concurso` e `distribuicao_coluna_por_concurso` ajudam a responder se há concentração central (linhas 2, 3 e 4) ou deslocamentos raros para extremidades.
- `matriz_numero_slot` + `analise_slot` ajudam a medir aderência do jogo ao perfil recente de slots.

### Para cruzar frequência, ausência e persistência

- `frequencia_por_dezena`, `atraso_por_dezena`, `frequencia_blocos`, `ausencia_blocos`, `estado_atual_dezena` e `assimetria_blocos` formam o núcleo para ranking por dezena.
- Use composições declarativas em vez de criar uma "probabilidade futura" artificial.

### Para filtrar jogos raros

- `entropia_linha`, `entropia_coluna`, `hhi_concentracao`, `quantidade_vizinhos`, `sequencia_maxima_vizinhos`, `analise_slot` e `outlier_score` são as métricas mais úteis para exclusão estrutural.

### Para entender deslocamento de regime

- `divergencia_kl`, `madn_janela`, `tendencia_linear`, `zscore_repeticao`, `hhi_linha_por_concurso` e `entropia_linha_por_concurso` ajudam a detectar mudança entre janelas.

## Observações críticas

- `slot` é posição ordenada no resultado, não ordem física de sorteio nem posição do volante.
- Estabilidade descritiva não implica capacidade preditiva.
- Probabilidade empírica histórica só pode aparecer com base de cálculo declarada e interpretação não preditiva.
- Métricas compostas que consomem histórico e jogo candidato mantêm `scope = "candidate_game"`.
- `distribuicao_linha_por_concurso` e `distribuicao_coluna_por_concurso` exigem agregação explícita quando usadas em ranking global ou associação.

## Rastreabilidade com ADRs

| Decisão | Impacto neste catálogo |
|---|---|
| ADR 0001 D4 | `coeficiente_variacao`, `madn_janela` |
| ADR 0001 D5 | `divergencia_kl`, `surpresa_slot` |
| ADR 0001 D6 | `entropia_linha`, `entropia_coluna` |
| ADR 0001 D7 | `tendencia_linear` |
| ADR 0001 D8 | `zscore_repeticao`, `persistencia_atraso_extremo` |
| ADR 0001 D9 | `outlier_score` |
| ADR 0001 D18 | `repeticao_concurso_anterior` |
| ADR 0002 D1 | `pares_no_concurso`, `quantidade_vizinhos_por_concurso`, `sequencia_maxima_vizinhos_por_concurso` |
| ADR 0002 D2 | `distribuicao_linha_por_concurso`, `distribuicao_coluna_por_concurso` |
| ADR 0002 D3 | `entropia_linha_por_concurso`, `entropia_coluna_por_concurso`, `hhi_linha_por_concurso`, `hhi_coluna_por_concurso` |
