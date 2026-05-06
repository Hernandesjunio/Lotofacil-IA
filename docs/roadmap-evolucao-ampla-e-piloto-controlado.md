# Roadmap — evolução ampla e piloto controlado (Lotofacil-IA / MCP)

**Status:** proposta de governança técnica (para revisão)  
**Data:** 2026-05-03  
**Público-alvo:** revisores técnicos, mantenedores do repo e hosts MCP  

**Navegação:** [← Brief (índice)](brief.md) · [Checklist testers (piloto)](piloto-checklist-testers.md) · [Issue — relatório/discovery/fluidez](issues/issue-mcp-relatorio-completo-metricas-fluidez.md) · [Plano MCP tools](mcp-tools-melhorias-planejamento.md)

---

## Objetivo deste documento

Registrar, de forma **acionável e auditável**, como evoluir o sistema após um período de **piloto com testers**, sem confundir:

- **produto educacional + MCP técnico** (contratos, determinismo, superfície real por build), com
- **“produto final” comercial** (fora do escopo atual explícito no [brief.md](brief.md)).

Este roadmap não substitui normas: quando uma mudança alterar semântica/contrato/métricas, a fonte continua sendo `docs/mcp-tool-contract.md`, `docs/metric-catalog.md` e ADRs aplicáveis.

---

## Definições (para alinhar revisão técnica)

### Piloto controlado (recomendado agora)

Um piloto onde consumidores aceitam explicitamente limitações operacionais:

- dataset configurado e estável o suficiente para auditoria (`Dataset:DrawsSourceUri`, ADR 0022);
- hosts MCP maduros (suportam `StructuredContent`, não dependem só de texto livre);
- limites claros de uso (tamanho de janela, número de métricas por chamada, timeouts aceitáveis);
- expectativa de mudanças compatíveis com versionamento (`tool_version`, versões de métricas).

### “Melhoria ampla” (este documento)

Conjunto de frentes necessárias quando o uso sai do piloto e escala em:

- variedade de hosts (CLI, IDE, agent cloud),
- variedade de datasets (local, HTTP, snapshots),
- SLAs percebidos (latência/payload),
- necessidade de suporte e observabilidade.

---

## Princípios não negociáveis (baseline técnico)

1. **Semântica nas specs**: mudança de comportamento ⇒ atualização coordenada de docs + testes + código ([spec-driven-execution-guide.md](spec-driven-execution-guide.md)).
2. **Determinismo normativo** onde o contrato exige ([ADR 0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md)).
3. **Sem defaults ocultos no servidor** para parâmetros semânticos ([mcp-tool-contract.md](mcp-tool-contract.md)).
4. **Dataset obrigatório** sem fallback implícito quando ausente/ inválido ([ADR 0022](adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md)).
5. **`StructuredContent` como fonte de verdade** para decisões automáticas; `Content` é canal humano útil ([ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)).

---

## Mapa de trabalhos (macro-frentes)

### A — Contrato MCP + superfície por build (DX / agent fluency)

**Objetivo técnico:** reduzir tentativa-erro e drift entre “norma” e “instância”.

**Base já existente / em curso:**

- discovery estruturada e compatível com erros de rota (`UNKNOWN_METRIC` + detalhes), ver ADR 0006/0008 e a issue consolidada:
  - [issues/issue-mcp-relatorio-completo-metricas-fluidez.md](issues/issue-mcp-relatorio-completo-metricas-fluidez.md)

**Melhorias típicas (futuro):**

- “bundle auditável” só se virar **ADR + versão de tool** (a issue já alerta para não inventar preset mágico por omissão).

### B — UX canónica multi-canal: `verbosity`, `fields`, paginação

**Objetivo técnico:** cada nível cobre um papel diferente sem mudar semântica (“fluidez” não pode virar ambiguidade).

Referência: [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md).

**Problema comum em hosts:** exibir apenas `OK`/JSON opaco no chat.

- **Justificativa:** MCP separa canais; hosts variam no que imprimem.
- **Mitigação técnica:** documentar “modo recomendado por host” + manter utilidade mínima do `Content` em `standard/full` (já é tema de testes de contrato na linha ADR 0023).

### C — Dataset versionado + operações de ingestão

Referência: [ADR 0022](adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md).

**Riscos ao escalar:**

- mudança silenciosa de snapshot HTTP (por isso `dataset_version` deve refletir conteúdo efetivo);
- necessidade de limites de tamanho/tempo e falhas claras (`invalid_format`, `too_large`).

### D — Segurança e governança (mesmo projeto educacional)

**Problema:** `DrawsSourceUri` HTTP/HTTPS cria superfície clássica de risco operacional (rede, TLS, credenciais em querystring).

- **Justificativa técnica:** configurabilidade de URL é ótima para DX; para ambiente multiusuário precisa política explícita.
- **Sugestão técnica:** allowlist de hosts, timeouts, limites de payload, redação de segredos em logs, e modo “somente file://” para ambientes mais restritos.

### E — Observabilidade, custo e performance

**Problema:** MCP STDIO não “mostra” latência internamente; hosts percebem só tempo de ida/volta.

- **Justificativa:** produtividade real depende de diagnóstico quando uma tool demora ou explode payload.
- **Sugestão técnica:** logging estruturado mínimo com correlação (`tool_name`, `dataset_version`, duração, tamanho aproximado do payload canónico). Isso não muda semântica, mas muda operação.

