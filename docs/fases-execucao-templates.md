# Templates atômicos de execução (compacto)

**Navegação:** [← Brief (índice)](brief.md) · [spec-driven-execution-guide.md](spec-driven-execution-guide.md) · **Arquivo arquivado:** `docs/archived/fases-execucao-templates.md`

Este documento fornece **um template por fase** do `docs/spec-driven-execution-guide.md`, pronto para copiar/colar em um chat/agent.

**Regra de ouro:** implemente **apenas** o recorte descrito na fase. Se faltar parâmetro, **pergunte** (não infira).

---

## Fase 0 — Congelar a base

```md
Implemente apenas um checkpoint de alinhamento normativo da base (sem codar feature), registrando conformidade e contradições.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 0)
- docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md
- docs/vertical-slice.md
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md

Arquivos esperados:
- docs/ (nota curta de alinhamento, se necessário)

Regras:
- não implementar funcionalidades nesta fase
- apontar contradições estruturais críticas, se existirem

Critério de pronto:
- estado normativo vigente está explícito e rastreável
```

## Fase 1 — Preparar o esqueleto mínimo do repositório

```md
Implemente apenas o esqueleto mínimo do repositório para compilar e testar a V0, sem cenografia adicional.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 1)
- docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md
- docs/project-guide.md

Arquivos esperados:
- LotofacilMcp.sln
- Directory.Build.props
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Infrastructure/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.Domain.Tests/
- tests/LotofacilMcp.ContractTests/
- tests/fixtures/

Regras:
- manter fronteiras e nomes conforme ADR 0004
- não criar projetos de integração/E2E antes da necessidade

Critério de pronto:
- solution compila vazia e referências entre projetos respeitam fronteiras
```

## Fase 2 — Preparar fixture mínima e testes vermelhos da V0

```md
Implemente apenas a fixture mínima e os testes vermelhos da V0 (domínio + contrato) antes de qualquer implementação funcional.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 2)
- docs/vertical-slice.md
- docs/contract-test-plan.md
- docs/test-plan.md
- docs/mcp-tool-contract.md
- docs/metric-catalog.md

Arquivos esperados:
- tests/fixtures/synthetic_min_window.json
- tests/LotofacilMcp.Domain.Tests/
- tests/LotofacilMcp.ContractTests/

Regras:
- testes devem falhar pelo motivo correto (TDD)
- cobrir: normalização, janela, fórmula base, propriedade `15 × window_size`, erros mínimos de contrato

Critério de pronto:
- existe ao menos um teste explícito para normalização e para os negativos de contrato mínimos
```

## Fase 3 — Materializar o núcleo canônico mínimo

```md
Implemente apenas o núcleo de domínio mínimo da V0: modelos canônicos, normalização, janela e `frequencia_por_dezena@1.0.0`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 3)
- docs/metric-catalog.md
- docs/mcp-tool-contract.md
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md

Arquivos esperados:
- src/LotofacilMcp.Domain/Models/
- src/LotofacilMcp.Domain/Normalization/
- src/LotofacilMcp.Domain/Windows/
- src/LotofacilMcp.Domain/Metrics/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não colocar lógica de transporte/contrato no Domain

Critério de pronto:
- testes de domínio passam sem infraestrutura/HTTP
```

## Fase 4 — Materializar infraestrutura determinística mínima

```md
Implemente apenas a infraestrutura determinística mínima: provider de snapshot, `dataset_version`, JSON canônico e SHA-256 para `deterministic_hash`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 4)
- docs/mcp-tool-contract.md
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Arquivos esperados:
- src/LotofacilMcp.Infrastructure/Providers/
- src/LotofacilMcp.Infrastructure/DatasetVersioning/
- src/LotofacilMcp.Infrastructure/CanonicalJson/
- src/LotofacilMcp.Infrastructure/Hashing/
- tests/LotofacilMcp.Infrastructure.Tests/

Regras:
- `DATASET_UNAVAILABLE` deve ter `details` úteis ao host (sem segredos)

Critério de pronto:
- dataset_version e deterministic_hash são estáveis em testes
```

## Fase 5 — Materializar casos de uso da V0

```md
Implemente apenas os casos de uso da V0 e validações cross-field em `Application`, sem lógica de cálculo no `Server`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 5)
- docs/vertical-slice.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Application/UseCases/
- src/LotofacilMcp.Application/Validation/

Regras:
- validação deve produzir erros de contrato coerentes (code/details)

Critério de pronto:
- use cases retornam envelopes com janela + versões + hash conforme contrato
```

## Fase 6 — Preparar testes de contrato da V0

