# Catálogo de métricas

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Fonte de verdade semântica das métricas da V1 ampliada. Alinha-se a [mcp-tool-contract.md](mcp-tool-contract.md), [generation-strategies.md](generation-strategies.md) e aos ADRs [0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md), [0002](adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md) e [0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) (disponibilidade por rota, pipeline, correlação pares–entropia canónica em testes).

Para **interpretação em linguagem simples**, **o que cada métrica observa** e **exemplos de uso** por nome, ver [metric-glossary.md](metric-glossary.md) (complemento pedagógico; fórmulas e versões permanecem aqui).

## Linguagem ubíqua (vocabulário da Tabela 1)

Este catálogo é a **fonte de verdade** dos termos do subdomínio **Medição e indicadores** (alinhado a ideias de *ubiquitous language* em DDD): nomes estáveis, sem sinônimos soltos no código e no contrato MCP. A Tabela 1 combina **dois eixos independentes** — *o que a métrica representa no domínio* (**Categoria**) e *como a especificação está amarrada no catálogo* (**Status**). Os valores de enum **não** se repetem entre colunas: **Categoria** `por_transformacao` ≠ **Status** `satelite`.

### Categoria — papel da métrica no modelo

Classifica **para que serve** no fluxo analítico (leitura humana, documentação e navegação); **não** altera `Nome`, fórmula, `Shape` nem contrato MCP.

| Valor | Definição |
|-------|-----------|
| **base** | **Medição primitiva** sobre o histórico (ou estrutura canônica única): entrada natural para composições — ex.: `frequencia_por_dezena`, `repeticao_concurso_anterior`, `matriz_numero_slot`. |
| **por_transformacao** | **Indicador obtido por transformação determinística** de insumos já definidos neste catálogo (outra(s) métrica(s), ordenação ou seleção sobre vetores) — ex.: `top10_mais_sorteados` a partir de `frequencia_por_dezena`. |
| **apoio** | **Vetor, série auxiliar ou agregado estruturado** que descreve o histórico para padrões, correlações ou composições; não é regra de estabilidade nem foco exclusivo no candidato — ex.: `pares_no_concurso`, `entropia_linha_por_concurso`, `estado_atual_dezena`. |
| **geração** | Métrica cujo **objeto central** é o **jogo candidato** (ou o cruzamento explícito **candidato × janela** para pontuação/filtro) — ex.: `pares_impares`, `analise_slot`, `outlier_score`. |
| **estabilidade** | **Resumo estatístico ou comparativo** sobre séries ou distribuições em janela(s): dispersão, tendência, divergência, extremos — ex.: `madn_janela`, `divergencia_kl`. |

**Regra normativa — categorias no catálogo:** o catálogo só lista valores de **Categoria** quando existe ao menos **uma** métrica canônica real atribuída a esse valor, com **fórmula fechada**, **unidade/escala definida**, **documentação** e **teste**. Categorias sem métrica canônica não constam no vocabulário. **Reintrodução** de novo valor de categoria exige decisão formal (ADR ou mecanismo equivalente).

**Justificativa:** remove superfície especulativa, evita inferência por consumidores e elimina ambiguidade semântica no catálogo (sem prejuízo da tool MCP `compose_indicator_analysis`, que permanece contratada em [mcp-tool-contract.md](mcp-tool-contract.md)).

### Status — maturidade da especificação

| Valor | Definição |
|-------|-----------|
| **canonica** | Especificação **fechada** para a `Versão` indicada: Tabelas 1–2 + referências a ADRs são o critério de conformidade. |
| **satelite** | Especificação **válida e versionada**, porém com **prioridade ou estabilidade editorial** de segunda linha: pode depender de parâmetro extra, agregação sobre listas por dezena ou políticas com maior chance de *minor* documental — implementar conforme Tabela 2 e notas. Uma métrica pode ser **Categoria** `apoio` e **Status** `satelite` (ex.: `frequencia_blocos`). |
| **pendente de detalhamento** | **Sem fechamento** de fórmula ou contrato suficiente para conformidade total; uso apenas com opt-in explícito no contrato MCP (`allow_pending: true`) quando aplicável. |

