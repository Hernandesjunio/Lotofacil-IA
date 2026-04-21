# Guia de Execução Spec-Driven

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

## Objetivo

Transformar a documentação normativa do repositório em uma sequência prática de execução para implementação com IA, mantendo:

- semântica fechada;
- determinismo forte;
- contrato auditável;
- TDD como regra de trabalho;
- passos atômicos que cabem em janelas de contexto distintas.

Este guia não substitui os specs. Ele define **como executar** a partir deles.

## O que significa spec-driven aqui

Neste repositório, **spec-driven** significa:

1. a fonte de verdade vem primeiro em `docs/`;
2. cada implementação nasce de um recorte explícito do spec;
3. cada recorte precisa de teste correspondente;
4. código sem referência clara a um spec é suspeito;
5. mudança semântica exige atualização coordenada de documentação, testes e implementação.

Referências normativas:

- [brief.md](brief.md)
- [metric-catalog.md](metric-catalog.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [generation-strategies.md](generation-strategies.md)
- [test-plan.md](test-plan.md)
- [vertical-slice.md](vertical-slice.md)
- [contract-test-plan.md](contract-test-plan.md)
- [ADR 0003](adrs/0003-processo-desenvolvimento-bmad-vs-spec-driven.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

## Regra operacional principal

Antes de pedir implementação para a IA, responda quatro perguntas:

1. **Qual spec estou materializando?**
2. **Qual arquivo ou conjunto mínimo de arquivos isso deve tocar?**
3. **Qual teste prova que este recorte está correto?**
4. **O passo cabe sozinho em uma PR / janela de contexto?**

Se alguma resposta não estiver clara, o passo ainda não está pronto para execução.

## Ordem correta de trabalho

### Fase 0 — Congelar a base

Objetivo: não começar implementação em cima de arquitetura ou semântica em aberto.

Passos atômicos:

- Confirmar que a arquitetura vigente é a do [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md).
- Confirmar que a stack base é C# / .NET 10 em [brief.md](brief.md) e [vertical-slice.md](vertical-slice.md).
- Confirmar que a V0 vigente é a de [vertical-slice.md](vertical-slice.md).
- Confirmar que o contrato vigente é [mcp-tool-contract.md](mcp-tool-contract.md).
- Confirmar que a cobertura inicial obrigatória é a de [contract-test-plan.md](contract-test-plan.md).

Critério mínimo de aceite:

- nenhum ponto estrutural crítico da V0 pode estar pendente ou contraditório.

### Fase 1 — Preparar o esqueleto mínimo do repositório

Objetivo: criar apenas o necessário para compilar, testar e organizar a V0 sem antecipar estrutura que ainda não gera valor.

Passos atômicos:

- Criar `LotofacilMcp.sln`.
- Criar `Directory.Build.props`.
- Criar `src/LotofacilMcp.Domain/LotofacilMcp.Domain.csproj`.
- Criar `src/LotofacilMcp.Application/LotofacilMcp.Application.csproj`.
- Criar `src/LotofacilMcp.Infrastructure/LotofacilMcp.Infrastructure.csproj`.
- Criar `src/LotofacilMcp.Server/LotofacilMcp.Server.csproj`.
- Criar as suítes mínimas para a V0: `tests/LotofacilMcp.Domain.Tests/`, `tests/LotofacilMcp.ContractTests/` e `tests/fixtures/`.
- Adiar `IntegrationTests` e `E2E` até existir superfície suficiente para validá-los de forma útil.

Referências:

- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)
- [project-guide.md](project-guide.md)

Critério mínimo de aceite:

- a solution compila vazia;
- as referências entre projetos refletem as fronteiras definidas;
- não há projeto/pasta criado apenas por cenografia.

### Fase 2 — Preparar fixture mínima e testes vermelhos da V0

Objetivo: escrever primeiro os testes que materializam a fatia vertical mínima por dentro e por fora.

Passos atômicos:

- Criar `tests/fixtures/synthetic_min_window.json` conforme [contract-test-plan.md](contract-test-plan.md).
- Escrever teste da barreira de normalização com `Draw` de entrada potencialmente não normalizado.
- Escrever teste de resolução de janela.
- Escrever teste de fórmula de `frequencia_por_dezena@1.0.0`.
- Escrever teste de propriedade da soma `15 × window_size`.
- Escrever teste negativo de `compute_window_metrics` sem `metrics`.
- Escrever teste negativo de métrica desconhecida com `UNKNOWN_METRIC`.
- Escrever teste de ordenação de `get_draw_window`.

Referências:

- [vertical-slice.md](vertical-slice.md)
- [contract-test-plan.md](contract-test-plan.md)
- [test-plan.md](test-plan.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [metric-catalog.md](metric-catalog.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

Critério mínimo de aceite:

- os testes falham pelo motivo esperado antes da implementação;
- já existe pelo menos um teste explícito para a barreira canônica de normalização.

### Fase 3 — Materializar o núcleo canônico mínimo

Objetivo: implementar apenas o necessário para o domínio suportar a V0 com semântica fechada.

Passos atômicos:

- Criar `Domain/Models/Draw`.
- Criar `Domain/Models/Window`.
- Criar `Domain/Normalization/` com a barreira canônica de `Draw`.
- Criar `Domain/Windows/` com a regra de resolução de janela.
- Criar `Domain/Metrics/` com `frequencia_por_dezena@1.0.0`.
- Criar apenas erros/invariantes semânticos que realmente pertençam ao domínio; não mover para `Domain` códigos de contrato que são responsabilidade de `Server`/`Application`.

Referências:

- [metric-catalog.md](metric-catalog.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [ADR 0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

Critério mínimo de aceite:

- os testes de normalização, janela e fórmula da V0 passam sem depender de transporte HTTP;
- a métrica retorna `scope`, `shape`, `unit` e `version` coerentes com o catálogo.

### Fase 4 — Materializar infraestrutura determinística mínima

Objetivo: ler fixture, versionar dataset e implementar determinismo técnico sem invadir a semântica do núcleo.

Passos atômicos:

- Criar provider de fixture em `Infrastructure/Providers/`.
- Criar implementação de `dataset_version` em `Infrastructure/DatasetVersioning/`.
- Criar implementação de JSON canônico em `Infrastructure/CanonicalJson/`.
- Criar implementação de hashing SHA-256.
- Escrever teste explícito de estabilidade de `dataset_version` para o mesmo snapshot.
- Escrever teste explícito de estabilidade de `deterministic_hash` para o mesmo input canônico.

Referências:

- [mcp-tool-contract.md](mcp-tool-contract.md)
- [brief.md](brief.md)
- [ADR 0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

Critério mínimo de aceite:

- o mesmo snapshot gera o mesmo `dataset_version`;
- o mesmo input canônico gera o mesmo hash;
- a política de determinismo já é verificável antes do host HTTP.

### Fase 5 — Materializar casos de uso da V0

Objetivo: orquestrar a V0 sem mover regra estatística para fora do domínio.

Passos atômicos:

- Criar `Application/UseCases/GetDrawWindowUseCase`.
- Criar `Application/UseCases/ComputeWindowMetricsUseCase`.
- Criar validações cross-field da V0 em `Application/Validation/`.
- Criar mapping interno de request para objetos do domínio.

Referências:

- [vertical-slice.md](vertical-slice.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

Critério mínimo de aceite:

- os casos de uso resolvem a janela correta;
- `ComputeWindowMetricsUseCase` consegue produzir `MetricValue` tipado sem depender de lógica no `Server`;
- `dataset_version`, `tool_version` e insumos do `deterministic_hash` já ficam disponíveis para a camada de entrega.

### Fase 6 — Preparar testes de contrato da V0

Objetivo: explicitar o contrato mínimo que a primeira superfície pública deve cumprir.

Passos atômicos:

- Escrever teste de envelope mínimo de resposta com `dataset_version`, `tool_version` e `deterministic_hash`.
- Escrever teste de `MetricValue` para `frequencia_por_dezena` com `metric_name`, `scope`, `shape`, `unit`, `version`, `window`, `value` e `explanation`.
- Escrever teste do shape de erro para `UNKNOWN_METRIC`.
- Escrever teste de contrato para `compute_window_metrics` sem `metrics` com `INVALID_REQUEST`.

Referências:

- [vertical-slice.md](vertical-slice.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [contract-test-plan.md](contract-test-plan.md)

Critério mínimo de aceite:

- a V0 já tem contrato mínimo descrito em testes objetivos;
- nenhuma obrigação do envelope básico fica implícita.

### Fase 7 — Materializar o servidor HTTP/MCP da V0

Objetivo: expor a V0 sem colocar cálculo no host e sem deixar metadados contratuais para depois.

Passos atômicos:

- Criar `Server/Program.cs`.
- Criar `Server/DependencyInjection/`.
- Criar `Server/Tools/` ou endpoints equivalentes para `get_draw_window` e `compute_window_metrics`.
- Implementar binding e validação estrutural no `Server`.
- Implementar serialização de erros conforme o contrato.
- Implementar o envelope mínimo de resposta com `dataset_version`, `tool_version` e `deterministic_hash`.
- Adicionar toggles operacionais de acesso como desligados por padrão.

Referências:

- [mcp-tool-contract.md](mcp-tool-contract.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)
- [project-guide.md](project-guide.md)

Critério mínimo de aceite:

- a V0 responde pelos endpoints/tools previstos;
- os testes de contrato do envelope mínimo passam;
- `get_draw_window` retorna concursos em ordem crescente;
- auth/throttle/quota ficam explicitamente desligados, não omitidos por acidente.

### Fase 8 — Fechar a V0 por evidência

Objetivo: encerrar a primeira fatia vertical com rastreabilidade objetiva.

Passos atômicos:

- Rodar testes de domínio da V0.
- Rodar testes de contrato da V0.
- Rodar os testes mínimos de integração da V0.
- Confirmar que os critérios obrigatórios de [vertical-slice.md](vertical-slice.md) estão cobertos por testes.
- Confirmar que a documentação permanece alinhada ao comportamento observado.

Critério mínimo de aceite:

- a V0 está verde e rastreável por testes;
- a barreira de normalização, o envelope contratual mínimo e o determinismo já estão cobertos antes da próxima fatia.

## Como pedir implementação para IA

O erro mais comum em fluxo spec-driven é pedir “implemente a V1” ou “crie o MCP”. Isso é amplo demais.

O pedido correto para IA deve ter:

1. **objetivo único**;
2. **arquivos-alvo**;
3. **specs de referência**;
4. **teste esperado**;
5. **critério de pronto**.

### Template de pedido atômico

```md
Implemente apenas <passo único>.

Referências obrigatórias:
- <spec 1>
- <spec 2>

Arquivos esperados:
- <arquivo A>
- <arquivo B>

Regras:
- não extrapolar além da V0;
- manter TDD;
- não criar camadas fora do ADR 0004;
- seguir nomes canônicos do catálogo/contrato.

Critério de pronto:
- <teste X passa>
- <erro Y é emitido>
- <payload Z contém campos obrigatórios>
```

## Como saber se um passo está atômico o suficiente

Um passo está atômico quando:

- altera um conceito por vez;
- pode ser validado por um teste ou pequeno grupo de testes;
- não exige discutir dois temas arquiteturais ao mesmo tempo;
- cabe em uma revisão humana curta;
- não força a IA a “adivinhar” detalhes fora do spec citado.

Se o passo mexe simultaneamente em:

- arquitetura + domínio + transporte;
- múltiplas tools;
- múltiplas métricas não relacionadas;
- validação + geração + observabilidade,

então ele ainda está grande demais.

## Regra de ouro para evolução após a V0

Depois da V0, cada nova entrega deve seguir sempre esta ordem operacional:

1. escolher a próxima fatia;
2. localizar os specs normativos;
3. escrever/ajustar testes;
4. implementar domínio;
5. implementar orquestração;
6. expor no server;
7. validar contrato;
8. atualizar docs se a semântica tiver mudado.

Além dessa ordem operacional, a progressão de conteúdo deve ir do mais simples para o mais complexo:

1. métricas base por janela e de fórmula fechada;
2. métricas por transformação derivadas diretamente das bases;
3. séries escalares simples por concurso;
4. métricas de estabilidade sobre séries escalares;
5. vetores e séries vetoriais com agregação explícita;
6. composição declarativa, associações e padrões;
7. métricas de `candidate_game` mais simples;
8. métricas e estratégias mais sensíveis, como `slot`, `outlier` e perfis compostos.

Ordem prática recomendada para evolução do catálogo:

1. `frequencia_por_dezena`
2. `top10_mais_sorteados` e `top10_menos_sorteados`
3. `repeticao_concurso_anterior`, `pares_no_concurso`, `quantidade_vizinhos_por_concurso`
4. `media_janela`, `desvio_padrao_janela`, `mad_janela`, `madn_janela`, `tendencia_linear`
5. distribuições e séries vetoriais (`distribuicao_*`, `entropia_*`, `hhi_*`)
6. `analyze_indicator_stability`, `compose_indicator_analysis`, `analyze_indicator_associations`, `summarize_window_patterns`
7. métricas simples de `candidate_game`
8. `matriz_numero_slot`, `analise_slot`, `surpresa_slot`, `outlier_score`, `generate_candidate_games` e perfis compostos

## Quando criar novos documentos

Criar novo documento apenas quando houver pergunta concreta que os atuais não respondem.

### Criar ADR quando:

- houver decisão estrutural;
- a reversão for cara;
- a decisão afetar múltiplos componentes.

### Criar ou atualizar guia operacional quando:

- a equipe não souber a ordem de execução;
- a dúvida for de processo e não de semântica;
- o spec-driven estiver correto, mas difícil de operar.

### Não criar documento novo quando:

- o que falta é só implementar um item já fechado;
- o problema cabe em ajuste pequeno de documento existente;
- a dúvida é apenas sobre sequência de execução e já existe seção adequada para isso.

## Checklist de início da execução

- [ ] Arquitetura congelada no [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)
- [ ] V0 confirmada em [vertical-slice.md](vertical-slice.md)
- [ ] Ordem de teste confirmada em [contract-test-plan.md](contract-test-plan.md)
- [ ] Métrica inicial confirmada em [metric-catalog.md](metric-catalog.md)
- [ ] Contrato inicial confirmado em [mcp-tool-contract.md](mcp-tool-contract.md)
- [ ] Estrutura de projetos confirmada em [project-guide.md](project-guide.md)
- [ ] Fixture mínima definida
- [ ] Teste explícito da barreira de normalização escrito
- [ ] Primeiro teste negativo escrito
- [ ] Primeiro teste de fórmula escrito
- [ ] Primeiro teste do envelope mínimo (`dataset_version`, `tool_version`, `deterministic_hash`) escrito
- [ ] Primeiro teste de determinismo escrito

## Recomendação prática

Se houver dúvida sobre “qual passo pedir agora para a IA”, comece sempre por este recorte:

1. fixture mínima;
2. testes vermelhos de normalização, janela e fórmula;
3. tipos canônicos do domínio;
4. métrica `frequencia_por_dezena`;
5. provider de fixture + `dataset_version` + `canonical_json` + hash determinístico;
6. caso de uso;
7. teste de contrato do envelope mínimo;
8. tool/endpoint;
9. fechamento da V0.

Essa ordem é a forma prática de usar spec-driven neste projeto: **spec → teste → implementação mínima → validação → próxima fatia**.
