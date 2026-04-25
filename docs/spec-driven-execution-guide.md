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
- [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md)
- [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)
- [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md)
- [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)
- [ADR 0009](adrs/0009-help-e-catalogo-de-templates-resources-v1.md)
- [ADR 0017](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md)
- [ADR 0019](adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md)
- [ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)
- [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) — tabelas resumidas e linguagem acessível para *humanos* (não altera o JSON do MCP; modos resumo *vs.* interpretação)

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

- Confirmar que a arquitetura vigente é a do [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md) e que a superfície MCP + HTTP está alinhada ao [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md) quando o trabalho for pós-V0.
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

### Extensão (hotfix pós-V0) — Métricas autoexplicativas: total na janela e sequência atual

Objetivo: introduzir métricas com nomes **não ambíguos** para leitura humana e validação de “sequência atual reinicia ao ausente”, mantendo compatibilidade com nomes antigos durante a janela de migração (sunset).

Passos atômicos (ordem recomendada):

- **H1 — Fechar spec (catálogo + ADR 0021 + glossário):**
  - Garantir que [metric-catalog.md](metric-catalog.md) lista:
    - `total_de_presencas_na_janela_por_dezena@1.0.0` (equivalente semântico de `frequencia_por_dezena@1.0.0`);
    - `sequencia_atual_de_presencas_por_dezena@1.0.0` (streak atual com reinício ao ausente);
    - `top10_maiores_totais_de_presencas_na_janela@1.0.0` e `top10_menores_totais_de_presencas_na_janela@1.0.0` derivados do “total”.
  - Garantir que [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) inclui frases modelo para as novas métricas (tabela A).
  - Garantir que [metric-glossary.md](metric-glossary.md) possui entradas e textos de tabela para as novas métricas.

- **H2 — Testes vermelhos (fórmula + equivalência):**
  - Adicionar testes de fórmula para `sequencia_atual_de_presencas_por_dezena` provando reinício ao ausente.
  - Adicionar teste de equivalência: `total_de_presencas_na_janela_por_dezena == frequencia_por_dezena` na mesma janela/fixture.
  - Adicionar testes para novos top10 (ties estáveis, desempate por dezena asc), usando fixture tipo `tie_heavy.json`.

- **H3 — Implementação mínima (após testes):**
  - Implementar as duas novas métricas (total e sequência) e os dois novos top10.
  - Manter nomes antigos funcionando durante a janela de migração.

- **H4 — Contrato de migração (sunset):**
  - Atualizar [mcp-tool-contract.md](mcp-tool-contract.md) para que `UNKNOWN_METRIC` possa carregar `details.replacement_metric_name` e `details.sunset_date` quando o nome pedido foi removido por sunset.
  - Atualizar [contract-test-plan.md](contract-test-plan.md) com o teste negativo de sunset.
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

### Fase 7 — Materializar o servidor HTTP da V0 (superfície mínima)

