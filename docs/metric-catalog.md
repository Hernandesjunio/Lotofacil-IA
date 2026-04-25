# Catálogo de métricas

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Fonte de verdade semântica das métricas da V1 ampliada. Alinha-se a [mcp-tool-contract.md](mcp-tool-contract.md), [generation-strategies.md](generation-strategies.md) e aos ADRs [0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md), [0002](adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md), [0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) (disponibilidade por rota, pipeline, correlação pares–entropia canónica em testes) e [0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) (janela por extremos, descoberta, mapeamento legado Top 10).

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
| Recortes “últimos N” de UI legada (N fixo) | No MCP, o **chamador** declara o recorte via `window_size` + `end_contest_id` (ou par `start`/`end` equivalente, [ADR 0008 D2](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)). O servidor **não** reproduz *defaults* de painéis antigos — [ADR 0008 D4](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) e proibição de *defaults* no [mcp-tool-contract.md](mcp-tool-contract.md). |
| Toda composição dinâmica deve declarar agregação, transformação e referência quando a métrica não for escalar. | Evitar regras implícitas do modelo ou do código; o request documenta a semântica para IA e para o hash determinístico. |
| Estabilidade descritiva usa `madn_janela` como default; `coeficiente_variacao` é opt-in e restrito a séries positivas. | Default robusto (evita explosão numérica perto de zero); CV só onde a razão σ/μ é válida (ver ADR 0001 D4). |
| Toda distribuição em `log` usa add-α smoothing; `α` é o inverso da cardinalidade do suporte indicada na métrica. | Valores finitos comparáveis entre implementações; evita +∞ e NaN em janelas pequenas (ADR 0001 D5). |
| Entropias são reportadas em bits e, quando aplicável, com `H_norm`. | Comparabilidade entre métricas e filtros; `H_norm ∈ [0,1]` facilita scores e limites declarados (ADR 0001 D6). |
| "Persistência" significa regularidade observada na janela ou no histórico declarado; não implica previsão. | Alinha linguagem do projeto ao escopo não preditivo do [brief](brief.md). |
| Séries temporais obtidas a partir dos concursos continuam com `scope = "series"`; o catálogo não reintroduz `scope = "draw"`. | Consistência com o contrato MCP até existir métrica canônica com escopo `draw` (ADR 0001 D13). |

## Janela por extremos (equivalência normativa)

Quando o consumidor expressa o recorte como **concurso inicial e final** (inclusivos e contíguos no dataset canónico), a resolução do comprimento e do ancoramento segue [ADR 0008 D2](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md):

- `end_contest_id` = extremo **mais recente** do recorte;
- `window_size` = `end_contest_id - start_contest_id + 1`.

O *request* do contrato pode expor apenas `window_size` + `end_contest_id` ou estender o JSON com `start_contest_id` / `end_contest_id` desde que a janela resultante seja a mesma e o servidor rejeite combinações ambíguas com o código apropriado (ver [mcp-tool-contract.md](mcp-tool-contract.md), entidade `Window` e validações).

## Rótulo de export legado `HistoricoTop10MaisSorteados`

*Exports* de sistemas de UI anteriores podem usar rótulos que sugerem “histórico” e regras temporais múltiplas. **No domínio canónico deste repositório,** a substituição do conceito “dez dezenas mais sorteadas **no intervalo de concursos que o utilizador declara**” é a métrica `top10_mais_sorteados@1.0.0` (Tabelas 1 e 2), calculada **só** sobre a janela resolvida acima. Séries com lookback móvel, top-K *rolling* ou outras regras **não** reutilizam este rótulo; exigem nova entrada no catálogo com `Nome`, `params` e `Versão` próprios. Ver [ADR 0008 D3](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md).

## Rótulo de export legado `QtdFrequencia` (vector 1..25 em gráfico)

<a id="export-legado-qtdfrequencia"></a>

O *export* `QtdFrequencia` (p.ex. em `indicadores.json` referência do repositório) contém, por posição 1..25, **contagens de ocorrência** da dezena na janela de análise (valores inteiros não negativos pequenos, soma plausível = `15 × window_size` quando a métrica é frequência de presença por sorteio). **A substituição canónica no MCP é** `frequencia_por_dezena@1.0.0` (`scope=window`, `shape=vector_by_dezena`) com a janela explícita ([ADR 0008 D2](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)).

