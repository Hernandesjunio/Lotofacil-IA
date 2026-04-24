# Lotofacil-IA — Getting started (MCP)

Este é um **onboarding curto** (resource Markdown) para começar a usar o MCP `lotofacil-ia`.

## O que este MCP é (e não é)
- É um MCP para **análise descritiva** do histórico e geração **reproduzível** de jogos candidatos, com **rastreabilidade**.
- **Não é predição**: evite linguagem do tipo “vai sair”, “mais provável”, “garantia”, “aumenta a chance”.

## Fluxo recomendado (agnóstico ao host)
1. **Chame a tool `help`** para obter o índice e metadados dos templates.
2. **Leia o índice**: `lotofacil-ia://prompts/index@1.0.0`.
3. **Escolha um template** (ou o mini-template de “prompt livre”) e execute um **pipeline mínimo** de tools.

## Pipeline mínimo (sem defaults ocultos)
- `get_draw_window` (opcional): use quando precisar do recorte bruto ou quando quiser **ancorar** o `end_contest_id` no concurso mais recente.
- `compute_window_metrics` (batch): peça explicitamente a lista `metrics` que você quer calcular.
- Tools analíticas conforme o objetivo: `summarize_window_patterns`, `analyze_indicator_stability`, `analyze_indicator_associations`, etc.

## Janela sempre explícita (ADR 0008)
- Declare **sempre** a janela via `window_size` + `end_contest_id` (ou a forma equivalente por extremos quando suportada).
- O servidor **não** aplica “últimos N” herdado de UI legado como default escondido.

### Preciso informar `end_contest_id`?
- Se você **souber** o concurso final, informe.
- Se você **não souber**, ancore assim:
  - chame `get_draw_window(window_size=1)`
  - use o `contest_id` retornado como `end_contest_id` nas próximas chamadas

## Rastreabilidade e determinismo (obrigatório)
Em toda resposta (tools), confira:
- `dataset_version`
- `tool_version`
- `deterministic_hash`
- `window` (tamanho e extremos usados)

Se duas chamadas idênticas (mesma janela e mesmos argumentos) retornarem `deterministic_hash` diferente, isso é um bug de determinismo.

## Por que apareceu `UNKNOWN_METRIC`?
Significa que o nome pode estar no catálogo semântico, mas **esta build** não expõe a métrica nessa rota (allowlist por tool/build). Use o índice/templates como guia e ajuste para métricas suportadas.

## Por que apareceu `INVALID_REQUEST`?
Faltou campo obrigatório, tipo errado, enum inválido ou combinação ambígua (por exemplo, parâmetros de janela incompatíveis). Corrija o JSON do request; o servidor não “adivinha” defaults.

