# Template — Dashboard essencial (presets 20/100/500)

Quero montar um painel de indicadores usando o MCP **`lotofacil-ia`** (Lotofacil-IA).  
Objetivo: painel **descritivo** (sem linguagem de previsão), rastreável e reproduzível.

## Preferência de exibição (display_mode)
- `simple`: linguagem bem simples (sem jargão), 3–6 linhas por seção.
- `advanced`: incluir nomes das métricas, janelas e números-chave (sem extrapolar para previsão).
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Presets de janela
- **RECENTE**: `window_size = 20` (séries por concurso)
- **RANKING**: `window_size = 100` (por dezena, menos ruído)
- **BASELINE (opcional)**: `window_size = 300–500` (comparações e referência)

## Regras do output
- Em cada chamada: incluir `dataset_version`, `tool_version`, `deterministic_hash` e `window`.
- Sem defaults ocultos: janelas sempre explícitas.

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, chame:
- `get_draw_window(window_size=1)`  
e use o `contest_id` retornado como `end_contest_id` em tudo.

## Painel A — RECENTE (20)
Quero **séries** (gráficos) e cartões (resumos) para:
- Séries:
  - `pares_no_concurso`
  - `repeticao_concurso_anterior`
  - `quantidade_vizinhos_por_concurso`
  - `sequencia_maxima_vizinhos_por_concurso`
  - `entropia_linha_por_concurso`, `entropia_coluna_por_concurso`
  - `hhi_linha_por_concurso`, `hhi_coluna_por_concurso`
- Cartões (para cada série escalar): `media_janela`, `madn_janela`, `tendencia_linear`

Se você precisar de “faixa típica”, pode usar `summarize_window_patterns` com feature(s) acima.

## Painel B — RANKING (100)
Quero um painel por dezena com:
- `frequencia_por_dezena`
- `top10_mais_sorteados`, `top10_menos_sorteados`
- `atraso_por_dezena`
- `estado_atual_dezena`

## Painel C — BASELINE (opcional)
Se eu fornecer duas janelas, proponha como comparar:
- `divergencia_kl` (quando aplicável)
- e/ou comparação de resumos (média/MADN/tendência) entre janelas

## Como responder
1) Proponha as chamadas MCP (com payloads JSON) por painel.  
2) Liste possíveis `UNKNOWN_METRIC` (subconjunto por build) e fallback.  
3) Sugira como apresentar no UI (cartões, linhas, heatmap simples).

