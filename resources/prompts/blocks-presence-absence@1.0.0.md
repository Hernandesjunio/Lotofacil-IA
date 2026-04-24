# Template — Blocos de presença/ausência (comportamento repetitivo por dezena)

Quero analisar comportamento repetitivo por dezena via **blocos**: sequências consecutivas em que a dezena aparece (presença) ou não aparece (ausência).

> Nota: algumas métricas deste template podem ser **satélites** e/ou depender de disponibilidade por build.

## Preferência de exibição (display_mode)
- `simple`: explicar como “sequências em que aparece” vs “sequências em que some” e exemplos curtos.
- `advanced`: incluir nomes das métricas (`frequencia_blocos`, `ausencia_blocos`, `assimetria_blocos`) e números-chave.
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janela sugerida
- Principal: `window_size = 100`
- (Opcional) BASELINE: `window_size = 300–500`

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, chame `get_draw_window(window_size=1)`.

## Parte 1 — Métricas alvo (100)
Quero:
- `frequencia_blocos`
- `ausencia_blocos`
- `assimetria_blocos`

Para contexto no mesmo painel:
- `frequencia_por_dezena`
- `atraso_por_dezena`
- `estado_atual_dezena`

## Parte 2 — Como apresentar no UI
- Por dezena: mostrar estatísticas simples sobre blocos (ex.: max/mediana do comprimento de blocos).
- Mostrar “assimetria” como score adicional (sempre como descrição histórica).

## Como responder
1) Proponha as calls MCP e payloads JSON.  
2) Se houver `UNKNOWN_METRIC`, indique fallback: usar frequência/atraso/estado sem blocos.  
3) Reforçar ausência de predição: blocos descrevem padrões passados no recorte.

