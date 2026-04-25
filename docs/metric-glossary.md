# Glossário de métricas (definição, interpretação e exemplos)

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md) · [Vocabulário: «ausência» (ADR)](#vocab-ausencia-adr-0021) · [Textos de resumo para tabelas (ADR 0021)](#textos-de-resumo-para-tabelas-adr-0021)

**Sumário:** [Vocabulário: «ausência» × ADR 0021](#vocab-ausencia-adr-0021) · [Textos de resumo para tabelas (ADR 0021)](#textos-de-resumo-para-tabelas-adr-0021) (norma de apresentação humana, tabelas A e B, D2, D5, alinhado a [adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md)).

Documento pedagógico complementar ao catálogo técnico em [metric-catalog.md](metric-catalog.md). Aqui cada métrica tem **definição**, **o que observa** (interpretação em linguagem simples) e **exemplo de uso**. Fórmulas detalhadas, tipagem, versões e **léxico das colunas da Tabela 1** (Categoria, Status, Shape etc.) permanecem no catálogo.

**Nota sobre predição:** todas descrevem padrões no histórico ou estrutura de um jogo; nenhuma implica acerto futuro.

**Interação entre métricas (ex. pares e entropia de linha no mesmo recorte):** o co-movimento estatístico (Spearman/Pearson) entre séries alinhadas por concurso — p.ex. `pares_no_concurso` e `entropia_linha_por_concurso` — descreve-se com `analyze_indicator_associations` conforme [test-plan.md](test-plan.md) e [ADR 0006 D5](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md). Isto **não** implica que “mais pares causam” mais ou menos entropia; a janela é descritiva.

<h2 id="vocab-ausencia-adr-0021">Vocabulário: «ausência» × frequência × atraso × <code>ausencia_blocos</code></h2>

*Quatro **papéis** (o último é o modo como estes três *tipos* de medida são separados no texto, sem renomear métrica):* **(1)** *frequência na janela*; **(2)** *atraso* e *estado* (leitura *agora*); **(3)** *`ausencia_blocos`* (e, por simetria, `frequencia_blocos`); **(4)** *distinção explícita* nesta tabela (e tabela A / nota *Ausência* no [Apêndice, ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md#apendice-frases-modelo-pt-adr-0021) — ponte alinhada a [Ponte *— quatro papéis* (catálogo, *QtdFrequencia*)](metric-catalog.md#export-legado-qtdfrequencia)). *Definições e fórmulas fechadas* permanecem no catálogo ([Tabela 1](metric-catalog.md#tabela-1-identificacao-e-tipagem), [Tabela 2](metric-catalog.md#tabela-2-semantica)); *não* reabrem-se aqui além de remeter.

O quadro alinha *intenção* de leitor com **vector 1..25** ou `count_list_by_dezena` conforme o [catálogo](metric-catalog.md); com [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) (tabelas A, nota *Ausência* à [âncora `nota-ausencia-adr-0021`](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md#nota-ausencia-adr-0021)) evita trocar rótulos.

| O que a pessoa quis dizer (exemplo de intenção) | Métrica canónica | Forma / lembrete |
|-----------------------------------------------|------------------|------------------|
| “Saiu quantas vezes nesta janela?” (popularidade *bruta*) | `frequencia_por_dezena` | `vector_by_dezena`, contagens; **soma** dos 25 = `15 × N` (N = concursos da janela). |
| “Qual é o total de vezes que saiu nesta janela?” (nome autoexplicativo; contagem total) | `total_de_presencas_na_janela_por_dezena` | `vector_by_dezena`, contagens; **equivalente** a `frequencia_por_dezena` no mesmo recorte; soma dos 25 = `15 × N`. |
| “Faz *N* concursos que *não* saiu / está *ausente* desde a última?” (frio) | `atraso_por_dezena` (e, no fim do recorte, muito do que se lê com `estado_atual_dezena`) | `vector_by_dezena`, valores 0…N; **não** soma `15×N`. O *look* de vector com muitos 0, 1, 2, … alinha a **atraso**, não a soma de `frequencia_por_dezena`. |
| “Está saindo em sequência? há quantos concursos vem saindo sem falhar?” (streak; reinicia ao ausente) | `sequencia_atual_de_presencas_por_dezena` | `vector_by_dezena`, valores 0…N; **reinicia** quando a dezena não sai. Não é contagem total na janela. |
| “Quais foram os **tamanhos** dos períodos **seguidos** *sem* a dezena?” (estrutura de blocos) | `ausencia_blocos` | `count_list_by_dezena` (codificação por dezena: dezena, nº de blocos, comprimentos…), **não** um simples vector[25] de inteiros. |
| (Análogo à linha de cima, para blocos de **presença**) | `frequencia_blocos` | `count_list_by_dezena`. |

*Redação de painéis e tabelas A (ADR 0021):* quando um exemplo de vector “como” `[0,1,1,0,1,0,0,4,0,…]` for usado em contexto de **ausência** *no sentido de atraso*, a **definição canónica** a citar no texto é `atraso_por_dezena` (ou `estado_atual_dezena` consoante o fim de janela), não `frequencia_por_dezena`.

---

## Textos de resumo para tabelas (ADR 0021)

Norma de **redação curta** para tabelas voltadas a leitores humanos quando se resume saída de `compute_window_metrics` (e equivalente), sem alterar o **contrato** JSON do MCP. Fonte de semântica e fórmulas: [metric-catalog.md](metric-catalog.md); vinculação a templates: [ADR 0021 — apresentação de resumos de janela](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) (D1–D3, Apêndice, D5).

**Não** substituem as seções *Definição* e *O que observa* por métrica abaixo: fundem ou condensam o mesmo conteúdo para a coluna certa, **ajustando** à janela (p.ex. “nestes *N* concursos”) quando fizer sentido, **sem** prometer acerto ou resultado futuro.

### Templates D1: coluna de texto a preencher

| Tipo | Onde se usa | Coluna de redação acessível | O que se omite neste *modo* (quem precisa, usa JSON ou o catálogo) |
|------|-------------|----------------------------|-------------------------------------------------------------------|
| **A** — escalares, vetores pequenos, listas 10 | Painéis de um valor ou listas curtas (ex. top 10) | **Descrição** (comportamento ou o que o número *mede* no recorte) | `shape` / unidade *no texto da tabela*; não misturar semântica com a coluna **Valor** |
| **B** — séries (um valor por concurso) | Tabelas min. / máx. (ou resumo) ao longo da janela | **O que esta série indica** (linguagem acessível; 1–2 frases) | Coluna vaga do tipo “(nota)” sem substância: deve ser trocada por esta coluna (D1 da ADR 0021) |

**Requisitos (catálogo + D1):** para `estabilidade_ranking` e outro escalar *opaco*, a coluna A **Descrição** declara, no mínimo, a **comparação de rankings de frequência entre sub-janelas consecutivas** e o intervalo \([0,1]\), alinhado ao bloco *O que observa* e à *Nota normativa — `estabilidade_ranking`* em [metric-catalog.md](metric-catalog.md). Na coluna B, a última explica o que **varia de concurso a concurso** em concreto (p.ex. “como a sorte se reparte pelas linhas do volante”); a unidade técnica, se útil, **depois** (entre parênteses).

### D2 — Vocabulário acessível mínimo (séries da Tabela B)

Obrigatório quando a métrica for uma das séries listadas em **Tabela B** (ou irmã no catálogo, com o mesmo *paper* de significado). Redação: primeiro o comportamento em português claro, depois o rótulo técnico se preciso.

- **`entropia_*_por_concurso` (linha e coluna):** grau de **mistura** das 15 dezenas entre as 5 **linhas** (ou 5 **colunas**) do volante. Mais **alto** ≈ mais **espalhado**; mais **baixo** ≈ mais **concentrado** em poucas linhas (ou colunas). Unidade: bits de Shannon (pode constar após a frase).
- **`hhi_*_por_concurso`:** **concentração** da distribuição. HHI mais **alto** ≈ mais dezenas nas **mesmas** linhas (ou colunas); mais **baixo** ≈ repartição mais **uniforme**. A sigla *HHI* (Herfindahl-Hirschman) pode seguir a frase.
- **`pares_no_concurso`:** quantas das 15 dezenas sorteadas são **números pares** (2, 4, 6, …) naquele concurso.
- **`quantidade_vizinhos_por_concurso` / `sequencia_maxima_vizinhos_por_concurso`:** pares de dezenas **consecutivas** (diferença 1) no sorteio ordenado; a segunda dá o **comprimento do maior** bloco desses vizinhos.

*Outras séries do [metric-catalog.md](metric-catalog.md):* reutilize ou condense a linha **O que observa** homónima neste glossário; mantenha coerência com a coluna B acima.

### Tabela A — textos reutilizáveis (coluna **Descrição**)

Texto padrão para o **modo resumo** ([ADR 0021 D5](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md)); pode ajustar-se a “N concursos” / “este recorte”.

| Métrica | Texto de tabela (coluna **Descrição**) |
|--------|----------------------------------------|
| `estabilidade_ranking` | Mede, entre **sub-janelas consecutivas** do recorte, se a **ordem** das 25 dezenas por **frequência** tende a manter-se parecida: 0 muito instável, 1 muito estável (intervalo \([0,1]\)). Não indica “confiança” de resultado futuro; descreve **persistência de ranking** no histórico (ver *O que observa* e catálogo). |
| `frequencia_por_dezena` | Conta quantas vezes cada dezena 1 a 25 **saiu** nos concursos **desta janela** (cada concurso conta no máximo uma vez por dezena). Soma global dos 25 = `15×N`. *Não* descreve «há *N* concursos *sem* sair»; ver a tabela *Vocabulário* [acima](#vocab-ausencia-adr-0021) e a linha de `atraso_por_dezena`. |
| `total_de_presencas_na_janela_por_dezena` | Total de vezes que cada dezena 1 a 25 **saiu** nos concursos **desta janela** (mesma contagem da frequência total; nome autoexplicativo). Soma global dos 25 = `15×N`. |
| `sequencia_atual_de_presencas_por_dezena` | Quantos concursos **seguidos** (no fim desta janela) a dezena vem **saindo sem interrupção**: quando a dezena não sai em um concurso, a sequência **reinicia** (volta a 0). |
| `atraso_por_dezena` | Número de **concursos desde a última ocorrência**; 0 = saiu no **último** sorteio do recorte. Em prosa, «ausente há *N* edições» alinha aqui, **não** a `frequencia_por_dezena` (ver [ADR 0021, apêndice tabela A](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md)). |
| `estado_atual_dezena` | `0` se a dezena saiu no último concurso do recorte; senão, atraso corrente. Mesma família de *look* de vector que o atraso, para a leitura «agora» (Tabela 2 do catálogo). |
| `top10_mais_sorteados` | Lista compacta das 10 dezenas com **mais** ocorrências de saída na janela declarada (frequência bruta, como no catálogo). |
| `top10_menos_sorteados` | Lista compacta das 10 dezenas com **menos** ocorrências de saída na mesma janela (frequência bruta). |
| `top10_maiores_totais_de_presencas_na_janela` | Lista compacta das 10 dezenas com maior **total de presenças na janela**, derivada de `total_de_presencas_na_janela_por_dezena` (desempate por dezena asc). |
| `top10_menores_totais_de_presencas_na_janela` | Lista compacta das 10 dezenas com menor **total de presenças na janela**, derivada de `total_de_presencas_na_janela_por_dezena` (desempate por dezena asc). |

### Tabela B — textos reutilizáveis (**O que esta série indica**)

| Métrica | O que a série indica (linguagem acessível) |
|--------|-------------------------------------------|
| `entropia_linha_por_concurso` | Quanto o sorteio **mistura** dezenas pelas **5 linhas** do volante: quanto **mais alto**, mais “espalhado”; **mais baixo**, mais **concentrado** em poucas linhas. (Unidade técnica: bits de Shannon, conforme [metric-catalog.md](metric-catalog.md).) |
| `entropia_coluna_por_concurso` | Mesma ideia da entropia de **linha**, para as **5 colunas** do volante. |
| `hhi_linha_por_concurso` | **Concentração** espacial: se o sorteio pesa nas **mesmas** linhas. HHI **mais alto** = mais concentrado; **mais baixo** = repartição mais **uniforme** entre linhas. |
| `hhi_coluna_por_concurso` | O **mesmo** que o HHI de **linha**, para as **5 colunas** do volante. |
| `repeticao_concurso_anterior` | **Quantas dezenas coincidem** com o concurso **imediatamente anterior** na janela (não com um sorteio afastado). |
| `pares_no_concurso` | Quantas das 15 dezenas sorteadas são **números pares** (2, 4, 6, …) naquele concurso. |
| `quantidade_vizinhos_por_concurso` | Pares de dezenas com **diferença 1** no sorteio **ordenado** (p.ex. 7 e 8) — a “colagem” numérica ao longo do tempo. |
| `sequencia_maxima_vizinhos_por_concurso` | Comprimento do **maior** bloco contíguo de dezenas consecutivas (diferença 1) **naquele** sorteio. |
| `distribuicao_linha_por_concurso` / `distribuicao_coluna_por_concurso` | **Cinco** inteiros que somam 15: **quantas** dezenas caem em cada **linha** (ou coluna) do volante, concurso a concurso. |

### D5 (profundidade) — resumo padrão *vs.* interpretação longa

| Modo | Papel desta subsecção |
|------|------------------------|
| **Resumo padrão** | Estas tabelas e o bloco *O que observa* (por métrica) fornecem o **piso** de fidelidade à semântica; preferir 1–2 frases, sem redefinir o contrato. |
| **Interpretação explícita a pedido** (mais *tokens*) | Pode alargar-se com min/máx, comparação entre séries, leitura **descritiva** do que ocorreu **na** janela, desde que **ancorada** no catálogo + glossário e nos **valores reais** devolvidos; ainda proibido prometer resultado futuro ou afirmar função inexistente no [metric-catalog.md](metric-catalog.md). |

*Última intenção desta subsecção:* cumprir D3 e o Apêndice (frases PT) de [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) no repositório; alterações de fórmula exigem alinhamento com o catálogo e revisão destes textos se a interpretação mudar.

---

## `frequencia_por_dezena`

- **Definição:** contagem de quantas vezes cada dezena (1–25) apareceu nos sorteios de uma janela temporal declarada.
- **O que observa:** popularidade bruta de cada número naquele recorte — quais saíram mais vezes.
- **Exemplo de uso:** “Nas últimas 50 edições, quais dezenas acumularam mais ocorrências para montar um ranking de frequência?”
- **Mapeamento do gráfico `QtdFrequencia` (export `indicadores.json` de referência):** o vector 1..25 nesse ficheiro é **frequência na janela** e corresponde a `frequencia_por_dezena`, não a `atraso_por_dezena` (ver secção homónima no [metric-catalog.md](metric-catalog.md)). A documentação antiga do *controller* podia chamar o endpoint de “atraso” com exemplos numéricos ambíguos; a **norma de substituição MCP** para esse export é frequência.
- **Vocabulário acessível *vs.* «ausência» (ADR 0021):** um *look* de vector com muitos 0, 1, 2, … no sentido «*N* concursos *sem* sair» alinha a [`atraso_por_dezena`](#atraso_por_dezena) (e `estado_atual_dezena` no fim do recorte), não a esta contagem; ver a tabela *Vocabulário* [acima](#vocab-ausencia-adr-0021).

---

## `total_de_presencas_na_janela_por_dezena`

- **Definição:** total de vezes que cada dezena (1–25) apareceu nos sorteios da janela declarada.
- **O que observa:** a mesma “popularidade bruta” do recorte, com um nome que deixa explícito que é **total na janela**.
- **Exemplo de uso:** “Qual foi o total de presenças por dezena nesta janela?” (equivalente a `frequencia_por_dezena`).
- **Equivalência normativa:** para a mesma janela, o vetor deve ser idêntico a `frequencia_por_dezena@1.0.0` (ver [metric-catalog.md](metric-catalog.md)).

---

## `sequencia_atual_de_presencas_por_dezena`

- **Definição:** para cada dezena, quantos concursos **seguidos** (no final da janela) ela vem saindo sem interrupção.
- **O que observa:** “streak atual” de presença; quando a dezena não sai em um concurso, o contador **reinicia** para 0.
- **Exemplo de uso:** “Quais dezenas estão em sequência de saída agora (no fim da janela)?”.

## `top10_mais_sorteados`

- **Definição:** as dez dezenas com maior `frequencia_por_dezena` na janela, com regra de desempate explícita no catálogo. O recorte temporal é **sempre o que o pedido declara** (p.ex. equivalência `start`/`fim` ↔ `window_size`+`end_contest_id` em [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) D2).
- **O que observa:** o “top quente” do período — subconjunto compacto das mais frequentes.
- **Exemplo de uso:** “Liste o top 10 de dezenas nos últimos 100 concursos para comparar com o jogo que estou avaliando.”
- **Uso em geração com range (ADR 0019):** quando o objetivo for restringir “quantas dezenas do jogo pertencem ao top 10”, não se aplica range à lista `top10_mais_sorteados` (shape de lista). Em vez disso, use uma feature escalar derivada no contexto de geração/explicação:
  - `top10_overlap_count(game) = |game ∩ top10_mais_sorteados|` (0..10)
  - `top10_overlap_ratio(game) = top10_overlap_count / 10`
  Essas features podem ser usadas como `range` ou `allowed_values` para evitar “valor fixo”.
- **Preferência (nome novo):** quando a build expuser `top10_maiores_totais_de_presencas_na_janela`, prefira derivar as mesmas features a partir dele (mesma ideia; nome deixa explícito “total na janela”):
  - `top10_overlap_count(game) = |game ∩ top10_maiores_totais_de_presencas_na_janela|` (0..10)
- **Não confundir com *exports* de UI legado** cujo rótulo sugere “histórico” com janela *rolling* implícita: esse comportamento **não** é `top10_mais_sorteados` até existir outra métrica no [metric-catalog.md](metric-catalog.md). Para o caso “top 10 **no intervalo de concursos que escolhi**”, use esta métrica (ver [ADR 0008 D3](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)).
- **D4 (sem N mágico de ecrã antigo):** o tamanho da janela **não** vem de constantes de interface legada (“últimos 10 concursos” no gráfico); o consumidor passa a janela explícita no request MCP ([ADR 0008 D4](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)).

---

## `top10_menos_sorteados`

- **Definição:** as dez dezenas com menor `frequencia_por_dezena` na janela (empates conforme catálogo).
- **O que observa:** as menos frequentes no recorte — útil para contraste com o top positivo.
- **Exemplo de uso:** “Quais dezenas apareceram menos nos últimos 30 sorteios?”

---

## `top10_maiores_totais_de_presencas_na_janela`

- **Definição:** as 10 dezenas com maior `total_de_presencas_na_janela_por_dezena` na janela (desempate por dezena asc conforme catálogo).
- **O que observa:** “top 10” do recorte baseado explicitamente no **total na janela** (nome evita ambiguidade).
- **Exemplo de uso:** “Liste o top 10 por total de presenças na janela para comparar com meu jogo.”.

---

## `top10_menores_totais_de_presencas_na_janela`

- **Definição:** as 10 dezenas com menor `total_de_presencas_na_janela_por_dezena` na janela (desempate por dezena asc conforme catálogo).
- **O que observa:** “bottom 10” do recorte baseado explicitamente no **total na janela**.
- **Exemplo de uso:** “Quais dezenas tiveram o menor total de presenças na janela?”.

## `repeticao_concurso_anterior`

- **Definição:** quantidade de dezenas comuns entre um sorteio e o imediatamente anterior (interseção entre concursos consecutivos).
- **O que observa:** quanto o resultado “carrega” do concurso passado — repetição imediata.
- **Exemplo de uso:** “Qual tem sido o número típico de dezenas repetidas em relação ao sorteio anterior nos últimos 80 concursos?”

---

## `intersecoes_multiplas`

- **Definição:** tamanho da interseção entre o sorteio atual e outro separado por uma defasagem `l` (parâmetro explícito).
- **O que observa:** sobreposição com o passado distante (não só o concurso anterior).
- **Exemplo de uso:** “Para `l = 5`, quantas dezenas coincidem em média entre sorteios afastados de 5 edições?”

---

## `atraso_por_dezena`

- **Definição:** para cada dezena, quantos concursos se passaram desde a última aparição (com política de saturação se nunca saiu, conforme documentação).
- **O que observa:** “frio” ou tempo sem sair por número — atraso actual ou na janela definida.
- **Exemplo de uso:** “Quais dezenas estão há mais edições sem ser sorteadas no histórico considerado?”
- **Não confundir com o rótulo de gráfico `QtdFrequencia` do export** `indicadores.json` de referência: esse bloco normativo mapeia para `frequencia_por_dezena` (contagens), não para atraso.
- **Vocabulário acessível (ADR 0021):** o discurso «*ausente* *N* concursos» e o padrão numérico 0, 1, 2, … batem com **esta** métrica; confundem-se em painéis com `frequencia_por_dezena` (somas) e com `ausencia_blocos` (listas de blocos) — ver [Vocabulário](#vocab-ausencia-adr-0021).

---

## `frequencia_blocos`

- **Definição:** para cada dezena, comprimentos das sequências consecutivas em que ela **apareceu** (blocos de presença).
- **O que observa:** hábito de “sequências de sorteios” em que o número reaparece de forma contínua.
- **Exemplo de uso:** “A dezena 12 costuma vir em sequências longas de concursos seguidos ou só flashes isolados?”

---

## `ausencia_blocos`

- **Definição:** para cada dezena, comprimentos das sequências consecutivas em que ela **não apareceu**.
- **O que observa:** períodos de ausência contínua — quanto tempo o número fica fora antes de voltar.
- **Exemplo de uso:** “Qual o maior período seguido sem a dezena 7 no histórico analisado?”

---

## `estado_atual_dezena`

- **Definição:** ao fim da janela, se a dezena saiu no último concurso considerado (`0`) ou o atraso corrente caso contrário.
- **O que observa:** situação “agora” da dezena para composição com outros indicadores.
- **Exemplo de uso:** “Para fechar um painel, quero saber se cada dezena do meu conjunto saiu no último sortio da janela ou há quanto tempo não sai.”

---

## `pares_impares`

- **Definição:** quantidade de dezenas pares e ímpares em um **jogo candidato** (15 dezenas); ímpares = 15 − pares.
- **O que observa:** equilíbrio paridade — estrutura simples do volante escolhido.
- **Exemplo de uso:** “Este jogo tem 8 pares e 7 ímpares; isso está dentro do perfil que quero gerar?”

---

## `pares_no_concurso`

- **Definição:** série com o número de pares em cada sorteio da janela.
- **O que observa:** como a paridade variou no tempo nos resultados reais.
- **Exemplo de uso:** “Nos últimos 50 concursos, qual a faixa típica de dezenas pares por sorteio?”

---

## `quantidade_vizinhos`

- **Definição:** em um jogo ordenado, quantos pares de dezenas consecutivas no valor (diferença 1) existem (ex.: 7 e 8).
- **O que observa:** “colagem” numérica no jogo — vizinhos no eixo 1–25.
- **Exemplo de uso:** “Quero rejeitar jogos com mais de 6 pares de vizinhos adjacentes.”

---

## `quantidade_vizinhos_por_concurso`

- **Definição:** série da métrica `quantidade_vizinhos` aplicada a cada resultado histórico na janela.
- **O que observa:** padrão histórico de adjacências nos sorteios.
- **Exemplo de uso:** “A mediana de vizinhos nos últimos 100 resultados foi quanto?”

---

## `sequencia_maxima_vizinhos`

- **Definição:** no jogo candidato, o maior comprimento de uma cadeia onde cada próxima dezena é vizinha da anterior (diferença 1).
- **O que observa:** existe uma “sequência longa” de números colados, não só a contagem total de vizinhos.
- **Exemplo de uso:** “Filtrar jogos cuja maior sequência consecutiva de vizinhos não ultrapasse 4.”

---

## `sequencia_maxima_vizinhos_por_concurso`

- **Definição:** série obtida aplicando `sequencia_maxima_vizinhos` a cada concurso da janela.
- **O que observa:** em sorteios reais, quão longas são as cadeias de vizinhos.
- **Exemplo de uso:** “Qual o percentil 90 da maior sequência de vizinhos nos últimos 200 jogos?”

---

## `distribuicao_linha`

- **Definição:** no volante 5×5 (dezenas 1–25), quantas das 15 dezenas caem em cada uma das 5 linhas.
- **O que observa:** espalhamento vertical do jogo — concentração em faixas horizontais do cartão.
- **Exemplo de uso:** “Verificar se o jogo não concentra demais nas linhas 1 e 5.”

---

## `distribuicao_linha_por_concurso`

- **Definição:** para cada sorteio na janela, o vetor de contagens por linha (série de vetores).
- **O que observa:** evolução do padrão espacial por linhas nos resultados oficiais.
- **Exemplo de uso:** “Os sorteios recentes tendem a concentrar-se nas linhas centrais?”

---

## `distribuicao_coluna`

- **Definição:** análogo a `distribuicao_linha`, mas para as 5 colunas do volante.
- **O que observa:** espalhamento horizontal por coluna no candidato.
- **Exemplo de uso:** “Checar equilíbrio entre colunas antes de fixar o jogo.”

---

## `distribuicao_coluna_por_concurso`

- **Definição:** série dos vetores coluna por sorteio na janela.
- **O que observa:** padrão histórico de distribuição por colunas.
- **Exemplo de uso:** “Comparar a coluna mais carregada no histórico recente com meu jogo.”

---

## `entropia_linha`

- **Definição:** entropia de Shannon (em bits) da distribuição das 15 dezenas pelas 5 linhas; normalmente acompanhada de `H_norm` (0–1) dividindo pelo máximo possível `log2(5)`.
- **O que observa:** o quão “espalhado” está o jogo entre linhas — baixa entropia significa concentração em poucas linhas; alta, repartição mais uniforme. Em teoria da informação, entropia mede incerteza/dispersão de uma distribuição; aqui aplica-se à forma do jogo no volante, não ao “acaso” do próximo sorteio.
- **Exemplo de uso:** “Exigir `H_norm >= 0,82` para não gerar jogos com excesso de dezenas na mesma faixa horizontal.”

---

## `entropia_linha_por_concurso`

- **Definição:** série da entropia de linha calculada para cada resultado na janela.
- **O que observa:** variabilidade histórica da “forma” por linhas dos sorteios.
- **Exemplo de uso:** “Comparar entropia de linha dos últimos 20 concursos com a do meu candidato.”

---

## `entropia_coluna`

- **Definição:** idêntica em espírito a `entropia_linha`, usando a distribuição por colunas.
- **O que observa:** dispersão lateral do jogo no volante.
- **Exemplo de uso:** “Aplicar filtro mínimo de entropia de coluna em conjunto com a de linha.”

---

## `entropia_coluna_por_concurso`

- **Definição:** série da entropia de coluna por sorteio.
- **O que observa:** como a dispersão por colunas oscilou no tempo.
- **Exemplo de uso:** “Detectar se a série recente ficou com entropias de coluna mais baixas (mais concentradas).”

---

## `hhi_concentracao`

- **Definição:** índice Herfindahl-Hirschman aplicado às proporções por linha e por coluna (par `hhi_linha`, `hhi_coluna`): soma dos quadrados das frações; valores altos indicam concentração em poucas linhas/colunas.
- **O que observa:** “quão concentrado” geometricamente está o jogo — complementar à entropia (ambos descrevem forma).
- **Exemplo de uso:** “Combinar 50% peso em entropia de linha e 50% em HHI para marcar jogos estruturalmente raros.”

---

## `hhi_linha_por_concurso` / `hhi_coluna_por_concurso`

- **Definição:** séries do HHI de linha e de coluna por sorteio na janela.
- **O que observa:** concentração espacial histórica — tendência a blocos em linhas ou colunas.
- **Exemplo de uso:** “Ver se houve mudança de regime comparando HHI médio entre duas janelas.”

---

## `matriz_numero_slot`

- **Definição:** após ordenar as 15 dezenas do sorteio, matriz de frequências `M[dezena, slot]` com dezena 1..25 e posição na ordenação 1..15, na janela.
- **O que observa:** padrão “em qual posição relativa cada número costuma aparecer” quando os resultados são ordenados — perfil de slot (sem confundir com ordem de sorteio ao vivo).
- **Exemplo de uso:** “Construir probabilidades empíricas suavizadas para `analise_slot` e `surpresa_slot`.”

---

## `analise_slot`

- **Definição:** pontuação em [0, 1] de aderência do jogo ao perfil histórico de slots (média das probabilidades suavizadas das posições ocupadas pelo jogo).
- **O que observa:** se o candidato “combina” com onde os números costumam cair nas posições ordenadas.
- **Exemplo de uso:** “Descartar jogos com aderência de slot abaixo de 0,08 na janela de referência.”

---

## `surpresa_slot`

- **Definição:** soma de −log₂ das probabilidades suavizadas nos slots ocupadas (“surpresa” em bits, estilo perplexidade).
- **O que observa:** quão “incomum” é o perfil de slots desse jogo face ao histórico — alto = padrão de posições mais raro segundo o modelo empírico.
- **Exemplo de uso:** “Priorizar jogos com menor surpresa se o objetivo é colar ao perfil recente de posições.”

---

## `intersecao_conjunto_referencia`

- **Definição:** número de dezenas em comum entre o jogo candidato e um conjunto externo declarado (ex.: fixos, lista manual).
- **O que observa:** sobreposição controlada com qualquer referência escolhida.
- **Exemplo de uso:** “Garantir pelo menos 3 acertos com um conjunto de dezenas favoritas pré-definido.”

---

## `media_janela`

- **Definição:** média aritmética dos valores de uma série numa janela.
- **O que observa:** nível típico da série no período.
- **Exemplo de uso:** “Média de `repeticao_concurso_anterior` nos últimos 40 concursos.”

---

## `desvio_padrao_janela`

- **Definição:** desvio padrão amostral dos valores da série na janela.
- **O que observa:** dispersão clássica — quanto os pontos variam em torno da média.
- **Exemplo de uso:** “Medir volatilidade da série de vizinhos por concurso.”

---

## `coeficiente_variacao`

- **Definição:** razão σ/μ, disponível apenas para séries estritamente positivas e com `μ > ε_cv`; fora disso, a normalização deve ser rejeitada conforme ADR/catálogo.
- **O que observa:** variabilidade relativa à escala da série — comparável entre magnitudes diferentes.
- **Exemplo de uso:** “Comparar estabilidade relativa entre duas métricas de escalas diferentes.”

---

## `madn_janela`

- **Definição:** desvio absoluto mediano em torno da mediana, normalizado (MADN) — medida robusta de dispersão relativa.
- **O que observa:** flutuação “típica” sem ser tão sensível a outliers quanto desvio padrão.
- **Exemplo de uso:** “Estabilidade descritiva default da série `entropia_linha_por_concurso` na janela.”

---

## `mad_janela`

- **Definição:** MAD absoluto (escala da própria série).
- **O que observa:** amplitude típica de desvio robusto, preservando unidade do indicador base.
- **Exemplo de uso:** “Reportar dispersão robusta quando a escala absoluta importa para o usuário.”

---

## `tendencia_linear`

- **Definição:** inclinação da reta de mínimos quadrados quando o eixo X é o índice temporal do recorte e Y é a série.
- **O que observa:** tendência subindo, descendo ou plana — drift linear na janela.
- **Exemplo de uso:** “A repetição com o concurso anterior está em tendência de alta nos últimos 30 sorteios?”

---

## `estabilidade_ranking`

- **Definição:** score em \([0,1]\) que mede a **persistência de posição relativa** das 25 dezenas quando a janela é dividida em `k` sub-janelas contíguas e se compara a correlação de Spearman (via Pearson nos vetores de rank) entre rankings de frequência de blocos consecutivos — ver [metric-catalog.md](metric-catalog.md) (Nota normativa).
- **O que observa:** se as dezenas que estavam mais altas no ranking de frequência num bloco tendem a permanecer altas no bloco seguinte (estabilidade de ordem global nas 25 dezenas), sem top-K fixo e sem leitura preditiva.
- **Exemplo de uso:** “No recorte recente, o ranking de frequências ficou estável entre sub-janelas ou mudou de bloco para bloco?”

---

## `divergencia_kl`

- **Definição:** divergência Kullback-Leibler D_KL(p‖q) entre distribuições empíricas de duas janelas (com smoothing add-α).
- **O que observa:** quanto duas distribuições diferem — “mudança de forma” entre períodos (não distância simétrica).
- **Exemplo de uso:** “Comparar distribuição de frequência por dezena entre ano atual e anterior.”

---

## `zscore_repeticao`

- **Definição:** valor Z da repetição observada em relação a uma referência explícita (média e desvio de baseline declarados).
- **O que observa:** se a repetição atual está alta ou baixa em desvios padrão em relação ao referencial escolhido.
- **Exemplo de uso:** “Destacar sorteios cuja repetição com o anterior foge do esperado para o baseline histórico de longo prazo.”

---

## `persistencia_atraso_extremo`

- **Definição:** conta quantas dezenas têm atraso acima de um limiar extremo de referência (ex.: P95 de um baseline), com `reference` e `baseline_version` declarados explicitamente no request.
- **O que observa:** quantidade de números simultaneamente “muito atrasados” — stress de cauda na malha de atrasos.
- **Exemplo de uso:** “Medir se o sistema de atrasos está com muitas dezenas em extremo simultâneo.”

---

## `assimetria_blocos`

- **Definição:** por dezena, razão `(presenças − ausências)/(presenças + ausências)` em blocos; agregação típica por mediana entre dezenas (ver catálogo).
- **O que observa:** desequilíbrio entre períodos de aparecer vs. sumir — tendência a cluster de presença ou ausência.
- **Exemplo de uso:** “Enriquecer ranking composto com medida de desequilíbrio presença/ausência.”

---

## `estatistica_runs`

- **Definição:** par `(sequencia_maxima_vizinhos, quantidade_vizinhos)` resumindo “runs” de vizinhança no jogo.
- **O que observa:** atalho estrutural para regras de geração que dependem dos dois números ao mesmo tempo.
- **Exemplo de uso:** “Expor um único objeto para filtros que limitam vizinhos totais e sequência máxima.”

---

## `outlier_score`

- **Definição:** distância de Mahalanobis regularizada entre o jogo e um centroide na janela, sobre cinco *features* canônicas (alinhamento de frequência, `analise_slot`, `entropia_linha.H_norm`, proporção de pares, repetição com o último sorteio — ver catálogo).
- **O que observa:** quão “afastado” o jogo está do “centro” dos padrões recentes no espaço estrutural escolhido.
- **Exemplo de uso:** “Eliminar candidatos com `outlier_score` acima do limiar para não gerar jogos muito atípicos no perfil composto.”

---

## Manutenção

- **Agregados canônicos (histogramas/padrões/matrizes):** não são “métricas novas” do catálogo. São derivações determinísticas sobre métricas canônicas, expostas pela tool `summarize_window_aggregates` conforme [mcp-tool-contract.md](mcp-tool-contract.md) e [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md). Isso evita acoplar o contrato a formatos de gráfico e garante ordenação/desempates canônicos no servidor.
- Alterações de fórmula, versão ou tipo: editar primeiro [metric-catalog.md](metric-catalog.md), depois alinhar entradas aqui se a interpretação mudar.
- Métricas novas: adicionar linha nas tabelas do catálogo e seção correspondente neste glossário.