### Janela — texto da coluna

| Valor / padrão | Definição |
|----------------|-----------|
| **configurável** | O recorte temporal (tamanho, limites de concurso) vem **do request**; não existe default invisível. |
| **não se aplica** | O cálculo **não** parametrizado por janela (ex.: propriedades só do jogo candidato). |
| **histórico acumulado ou janela declarada** | Política de atraso/contagem depende de regra explícita no request (acumulado global vs janela). |
| **comparação entre 2 janelas** | Requer **duas** janelas definidas no pedido — ex.: `divergencia_kl`. |
| *(texto livre)* | Qualquer outra frase na coluna é **normativa** para aquela métrica (regra especial). |

### Scope — escopo do valor (alinhado ao contrato MCP)

| Valor | Definição |
|-------|-----------|
| **window** | Um **único agregado** referente à janela (ou ao último estado nela contido, conforme métrica). |
| **series** | **Série temporal** indexada por concurso (ou por passo dentro da janela), com regra de comprimento explícita na Tabela 2. |
| **candidate_game** | Valor obtido sobre **um jogo candidato** (15 dezenas), possivelmente usando o histórico como referência declarada. |

### Unidade

| Valor | Definição |
|-------|-----------|
| **count** | Contagem inteira não negativa (ou política de saturação explícita na Tabela 2). |
| **ratio** | Razão em \([0,1]\) ou escala declarada na métrica. |
| **bits** | Informação em **base 2** (Shannon); pode acompanhar forma normalizada quando a métrica definir. |
| **dimensionless** | Grandeza adimensional (índices, scores, Z-scores conforme definição). |
| **herda do indicador** | A unidade é a **do indicador de entrada** da série ou composição (declarado no request). |

### Shape — forma do `value` (tipagem lógica)

| Valor | Definição |
|-------|-----------|
| **scalar** | Um número (ou par identificado na métrica). |
| **series** | Sequência ao longo da janela (uma ordem canônica na Tabela 2). |
| **vector_by_dezena** | Vetor indexado \(1..25\) por dezena. |
| **count_vector[5]** | Vetor de 5 contagens (ex.: linhas ou colunas do volante). |
| **series_of_count_vector[5]** | Série em que cada ponto é um `count_vector[5]`. |
| **count_matrix[25x15]** | Matriz dezena × slot posicional (dezenas ordenadas no concurso). |
| **count_pair** | Par ordenado de contagens com semântica fixa na Tabela 2 (ex.: pares e ímpares; runs). |
| **dezena_list[10]** | Lista ordenada de dezenas com regra de empate na Tabela 2. |
| **count_list_by_dezena** | Por dezena, lista de inteiros (ex.: comprimentos de blocos). |
| **dimensionless_pair** | Par de valores adimensionais com papéis fixos (ex.: HHI linha e coluna). |

### Versão

SemVer **por métrica**: *major* se mudar fórmula ou semântica incompatível; *minor* se estender sem quebrar consumidores; *patch* se apenas clarificar documentação sem alterar resultado.

---

## Convenções

Os campos da Tabela 1 e seus valores controlados estão definidos na seção **Linguagem ubíqua (vocabulário da Tabela 1)** acima. Abaixo, **Objetivo principal** resume o papel de cada coluna no sistema (contrato MCP, validação e testes determinísticos).

