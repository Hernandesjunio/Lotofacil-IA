# Template — Mudança de regime (comparar janelas)

Quero comparar **duas janelas** para descrever mudança de regime (sem inferir previsão), usando o MCP `lotofacil-ia`.

## Preferência de exibição (display_mode)
- `simple`: dizer “ficou mais X / menos Y” com poucos números e exemplos.
- `advanced`: incluir métricas, janelas comparadas e valores-chave; sem extrapolar.
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janelas sugeridas
Escolha duas janelas comparáveis (exemplos):
- RECENTE vs ANTERIOR: 20 vs 20
- CURTA vs MÉDIA: 20 vs 100
- RECENTE vs BASELINE: 100 vs 300–500

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, chame `get_draw_window(window_size=1)`.

## Parte 1 — Comparação por divergência (quando aplicável)
Se disponível/adequado, usar:
- `divergencia_kl` (comparação entre 2 janelas) para distribuições relevantes

## Parte 2 — Comparação por resumos (fallback universal)
Comparar, entre janelas, resumos de séries como:
- `entropia_linha_por_concurso`, `entropia_coluna_por_concurso`
- `hhi_linha_por_concurso`, `hhi_coluna_por_concurso`
- `pares_no_concurso`

Resumos por janela:
- `media_janela`, `madn_janela`, `tendencia_linear`

## Como responder
1) Proponha as calls MCP (incluindo como materializar as duas janelas).  
2) Explique quais diferenças são “grandes” apenas no sentido descritivo (magnitude, tendência, dispersão).  
3) Se `divergencia_kl` não estiver disponível, use só o fallback de resumos.

