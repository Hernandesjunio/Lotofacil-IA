# Template — Vizinhos e runs no tempo (comportamento repetitivo)

Quero analisar padrões repetitivos de **adjacências** (vizinhos) nos sorteios recentes via MCP `lotofacil-ia`.

## Preferência de exibição (display_mode)
- `simple`: resumo curto em termos cotidianos (ex.: “muitos vizinhos” vs “poucos vizinhos”).
- `advanced`: incluir nomes das métricas e valores de janela/séries; sem inferir previsão.
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janela sugerida
- RECENTE: `window_size = 20`
- (Opcional) BASELINE: `window_size = 200–500` para comparar “recente vs típico”

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, use `get_draw_window(window_size=1)`.

## Parte 1 — Séries principais (RECENTE)
Quero séries (por concurso):
- `quantidade_vizinhos_por_concurso`
- `sequencia_maxima_vizinhos_por_concurso`

E cartões (resumo estatístico por série):
- `media_janela`, `madn_janela`, `tendencia_linear`

Quero também faixa típica:
- `summarize_window_patterns` para as features acima (IQR)

## Parte 2 — Contexto (RECENTE)
Para contextualizar comportamento repetitivo:
- `pares_no_concurso`
- `repeticao_concurso_anterior`

## Parte 3 — Comparação com baseline (opcional)
Se eu fornecer duas janelas, proponha comparação de:
- médias/MADN/tendência das séries de vizinhos

## Como responder
1) Proponha as calls MCP e payloads JSON.  
2) Explique como representar no UI (linha do tempo + cartões).  
3) Liste `UNKNOWN_METRIC` e fallback (se houver).

