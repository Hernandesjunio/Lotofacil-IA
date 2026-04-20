# Glossário de métricas (definição, interpretação e exemplos)

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Documento pedagógico complementar ao catálogo técnico em [metric-catalog.md](metric-catalog.md). Aqui cada métrica tem **definição**, **o que observa** (interpretação em linguagem simples) e **exemplo de uso**. Fórmulas detalhadas, tipagem e versões permanecem no catálogo.

**Nota sobre predição:** todas descrevem padrões no histórico ou estrutura de um jogo; nenhuma implica acerto futuro.

---

## `frequencia_por_dezena`

- **Definição:** contagem de quantas vezes cada dezena (1–25) apareceu nos sorteios de uma janela temporal declarada.
- **O que observa:** popularidade bruta de cada número naquele recorte — quais saíram mais vezes.
- **Exemplo de uso:** “Nas últimas 50 edições, quais dezenas acumularam mais ocorrências para montar um ranking de frequência?”

---

## `top10_mais_sorteados`

- **Definição:** as dez dezenas com maior `frequencia_por_dezena` na janela, com regra de desempate explícita no catálogo.
- **O que observa:** o “top quente” do período — subconjunto compacto das mais frequentes.
- **Exemplo de uso:** “Liste o top 10 de dezenas nos últimos 100 concursos para comparar com o jogo que estou avaliando.”

---

## `top10_menos_sorteados`

- **Definição:** as dez dezenas com menor `frequencia_por_dezena` na janela (empates conforme catálogo).
- **O que observa:** as menos frequentes no recorte — útil para contraste com o top positivo.
- **Exemplo de uso:** “Quais dezenas apareceram menos nos últimos 30 sorteios?”

---

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

- **Definição:** razão σ/μ (com restrições e fallbacks para séries não positivas, ver ADR/catálogo).
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

- **Definição:** *pendente de detalhamento no catálogo (versão 0.1.0).* Pretende capturar persistência de ranking entre sub-janelas.
- **O que observa:** *a definir quando a métrica for fechada.*
- **Exemplo de uso:** *uso apenas com `allow_pending: true` após leitura da especificação futura.*

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

- **Definição:** conta quantas dezenas têm atraso acima de um limiar de referência (ex.: P95 de um baseline), conforme catálogo.
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

- Alterações de fórmula, versão ou tipo: editar primeiro [metric-catalog.md](metric-catalog.md), depois alinhar entradas aqui se a interpretação mudar.
- Métricas novas: adicionar linha nas tabelas do catálogo e seção correspondente neste glossário.
