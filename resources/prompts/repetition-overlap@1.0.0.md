# Template — Repetição e sobreposição entre concursos

Quero identificar **comportamentos repetitivos** no histórico recente usando o MCP `lotofacil-ia`.  
Leitura sempre **descritiva na janela** (sem prever futuro).

## Preferência de exibição (display_mode)
- `simple`: explicar em linguagem simples o que mudou/permaneceu no recorte.
- `advanced`: incluir métricas, janelas e números-chave; sem leitura preditiva.
- `both` (default se não declarar): primeiro **Resumo simples**, depois **Detalhes avançados**.

## Janela sugerida
- RECENTE: `window_size = 20`
- (Opcional) RANKING: `window_size = 100` para contexto por dezena

## Passo 0 — Ancorar no último concurso (se necessário)
Se eu não fornecer `end_contest_id`, chame `get_draw_window(window_size=1)` e ancore a janela nesse concurso.

## Parte 1 — Repetição entre concursos (RECENTE)
Quero as séries e resumos:
- `repeticao_concurso_anterior`
- `pares_no_concurso` (contexto rápido)

Cartões para a série de repetição:
- `media_janela`, `madn_janela`, `tendencia_linear`

Quero também um resumo de “faixa típica”:
- usar `summarize_window_patterns` na feature `repeticao_concurso_anterior` (IQR/mediana)

## Parte 2 — (Opcional) Interseções com defasagem
Se a build suportar:
- `intersecoes_multiplas` com lag explícito (ex.: 2 e 3) para ver repetição “não imediata”.

## Parte 3 — Contexto por dezena (opcional, 100)
Se eu pedir, traga:
- `frequencia_por_dezena`
- `atraso_por_dezena`
- `estado_atual_dezena`

## Como responder
1) Proponha payloads JSON das tools necessárias.  
2) Explique como interpretar “repetição” como padrão histórico na janela, sem promessa.  
3) Se houver erro por indisponibilidade (`UNKNOWN_METRIC`), sugira substituições.

