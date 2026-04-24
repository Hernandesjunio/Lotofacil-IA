# Templates atômicos de execução

**Navegação:** [← Brief (índice)](brief.md) · [spec-driven-execution-guide.md](spec-driven-execution-guide.md)

Este documento transforma as fases do [spec-driven-execution-guide.md](spec-driven-execution-guide.md) (numeradas 0 a 20 no guia) em **pedidos atômicos** prontos para uso com IA, preservando o formato normativo de template. A **contagem de fases no guia não é teto** — secções adicionais (a partir da *Fase 21* abaixo) estendem o roteiro quando surgem entregas normativas (ex.: [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md), [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) e [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md), incluindo a [Fase 23](#fase-23-adr-0008-descoberta-janela-por-extremos-e-mapeamento-legado) para o 0008) sem reabrir a numeração fechada do guia; novas fases seguem o **mesmo padrão** (bloco `Implemente apenas…`, referências, arquivos, regras, critério de pronto). A Fase 23 do [guia de execução](spec-driven-execution-guide.md) (secção *Fase 23: Descoberta híbrida…*) e esta secção cobrem a mesma entrega normativa.

## Fase 0 - Congelar a base

### Template 0.1 - Checkpoint normativo inicial

```md
Implemente apenas um checkpoint de alinhamento normativo da base (sem codar feature), registrando conformidade com ADRs e specs da V0.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 0)
- docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md
- docs/vertical-slice.md
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md

Arquivos esperados:
- docs/ (nota curta de alinhamento, se necessário)
- README.md (somente se houver ajuste textual de contexto)

Regras:
- não extrapolar além do recorte citado;
- não implementar novas funcionalidades nesta fase;
- apontar explicitamente qualquer contradição estrutural pendente.

Critério de pronto:
- não há pendência estrutural crítica contraditória à V0;
- o estado normativo vigente está explícito e rastreável.
```

## Fase 1 - Esqueleto mínimo do repositório

### Template 1.1 - Estrutura mínima compilável

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
- não extrapolar além do recorte citado;
- manter nomes e fronteiras dos projetos conforme ADR 0004;
- não criar projetos de integração/E2E antes da necessidade.

Critério de pronto:
- solution compila vazia;
- referências entre projetos respeitam fronteiras;
- não há pasta/projeto sem função objetiva.
```

## Fase 2 - Fixture mínima e testes vermelhos da V0

### Template 2.1 - Testes vermelhos da fatia mínima

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
- não extrapolar além do recorte citado;
- manter TDD (testes devem falhar pelo motivo esperado);
- cobrir explicitamente normalização, janela, fórmula base e erros mínimos de contrato.

Critério de pronto:
- existe teste explícito da barreira de normalização;
- existem testes negativos para `metrics` ausente e `UNKNOWN_METRIC`;
- suíte inicial falha pelo motivo correto antes do código.
```

## Fase 3 - Núcleo canônico mínimo

### Template 3.1 - Domínio mínimo da V0

```md
Implemente apenas o núcleo de domínio mínimo da V0: modelos canônicos, normalização, janela e `frequencia_por_dezena@1.0.0`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 3)
- docs/metric-catalog.md
- docs/mcp-tool-contract.md
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md
- docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md

Arquivos esperados:
- src/LotofacilMcp.Domain/Models/
- src/LotofacilMcp.Domain/Normalization/
- src/LotofacilMcp.Domain/Windows/
- src/LotofacilMcp.Domain/Metrics/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- não mover para `Domain` códigos que são de contrato/transporte.

Critério de pronto:
- testes de normalização, janela e fórmula passam sem HTTP;
- a métrica retorna tipagem coerente com o catálogo.
```

## Fase 4 - Infraestrutura determinística mínima

### Template 4.1 - Fixture, versionamento e hash canônico

```md
Implemente apenas a infraestrutura determinística mínima: provider de fixture, `dataset_version`, JSON canônico e SHA-256.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 4)
- docs/mcp-tool-contract.md
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md
- docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md

Arquivos esperados:
- src/LotofacilMcp.Infrastructure/Providers/
- src/LotofacilMcp.Infrastructure/DatasetVersioning/
- src/LotofacilMcp.Infrastructure/CanonicalJson/
- tests/LotofacilMcp.Infrastructure.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- não introduzir semântica estatística na infraestrutura.

Critério de pronto:
- mesmo snapshot gera mesmo `dataset_version`;
- mesmo input canônico gera mesmo `deterministic_hash`;
- testes explícitos de estabilidade passam.
```

## Fase 5 - Casos de uso da V0

### Template 5.1 - Orquestração Application da V0

```md
Implemente apenas os casos de uso da V0 (`GetDrawWindowUseCase` e `ComputeWindowMetricsUseCase`) e validações cross-field em `Application`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 5)
- docs/vertical-slice.md
- docs/mcp-tool-contract.md
- docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md

Arquivos esperados:
- src/LotofacilMcp.Application/UseCases/
- src/LotofacilMcp.Application/Validation/
- tests/LotofacilMcp.Domain.Tests/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- não mover regra estatística do domínio para o server.

Critério de pronto:
- casos de uso resolvem janela corretamente;
- `MetricValue` sai tipado sem lógica de cálculo no `Server`;
- insumos de envelope (`dataset_version`, `tool_version`, hash) estão prontos para entrega.
```

## Fase 6 - Testes de contrato da V0

### Template 6.1 - Contrato mínimo explícito

```md
Implemente apenas os testes de contrato mínimos da V0 (envelope, `MetricValue`, `UNKNOWN_METRIC` e `INVALID_REQUEST`).

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 6)
- docs/vertical-slice.md
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md

Arquivos esperados:
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- nenhuma obrigação do envelope pode ficar implícita.

Critério de pronto:
- testes cobrem `dataset_version`, `tool_version`, `deterministic_hash`;
- testes cobrem shape de erro `UNKNOWN_METRIC` e `INVALID_REQUEST`;
- contrato mínimo da V0 fica rastreável por testes.
```

## Fase 7 - Servidor HTTP da V0 (superfície mínima)

### Template 7.1 - Exposição HTTP mínima da V0

```md
Implemente apenas a superfície HTTP mínima da V0 para `get_draw_window` e `compute_window_metrics`, sem incluir MCP nesta fase.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 7)
- docs/mcp-tool-contract.md
- docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md
- docs/project-guide.md

Arquivos esperados:
- src/LotofacilMcp.Server/Program.cs
- src/LotofacilMcp.Server/DependencyInjection/
- src/LotofacilMcp.Server/Tools/ (ou endpoints equivalentes)
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- implementar binding/validação estrutural e serialização de erros no `Server`;
- manter auth/throttle/quota explicitamente desligados por padrão.

Critério de pronto:
- endpoints da V0 respondem conforme contrato;
- envelope mínimo está presente;
- `get_draw_window` retorna concursos em ordem crescente.
```

## Fase 8 - Fechar V0 por evidência

### Template 8.1 - Evidências de fechamento da V0

```md
Implemente apenas o fechamento da V0 por evidência: execução e registro de testes de domínio, contrato e integração mínima.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 8)
- docs/vertical-slice.md
- docs/contract-test-plan.md

Arquivos esperados:
- docs/ (evidência de cobertura/fechamento, se necessário)
- pipeline/fluxo de testes já existente

Regras:
- não extrapolar além do recorte citado;
- não iniciar novas features;
- alinhar documentação ao comportamento observado.

Critério de pronto:
- V0 está verde e rastreável por testes;
- normalização, envelope e determinismo estão cobertos antes da próxima fatia.
```

## Fase 9 - Transporte MCP (protocolo real + paridade)

### Template 9.1 - MCP real via `stdio`

```md
Implemente apenas a exposição das tools já existentes via protocolo MCP real em `stdio`, com paridade semântica com HTTP espelhado.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 9 e 9A)
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Server/
- executável/entrada `--mcp-stdio` (se separado)
- tests/ (integração MCP + paridade)
- README.md (configuração mínima de host MCP)

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- confinar SDK MCP ao `Server`;
- tratar `/tools/*` como REST espelhado, não MCP.

Critério de pronto:
- host MCP desktop lista/chama tools via `stdio`;
- `tools/list` e `tools/call` funcionam;
- paridade JSON MCP/stdio ↔ REST `/tools/*` é provada por teste.
```

## Fase 10 - Expandir tools documentadas (ondas B e C)

### Template 10.1 - Vertical slice de uma tool por vez

```md
Implemente apenas uma tool da Fase 10 por vez (começando por `analyze_indicator_stability`), em fatia vertical completa com HTTP + MCP.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 10)
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md
- docs/adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (se a tool mexe em disponibilidade, pipeline, GAPS, `stability_check` ou pares–entropia)
- docs/metric-catalog.md
- docs/generation-strategies.md (quando aplicável)

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- fechar semântica no spec antes de codar quando houver lacuna;
- expor HTTP + MCP no mesmo recorte, salvo exceção documentada.

Critério de pronto:
- tool escolhida tem teste(s) objetivo(s) de contrato/domínio;
- entrada/saída segue contrato;
- envelope exigido pelo contrato está presente.
```

## Fase 11 - Fechar evidências da V1

### Template 11.1 - Evidência de escopo V1 entregue

```md
Implemente apenas o fechamento de evidências da V1 para o escopo entregue (transportes MCP + tools implementadas).

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 11)
- docs/contract-test-plan.md
- docs/vertical-slice.md
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md

Arquivos esperados:
- docs/contract-test-plan.md
- docs/vertical-slice.md (ou documento de escopo V1 equivalente)
- registros de execução das suítes relevantes

Regras:
- não extrapolar além do recorte citado;
- não declarar fechado sem evidência de teste;
- explicitar superfícies entregues (ex.: `stdio` apenas, `stdio` + HTTP MCP).

Critério de pronto:
- nenhuma tool em escopo fica unilateral (HTTP ou MCP) sem justificativa;
- documentação, testes e comportamento observado estão alinhados;
- REST deprecado (`/mcp/tools/*`, quando existir) está explicitamente classificado.
```

## Fase 12 - Correção de drift (spec ↔ implementação)

### Template 12.1 - Classificar e corrigir drift

```md
Implemente apenas a correção mínima de um drift explicitamente classificado (semântico, transporte, estrutural ou evidência), com rastreabilidade.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 12)
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md

Arquivos esperados:
- código afetado pelo desvio
- docs/ operacionais e de contrato afetados
- testes de regressão do drift

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- escolher explicitamente: reconduzir código ao spec OU revisar spec;
- marcar compatibilidades ambíguas como `deprecated` quando não puder remover.

Critério de pronto:
- drift está classificado e documentado;
- documentação, testes e comportamento observado reconvergem;
- não permanece superfície “MCP-like” sem protocolo MCP real.
```

## Fase 13 - Transporte MCP via HTTP (SSE/Streamable HTTP)

### Template 13.1 - MCP HTTP real com paridade

```md
Implemente apenas o transporte MCP HTTP real (`/sse` e/ou `/mcp`) com discovery/call e paridade semântica com REST espelhado.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 13)
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Server/ (integração MCP HTTP)
- testes de integração MCP HTTP
- documentação operacional de cliente MCP HTTP

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- distinguir explicitamente MCP HTTP real de REST espelhado (`/tools/*`);
- não reabrir a Fase 9.

Critério de pronto:
- cliente MCP conecta e executa `tools/list` + `tools/call` por HTTP;
- paridade MCP HTTP ↔ REST é provada por teste;
- `/mcp/tools/*` (quando existir) segue marcado como REST deprecado.
```

## Fase 14 - `compute_window_metrics` como catálogo executável

### Template 14.1 - Dispatcher por `metrics[].name`

```md
Implemente apenas o dispatcher real de `compute_window_metrics` por `metrics[].name`, garantindo paridade 1:1 entre lista de métricas pedida e lista de métricas retornada.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 14)
- docs/mcp-tool-contract.md
- docs/metric-catalog.md

Arquivos esperados:
- src/LotofacilMcp.Application/ (orquestração da tool)
- src/LotofacilMcp.Domain/ (resolução de métricas por nome canônico)
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- respeitar fronteiras do ADR 0004 e superfície MCP do ADR 0005;
- seguir nomes canônicos do catálogo/contrato;
- não colapsar métricas duplicadas no request.

Critério de pronto:
- `compute_window_metrics` despacha por nome canônico;
- o output mantém mesma cardinalidade e ordem lógica esperada para o pedido;
- testes de contrato para paridade pedido↔resposta passam.
```

### Template 14.2 - Erro `UNKNOWN_METRIC`

```md
Implemente apenas a validação de nomes fora do catálogo em `compute_window_metrics`, emitindo `UNKNOWN_METRIC` no envelope de erro contratual.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 14)
- docs/mcp-tool-contract.md
- docs/metric-catalog.md

Arquivos esperados:
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- respeitar fronteiras do ADR 0004 e superfície MCP do ADR 0005;
- seguir nomes canônicos do catálogo/contrato;
- preservar `dataset_version`, `tool_version` e `deterministic_hash`.

Critério de pronto:
- nome inválido retorna `UNKNOWN_METRIC`;
- o código de erro e payload batem com o contrato;
- testes negativos de contrato passam.
```

## Fase 15 - Métricas derivadas de `frequencia_por_dezena`

### Template 15.1 - `top10_mais_sorteados@1.0.0`

```md
Implemente apenas a métrica `top10_mais_sorteados@1.0.0`, derivada de `frequencia_por_dezena`, com desempate canônico por dezena ascendente.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 15)
- docs/metric-catalog.md
- docs/test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- respeitar fronteiras do ADR 0004 e superfície MCP do ADR 0005;
- seguir nomes canônicos do catálogo/contrato;
- manter determinismo completo.

Critério de pronto:
- lista de 10 dezenas é determinística;
- empates obedecem dezena ascendente;
- `scope`, `shape`, `unit` e `version` batem com o catálogo.
```

### Template 15.2 - `top10_menos_sorteados@1.0.0`

```md
Implemente apenas a métrica `top10_menos_sorteados@1.0.0`, derivada de `frequencia_por_dezena`, com desempate canônico por dezena ascendente.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 15)
- docs/metric-catalog.md
- docs/test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- respeitar fronteiras do ADR 0004 e superfície MCP do ADR 0005;
- seguir nomes canônicos do catálogo/contrato;
- manter determinismo completo.

Critério de pronto:
- lista de 10 dezenas é determinística;
- empates obedecem dezena ascendente;
- `scope`, `shape`, `unit` e `version` batem com o catálogo.
```

## Fase 16 - Séries escalares por concurso

### Template 16.1 - `pares_no_concurso@1.0.0`

```md
Implemente apenas a série escalar `pares_no_concurso@1.0.0`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 16)
- docs/metric-catalog.md
- docs/test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir nomes e tipagem canônicos do catálogo/contrato;
- validar comprimento da série conforme janela resolvida.

Critério de pronto:
- série possui comprimento correto para a janela;
- cálculo é determinístico na fixture mínima;
- metadados de tipo da métrica batem com o catálogo.
```

### Template 16.2 - `repeticao_concurso_anterior@1.0.0`

```md
Implemente apenas a série escalar `repeticao_concurso_anterior@1.0.0`, respeitando o comprimento normativo descrito no catálogo/ADR aplicável.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 16)
- docs/metric-catalog.md
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir nomes e tipagem canônicos do catálogo/contrato;
- tratar corretamente a borda da série (primeiro concurso elegível).

Critério de pronto:
- comprimento da série respeita regra normativa;
- valores são determinísticos;
- testes de borda passam.
```

### Template 16.3 - `quantidade_vizinhos_por_concurso@1.0.0`

```md
Implemente apenas a série escalar `quantidade_vizinhos_por_concurso@1.0.0`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 16)
- docs/metric-catalog.md
- docs/test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir nomes e tipagem canônicos do catálogo/contrato;
- preservar determinismo em empate/ordenação quando aplicável.

Critério de pronto:
- série escalar calculada para toda a janela válida;
- comprimento e tipagem batem com o catálogo;
- testes determinísticos passam.
```

### Template 16.4 - `sequencia_maxima_vizinhos_por_concurso@1.0.0`

```md
Implemente apenas a série escalar `sequencia_maxima_vizinhos_por_concurso@1.0.0`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 16)
- docs/metric-catalog.md
- docs/test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir nomes e tipagem canônicos do catálogo/contrato;
- manter coerência com a definição de vizinhança adotada no domínio.

Critério de pronto:
- série calculada de forma determinística;
- comprimento da série é coerente com a janela;
- testes da métrica passam no domínio e no caminho da tool.
```

## Fase 17 - Séries estruturais e derivados

### Template 17.1 - `distribuicao_linha_por_concurso@1.0.0`

```md
Implemente apenas `distribuicao_linha_por_concurso@1.0.0` com `shape=series_of_count_vector[5]`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 17)
- docs/metric-catalog.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir shape e metadados canônicos;
- cada vetor por concurso deve somar 15.

Critério de pronto:
- shape retornado é `series_of_count_vector[5]`;
- cada ponto da série soma 15;
- testes determinísticos passam.
```

### Template 17.2 - `distribuicao_coluna_por_concurso@1.0.0`

```md
Implemente apenas `distribuicao_coluna_por_concurso@1.0.0` com `shape=series_of_count_vector[5]`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 17)
- docs/metric-catalog.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir shape e metadados canônicos;
- cada vetor por concurso deve somar 15.

Critério de pronto:
- shape retornado é `series_of_count_vector[5]`;
- cada ponto da série soma 15;
- testes determinísticos passam.
```

### Template 17.3 - `entropia_linha` e `entropia_coluna`

```md
Implemente apenas as métricas `entropia_linha_por_concurso@1.0.0` e `entropia_coluna_por_concurso@1.0.0`, em bits.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 17)
- docs/metric-catalog.md
- docs/test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir nomes canônicos e unidades do catálogo;
- assegurar valores finitos para todos os pontos da série.

Critério de pronto:
- unidade é bits conforme catálogo;
- não há NaN/Infinity;
- testes determinísticos passam para linha e coluna.
```

### Template 17.4 - `hhi_linha` e `hhi_coluna`

```md
Implemente apenas as métricas `hhi_linha_por_concurso@1.0.0` e `hhi_coluna_por_concurso@1.0.0`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 17)
- docs/metric-catalog.md
- docs/test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Domain/
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir shape/unidade/version do catálogo;
- preservar determinismo no cálculo da série.

Critério de pronto:
- séries HHI batem com fórmula e catálogo;
- valores são finitos e consistentes com as distribuições base;
- testes determinísticos passam.
```

## Fase 18 - `compose_indicator_analysis` (recorte mínimo)

### Template 18.1 - `target=dezena`, `operator=weighted_rank`

```md
Implemente apenas o recorte mínimo de `compose_indicator_analysis` com `target=dezena` e `operator=weighted_rank`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 18)
- docs/mcp-tool-contract.md
- docs/metric-catalog.md

Arquivos esperados:
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- validar pesos com tolerância `1.0 ± 1e-9`;
- rejeitar transforms fora do enum com código contratual correto;
- manter paridade semântica HTTP + MCP.

Critério de pronto:
- existe 1 teste positivo determinístico no recorte mínimo;
- há testes negativos para pesos inválidos e transform inválida;
- payload segue contrato nos dois transportes.
```

## Fase 19 - Associações e padrões (uma tool por vez)

### Template 19.1 - `analyze_indicator_associations` (escalares + Spearman)

```md
Implemente apenas o primeiro recorte de `analyze_indicator_associations` para séries escalares com `method=spearman`.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 19)
- docs/mcp-tool-contract.md
- docs/metric-catalog.md

Arquivos esperados:
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- vetoriais só podem entrar com `aggregation` explícita;
- garantir paridade semântica MCP e HTTP.

Critério de pronto:
- há teste negativo com código de erro contratual;
- há teste positivo determinístico com fixture;
- respostas MCP/HTTP são semanticamente equivalentes no mesmo request.
```

### Template 19.2 - `summarize_window_patterns` (`range_method=iqr`)

```md
Implemente apenas o primeiro recorte de `summarize_window_patterns` com `range_method=iqr` e uma feature suportada.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 19)
- docs/mcp-tool-contract.md
- docs/metric-catalog.md

Arquivos esperados:
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- output deve declarar `Q1`, `median`, `Q3`, `IQR`, cobertura e contagens;
- garantir paridade semântica MCP e HTTP.

Critério de pronto:
- há teste negativo com código de contrato correto;
- há teste positivo determinístico com fixture;
- payload contém todos os campos estatísticos obrigatórios.
```

## Fase 20 - Geração e explicação (uma tool por vez)

### Template 20.1 - `generate_candidate_games` (estratégia nominal simples)

```md
Implemente apenas o primeiro recorte de `generate_candidate_games` com uma estratégia nominal simples, orçamento e determinismo.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 20)
- docs/generation-strategies.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- exigir `seed` quando `search_method` for `sampled` ou `greedy_topk`;
- output deve trazer `strategy_name`, `strategy_version`, `search_method`, `tie_break_rule` e `seed_used` quando aplicável;
- manter paridade HTTP + MCP.

Critério de pronto:
- teste negativo de contrato (seed obrigatória) passa;
- teste positivo determinístico passa;
- output inclui toda a linhagem obrigatória.
```

### Template 20.2 - `explain_candidate_games` (ranking determinístico)

```md
Implemente apenas o primeiro recorte de `explain_candidate_games` com ranking determinístico de estratégias e breakdown de métricas/exclusões com versões.

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (Fase 20)
- docs/generation-strategies.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- ranking deve ser determinístico;
- explicação deve incluir versões das métricas/exclusões usadas;
- garantir paridade semântica MCP e HTTP.

Critério de pronto:
- há teste negativo de contrato para input inválido;
- há teste positivo determinístico com fixture;
- payload de explicação contém ranking e breakdown rastreáveis.
```

## Fase 21 - ADR 0006: disponibilidade por rota, pipeline, GAPS, estabilidade de associação e pares–entropia

*Extensão pós–Fase 20. Norma: [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md). Alinha [mcp-tool-contract.md](mcp-tool-contract.md), [metric-catalog.md](metric-catalog.md), [contract-test-plan.md](contract-test-plan.md) e [test-plan.md](test-plan.md) na mesma entrega lógica quando a implementação acompanhar.*

### Template 21.1 - Matriz catálogo × `compute_window_metrics` e erros com `details`

```md
Implemente apenas a conformidade da rota `compute_window_metrics` com a matriz de disponibilidade e mensagens de `UNKNOWN_METRIC` com `details.metric_name` (e `allowed_metrics` quando aplicável), alinhada à secção *Disponibilidade* do contrato e à tabela do [metric-catalog.md](metric-catalog.md).

Referências obrigatórias:
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (D1)
- docs/mcp-tool-contract.md (tool `compute_window_metrics`, tabela de erros)
- docs/metric-catalog.md (Disponibilidade normativa)
- docs/vertical-slice.md (recorte mínimo V0 vs. extensão)
- docs/contract-test-plan.md (GAPS, cenário A)
- docs/test-plan.md (GAPS)

Arquivos esperados:
- src/LotofacilMcp.Application/ (validação de allowlist por build, se aplicável)
- src/LotofacilMcp.Domain/ (opcional: registro de métricas expostas)
- tests/LotofacilMcp.ContractTests/ e/ou tests/LotofacilMcp.Infrastructure.Tests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- não alterar semântica de métricas no catálogo — apenas rota, erros e pistas;
- manter paridade HTTP + MCP.

Critério de pronto:
- teste congelado do payload de erro (cenário A) passa;
- documentação e `tool_version` rastreáveis.
```

### Template 21.2 - `analyze_indicator_stability`: `min_history` vs. janela e `INSUFFICIENT_HISTORY`

```md
Implemente apenas a validação de `min_history` em relação ao tamanho efetivo da janela resolvida em `analyze_indicator_stability`, emitindo `INSUFFICIENT_HISTORY` com `details` coerentes (ADR 0006 D4).

Referências obrigatórias:
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (D4)
- docs/mcp-tool-contract.md (`analyze_indicator_stability`)
- docs/contract-test-plan.md (GAPS, cenário C)
- docs/test-plan.md (GAPS)

Arquivos esperados:
- src/LotofacilMcp.Application/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- não introduzir default mágico de `min_history` no servidor;
- manter paridade HTTP + MCP.

Critério de pronto:
- teste negativo com `min_history` > janela efetiva passa com código e `details` corretos.
```

### Template 21.3 - `analyze_indicator_associations`: `stability_check` implementado ou `UNSUPPORTED_STABILITY_CHECK`

```md
Implemente apenas o comportamento de `stability_check` em `analyze_indicator_associations`: ou cálculo de estabilidade em subjanelas conforme contrato, ou erro `UNSUPPORTED_STABILITY_CHECK` quando o request declara `stability_check` e a build ainda não suporta (ADR 0006 D2).

Referências obrigatórias:
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (D2)
- docs/mcp-tool-contract.md (semântica de `stability_check`, tabela de erros)
- docs/contract-test-plan.md (GAPS, cenário D)

Arquivos esperados:
- src/LotofacilMcp.Domain/ ou Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- sucesso sem `stability_check` no request continua a devolver magnitude global;
- manter paridade HTTP + MCP.

Critério de pronto:
- teste com `stability_check` e build sem suporte emite `UNSUPPORTED_STABILITY_CHECK`, ou teste de sucesso com subjanelas passa (conforme escolha de implementação nesta entrega);
- sem sucesso “vazio” quando o cliente pediu `stability_check` explicitamente.
```

### Template 21.4 - Bateria GAPS (B, E) e coerência explain / geração vs. `compute`

```md
Implemente apenas testes (e ajustes mínimos de código) para: (B) coerência entre `explain_candidate_games`/estratégias e `compute_window_metrics` quando a métrica ainda não está na rota; (E) teste de contrato determinístico do cenário pares–entropia (Spearman entre `pares_no_concurso` e `entropia_linha_por_concurso`, mesma janela), conforme [test-plan.md](test-plan.md) e ADR 0006 D5.

Referências obrigatórias:
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (D5, D6)
- docs/contract-test-plan.md (GAPS B, E)
- docs/test-plan.md (Cenário canónico, GAPS)
- docs/generation-strategies.md
- tests/fixtures/ (golden conforme convenção)

Arquivos esperados:
- tests/LotofacilMcp.ContractTests/
- tests/fixtures/ ou `tests/fixtures/golden/`

Regras:
- não extrapolar além do recorte citado;
- golden revisado em PR com mudança intencional de semântica;
- linguagem descritiva nos asserts/comentários de intenção de teste.

Critério de pronto:
- (B) regressão documentada ou resolvida por promoção de métrica na mesma PR, conforme decisão;
- (E) magnitude reprodutível ou tolerância fixa no teste, com `dataset_version` estável.
```

### Template 21.5 - (Opcional) Esteira L6 — roteamento prompt pares–entropia

```md
Implemente ou estenda apenas o teste de integração real (OpenAI) com o cenário L6 descrito em [live-openai-integration-pipeline.md](live-openai-integration-pipeline.md), usando o prompt da [prompt-catalog.md](prompt-catalog.md) §3 item 10, sem exigir L6 no gate mínimo L1–L5.

Referências obrigatórias:
- docs/live-openai-integration-pipeline.md
- docs/prompt-catalog.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- pasta de testes de integração / workflow GitHub (conforme o repositório)

Regras:
- controlar custo e `OPENAI_MAX_ROUNDS`;
- validar tool `analyze_indicator_associations` e janela explícita;
- não exigir determinismo do LLM, apenas roteamento e resposta de servidor conforme contrato.

Critério de pronto:
- L6 conclui sem exceção não tratada, quando a suíte estendida estiver ativa;
- falha de L6 não quebra a definição de suíte mínima de cinco cenários, até promoção a bloqueador.
```

## Fase 22 - ADR 0007: agregados canônicos (histogramas, padrões e matrizes) via `summarize_window_aggregates`

*Extensão pós–Fase 20. Norma: [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md). Alinha `summarize_window_aggregates` em [mcp-tool-contract.md](mcp-tool-contract.md), roteamento em [prompt-catalog.md](prompt-catalog.md), e testes em [test-plan.md](test-plan.md) + [contract-test-plan.md](contract-test-plan.md) na mesma entrega lógica quando a implementação acompanhar.*

### Template 22.1 - Contrato fechado da tool `summarize_window_aggregates` (schema + erros + exemplos)

```md
Implemente apenas a atualização do contrato e dos documentos de teste para introduzir a tool `summarize_window_aggregates` (sem codar a tool ainda).

Referências obrigatórias:
- docs/adrs/0007-agregados-canonicos-de-janela-v1.md
- docs/mcp-tool-contract.md
- docs/test-plan.md
- docs/contract-test-plan.md
- docs/prompt-catalog.md

Arquivos esperados:
- docs/mcp-tool-contract.md (seção da tool, tipos e regras)
- docs/test-plan.md (linha de cobertura por tool)
- docs/contract-test-plan.md (matriz mínima + fase B.1 de agregados)
- docs/prompt-catalog.md (prompts que exigem agregados com tools esperadas)

Regras:
- não extrapolar além do recorte citado;
- `aggregate_type` deve ser enum fechado; parâmetros obrigatórios por tipo;
- proibir defaults semânticos ocultos: bucketização e dimensões de matriz são declaradas no request;
- declarar ordenação canônica e desempates no contrato;
- manter linguagem descritiva (sem prometer “chance de sair”).

Critério de pronto:
- contrato da tool está fechado com regras de validação e ordenação;
- plano de testes descreve casos positivos/negativos;
- prompts roteiam para a tool nova onde apropriado.
```

### Template 22.2 - Testes de contrato (vermelhos) para agregados canônicos

```md
Implemente apenas os testes de contrato (vermelhos primeiro) para `summarize_window_aggregates`: validação de request, determinismo (`deterministic_hash`) e ordenação canônica.

Referências obrigatórias:
- docs/mcp-tool-contract.md (seção `summarize_window_aggregates`)
- docs/contract-test-plan.md (Fase B.1)
- docs/adrs/0007-agregados-canonicos-de-janela-v1.md

Arquivos esperados:
- tests/LotofacilMcp.ContractTests/
- tests/fixtures/ (novas fixtures pequenas se necessário)

Regras:
- não codar a implementação da tool antes dos testes falharem pelo motivo certo;
- incluir testes negativos: `aggregates` ausente, `aggregate_type` inválido, bucket spec inválida, bounds inválidos, `UNKNOWN_METRIC`, `UNSUPPORTED_SHAPE`;
- incluir teste de repetição do mesmo request para afirmar determinismo de payload e hash.

Critério de pronto:
- testes falham pelo motivo correto antes da implementação;
- casos de ordenação canônica são explicitamente verificados.
```

### Template 22.3 - Implementação mínima da tool `summarize_window_aggregates` (após testes vermelhos)

```md
Implemente apenas a primeira versão funcional de `summarize_window_aggregates`, fazendo os testes de contrato da fase 22.2 passarem sem afrouxar regras de validação.

Referências obrigatórias:
- docs/adrs/0007-agregados-canonicos-de-janela-v1.md
- docs/mcp-tool-contract.md (seção `summarize_window_aggregates`)
- docs/contract-test-plan.md (Fase B.1)
- docs/test-plan.md (cobertura por tool + agregados canônicos)

Arquivos esperados:
- src/LotofacilMcp.Application/
- src/LotofacilMcp.Server/
- tests/LotofacilMcp.ContractTests/ (ajustes mínimos só se necessário para refletir schema final)

Regras:
- não extrapolar além do recorte citado;
- implementar apenas os `aggregate_type` do recorte inicial do ADR 0007;
- manter enum fechado e parâmetros obrigatórios por tipo;
- não introduzir defaults semânticos ocultos (bucketização e bounds sempre explícitos no request);
- preservar ordenação canônica e desempates determinísticos.

Critério de pronto:
- testes de contrato da 22.2 passam;
- erros mínimos (`UNSUPPORTED_AGGREGATE_TYPE`, `UNSUPPORTED_SHAPE`, `INVALID_REQUEST`, `UNKNOWN_METRIC`) são emitidos conforme contrato;
- resposta preserva ordem de `aggregates[]` do request e `deterministic_hash` estável.
```

### Template 22.4 - Paridade MCP/HTTP e evidências de agregados (fixtures/goldens)

```md
Implemente apenas a validação final de paridade e evidências da `summarize_window_aggregates`, sem ampliar escopo funcional da tool.

Referências obrigatórias:
- docs/adrs/0007-agregados-canonicos-de-janela-v1.md
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md (Fase B.1)
- docs/test-plan.md (determinismo + cobertura por tool)

Arquivos esperados:
- tests/LotofacilMcp.ContractTests/ (paridade MCP/HTTP para sucesso e erro)
- tests/fixtures/ e/ou tests/fixtures/golden/
- docs/contract-test-plan.md (se precisar registrar evidência específica)

Regras:
- não extrapolar além do recorte citado;
- não adicionar novo `aggregate_type` nesta etapa;
- provar paridade semântica MCP ↔ HTTP para o mesmo request;
- congelar golden apenas quando o payload estiver estável e auditável.

Critério de pronto:
- chamadas MCP e HTTP retornam payload semanticamente equivalente para sucesso e erro;
- determinismo (`deterministic_hash`) permanece estável para requests repetidos;
- fixtures/goldens de agregados ficam rastreáveis e coerentes com o contrato.
```

## Fase 23 - ADR 0008: descoberta, janela por extremos e mapeamento legado

*Extensão pós–Fase 20. Norma: [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md). Alinha [mcp-tool-contract.md](mcp-tool-contract.md), [metric-catalog.md](metric-catalog.md), [metric-glossary.md](metric-glossary.md), [contract-test-plan.md](contract-test-plan.md) (Fase B.2) e, onde couber, [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) (erros com `details` / allowlist) na mesma entrega lógica quando a implementação acompanhar. Não confundir com o [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) (agregados canônicos). Espelha a [Fase 23 do guia](spec-driven-execution-guide.md).*

### Template 23.1 - Revisão de docs e plano de testes (Fase B.2) sem implementação nova

```md
Implemente apenas a coordenação de documentação: confirmar alinhamento entre [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md), [mcp-tool-contract.md](mcp-tool-contract.md) (entidade `Window`, *Prompts e Resources*), [metric-catalog.md](metric-catalog.md) (janela por extremos, `HistoricoTop10MaisSorteados`, `QtdFrequencia`) e [metric-glossary.md](metric-glossary.md); acrescentar ou ajustar a matriz **Fase B.2** em [contract-test-plan.md](contract-test-plan.md) para janela, ambiguidade e `top10_mais_sorteados`. Não codar resolução de janela nem tool de listagem além do que o ADR e o contrato fecharem.

Referências obrigatórias:
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md
- docs/mcp-tool-contract.md
- docs/metric-catalog.md
- docs/metric-glossary.md
- docs/contract-test-plan.md (Fase B.2)
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (D1, se tocar em `allowed_metrics`)

Arquivos esperados:
- docs/mcp-tool-contract.md (se necessário, alinhamento fino)
- docs/contract-test-plan.md
- docs/metric-catalog.md / metric-glossary.md (apenas se fechar lacuna em relação ao 0008)

Regras:
- não exigir nome concreto de tool `list_mcp_surface` (ou similar) se o ADR 0008 ainda não tiver fechado isso no contrato — apenas semântica D1;
- não reutilizar o [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) como fonte de descoberta ou mapeamento Top 10;
- proibir N mágico de UI legada no servidor (D4 do ADR 0008).

Critério de pronto:
- documentos normativos sem contradição com D1–D6 do ADR 0008;
- Fase B.2 descrita no *contract-test-plan* com casos mínimos reprodutíveis;
- referências cruzadas entre contrato, catálogo e ADR 0008 coerentes.
```

### Template 23.2 - Testes de contrato vermelhos: janela, ambiguidade e `top10_mais_sorteados`

```md
Implemente apenas testes (vermelhos primeiro) para: equivalência de recorte `start_contest_id` / `end_contest_id` (inclusivos) com `window_size` + `end_contest_id` quando o protocolo suportar ambas as formas; rejeição com código fechado (`INVALID_REQUEST` ou equivalente documentado) quando a combinação for ambígua; e `top10_mais_sorteados@1.0.0` alinhado à Tabela 2 do [metric-catalog.md](metric-catalog.md) (usar p.ex. `tie_heavy.json` para stress de empate).

Referências obrigatórias:
- docs/contract-test-plan.md (Fase B.2)
- docs/mcp-tool-contract.md (entidade `Window`, tools com janela, tabela de erros)
- docs/metric-catalog.md
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md
- tests/fixtures/ (fixtures existentes e convenção de golden)

Arquivos esperados:
- tests/LotofacilMcp.ContractTests/
- tests/fixtures/ e/ou tests/fixtures/golden/

Regras:
- não ampliar semântica além do ADR 0008 e do contrato;
- manter paridade de asserção entre HTTP e MCP quando a tool for exposta em ambos;
- golden só atualizado em PR que altere semântica de métrica ou contrato, com revisão.

Critério de pronto:
- testes falham ou passam pelos motivos corretos antes/depois da implementação mínima;
- critério 3 (e correlatos) dos *Critérios de verificação* do ADR 0008 endereçado na suíte.
```

### Template 23.3 - Implementação mínima: resolução de janela e `compute_window_metrics` coerente

```md
Implemente apenas a resolução única e auditável da janela nos requests das tools alvo (p.ex. `get_draw_window`, `compute_window_metrics`) e o alinhamento de `top10_mais_sorteados@1.0.0` ao catálogo, até os testes da fase 23.2 passarem, sem introduzir defaults temporais ocultos (D4).

Referências obrigatórias:
- docs/mcp-tool-contract.md
- docs/metric-catalog.md
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (se a rota preencher `details`)

Arquivos esperados:
- src/LotofacilMcp.Application/ (resolução de janela)
- src/LotofacilMcp.Server/ (se necessário)
- tests/LotofacilMcp.ContractTests/ (verde)

Regras:
- não replicar regras ad hoc de gráfico legado *rolling* sob o rótulo `top10_mais_sorteados` (D3);
- `UNKNOWN_METRIC` e `details` coerentes com a política do projeto (ADR 0006 D1) quando a métrica for conhecida no catálogo mas fora da allowlist;
- manter `deterministic_hash` e envelope conforme contrato.

Critério de pronto:
- testes da 23.2 passam;
- nenhum «últimos N» de UI legada embutido no servidor sem equivalente explícito no request.
```

### Template 23.4 - (Opcional) Paridade de transporte e, se aplicável, Resources MCP

```md
Implemente apenas evidência de paridade MCP ↔ HTTP para sucesso/erro nos casos de janela e `top10` da fase 23, **ou** a exposição mínima de **MCP Resources** alinhada ao D1 (glossário/catálogo) sem duplicar a allowlist de métricas, conforme *Prompts e Resources* em [mcp-tool-contract.md](mcp-tool-contract.md) e o ADR 0008. Se resources não forem entregues nesta fatia, o cliente pode continuar a injetar `docs/` — documentar a escolha.

Referências obrigatórias:
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md
- tests/LotofacilMcp.ContractTests/ (padrão de paridade existente)

Arquivos esperados:
- src/LotofacilMcp.Server/ (resources ou apenas testes de paridade)
- tests/LotofacilMcp.ContractTests/

Regras:
- resources **read-only**; cálculo permanece nas tools;
- não conflitar norma (catálogo) com instância (build): allowlist continua sinalizável em tool/erro.

Critério de pronto:
- paridade comprovada nos caminhos suportados, **ou** resources publicados com URIs e conteúdo rastreável às fontes do repositório.
```

## Fase 24 — ADR 0009: Help + catálogo de templates (resources)

### Template 24.1 — Expor help e resources de templates

```md
Implemente apenas a superfície de ajuda e templates do ADR 0009, sem alterar semântica de métricas nem introduzir defaults ocultos.

Referências obrigatórias:
- docs/mcp-tool-contract.md (Primitivas MCP opcionais: Prompts e Resources)
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md (Camadas A/B/C)
- docs/adrs/0009-help-e-catalogo-de-templates-resources-v1.md
- docs/test-plan.md (cobertura por tool e resources)

Escopo:
- Expor o resource de onboarding curto `lotofacil-ia://help/getting-started@1.0.0` (ponto de entrada agnóstico ao host) com **onboarding leigo-first**:
  - começar com “O que é / o que não é” em linguagem simples (sem promessa, sem predição);
  - incluir **3 passos** com CTA claro (“Peça ajuda” → “Escolha um caminho” → “Escolha o período”);
  - oferecer um **menu curto** (2–4 caminhos) e um “comece aqui” (ex.: painel geral);
  - incluir “Se der erro” com instruções humanas (sem despejar códigos);
  - manter detalhes técnicos (campos/invariantes) em secção separada “Para DEV/integração”, se necessário.
- Expor resources Markdown sob `lotofacil-ia://prompts/`, incluindo `index@1.0.0` e 10 templates versionados.
- Padronizar em todos os templates a preferência de exibição `display_mode = simple | advanced | both` (default `both` quando não declarado).
- Implementar a tool `help` retornando:
  - `tool_version`
  - (recomendado) um “topo de UX” curto opcional (ex.: `quick_start_markdown` ou entrypoints), para que um pedido “ajuda” não vire um catálogo
  - `getting_started_resource_uri` (opcional; recomendado quando o resource existir)
  - `index_resource_uri`
  - `index_markdown`
  - `templates[]` com metadados (id, uri, title, description, suggested_windows)

Arquivos esperados:
- resources/help/getting-started@1.0.0.md
- resources/prompts/index@1.0.0.md
- resources/prompts/*.md (10 templates)
- src/LotofacilMcp.Server/Resources/ (exposição MCP de resources)
- src/LotofacilMcp.Server/Tools/ (tool `help`)
- tests/LotofacilMcp.ContractTests/ (testes de tool discovery + resources list/read)

Regras:
- `help` não calcula métricas, não decide janela, não gera recomendações preditivas.
- templates são conteúdo read-only; manter MIME `text/markdown`.
- manter a distinção “prompt de chat” vs “Prompt MCP” (não confundir termos).

Critério de pronto:
- `help` aparece em `tools/list` e responde com payload estruturado válido.
- `resources/list` inclui `lotofacil-ia://help/getting-started@1.0.0`, o índice e os templates; `resources/read` do getting-started e do índice retorna Markdown.
- testes de contrato cobrem tool discovery e resources list/read.
- leitura humana: uma pessoa sem contexto consegue seguir o getting-started e executar “primeiro uso” sem ler ADRs nem catálogo completo.
```

### Template 24.2 — Refinar onboarding leigo-first (UX) e “ajuda” (progressive disclosure)

```md
Implemente apenas o refinamento editorial/UX do onboarding e da “ajuda” do ADR 0009, para reduzir confusão no primeiro contato (sem mudar semântica de métricas).

Referências obrigatórias:
- docs/adrs/0009-help-e-catalogo-de-templates-resources-v1.md (regras leigo-first e progressive disclosure)
- docs/mcp-tool-contract.md (Prompts e Resources; invariantes de não-calcular e não-esconder defaults)
- resources/help/getting-started@1.0.0.md
- resources/prompts/index@1.0.0.md

Escopo:
- Revisar `resources/help/getting-started@1.0.0.md` para:
  - começar com linguagem simples (“O que é / o que não é”);
  - trazer **3 passos** com CTA;
  - oferecer um **menu curto** (2–4 caminhos) com “comece aqui”;
  - incluir “Se der erro” com instruções humanas;
  - mover jargão/detalhes técnicos para uma secção final opcional “Para DEV/integração”.
- Revisar `resources/prompts/index@1.0.0.md` para começar com um menu curto antes do catálogo de 10 templates.
- (Opcional, não-breaking) Se existir a tool `help`, incluir um bloco curto adequado para “liste ajuda” (ex.: `quick_start_markdown`), mantendo o catálogo completo em `templates[]`.

Arquivos esperados:
- resources/help/getting-started@1.0.0.md
- resources/prompts/index@1.0.0.md
- (opcional) src/LotofacilMcp.Server/Tools/ (ajuste na tool `help`)
- (opcional) tests/LotofacilMcp.ContractTests/ (ajuste de contrato para o novo campo opcional do `help`)

Regras:
- não introduzir promessa, predição ou linguagem de “aumenta chance”;
- não mover/duplicar regras de cálculo: apenas UX e conteúdo read-only;
- evitar referências internas (ADRs, fases, nomes de decisões) no texto principal para iniciantes.

Critério de pronto:
- uma pessoa leiga consegue seguir o getting-started e escolher um caminho sem conhecer tools/ADRs;
- o índice começa com “escolha 1 opção” antes do catálogo;
- (se implementado) `help` responde “ajuda” com um bloco curto antes do catálogo completo.
```

## Fase 25 — ADR 0010–0018: fechamento sistemático de GAPs (brief vs `src/`)

*Extensão pós–Fase 24. Norma: [ADR 0010](adrs/0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) e ADRs 0011–0018. Objetivo: implementar o que já foi exposto no contrato público, reduzindo frustração no consumo do MCP, com discovery e erros canônicos/determinísticos. B18 (CEF) permanece congelado.*

### Template 25.1 — Tool `discover_capabilities` (discovery por build)

```md
Implemente apenas a nova tool MCP `discover_capabilities` conforme [ADR 0011](adrs/0011-tool-de-discovery-de-capacidades-por-build-v1.md), retornando um JSON determinístico com a superfície real desta build (métricas por rota, enums suportados, estratégias de geração, métodos de busca e modos de janela).

Referências obrigatórias:
- docs/adrs/0011-tool-de-discovery-de-capacidades-por-build-v1.md
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md (D1, descoberta instância vs norma)
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Server/Tools/ (registro e handler da tool)
- tests/LotofacilMcp.ContractTests/ (teste de contrato da tool)

Regras:
- não executar cálculo de métricas; retornar apenas metadados de capacidade.
- a resposta deve ser determinística para a mesma build/config.

Critério de pronto:
- `discover_capabilities` aparece em `tools/list` e responde com payload estruturado válido.
- teste(s) de contrato para a tool passam.
```

### Template 25.2 — Registro único de métricas/capacidades e derivação de allowlists

```md
Implemente apenas um registro único de métricas/capacidades (fonte de verdade) conforme [ADR 0012](adrs/0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md) e derive dele: allowlist de `compute_window_metrics`, allowlist de `summarize_window_aggregates`, compatibilidade para associações/composição e o conteúdo de `discover_capabilities`.

Referências obrigatórias:
- docs/adrs/0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (D1, `UNKNOWN_METRIC`/allowed_metrics)
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Application/ (catálogo/registro e validações)
- src/LotofacilMcp.Server/Tools/ (uso do registro na superfície)
- tests/LotofacilMcp.ContractTests/

Regras:
- remover drift entre listas divergentes; uma única fonte deve alimentar validação e discovery.
- manter semântica de erro canônica e determinística.

Critério de pronto:
- `compute_window_metrics` e `summarize_window_aggregates` passam a depender do registro único.
- `discover_capabilities` reflete exatamente o registro.
```

### Template 25.3 — Janela por extremos em todas as tools

```md
Implemente apenas a padronização de janela por extremos (`start_contest_id` + `end_contest_id` inclusivos) em todas as tools orientadas a janela conforme [ADR 0013](adrs/0013-janela-uniforme-por-extremos-em-todas-as-tools-v1.md) e [ADR 0008 D2](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md).

Referências obrigatórias:
- docs/adrs/0013-janela-uniforme-por-extremos-em-todas-as-tools-v1.md
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md (D2)
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md (Fase B.2, se os testes já existirem)

Arquivos esperados:
- src/LotofacilMcp.Server/Tools/
- src/LotofacilMcp.Application/Windows/
- tests/LotofacilMcp.ContractTests/

Regras:
- rejeitar combinações ambíguas/incompatíveis com erro canônico.
- manter paridade HTTP ↔ MCP onde aplicável.

Critério de pronto:
- todas as tools citadas na ADR 0013 aceitam extremos e retornam `window` coerente.
```

### Template 25.4 — Semântica de `allow_pending`

```md
Implemente apenas a semântica observável de `allow_pending` conforme [ADR 0014](adrs/0014-semantica-real-de-allow-pending-v1.md), habilitando métricas marcadas como `pending` apenas quando o opt-in estiver ativo e tornando isso visível em `discover_capabilities`.

Referências obrigatórias:
- docs/adrs/0014-semantica-real-de-allow-pending-v1.md
- docs/adrs/0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Application/Validation/
- src/LotofacilMcp.Server/Tools/
- tests/LotofacilMcp.ContractTests/

Regras:
- sem defaults ocultos: o comportamento deve variar apenas por request explícito.

Critério de pronto:
- com `allow_pending=false`, métricas pendentes falham com erro canônico.
- com `allow_pending=true`, as mesmas métricas funcionam onde suportadas.
```

### Template 25.5 — Implementar `stability_check` em `analyze_indicator_associations`

```md
Implemente apenas o cálculo de `stability_check` (subjanelas determinísticas) e o preenchimento de `association_stability` conforme [ADR 0015](adrs/0015-estabilidade-em-subjanelas-para-associacoes-stability-check-v1.md), mantendo determinismo e rastreabilidade.

Referências obrigatórias:
- docs/adrs/0015-estabilidade-em-subjanelas-para-associacoes-stability-check-v1.md
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (D2)
- docs/mcp-tool-contract.md
- docs/test-plan.md / docs/contract-test-plan.md (quando o cenário existir)

Arquivos esperados:
- src/LotofacilMcp.Domain/Analytics/
- src/LotofacilMcp.Application/UseCases/
- src/LotofacilMcp.Server/Tools/
- tests/LotofacilMcp.ContractTests/

Regras:
- parâmetros de `stability_check` explícitos; sem defaults semânticos ocultos.

Critério de pronto:
- com `stability_check`, `association_stability` é retornado e determinístico.
```

### Template 25.6 — Expandir `summarize_window_patterns`

```md
Implemente apenas a expansão de `summarize_window_patterns` para múltiplas features escalares prioritárias conforme [ADR 0016](adrs/0016-expansao-de-resumos-de-janela-e-padroes-v1.md), mantendo validação e erros canônicos para casos incompatíveis.

Referências obrigatórias:
- docs/adrs/0016-expansao-de-resumos-de-janela-e-padroes-v1.md
- docs/adrs/0007-agregados-canonicos-de-janela-v1.md (quando a alternativa for agregados)
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Application/UseCases/
- src/LotofacilMcp.Application/Validation/
- tests/LotofacilMcp.ContractTests/

Regras:
- não misturar escalar com vetorial sem agregação explícita.

Critério de pronto:
- features novas aceitas retornam resumo determinístico.
- casos incompatíveis retornam erro canônico com orientação.
```

### Template 25.7 — Geração declarativa + filtros + múltiplas estratégias

```md
Implemente apenas a evolução do contrato de `generate_candidate_games` para suportar critérios/pesos/filtros declarativos e múltiplas estratégias públicas conforme [ADR 0017](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md), retornando `applied_configuration` para auditoria.

Referências obrigatórias:
- docs/adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md
- docs/adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md
- docs/mcp-tool-contract.md
- docs/generation-strategies.md

Arquivos esperados:
- src/LotofacilMcp.Application/UseCases/
- src/LotofacilMcp.Server/Tools/
- tests/LotofacilMcp.ContractTests/

Regras:
- determinismo obrigatório; defaults aplicados devem aparecer em `applied_configuration`.

Critério de pronto:
- é possível gerar candidatos com filtros configuráveis (não apenas explicar).
- ao menos 2 estratégias públicas retornam resultados comparáveis na mesma janela.
```

### Template 25.8 — Pacote de métricas prioritárias (slots/pares/blocos/estabilidade/outlier)

```md
Implemente apenas o pacote de métricas prioritárias definido na [ADR 0018](adrs/0018-pacote-de-metricas-prioritarias-slots-pares-blocos-outliers-v1.md), em etapas pequenas (uma família por vez), garantindo exposição, discovery e testes de contrato para cada métrica promovida.

Referências obrigatórias:
- docs/adrs/0018-pacote-de-metricas-prioritarias-slots-pares-blocos-outliers-v1.md
- docs/metric-catalog.md
- docs/mcp-tool-contract.md
- docs/test-plan.md / docs/contract-test-plan.md

Arquivos esperados:
- src/LotofacilMcp.Domain/Metrics/
- src/LotofacilMcp.Application/Validation/
- src/LotofacilMcp.Server/Tools/
- tests/LotofacilMcp.ContractTests/

Regras:
- atualizar catálogo + testes + código juntos quando a semântica for entregue.
- manter discovery coerente com a implementação real da build.

Critério de pronto:
- cada métrica entregue aparece em `compute_window_metrics` (quando aplicável) e em `discover_capabilities` com status correto.
```

