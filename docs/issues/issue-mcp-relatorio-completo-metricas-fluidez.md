# Issue — Relatório completo de métricas de janela, descoberta e fluidez MCP

**Estado:** aberto (backlog documental + técnico)  
**Data:** 2026-05-02  
**Tipo:** melhoria de produto/DX — sem alteração de semântica de métricas até especificação explícita

**Navegação:** [← Brief (índice)](../brief.md) · [Contrato MCP](../mcp-tool-contract.md) · [Catálogo de métricas](../metric-catalog.md) · [Plano MCP tools](../mcp-tools-melhorias-planejamento.md)

---

## Resumo do problema

Consumidores (agentes, IDE, scripts) precisam:

1. Saber **sem tentativa-erro** quais métricas a instância aceita em `compute_window_metrics` **nesta build**, quais exigem `allow_pending`, e quais nomes do catálogo normativo **não** estão nesta rota (ex.: `scope = candidate_game`).
2. Obter, quando aplicável, um **único relatório** de todas as métricas de **janela** suportadas, com payload **auditável** (`dataset_version`, `tool_version`, `deterministic_hash`) e sem ambiguidade entre resumo humano (`Content`) e fonte canônica estruturada (`StructuredContent`).
3. Operar com dataset configurado (`Dataset__DrawsSourceUri`) de forma documentada e previsível, sem inferência implícita de fallback, fixture escondida ou auto-descoberta fora do contrato.

Este documento **não** redefine fórmulas de métricas; referencia o [metric-catalog.md](../metric-catalog.md) como norma.

### Fecho anti-ambiguidade desta issue

Para evitar margem de interpretação livre durante a implementação:

- **`StructuredContent` é a fonte de verdade**; `Content` é apenas resumo humano útil, conforme [ADR 0023](../adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md). Esta issue **não** autoriza usar `Content` como canal exclusivo de discovery técnica.
- **Esta issue não fecha sozinha um “preset mágico”** para “todas as métricas”. Enquanto a Fase B não virar contrato explícito, o caminho seguro continua sendo enviar `metrics[]` de forma declarada.
- **Dataset é pressuposto operacional externo ao MCP**: o servidor presume a fonte configurada; a entrega aqui é clareza documental + erro canônico quando a fonte estiver ausente/inválida, não sincronização automática.
- **Categorias de disponibilidade devem ser observáveis por máquina**, não apenas por texto livre em `Content` ou docs narrativas.

---

## Fundamento: norma (47) vs instância (rota)

| Camada | Fonte | Papel |
|--------|--------|--------|
| **Norma semântica** | [metric-catalog.md](../metric-catalog.md) Tabela 1 | Nomes, shapes, versões, fórmulas. |
| **Instância / build** | Registo em código (`MetricAvailabilityCatalog`) + `discover_capabilities` | O que **esta build** expõe por rota; erros com `allowed_metrics` quando aplicável. |

O contrato já estabelece que catálogo ≠ lista aceite na rota — ver secção *Catálogo vs `compute_window_metrics`* em [mcp-tool-contract.md](../mcp-tool-contract.md).

---

## Relação com ADRs existentes (não duplicar decisões)

| ADR | Ligação a esta issue |
|-----|----------------------|
| [ADR 0011](../adrs/0011-tool-de-discovery-de-capacidades-por-build-v1.md) | `discover_capabilities` deve publicar allowlists reais; esta issue pede **fecho de lacunas** entre spec ADR e payload observável. |
| [ADR 0012](../adrs/0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md) | Registo único já normatizado; esta issue pede **uso consistente** nas respostas de erro e na discovery. |
| [ADR 0014](../adrs/0014-semantica-real-de-allow-pending-v1.md) | Comportamento de `allow_pending` e métricas `pending`. |
| [ADR 0023](../adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md) | `verbosity`, `fields`, paginação e utilidade do `Content`. |
| [ADR 0006](../adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) | Disponibilidade por rota, pipeline, `UNKNOWN_METRIC` com `details`. |
| [ADR 0008](../adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) | Descoberta instância vs norma. |

