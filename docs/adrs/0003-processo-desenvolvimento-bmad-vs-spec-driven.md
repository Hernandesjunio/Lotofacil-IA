# ADR 0003 — Processo de desenvolvimento: spec-driven como padrão; BMAD opcional

**Navegação:** [← Brief (índice)](../brief.md) · [README](../../README.md)

## Status

Aceito — orientação de processo para o repositório.

## Contexto

O projeto possui especificação densa e versionada: [`metric-catalog.md`](../metric-catalog.md), [`mcp-tool-contract.md`](../mcp-tool-contract.md), [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md) e [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md), [`generation-strategies.md`](../generation-strategies.md), [`test-plan.md`](../test-plan.md). Surge a dúvida se frameworks de orquestração de agentes (ex.: BMad Method) devem ser adotados como processo obrigatório ou se o fluxo spec-first + issues/PRs é suficiente.

## Decisão

1. **Fonte de verdade semântica** permanece a documentação versionada no repositório e os testes que a materializam — **modelo spec-driven / contrato primeiro**.
2. **BMad Method** (ou equivalentes de workflows multi-agente) é **opcional**: pode ser usado por quem deseja roteiro de PRD, épicos e revisões facilitadas, mas **não substitui** ADRs, catálogo de métricas nem o contrato MCP.
3. Trabalho de implementação é fatiado conforme [`vertical-slice.md`](../vertical-slice.md) e [`contract-test-plan.md`](../contract-test-plan.md), com critérios de aceite testáveis.

## O que é cada abordagem (neste repositório)

| | Spec-driven (padrão aqui) | BMAD / workflows de agentes (opcional) |
|--|---------------------------|----------------------------------------|
| **Objeto** | Definições de domínio, APIs, comportamento verificável | Como organizar sessões de análise, planejamento e implementação com IA |
| **Artefatos centrais** | `metric-catalog`, `mcp-tool-contract`, ADRs, goldens | Comandos, agentes e fases definidos pelo framework escolhido |
| **Vínculo com código** | Direto (testes de contrato, golden files) | Indireto (gera texto/épicos que devem ser alinhados ao repo) |

## Prós e contras registrados

### Spec-driven (adotado como backbone)

- **Prós:** alinhamento com determinismo e auditoria; regressão clara quando o spec muda; sem dependência de ferramenta externa para a verdade semântica.
- **Contras:** exige disciplina de manter docs e testes sincronizados; não ensina por si só *como* fatiar trabalho em sprints.

### BMAD / processos multi-agente (opcional)

- **Prós:** estrutura para times que querem passos explícitos (brief → PRD → arquitetura → stories); pode acelerar descoberta quando o domínio ainda é nebuloso.
- **Contras:** overhead operacional; risco de duplicar ou divergir dos ADRs já existentes; não prova correção matemática das métricas.

## Consequências

- Issues e PRs devem referenciar métricas e tools pelos **nomes versionados** do catálogo e do contrato.
- Se alguém usar BMAD para gerar PRD ou épicos, o output deve ser **reconciliado** com a [documentação versionada](../brief.md) antes de virar escopo implementável.
- **Política normativa — backlog externo além do Git:** a adoção de ferramenta externa de backlog não ocorre por percepção subjetiva; ocorre quando houver evidência operacional observável de que o fluxo atual (issues e PRs no Git) deixou de ser suficiente.
  - **Gatilhos objetivos:** `(devs_ativos ≥ 3 e PRs_por_semana ≥ 10)` **ou** lead time mediano acima de um **limite numericamente definido pelo projeto** (o valor do limiar não consta deste ADR até ser fixado em política operacional explícita).
  - **Justificativa:** evita burocracia prematura e ancora a decisão em volume e fluxo mensuráveis, mantendo documentação versionada e testes como fonte de verdade semântica.

## Referências internas

- [`../vertical-slice.md`](../vertical-slice.md)
- [`../contract-test-plan.md`](../contract-test-plan.md)
- [`../brief.md`](../brief.md) (princípios e restrições)
