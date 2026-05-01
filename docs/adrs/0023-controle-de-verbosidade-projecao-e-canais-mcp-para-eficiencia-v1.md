# ADR 0023 — Controle de verbosidade, projeção de resposta e canais MCP (StructuredContent vs Content) para eficiência

**Navegação:** [← Brief (índice)](../brief.md) · [Contrato MCP](../mcp-tool-contract.md) · [ADR 0005 (transportes)](0005-transporte-mcp-e-superficie-tools-v1.md) · [ADR 0008 (descoberta)](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) · [ADR 0009 (help/resources)](0009-help-e-catalogo-de-templates-resources-v1.md)

## Status

Proposto.

**Data:** 2026-05-01

## Contexto

1. **Objetivo de produto (eficiência):** o MCP é consumido hoje via **Cursor chat** e também via **HTTP** (endpoints espelhados) e **stdio**. O custo relevante é:
   - tokens gerados pelo servidor nas respostas (principalmente no canal textual);
   - round-trips (tool calls) necessárias para obter um resultado útil.
2. **Duplicação de payload na resposta MCP:** o SDK MCP permite devolver payload estruturado e texto. Se o servidor repetir o JSON completo em ambos, o host que lê ambos pode pagar tokens duplicados.
3. **Escolha de detalhe deve ser do operador:** o nível de detalhe (resumo vs detalhado) deve ser escolhido por quem opera o chat (ou política do host/agente), não por defaults ocultos do servidor.
4. **Servidor é stateless por request:** o contrato proíbe que o servidor mantenha “preferências” de sessão; preferências devem ser mantidas no host/agente (ver [mcp-tool-contract.md](../mcp-tool-contract.md), invariantes; e [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md) sobre o papel do host no protocolo).
5. **Extensibilidade futura:** haverá um agente dedicado com LLM e API key própria, que deve preferir consumir **JSON estruturado** de forma estável e barata, sem depender de texto.
6. **Streaming:** no transporte MCP (tools/call) a resposta é um payload final; já em HTTP, streaming (SSE/chunked/NDJSON) pode existir, mas não deve ser a única forma de eficiência (projeção/paginação/verbosidade são mais simples, testáveis e determinísticos).

## Decisões

### D1 — Introduzir parâmetros transversais de economia (`verbosity`, `include_explanations`, `fields/response_projection`)

**Decisão:** Tools que retornam payloads potencialmente grandes (listas, matrizes, breakdowns) devem aceitar parâmetros consistentes:

- `verbosity`: `"minimal" | "standard" | "full"`
- `include_explanations`: `boolean` (quando aplicável)
- `fields` (ou `response_projection`): lista de campos a incluir na resposta (projeção server-side), quando a tool tiver resposta grande e parcialmente útil.

**Semântica normativa (resumo):**

- `minimal`: deve omitir texto explicativo e campos redundantes; deve priorizar “dados mínimos auditáveis”.
- `standard`: mantém comportamento atual (ou próximo), com explicações curtas.
- `full`: inclui detalhamento completo, podendo exigir paginação (D4).

**Regra de determinismo:** se `deterministic_hash` for hash do payload canônico, mudanças de `verbosity/fields` devem ser refletidas no input hashed **ou** o contrato deve definir que o hash é da semântica e não da apresentação. A decisão específica de hashing por tool deve ser fechada no contrato quando estes parâmetros forem implementados.

### D2 — Separar canais: `StructuredContent` como fonte canônica; `Content` como resumo humano (nunca duplicar JSON)

**Decisão:** Em respostas MCP (stdio e HTTP MCP), o servidor deve tratar os canais como complementares:

- `StructuredContent`: **JSON canônico** (contrato) — destinado a automação/SDK/agentes.
- `Content`: **texto humano curto** — destinado a chat/UX e alinhado a `verbosity`.

**Proibição:** o servidor **não** deve repetir o JSON completo do `StructuredContent` dentro de `Content` por padrão.

**Justificativa:** reduz tokens no consumo por chat e preserva extensibilidade para agentes que consomem structured JSON.

### D3 — Preferência e escolha do modo pertencem ao host/agente; o servidor não persiste “modo”

