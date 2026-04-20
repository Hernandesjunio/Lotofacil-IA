# Catálogo de métricas

## Convenções


| Campo               | Descrição                        |
| ------------------- | -------------------------------- |
| **Nome**            | Identificador estável da métrica |
| **Definição**       | O que a métrica mede             |
| **Fórmula / regra** | Como é obtida                    |
| **Fonte**           | Dados de entrada                 |
| **Consumidor**      | Módulos ou relatórios que usam   |


## Métricas


| Nome                           | Definição                                                          | Fórmula / regra                                                        | Fonte                            | Consumidor |
| ------------------------------ | ------------------------------------------------------------------ | ---------------------------------------------------------------------- | -------------------------------- | ---------- |
| top10_mais_sorteados           | 10 dezenas mais frequentes na coleção histórica                    | freq[num] = contagem em concursos; ordenar desc; top 10                | Histórico de concursos Lotofácil | MCP / API  |
| top10_menos_sorteados          | 10 dezenas menos frequentes na coleção histórica                   | freq[num] = contagem em concursos; ordenar asc; top 10                 | Histórico de concursos Lotofácil | MCP / API  |
| intersecao_conjunto_referencia | Quantidade de dezenas em comum entre jogo e conjunto de referência | count(jogo ∩ referencia)                                               | N/A                              | MCP / API  |
| top10_dinamico                 | 10 dezenas mais frequentes em concursos recentes                   | N/A                                                                    | Histórico de concursos Lotofácil | MCP / API  |
| repeticao_concurso_anterior    | Quantidade de dezenas repetidas entre concursos consecutivos       | |J_t ∩ J_{t-1}|                                                        | Histórico de concursos Lotofácil | MCP / API  |
| pares_impares                  | Quantidade de dezenas pares em um jogo                             | pares = n % 2 == 0                                                     | N/A                              | MCP / API  |
| quantidade_vizinhos            | Quantidade de números consecutivos no jogo                         | if (n[i+1] - n[i] == 1)                                                | N/A                              | MCP / API  |
| sequencia_maxima_vizinhos      | Maior sequência consecutiva de números                             | Dos últimos x jogos calcula a quantidade máxima de números sequenciais | Histórico de concursos Lotofácil | MCP / API  |
| atraso_por_dezena              | Distância até última ocorrência de cada dezena                     | N/A                                                                    | Histórico de concursos Lotofácil | MCP / API  |
| frequencia_blocos              | Sequências consecutivas de presença de uma dezena                  | N/A                                                                    | Histórico de concursos Lotofácil | MCP / API  |
| ausencia_blocos                | Sequências consecutivas de ausência de uma dezena                  | N/A                                                                    | Histórico de concursos Lotofácil | MCP / API  |
| estado_atual                   | Estado da dezena baseado no último concurso                        | saiu → ausência = 0; não saiu → frequência = 0                         | Histórico de concursos Lotofácil | MCP / API  |
| matriz_numero_slot             | Distribuição de números por posição no sorteio                     | matriz número (1..25) x posição (1..15)                                | Histórico de concursos Lotofácil | MCP / API  |
| analise_slot                   | Classificação de recorrência por posição                           | N/A                                                                    | Histórico de concursos Lotofácil | MCP / API  |
| distribuicao_linha             | Distribuição de dezenas por linha do volante                       | contagem por linha (0..5)                                              | N/A                              | MCP / API  |
| distribuicao_coluna            | Distribuição de dezenas por coluna do volante                      | contagem por coluna (0..5)                                             | N/A                              | MCP / API  |
| percentual                     | Representação percentual de um valor sobre o total                 | (valor / total) * 100                                                  | N/A                              | MCP / API  |
| recorte_recente                | Seleção de concursos recentes                                      | concurso > max - 10                                                    | Histórico de concursos Lotofácil | MCP / API  |
| entropia_linha                 | Medida de dispersão por linha                                      | H = -Σp_i log(p_i)                                                     | N/A                              | MCP / API  |
| entropia_coluna                | Medida de dispersão por coluna                                     | H = -Σp_i log(p_i)                                                     | N/A                              | MCP / API  |
| hhi_concentracao               | Índice de concentração das probabilidades                          | HHI = Σp_i^2                                                           | N/A                              | MCP / API  |
| zscore_repeticao               | Desvio padronizado da repetição                                    | Z = (R - μ) / σ                                                        | Histórico de concursos Lotofácil | MCP / API  |
| zscore_top10                   | Desvio padronizado baseado no Top10                                | N/A                                                                    | Histórico de concursos Lotofácil | MCP / API  |
| divergencia_kl                 | Divergência entre distribuições                                    | D_KL = Σ p log(p/q)                                                    | N/A                              | MCP / API  |
| cusum_ewma                     | Detecção de mudança em sequência de eventos                        | modelo CUSUM / EWMA                                                    | N/A                              | MCP / API  |
| persistencia_atraso_extremo    | Contagem de atrasos acima do percentil 95                          | Σ I(A_i > P95)                                                         | Histórico de concursos Lotofácil | MCP / API  |
| assimetria_blocos              | Assimetria entre presença e ausência                               | (pres - aus) / (pres + aus)                                            | Histórico de concursos Lotofácil | MCP / API  |
| estatistica_runs               | Combinação de métricas de sequência                                | combinação de vizinhos e sequência máxima                              | N/A                              | MCP / API  |
| surpresa_slot                  | Medida de surpresa por posição                                     | -Σ log p(n,s)                                                          | N/A                              | MCP / API  |
| intersecoes_multiplas          | Interseção entre concursos com defasagem                           | |J_t ∩ J_{t-l}|                                                        | Histórico de concursos Lotofácil | MCP / API  |