**Plano relacionado (anti-ambiguidade payload):** [mcp-tools-melhorias-planejamento.md](../mcp-tools-melhorias-planejamento.md) (matrizes grandes, `matriz_numero_slot`, etc.).

---

## Será necessário um ADR novo?

| Situação | ADR? |
|----------|------|
| Implementar esta issue **como backlog** (issues, testes, docs de execução) | **Não obrigatório** — adere aos ADRs já citados. |
| Introduzir **decisão arquitetural nova** (ex.: campo `metrics_preset` ou `candidate_numbers` em `compute_window_metrics`, novo contrato de bundle fechado) | **Sim** — novo ADR ou revisão major do contrato + versão de tool, para não violar [ADR 0001](../adrs/0001-fechamento-semantico-e-determinismo-v1.md) / stateless explícito. |

Regra prática: **issue + PR** bastam para execução incremental alinhada aos ADRs existentes; **ADR novo** quando houver **contrato ou semântica de transporte** nova que os ADRs atuais não cubram de forma fechada.

---

## Plano de trabalho (fases)

### Fase A — Descoberta sem tentativa-erro

- Estender `discover_capabilities` (conforme ADR 0011) para distinguir, de forma **estruturada e determinística**, pelo menos quatro classes observáveis para nomes do catálogo:  
  1. **aceite agora** em `compute_window_metrics` com `allow_pending=false`;  
  2. **aceite apenas com opt-in** (`allow_pending=true`);  
  3. **conhecida, mas não exposta nesta rota/build**;  
  4. **fora do escopo desta rota por desenho** (ex.: requer candidato, requer outra tool, requer outro tipo de janela).  
- A issue **não fixa** os nomes finais dos campos JSON dessa classificação; a implementação deve fechá-los antes em contrato/testes para não gerar payload “interpretável por contexto”.
- Testes de contrato: payload de discovery **determinístico** para a mesma build.

**Alterações técnicas prováveis no código**

- `src/LotofacilMcp.Server/Tools/V0Tools.cs`
  - ampliar `MetricsCapabilitiesEnvelope` e/ou `DiscoverCapabilitiesResponse` para publicar classificação por rota de forma legível por máquina;
  - ajustar o método `DiscoverCapabilities(...)` para derivar a nova estrutura a partir do registro único, sem listas montadas manualmente.
- `src/LotofacilMcp.Application/Validation/MetricAvailabilityCatalog.cs`
  - hoje já contém os sinais brutos (`Scope`, `Status`, `Implemented`, `ComputeWindowMetrics`, etc.);
  - a mudança provável é adicionar helper(s) orientados a discovery, por exemplo uma projeção/classificação por métrica para não espalhar lógica ad hoc em `V0Tools.cs`.
- `src/LotofacilMcp.Application/Validation/V0CrossFieldValidator.cs`
  - pode permanecer com a mesma validação central;
  - opcionalmente, alinhar `details` dos erros para reutilizar a mesma taxonomia de descoberta (evitando drift entre discovery e erro em runtime).
- `tests/LotofacilMcp.ContractTests/*`
  - novos asserts para a estrutura de discovery;
  - testes de determinismo e cobertura das classes “allowed / pending opt-in / fora da rota / fora do escopo”.

**De/Para técnico provável**

| Aspecto | Hoje | Depois |
|---------|------|--------|
| Discovery de métricas | `implemented_metric_names`, `pending_metric_names`, `compute_window_metrics_allowed` | Classificação explícita por métrica/rota ou listas equivalentes por classe |
| Fonte da classificação | lógica espalhada entre arrays e filtros em `V0Tools.cs` | projeção determinística derivada do `MetricAvailabilityCatalog` |
| Erro para métrica conhecida fora da rota | `UNKNOWN_METRIC` + `allowed_metrics` | mesmo erro, mas com semântica documental espelhada na discovery |

**Avaliação sincera**

