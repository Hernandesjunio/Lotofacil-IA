# ADR 0025 — Deploy HTTP (Docker/IIS/cloud) para MCP HTTP (v1)

**Navegação:** [← Brief (índice)](../brief.md) · [ADR 0005 (transportes MCP)](0005-transporte-mcp-e-superficie-tools-v1.md) · [ADR 0022 (dataset)](0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md) · [ADR 0024 (ZIP/STDIO)](0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md)

## Status

Proposto.

**Data:** 2026-05-01

## Contexto

Além do uso em hosts desktop via **MCP STDIO** (tratado na [ADR 0024](0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md)), o servidor também deve poder ser hospedado como **serviço HTTP** para:

- consumo por URL (hosts MCP que conectam via endpoint MCP real);
- ambientes de servidor (VM/IIS), PaaS e container;
- depuração e testes operacionais.

Isso deve manter as invariantes do projeto:

- sem defaults ocultos de dataset (configuração obrigatória; ver ADR 0022);
- MCP HTTP é **protocolo MCP real** (SSE/Streamable) — não confundir com endpoints REST espelhados (ver ADR 0005).

## Decisão

### D1 — Artefato preferencial para deploy HTTP: imagem Docker (quando aplicável)

Para ambientes que suportam containers, o artefato preferencial é uma **imagem Docker** executando o mesmo servidor em modo HTTP.

### D2 — IIS (Windows Server) é suportado como hosting HTTP

O servidor pode ser publicado/hospedado como app ASP.NET Core atrás do IIS (reverse proxy), desde que exponha um endpoint MCP real conforme ADR 0005.

### D3 — Cloud serverless (Azure Functions / AWS Lambda / GCP Functions/Run)

É considerado “coberto” quando o alvo consegue:

- expor um endpoint HTTP compatível com o transporte MCP HTTP escolhido;
- manter o comportamento operacional estável (inclusive para **conexões longas/streaming** do transporte MCP HTTP, quando aplicável);
- receber `Dataset__DrawsSourceUri` por **configuração do ambiente** (config/secret), conforme [ADR 0022](0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md).

#### D3.1 — Critério operacional: quando **não** usar serverless

Quando a plataforma serverless **não for adequada** a **conexões longas** e/ou **streaming** exigidos pelo transporte MCP HTTP (ex.: SSE / respostas streamadas), o caminho preferencial é **container/app service** (PaaS com processo/instância estável) em vez de serverless.

Indicadores típicos de “não adequado” (não exaustivo):

- **Timeouts rígidos** (request/function) que encerram a conexão antes do fim do streaming.
- **Infra de front door/gateway** que faz buffer e **não entrega streaming** ao cliente.
- **Cold start / escala para zero** que degrada a experiência de sessões longas.
- **Restrições de conexões concorrentes** que tornam SSE/streaming instável sob carga.

### D4 — Endpoint MCP HTTP mínimo (para evitar ambiguidade)

O deploy HTTP deve expor ao menos **um endpoint MCP real** (protocolo) conforme ADR 0005. O endpoint mínimo obrigatório (v1) deve ser definido no `README.md` (ex.: `/mcp`).

### D5 — Configuração do dataset permanece obrigatória

`Dataset__DrawsSourceUri` é obrigatória e **deve vir de configuração do ambiente** (ex.: variável de ambiente / secret do provider) — **não** deve haver fallback para fixtures internas (ver [ADR 0022](0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md)).

Nota operacional: quando `Dataset__DrawsSourceUri` contiver credenciais (ex.: token/SAS em URL HTTP), tratar como **secret** (não logar o valor completo; armazenar em secret manager do ambiente).

## Critérios de verificação (aceite)

1. Existe um modo documentado de executar o servidor como HTTP service em:
   - container (quando aplicável), e
   - IIS (quando aplicável).
2. O endpoint MCP HTTP mínimo (definido no README) aceita conexão de um host MCP e permite `tools/list` e `tools/call`.
3. Sem `Dataset__DrawsSourceUri`, tools dependentes do histórico retornam `DATASET_UNAVAILABLE`.

## Fora de escopo

- Terraform/Helm/Kustomize e scripts de provisionamento detalhados.
- Estratégia de TLS/certificados, autenticação e rate limit.
- Observabilidade (logs/metrics/traces) além do necessário para diagnóstico básico.