| Campo | Descrição | Objetivo principal |
|-------|-----------|-------------------|
| **Nome** | Identificador estável (`snake_case`). | Referência única em payloads, composições e prompts; evita drift por sinônimos. |
| **Categoria** | `base`, `por_transformacao`, `apoio`, `geração` ou `estabilidade`. | Organizar o domínio e orientar leitura; não altera semântica nem fórmula. |
| **Janela** | `configurável`, `não se aplica` ou regra específica. | Deixar explícito se o cálculo depende de recorte temporal e qual tipo de janela. |
| **Scope** | `window`, `series` ou `candidate_game`. | Separar agregado de janela, série temporal e avaliação sobre jogo candidato — evita misturar semânticas numa mesma métrica. |
| **Unidade** | `count`, `ratio`, `bits`, `dimensionless` ou `herda do indicador`. | Impedir comparações e composições inválidas; documentar escala para explicação e agregação. |
| **Shape** | `scalar`, `series`, `vector_by_dezena`, `count_vector[5]`, `series_of_count_vector[5]`, `count_matrix[25x15]`, `count_pair`, `dezena_list[10]`, `count_list_by_dezena` ou `dimensionless_pair`. | Tipar o `value`, exigir `aggregation` quando vetorial/matricial e alinhar ao contrato — sem descoberta tardia em runtime. |
| **Versão** | SemVer por métrica. | Rastrear mudanças de fórmula ou semântica; permite fixtures e consumidores versionados. |
| **Status** | `canonica`, `satelite` ou `pendente de detalhamento`. | Sinalizar o que é normativo na V1 versus especificação satélite ou ainda não fechada. |

## Regras de uso

| Regra | Objetivo principal |
|-------|-------------------|
| Janela nunca é implícita. | Reprodutibilidade e explicação auditável: todo resultado deve citar o mesmo recorte que o chamador pretendia. |
| Toda composição dinâmica deve declarar agregação, transformação e referência quando a métrica não for escalar. | Evitar regras implícitas do modelo ou do código; o request documenta a semântica para IA e para o hash determinístico. |
| Estabilidade descritiva usa `madn_janela` como default; `coeficiente_variacao` é opt-in e restrito a séries positivas. | Default robusto (evita explosão numérica perto de zero); CV só onde a razão σ/μ é válida (ver ADR 0001 D4). |
| Toda distribuição em `log` usa add-α smoothing; `α` é o inverso da cardinalidade do suporte indicada na métrica. | Valores finitos comparáveis entre implementações; evita +∞ e NaN em janelas pequenas (ADR 0001 D5). |
| Entropias são reportadas em bits e, quando aplicável, com `H_norm`. | Comparabilidade entre métricas e filtros; `H_norm ∈ [0,1]` facilita scores e limites declarados (ADR 0001 D6). |
| "Persistência" significa regularidade observada na janela ou no histórico declarado; não implica previsão. | Alinha linguagem do projeto ao escopo não preditivo do [brief](brief.md). |
| Séries temporais obtidas a partir dos concursos continuam com `scope = "series"`; o catálogo não reintroduz `scope = "draw"`. | Consistência com o contrato MCP até existir métrica canônica com escopo `draw` (ADR 0001 D13). |

## Disponibilidade normativa (catálogo × `compute_window_metrics`)

- **O catálogo (Tabelas 1 e 2)** indica o que a métrica *é*; a tool `compute_window_metrics` indica o que uma **build** *expõe* em JSON, para uma janela explícita. A matriz e o padrão de *promoção* estão em [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md).  
- **Recorte mínimo (V0 documental):** a [vertical-slice.md](vertical-slice.md) exige, para a primeira fatia, **sucesso** apenas de `frequencia_por_dezena@1.0.0` por `compute_window_metrics`. Nomes canónicos adicionais nessa tabela, quando pedidos antes de serem ligados a esta rota na build, justificam resposta de erro de contrato documentada (`UNKNOWN_METRIC` com `details`), não ambiguidade.  
- **V1 alvo (contrato expandido):** `compute_window_metrics` aplica a todas as métricas cujo `Janela` e `Scope` forem coerentes com a Tabela 1, com paridade ao [mcp-tool-contract.md](mcp-tool-contract.md).  
- **Coesão:** métricas consumidas em [generation-strategies.md](generation-strategies.md) ou explicadas em `explain_candidate_games` devem confluir, ao longo dos incrementos, com a mesma disponibilidade canónica; até lá, o plano de testes reforça testes de GAPS (ver [test-plan.md](test-plan.md) e [contract-test-plan.md](contract-test-plan.md)).