Esta fase **faz muito sentido** e é a de **maior retorno**. O código já tem quase toda a matéria-prima; falta sobretudo fechar a forma de exposição. É uma fase relativamente barata e reduz muito a margem para alucinação do host/agente.

### Fase B — Um pedido = relatório de janela (opcional normativo)

- Se o produto quiser suportar “um pedido = relatório completo”, documentar no [mcp-tool-contract.md](../mcp-tool-contract.md) **um único mecanismo explícito e versionado** para pedir todas as métricas de janela da build.  
- Até esse fecho, permanece vedado assumir por convenção que `metrics=null`, `metrics=[]` ou qualquer outro atalho signifique “todas as métricas”.
- O mecanismo escolhido **não** pode incluir métricas `candidate_game` por omissão nem depender de inferência textual do host.
- Garantir `fields` / paginação para payloads grandes ([ADR 0023](../adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)).

**Alterações técnicas prováveis no código**

- `src/LotofacilMcp.Server/Tools/V0McpTools.cs`
  - eventual adição de novo parâmetro no schema MCP (`metrics_preset`, por exemplo) **se** essa for a escolha contratual;
  - manter compatibilidade explícita com `metrics[]`.
- `src/LotofacilMcp.Server/Tools/V0Tools.cs`
  - ampliar `ComputeWindowMetricsRequest` para suportar o mecanismo escolhido;
  - materializar o request efetivo no hash canônico e, se necessário, refletir a nova capacidade em `discover_capabilities`.
- `src/LotofacilMcp.Application/Validation/V0CrossFieldValidator.cs`
  - impedir combinações ambíguas (`metrics[]` + preset conflitando, preset inválido, preset que inclua métrica fora da rota).
- `src/LotofacilMcp.Application/UseCases/ComputeWindowMetricsUseCase.cs`
  - continuar calculando a lista resolvida de métricas, mas agora a lista pode vir expandida a partir de um preset versionado em vez de vir só do request literal.
- `tests/LotofacilMcp.ContractTests/*`
  - goldens multi-métrica;
  - validação de paginação/projeção em `verbosity=full` para payloads extensos.

**De/Para técnico provável**

| Aspecto | Hoje | Depois |
|---------|------|--------|
| Seleção de métricas | apenas `metrics[]` explícito | `metrics[]` explícito **ou** mecanismo único/fechado para “relatório completo” |
| Semântica de `metrics=null` / `[]` | não deve significar “todas” | continua proibido, salvo decisão contratual explícita diferente |
| `compute_window_metrics` | request estritamente declarativo | request declarativo + expansão controlada/versionada |

**Avaliação sincera**

Esta fase **faz sentido**, mas só se houver um caso real de consumo que precise de **one-shot report**. Caso contrário, ela tende a aumentar o contrato e o custo cognitivo. Minha avaliação: é útil, mas deve vir **depois** da Fase A e com um mecanismo **único**; se abrir muitas opções, piora a UX em vez de melhorar.

### Fase C — Métricas sobre jogo candidato (lacuna normativa vs rota atual)

- Decisão de produto: estender request com `candidate_numbers` **ou** criar nova tool dedicada.  
- Esta issue **não** autoriza “encaixar” métricas de candidato em `compute_window_metrics` por analogia. Só atualizar contrato + catálogo quando a superfície final estiver fechada.

**Alterações técnicas prováveis no código**

- Caminho 1: **estender `compute_window_metrics`**
  - exigiria mudar `ComputeWindowMetricsRequest`, `ComputeWindowMetricsInput`, validação cruzada e provavelmente o `WindowMetricDispatcher` ou camada adjacente para aceitar contexto de candidato;
  - aumentaria o acoplamento entre métricas de janela e métricas de `candidate_game`.
- Caminho 2: **nova tool dedicada**
  - adicionaria nova request/response em `src/LotofacilMcp.Server/Tools/V0Tools.cs`;
  - criaria novo use case em `src/LotofacilMcp.Application/UseCases/`;
  - reutilizaria `MetricAvailabilityCatalog` com filtro por `Scope = candidate_game`, mas sem poluir a rota de janela.