```md
Implemente apenas os testes de contrato mínimos da V0 (envelope, `MetricValue`, `UNKNOWN_METRIC` e `INVALID_REQUEST`).

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 6)
- docs/mcp-tool-contract.md
- docs/vertical-slice.md

Arquivos esperados:
- tests/LotofacilMcp.ContractTests/

Regras:
- testar presença de `dataset_version`, `tool_version`, `deterministic_hash` e `window`

Critério de pronto:
- testes de contrato passam com a implementação V0 e falham quando quebrar o envelope
```

## Fase 7 — Materializar o servidor HTTP da V0 (superfície mínima)

```md
Implemente apenas os endpoints REST espelhados sob `/tools/*` para V0 e o binding de request/response.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 7)
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Server/

Regras:
- manter paridade semântica com o contrato (não “inventar” campos)

Critério de pronto:
- testes de contrato HTTP passam para `get_draw_window` e `compute_window_metrics`
```

## Fase 8 — Fechar a V0 por evidência

```md
Implemente apenas o fechamento da V0 por evidência (checklist + limpeza de drift), sem adicionar features novas.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 8)
- docs/vertical-slice.md

Arquivos esperados:
- (ajustes mínimos em docs/tests se houver drift)

Regras:
- não expandir escopo; só encerrar a fatia

Critério de pronto:
- critérios obrigatórios da V0 estão cobertos por testes e passam em CI
```

## Fase 9 — Transporte MCP (protocolo real + paridade com o contrato)

```md
Implemente apenas o transporte MCP real (stdio e/ou HTTP MCP) e paridade com os endpoints REST espelhados.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 9)
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md

Arquivos esperados:
- src/LotofacilMcp.Server/ (binding MCP)

Regras:
- MCP != REST; `tools/list` e `tools/call` precisam funcionar

Critério de pronto:
- um host MCP consegue listar e chamar ao menos as tools da V0
```

## Fase 10 — Expandir tools documentadas (ondas B e C)

```md
Implemente apenas uma tool (ou um pacote mínimo) por PR, sempre com testes de contrato primeiro.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 10)
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md

Arquivos esperados:
- src/ (tool + use case + validação)
- tests/ (contrato + domínio conforme necessário)

Regras:
- atualizar docs+tests+código juntos

Critério de pronto:
- nova tool está coberta por testes e aparece corretamente na descoberta da build
```

## Fase 11 — Fechar evidências da V1

```md
Implemente apenas o fechamento de evidências da V1 em escopo (transportes + tools implementadas), com foco em regressão.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 11)
- docs/mcp-tool-contract.md

Regras:
- sem features novas; apenas evidência e alinhamento

Critério de pronto:
- invariantes globais do contrato estão cobertos por testes relevantes
```

## Fase 12 — Correção de drift (spec ↔ implementação)

```md
Implemente apenas correções de drift (doc vs runtime vs testes) com versionamento e testes que impeçam regressão.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 12)
- docs/mcp-tool-contract.md

Regras:
- sem “remendos” silenciosos; corrigir na fonte de verdade

Critério de pronto:
- drift identificado não reaparece (teste falha se voltar)
```

## Fase 13 — Transporte MCP via HTTP (SSE/Streamable HTTP)

```md
Implemente apenas o MCP via HTTP (SSE e/ou streamable HTTP) conforme ADR 0005 e teste básico de list/call.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 13)
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md

Critério de pronto:
- um host MCP conecta ao endpoint HTTP MCP e executa ao menos uma tool baseline
```

## Fase 14 — `compute_window_metrics` como catálogo executável (dispatcher)

```md
Implemente apenas o dispatcher/registry de métricas e a política de `UNKNOWN_METRIC` com allowlist por build.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 14)
- docs/metric-catalog.md
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md

Critério de pronto:
- métrica conhecida no catálogo mas indisponível na rota falha com `UNKNOWN_METRIC` + `allowed_metrics`
```

## Fase 15 — Métricas derivadas diretas de `frequencia_por_dezena`

```md
Implemente apenas métricas novas de dependência simples (derivadas de `frequencia_por_dezena`) com testes de fórmula.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 15)
- docs/metric-catalog.md

Critério de pronto:
- cada métrica nova tem teste de fórmula e envelope de contrato coerente
```

## Fase 16 — Séries escalares por concurso

```md
Implemente apenas métricas `scope=series`, `shape=series` (ex.: pares, repetição, vizinhos) e ferramentas que as consumam.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 16)
- docs/metric-catalog.md

Critério de pronto:
- série tem comprimento igual à janela resolvida e é determinística
```

## Fase 17 — Séries estruturais (vetoriais por concurso)

```md
Implemente apenas séries estruturais (ex.: `series_of_count_vector[5]`) com representação sem ambiguidade (dims/layout ou 2D).

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 17)
- docs/mcp-tools-melhorias-planejamento.md (Anexo A)
- docs/mcp-tool-contract.md

Critério de pronto:
- cliente não precisa de parsing posicional frágil para entender o shape
```

## Fase 18 — `compose_indicator_analysis` (recorte mínimo)

```md
Implemente apenas o recorte mínimo de composição (target/operador restritos), com validação de pesos e ranking determinístico.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 18)
- docs/mcp-tool-contract.md

Critério de pronto:
- empates têm desempate canônico e a saída é reproduzível
```

## Fase 19 — Associações e padrões (uma tool por vez)

```md
Implemente apenas `analyze_indicator_associations` OU `summarize_window_patterns` por PR, com validações rigorosas e testes de contrato.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 19)
- docs/mcp-tool-contract.md

Critério de pronto:
- tool rejeita inputs ambíguos/incompatíveis com erro estruturado
```

## Fase 20 — Geração e explicação (uma tool por vez)

```md
Implemente apenas `generate_candidate_games` OU `explain_candidate_games` por PR, seguindo ADR 0020 (modo/seed/interseção/orçamento).

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 20)
- docs/adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- `replay_guaranteed` e `deterministic_hash` seguem a política do contrato
```

## Fase 21 — ADR 0006: disponibilidade por rota, pipeline e GAPS

```md
Implemente apenas o fechamento de disponibilidade por rota e sinais de GAPS, para reduzir tentativa/erro do consumidor.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 21)
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md

Critério de pronto:
- consumidor descobre allowlists e erros sem round-trips desnecessários
```

## Fase 22 — ADR 0007: `summarize_window_aggregates`

```md
Implemente apenas `summarize_window_aggregates` em batch com enum fechado, validação e ordenação canônica.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 22)
- docs/adrs/0007-agregados-canonicos-de-janela-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- agregados reduzem tool calls e mantêm determinismo
```

## Fase 23 — ADR 0008: descoberta híbrida e janela por extremos

```md
Implemente apenas a descoberta híbrida (instância/build vs norma) e equivalência de janela por extremos, sem defaults temporais mágicos.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 23)
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md

Critério de pronto:
- integrador não precisa “adivinhar” janela ou mapeamentos legados
```

## Fase 24 — ADR 0009: help + catálogo de templates/resources

```md
Implemente apenas `help` e resources/prompts (quando aplicável) para reduzir lacunas de parâmetros e tornar uso intuitivo.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 24)
- docs/adrs/0009-help-e-catalogo-de-templates-resources-v1.md

Critério de pronto:
- leigo consegue pedir a tool correta com poucas tentativas
```

## Fase 25 — ADR 0010–0018: fechamento de GAPs (brief vs src)

```md
Implemente apenas um GAP por PR (doc + teste + código) seguindo os ADRs 0010–0018 conforme aplicável.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 25)

Critério de pronto:
- GAP deixa de existir como “não especificado” sem evidência
```

## Fase 26 — ADR 0020: flexibilidade de geração (modo/seed/interseção)

```md
Implemente apenas ajustes de geração exigidos por ADR 0020: modo explícito, filtros opt-in, interseção, teto 1k, seed opcional.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 26)
- docs/adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md

Critério de pronto:
- comportamento não tem defaults invisíveis e é auditável
```

## Fase 27 — ADR 0021: apresentação de resumos de janela

```md
Implemente apenas a camada de apresentação (texto humano) conforme ADR 0021: tabelas A/B e distinção “resumo” vs “interpretação”.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 27)
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md

Critério de pronto:
- textos não sugerem predição e estão ancorados ao glossário/catálogo
```

## Fase 28A — ADR 0022: unificar `Dataset:DrawsSourceUri` e aceitar `file://`

```md
Implemente apenas a cirurgia de configuração: uma fonte normativa `Dataset:DrawsSourceUri` sem fallback e suporte a `file://`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 28A)
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- config é inequívoca e falhas retornam `DATASET_UNAVAILABLE` com `details` úteis
```

## Fase 28B — ADR 0022: `Dataset:DrawsSourceUri` por HTTP/HTTPS (JSON)

```md
Implemente apenas o suporte a snapshot JSON via HTTP/HTTPS com versionamento determinístico do dataset.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 28B)
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- mesmo snapshot => mesmo `dataset_version` e mesmas respostas determinísticas
```

## Fase 28 — Implementar métricas canônicas pendentes do catálogo

```md
Implemente apenas um recorte de métricas pendentes por PR, sempre com fórmula, representação não ambígua e testes (domínio + contrato).

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 28)
- docs/metric-catalog.md
- docs/mcp-tool-contract.md

Critério de pronto:
- cada métrica nova é reproduzível e testada (fórmula + envelope + determinismo)
```