Objetivo: expor a V0 por **HTTP** sem colocar cálculo no host e sem deixar metadados contratuais para depois. O **transporte MCP** entra na [Fase 9](#fase-9-transporte-mcp-protocolo-real--paridade-com-o-contrato).

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

### Fase 9: Transporte MCP (protocolo real + paridade com o contrato)

Objetivo: expor as tools já implementadas via **protocolo MCP** real ([ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md)), com a **mesma semântica JSON** que os POSTs HTTP.

Nota de consistência (normativa):

- “MCP” aqui significa **protocolo MCP** (`tools/list`, `tools/call` etc.) sobre `stdio`, `SSE` ou `Streamable HTTP` (ver tabela explícita na ADR 0005).
- Endpoints REST que recebem JSON por `POST` (ex.: `/tools/*`) são **HTTP espelhado/compatibilidade**, não MCP.
- Prefixos de rota não definem protocolo: `/mcp/tools/*` (quando existir) continua sendo REST e deve ser tratado como **deprecado** por confundir “MCP via HTTP”.

Passos atômicos:

- Adicionar a família de pacotes `**ModelContextProtocol`** (SDK oficial C#) conforme documentação vigente do repositório `csharp-sdk`; incluir `ModelContextProtocol.AspNetCore` apenas se o transporte HTTP MCP for necessário na mesma PR.
- Confinar atributos/tipos do SDK ao `LotofacilMcp.Server` (e a um executável stdio mínimo, se for o caso), sem referências MCP em `Domain`/`Application`.
- Registrar `get_draw_window` e `compute_window_metrics` como tools MCP com nomes e schemas alinhados a [mcp-tool-contract.md](mcp-tool-contract.md).
- Implementar transporte MCP via **stdio** (prioridade para hosts desktop), conforme [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md).
- Escrever testes de integração que provem descoberta e invocação (ex.: fluxo equivalente a `tools/list` e `tools/call`) e **paridade** com a resposta dos endpoints HTTP para o mesmo input.
- Documentar configuração mínima do host (ex.: entrada MCP no cliente) no [README.md](../README.md) ou doc operacional existente.

#### Fase 9A (recomendada primeiro): MCP via `stdio` (Cursor “plug-and-play”)

Justificativa: reduz variáveis operacionais (deploy, streaming HTTP, proxies) e valida o núcleo do protocolo MCP no ambiente desktop com menor superfície.

Critério mínimo de aceite adicional:

- um host MCP desktop (ex.: Cursor) consegue iniciar o servidor local via comando e `--mcp-stdio`;
- `tools/list` e `tools/call` funcionam para as tools em escopo;
- paridade MCP/stdio ↔ REST `/tools/*` é provada por teste (deep-equals do JSON de payload).

Referências:

- [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [contract-test-plan.md](contract-test-plan.md)

Critério mínimo de aceite:

- um host MCP consegue listar e chamar as tools da V0;
- sucesso e erros de contrato são consistentes entre MCP e HTTP;
- D3 do [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md) (servidor sem IA embarcada) permanece verdadeiro.

### Fase 10: Expandir tools documentadas (ondas B e C)

Objetivo: implementar as ferramentas restantes de [mcp-tool-contract.md](mcp-tool-contract.md) (*Ferramentas propostas*, itens 3–8), em **fatias verticais** independentes.

Ordem recomendada (dependências e complexidade crescente):

1. `analyze_indicator_stability`
2. `compose_indicator_analysis`
3. `analyze_indicator_associations`
4. `summarize_window_patterns`
5. `generate_candidate_games`
6. `explain_candidate_games`

Passos atômicos (repetir por tool):

- Fixar semântica no [metric-catalog.md](metric-catalog.md) / [generation-strategies.md](generation-strategies.md) se ainda houver lacuna normativa.
- Escrever testes de contrato e/ou domínio **antes** (ou em paralelo explícito) da superfície.
- Implementar `Application` → `Domain` conforme [ADR 0002](adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md) quando aplicável.
- Expor a tool em **HTTP** e **MCP** no mesmo recorte, salvo exceção documentada no PR.

Referências:

- [ADR 0002](adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [contract-test-plan.md](contract-test-plan.md)

Critério mínimo de aceite:

- cada tool concluída tem testes objetivos e entradas/saídas conforme o contrato;
- envelope com `dataset_version`, `tool_version`, `deterministic_hash` onde o contrato exigir.

### Fase 11: Fechar evidências da V1 (transportes MCP + catálogo em escopo)

Objetivo: declarar a V1 “fechada” para o escopo acordado (transporte MCP + tools implementadas) com rastreabilidade na CI.

Passos atômicos:

- Rodar suítes de domínio, contrato e integração MCP relevantes.
- Atualizar [contract-test-plan.md](contract-test-plan.md) e, se necessário, [vertical-slice.md](vertical-slice.md) ou um doc de escopo V1 para refletir tools e transportes entregues.
- Confirmar que [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md) critérios de verificação são atendidos para o recorte entregue, usando a tabela explícita de superfícies (MCP real vs REST espelhado) como referência.

Critério mínimo de aceite:

- nenhuma tool em escopo documentado fica só em HTTP ou só em MCP sem justificativa escrita;
- o documento de evidências explicita quais superfícies foram entregues (ex.: MCP `stdio` apenas; ou `stdio` + MCP HTTP/SSE) e trata `/mcp/tools/*` como REST deprecado quando existir;
- documentação, testes e comportamento observado permanecem alinhados.

### Fase 12: Correção de drift (desalinhamento spec ↔ implementação)

Objetivo: tratar de forma explícita os casos em que a execução técnica se afasta do que estava definido em ADR/spec, sem “retroceder o roadmap”, e sim reconvergindo documentação, testes e código com rastreabilidade.

Quando acionar esta fase (gatilhos):

- implementação parcial que cumpre funcionalidade, mas viola nomenclatura/protocolo/superfície descrita nos specs;
- divergência entre docs e comportamento observado (ex.: rota/fluxo descrito como MCP sem implementar `tools/list`/`tools/call`);
- criação de artefato/projeto improvisado fora do recorte planejado;
- necessidade de deprecar alias/atalho que induz interpretação errada no cliente.

Classificação mínima do drift (registrar no PR/issue):

1. **Semântico** (campo, contrato, invariantes, linguagem normativa).
2. **Transporte/superfície** (MCP real vs REST espelhado, rotas e discovery).
3. **Estrutural** (projeto/pasta/dependência fora das fronteiras ADR).
4. **Evidência** (testes e documentação não cobrem o que foi entregue).

Passos atômicos:

- Documentar o desvio com referência explícita ao spec violado (ADR, contrato ou guia).
- Definir decisão de correção: reconduzir código ao spec atual ou revisar o spec (nunca os dois de forma implícita).
- Aplicar correção mínima em código/rotas/comentários para eliminar sinais ambíguos.
- Atualizar documentação operacional e plano de testes para refletir o estado corrigido.
- Adicionar ou ajustar testes de regressão para impedir retorno do mesmo drift.
- Marcar itens de compatibilidade como `deprecated` quando não puder removê-los no mesmo recorte.

Referências:

- [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [contract-test-plan.md](contract-test-plan.md)

Critério mínimo de aceite:

- o drift fica classificado e rastreável;
- documentação, testes e comportamento observado voltam a estar alinhados;
- não permanece no código nenhuma superfície “parece MCP” mas não implementa protocolo MCP;
- o recorte resultante fica claro para o próximo ciclo (retomar Fase 10/11 ou executar a Fase 13).

### Fase 13: Transporte MCP via HTTP (SSE/Streamable HTTP)

Objetivo: adicionar transporte MCP por HTTP sem reabrir a Fase 9, mantendo a distinção normativa entre protocolo MCP real e REST espelhado.

Justificativa: “MCP via HTTP” é uma camada de transporte diferente e historicamente foi o ponto de confusão (MCP-like REST ≠ MCP). Tratar como etapa própria evita retrabalho retroativo e preserva a rastreabilidade da evolução.

Passos atômicos:

- Adicionar integração ASP.NET Core do SDK MCP (ex.: `ModelContextProtocol.AspNetCore`, conforme versão vigente).
- Expor endpoint MCP HTTP real (`/sse` e/ou `/mcp`) capaz de responder discovery/call do protocolo.
- Garantir convivência explícita com REST espelhado (`/tools/*`) sem declarar REST como MCP.
- Escrever testes de integração para `tools/list` e `tools/call` no transporte HTTP MCP e validar paridade semântica com REST.
- Atualizar documentação operacional com configuração de cliente MCP HTTP.

Referências:

- [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [contract-test-plan.md](contract-test-plan.md)

Critério mínimo de aceite:

- um cliente MCP conecta via HTTP (`/sse` e/ou `/mcp`) e executa `tools/list` + `tools/call`;
- paridade MCP HTTP ↔ REST `/tools/*` é comprovada por teste;
- `/mcp/tools/*` (quando existir) permanece tratado como REST deprecado.
 
### Fase 14: Fechar `compute_window_metrics` como “catálogo executável” (dispatcher)

Objetivo: tornar `compute_window_metrics` um executor real do catálogo (despachando por `metrics[].name`), com erros e paridade pedido↔resposta conforme contrato.

Contexto: a tool pode existir e responder, mas sem um dispatcher real ela não cumpre o “catálogo fechado” (e bloqueia composição/padrões/associações ao forçar métricas erradas).

Passos atômicos recomendados:

- Implementar dispatcher real por `metrics[].name` (paridade pedido↔resposta).
- Implementar erro `UNKNOWN_METRIC` para nomes fora do catálogo.

Critério mínimo de aceite:

- `compute_window_metrics`:
  - despacha por nome canônico do catálogo;
  - retorna `UNKNOWN_METRIC` para nomes fora do catálogo;
  - preserva o envelope (`dataset_version`, `tool_version`, `deterministic_hash`) e a tipagem (`scope`, `shape`, `unit`, `version`);
  - não colapsa pedidos duplicados (paridade 1:1).

### Fase 15: Métricas derivadas diretas de `frequencia_por_dezena`

Objetivo: materializar as primeiras métricas `por_transformacao` do catálogo, mantendo determinismo e tie-break canônico.

- Implementar:
  - `top10_mais_sorteados@1.0.0`
  - `top10_menos_sorteados@1.0.0`

Critério mínimo de aceite:

- as duas métricas retornam listas determinísticas com regra de empate por dezena asc;
- `scope`, `shape`, `unit` e `version` batem com o catálogo.

### Fase 16: Séries escalares por concurso (destravando associações e padrões)

Objetivo: materializar séries canônicas por concurso (escalares) que servem como base para associações e resumo de padrões.

- Implementar:
  - `pares_no_concurso@1.0.0`
  - `repeticao_concurso_anterior@1.0.0` (comprimento conforme regra normativa no catálogo/ADR referenciado)
  - `quantidade_vizinhos_por_concurso@1.0.0`
  - `sequencia_maxima_vizinhos_por_concurso@1.0.0`

Critério mínimo de aceite:

- cada série tem comprimento coerente com a janela resolvida e contrato;
- o cálculo é determinístico na fixture mínima.

### Fase 17: Séries estruturais (vetoriais por concurso) para agregações e resumos

Objetivo: materializar shapes estruturados necessários para agregações explícitas e para features vetoriais em tools de associações/padrões.

- Implementar:
  - `distribuicao_linha_por_concurso@1.0.0` (`shape=series_of_count_vector[5]`)
  - `distribuicao_coluna_por_concurso@1.0.0` (`shape=series_of_count_vector[5]`)
  - `entropia_linha_por_concurso@1.0.0`
  - `entropia_coluna_por_concurso@1.0.0`
  - `hhi_linha_por_concurso@1.0.0`
  - `hhi_coluna_por_concurso@1.0.0`

Critério mínimo de aceite:

- shapes seguem o catálogo (incluindo `series_of_count_vector[5]`);
- entropias são em bits e valores permanecem finitos;
- cada ponto da distribuição por linha/coluna soma 15.

### Fase 18: Implementar `compose_indicator_analysis` (recorte mínimo)

Objetivo: materializar a primeira tool de composição declarativa, começando pelo menor recorte útil e totalmente validável.

- Implementar `compose_indicator_analysis` começando pelo recorte mínimo `target=dezena`, `operator=weighted_rank`.
- Validar pesos \(1.0 ± 1e-9\) e transforms do enum; falhas com códigos do contrato.

Critério mínimo de aceite:

- testes negativos: pesos não somam 1; transform inválida;
- 1 teste positivo determinístico;
- exposição em HTTP + MCP no mesmo recorte.

### Fase 19: Implementar `analyze_indicator_associations` e `summarize_window_patterns` (uma por vez)

Objetivo: materializar as duas tools analíticas restantes antes da geração, com validação de enums/agregações e outputs explícitos.

- `analyze_indicator_associations`:
  - começar com séries escalares e `method=spearman`;
  - vetoriais exigem `aggregation` explícita antes de correlacionar.
- `summarize_window_patterns`:
  - começar com `range_method=iqr` e 1 feature suportada;
  - output declara `Q1`, `median`, `Q3`, `IQR`, cobertura e contagens.

Critério mínimo de aceite:

- cada tool nova tem:
  - pelo menos 1 teste negativo de contrato (código correto);
  - pelo menos 1 teste positivo determinístico com fixture;
  - paridade semântica entre MCP e HTTP para o mesmo request.

### Fase 20: Implementar `generate_candidate_games` e `explain_candidate_games` (uma por vez)

Objetivo: fechar o ciclo de geração/explicação com rastreabilidade e determinismo, seguindo o catálogo fechado de estratégias e exclusões.

- `generate_candidate_games`:
  - começar com 1 estratégia nominal simples, orçamento e determinismo;
  - seed obrigatória quando houver `sampled`/`greedy_topk`;
  - output sempre traz linhagem (`strategy_name`, `strategy_version`, `search_method`, `tie_break_rule`, `seed_used` quando aplicável).
  - quando evoluir o recorte para flexibilidade de critérios, seguir [ADR 0019](adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md):
    - aceitar restrições por **faixa** (`range`) e **multi-valor** (`allowed_values`) sem enumerar combinações no cliente;
    - quando o cliente declarar `typical_range`, ecoar `resolved_range` e `coverage_observed` em `applied_configuration.resolved_defaults` (sem inferência silenciosa);
    - quando suportado, permitir `mode = hard | soft` (default explícito em `resolved_defaults`) para evitar colapso do espaço ao combinar muitas restrições;
    - expor um orçamento determinístico (ex.: `max_attempts`/pool multiplier) e ecoar contadores (`attempts_used`, `accepted_count`, rejeições agregadas) para suportar `count` alto.
  - sequência recomendada (ADR 0019) para manter o recorte atômico:
    - **20.1 — Contrato + validação estrutural**: aceitar `range`, `allowed_values`, `typical_range` e `mode` no request (sem remover o modo atual); rejeitar “modos mistos” com `INVALID_REQUEST`; normalizar `allowed_values` (ordem + dedup) e registrar em `resolved_defaults`.
    - **20.2 — Resolvedor determinístico de `typical_range`**: implementar `iqr` e `percentile` (com `params`) sobre a janela declarada; retornar `resolved_range`, `coverage_observed` e `method_version` e ecoar tudo em `resolved_defaults`.
    - **20.3 — Aplicação no motor de geração (hard)**: aplicar `range`/`allowed_values` como conjunto válido (pass/fail) sem enumerar combinações; garantir determinismo do lote com `count` e orçamento (`max_attempts`/pool multiplier).
    - **20.4 — `mode="soft"` (quando suportado)**: definir penalidade determinística (canônica e versionada) e ecoar defaults/penalidades aplicadas em `resolved_defaults`; manter `mode="hard"` como default explícito.
    - **20.5 — Explicabilidade**: atualizar `explain_candidate_games` para indicar, por critério, o valor observado, a faixa/conjunto (incl. `resolved_range`) e o resultado (pass/fail ou penalidade).
    - **20.6 — Testes**: retrocompatibilidade (requests antigos), equivalência `range` vs (`min`,`max`) quando aplicável, determinismo (`deterministic_hash`) e eco completo de `resolved_defaults`.
- `explain_candidate_games`:
  - ranking determinístico de estratégias e breakdown de métricas/exclusões com versões.

Critério mínimo de aceite:

- cada tool nova tem:
  - pelo menos 1 teste negativo de contrato (código correto);
  - pelo menos 1 teste positivo determinístico com fixture;
  - exposição em HTTP + MCP no mesmo recorte (salvo exceção documentada).

### Fase 21: Disponibilidade por rota, pipeline e GAPS (ADR 0006) em sequência spec-first

Objetivo: materializar (ou ajustar) regras de **disponibilidade por build/rota**, fluidez de pipeline e respostas de erro com `details` quando aplicável, mantendo:

- norma (catálogo/docs) separada da instância (build/allowlist);
- determinismo forte;
- ausência de defaults semânticos ocultos.

Sequência recomendada:

1. **21.1 — Fechamento coordenado de docs/contrato**
   - Revisar [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) e refletir em [mcp-tool-contract.md](mcp-tool-contract.md), [metric-catalog.md](metric-catalog.md), [contract-test-plan.md](contract-test-plan.md) e [test-plan.md](test-plan.md) quando a entrega tocar em: `details.allowed_metrics`, `UNKNOWN_METRIC`, erros ricos, GAPS (pares–entropia) e fluidez inter-tool.
2. **21.2 — Testes de contrato vermelhos (pelo menos 1 cenário por tema)**
   - Escrever testes que falhem antes do código para: erros com `details`, comportamento de rota/build, e os cenários mínimos da matriz do contrato.
3. **21.3 — Implementação mínima por recorte**
   - Implementar/ajustar apenas o recorte coberto pelos testes, sem expandir catálogo ou introduzir heurísticas.
4. **21.4 — Evidências de paridade**
   - Validar paridade MCP ↔ HTTP nos caminhos suportados e registrar evidência quando aplicável.

Referências obrigatórias:

- [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [contract-test-plan.md](contract-test-plan.md)
- [test-plan.md](test-plan.md)
- [fases-execucao-templates.md — Fase 21](fases-execucao-templates.md#fase-21---adr-0006-disponibilidade-por-rota-pipeline-gaps-estabilidade-de-associa-o-e-paresentropia)

Critério mínimo de aceite:

- o recorte tocado possui docs/contrato coerentes + testes de contrato correspondentes;
- erros e `details` (quando parte do recorte) são estáveis e auditáveis;
- nenhuma regra de disponibilidade “vaza” como default semântico no servidor.

### Fase 22: Implementar `summarize_window_aggregates` (ADR 0007) em sequência spec-first

Objetivo: materializar agregados canônicos de janela (histogramas, top-k de padrões e matriz por posição×valor) com enum fechado, parâmetros explícitos por tipo, ordenação/desempates canônicos e determinismo auditável.

Sequência obrigatória (não pular etapas):

1. **22.1 — Fechamento de contrato/documentação antes de código**
   - Atualizar [mcp-tool-contract.md](mcp-tool-contract.md), [test-plan.md](test-plan.md), [contract-test-plan.md](contract-test-plan.md) e [prompt-catalog.md](prompt-catalog.md) conforme [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md).
   - Fechar `aggregate_type` como enum e declarar validações/ordenação.
2. **22.2 — Testes de contrato vermelhos (request, erros, determinismo, ordenação)**
   - Escrever testes antes da implementação da tool.
   - Cobrir negativos mínimos: `aggregates` ausente, `aggregate_type` inválido, bucket spec inválida, bounds inválidos, `UNKNOWN_METRIC`, `UNSUPPORTED_SHAPE`.
3. **22.3 — Implementação mínima da tool e exposição HTTP + MCP**
   - Implementar `Application`/`Server` para os três `aggregate_type` do recorte inicial, sem defaults semânticos ocultos.
   - Expor no mesmo recorte em HTTP espelhado e MCP.
4. **22.4 — Paridade + evidências (incluindo goldens quando estável)**
   - Validar paridade semântica MCP ↔ HTTP para sucesso e erro.
   - Congelar fixtures/goldens de agregados quando payload estiver estável e auditável.

Referências obrigatórias:

- [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [contract-test-plan.md](contract-test-plan.md)
- [test-plan.md](test-plan.md)
- [prompt-catalog.md](prompt-catalog.md)

Critério mínimo de aceite:

- `summarize_window_aggregates` possui contrato fechado e testes de contrato cobrindo validação, determinismo e ordenação canônica;
- implementação passa nos testes da fase 22 sem defaults ocultos;
- paridade MCP/HTTP e evidências de fixture/golden estão registradas.

### Fase 23: Descoberta híbrida, janela por extremos e mapeamento legado Top 10 (ADR 0008) em sequência spec-first

Objetivo: cumprir [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) — *superfície de instância* (allowlist, erros com `details`) *vs.* *norma* (catálogo em `docs/`, resources MCP opcionais), equivalência **concurso inicial / final (inclusivos)** ↔ `window_size` + `end_contest_id`, e mapeamento normativo do export `HistoricoTop10MaisSorteados` → `top10_mais_sorteados@1.0.0` **só** sobre a janela declarada, **sem** reproduzir «últimos N» fixos de UI legada (D4).

Sequência obrigatória (não pular etapas):

1. **23.1 — Fechamento e revisão coordenada de documentação (antes de ampliar código)**
   - Garantir que [mcp-tool-contract.md](mcp-tool-contract.md) (entidade `Window`, secção *Prompts e Resources*, invariantes de janela), [metric-catalog.md](metric-catalog.md) (secções *Janela por extremos*, *HistoricoTop10MaisSorteados*, *QtdFrequencia*) e [metric-glossary.md](metric-glossary.md) estão alinhados ao ADR e entre si.
   - Atualizar [contract-test-plan.md](contract-test-plan.md) com a **Fase B.2** (matriz mínima para janela, ambiguidade e `top10_mais_sorteados`).
   - Religar às [templates — Fase 21](fases-execucao-templates.md) (secção *Fase 21 - ADR 0006*) e ao [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) onde a descoberta exigir `allowed_metrics` ou pistas em erros — **não** usar o [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) para substituir decisões de descoberta ou Top 10 (o 0007 permanece agregados).
2. **23.2 — Testes de contrato vermelhos (janela, rejeição, ranking)**
   - Escrever testes que falhem até a implementação existir: equivalência numérica entre as duas formas de janela quando o protocolo as suportar; combinação **ambígua** → `INVALID_REQUEST` (ou código fechado no contrato); `top10_mais_sorteados@1.0.0` reprodutível e alinhado à Tabela 2 do catálogo (p.ex. fixture `tie_heavy.json` para empates).
3. **23.3 — Implementação mínima alinhada ao recorte**
   - Resolver a janela no pedido de forma **única** e auditável: aceitar só `window_size` + `end_contest_id` **ou** estender o JSON com `start_contest_id` / `end_contest_id` se o contrato o permitir, desde que a equivalência de D2 seja a mesma.
   - Garantir `compute_window_metrics` (e tools com janela) coerentes com a resolução; manter a proibição de defaults temporais ocultos.
4. **23.4 — Evidências e superfície opcional**
   - Paridade MCP ↔ HTTP para sucesso e erro nos casos do 23.2, quando a tool estiver exposta em ambos.
   - *Opcional* nesta fatia: expor **MCP Resources** (ou manter injeção de `docs/` no cliente) sem duplicar allowlist; o nome exato de uma tool dedicada a listar superfície **não** é exigido pelo ADR enquanto a semântica de D1 for respeitada e o [mcp-tool-contract.md](mcp-tool-contract.md) for atualizado em conjunto (ver *Não decisão* no ADR 0008).

Referências obrigatórias:

- [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [metric-catalog.md](metric-catalog.md)
- [metric-glossary.md](metric-glossary.md)
- [contract-test-plan.md](contract-test-plan.md) (Fase B.2)
- [test-plan.md](test-plan.md)
- [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) (cruzamento com `details` / allowlist)
- [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) (apenas para **não** confundir agregados com descoberta/Top 10)

Critério mínimo de aceite:

- testes da Fase B.2 passam quando a build implementa o recorte; nenhum documento normativo contradiz D1–D6 do ADR 0008;
- janela e `top10_mais_sorteados` são rastreáveis ao recorte que o **chamador** declara; paridade de transporte documentada para os casos de evidência.

### Fase 24 — Help + catálogo de templates (resources) (ADR 0009)

Objetivo: expor uma superfície mínima de **ajuda** e **templates Markdown** para orientar o primeiro uso (onboarding) e reduzir atrito de descoberta, sem alterar o contrato de cálculo determinístico.

Passos atômicos (recorte):

- Adicionar o resource de onboarding curto `lotofacil-ia://help/getting-started@1.0.0` com **linguagem simples (leigo-first)**, seguindo:
  - guia de 3 passos com CTA (“Peça ajuda” → “Escolha um caminho” → “Escolha o período”);
  - menu curto (2–4 caminhos) com rótulos humanos (“Painel geral”, “Frequência e atraso”, etc.);
  - secção “Se der erro” em termos humanos;
  - secção separada “Para DEV/integração” (opcional), para detalhes técnicos e invariantes.
- Adicionar resources `lotofacil-ia://prompts/index@1.0.0` e 10 templates versionados.
- Adicionar a tool `help` retornando `index_markdown`, `index_resource_uri`, `templates[]` (metadados) e, quando existir, `getting_started_resource_uri`.
- (Recomendado, não-breaking) Incluir no `help` um “topo de UX” opcional (ex.: `quick_start_markdown` ou entrypoints curtos) para evitar que “liste ajuda” vire um catálogo confuso.
- Padronizar nos templates a preferência de exibição `display_mode = simple | advanced | both` (default `both`) para suportar usuários leigos e experts.
- Atualizar `docs/brief.md`, `docs/prompt-catalog.md` e `docs/test-plan.md` com a nova superfície.
- Adicionar testes de contrato para:
  - discovery de tool `help`;
  - `resources/list` contendo `lotofacil-ia://help/getting-started@1.0.0`;
  - `resources/list` contendo o índice e ao menos um template;
  - `resources/read` de `lotofacil-ia://help/getting-started@1.0.0` retornando Markdown com MIME correto;
  - `resources/read` do índice retornando Markdown com MIME correto.

Referências:

- [docs/mcp-tool-contract.md](mcp-tool-contract.md) (primitivas MCP opcionais)
- [ADR 0009](adrs/0009-help-e-catalogo-de-templates-resources-v1.md)

#### 24.2 — Refinar onboarding leigo-first (UX) e reduzir confusão de “ajuda”

Objetivo: garantir que “getting started” e “ajuda” funcionam para **iniciante** (sem jargão, sem catálogo confuso), usando progressão por camadas (progressive disclosure).

Passos atômicos (recorte):

- Revisar `resources/help/getting-started@1.0.0.md` para:
  - começar com “O que é / o que não é” em linguagem simples;
  - trazer **3 passos** acionáveis com CTA;
  - oferecer um **menu curto** (2–4 caminhos) com “comece aqui”;
  - ter “Se der erro” com orientação humana;
  - mover detalhes técnicos para uma secção final opcional “Para DEV/integração”.
- Revisar `resources/prompts/index@1.0.0.md` para começar com “Escolha 1 destas opções” antes do catálogo de 10 templates.
- (Opcional, não-breaking) Ajustar o tool `help` para retornar um bloco curto (ex.: `quick_start_markdown`) adequado para o pedido “liste ajuda”, mantendo o catálogo completo em `templates[]`.
- Atualizar `docs/adrs/0009-help-e-catalogo-de-templates-resources-v1.md` se a revisão editorial alterar regras de linguagem/estrutura.

Critério mínimo de aceite:

- uma pessoa sem contexto consegue executar o “primeiro uso” lendo apenas `getting-started` (sem abrir ADRs nem aprender nomes de tools);
- o pedido “ajuda / liste ajuda” pode ser respondido por um bloco curto antes de listar o catálogo completo.

### Fase 25 — Fechamento sistemático de GAPs do brief vs `src/` (ADR 0010–0018)

Objetivo: fechar os GAPs levantados em `docs/brief-vs-src-gap-matrix.md` sem remover contratos públicos, implementando as capacidades expostas e reduzindo frustração no consumo do MCP.

Norma:

- [ADR 0010](adrs/0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) (governança, clusters e definição de “gap fechado”)
- ADRs por cluster: [0011](adrs/0011-tool-de-discovery-de-capacidades-por-build-v1.md) · [0012](adrs/0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md) · [0013](adrs/0013-janela-uniforme-por-extremos-em-todas-as-tools-v1.md) · [0014](adrs/0014-semantica-real-de-allow-pending-v1.md) · [0015](adrs/0015-estabilidade-em-subjanelas-para-associacoes-stability-check-v1.md) · [0016](adrs/0016-expansao-de-resumos-de-janela-e-padroes-v1.md) · [0017](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md) · [0018](adrs/0018-pacote-de-metricas-prioritarias-slots-pares-blocos-outliers-v1.md)

Passos atômicos (recorte, em ordem recomendada):

- **25.1 — Implementar discovery por build** (`discover_capabilities`) para publicar: métricas por rota, enums suportados, estratégias/filtros e modos de janela (ADR 0011). O objetivo é eliminar tentativa-e-erro antes de chamar tools de cálculo.
- **25.2 — Introduzir registro único de métricas/capacidades** e derivar dele validações e allowlists (ADR 0012). A partir daqui, drift entre listas manuais deve ser tratado como bug.
- **25.3 — Uniformizar janela por extremos em toda a superfície** (ADR 0013), alinhado ao ADR 0008 D2.
- **25.4 — Dar semântica observável a `allow_pending`** (ADR 0014), com status explícito na discovery.
- **25.5 — Implementar `stability_check` em associações** com saída `association_stability` determinística (ADR 0015), alinhado ao ADR 0006 D2.
- **25.6 — Expandir resumos de janela/padrões** para cobrir features escalares prioritárias (ADR 0016).
- **25.7 — Evoluir geração para contrato declarativo** (critérios/pesos/filtros/estratégias) com rastreabilidade completa (ADR 0017).
- **25.8 — Implementar o pacote de métricas prioritárias** (slots, pares/ímpares, blocos, estabilidade/divergência, runs/outliers) com exposição e testes (ADR 0018).

Restrições:

- **B18 (ingestão CEF real) permanece congelado** enquanto não aprovado; não usar isso como dependência para fechar os demais GAPs (ver ADR 0010).

Critério mínimo de aceite:

- discovery lista a superfície real por build (métricas por rota, enums, estratégias);
- o MCP deixa de “prometer e falhar” sem diagnóstico: gaps residuais ficam claros via discovery + erro canônico/determinístico;
- cada cluster fechado cumpre a definição operacional do ADR 0010 (código + contrato + discovery + testes).

### Fase 26 — Flexibilidade de geração: aleatório explícito, filtros opt-in, interseção, teto 1k e `seed` opcional (ADR 0020)

Objetivo: materializar o [ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md) na superfície `generate_candidate_games` / `explain_candidate_games`, sem alterar a premissa do [brief.md](brief.md) (descritivo, não preditivo): o utilizador pode pedir **candidatos aleatórios sem guardrails** ou **filtrar por comportamentos** declarados (muitas vezes por **faixas**, alinhado ao [ADR 0019](adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md)); a combinação de critérios segue **interseção** salvo modo documentado em contrário.

Norma:

- [ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md) (modos, opt-in, interseção, teto de volume, `seed` opcional e semântica de replay)
- Cruzamento: [ADR 0017](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md), [ADR 0019](adrs/0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md), [ADR 0002](adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md), [ADR 0001](adrs/0001-fechamento-semantico-e-determinismo-v1.md) (afinamento na rota de geração quando `seed` ausente)

Passos atômicos (recorte, ordem recomendada):

- **26.1 — Contrato MCP + `generation-strategies.md`:** fechar nomes e localização dos campos (modo de geração, `replay_guaranteed` ou equivalente, erro para `sum(plan[].count) > 1000`, semântica de `deterministic_hash` com e sem `seed`). Documentar interseção e opt-in de `structural_exclusions` face aos defaults atuais.
- **26.2 — Validação e erros:** implementar rejeição determinística do teto **1000** jogos por pedido; mensagem que orienta **nova rodada** para lotes maiores.
- **26.3 — Motor de geração:** implementar modos `random_unrestricted` vs `behavior_filtered` (ou nomes finais do contrato); no modo aleatório, **não** aplicar defaults conservadores de exclusão estrutural não solicitados; no modo filtrado, aplicar **só** o declarado + eco em `applied_configuration.resolved_defaults`.
- **26.4 — `seed` opcional:** com `seed` → replay da parte estocástica garantido (demais inputs iguais); sem `seed` → episódio não replayável com campo explícito na resposta; ajustar testes que hoje exigem `seed` obrigatório onde a nova semântica aplicar.
- **26.5 — Discovery e contrato:** atualizar `discover_capabilities` / schemas expostos para refletir limites, modos e obrigatoriedade de `seed` por caminho; paridade HTTP ↔ MCP onde existir.
- **26.6 — Testes de contrato:** cobrir: teto 1000; modo aleatório sem defaults indesejados; interseção de dois critérios; `seed` presente vs ausente (comportamento de hash/replay conforme contrato fechado em 26.1).

Critério mínimo de aceite:

- o contrato e a implementação refletem o ADR 0020 sem contradição com ADR 0017/0019;
- pedidos acima de 1000 jogos falham de forma clara;
- o utilizador consegue gerar candidatos **sem** filtros explícitos sem herdar silenciosamente os defaults atuais de exclusão estrutural no modo aleatório;
- `seed` deixa de ser obrigatório nos caminhos normativos definidos em 26.1, com semântica de replay documentada e testada.

### Fase 27 - Apresentacao de resumos de janela (ADR 0021)

Objetivo: cumprir o [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) na **camada de documentação e de texto a humanos** (tutoriais, glossário, ajuda, orientação a agentes). O **envelope** `MetricValue` e as tools **não** mudam de forma obrigatória: o ADR regula *como* apresentar respostas (templates A e B, vocabulário acessível, dois modos de profundidade).

Norma:

- [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) (D1–D5, Apêndice de frases modelo)
- Cruzamento: [metric-glossary.md](metric-glossary.md) (definição e *“O que observa”*), [metric-catalog.md](metric-catalog.md) quando o ADR 0021 remeta à leitura *legada* (ex. *QtdFrequencia* e distinção de nomes), [ADR 0009](adrs/0009-help-e-catalogo-de-templates-resources-v1.md) quando a entrega tocar em *getting-started* / *index* de templates

Passos atômicos (ordem recomendada):

- **27.1 — `metric-glossary`:** adicionar subsecção **“Textos de resumo para tabelas (ADR 0021)”** (nome alinhado às *Consequências* da ADR) com as frases do Apêndice da ADR, condensando ou reutilizando o bloco *“O que observa”* existente, sem contradizer [metric-catalog.md](metric-catalog.md). Cobrir, na redação, os requisitos D1: coluna **Descrição** (A) com, no mínimo para `estabilidade_ranking`, a leitura de *sub-janelas* e \([0,1]\) sem previsão; tabela B com última coluna *obrigatória* e vocabulário D2 (entropia, HHI, pares, vizinhos) quando a métrica for citada.
- **27.1b (hotfix documental, *opcional* salvo *gap* aberto) — ponte *Vocabulário* («ausência» / frequência / atraso / `ausencia_blocos`):** alinhar em cadeia [metric-catalog.md](metric-catalog.md) (secção *QtdFrequencia* e ponte *quatro papéis*, âncora estável `#export-legado-qtdfrequencia`; *Tabelas 1 e 2* canónicas se só precisar de ligação normativa, sem reescrever fórmulas), o Apêndice e o D3 da [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) (âncoras estáveis, entre outras: `#vocab-ausencia-adr-0021` no glossário, `#apendice-frases-modelo-pt-adr-0021` e `#nota-ausencia-adr-0021` no apêndice) e o glossário (tabela *Vocabulário* com `#vocab-ausencia-adr-0021`), para a conversa *ausente *N* concursos* **não** se confundir com `frequencia_por_dezena` (somas) nem com o *shape* de `ausencia_blocos`. **Não** altera fórmulas nem o contrato MCP. Usar quando, após 27.1, ainda houver risco de ambiguidade editorial entre o catálogo, o apêndice e o glossário.
- **27.2 — `AGENTS.md` e consumidores de texto:** confirmar que o atalho para a ADR 0021 (e, se existir, regra em *rules* do repositório) aponta para a distinção **resumo padrão** (baixo custo em tokens) *vs.* **interpretação explícita** sob pedido (mais tokens, ancorada no catálogo e nos dados do MCP), conforme D5; ver [ADR 0009](adrs/0009-help-e-catalogo-de-templates-resources-v1.md) (D4) se o texto for onboarding/template.
- **27.3 (opcional) — `compute_window_metrics` e texto no `MetricValue`:** ponto de extensão em código: [ComputeWindowMetricsUseCase.cs](../src/LotofacilMcp.Application/UseCases/ComputeWindowMetricsUseCase.cs) (`ExplanationFor` → campo `explanation` por métrica, quando existir no contrato). **Estado de referência:** o `switch` já cobre as métricas canónicas conhecidas; o genérico *“Metrica de janela.”* aplica-se **só** ao ramo *default* (nomes de métrica fora do mapa / futuras extensões). 27.3: (a) alargar o mapa se novas métricas deixarem cair no default, e/ou (b) alinhar *strings* técnicas ao tom mínimo do glossário/ADR **sem** substituir a norma A/B (apresentação humana). Não muda `MetricValue.value` nem semântica.
- **27.4 (opcional) — resources D4:** rever `resources/help/`, `resources/prompts/` (ex. `index@1.0.0.md`, prompts temáticos) e modelos alinhados ao [ADR 0009](adrs/0009-help-e-catalogo-de-templates-resources-v1.md) para exemplos de tabela A/B *sem* coluna redundante `shape`/`unit` em resumos a leigo; o [LotofacilMcp.Server.csproj](../src/LotofacilMcp.Server/LotofacilMcp.Server.csproj) copia `resources/**` para o *output* — após editar, garantir a mesma cópia/versão usada no *publish* (aberto no item *resources* da [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md)).
- **27.5 — Checklist (cobertura do Apêndice da ADR 0021):** garantir que **todas** as métricas citadas explicitamente no **Apêndice** (tabelas A e B) têm texto reutilizável e coerente com D1–D2 no `metric-glossary` (27.1) e, quando aplicável, não caem no ramo genérico do `explanation` (27.3). Lista nominal (não inventar novas além do Apêndice):  
  - **Tabela A:** `estabilidade_ranking`, `frequencia_por_dezena`, `total_de_presencas_na_janela_por_dezena`, `sequencia_atual_de_presencas_por_dezena`, `atraso_por_dezena`, `estado_atual_dezena`, `top10_mais_sorteados`, `top10_menos_sorteados`, `top10_maiores_totais_de_presencas_na_janela`, `top10_menores_totais_de_presencas_na_janela`  
  - **Tabela B:** `entropia_linha_por_concurso`, `entropia_coluna_por_concurso`, `hhi_linha_por_concurso`, `hhi_coluna_por_concurso`, `repeticao_concurso_anterior`, `pares_no_concurso`, `quantidade_vizinhos_por_concurso`, `sequencia_maxima_vizinhos_por_concurso`, `distribuicao_linha_por_concurso`, `distribuicao_coluna_por_concurso`

**Superfície de ficheiros (Fase 27, exceto 27.3 *strictly* code):** `docs/metric-glossary.md`; opc. `docs/metric-catalog.md`; `AGENTS.md`; `.cursor/rules/*`; `resources/help/*.md`, `resources/prompts/*.md`; código `src/LotofacilMcp.Application/UseCases/ComputeWindowMetricsUseCase.cs` (27.3).

Critério mínimo de aceite:

- existe secção de suporte no glossário (ou documento claramente referenciado) com frases reutilizáveis alinhadas ao Apêndice da ADR 0021; se 27.1b for aplicada, a ponte *Vocabulário* e o *QtdFrequencia* estão coerentes entre catálogo, ADR 0021 e glossário, sem conflito de nomes, e as âncoras D3 (ex. `#vocab-ausencia-adr-0021`, `#export-legado-qtdfrequencia`, apêndice `#nota-ausencia-adr-0021`) resolvam-se;
- a distinção D1/D5 (tabelas *vs.* interpretação; custo consciente de tokens) e D2 (mínimo léxico em séries) estão refletidas na documentação e, para agentes, em `AGENTS`/regras;
- nenhuma frase de apresentação contradiz fórmulas do catálogo nem implica previsão de sorteio.

### Fase 28 — Implementar métricas canônicas pendentes do catálogo (execução dirigida por plano)

Objetivo: garantir que **todas as métricas canônicas** de `docs/metric-catalog.md` (Tabela 1/2) que ainda não estejam materializadas no código sejam implementadas via execução spec-driven, **sem implementação avulsa fora de uma fase** e mantendo rastreabilidade por testes.

Escopo (normativo):

- Métricas canônicas e satélites **são definidas no catálogo**; a implementação deve seguir **nome**, **scope**, **shape**, **unit** e **version** declarados.
- Métricas de apresentação humana seguem [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) **quando houver texto/tabela** (glossário/resources/explicação), mas a ADR 0021 **não** substitui fórmula nem tipagem.

Sequência obrigatória (repetir por “lote pequeno” de métricas):

- **28.1 — Auditoria de lacunas (catálogo → código):**
  - Derivar a lista de métricas alvo a partir de `metric-catalog.md` (Tabela 1) filtrando por `Status=canonica` (e, se o passo explicitamente incluir, `satelite`).
  - Cruzar com a superfície real (ex.: `discover_capabilities`/registro de métricas por build) e produzir uma lista: **implementadas** vs **faltantes**.
  - Proibir “assumir implementado porque existe arquivo”: só conta como implementado se estiver **registrado/exposto** conforme o recorte (ex.: dispatcher/allowlist por tool quando aplicável).

#### Ferramenta auxiliar (opcional) — Auditoria MCP STDIO de métricas expostas e invariantes (runner)

Quando a intenção for auditar a **superfície real** disponível via **MCP STDIO** (instância/build) e validar rapidamente se o texto de `MetricValue.explanation` e as formas retornadas estão coerentes com invariantes simples (janela, shapes e tamanhos), use o runner:

- `tools/McpMetricAudit/McpMetricAudit.csproj`

O runner executa, via MCP STDIO:

- `discover_capabilities` (fonte de verdade da allowlist da **instância**) e extrai `metrics.compute_window_metrics_allowed`;
- `compute_window_metrics` em batch para todas as métricas permitidas, em uma janela fixa, e imprime:
  - tabela “métrica → explanation → evidência de resultado”;
  - tabela final de divergências quando houver.

**Como rodar (local):**

```bash
dotnet build LotofacilMcp.sln -c Debug
dotnet run --project tools/McpMetricAudit/McpMetricAudit.csproj -c Debug
```

**Premissas atuais (V0 / fixture):**

- A instância usa a fixture configurada em `src/LotofacilMcp.Server/appsettings*.json` (por padrão `tests/fixtures/synthetic_min_window.json`).
- A auditoria usa uma janela declarada de 20 concursos e ancoragem no fim do recorte (no fixture atual, `end_contest_id=3666`).

**O que este runner valida (escopo):**

- Descoberta e allowlist **real** da build (não inferida por código-fonte).
- Invariantes simples de forma e tamanho por métrica (ex.: `frequencia_por_dezena` com 25 posições e soma `15×N`, séries com N pontos, distribuições por linha/coluna como série de blocos de 5).

**O que este runner não substitui:**

- o catálogo normativo (`metric-catalog.md`) e suas fórmulas;
- testes de domínio e contrato; ele é uma evidência rápida e auditável de superfície MCP STDIO para o recorte atual.

- **28.2 — Fechar precondições e bordas no spec (se houver ambiguidade):**
  - Se alguma métrica faltante tiver regra de borda não testável a partir do catálogo (ex.: comprimento de série, política de saturação, smoothing, erro de insuficiência), **não inventar**: abrir o trecho exato do catálogo/ADR aplicável e, se necessário, criar um ajuste mínimo no spec **antes** do código.

- **28.3 — Testes vermelhos (domínio + contrato) para o lote escolhido:**
  - Domínio: testes de fórmula com fixture pequena (e testes de propriedade quando aplicável, ex.: somas, limites, monotonicidade).
  - Contrato: quando a métrica for exposta por tool (ex.: `compute_window_metrics`), testes de shape (`scope`, `shape`, `unit`, `version`) e erros canônicos (ex.: `UNKNOWN_METRIC`) conforme contrato.

- **28.4 — Implementação mínima (somente após 28.3):**
  - Implementar no `Domain` e registrar no dispatcher/registro único (se existente) para que a tool consiga despachar.
  - Manter determinismo e desempates canônicos do catálogo.

- **28.5 — Exposição e discovery coerentes:**
  - Atualizar `discover_capabilities`/registro para refletir a superfície real (sem drift).
  - Atualizar allowlists por rota (quando aplicável) em conjunto com os testes.

- **28.6 — Validação final do objetivo principal (gate de sucesso):**
  - Rodar uma validação “catálogo completo” que falhe se existir métrica canônica no catálogo sem implementação/registro no recorte alvo.
  - Se o projeto optar por implementar em ondas (por tool/rota), a validação deve ser parametrizada por **perfil/build**, mas nunca “silenciosa”.

Critério mínimo de aceite:

- existe um passo executável (template) que, ao ser seguido, implementa **todas** as métricas canônicas faltantes sem improviso;
- existe uma validação final que impede regressão (catálogo diz que existe, mas build não implementa).

Nota operacional: template para pedidos atômicos

- Catálogo completo de **pedidos atômicos por fase** (0–20 do guia e **extensões** posteriores, ex.: Fase 21 alinhada ao [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md), Fase 22 ao [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md), **Fase 23** ao [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md), **Fase 26** ao [ADR 0020](adrs/0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md) e **Fase 27** ao [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md)): [fases-execucao-templates.md](fases-execucao-templates.md). O nome do ficheiro **não** fixa a quantidade de fases; novas entregas normativas podem acrescentar secções no mesmo padrão.
- O template abaixo pode (e deve) ser usado para gerar “pedidos atômicos” para implementação, mantendo o fluxo spec-driven:

```md
Implemente apenas <passo único>.

Referências obrigatórias:
- <spec 1>
- <spec 2>

Arquivos esperados:
- <arquivo A>
- <arquivo B>

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- respeitar fronteiras do ADR 0004 e superfície MCP do ADR 0005;
- seguir nomes canônicos do catálogo/contrato.

Critério de pronto:
- <teste X passa>
- <erro Y é emitido>
- <payload Z contém campos obrigatórios>
```

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
- não extrapolar além do recorte citado (V0, Fase 9/10, Fase 11 de evidências, ou Fase 12 de correção de drift);
- manter TDD;
- respeitar fronteiras do [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md) e superfície MCP do [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md);
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

1. escolher a próxima fatia ([Fase 9](#fase-9-transporte-mcp-protocolo-real--paridade-com-o-contrato) MCP, depois [Fase 10](#fase-10-expandir-tools-documentadas-ondas-b-e-c) por tool);
2. localizar os specs normativos;
3. escrever/ajustar testes;
4. implementar domínio;
5. implementar orquestração;
6. expor no server (**HTTP + MCP** para a mesma tool, salvo exceção documentada);
7. validar contrato nos dois caminhos quando a tool já existir em ambos;
8. atualizar docs se a semântica tiver mudado.

Mudanças de **inter-tool, disponibilidade de métricas em rota, pipeline, fluidez, erros `UNSUPPORTED_STABILITY_CHECK` / pistas de `UNKNOWN_METRIC` ou bateria GAPS** (pares–entropia) devem seguir o [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) em **conjunto** com [mcp-tool-contract.md](mcp-tool-contract.md), [metric-catalog.md](metric-catalog.md), [contract-test-plan.md](contract-test-plan.md) e [test-plan.md](test-plan.md) na **mesma entrega lógica** (spec coerente, testes, depois código), sem alterar a ordem base acima além de referenciar explicitamente a matriz e os cenários A–E do *contract-test-plan*.

Mudanças que introduzam ou alterem **agregados canônicos** (histogramas, padrões e matrizes derivadas) devem seguir o [ADR 0007](adrs/0007-agregados-canonicos-de-janela-v1.md) em conjunto com [mcp-tool-contract.md](mcp-tool-contract.md), [test-plan.md](test-plan.md) e [contract-test-plan.md](contract-test-plan.md), garantindo:

- schema fechado (`aggregate_type` enum, parâmetros explícitos) antes do código;
- testes de contrato (incl. determinismo e ordenação canônica) antes da implementação;
- fixtures/goldens para agregados quando o payload for estável e auditável.

Mudanças em **descoberta para consumidores** (norma *vs.* allowlist por build), **janela por concurso inicial e final (inclusivos)**, mapeamento **`HistoricoTop10MaisSorteados` → `top10_mais_sorteados`**, ou **rótulos de export legado** (`QtdFrequencia`, *etc.*) devem seguir o [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) em conjunto com [mcp-tool-contract.md](mcp-tool-contract.md), [metric-catalog.md](metric-catalog.md), [metric-glossary.md](metric-glossary.md) e [contract-test-plan.md](contract-test-plan.md) (Fase B.2), e cruzar [ADR 0006 D1](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) quando a entrega tocar em `details.allowed_metrics` ou erros ricos. Executar a Fase 23 (secção homónima neste documento e as [templates — Fase 23](fases-execucao-templates.md#fase-23-adr-0008-descoberta-janela-por-extremos-e-mapeamento-legado) em *fases-execucao-templates*) na mesma lógica spec → teste → código.

Entregas que alterem **apenas** a forma de **explicar** resultados de janela a pessoas ou a agentes (tabelas A/B, textos acessíveis, modos *resumo* *vs.* *interpretação* conforme D5) devem seguir o [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md) em conjunto com [metric-glossary.md](metric-glossary.md), sem reabrir fórmulas canónicas sem bump no [metric-catalog.md](metric-catalog.md). A [Fase 27](#fase-27---apresentacao-de-resumos-de-janela-adr-0021) e as [templates — Fase 27](fases-execucao-templates.md#fase-27---adr-0021-apresentacao-de-resumos-de-janela-tabelas-a-b-glossario-d5) cobrem o recorte documental.

Se durante esse ciclo surgir desalinhamento explícito entre spec e implementação, interromper a fatia atual e executar a [Fase 12](#fase-12-correção-de-drift-desalinhamento-spec--implementação) antes de seguir.

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
5. distribuições e séries vetoriais (`distribuicao_`*, `entropia_*`, `hhi_*`)
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

- Pedidos atômicos por fase (e extensões, incl. [Fase 27](fases-execucao-templates.md#fase-27---adr-0021-apresentacao-de-resumos-de-janela-tabelas-a-b-glossario-d5) / [ADR 0021](adrs/0021-apresentacao-resumos-metricas-janela-descricoes-acessiveis-v1.md)) consultáveis em [fases-execucao-templates.md](fases-execucao-templates.md)
- Arquitetura congelada no [ADR 0004](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)
- Superfície MCP + rollout de tools conforme [ADR 0005](adrs/0005-transporte-mcp-e-superficie-tools-v1.md) (quando pós-V0)
- Inter-tool, disponibilidade, pipeline, GAPS: [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) quando a entrega mexe nesses temas
- Descoberta (instância *vs.* norma), janela por extremos, mapeamento legado Top 10 / export: [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) e a Fase 23 (guia + [templates](fases-execucao-templates.md#fase-23-adr-0008-descoberta-janela-por-extremos-e-mapeamento-legado)) quando a entrega mexe nesses temas
- V0 confirmada em [vertical-slice.md](vertical-slice.md)
- Ordem de teste confirmada em [contract-test-plan.md](contract-test-plan.md)
- Métrica inicial confirmada em [metric-catalog.md](metric-catalog.md)
- Contrato inicial confirmado em [mcp-tool-contract.md](mcp-tool-contract.md)
- Estrutura de projetos confirmada em [project-guide.md](project-guide.md)
- Fixture mínima definida
- Teste explícito da barreira de normalização escrito
- Primeiro teste negativo escrito
- Primeiro teste de fórmula escrito
- Primeiro teste do envelope mínimo (`dataset_version`, `tool_version`, `deterministic_hash`) escrito
- Primeiro teste de determinismo escrito

## Recomendação prática

Se houver dúvida sobre “qual passo pedir agora para a IA”, comece sempre por este recorte:

**Até fechar a V0 (Fases 0–8):**

1. fixture mínima;
2. testes vermelhos de normalização, janela e fórmula;
3. tipos canônicos do domínio;
4. métrica `frequencia_por_dezena`;
5. provider de fixture + `dataset_version` + `canonical_json` + hash determinístico;
6. caso de uso;
7. teste de contrato do envelope mínimo;
8. tool/endpoint HTTP;
9. fechamento da V0.

**Depois da V0:**

1. [Fase 9](#fase-9-transporte-mcp-protocolo-real--paridade-com-o-contrato): transporte MCP `stdio` com paridade aos endpoints HTTP;
2. [Fase 10](#fase-10-expandir-tools-documentadas-ondas-b-e-c): uma tool de cada vez, na ordem listada;
3. [Fase 11](#fase-11-fechar-evidências-da-v1-transportes-mcp--catálogo-em-escopo): evidência e documentação alinhadas ao escopo V1;
4. [Fase 12](#fase-12-correção-de-drift-desalinhamento-spec--implementação): executar quando houver desvio entre o que foi especificado e o que foi entregue;
5. [Fase 13](#fase-13-transporte-mcp-via-http-ssestreamable-http): transportar MCP via HTTP como evolução explícita (sem reabrir a Fase 9).

Essa ordem é a forma prática de usar spec-driven neste projeto: **spec → teste → implementação mínima → validação → próxima fatia**.