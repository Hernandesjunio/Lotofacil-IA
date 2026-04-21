# ADR 0004 — Estrutura arquitetural inicial do MCP em C# / .NET 10

**Navegação:** [← Brief (índice)](../brief.md) · [README](../../README.md)

## Status

Aceito — congela a estrutura arquitetural inicial para a implementação da V0/V1.

## Contexto

Após o fechamento semântico do domínio ([ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md) e [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md)) e a definição do processo spec-driven como padrão ([ADR 0003](0003-processo-desenvolvimento-bmad-vs-spec-driven.md)), a arquitetura inicial ainda precisava de uma decisão explícita sobre:

1. quantos projetos .NET devem existir no início;
2. como separar domínio, orquestração, infraestrutura e host HTTP;
3. onde ficam determinismo, normalização canônica e validação;
4. se o servidor deve incorporar bibliotecas de IA;
5. como tratar autenticação, throttling e quotas na V0/V1 inicial sem quebrar o contrato MCP;
6. como evitar estrutura cenográfica antes da primeira fatia vertical executável.

A revisão arquitetural concluiu que a estrutura anterior corria dois riscos opostos:

- **subestruturação**, misturando regra semântica, transporte, providers e bootstrap do host;
- **sobre-estruturação**, criando projetos e camadas demais antes da V0, com baixa utilidade real.

## Decisão

### D1 — Estrutura inicial em quatro projetos

O repositório inicia com exatamente estes quatro projetos .NET:

1. `LotofacilMcp.Domain`
2. `LotofacilMcp.Application`
3. `LotofacilMcp.Infrastructure`
4. `LotofacilMcp.Server`

**Justificativa:** quatro projetos são suficientes para proteger a semântica do domínio, isolar IO/serialização, separar orquestração de casos de uso e manter o host HTTP fino, sem criar estrutura cenográfica.

### D2 — Servidor único, HTTP-only na V0/V1 inicial

Há um único host de execução: `LotofacilMcp.Server`, exposto via HTTP.

**Justificativa:** a decisão atual não exige múltiplos transportes nem a separação entre “MCP adapter” e “host”. Manter dois projetos de delivery neste momento introduziria duplicação de bootstrap, DI e tradução de erro sem ganho funcional.

### D3 — Servidor sem IA embarcada

O servidor MCP/HTTP **não** contém orquestração de LLM, interpretação de prompt livre, bibliotecas de AI orchestration ou qualquer dependência funcional de cliente inteligente.

**Justificativa:** o contrato exige servidor stateless por request e sem inferência oculta. O cliente/agente inteligente existe fora do servidor.

### D4 — Fronteiras de responsabilidade por projeto

#### `LotofacilMcp.Domain`

Contém:

- modelos canônicos;
- métricas;
- estratégias;
- composição;
- associações;
- padrões;
- erros semânticos;
- janelas;
- normalização canônica;
- abstrações/ports do núcleo;
- política normativa de determinismo.

Não contém:

- tipos do SDK MCP;
- DTOs HTTP;
- leitura de arquivo;
- banco;
- config de ambiente;
- logging operacional.

#### `LotofacilMcp.Application`

Contém:

- casos de uso;
- coordenação entre domínio e infraestrutura;
- validações cross-field de request;
- mapping interno entre modelos de entrada e objetos do domínio;
- montagem de resultados explicáveis conforme o contrato.

Não contém:

- regra estatística canônica;
- serialização HTTP/MCP;
- detalhes de transporte.

#### `LotofacilMcp.Infrastructure`

Contém:

- providers de dados;
- versionamento concreto de dataset;
- implementação de `canonical_json`;
- hashing;
- IO;
- observabilidade;
- integrações externas.

Não contém:

- semântica de métrica;
- estratégia;
- defaults implícitos de contrato.

#### `LotofacilMcp.Server`

Contém:

- `Program.cs`;
- binding de request;
- tools/endpoints;
- validação estrutural;
- tradução entre payload externo e use case;
- DI;
- configuração;
- toggles operacionais;
- políticas futuras de acesso.

Não contém:

- cálculo estatístico canônico;
- score de estratégia;
- decisões semânticas sobre métricas.

### D5 — Barreira canônica obrigatória entre provider e domínio

