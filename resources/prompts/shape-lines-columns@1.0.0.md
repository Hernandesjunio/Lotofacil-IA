# Template — Repetição de forma (linhas/colunas, entropia e HHI)

Quero identificar **repetição de forma** dos sorteios: padrões espaciais no volante (linhas/colunas), dispersão (entropia) e concentração (HHI).

## Preferência de exibição (display_mode)
- `simple`: explicar como “mais espalhado” vs “mais concentrado” e o que mudou entre períodos.
- `advanced`: incluir nomes das métricas (`entropia_*`, `hhi_*`, distribuições) e números-chave; sem previsão.
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janela sugerida
- RECENTE: `window_size = 20`
- (Opcional) BASELINE: `window_size = 200–500`

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, use `get_draw_window(window_size=1)`.

## Parte 1 — Séries de forma no RECENTE
Quero séries:
- `distribuicao_linha_por_concurso` (série de vetores)
- `distribuicao_coluna_por_concurso` (série de vetores)
- `entropia_linha_por_concurso`, `entropia_coluna_por_concurso`
- `hhi_linha_por_concurso`, `hhi_coluna_por_concurso`

Quero cartões (para as séries escalares de entropia/HHI):
- `media_janela`, `madn_janela`, `tendencia_linear`

## Parte 2 — Visualizações sugeridas
- Distribuição por linha/coluna: heatmap simples ou stacked por concurso.
- Entropia/HHI: linha do tempo + cartões.

## Parte 3 — Comparação com baseline (opcional)
Se eu fornecer duas janelas, comparar:
- médias/MADN/tendência em entropia/HHI
- e destacar mudanças qualitativas nos padrões por linha/coluna

## Como responder
1) Proponha payloads das tools MCP necessárias.  
2) Mostre como evitar leitura preditiva: interpretar como “forma observada na janela”.  
3) Liste possíveis `UNKNOWN_METRIC` e fallback.

