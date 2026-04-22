# ADR 0005 — Transporte MCP, convivência com HTTP e rollout do catálogo de tools (V1)

**Navegação:** [← Brief (índice)](../brief.md) · [ADR 0004](0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

## Status

Aceito — altera a decisão de superfície **D2** do [ADR 0004](0004-estrutura-arquitetural-inicial-mcp-dotnet10.md), que previa apenas HTTP na entrega inicial.

## Contexto

O [ADR 0004](0004-estrutura-arquitetural-inicial-mcp-dotnet10.md) fixou quatro projetos e um host fino em `LotofacilMcp.Server`, com **HTTP-only** na V0 para reduzir duplicação de bootstrap antes de existir semântica canônica validada.

Com a V0 fechada por testes, o [brief.md](../brief.md) e o [mcp-tool-contract.md](../mcp-tool-contract.md) exigem **consumo por agentes via protocolo MCP** (descoberta de tools, invocação no formato do protocolo), não apenas JSON por POST. Permanece desejável **compatibilidade** com clientes que já integram via HTTP e com a suíte de testes de contrato existente.

A “escolha definitiva do SDK MCP” estava explicitamente **fora** do escopo fechado do ADR 0004; este ADR a fixa.

## Decisão

### D1 — SDK oficial MCP para C# / .NET

Usar o **C# SDK oficial** do Model Context Protocol (repositório `modelcontextprotocol/csharp-sdk`, pacotes NuGet da família **`ModelContextProtocol`**, em colaboração com Microsoft), como ponto de integração para servidor MCP.

**Justificativa:** manutenção alinhada à spec, suporte a transportes (`stdio`, integração ASP.NET Core quando aplicável) e evolução do protocolo sem reinventar JSON-RPC/sequência de mensagens.

**Nota:** versões exatas de pacote ficam no `csproj`; este ADR não fixa número de versão, apenas a **linha** de pacotes oficial.

### D2 — Protocolo MCP como superfície obrigatória para integração de agentes

O produto deve expor as ferramentas documentadas em [mcp-tool-contract.md](../mcp-tool-contract.md) como **tools MCP** (descoberta via fluxo do protocolo e invocação com argumentos JSON alinhados ao contrato).

**Justificativa:** hosts (IDE, clientes MCP) esperam MCP; o contrato já descreve payloads de tools — o transporte MCP é a cola operacional.

### D3 — Convivência com HTTP (compatibilidade e testes)

Manter **endpoints HTTP** que espelham as mesmas tools (incluindo prefixos já usados, ex.: `/tools/...`, `/mcp/tools/...`), com **a mesma semântica de request/response** que a invocação MCP, salvo o envelope do protocolo MCP nas mensagens de camada inferior.

**Justificativa:** regressão por testes de contrato já existentes, clientes legados, depuração com `curl`/REST sem host MCP.

### D4 — Transportes MCP suportados (ordem de prioridade)

1. **stdio** — processo filho padrão para muitos hosts desktop (ex.: subprocesso com JSON-RPC MCP sobre stdin/stdout).
2. **HTTP (ASP.NET Core)** — quando o SDK expuser integração com o mesmo pipeline Kestrel, permitindo um único `LotofacilMcp.Server` com HTTP “legado” + rota MCP streamable/SSE conforme documentação do pacote `ModelContextProtocol.AspNetCore` (ou equivalente na versão do SDK).

A equipe pode implementar **primeiro** o transporte que desbloquear o caso de uso principal (em geral **stdio** para Cursor/Claude Desktop), desde que o contrato de tools e os testes cubram esse caminho; o segundo transporte entra como fatia seguinte.

**Justificativa:** ADR 0004 evitou dois hosts sem necessidade; aqui o segundo transporte é **o mesmo protocolo** com outro binding, não duas semânticas.

### D5 — Fronteira de dependências (inalterada em espírito)

Tipos e atributos específicos do **SDK MCP** ficam confinados a **`LotofacilMcp.Server`** (e, se existir, um projeto executável mínimo que só faz bootstrap stdio apontando para os mesmos registros de DI). **`Domain`** e **`Application`** não referenciam pacotes MCP.

**Justificativa:** preserva D4 do ADR 0004; o núcleo permanece agnóstico de transporte.

### D6 — Catálogo de tools: implementação em ondas

As ferramentas nomeadas em [mcp-tool-contract.md](../mcp-tool-contract.md) (seção *Ferramentas propostas*) devem ser implementadas de forma **incremental**, cada uma com testes de contrato antes da codificação de superfície:

| Onda | Tools | Notas |
|------|--------|--------|
| A (V0 — feito) | `get_draw_window`, `compute_window_metrics` | Baseline + métricas conforme fatia vertical. |
| B | `analyze_indicator_stability`, `compose_indicator_analysis`, `analyze_indicator_associations`, `summarize_window_patterns` | Dependem de [metric-catalog.md](../metric-catalog.md) e [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md). |
| C | `generate_candidate_games`, `explain_candidate_games` | Dependem de [generation-strategies.md](../generation-strategies.md), determinismo com `seed` e invariantes de candidato. |

Cada onda deve expor as novas tools **tanto** em MCP **quanto** nos endpoints HTTP espelhados, salvo decisão documentada de “tool só MCP” (evitar divergência sem motivo).

### D7 — Primitivas opcionais MCP (Prompts / Resources)

Permanecem **opcionais** e só entram com teste e caso de uso real, em linha com **D9** do ADR 0004 — agora explicitamente compatível com a spec MCP, mas sem obrigatoriedade para fechar V1 transporte + tools.

## Consequências

### Positivas

- Integração nativa com ecossistema MCP e com o texto normativo do repositório.
- HTTP mantido para testes e transição suave.
- Rollout do catálogo em ondas evita PR monolítica.

### Custos

- Manutenção de **dois bindings** (MCP + HTTP) até eventual consolidação ou descontinuação documentada de um deles.
- Testes de integração MCP além dos testes HTTP já existentes.

## O que fica fora desta decisão

- Política de autenticação e rate limit (continua em toggles, como no ADR 0004).
- Conteúdo pedagógico em Resources/Prompts antes de haver necessidade.
- Detalhe de cada schema de tool (fonte de verdade: [mcp-tool-contract.md](../mcp-tool-contract.md)).

## Critérios de verificação

Esta decisão está implementada quando:

1. Pelo menos um **transporte MCP** (stdio ou HTTP MCP) passa testes de integração que cobrem `tools/list` e `tools/call` para as tools já em escopo.
2. As chamadas MCP produzem **paridade semântica** com os endpoints HTTP para o mesmo JSON de argumentos e respostas de sucesso/erro de contrato.
3. As ondas B e C seguem o [spec-driven-execution-guide.md](../spec-driven-execution-guide.md) (fases 10+) com testes de contrato por tool.

## Referências internas

- [ADR 0004](0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
- [spec-driven-execution-guide.md](../spec-driven-execution-guide.md)
- [vertical-slice.md](../vertical-slice.md)
