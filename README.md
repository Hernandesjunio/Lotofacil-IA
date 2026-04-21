# Lotofacil-IA

Projeto para análise e políticas relacionadas à Lotofácil.

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
| [adrs/](docs/adrs/) | Decisões arquiteturais (ADRs) |

Para implementação incremental, use [vertical-slice.md](docs/vertical-slice.md) e [contract-test-plan.md](docs/contract-test-plan.md). A V0/V1 inicial assume servidor HTTP único, sem IA embarcada no servidor, e com autenticação/throttling mantidos como capacidade contratual reservada que pode permanecer desligada por configuração.