**De/Para técnico provável**

| Aspecto | Hoje | Depois |
|---------|------|--------|
| Métricas de candidato | só conhecidas no catálogo/estratégias, não expostas por rota dedicada aqui | ou entram numa tool própria, ou exigem contrato novo em `compute_window_metrics` |
| Separação de escopos | `window` e `candidate_game` separados no catálogo, mas discovery ainda pouco explícita | separação operacional também no transporte |

**Avaliação sincera**

Esta fase **não deveria andar junto** com as anteriores no mesmo PR lógico. Ela é válida como problema, mas é outra frente de produto. Minha avaliação honesta: manter na mesma issue é aceitável como “lacuna relacionada”, porém para execução real convém **desmembrar** ou, no mínimo, marcar como dependente de ADR/decisão de produto.

### Fase D — Experiência agente / host

- Canal estruturado sempre serializável no cliente; `verbosity=full` deve significar **completude do payload canônico**, não mudança de semântica nem licença para mover detalhes essenciais apenas para `Content`.
- O `Content` em `standard/full` deve resumir o resultado principal sem truncamento enganoso; detalhes extensos permanecem em `StructuredContent` e podem ser controlados por `fields` / paginação.

**Alterações técnicas prováveis no código**

- `src/LotofacilMcp.Server/Tools/V0Tools.cs`
  - revisar builders de `Content` para `discover_capabilities` e `compute_window_metrics` (ou helpers equivalentes) garantindo que o fato principal continue legível em `standard/full`;
  - garantir que `fields` e paginação incidam sobre `StructuredContent`, não sobre heurística textual.
- `src/LotofacilMcp.Server/Tools/V0McpTools.cs`
  - em geral, sem mudança estrutural grande; o impacto aqui é mais de schema/documentação das tools.
- `tests/LotofacilMcp.ContractTests/*`
  - adicionar asserts de utilidade mínima do `Content` e de não duplicação do JSON completo.

**De/Para técnico provável**

| Aspecto | Hoje | Depois |
|---------|------|--------|
| `verbosity=full` | já tende a significar mais detalhe, mas a leitura pode variar por tool | semântica fechada: completude do payload canônico + resumo humano suficiente |
| `Content` | resumo útil, porém suscetível a drift entre tools | regra testada: nunca omite o fato principal nem vira cópia do JSON |

**Avaliação sincera**

Esta fase **faz sentido**, mas é mais uma fase de **hardening** do que de feature nova. O ganho é real para hosts/agentes, porém depende muito de bons testes de contrato. Eu a colocaria como consequência natural de A/B, não como eixo isolado de produto.

### Fase E — Qualidade

- Fixture golden **multi-métrica** (uma chamada, N métricas) em `tests/fixtures/golden/`.
- Teste `allow_pending` false vs true para métricas `pending`.
- Teste documental/contratual de dataset: sem `Dataset__DrawsSourceUri` válido, a superfície deve falhar com erro canônico/documentado, sem fallback implícito.

**Alterações técnicas prováveis no código**

- `tests/fixtures/golden/`
  - incluir payload multi-métrica suficientemente variado para provar ordenação, hashing e paginação/projeção quando aplicável.
- `tests/LotofacilMcp.ContractTests/*`
  - adicionar cenários para `allow_pending`;
  - adicionar cenário de dataset ausente/inválido;
  - cobrir `discover_capabilities` e `compute_window_metrics` em conjunto.
- `src/`
  - idealmente pouca ou nenhuma lógica nova aqui; se os testes exigirem mudanças, isso é sinal de drift ou comportamento ainda não fechado nas fases anteriores.

**De/Para técnico provável**

| Aspecto | Hoje | Depois |
|---------|------|--------|
| Evidência de regressão | parcial e dispersa | cenários fechados por golden e por matriz de erro |
| Dataset ausente/inválido | comportamento existe, mas pode não estar explicitamente travado por teste nesta issue | comportamento documentado e coberto por contrato |

**Avaliação sincera**

