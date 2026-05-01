# Templates atômicos de execução — ADR 0023 (eficiência)

**Navegação:** [← Brief (índice)](brief.md) · [spec-driven-execution-guide.md](spec-driven-execution-guide.md) · [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md) · **Templates antigos (arquivado):** `docs/archived/fases-execucao-templates.md`

Este arquivo contém **somente** os templates das fases presentes no `docs/spec-driven-execution-guide.md` (enxugado para ADR 0023).

**Regra de ouro:** implemente **apenas** o recorte descrito na fase.

---

## Fase 23.1 — Fechar contrato dos knobs de economia

```md
Implemente apenas o recorte normativo da ADR 0023 no contrato:

- Definir `verbosity`: `minimal | standard | full`.
- Definir `include_explanations` onde aplicável.
- Definir `fields` (ou `response_projection`) para projeção server-side em respostas grandes.
- Fechar a regra de `deterministic_hash` quando `verbosity/include_explanations/fields` alterarem a apresentação.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Contrato documenta knobs + hashing sem ambiguidade.
```

## Fase 23.2 — Separar canais (StructuredContent vs Content) sem duplicar JSON

```md
Implemente apenas a política de canais da ADR 0023:

- `StructuredContent` permanece como JSON canônico.
- `Content` vira resumo humano curto, alinhado a `verbosity`.
- Proibir duplicar o JSON completo do `StructuredContent` dentro do `Content` por padrão.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Em `verbosity="minimal"`, `Content` não contém JSON completo e segue útil como resumo.
```

## Fase 23.3 — Projeção (`fields`) e explicações opt-in

```md
Implemente apenas projeção e explicações opt-in:

- Definir por tool quais campos suportam `fields` e como validar.
- Definir por tool quando `include_explanations` é aceito e o que muda na resposta.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Projeção é determinística e valida `fields` com erro estruturado quando inválido.
```

## Fase 23.4 — Paginação determinística (quando aplicável)

```md
Implemente apenas paginação determinística para respostas grandes em `full`:

- Definir esquema (`page/page_size` ou `cursor`) e limites.
- Fixar ordenação canônica e estabilidade do cursor (quando existir).

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Mesmas entradas + mesmos knobs retornam o mesmo subconjunto paginado.
```

## Fase 23.5 — Descoberta/UX: `help` e `discover_capabilities`

```md
Implemente apenas a descoberta/UX dos knobs:

- `help` explica como pedir "modo econômico" vs detalhado, mapeando para `verbosity/include_explanations/fields`.
- `discover_capabilities` declara quais tools suportam quais knobs, enums aceitos e defaults recomendados, sem payload excessivo.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md (alinhamento de descoberta)
- docs/adrs/0009-help-e-catalogo-de-templates-resources-v1.md (quando aplicável)

Critério de pronto:
- Um leigo consegue pedir `minimal`/`full` com poucas tentativas e sem tentativa/erro de parâmetros.
```

## Fase 23.6 — Evidências e testes (contrato + determinismo + custo)

```md
Implemente apenas as evidências e testes para travar eficiência:

- Testes de contrato cobrindo ao menos `minimal` e `full` nas tools alvo.
- Teste garantindo que `Content` não duplica JSON do `StructuredContent`.
- Testes cobrindo a regra de `deterministic_hash` definida para knobs de apresentação.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Regressões de duplicação/knobs/hashing quebram testes.
```