| Situação | Onde a ler |
|----------|------------|
| Nome fora de Tabela 1 | Sempre `UNKNOWN_METRIC` (não está no catálogo). |
| Nome canónico, rota ainda fechada na build | `UNKNOWN_METRIC` com `details.metric_name` (e, se existir, lista do subconjunto aceite). |
| Janela curta e `min_history` em análise de estabilidade | Regra em `analyze_indicator_stability` (ver [ADR 0006 D4](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)). |

## Tabela 1 — Identificação e tipagem

| Nome | Categoria | Janela | Scope | Unidade | Shape | Versão | Status |
|------|-----------|--------|-------|---------|-------|--------|--------|
| `frequencia_por_dezena` | base | configurável | `window` | `count` | `vector_by_dezena` | 1.0.0 | canonica |
| `top10_mais_sorteados` | por_transformacao | configurável | `window` | `dimensionless` | `dezena_list[10]` | 1.0.0 | canonica |
| `top10_menos_sorteados` | por_transformacao | configurável | `window` | `dimensionless` | `dezena_list[10]` | 1.0.0 | canonica |
| `repeticao_concurso_anterior` | base | configurável | `series` | `count` | `series` | 1.0.0 | canonica |
| `intersecoes_multiplas` | apoio | configurável | `series` | `count` | `series` | 1.0.0 | satelite |
| `atraso_por_dezena` | base | histórico acumulado ou janela declarada | `window` | `count` | `vector_by_dezena` | 1.0.0 | canonica |
| `frequencia_blocos` | apoio | configurável | `window` | `count` | `count_list_by_dezena` | 1.0.0 | satelite |
| `ausencia_blocos` | apoio | configurável | `window` | `count` | `count_list_by_dezena` | 1.0.0 | satelite |
| `estado_atual_dezena` | apoio | configurável | `window` | `count` | `vector_by_dezena` | 1.0.0 | satelite |
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
| `estabilidade_ranking` | estabilidade | configurável | `window` | `dimensionless` | `scalar` | 1.0.0 | canonica |
| `divergencia_kl` | estabilidade | comparação entre 2 janelas | `window` | `bits` | `scalar` | 1.0.0 | canonica |
| `zscore_repeticao` | estabilidade | configurável | `window` | `dimensionless` | `scalar` | 2.0.0 | canonica |
| `persistencia_atraso_extremo` | estabilidade | configurável | `window` | `count` | `scalar` | 2.0.0 | canonica |
| `assimetria_blocos` | estabilidade | configurável | `window` | `dimensionless` | `scalar` | 1.0.0 | satelite |
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
| `coeficiente_variacao` | Variação relativa. | `CV = σ / μ` apenas se a série for estritamente positiva e `μ > ε_cv`; caso contrário, erro `UNSUPPORTED_NORMALIZATION_METHOD`. | Série | MCP / estabilidade |
| `madn_janela` | Dispersão robusta normalizada pela mediana. | `MADN = median(|x_i - median(x)|) / median(x)` com fallback robusto quando necessário. | Série | MCP / estabilidade |
| `mad_janela` | Dispersão robusta absoluta. | `MAD = median(|x_i - median(x)|)`. | Série | MCP / estabilidade |
| `tendencia_linear` | Inclinação linear canônica da série. | `x = 0..N-1`; `slope = Σ(x-x̄)(y-ȳ) / Σ(x-x̄)^2`. | Série | MCP / estabilidade |
| `estabilidade_ranking` | Persistência de **posição relativa** entre rankings de frequência em sub-janelas consecutivas (não mede estabilidade de Top-K nem churn em buckets). | Fórmula e particionamento normativos em **Nota normativa — `estabilidade_ranking`** (abaixo). **Pré-condição:** `window_size ≥ 2`; caso contrário → `INSUFFICIENT_HISTORY` (contrato MCP). **Justificativa:** correlação de Spearman entre rankings consecutivos mede diretamente persistência de posição relativa, alinhada ao nome `estabilidade_ranking`. | Histórico | MCP / estabilidade |
| `divergencia_kl` | Mudança entre distribuições observadas em janelas distintas. | `D_KL(p||q)` com add-α smoothing. | Duas distribuições empíricas (janelas comparadas) | MCP / estabilidade |
| `zscore_repeticao` | Distância padronizada da repetição observada. | `Z = (R - μ_ref) / σ_ref`, com `reference` e `baseline_version` explícitos. | Histórico | MCP / estabilidade |
| `persistencia_atraso_extremo` | Quantas dezenas estão acima do atraso extremo de referência. | `Σ_d I(atraso[d] > P95_ref(reference, baseline_version))`, com `reference` e `baseline_version` explícitos. | Histórico | MCP / estabilidade |
| `assimetria_blocos` | Desequilíbrio entre presença e ausência. | Por dezena: `(pres-aus)/(pres+aus)`; agregação padrão: mediana das dezenas. | Histórico | MCP / estabilidade / composição |
| `estatistica_runs` | Resumo dos runs do jogo. | `runs = (sequencia_maxima_vizinhos, quantidade_vizinhos)`. | Jogo candidato | MCP / geração |
| `outlier_score` | Distância do jogo ao centroide da janela. | Distância de Mahalanobis regularizada sobre 5 features canônicas. | Histórico + jogo | MCP / geração |