Esta fase é **obrigatória** se qualquer uma das anteriores for implementada. Sozinha não resolve o problema, mas sem ela as fases A-D ficam sujeitas a drift.

---

## Avaliação técnica sincera da proposta como um todo

### O que faz sentido

- A direção geral da issue é boa: ela ataca um problema real de **descoberta operacional** e de **fluidez** para consumidores MCP.
- O melhor núcleo da proposta é: **descobrir a superfície real da build sem trial-and-error** e **não misturar contrato canônico com texto humano**.
- A issue está correta em separar **norma** (catálogo) de **instância** (rota/build).

### O que estava mais fraco antes dos ajustes

- Havia risco de parecer que tudo seria resolvido “só com discovery”, quando parte do problema é de **contrato** e parte é de **UX do host**.
- A Fase B podia induzir implementador a inventar um atalho implícito para “todas as métricas”.
- A Fase C tem escopo grande demais para andar misturada com discovery/fluidez.

### Minha recomendação honesta de execução

1. Fazer **Fase A + Fase E** primeiro.
2. Só depois decidir se **Fase B** merece contrato novo.
3. Tratar **Fase C** como issue separada ou sub-backlog dependente de ADR.
4. Considerar **Fase D** como regra transversal de acabamento e teste, não como “feature principal”.

### Julgamento final

**Sim, a proposta faz sentido.**  
Mas ela fica **muito melhor** se for lida como:

- **núcleo executável:** Fase A + Fase E;
- **extensão opcional, porém valiosa:** Fase B;
- **tema relacionado, mas separado:** Fase C;
- **disciplina transversal de UX/contrato:** Fase D.

## Critérios de aceite

- Em ≤ 2 chamadas (`discover_capabilities` + `compute_window_metrics`), um consumidor consegue listar o que é válido e produzir relatório de janela sem ler `MetricAvailabilityCatalog.cs`.
- Nenhuma métrica do catálogo aparece “omitida sem classe”: ou está na rota, ou aparece com classificação estruturada equivalente a **requer candidato**, **requer outra tool/tipo de request**, **opt-in via `allow_pending`**, ou **não implementada / não exposta nesta build**.
- `Content` nunca é a única fonte da discovery técnica; qualquer dado necessário para decisão automática do cliente também está presente em `StructuredContent`.
- A documentação operacional deixa explícito que `Dataset__DrawsSourceUri` é requisito externo do servidor e que esta issue não adiciona mecanismo de atualização automática da fonte.
- Regressões de determinismo cobertas por testes referenciados no [test-plan.md](../test-plan.md) quando o contrato mudar.

---

## Documentação a atualizar **quando** houver implementação

| Documento | Momento |
|-----------|---------|
| [mcp-tool-contract.md](../mcp-tool-contract.md) | Sempre que JSON da tool ou códigos de erro mudarem. |
| [contract-test-plan.md](../contract-test-plan.md) | Novos fixtures/cenários. |
| [test-plan.md](../test-plan.md) | Nova matriz de testes. |
| [metric-catalog.md](../metric-catalog.md) | Apenas se nome/versão/shape **normativo** mudar. |
| [metric-glossary.md](../metric-glossary.md) | Apenas se linguagem pedagógica precisar alinhar a nova superfície. |
| [spec-driven-execution-guide.md](../spec-driven-execution-guide.md) | Novo passo/fase atómico, se o fluxo de entrega mudar. |
| [brief.md](../brief.md) | Só se o índice ou objetivos de produto incorporarem requisito novo explícito. |

**Até lá:** esta issue + referências no [brief.md](../brief.md) mantêm coesão sem antecipar semântica.

---

## Referências

- [mcp-tools-melhorias-planejamento.md](../mcp-tools-melhorias-planejamento.md)
- `src/LotofacilMcp.Application/Validation/MetricAvailabilityCatalog.cs` — allowlists por rota (instância)
- `src/LotofacilMcp.Domain/Metrics/WindowMetricDispatcher.cs` — despacho efetivo de métricas de janela