**Nota editorial (usabilidade sistêmica; migração com sunset):** para reduzir ambiguidade em leitura humana, o catálogo introduz o nome autoexplicativo `total_de_presencas_na_janela_por_dezena@1.0.0`, semanticamente **equivalente** a `frequencia_por_dezena@1.0.0` (mesma contagem total por janela). Durante a janela de migração, ambas podem coexistir; após o **sunset** (ver “Política de deprecação e sunset” abaixo), builds futuras podem deixar de aceitar o nome antigo e retornar `UNKNOWN_METRIC` com `details` apontando o nome de substituição.

- **Não** confundir com a métrica `atraso_por_dezena` (também `vector_by_dezena`, mas a unidade semântica é *concursos desde a última ocorrência*, não contagens de saída na janela). Para **atraso**, o pedido deve usar `atraso_por_dezena@1.0.0` com a política de *Janela* e `params` descritas na Tabela 2, não `frequencia_por_dezena`.
- **Linguagem acessível (ADR 0021):** quando, em conversa, se diz que uma dezena «está *ausente*» no sentido *há N concursos em que a dezena ainda não voltou a sair* (vector com padrão tipicamente 0, 1, 2, …), isso corresponde à métrica **`atraso_por_dezena`** (e, no fim de janela, a `estado_atual_dezena` para a leitura *agora*), **não** à soma de ocorrências de `frequencia_por_dezena`. Isto distingue-se também de `ausencia_blocos` (comprimentos de blocos consecutivos, `count_list_by_dezena` na Tabela 1), que **não** partilha o *shape* de 25 inteiros do vector de atraso. Síntese: [secção *Vocabulário* (âncora estável `vocab-ausencia-adr-0021`)](metric-glossary.md#vocab-ausencia-adr-0021), [nota *Ausência* (Apêndice, ADR 0021)](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md#nota-ausencia-adr-0021) e tabela A do mesmo [Apêndice](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md#apendice-frases-modelo-pt-adr-0021).

**Ponte — quatro *papéis* (nomes de métrica inalterados):** o quadro resume *intenção de leitor* e encaminhamento; **fórmulas e semântica fechadas** continuam nas Tabelas 1 e 2 abaixo, sem duplicação.

| Papel (o que se quer expressar) | Mapeamento / ancoragem canónica | Lembrete curto |
|---------------------------------|----------------------------------|----------------|
| **(1) Frequência na janela** (quantas vezes saiu) | *Export* `QtdFrequencia` e métrica `frequencia_por_dezena` (esta secção) | Contagens; soma plausível `15 ×` tamanho da janela (presença por sorteio). Não descreve «há *N* concursos *sem* sair». |
| **(2) Atraso e *estado*** (leitor: *ausente* há *N* edições) | `atraso_por_dezena`; `estado_atual_dezena` no fim do recorte | *Vector* 0, 1, 2, …; não confundir com a soma de `frequencia_por_dezena` nem com `QtdFrequencia`. |
| **(3) Blocos consecutivos *sem* a dezena** | `ausencia_blocos` | *Shape* `count_list_by_dezena` (Tabela 1); não é o mesmo *layout* de vector 1..25 de atraso. |
| **(4) Distinção em tabela (evitar trocar rótulos)** | Tabela no [glossário, âncora *Vocabulário*](metric-glossary.md#vocab-ausencia-adr-0021); tabela A + [nota *Ausência*](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md#nota-ausencia-adr-0021) (ADR 0021) | Papel *editorial* / pedagógico: mesmo vocabulário leigo alinhado aos quatro *papéis* sem renomear métricas. |

## Subcampos de `PadroesLinha` (legado) sem linha na Tabela 1

O bloco `PadroesLinha` de *exports* antigos pode incluir:

- **`Dados`**, **`Ultimos200PadroesSorteados`**: alimentados por `distribuicao_linha_por_concurso` e, quando aplicável, agregados canónicos ([ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md));
- **`PercentualExistente`**: regra e unidade **não** fechadas nesta versão do catálogo; **não** há métrica canónica com esse nome. Qualquer reimplementação exige proposta (nome, fórmula, `version`, testes) ou fica **fora do escopo** do MCP até decisão;
- **`ListaComparacaoHistorica`**: vector booleano *ad hoc* **sem** definição normativa no catálogo; **não** é mapeamento 1:1 a uma métrica canónica. Até existir definição fechada, tratar como dado de UI/legado, não como `MetricValue`.

## Disponibilidade normativa (catálogo × `compute_window_metrics`)

- **O catálogo (Tabelas 1 e 2)** indica o que a métrica *é*; a tool `compute_window_metrics` indica o que uma **build** *expõe* em JSON, para uma janela explícita. A matriz e o padrão de *promoção* estão em [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md).  
- **Agregados canônicos de janela:** estruturas derivadas como histogramas, top-k de padrões e matrizes auxiliares **não** são “métricas novas” do catálogo; elas são produzidas por `summarize_window_aggregates` (ver [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) e [mcp-tool-contract.md](mcp-tool-contract.md)). A mesma lógica de disponibilidade por build/rota se aplica: se a métrica fonte necessária não estiver disponível na build, a tool de agregados deve falhar de forma rastreável (ver ADR 0006 D1).
- **Descoberta e rótulos de export (UI legado):** ver [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) (janela por extremos, `HistoricoTop10MaisSorteados` → `top10_mais_sorteados`). *Instância* (subconjunto aceite por build, p.ex. `details.allowed_metrics` alinhado a [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)) *vs.* *norma* (este ficheiro, glossário e a secção *Primitivas MCP opcionais* de [mcp-tool-contract.md](mcp-tool-contract.md)) — ADR 0008 D1; o contrato **não** exige identificador fechado de tool de listagem, só a semântica.
- **Recorte mínimo (V0 documental):** a [vertical-slice.md](vertical-slice.md) exige, para a primeira fatia, **sucesso** apenas de `frequencia_por_dezena@1.0.0` por `compute_window_metrics`. Nomes canónicos adicionais nessa tabela, quando pedidos antes de serem ligados a esta rota na build, justificam resposta de erro de contrato documentada (`UNKNOWN_METRIC` com `details`), não ambiguidade.  
- **V1 alvo (contrato expandido):** `compute_window_metrics` aplica a todas as métricas cujo `Janela` e `Scope` forem coerentes com a Tabela 1, com paridade ao [mcp-tool-contract.md](mcp-tool-contract.md).  
- **Coesão:** métricas consumidas em [generation-strategies.md](generation-strategies.md) ou explicadas em `explain_candidate_games` devem confluir, ao longo dos incrementos, com a mesma disponibilidade canónica; até lá, o plano de testes reforça testes de GAPS (ver [test-plan.md](test-plan.md) e [contract-test-plan.md](contract-test-plan.md)).

| Situação | Onde a ler |
|----------|------------|
| Nome fora de Tabela 1 | Sempre `UNKNOWN_METRIC` (não está no catálogo). |
| Nome canónico, rota ainda fechada na build | `UNKNOWN_METRIC` com `details.metric_name` (e, se existir, lista do subconjunto aceite). |
| Janela curta e `min_history` em análise de estabilidade | Regra em `analyze_indicator_stability` (ver [ADR 0006 D4](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)). |

### Política de deprecação e sunset (nomes de métricas)

Esta secção regula **nomes** (ergonomia e migração), não altera o contrato `MetricValue` nem reabre fórmulas fora das Tabelas 1–2.

- **Motivo:** nomes autoexplicativos reduzem ambiguidade (“contagem total na janela” vs “sequência atual com reinício”).
- **Coexistência (janela de migração):** um nome novo pode ser introduzido como sinônimo semântico de um nome antigo para permitir comparação/rollback.
- **Sunset (prazo máximo):** métricas marcadas como *deprecated* permanecem disponíveis até **2026-05-25** (inclusive). Após esta data, builds podem passar a rejeitar o nome deprecated com `UNKNOWN_METRIC`, incluindo em `details`:
  - `deprecated_metric_name`
  - `replacement_metric_name`
  - `sunset_date` (ISO-8601)

**Regra:** enquanto um nome deprecated ainda for aceito, sua semântica deve permanecer **idêntica** à versão documentada; mudanças semânticas exigem nova versão (SemVer) e testes correspondentes.

<a id="tabela-1-identificacao-e-tipagem"></a>

## Tabela 1 — Identificação e tipagem

| Nome | Categoria | Janela | Scope | Unidade | Shape | Versão | Status |
|------|-----------|--------|-------|---------|-------|--------|--------|
| `frequencia_por_dezena` | base | configurável | `window` | `count` | `vector_by_dezena` | 1.0.0 | canonica |
| `total_de_presencas_na_janela_por_dezena` | base | configurável | `window` | `count` | `vector_by_dezena` | 1.0.0 | canonica |
| `sequencia_atual_de_presencas_por_dezena` | apoio | configurável | `window` | `count` | `vector_by_dezena` | 1.0.0 | canonica |
| `top10_mais_sorteados` | por_transformacao | configurável | `window` | `dimensionless` | `dezena_list[10]` | 1.0.0 | canonica |
| `top10_menos_sorteados` | por_transformacao | configurável | `window` | `dimensionless` | `dezena_list[10]` | 1.0.0 | canonica |
| `top10_maiores_totais_de_presencas_na_janela` | por_transformacao | configurável | `window` | `dimensionless` | `dezena_list[10]` | 1.0.0 | canonica |
| `top10_menores_totais_de_presencas_na_janela` | por_transformacao | configurável | `window` | `dimensionless` | `dezena_list[10]` | 1.0.0 | canonica |
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

<a id="tabela-2-semantica"></a>

## Tabela 2 — Semântica

| Nome | Definição | Fórmula / regra | Fonte | Consumidor |
|------|-----------|-----------------|-------|------------|
| `frequencia_por_dezena` | Frequência absoluta de cada dezena na janela. | `freq[d] = contagem(d em concursos da janela)`. | Histórico | MCP / composição / geração |
| `total_de_presencas_na_janela_por_dezena` | Total de presenças de cada dezena na janela (nome autoexplicativo; equivalente semântico de `frequencia_por_dezena`). | `total[d] = contagem(d em concursos da janela)`; soma dos 25 = `15 × window_size`. | Histórico | MCP / composição / geração |
| `sequencia_atual_de_presencas_por_dezena` | Sequência atual (streak) de presenças por dezena ao fim da janela. | Percorrer concursos em ordem; para cada dezena `d`: se `d ∈ J_t` então `c[d] = c[d] + 1`, senão `c[d] = 0`. Retornar `c[d]` no fim do recorte. | Histórico | MCP / composição |
| `top10_mais_sorteados` | 10 dezenas mais frequentes da janela. | Ordenar `frequencia_por_dezena` desc; empate por dezena asc; top 10. | Histórico | MCP / composição / geração |
| `top10_menos_sorteados` | 10 dezenas menos frequentes da janela. | Ordenar `frequencia_por_dezena` asc; empate por dezena asc; top 10. | Histórico | MCP / composição |
| `top10_maiores_totais_de_presencas_na_janela` | 10 dezenas com maior total de presenças na janela. | Ordenar `total_de_presencas_na_janela_por_dezena` desc; empate por dezena asc; top 10. | Histórico | MCP / composição / geração |
| `top10_menores_totais_de_presencas_na_janela` | 10 dezenas com menor total de presenças na janela. | Ordenar `total_de_presencas_na_janela_por_dezena` asc; empate por dezena asc; top 10. | Histórico | MCP / composição |
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

1. `freq_alignment = (1/15) · Σ_{d ∈ jogo} (total_de_presencas_na_janela_por_dezena[d] / max(total_de_presencas_na_janela_por_dezena))` (equivalente semântico de `frequencia_por_dezena`, mas com nome autoexplicativo).
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
- **Nota (ADR 0019):** algumas restrições usadas na geração são **features derivadas escalares** (ex.: `top10_overlap_count(game)`), calculadas a partir de métricas canônicas (ex.: `top10_mais_sorteados`). Essas features não precisam ser expostas como métricas independentes em `compute_window_metrics`; devem ser documentadas no contrato de geração/estratégias e ecoadas em `explain_candidate_games` quando participarem de critérios/filtros.

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
