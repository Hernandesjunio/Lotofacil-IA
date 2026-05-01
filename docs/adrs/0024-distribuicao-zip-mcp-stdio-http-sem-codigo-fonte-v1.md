# ADR 0024 — Distribuição ZIP (self-contained) do MCP para STDIO sem exigir código fonte (v1)

**Navegação:** [← Brief (índice)](../brief.md) · [ADR 0005 (transportes MCP)](0005-transporte-mcp-e-superficie-tools-v1.md) · [ADR 0009 (`help`/resources)](0009-help-e-catalogo-de-templates-resources-v1.md) · [ADR 0011 (`discover_capabilities`)](0011-tool-de-discovery-de-capacidades-por-build-v1.md) · [ADR 0022 (dataset)](0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md)

## Status

Proposto.

**Data:** 2026-05-01

## Contexto

O projeto precisa operar como um **produto instalável** (especialmente para uso em hosts MCP desktop como Cursor), de modo que o usuário consiga:

- executar o MCP via **STDIO** (processo local) para `tools/list` e `tools/call`;
- usar `help` e `discover_capabilities` como onboarding/discovery;
- **sem clonar este repositório** e sem depender da presença de arquivos do código fonte no workspace.

No desenvolvimento, é comum configurar o host MCP apontando para `dotnet run --project ...` dentro do workspace. Isso **não** atende o objetivo de distribuição: o host deve ser capaz de iniciar o servidor com um **executável distribuído**.

Além disso, a superfície MCP **não pode depender** de “descritores locais” (JSON de tools) em diretórios específicos do editor (ex.: `.cursor/projects/...`). A fonte de verdade para descoberta operacional deve ser o **protocolo MCP** (`tools/list`) e as tools meta (`help`, `discover_capabilities`) — ver [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md), [ADR 0009](0009-help-e-catalogo-de-templates-resources-v1.md), [ADR 0011](0011-tool-de-discovery-de-capacidades-por-build-v1.md).

## Decisão

### D1 — Artefato de distribuição v1: pacote ZIP com executável self-contained

Distribuir o servidor como um **ZIP** contendo um executável **self-contained** (por plataforma), de forma que:

- o usuário final não precise de SDK .NET nem do código fonte;
- o host MCP (Cursor) consiga configurar `command` para apontar diretamente para o executável;
- o servidor suporte **MCP STDIO** (para hosts que executam subprocessos).

Observação: **deploy HTTP** (Docker/IIS/cloud) é uma decisão irmã e não é fechada aqui; ver [ADR 0025](0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md).

### D2 — Independência do repositório: sem dependências em paths do workspace

O comportamento do servidor distribuído (ZIP) não pode depender de:

- arquivos presentes no repo (`src/`, `tests/`, `docs/`);
- paths “específicos do editor” (ex.: `.cursor/projects/...`);
- “fixtures default” implícitas.

Consequência: a configuração do dataset permanece **obrigatória** e explícita via `Dataset:DrawsSourceUri` (ver [ADR 0022](0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md)).

### D3 — Descoberta operacional vem do protocolo MCP (não de descritores externos)

Para o usuário final (sem código fonte), a descoberta deve funcionar com:

- `tools/list` (protocolo MCP) para descobrir tools instaladas;
- tool `help` para onboarding e índice de templates/resources (ver ADR 0009);
- tool `discover_capabilities` para discovery técnico de capacidades reais da build (ver ADR 0011).

Observação: arquivos JSON de “descrição de tool” podem existir no repositório ou em ambientes de desenvolvimento como **apoio** (ex.: lint/IDE), mas **não** são requisito de execução do produto distribuído.

### D4 — Interface de linha de comando (CLI) do executável distribuído

O executável distribuído deve suportar ao menos:

- `--mcp-stdio`: inicia o servidor MCP sobre stdin/stdout.

O comportamento padrão (sem flags) deve ser definido de forma explícita no `README.md` para evitar ambiguidade.

### D5 — Conteúdo do ZIP e convenções de instalação

O ZIP deve conter (por plataforma):

- executável do servidor (ex.: `LotofacilMcp.Server.exe` no Windows);
- um `README` curto dentro do ZIP (ou link para o README do repo) com:
  - configuração do host MCP (Cursor) para STDIO;
  - execução em modo HTTP;
  - variáveis de ambiente obrigatórias (dataset).

O ZIP não deve incluir datasets por padrão (para evitar defaults ocultos e para preservar escolha explícita do operador). Se for útil, um dataset de exemplo pode ser distribuído como “sample”, mas deve ser **opt-in** e não selecionado automaticamente.

### D6 — Publicação do ZIP (onde, como versionar, como nomear)

O ZIP deve ser publicado como asset versionado (ex.: em “Releases”), com convenção de nome que inclua plataforma e versão.

Convenção sugerida (não-breaking):

- `lotofacil-ia-mcp-stdio-win-x64-vX.Y.Z.zip`
- `lotofacil-ia-mcp-stdio-linux-x64-vX.Y.Z.zip`
- `lotofacil-ia-mcp-stdio-osx-x64-vX.Y.Z.zip` (quando aplicável)

### D7 — Matriz de suporte (plataformas alvo)

Para evitar expectativa implícita, esta ADR deve declarar quais plataformas são suportadas na v1 do ZIP (mínimo) e quais são planejadas.

Exemplo (ajustar conforme decisão do projeto):

- Obrigatório v1: **Windows x64**
- Planejado: Linux x64, macOS x64/arm64

## Fora de escopo (desta ADR)

- Imagens Docker, Helm charts, IaC (Terraform), scripts de provisionamento.
- Hospedagem HTTP (IIS, PaaS, serverless) e seus detalhes operacionais — ver ADR irmã.
- Políticas de TLS/certificados, autenticação, rate limiting, observabilidade.

## Consequências

### Positivas

- Experiência “instala e usa” no Cursor sem código fonte.
- Mesmo binário pode servir STDIO e HTTP, reduzindo divergência operacional.
- Ajuda/discovery (`help`, `discover_capabilities`) funcionam no produto final e não apenas em dev.

### Trade-offs

- Exige pipeline de build/publish por plataforma.
- Requer documentação cuidadosa para evitar ambiguidades de instalação/configuração (path do executável, env vars, flags).

## Critérios de verificação (aceite)

Considera-se esta ADR implementada quando:

1. Existe um ZIP publicado (Release) contendo um executável self-contained.
2. Um usuário sem o repo consegue:
   - configurar um `mcpServers` no Cursor apontando para o executável e `--mcp-stdio`;
   - executar `help` e `discover_capabilities` com sucesso.
3. Sem `Dataset__DrawsSourceUri`, tools que dependem do histórico retornam `DATASET_UNAVAILABLE` (sem fallback).
4. O ZIP (produto STDIO) não depende de paths do workspace/editor nem de descritores externos para discovery: a descoberta ocorre via `tools/list` + tools meta (`help`, `discover_capabilities`).

## Atualizações de documentação (normativas)

- Atualizar o [brief.md](../brief.md) para referenciar esta ADR.
- Atualizar o `README.md` com:
  - instruções explícitas de execução por **ZIP** (STDIO e HTTP);
  - exemplos completos de `mcpServers` com `command`, `args` e `env`.