### F — Qualidade contínua (gates)

Referências:

- [docs/test-plan.md](test-plan.md)
- [docs/contract-test-plan.md](contract-test-plan.md)

**Gate recomendado para qualquer release de piloto:**

1. `dotnet test` nas suítes do repo (pelo menos Domain + Infrastructure + ContractTests conforme maturidade do recorte).
2. smoke MCP mínimo: `help` → `discover_capabilities` → `get_draw_window` → `compute_window_metrics` com `verbosity` em 3 níveis (objetivos distintos).

---

## Plano de execução por fases (para implementação posterior)

> Observação: isto é um **roteiro de engenharia**. Detalhes normativos continuam nos documentos citados.

### Fase 0 — Baseline de piloto (agora)

**Pronto quando:**

- contrato e superfície por build são observáveis por máquina (`discover_capabilities`);
- dataset configurado é tratado como pré-requisito explícito (sem fallback);
- testes de contrato cobrem erros canônicos e regressões críticas.

**Saídas:**

- um “guia do piloto” curto para testers (pode viver como doc separado no futuro; hoje o núcleo está em `help`/resources e em [mcp-tool-contract.md](mcp-tool-contract.md)).
  - **Material já disponível para testers:** [piloto-checklist-testers.md](piloto-checklist-testers.md)


### Fase 1 — Fechar lacunas de discovery + relatório de janela (fluidez real)

**Âncora:** [issues/issue-mcp-relatorio-completo-metricas-fluidez.md](issues/issue-mcp-relatorio-completo-metricas-fluidez.md) e [mcp-tools-melhorias-planejamento.md](mcp-tools-melhorias-planejamento.md).

**Problema:** hosts precisam decidir automaticamente sem tentativa-erro.

- **Justificativa técnica:** catálogo normativo ≠ allowlist da build (já é invariante do projeto).
- **Sugestão técnica:** consolidar taxonomia única entre discovery e erros (`UNKNOWN_METRIC.details.reason`), mantendo registro único como fonte.

### Fase 2 — Endurecimento operacional (dataset HTTP + limites + auditoria)

**Âncora:** [ADR 0022](adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md).

**Problema:** piloto interno tolera manualidade; escala não tolera incidentes silenciosos.

- **Sugestão técnica:** política explícita de snapshot, limites e falhas com `details` úteis (sem vazar segredos).

### Fase 3 — Performance e payloads grandes (sem “inventar estatística no cliente”)

**Âncora:** [mcp-tools-melhorias-planejamento.md](mcp-tools-melhorias-planejamento.md) (knobs e representação).

**Problema:** métricas com séries/matrizes grandes degradam UX e aumentam custo de tokens.

- **Sugestão técnica:** `fields` + paginação em `verbosity=full` como primeiro escalonamento; summaries determinísticos só com ADR/contrato quando necessário.

### Fase 4 — Multi-transporte e matriz de suporte (se necessário)

**Problema:** paridade MCP STDIO vs MCP HTTP vs REST aumenta superfície de testes.

- **Sugestão técnica:** declarar oficialmente quais transportes são “suportados no piloto” e manter testes de paridade como gate apenas para o escopo suportado.

---

## Backlog transversal (priorização sugerida)

### P0 — correção/contrato/determinismo

- regressões em discovery, `allow_pending`, dataset ausente/ inválido;
- drift entre docs ↔ implementação (política do repo).

### P1 — DX e fluidez MCP

- verbosidade e projeção (`ADR 0023`);
- consistência `help` / resources / `discover_capabilities`.

### P2 — operação e escala

- observabilidade, limites, segurança de URL, performance em fixtures realistas.

---

## Critérios de saída do piloto (definição revisável)

Um piloto pode ser considerado “maduro o suficiente” para ampliar audiência quando:

1. **Incidência baixa** de erros `INVALID_REQUEST` por ambiguidade de janela (sinal de UX/doc suficiente).
2. **Hosts conseguem operar** usando `StructuredContent` (não só texto do `Content`).
3. **Dataset** tem política clara (origem, atualização, auditoria de versão).
4. **CI** impede regressões conhecidas (pelo menos contrato + domínio conforme recorte).

---

## Nota explícita sobre linguagem e produto (requisito do projeto)

Mesmo em materiais para testers, o sistema permanece **educacional** e **descritivo** sobre histórico e métricas; evitar promessa de resultado futuro, “vantagem” no sorteio ou linguagem preditiva — alinhado ao [brief.md](brief.md) e ao glossário.

---

## Referências cruzadas (leitura obrigatória antes de implementar mudanças grandes)

- [docs/mcp-tool-contract.md](mcp-tool-contract.md)
- [docs/metric-catalog.md](metric-catalog.md)
- [docs/test-plan.md](test-plan.md) / [docs/contract-test-plan.md](contract-test-plan.md)
- [ADR 0006 — fluidez/inter-tool/disponibilidade](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)
- [ADR 0008 — descoberta instância vs norma](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)
- [ADR 0014 — allow_pending](adrs/0014-semantica-real-de-allow-pending-v1.md)
- [ADR 0022 — dataset configurável](adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md)
- [ADR 0023 — verbosity/fields/paginação](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)
