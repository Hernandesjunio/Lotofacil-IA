# Guia de Execução Spec-Driven — ADR 0023 (eficiência)

**Navegação:** [← Brief (índice)](brief.md) · [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md) · **Guia completo (arquivado):** `docs/archived/spec-driven-execution-guide.md`

Este arquivo foi **reduzido** para ganhar eficiência no consumo de tokens. As fases antigas (V0/V1 e ADRs anteriores) permanecem no guia arquivado.

## Objetivo

Materializar a [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md):

- reduzir tokens (evitar duplicação de JSON no canal textual),
- permitir controle explícito de detalhe (`verbosity`, `include_explanations`),
- permitir **projeção** (`fields`) e paginação determinística quando necessário,
- tornar knobs descobríveis via `help` e `discover_capabilities`,
- manter determinismo e regra clara para `deterministic_hash` quando a apresentação variar.

## Referências normativas mínimas

- [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [ADR 0005 (transportes)](adrs/0005-transporte-mcp-e-superficie-tools-v1.md)
- (quando aplicável) [ADR 0008 (descoberta)](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md), [ADR 0009 (help/resources)](adrs/0009-help-e-catalogo-de-templates-resources-v1.md)

---

## Fases (ADR 0023)

### Fase 23.1 — Fechar contrato dos knobs de economia

**Objetivo:** padronizar parâmetros transversais onde aplicável.

- Definir `verbosity`: `minimal | standard | full`.
- Definir `include_explanations` onde aplicável.
- Definir `fields` (ou `response_projection`) para projeção server-side quando a resposta for grande.
- Fechar a regra de `deterministic_hash` quando `verbosity/include_explanations/fields` alterarem a apresentação.

**Pronto quando:** o contrato documenta knobs + hashing sem ambiguidade.

### Fase 23.2 — Separar canais (StructuredContent vs Content) sem duplicar JSON

**Objetivo:** `StructuredContent` é a fonte canônica; `Content` é resumo humano curto.

- Garantir que `Content` não repita o JSON completo do `StructuredContent` por padrão.
- Mapear `verbosity` → tamanho/estilo do resumo humano em `Content`.

**Pronto quando:** em `minimal`, `Content` é curto e não contém JSON completo.

### Fase 23.3 — Projeção (`fields`) e explicações opt-in

**Objetivo:** reduzir payload na fonte.

- Definir por tool quais campos suportam projeção e como validar `fields`.
- Definir por tool quando `include_explanations` é aceito e o que muda.

**Pronto quando:** projeção é determinística e validada (erro claro para campos inválidos).

### Fase 23.4 — Paginação determinística (quando aplicável)

**Objetivo:** controlar respostas grandes em `full` de forma reprodutível.

- Definir esquema de paginação determinística (ex.: `page/page_size` ou `cursor`).
- Fixar ordenação canônica e estabilidade do cursor (quando existir).

**Pronto quando:** mesmas entradas + mesmos knobs retornam o mesmo subconjunto paginado.

### Fase 23.5 — Descoberta/UX: `help` e `discover_capabilities`

**Objetivo:** reduzir tentativa/erro e tornar knobs visíveis.

- `help` explica “modo econômico” vs detalhado (mapeando para knobs).
- `discover_capabilities` declara suporte por tool (knobs, enums aceitos, defaults recomendados) sem payload excessivo.

**Pronto quando:** um leigo consegue pedir `minimal/full` com poucas tentativas.

### Fase 23.6 — Evidências e testes (contrato + determinismo + custo)

**Objetivo:** travar eficiência e determinismo por testes.

- Testes cobrindo ao menos `minimal` e `full` nas tools alvo.
- Testes garantindo: `Content` não duplica JSON; `StructuredContent` permanece canônico.
- Testes cobrindo a regra de `deterministic_hash` definida na Fase 23.1.

**Pronto quando:** regressões de duplicação/knobs/hashing quebram testes.