Todo dado vindo de provider entra no núcleo como **potencialmente não normalizado** e passa por uma barreira explícita de canonização antes de participar de qualquer cálculo.

**Justificativa:** isso protege o significado de `slot`, a ordenação das dezenas e a independência semântica do domínio em relação à origem dos dados.

### D6 — Determinismo como regra de contrato, não só detalhe de infraestrutura

A política de determinismo (`dataset_version`, `tool_version`, composição do `deterministic_hash`, invariantes de reprodutibilidade) é normativa e protegida pelo núcleo/aplicação. A implementação concreta de RFC 8785 e SHA-256 fica em infraestrutura.

**Justificativa:** evita que uma troca de biblioteca ou detalhe de serialização mude o comportamento do contrato sem revisão arquitetural correspondente.

### D7 — Validação em três níveis

A validação fica explicitamente distribuída assim:

1. **Server** — estrutura do request, tipos básicos, binding e shape externo;
2. **Application** — coerência cross-field e requisitos do caso de uso;
3. **Domain** — invariantes matemáticos, semânticos e regras canônicas.

**Justificativa:** evita validação espalhada e reduz inconsistência entre erros de schema, erros de uso e erros do domínio.

### D8 — Explicabilidade em runtime sem duplicar o catálogo

`metric-catalog.md` continua sendo a fonte de verdade semântica das métricas. O runtime deve produzir explicabilidade estruturada mínima no payload, sem tentar replicar toda a documentação textual dentro da tool.

**Justificativa:** a documentação define o significado; a implementação deve informar o que foi executado de fato: janela, versão, parâmetros efetivos, agregações, filtros, score, `seed_used`, `search_method` e `tie_break_rule` quando aplicável.

### D9 — Prompts e Resources são opcionais e entram só com uso real

As primitivas MCP opcionais (`Prompts` e `Resources`) não entram na estrutura inicial por default. Só devem ser adicionadas quando o primeiro caso real exigir isso e vier acompanhado de teste.

**Justificativa:** o contrato principal continua sendo tool + JSON explícito; superfícies adicionais antes da hora aumentam o risco de drift documental.

### D10 — Autenticação, throttling e quotas ficam reservados por contrato e podem nascer desligados

Os códigos `UNAUTHORIZED`, `RATE_LIMITED` e `QUOTA_EXCEEDED` permanecem documentados no contrato público, mas a V0/V1 inicial pode manter os respectivos mecanismos desabilitados por `feature toggle`.

**Justificativa:** preserva a evolução futura do contrato sem obrigar a implementação prematura de controles operacionais antes da validação da semântica central.

## Consequências

### Positivas

- reduz risco de overengineering inicial;
- reduz risco de infiltração de infraestrutura no domínio;
- facilita TDD por fatia vertical;
- mantém o servidor compatível com cliente/agente inteligente externo sem acoplá-lo a IA;
- torna explícita a matriz de validação por camada;
- preserva o contrato de erro mesmo com controles operacionais desligados.

### Custos

- exige disciplina para não empurrar regra estatística para `Application` ou `Server`;
- exige testes claros para validar a barreira de normalização e o determinismo;
- exige governança de documentação para manter ADR, contrato e testes sincronizados.

## O que fica fora desta decisão

- escolha definitiva do SDK MCP/HTTP em nível de pacote NuGet;
- política futura de autenticação, throttling e quotas quando forem ativados;
- introdução de cache, banco ou pipelines assíncronos;
- estrutura do cliente/agente externo que consumirá o servidor.

## Critérios de verificação

Esta decisão só deve ser considerada implementada quando:

1. a solution refletir os quatro projetos definidos neste ADR;
2. a V0 atravessar `Infrastructure` → `Domain` → `Application` → `Server`;
3. houver teste provando a barreira de normalização;
4. houver teste de determinismo com mesmo `dataset_version` e mesmo `tool_version`;
5. o servidor não depender de bibliotecas de IA para executar tools;
6. a configuração deixar explícito quando mecanismos de acesso estiverem desligados.

## Referências internas

- [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md)
- [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md)
- [ADR 0003](0003-processo-desenvolvimento-bmad-vs-spec-driven.md)
- [project-guide.md](../project-guide.md)
- [vertical-slice.md](../vertical-slice.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
