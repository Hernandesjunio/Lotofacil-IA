# Template — Associações (Spearman) para co-movimento descritivo

Quero medir **associações (Spearman)** entre séries alinhadas por concurso na mesma janela, apenas como **co-movimento** (sem causalidade).

## Preferência de exibição (display_mode)
- `simple`: explicar “andam juntos / andam ao contrário” na janela (sem falar em causa).
- `advanced`: incluir método, itens e magnitude; destacar limitações (correlação ≠ causalidade).
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janela sugerida
- Recomendado: `window_size = 100`
- (Opcional) rápido: `window_size = 20` para “sanity check” recente

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, chame `get_draw_window(window_size=1)`.

## Itens típicos para testar
Usar `analyze_indicator_associations` com `method="spearman"` em combinações como:
- `pares_no_concurso` × `entropia_linha_por_concurso`
- `quantidade_vizinhos_por_concurso` × `entropia_linha_por_concurso`
- `repeticao_concurso_anterior` × `pares_no_concurso`

## Como responder
1) Proponha o payload JSON para `analyze_indicator_associations` (itens e janela).  
2) Explique como reportar: “associação positiva/negativa na janela” e magnitude; sem extrapolar.  
3) Se a build tiver restrições (ex.: `stability_check` não suportado), não usar campos não suportados.