### Nota normativa — `estabilidade_ranking`

**Definição:** mede estabilidade da **posição relativa** entre as 25 dezenas quando a janela é particionada em `k` sub-janelas contíguas no tempo; em cada sub-janela calcula-se o ranking por frequência; entre pares consecutivos de sub-janelas calcula-se a correlação de Spearman dos rankings; o score final é a média das correlações normalizadas.

**Parâmetros operacionais**

- `k_default = 4`, `k_min = 2`.
- Seja `N = window_size` (número de concursos na janela). **Requer** `N ≥ 2`. Se `N < 2`, a implementação deve falhar com `INSUFFICIENT_HISTORY`.
- **Número de blocos:** `k = min(k_default, N)`. Se `N < k_default`, `k` reduz-se automaticamente (ex.: `N = 3` ⇒ `k = 3`); o mínimo aplicável com `N ≥ 2` é `k = 2` quando `N = 2`.
- **Particionamento do tempo:** dividir os `N` concursos em blocos consecutivos `B1..Bk` com tamanhos `n_1..n_k` tais que `Σ n_i = N`, cada `n_i ≥ 1`, e os tamanhos sejam o mais uniformes possível: `n_i ∈ {⌊N/k⌋, ⌈N/k⌉}` e `max(n_i) - min(n_i) ≤ 1`.

**Fórmula**

1. Para cada bloco `Bi`, para cada dezena `d ∈ {1..25}`, `freq_i[d]` = contagem de ocorrências de `d` nos concursos de `Bi`.
2. Para cada `Bi`, construir `rank_i[d]` ordenando `freq_i` por valor **descendente**; empates usam **average rank** sobre as posições 1..25.
3. Para cada par consecutivo `(Bi, B{i+1})`, sejam `r1[d] = rank_i[d]` e `r2[d] = rank_{i+1}[d]`. Calcular `rho_i` como a **correlação de Pearson** entre os vetores `r1[1..25]` e `r2[1..25]`.
4. **Borda — variância nula nos ranks:** seja `var` a variância amostral com divisor `24` (desvio padrão amostral em 25 pontos). Se `var(r1)=0` e `var(r2)=0`, então `rho_i = 1` se `r1[d]=r2[d]` para todo `d`, senão `rho_i = 0`. Se exatamente uma de `var(r1)`, `var(r2)` for zero, então `rho_i = 0`.
5. `s_i = (rho_i + 1) / 2`.
6. `estabilidade_ranking = (1/(k-1)) * Σ_{i=1..k-1} s_i` ∈ `[0,1]`.

**Impacto de implementação:** particionamento determinístico da janela, ranking com average rank, Spearman implementado via Pearson nos ranks, regra de borda para `rho_i` quando variância amostral é zero, e testes golden para valores esperados.

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
