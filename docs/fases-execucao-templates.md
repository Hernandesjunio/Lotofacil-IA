# Templates atômicos de execução

**Navegação:** [← Brief (índice)](brief.md) · [spec-driven-execution-guide.md](spec-driven-execution-guide.md)

Este documento transforma as fases do [spec-driven-execution-guide.md](spec-driven-execution-guide.md) (numeradas 0 a 20 no guia) em **pedidos atômicos** prontos para uso com IA, preservando o formato normativo de template. A **contagem de fases no guia não é teto** — secções adicionais (a partir da *Fase 21* abaixo) estendem o roteiro quando surgem entregas normativas (ex.: [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md), [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) e [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md), incluindo a [Fase 23](#fase-23-adr-0008-descoberta-janela-por-extremos-e-mapeamento-legado) para o 0008; [Fase 26](#fase-26---adr-0020-flexibilidade-de-geracao-aleatorio-explicito-filtros-opt-in-intersecao-teto-1k-seed-opcional) para o [ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md); [Fase 27](#fase-27---adr-0021-apresentacao-de-resumos-de-janela-tabelas-a-b-glossario-d5) para o [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) (apresentação legível de resumos de janela, sem alterar o JSON do MCP) sem reabrir a numeração fechada do guia; novas fases seguem o **mesmo padrão** (bloco `Implemente apenas…`, referências, arquivos, regras, critério de pronto). A Fase 23 do [guia de execução](spec-driven-execution-guide.md) (secção *Fase 23: Descoberta híbrida…*) e esta secção cobrem a mesma entrega normativa. A Fase 26 do [guia de execução](spec-driven-execution-guide.md) (secção homónima) corresponde à secção *Fase 26* abaixo (ADR 0020). A Fase 27 do [guia de execução](spec-driven-execution-guide.md) (secção homónima) corresponde à secção *Fase 27* abaixo (ADR 0021).

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
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md
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
- o provider deve suportar `Dataset:DrawsSourceUri` (via env var `Dataset__DrawsSourceUri` em .NET) e falhar com `DATASET_UNAVAILABLE` quando a fonte estiver ausente/inválida, com `details` orientando o host;
- nesta fase, o requisito **mínimo e obrigatório** é: aceitar **path local** e **URI `file://`**, e **não ter fallback** quando `Dataset__DrawsSourceUri` estiver ausente (ver ADR 0022 e Subfase 4.0 do guia);
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
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md (quando o recorte incluir ranges/multi-valores)

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

Extensão (somente se no recorte desta entrega):
- suportar restrições flexíveis por `range` e `allowed_values` (ADR 0019) como definição de conjunto válido (não enumeração de valores);
- quando `typical_range` for usado, ecoar `resolved_range` e `coverage_observed` em `applied_configuration.resolved_defaults`;
- se `mode=soft` for suportado, o default e a penalidade devem ser determinísticos e rastreáveis.

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
Implemente apenas a evolução do contrato de `generate_candidate_games` para suportar critérios/pesos/filtros declarativos e múltiplas estratégias públicas conforme [ADR 0017](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md) e as restrições flexíveis (faixa / multi-valor / faixa típica) conforme [ADR 0019](adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md), retornando `applied_configuration` para auditoria.

Referências obrigatórias:
- docs/adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md
- docs/adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md
- docs/mcp-tool-contract.md
- docs/generation-strategies.md

Arquivos esperados:
- src/LotofacilMcp.Application/UseCases/
- src/LotofacilMcp.Server/Tools/
- tests/LotofacilMcp.ContractTests/

Regras:
- determinismo obrigatório; defaults aplicados devem aparecer em `applied_configuration`.
- restrições por `range`/`allowed_values` devem ser tratadas como “região válida”: o servidor retorna qualquer lote determinístico de tamanho `count` que satisfaça as restrições; não enumerar combinações no cliente.
- quando `typical_range` for declarado, ecoar `resolved_range` e `coverage_observed` em `resolved_defaults` (sem inferência silenciosa).
- quando expor orçamento/budget, ecoar contadores de tentativa/aceite/rejeição em `resolved_defaults`.

Critério de pronto:
- é possível gerar candidatos com filtros configuráveis (não apenas explicar).
- ao menos 2 estratégias públicas retornam resultados comparáveis na mesma janela.
```

### Templates 25.7A–25.7F — Passos atômicos (ADR 0019) para implementar faixa/cobertura na geração

#### Template 25.7A — Contrato: `range` / `allowed_values` / `mode` (sem `typical_range` ainda)

```md
Implemente apenas a evolução do request/validator de `generate_candidate_games` para aceitar restrições por `range` e `allowed_values` (e `mode`), mantendo retrocompatibilidade com o modo atual (`value`, `min`, `max`) e rejeitando “modos mistos”.

Referências obrigatórias:
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md (RangeSpec, AllowedValuesSpec, modo hard/soft, regras de erro)
- docs/mcp-tool-contract.md (shape de erro `INVALID_REQUEST`)

Arquivos esperados:
- src/LotofacilMcp.Server/Tools/
- src/LotofacilMcp.Application/Validation/
- tests/LotofacilMcp.ContractTests/

Regras:
- não introduzir heurísticas de geração; apenas parsing/validação/normalização.
- `allowed_values.values` deve ser normalizado (ordenar + deduplicar) e o resultado deve aparecer em `applied_configuration.resolved_defaults`.
- `mode` default `hard` deve aparecer em `resolved_defaults` quando omitido.

Critério de pronto:
- requests antigos continuam válidos.
- request com `value` + `range` (ou qualquer modo misto) falha com `INVALID_REQUEST`.
- `allowed_values` inválido (vazio, não inteiro quando aplicável) falha com `INVALID_REQUEST`.
```

#### Template 25.7B — Resolvedor determinístico de `typical_range` (IQR + percentis)

```md
Implemente apenas o resolvedor determinístico de `typical_range` (métodos `iqr` e `percentile`) sobre a janela declarada, incluindo validação de parâmetros e eco de `resolved_range` + `coverage_observed` + `method_version`.

Referências obrigatórias:
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md (TypicalRangeSpec, regras de params, output de resolução)
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md (determinismo)

Arquivos esperados:
- src/LotofacilMcp.Domain/Generation/ (ou módulo equivalente de resolução determinística)
- src/LotofacilMcp.Application/UseCases/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- método deve ser fechado e versionado (ex.: `method_version="1.0.0"`).
- `coverage_observed` é sempre reportado; não prometer atingir `coverage`.
- erro para `coverage` fora de [0,1] e percentis inválidos: `INVALID_REQUEST`.

Critério de pronto:
- para a mesma janela + input canônico, `resolved_range` é estável.
- testes cobrem `iqr` e `percentile` com fixture pequena.
```

#### Template 25.7C — Integrar `typical_range` no request e ecoar em `resolved_defaults`

```md
Implemente apenas a aceitação de `typical_range` em critérios/filtros de `generate_candidate_games`, chamando o resolvedor determinístico e ecoando `resolved_range`, `coverage_observed` e defaults (`inclusive`, `mode`) em `applied_configuration.resolved_defaults`.

Referências obrigatórias:
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Server/Tools/
- src/LotofacilMcp.Application/UseCases/
- tests/LotofacilMcp.ContractTests/

Regras:
- `deterministic_hash` deve incluir o payload original (`typical_range`) e qualquer `window_ref` quando existir.
- não implementar ainda `mode="soft"` se isso aumentar o escopo; manter `hard` como default explícito.

Critério de pronto:
- um request com `typical_range` válido retorna `applied_configuration.resolved_defaults` com a resolução ecoada.
- métrica desconhecida em `typical_range.metric_name` falha com `UNKNOWN_METRIC`.
```

#### Template 25.7D — Aplicar `range`/`allowed_values` no motor de geração (hard) + orçamento determinístico

```md
Implemente apenas a aplicação de `range`/`allowed_values` como restrições hard no motor de geração (pass/fail), adicionando orçamento determinístico (`max_attempts` e/ou pool multiplier) e contadores ecoados (`attempts_used`, `accepted_count`, `rejected_count_by_reason`).

Referências obrigatórias:
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md (orçamento, contadores, `STRUCTURAL_EXCLUSION_CONFLICT`)
- docs/adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md (applied_configuration)

Arquivos esperados:
- src/LotofacilMcp.Domain/Generation/
- src/LotofacilMcp.Application/UseCases/
- tests/LotofacilMcp.Domain.Tests/
- tests/LotofacilMcp.ContractTests/

Regras:
- `range`/`allowed_values` definem conjunto válido; não enumerar valores no cliente.
- orçamento e contadores devem ser determinísticos para o mesmo request (incl. seed quando aplicável).

Critério de pronto:
- `count` alto funciona com orçamento explícito.
- quando inviável, erro `STRUCTURAL_EXCLUSION_CONFLICT` inclui pista determinística do colapso.
```

#### Template 25.7E — `mode="soft"`: penalidade determinística versionada (opcional e isolado)

```md
Implemente apenas o suporte a `mode="soft"` para um subconjunto pequeno de restrições (1–2 métricas/guardrails), aplicando penalidade determinística versionada e ecoando defaults + penalidades em `resolved_defaults`.

Referências obrigatórias:
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md (hard vs soft, determinismo, eco)
- docs/generation-strategies.md (se a penalidade for específica de estratégia)

Arquivos esperados:
- src/LotofacilMcp.Domain/Generation/
- tests/LotofacilMcp.Domain.Tests/

Regras:
- `soft` nunca pode virar default oculto; `hard` permanece default e aparece em `resolved_defaults`.
- penalidade deve ser canônica (mesma entrada ⇒ mesma penalidade) e versionada.

Critério de pronto:
- requests idênticos produzem o mesmo ranking/seleção sob `soft` (determinístico).
```

#### Template 25.7F — Explicação: pass/fail vs faixa/conjunto (incl. `resolved_range`)

```md
Implemente apenas a evolução de `explain_candidate_games` para reportar, por critério/filtro, o valor observado, a faixa/conjunto aplicado (incl. `resolved_range` quando houver `typical_range`) e o resultado (pass/fail ou penalidade).

Referências obrigatórias:
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md (eco + explicabilidade)
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Server/Tools/
- src/LotofacilMcp.Application/UseCases/
- tests/LotofacilMcp.ContractTests/

Regras:
- não inventar métricas “vetoriais” como restrição; usar apenas features escalares derivadas quando necessário (ex.: `top10_overlap_count`).

Critério de pronto:
- a explicação permite auditar por que um candidato passou/falhou (ou foi penalizado) em cada restrição.
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

## Fase 26 - ADR 0020: flexibilidade de geracao (aleatorio explicito, filtros opt-in, intersecao, teto 1k, seed opcional)

*Extensão pós–Fase 25. Norma: [ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md). Espelha a [Fase 26 do guia](spec-driven-execution-guide.md). Cruzamento: [ADR 0017](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md), [ADR 0019](adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md), [ADR 0002](adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md), [ADR 0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md) (afinamento na rota de geração).*

### Template 26.1 — Contrato: modos de geração, opt-in de exclusões, interseção e semântica de `deterministic_hash`

```md
Implemente apenas a atualização normativa de `docs/mcp-tool-contract.md` e `docs/generation-strategies.md` para fechar: (1) modo de geração explícito (aleatório sem guardrails declarados *vs.* filtrado por comportamento); (2) que `structural_exclusions` e critérios são opt-in conforme ADR 0020; (3) semântica de interseção de restrições; (4) significado de `deterministic_hash` quando `seed` estiver ausente *vs.* presente; (5) campo de resposta para “replay não garantido” (nome canónico a fechar).

Referências obrigatórias:
- docs/adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md
- docs/adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md
- docs/adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md

Arquivos esperados:
- docs/mcp-tool-contract.md
- docs/generation-strategies.md
- (se aplicável) docs/contract-test-plan.md (pistas de teste a adicionar na próxima fatia)

Regras:
- não prometer predição de resultado; linguagem descritiva.
- qualquer default aplicado continua a ter de ser ecoado em `applied_configuration.resolved_defaults` quando a implementação existir.

Critério de pronto:
- o contrato descreve sem ambiguidade os modos, a interseção e a política de `seed`/hash alinhadas ao ADR 0020.
```

### Template 26.2 — Validação: teto de 1000 jogos por pedido (`sum(plan[].count)`)

```md
Implemente apenas validação + erro canónico quando a soma dos `count` em `plan[]` exceder 1000, com mensagem que orienta nova rodada (novo request) para lotes maiores, conforme ADR 0020 D6.

Referências obrigatórias:
- docs/adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Application/Validation/
- tests/LotofacilMcp.ContractTests/

Regras:
- erro determinístico (mesmo request inválido ⇒ mesma resposta de erro).
- não alterar o teto por build silenciosamente: 1000 é norma v1 desta ADR até revisão.

Critério de pronto:
- `sum(count) = 1001` falha sempre com o código/mensagem acordados;
- `sum(count) = 1000` continua válido (se o restante do request for válido).
```

### Template 26.3 — Motor: modos `random_unrestricted` vs `behavior_filtered` (nomes finais do contrato)

```md
Implemente apenas a ramificação do motor de `generate_candidate_games` para respeitar o modo declarado: no modo aleatório explícito, não aplicar defaults conservadores de `structural_exclusions` não solicitados; no modo filtrado, aplicar somente o declarado e ecoar resoluções em `applied_configuration.resolved_defaults`, conforme ADR 0020 D1–D3.

Referências obrigatórias:
- docs/adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md
- docs/adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md
- docs/generation-strategies.md

Arquivos esperados:
- src/LotofacilMcp.Domain/Generation/
- src/LotofacilMcp.Application/UseCases/
- src/LotofacilMcp.Server/Tools/
- tests/LotofacilMcp.ContractTests/ (mínimo: um caso por modo)

Regras:
- interseção de critérios declarados: candidato deve satisfazer todas as restrições (salvo modo documentado em contrário).
- não extrapolar para métricas fora do recorte da ADR 0020.

Critério de pronto:
- teste(s) mostram diferença observável entre “omitir filtros no modo aleatório” *vs.* o comportamento legado de defaults quando o contrato ainda exigir compatibilidade retroativa (se aplicável).
```

### Template 26.4 — `seed` opcional e envelope de resposta (replay garantido ou não)

```md
Implemente apenas a política de `seed` opcional em `generate_candidate_games`: com `seed`, garantir reprodutibilidade da parte estocástica para o mesmo request canónico + dataset; sem `seed`, não garantir replay e preencher campo explícito na resposta; ajustar `deterministic_hash` conforme contrato fechado em Template 26.1. Atualizar testes que assumem `seed` obrigatória onde a nova semântica aplicar.

Referências obrigatórias:
- docs/adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md (D7)
- docs/mcp-tool-contract.md
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md

Arquivos esperados:
- src/LotofacilMcp.Application/UseCases/
- src/LotofacilMcp.Server/Tools/
- tests/LotofacilMcp.ContractTests/

Regras:
- testes “golden” que dependem de candidatos idênticos devem passar a incluir `seed` ou declarar expectativa de não-replay quando `seed` for omitido.

Critério de pronto:
- contrato + testes cobrem presença e ausência de `seed` sem contradição com o envelope JSON.
```

### Template 26.5 — Discovery: `discover_capabilities` e paridade HTTP/MCP

```md
Implemente apenas a atualização de `discover_capabilities` (e paridade HTTP ↔ MCP, se existir) para publicar: teto de 1000 jogos por pedido, obrigatoriedade de `seed` por caminho, modos de geração suportados, e qualquer enum novo exigido pelo contrato fechado em 26.1.

Referências obrigatórias:
- docs/adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md
- docs/adrs/0011-tool-de-discovery-de-capacidades-por-build-v1.md (formato de discovery)

Arquivos esperados:
- src/LotofacilMcp.Server/Tools/ (discovery + registro de capacidades)
- tests/LotofacilMcp.ContractTests/

Regras:
- discovery continua determinística para a mesma build.

Critério de pronto:
- payload de discovery reflete fielmente as novas regras sem drift com validadores.
```

### Template 26.6 — `explain_candidate_games` alinhado a modos e a replay

```md
Implemente apenas a evolução de `explain_candidate_games` para refletir modo de geração, política de `seed`/replay e interseção de restrições quando relevante para a justificativa do candidato, sem alterar métricas fora do escopo do ADR 0020.

Referências obrigatórias:
- docs/adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- src/LotofacilMcp.Server/Tools/
- src/LotofacilMcp.Application/UseCases/
- tests/LotofacilMcp.ContractTests/

Regras:
- não introduzir linguagem preditiva.

Critério de pronto:
- explicação audita modo, restrições efetivas e (quando aplicável) se o episódio foi replayável.
```

## Fase 27 - ADR 0021: apresentacao de resumos de janela (tabelas A-B, glossario, D5)

*Extensão pós–Fase 26. Norma: [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md). Documentação e texto a humanos: **não** muda o envelope `MetricValue` do MCP. Espelha a [Fase 27 do guia](spec-driven-execution-guide.md#fase-27---apresentacao-de-resumos-de-janela-adr-0021) (inclui lista de ficheiros-alvo e 27.3/27.4 detalhados). Cruzamento: [metric-glossary.md](metric-glossary.md), [ADR 0009](adrs/0009-help-e-catalogo-de-templates-resources-v1.md) (D4, help/resources, quando a fatia tocar exemplos de tabela), [metric-catalog.md](metric-catalog.md) — secção *QtdFrequencia* / ponte *quatro papéis* (`#export-legado-qtdfrequencia`), Tabelas 1–2 *normativas* sem reescritura. Âncoras estáveis na cadeia D3 (ver 27.1b).*

**Mapa de mudança (código *vs.* documentação):** ficheiros *docs* (27.1, 27.1b) e, opcional, `AGENTS` / `.cursor/rules` (27.2), `src/.../ComputeWindowMetricsUseCase.cs` (27.3, só `ExplanationFor`), `resources/*` + `src/LotofacilMcp.Server/LotofacilMcp.Server.csproj` (27.4, cópia de *resources* no *build*). Nada disto exige *bump* de *shape* de `MetricValue`.

### Template 27.1 — `metric-glossary`: subsecção “Textos de resumo para tabelas (ADR 0021)”

```md
Implemente apenas a subsecção normativa de **textos de resumo** alinhada ao [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) (templates A e B, D1, D2, Apêndice) em [metric-glossary.md](metric-glossary.md), nome: **“Textos de resumo para tabelas (ADR 0021)”** (alinhado às *Consequências* da ADR): copiar, condensar ou fundir com o bloco *“O que observa”* de cada métrica citada no Apêndice, sem contradição com o [metric-catalog.md](metric-catalog.md).

Referências obrigatórias:
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md
- docs/metric-catalog.md
- (estado actual) docs/metric-glossary.md

Arquivos esperados:
- docs/metric-glossary.md

Regras:
- manter tom descritivo (nada de promessa de acerto futuro);
- cumprir D1: tabela A — coluna **Descrição**; para `estabilidade_ranking` incluir, no mínimo, *sub-janelas consecutivas*, *ordem por frequência*, \([0,1]\) e *não* previsão; tabela B — coluna *“O que esta série indica”* **obrigatória** e vocabulário D2 (entropia, HHI, pares, vizinhos, *etc.*);
- não conflitar com a ADR 0021 D5: resumo padrão *vs.* interpretação longa *sob pedido* (a subsecção fornece o piso; o agente pode alargar quando o utilizador pedir, ancorado no catálogo + dados reais — mais tokens).

Critério de pronto:
- a subsecção existe, está referenciada no sumário se o glossário tiver índice, e cita a ADR 0021;
- pelo menos as métricas do Apêndice da ADR 0021 têm uma linha de “texto de tabela” reutilizável em PT.
```

### Template 27.1b (hotfix documental, opcional) — ponte *Vocabulário* (ausência / frequência / atraso / `ausencia_blocos`)

*Aplica-se quando, após 27.1, o *gap* editorial ainda puder fazer o leitor confundir: «*ausente *N* concursos*» (vector 0,1,2,…) *vs.* `frequencia_por_dezena` (somas) *vs.* `ausencia_blocos` (listas). Não altera fórmulas nem o MCP.*

```md
Implemente apenas a **ponte de vocabulário** e referências cruzadas entre: (1) a secção *QtdFrequencia* (ponte *quatro papéis*, âncora `#export-legado-qtdfrequencia` em [metric-catalog.md](metric-catalog.md)); (2) o D3, o Apêndice (tabela A, nota *Ausência* — âncoras no texto da ADR, ex. `#apendice-frases-modelo-pt-adr-0021`, `#nota-ausencia-adr-0021`) e ligação ao glossário; (3) a secção e âncora `#vocab-ausencia-adr-0021` em [metric-glossary.md](metric-glossary.md), sem reabrir a semântica da Tabela 2 do catálogo (definição fechada = *Tabelas* 1 e 2 do *metric-catalog*, ligação por *link* apenas).

Referências obrigatórias:
- docs/metric-catalog.md
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md
- docs/metric-glossary.md

Arquivos esperados (conforme *gap*):
- docs/metric-catalog.md
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md
- docs/metric-glossary.md

Regras:
- nenhuma alteração a `MetricValue`, contrato MCP, nem a fórmula canónica das métricas; apenas texto, âncoras e tabelas de explicação;
- `QtdFrequencia` continua a mapear **só** `frequencia_por_dezena`; a ponte deixa claro o sentido *leigo* de “ausência (N concursos sem sair)” ≈ vector de atraso / `estado_atual_dezena` *vs.* contagem de *saídas* (frequência) e *vs.* `ausencia_blocos` — **não** renomear métricas;
- os ids `vocab-ausencia-adr-0021` (e homólogos `apendice-…` / `nota-ausencia-…` no apêndice da ADR) permanecem **estáveis** para ligação a partir da ADR e do catálogo.

Critério de pronto:
- catálogo, ADR (apêndice/D3) e glossário não contradizem-se sobre os quatro *papéis* (frequência na janela; atraso/estado; `ausencia_blocos`; distinção explícita em tabela);
- links internos (âncora `#vocab-ausencia-adr-0021`) resolvam no *preview* do repositório.
```

### Template 27.2 — `AGENTS.md` e regras do host: confirmar D5 (resumo *vs.* interpretação, tokens)

```md
Implemente apenas a confirmação/ajuste de [AGENTS.md](../AGENTS.md) (e de `.cursor/rules` ou equipe, se existirem) para explicitar: (1) tabelas A/B conforme ADR 0021; (2) modo **resumo padrão** (poucos tokens) *vs.* **interpretação explícita** quando o utilizador pedir (mais tokens, dados ancorados); (3) o glossário/ADR 0021 como fonte, não invenção de fórmulas.

Referências obrigatórias:
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md
- AGENTS.md

Arquivos esperados:
- AGENTS.md
- (opcional) .cursor/rules/*.md

Regras:
- uma linha ou parágrafo no mapa/atalhos basta, sem duplicar a ADR inteira.

Critério de pronto:
- alguém que lê AGENTS sabe que a apresentação de resultados de janela tem norma (ADR 0021) e trade-off de tokens;
- link para a ADR 0021 visível.
```

### Template 27.3 (opcional) — `ComputeWindowMetricsUseCase` / explicações por métrica no servidor

*Estado de referência: `ExplanationFor` em [ComputeWindowMetricsUseCase.cs](../src/LotofacilMcp.Application/UseCases/ComputeWindowMetricsUseCase.cs) mapeia cada métrica canónica conhecida; a string “Metrica de janela.” aplica-se **só** ao `default` (métrica desconhecida ou fora do `switch`). 27.3 cobre: novas chaves a acrescentar ao *switch*; retoques de *wording* técnico↔glossário (não confundir com tabelas A/B); ou testes *golden* de *explanation*.*

```md
Implemente apenas o enriquecimento de `ExplanationFor` (método privado) no [ComputeWindowMetricsUseCase.cs](src/LotofacilMcp.Application/UseCases/ComputeWindowMetricsUseCase.cs) — campo `explanation` que acompanha cada `MetricValue` no fluxo de `compute_window_metrics` (ver contrato), **(a)** para métricas que ainda caiam no *default* (genérico “Metrica de janela.”), e/ou **(b)** para aproximar, sem substituir o registo A/B, o texto a um piso mínimo coerente com [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) e [metric-glossary.md](metric-glossary.md). **Não** alterar `MetricValue.value` nem a semântica de cálculo.

Referências obrigatórias:
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md
- docs/metric-glossary.md
- src/LotofacilMcp.Application/UseCases/ComputeWindowMetricsUseCase.cs (ou ficheiro actual se o *use case* tiver migrado de pasta)

Arquivos esperados:
- src/LotofacilMcp.Application/UseCases/ComputeWindowMetricsUseCase.cs
- (testes) tests/ — apenas se o contrato fixar comparação literal de *explanation* (cuidado: mudança de string pode exigir golden / JSON snapshot)

Regras:
- strings **curtas** (consumidor de JSON, não tabela A/B com quatro colunas);
- nada de linguagem preditiva; não introduzir defaults semânticos novos;

Critério de pronto:
- nenhum teste de fórmula/valor quebre; ajustar goldens de *explanation* de forma consciente se a suíte comparar literalmente a string.
```

### Template 27.4 (opcional) — `resources/help` e `resources/prompts` (exemplos de tabela A/B)

```md
Implemente apenas a revisão de exemplos em [resources/help/](resources/help/) e [resources/prompts/](resources/prompts/) para alinhar, onde houver tabela de métricas *para leigos*, às colunas A/B da ADR 0021: sem coluna redundante *shape*/*unit* em resumos narrativos; ligação ao *getting-started* / índice conforme [ADR 0009](adrs/0009-help-e-catalogo-de-templates-resources-v1.md).

Referências obrigatórias:
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md
- docs/adrs/0009-help-e-catalogo-de-templates-resources-v1.md
- resources/help/
- resources/prompts/

Arquivos esperados:
- resources/help/getting-started@1.0.0.md (e outros tocados)
- resources/prompts/index@1.0.0.md, prompts analíticos (ex. `ranking-stability@1.0.0`, `frequency-vs-delay@1.0.0`, *etc.*, conforme tabelas exemplo)
- src/LotofacilMcp.Server/LotofacilMcp.Server.csproj (confirmar *Content* que copia `../../resources/...` para o *output*; não *duplicar* norma noutro sítio)
- (opcional) ficheiros que *embedam* o caminho a `resources/` em runtime, ex. [HelpCatalog.cs](src/LotofacilMcp.Server/Helping/HelpCatalog.cs)

Regras:
- Markdown only; *resources* read-only; não expor N mágico de janela;
- reforçar o opt-in a “explica o comportamento” = mais detalhe (D5) quando fizer sentido;
- alinhar a *Fase* aos *Abertos* da [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) (revisão voluntária *help*/*prompts*).

Critério de pronto:
- nenhum exemplo de tabela do resource contradiz a ADR 0021;
- cópia embebida no server actualizada se o repositório duplicar *resources* no output de build.
```

### Template 27.5 — Checklist de cobertura (métricas do Apêndice da ADR 0021)

```md
Implemente apenas um checklist de cobertura (sem criar métricas novas) para garantir que **todas** as métricas citadas no **Apêndice** do [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) aparecem com texto reutilizável na secção **“Textos de resumo para tabelas (ADR 0021)”** do [metric-glossary.md](metric-glossary.md) (tabela A e tabela B) e, quando houver `explanation` no servidor, não ficam presas ao texto genérico “Metrica de janela.”.

Referências obrigatórias:
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md (Apêndice, D1–D2, D5)
- docs/metric-glossary.md (secção “Textos de resumo para tabelas (ADR 0021)”)
- docs/spec-driven-execution-guide.md (Fase 27.5)

Arquivos esperados:
- docs/metric-glossary.md (ajuste só se faltar alguma linha de texto)
- (opcional) src/LotofacilMcp.Application/UseCases/ComputeWindowMetricsUseCase.cs (somente se alguma métrica cair no default do `ExplanationFor`)

Checklist (lista nominal do Apêndice; não inventar outras):
- Tabela A: `estabilidade_ranking`, `frequencia_por_dezena`, `total_de_presencas_na_janela_por_dezena`, `sequencia_atual_de_presencas_por_dezena`, `atraso_por_dezena`, `estado_atual_dezena`, `top10_mais_sorteados`, `top10_menos_sorteados`, `top10_maiores_totais_de_presencas_na_janela`, `top10_menores_totais_de_presencas_na_janela`
- Tabela B: `entropia_linha_por_concurso`, `entropia_coluna_por_concurso`, `hhi_linha_por_concurso`, `hhi_coluna_por_concurso`, `repeticao_concurso_anterior`, `pares_no_concurso`, `quantidade_vizinhos_por_concurso`, `sequencia_maxima_vizinhos_por_concurso`, `distribuicao_linha_por_concurso`, `distribuicao_coluna_por_concurso`

Regras:
- cumprir ADR 0021 D1: coluna “Descrição” (A) e “O que esta série indica” (B) são obrigatórias quando a métrica aparecer em tabela humana;
- cumprir ADR 0021 D2: vocabulário acessível mínimo para entropia/HHI/pares/vizinhos;
- cumprir ADR 0021 D5: resumo padrão por defeito; interpretação longa só sob pedido explícito;
- não alterar fórmulas nem versões (isso é `metric-catalog.md`); se detectar lacuna semântica, registrar como dúvida em vez de improvisar.

Critério de pronto:
- todas as métricas do Apêndice têm um texto reutilizável em PT no glossário (tabela A e B);
- nenhuma métrica do Apêndice fica sem texto (lacuna) ou com texto que contradiga o catálogo;
- (se aplicável) `ExplanationFor` não devolve a string genérica para métricas já canônicas e citadas no Apêndice.
```

## Fase 28 — Implementar métricas canônicas pendentes do catálogo (execução dirigida por plano)

*Extensão pós–Fase 27. Norma: [metric-catalog.md](metric-catalog.md) (Tabelas 1 e 2) + contrato MCP quando a métrica for exposta por tool. Esta fase existe para evitar implementação avulsa: o trabalho é executado por templates atômicos, sempre na ordem spec → teste → código.*

### Template 28.1 — Auditoria: catálogo (Tabela 1) × superfície real (build) e lista de lacunas

```md
Implemente apenas uma auditoria determinística (sem codar métricas novas) que produza a lista das métricas **canônicas** do [metric-catalog.md](metric-catalog.md) (Tabela 1) e marque, para esta build, quais estão **implementadas/expostas** vs **faltantes**, com referência explícita ao mecanismo de registro/dispatcher/discovery do repositório.

Referências obrigatórias:
- docs/metric-catalog.md (Tabela 1 — Identificação e tipagem; Status=canonica)
- docs/mcp-tool-contract.md (quando a auditoria depender de rota/tool exposta)
- docs/spec-driven-execution-guide.md (Fase 28 — sequência obrigatória)

Arquivos esperados:
- tests/ (novo teste ou utilitário de validação, conforme convenção do repositório)
- (opcional) docs/ (curta evidência da lista de lacunas, se o repositório exigir)

Regras:
- não considerar “implementada” só porque existe arquivo/classe: precisa estar registrada/descoberta e/ou despachável no recorte alvo;
- se a build operar por allowlist, a auditoria deve reportar o subconjunto permitido por rota;
- não inventar métricas fora da Tabela 1.

Critério de pronto:
- a auditoria falha se existir métrica canônica não implementada no recorte escolhido (ou produz lista explícita de faltantes, conforme decisão do template).
```

### Template 28.2 — Lote pequeno: testes vermelhos (domínio + contrato) para métricas faltantes

```md
Implemente apenas os testes (vermelhos primeiro) para um **lote pequeno** (2–5) de métricas canônicas marcadas como faltantes no Template 28.1, cobrindo fórmula, bordas e tipagem (`scope`, `shape`, `unit`, `version`) conforme o catálogo/contrato.

Referências obrigatórias:
- docs/metric-catalog.md (Tabela 2 — Semântica das métricas do lote)
- docs/test-plan.md e docs/contract-test-plan.md (quando existirem casos/goldens aplicáveis)
- docs/mcp-tool-contract.md (se a métrica for exposta por tool)

Arquivos esperados:
- tests/LotofacilMcp.Domain.Tests/
- (quando exposta) tests/LotofacilMcp.ContractTests/
- tests/fixtures/ (se precisar fixture mínima adicional)

Regras:
- não escrever implementação antes dos testes falharem pelo motivo correto;
- quando o catálogo remeter a ADR (ex.: bordas/length), citar o trecho aplicável no teste.

Critério de pronto:
- testes falham antes do código e descrevem o comportamento normativo com asserts objetivos.
```

### Template 28.3 — Lote pequeno: implementação mínima + registro/dispatcher + discovery (após 28.2)

```md
Implemente apenas a implementação mínima das métricas do lote (do Template 28.2) no `Domain`, mais o registro necessário (dispatcher/registro único/allowlist) para que elas sejam despacháveis e descobertas na build, até os testes passarem.

Referências obrigatórias:
- docs/metric-catalog.md (Tabela 1: nome/scope/shape/unit/version; Tabela 2: fórmula)
- docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md (determinismo)
- docs/adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md (se tocar em allowlist/erro `UNKNOWN_METRIC` com `details`)

Arquivos esperados:
- src/LotofacilMcp.Domain/Metrics/
- src/LotofacilMcp.Application/ (validação/registro se aplicável)
- src/LotofacilMcp.Server/Tools/ (se a rota precisar expor)

Regras:
- não mudar catálogo para “acomodar” implementação: código deve seguir o spec;
- se a métrica tiver texto humano (glossário/resources), alinhar com ADR 0021 (tabelas A/B) sem mexer no payload JSON.

Critério de pronto:
- testes do Template 28.2 passam;
- `discover_capabilities` (ou mecanismo equivalente) reflete a métrica como implementada;
- a auditoria do Template 28.1 não lista mais a métrica como faltante.
```

### Template 28.4 — Gate final: “catálogo completo implementado” (objetivo principal)

```md
Implemente apenas um gate de validação final que falhe se houver qualquer métrica `Status=canonica` no [metric-catalog.md](metric-catalog.md) sem implementação/registro na build alvo, e que gere uma lista determinística das faltantes para o próximo lote (loop 28.2–28.3).

Referências obrigatórias:
- docs/metric-catalog.md (Tabela 1)
- docs/spec-driven-execution-guide.md (Fase 28.6 — validação final)

Arquivos esperados:
- tests/ (validação automatizada rodando no pipeline local/CI)

Regras:
- o gate não pode ser “manual”: deve ser verificável por teste.

Critério de pronto:
- executar a suíte resulta em “zero métricas canônicas faltantes” para a build alvo.
```

## Extensão (hotfix pós-V0) — Métricas autoexplicativas + sunset (compatibilidade temporária)

*Objetivo:* introduzir métricas com nomes não ambíguos para (1) **total de presenças na janela** e (2) **sequência atual com reinício ao ausente**, mantendo compatibilidade com nomes antigos por um período definido e documentando comportamento pós-sunset via `UNKNOWN_METRIC` com `details` de migração.

### Template H1 — Fechar spec (catálogo + ADR 0021 + glossário) para as métricas novas

```md
Implemente apenas o fechamento coordenado de specs para as métricas autoexplicativas:
- `total_de_presencas_na_janela_por_dezena@1.0.0`
- `sequencia_atual_de_presencas_por_dezena@1.0.0`
- `top10_maiores_totais_de_presencas_na_janela@1.0.0`
- `top10_menores_totais_de_presencas_na_janela@1.0.0`

Referências obrigatórias:
- docs/metric-catalog.md (Tabelas 1 e 2; política de deprecação e sunset)
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md (tabela A, frases modelo)
- docs/metric-glossary.md (definição + “O que observa” + textos de tabela)

Arquivos esperados:
- docs/metric-catalog.md
- docs/metric-glossary.md
- docs/adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md

Regras:
- não renomear métricas antigas nesta etapa; apenas introduzir as novas e preparar migração.
- `total_de_presencas_na_janela_por_dezena` deve ser explicitamente equivalente a `frequencia_por_dezena` na mesma janela.
- `sequencia_atual_de_presencas_por_dezena` deve declarar explicitamente “reinicia ao ausente”.

Critério de pronto:
- catálogo tem fórmula/regra fechada para as 4 métricas.
- ADR 0021 e glossário têm texto humano coerente com o catálogo.
```

### Template H2 — Testes vermelhos (fórmula + equivalência + ties) para as métricas novas

```md
Implemente apenas os testes (vermelhos primeiro) para as métricas novas e suas propriedades:

Referências obrigatórias:
- docs/test-plan.md (matriz de cobertura por métrica)
- docs/contract-test-plan.md (goldens e organização)
- docs/metric-catalog.md (Tabelas 1 e 2)

Arquivos esperados:
- tests/LotofacilMcp.Domain.Tests/ (fórmula)
- tests/LotofacilMcp.ContractTests/ (contrato, se exposto em `compute_window_metrics`)
- tests/fixtures/ (fixture sintética pequena; opcional: tie_heavy)

Regras:
- `sequencia_atual_de_presencas_por_dezena` deve ter asserts que provem reinício ao ausente.
- deve existir teste de equivalência 1:1: `total_de_presencas_na_janela_por_dezena == frequencia_por_dezena` na mesma janela/fixture.
- novos top10 devem validar desempate por dezena asc quando houver empate.

Critério de pronto:
- testes falham antes da implementação e passam apenas quando as regras do catálogo forem satisfeitas.
```

### Template H3 — Contrato de migração (sunset): `UNKNOWN_METRIC` com `details` de substituição

```md
Implemente apenas o ajuste de contrato e testes para suportar sunset de nomes deprecated:

Referências obrigatórias:
- docs/mcp-tool-contract.md (`UNKNOWN_METRIC` e `details`)
- docs/metric-catalog.md (política de deprecação e sunset; data)
- docs/contract-test-plan.md (caso negativo de sunset)

Arquivos esperados:
- docs/mcp-tool-contract.md
- docs/contract-test-plan.md
- tests/LotofacilMcp.ContractTests/ (teste negativo)

Regras:
- após a data de sunset, nomes deprecated podem falhar com `UNKNOWN_METRIC`.
- `details` deve incluir `replacement_metric_name` e `sunset_date` (ISO-8601) quando o erro for por sunset (não por allowlist).

Critério de pronto:
- teste negativo de sunset passa com payload determinístico de erro.
```

