# Template — Estabilidade do ranking (persistência de ordem relativa)

Quero medir se o **ranking por dezena** (frequência) está “mudando muito” ao longo do recorte, de forma descritiva.

## Preferência de exibição (display_mode)
- `simple`: explicar “mudou pouco” vs “mudou bastante” usando exemplos concretos (ex.: top 10).
- `advanced`: incluir métricas, janelas, números-chave e limitações (ex.: métrica indisponível na build).
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janela sugerida
- Recomendado: `window_size = 100` (ou 200 se você quiser mais “massa”)

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, use `get_draw_window(window_size=1)`.

## Parte 1 — Estabilidade do ranking
Quero:
- `estabilidade_ranking` (score em [0,1] conforme catálogo)

E para contexto:
- `frequencia_por_dezena`
- `top10_mais_sorteados`, `top10_menos_sorteados`

## Como responder
1) Proponha as calls MCP e payloads JSON.  
2) Explique a leitura: “posição relativa permaneceu parecida” vs “ranking girou bastante” na janela.  
3) Se `estabilidade_ranking` não estiver disponível na build, ofereça fallback: comparar rankings de duas janelas manualmente (ex.: 50 vs 50) via `frequencia_por_dezena`.

