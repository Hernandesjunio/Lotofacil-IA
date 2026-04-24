# Template — Triagem candidato × histórico (explicação e aderência)

Quero avaliar jogos candidatos com base no histórico declarado e obter explicações reprodutíveis via MCP `lotofacil-ia`.

## Preferência de exibição (display_mode)
- `simple`: explicar “por que esse jogo parece comum/rarO no recorte” sem jargão e em poucas linhas.
- `advanced`: incluir métricas citadas no breakdown, janela, scores e filtros; sem previsão.
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janela sugerida
- Recomendado: `window_size = 100`
- (Opcional) RECENTE: `window_size = 20` para comparação “curta”

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, use `get_draw_window(window_size=1)`.

## Parte 1 — Se eu já tenho jogos candidatos
Quero chamar `explain_candidate_games` com:
- `games`: lista de jogos (15 dezenas cada)
- `include_metric_breakdown = true`
- `include_exclusion_breakdown = true`

E quero que a explicação enfatize métricas estruturais como:
- `pares_impares`
- `quantidade_vizinhos`, `sequencia_maxima_vizinhos`
- `entropia_linha`, `entropia_coluna`
- `hhi_concentracao`
- `analise_slot`, `surpresa_slot`
- `outlier_score`

## Parte 2 — Se eu quiser gerar candidatos antes (opcional)
Usar `generate_candidate_games` com `seed` explícito e plano por estratégia, e depois explicar os gerados.

## Como responder
1) Proponha os payloads JSON completos.  
2) Explique como interpretar “aderência” e “raridade” como descrições históricas na janela.  
3) Sem linguagem de previsão (“mais provável de sair”).

