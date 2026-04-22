# Templates atômicos - Fases 0 a 20

Este documento transforma todas as fases do `docs/spec-driven-execution-guide.md` (0 a 20) em pedidos atômicos prontos para uso com IA, preservando o formato normativo de template.

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
