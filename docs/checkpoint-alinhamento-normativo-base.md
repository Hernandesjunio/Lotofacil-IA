# Checkpoint de alinhamento normativo da base (sem feature)

Data: 2026-05-01  
Escopo: **checkpoint documental** (Fase 0), sem implementar funcionalidades.

## Fontes normativas (referências obrigatórias)

- `docs/spec-driven-execution-guide.md` (**Fase 0 — Congelar a base**)
- `docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md`
- `docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md`
- `docs/vertical-slice.md`
- `docs/mcp-tool-contract.md`
- `docs/contract-test-plan.md`

## Estado normativo vigente (o “deve ser” rastreável)

- **Processo**: spec-driven com “congelar a base” antes de codar features (Fase 0).  
  Critério explícito: “pronto quando **não há contradição estrutural crítica** para V0/V1”.
- **Arquitetura** (ADR 0004):
  - **4 projetos** principais (`Domain`, `Application`, `Infrastructure`, `Server`) e fronteiras de responsabilidade.
  - Servidor **sem IA embarcada** e com determinismo/contrato como obrigação, não detalhe.
- **Transporte e superfície pública** (ADR 0005):
  - MCP **real** é obrigatório (stdio e/ou HTTP MCP), com `tools/list` + `tools/call`.
  - HTTP REST existe como **espelho de compatibilidade** (`/tools/*`), não como “MCP via HTTP”.
- **Fatia vertical mínima V0** (`vertical-slice.md`):
  - Ponta-a-ponta “fixture → canônico → `frequencia_por_dezena@1.0.0` → tool”.
  - Superfície mínima: `get_draw_window` e `compute_window_metrics` (baseline).
- **Contrato** (`mcp-tool-contract.md` + `contract-test-plan.md`):
  - Sem defaults semânticos ocultos no servidor; lacunas são resolvidas no host/agente.
  - Invariantes globais: `dataset_version`, `tool_version`, `deterministic_hash`, janela explícita e erros estruturados.

## Evidências observadas na base (o “é hoje”)

### Conformidades claras

- **Estrutura em camadas (ADR 0004 D1/D4)**:
  - Existem `src/LotofacilMcp.Domain`, `src/LotofacilMcp.Application`, `src/LotofacilMcp.Infrastructure`, `src/LotofacilMcp.Server` (csproj presentes).
- **MCP real + paridade com HTTP espelhado (ADR 0005 D2/D3/D4)**:
  - `src/LotofacilMcp.Server/Program.cs` configura:
    - modo **stdio** (`--mcp-stdio`) e modo HTTP;
    - MCP HTTP streamable em `/mcp`;
    - endpoints REST em `/tools/*` (espelho).
  - `tests/LotofacilMcp.ContractTests/McpTransportParityIntegrationTests.cs` valida:
    - descoberta `tools/list` e chamadas `tools/call` (stdio e HTTP MCP),
    - **paridade semântica** entre MCP e HTTP espelhado por comparação de JSON,
    - paridade também para **erros de contrato**.
- **Janela por extremos (ADR 0008 D2, citada no contrato)**:
  - `src/LotofacilMcp.Server/Tools/V0McpTools.cs` aceita `start_contest_id` e `end_contest_id` como alternativa a `window_size+end`.

### Desvios não críticos (registrar, mas não bloqueiam o “congelamento”)

- **Projeto extra fora dos “4”**:
  - Existe `tools/McpMetricAudit/McpMetricAudit.csproj` além dos 4 projetos do ADR 0004.
  - Interpretação conservadora: não viola a intenção do ADR se for **ferramenta auxiliar** (não parte da arquitetura do servidor), mas é **desvio estrutural** que deve ser mantido explicitamente classificado como “tooling” (e não “camada”).
- **Resources/Prompts MCP implementados (ADR 0004 D9 + contrato)**:
  - O ADR 0004 trata Prompts/Resources como opcionais “quando houver caso real + teste”.
  - Há implementação e teste de descoberta/uso (`McpResourceDiscovery_IncludesPromptIndex_AndHelpReturnsIndex`), então fica registrado como **extensão justificada por evidência** (não contradição por si só).

## Contradições estruturais críticas (bloqueiam Fase 0 se mantidas sem reconciliação)

### C1 — `generation_mode` e “defaults conservadores” (runtime/testes vs contrato)

**O que a norma diz (contrato):**
- Em `mcp-tool-contract.md`, `generate_candidate_games` define `generation_mode` como **obrigatório** e proíbe aplicar guardrails/filtros não declarados (especialmente em `random_unrestricted`), reforçando “sem defaults ocultos no servidor”.

**O que a base faz hoje (evidência):**
- `tests/LotofacilMcp.ContractTests/Phase20GenerationMode0020ContractTests.cs` define e valida um comportamento **legado implícito** quando `GenerationMode: null`:
  - injeta filtros estruturais “conservadores” (ex.: `max_neighbor_count == 7`);
  - registra em `resolved_defaults` um modo `"legacy_implicit_structural_defaults"`.
- `src/LotofacilMcp.Server/Tools/V0McpTools.cs` descreve `generation_mode` como:  
  “`random_unrestricted | behavior_filtered` (**omitir = legado com defaults conservadores**)”.

**Por que é crítico:**
- Esta política contradiz o texto atual do contrato em **dois pontos**:
  - `generation_mode` deixa de ser estritamente obrigatório;
  - defaults semânticos (filtros) passam a ser aplicados quando o request omite o modo, o que conflita com “sem defaults ocultos”.

**Situação do checkpoint:** contradição registrada; **exige reconciliação** (docs+contrato vs runtime+testes) antes de declarar a base “congelada” no sentido estrito da Fase 0.

## Resultado do checkpoint (Critério de pronto)

- **Estado normativo vigente está explícito e rastreável**: sim (fontes listadas e mapeadas acima).
- **Contradições estruturais críticas apontadas**: sim (C1).
- **Sem implementar funcionalidades**: confirmado (apenas documentação adicionada).

