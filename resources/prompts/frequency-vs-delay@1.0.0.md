# Template — Frequência vs atraso (painel por dezena)

Quero um painel por dezena, com ranking e “estado” no fim da janela, usando o MCP `lotofacil-ia`.

## Preferência de exibição (display_mode)
- `simple`: explicar como “mais apareceu / menos apareceu” e “há quanto tempo não sai”, sem jargão.
- `advanced`: incluir nomes das métricas e listas (top10, vetor 1..25) e janela usada.
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janela sugerida
- RANKING: `window_size = 100` (recomendado)
- (Opcional) RECENTE: `window_size = 20` só para contextualizar com séries

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, chame `get_draw_window(window_size=1)` e use o `contest_id` como âncora.

## Parte 1 — Métricas por dezena (RANKING)
Quero:
- `frequencia_por_dezena`
- `top10_mais_sorteados`, `top10_menos_sorteados`
- `atraso_por_dezena`
- `estado_atual_dezena`

## Parte 2 — Sugestões de UI
- Ranking: top 10 / bottom 10 + tabela completa 1..25.
- “Estado atual”: destacar dezenas com `0` (saíram no último concurso da janela) vs atraso.
- Comparativo simples: frequência vs atraso (scatter ou duas colunas ordenáveis).

## Como responder
1) Proponha a call `compute_window_metrics` com as métricas acima.  
2) Se houver `UNKNOWN_METRIC`, explique se é falta de catálogo vs indisponibilidade por build.  
3) Não usar linguagem de “vai sair”; apenas “padrões observados na janela”.

