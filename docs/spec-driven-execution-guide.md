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

### Fase 1 — Preparar o repositório para execução

Objetivo: criar o mínimo necessário para compilar, testar e organizar a V0.

Passos atômicos:

- Criar `LotofacilMcp.sln`.
- Criar `Directory.Build.props`.
- Criar `src/LotofacilMcp.Domain/LotofacilMcp.Domain.csproj`.
- Criar `src/LotofacilMcp.Application/LotofacilMcp.Application.csproj`.
- Criar `src/LotofacilMcp.Infrastructure/LotofacilMcp.Infrastructure.csproj`.
- Criar `src/LotofacilMcp.Server/LotofacilMcp.Server.csproj`.
- Criar `tests/LotofacilMcp.Domain.Tests/`.
- Criar `tests/LotofacilMcp.Application.Tests/`.
- Criar `tests/LotofacilMcp.ContractTests/`.
- Criar `tests/LotofacilMcp.IntegrationTests/`.
- Criar `tests/LotofacilMcp.E2E.Tests/`.
- Criar `tests/fixtures/`.

Referências:

- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)
- [project-guide.md](project-guide.md)

Critério mínimo de aceite:

- a solution compila vazia;
- as referências entre projetos refletem as fronteiras definidas.

### Fase 2 — Preparar a V0 por TDD

Objetivo: escrever primeiro os testes que materializam a fatia vertical mínima.

Passos atômicos:

- Criar `tests/fixtures/synthetic_min_window.json` conforme [contract-test-plan.md](contract-test-plan.md).
- Escrever teste negativo de `compute_window_metrics` sem `metrics`.
- Escrever teste negativo de métrica desconhecida com `UNKNOWN_METRIC`.
- Escrever teste de ordenação de `get_draw_window`.
- Escrever teste de fórmula de `frequencia_por_dezena@1.0.0`.
- Escrever teste de propriedade da soma `15 × window_size`.
- Escrever teste de determinismo para `deterministic_hash`.

Referências:

- [vertical-slice.md](vertical-slice.md)
- [contract-test-plan.md](contract-test-plan.md)
- [test-plan.md](test-plan.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [metric-catalog.md](metric-catalog.md)

Critério mínimo de aceite:

- os testes falham pelo motivo esperado antes da implementação.

### Fase 3 — Materializar o núcleo canônico

Objetivo: implementar apenas o necessário para o domínio suportar a V0.

Passos atômicos:

- Criar `Domain/Models/Draw`.
- Criar `Domain/Models/Window`.
- Criar `Domain/Errors/` com erros canônicos da V0.
- Criar `Domain/Normalization/` com a barreira canônica de `Draw`.
- Criar `Domain/Windows/` com a regra de resolução de janela.
- Criar `Domain/Metrics/` com `frequencia_por_dezena@1.0.0`.

Referências:

- [metric-catalog.md](metric-catalog.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [ADR 0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

Critério mínimo de aceite:

- os testes de domínio da V0 passam sem depender de transporte HTTP.

### Fase 4 — Materializar infraestrutura mínima

Objetivo: ler fixture, versionar dataset e implementar determinismo técnico sem invadir a semântica do núcleo.

Passos atômicos:

- Criar provider de fixture em `Infrastructure/Providers/`.
- Criar implementação de `dataset_version` em `Infrastructure/DatasetVersioning/`.
- Criar implementação de JSON canônico em `Infrastructure/CanonicalJson/`.
- Criar implementação de hashing SHA-256.

Referências:

- [mcp-tool-contract.md](mcp-tool-contract.md)
- [brief.md](brief.md)
- [ADR 0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

Critério mínimo de aceite:

- o mesmo snapshot gera o mesmo `dataset_version`;
- o mesmo input canônico gera o mesmo hash.

### Fase 5 — Materializar casos de uso

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

- os casos de uso retornam payloads/objetos suficientes para o server serializar sem precisar “inventar semântica”.

### Fase 6 — Materializar o servidor HTTP/MCP

Objetivo: expor a V0 sem colocar cálculo no host.

Passos atômicos:

- Criar `Server/Program.cs`.
- Criar `Server/DependencyInjection/`.
- Criar `Server/Tools/` ou endpoints equivalentes para `get_draw_window` e `compute_window_metrics`.
- Implementar binding e validação estrutural no `Server`.
- Implementar serialização de erros conforme o contrato.
- Adicionar toggles operacionais de acesso como desligados por padrão.

Referências:

- [mcp-tool-contract.md](mcp-tool-contract.md)
- [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)
- [project-guide.md](project-guide.md)

Critério mínimo de aceite:

- a V0 responde pelos endpoints/tools previstos;
- o server continua fino;
- auth/throttle/quota ficam explicitamente desligados, não omitidos por acidente.

### Fase 7 — Fechar a V0

Objetivo: encerrar a primeira fatia vertical com evidência.

Passos atômicos:

- Rodar testes de domínio.
- Rodar testes de contrato.
- Rodar testes de integração da V0.
- Confirmar que os critérios obrigatórios de [vertical-slice.md](vertical-slice.md) estão cobertos.
- Confirmar que a documentação continua coerente com o comportamento observado.

Critério mínimo de aceite:

- a V0 está verde e rastreável por testes.

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

Depois da V0, cada nova entrega deve seguir sempre esta ordem:

1. escolher a próxima fatia;
2. localizar os specs normativos;
3. escrever/ajustar testes;
4. implementar domínio;
5. implementar orquestração;
6. expor no server;
7. validar contrato;
8. atualizar docs se a semântica tiver mudado.

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
- [ ] Primeiro teste negativo escrito
- [ ] Primeiro teste de fórmula escrito
- [ ] Primeiro teste de determinismo escrito

## Recomendação prática

Se houver dúvida sobre “qual passo pedir agora para a IA”, comece sempre por este recorte:

1. fixture mínima;
2. teste de contrato negativo;
3. teste de fórmula da V0;
4. tipos canônicos do domínio;
5. provider de fixture;
6. métrica `frequencia_por_dezena`;
7. caso de uso;
8. tool/endpoint;
9. hash determinístico;
10. fechamento da V0.

Essa ordem é a forma prática de usar spec-driven neste projeto: **spec → teste → implementação mínima → validação → próxima fatia**.
