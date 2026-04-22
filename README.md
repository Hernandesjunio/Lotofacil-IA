# Lotofacil-IA

Projeto para análise e políticas relacionadas à Lotofácil.

## Metodologia de desenvolvimento

Este repositório segue **spec-driven development**: a implementação não começa pelo código “livre”, mas pelos artefatos normativos em `docs/`.

Isso significa que:

- a **fonte de verdade semântica** está na documentação versionada;
- cada entrega nasce de um **recorte explícito do spec**;
- cada recorte precisa de **teste correspondente**;
- mudanças de semântica exigem atualização coordenada de **docs + testes + código**;
- o trabalho é executado em **fatias verticais pequenas**, começando pela V0;
- a implementação segue **TDD**, com foco em contrato, fórmula, determinismo e erros.

### Fluxo da aplicação (visão geral)

O serviço expõe ferramentas MCP/HTTP; o cliente (host ou agente) envia parâmetros alinhados ao contrato. O servidor delega a casos de uso, que combinam regras do domínio com dados versionados da infraestrutura e devolvem JSON determinístico e explicável.

```mermaid
flowchart TD
  Host["Cliente: host MCP / agente de IA"]
  Server["Servidor HTTP/MCP"]
  App["Aplicação: orquestração e validação"]
  Domain["Domínio: métricas, janelas, estratégias, candidatos"]
  Infra["Infraestrutura: snapshot, dataset_version, JSON canônico"]

  Host <-->|tools / JSON| Server
  Server --> App
  App --> Domain
  App --> Infra
  Infra -->|concursos e metadados| App
  Domain -->|cálculos e composições| App
  App -->|resposta com rastreabilidade| Server
```

Em termos práticos, a ordem de trabalho é:

1. definir ou confirmar o spec aplicável;
2. escolher a próxima fatia;
3. escrever os testes do recorte;
4. implementar o mínimo necessário;
5. validar contrato e determinismo;
6. só então avançar para a próxima fatia.

Os documentos centrais dessa metodologia são:

- [docs/brief.md](docs/brief.md) — contexto e mapa da documentação
- [docs/adrs/0003-processo-desenvolvimento-bmad-vs-spec-driven.md](docs/adrs/0003-processo-desenvolvimento-bmad-vs-spec-driven.md) — spec-driven como padrão
- [docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md](docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md) — arquitetura inicial congelada
- [docs/vertical-slice.md](docs/vertical-slice.md) — primeira fatia obrigatória
- [docs/contract-test-plan.md](docs/contract-test-plan.md) — ordem inicial de execução
- [docs/spec-driven-execution-guide.md](docs/spec-driven-execution-guide.md) — passo a passo operacional

## Como executar no host MCP (stdio)

Para clientes MCP desktop (ex.: Cursor), use o mesmo executável `LotofacilMcp.Server` em modo `stdio`:

```json
{
  "mcpServers": {
    "lotofacil-ia": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:/_projeto/Lotofacil-IA/src/LotofacilMcp.Server/LotofacilMcp.Server.csproj",
        "--",
        "--mcp-stdio"
      ]
    }
  }
}
```

Nesse modo o host MCP consegue descobrir e invocar as tools atualmente entregues no recorte V1 (`get_draw_window`, `compute_window_metrics` e `analyze_indicator_stability`) com a mesma semântica JSON usada nos POSTs HTTP `/tools/*`.

## Estrutura

| Caminho | Descrição |
|---------|-----------|
| [docs/brief.md](docs/brief.md) | Brief e escopo do projeto (índice da documentação em `docs/`) |
| [docs/metric-catalog.md](docs/metric-catalog.md) | Catálogo de métricas |
| [docs/vertical-slice.md](docs/vertical-slice.md) | Fatia vertical mínima e critérios de aceite |
| [docs/contract-test-plan.md](docs/contract-test-plan.md) | Plano de testes de contrato e fixtures douradas |
| `src/LotofacilMcp.Domain/` | Núcleo semântico: métricas, estratégias, janelas, erros e normalização |
| `src/LotofacilMcp.Application/` | Casos de uso, validação cross-field e orquestração |
| `src/LotofacilMcp.Infrastructure/` | Providers, dataset versioning, canonical JSON e observabilidade |
| `src/LotofacilMcp.Server/` | Servidor HTTP/MCP, tools, DI e toggles operacionais |
| `tests/fixtures/` | Dados e fixtures de teste (convênio em [contract-test-plan.md](docs/contract-test-plan.md)) |

## Documentação

O ponto de entrada da pasta **`docs/`** é o [**brief**](docs/brief.md): escopo, restrições e links para os demais artefatos. Em qualquer outro `.md` dessa pasta há navegação de volta ao brief e ao README.

| Documento | Conteúdo |
|-----------|------------|
| [brief.md](docs/brief.md) | Contexto, escopo e mapa da documentação |
| [metric-catalog.md](docs/metric-catalog.md) | Métricas (tipagem e fórmulas) |
| [metric-glossary.md](docs/metric-glossary.md) | Glossário pedagógico das métricas |
| [mcp-tool-contract.md](docs/mcp-tool-contract.md) | Contrato das ferramentas MCP |
| [generation-strategies.md](docs/generation-strategies.md) | Estratégias de geração |
| [project-guide.md](docs/project-guide.md) | Estrutura e convenções do projeto |
| [spec-driven-execution-guide.md](docs/spec-driven-execution-guide.md) | Guia prático de execução spec-driven |
| [vertical-slice.md](docs/vertical-slice.md) | Fatia vertical mínima (V0) |
| [contract-test-plan.md](docs/contract-test-plan.md) | Plano de testes de contrato |
| [test-plan.md](docs/test-plan.md) | Plano de testes do domínio |
| [live-openai-integration-pipeline.md](docs/live-openai-integration-pipeline.md) | Integração real com ChatGPT (OpenAI), suíte L1–L5 e workflow manual no GitHub |
| [prompt-catalog.md](docs/prompt-catalog.md) | Catálogo de prompts para testes |
| [0001-fechamento-semantico-e-determinismo-v1.md](docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md) | ADR: fechamento semântico e determinismo (v1) |
| [0002-composicao-analitica-e-filtros-estruturais-v1.md](docs/adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md) | ADR: composição analítica e filtros estruturais (v1) |
| [0003-processo-desenvolvimento-bmad-vs-spec-driven.md](docs/adrs/0003-processo-desenvolvimento-bmad-vs-spec-driven.md) | ADR: processo de desenvolvimento (BMAD vs spec-driven) |
| [0004-estrutura-arquitetural-inicial-mcp-dotnet10.md](docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md) | ADR: estrutura arquitetural inicial (MCP, .NET 10) |

Para implementação incremental, use [vertical-slice.md](docs/vertical-slice.md) e [contract-test-plan.md](docs/contract-test-plan.md). A V0/V1 inicial assume servidor HTTP único, sem IA embarcada no servidor, e com autenticação/throttling mantidos como capacidade contratual reservada que pode permanecer desligada por configuração.