**Decisão:** O servidor não mantém estado de “modo econômico” entre chamadas. O host/agente:

- interpreta intenção do usuário (“modo econômico”, “detalhado”, “só números”) e mapeia para `verbosity/include_explanations/fields`;
- mantém essa preferência na sessão (chat) e aplica nos próximos calls.

**Consequência:** `help` e `discover_capabilities` devem tornar essa escolha evidente para leigos (ver D5).

### D4 — Paginação determinística como mecanismo principal para respostas grandes; streaming em HTTP é opcional

**Decisão:** Para payloads grandes em `full`, a forma primária de controle é:

- **paginação determinística** (ex.: `page`/`page_size` ou `cursor` estável por ordenação canônica), e/ou
- **projeção** (`fields`) para reduzir payload na fonte.

**Streaming (HTTP):** pode ser suportado no futuro para reduzir latência percebida, mas:

- não substitui paginação/projeção;
- deve manter determinismo (ordem canônica, contagem total, e cursor reproduzível quando aplicável);
- não deve exigir que o consumidor implemente lógica complexa para obter o “resultado final”.

### D5 — Descoberta e UX para leigos: `help` e `discover_capabilities` declaram knobs e exemplos

**Decisão:** Para reduzir tentativa/erro e permitir uso por leigos:

- `help` deve explicar, em linguagem simples, como pedir `minimal/standard/full` e quando usar.
- `discover_capabilities` deve declarar:
  - quais tools suportam `verbosity/include_explanations/fields`;
  - valores aceitos (enums) e defaults recomendados;
  - avisos de custo (“para respostas curtas no chat, use minimal”).

**Alinhamento:** esta decisão complementa o modelo híbrido de descoberta do [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) (instância/build via tool + norma via docs/resources).

## Consequências

### Positivas

- Redução imediata de tokens no Cursor chat ao evitar duplicação de JSON no canal textual.
- Menos tool calls por permitir `fields` e `minimal` em tools “meta” e de payload grande.
- Extensibilidade: agentes futuros podem consumir `StructuredContent` de forma estável.

### Custos

- Mais combinatória de testes: cada tool que suportar `verbosity/fields` precisa de testes de contrato cobrindo ao menos `minimal` e `full` (e paginação quando aplicável).
- Decisão explícita de `deterministic_hash` por tool quando apresentação variar (contrato precisa ser claro para evitar ambiguidade).

## Critérios de verificação (para outra IA planejar os ajustes)

1. **Contrato:** [mcp-tool-contract.md](../mcp-tool-contract.md) documenta `verbosity/include_explanations/fields` onde suportado e define a regra de `deterministic_hash` quando esses knobs alteram a apresentação.
2. **Ferramentas:** `discover_capabilities` expõe suporte e valores aceitos, sem payload excessivo (preferir projeção).
3. **Tokens:** em `verbosity="minimal"`, `Content` não contém JSON completo; é um resumo curto. O JSON permanece em `StructuredContent`.
4. **Determinismo:** duas chamadas idênticas (mesmo dataset + mesmos knobs) produzem respostas equivalentes e hash conforme política definida.
5. **Leigo:** `help` contém exemplos de frases (“modo econômico”, “agora detalhado”) e como isso vira knobs.

## Alternativas consideradas

1. **Remover `StructuredContent` agora para economizar tokens** — rejeitado: quebra extensibilidade e automação futura; o problema é duplicação, não a existência do canal.
2. **Forçar `minimal` como default global** — rejeitado por risco de UX e quebra de expectativas; preferir defaults compatíveis e knobs explícitos.
3. **Apostar apenas em streaming HTTP** — rejeitado como solução principal: aumenta complexidade e não resolve eficiência no canal MCP stdio; paginação/projeção é mais simples e testável.

## Referências internas

- [mcp-tool-contract.md](../mcp-tool-contract.md)
- [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md)
- [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)
- [ADR 0009](0009-help-e-catalogo-de-templates-resources-v1.md)
- [docs/mcp-tools-melhorias-planejamento.md](../mcp-tools-melhorias-planejamento.md) (Anexo B — eficiência e UX)

