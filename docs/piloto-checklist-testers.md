# Piloto (testers) — checklist operacional curto (MCP `lotofacil-ia`)

**Status:** guia operacional (não normativo)  
**Data:** 2026-05-03  
**Público:** testers / hosts MCP  

**Navegação:** [← Brief (índice)](brief.md) · [Contrato MCP](mcp-tool-contract.md) · [Roadmap pós‑piloto](roadmap-evolucao-ampla-e-piloto-controlado.md)

---

## Objetivo

Dar um roteiro **reprodutível** para validar “fluidez” e utilidade mínima no consumo via MCP, sem substituir o contrato normativo.

Este checklist assume consumo **educacional/descritivo** sobre histórico (ver [brief.md](brief.md)): não avaliar “acerto” nem “previsão”.

---

## Pré‑requisitos (antes de testar)

1. **Dataset configurado** para o processo do servidor:
   - variável de ambiente **`Dataset__DrawsSourceUri`** (`.NET`: `Dataset:DrawsSourceUri`) apontando para uma fonte válida (local/`file://`/HTTP conforme perfil suportado pela build), sem fallback implícito — ver [ADR 0022](adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md).
2. **Host MCP** capaz de exibir **`StructuredContent`** (idealmente também `Content`), sem depender só do texto do chat.
3. **Expectativa de versão**: anotar `tool_version` retornado (varia por build).

---

## Smoke mínimo (5 minutos)

Execute nesta ordem:

1. **`help`**
   - Confirme que o quickstart menciona um fluxo mínimo (`get_draw_window` pequeno + `compute_window_metrics`) e campos de rastreabilidade (`dataset_version`, `tool_version`, `deterministic_hash`, `window`).
2. **`discover_capabilities`**
   - Confirme constraints operacionais de janela (ex.: `window_size > 0`, coerência `start/end`, quickstart `window_size=1`).
3. **`get_draw_window`**
   - `window_size=1` **sem** `end_contest_id` para ancorar no último concurso disponível na fonte configurada.
   - Verifique `window` e que `draws` está ordenado/canônico conforme contrato.
4. **`compute_window_metrics`**
   - Use o `end_contest_id` devolvido na etapa anterior (ou equivalente explícito).
   - Comece com **uma métrica** estável e barata: `frequencia_por_dezena`.
   - Confirme `dataset_version`, `tool_version`, `deterministic_hash`, `window`, e `metrics[]`.

---

## Varredura de verbosidade (`minimal` / `standard` / `full`)

Objetivos distintos (ver [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)):

| `verbosity` | O que validar no piloto |
|-------------|-------------------------|
| `minimal` | Economia de tokens; útil quando o host prioriza `StructuredContent`. Verifique que o essencial não “desaparece” só do lado humano sem você conseguir ler o canónico. |
| `standard` | Chat‑safe: `Content` deve carregar o fato principal (sem depender de JSON completo). |
| `full` | Completude canónica + uso de knobs (`fields`, `page`/`page_size` quando aplicável). |

**Sugestão de request fixo** (para comparar apple‑to‑apple entre níveis):

- `window_size`: 5 ou 10 (explícito)
- `end_contest_id`: o último observado no smoke mínimo
- `metrics`: `[{ "name": "frequencia_por_dezena" }]`
- `include_explanations`: começar com `false` se o objetivo for comparar hash/payload; depois repetir com `true` se quiser validar textos explicativos

---

## Casos negativos (alto sinal, baixo custo)

1. **Métrica fora da rota / fora do escopo**
   - Esperado: `UNKNOWN_METRIC` com `details` coerentes com `discover_capabilities` (ver [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) / [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)).
2. **`allow_pending`**
   - Para métricas `pending`, validar opt‑in conforme [ADR 0014](adrs/0014-semantica-real-de-allow-pending-v1.md).
3. **Dataset ausente/ inválido (somente se puder testar com segurança)**
   - Esperado: `DATASET_UNAVAILABLE` com `reason` canónico (ex.: `missing_env`, `invalid_format`) — ver [ADR 0022](adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md).

---

## O que registrar no feedback (template curto)

Para cada sessão, copie:

- **Build/host**: IDE/agente, SO, e como o MCP foi iniciado (STDIO).
- **Dataset**: tipo de fonte (local/http), e se houve mudança durante o teste.
- **Tool + request** (JSON canónico).
- **Saídas**: `tool_version`, `dataset_version`, `deterministic_hash`, e se `Content` foi útil em `standard/full`.
- **Falhas**: código (`DATASET_UNAVAILABLE`, `UNKNOWN_METRIC`, etc.), `details` e repro steps.

---

## Anti‑padrões (não reportar como “bug de métrica”)

- Interpretar indicadores como **previsão** ou “número forte” para o próximo sorteio (fora do escopo pedagógico).
- Comparar fluidez apenas pelo texto do chat se o host não mostrar `StructuredContent` (isso é limitação de UI/host, não necessariamente do servidor).
