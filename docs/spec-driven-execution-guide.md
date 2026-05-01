# Guia de Execução Spec-Driven — ADRs 0023–0025 (eficiência, distribuição, deploy)

**Navegação:** [← Brief (índice)](brief.md) · [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md) · [ADR 0024](adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md) · [ADR 0025](adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md) · **Guia completo (arquivado):** `docs/archived/spec-driven-execution-guide.md`

Este arquivo foi **reduzido** para ganhar eficiência no consumo de tokens e focar no recorte das ADRs 0023–0025. As fases antigas (V0/V1 e ADRs anteriores) permanecem no guia arquivado.

## Objetivo

Materializar:

- a [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md) (eficiência / knobs / canais),
- a [ADR 0024](adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md) (distribuição ZIP self-contained para MCP STDIO sem repo),
- a [ADR 0025](adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md) (deploy MCP HTTP via Docker/IIS/cloud),

sem quebrar invariantes do contrato e do dataset.

- reduzir tokens (evitar duplicação de JSON no canal textual),
- permitir controle explícito de detalhe (`verbosity`, `include_explanations`),
- permitir **projeção** (`fields`) e paginação determinística quando necessário,
- tornar knobs descobríveis via `help` e `discover_capabilities`,
- manter determinismo e regra clara para `deterministic_hash` quando a apresentação variar.

## Referências normativas mínimas

- [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)
- [ADR 0024](adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md)
- [ADR 0025](adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md)
- [mcp-tool-contract.md](mcp-tool-contract.md)
- [ADR 0005 (transportes)](adrs/0005-transporte-mcp-e-superficie-tools-v1.md)
- (quando aplicável) [ADR 0022 (dataset)](adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md)
- (quando aplicável) [ADR 0008 (descoberta)](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md), [ADR 0009 (help/resources)](adrs/0009-help-e-catalogo-de-templates-resources-v1.md)

---

## Fases (ADR 0023)

### Fase 23.1 — Fechar contrato dos knobs de economia

**Objetivo:** padronizar parâmetros transversais onde aplicável.

- Definir `verbosity`: `minimal | standard | full`.
- Definir `include_explanations` onde aplicável.
- Definir `fields` (ou `response_projection`) para projeção server-side quando a resposta for grande.
- Fechar a regra de `deterministic_hash` quando `verbosity/include_explanations/fields` alterarem a apresentação.

**Pronto quando:** o contrato documenta knobs + hashing sem ambiguidade.

### Fase 23.2 — Separar canais (StructuredContent vs Content) sem duplicar JSON

**Objetivo:** `StructuredContent` é a fonte canônica; `Content` é resumo humano curto.

- Garantir que `Content` não repita o JSON completo do `StructuredContent` por padrão.
- Mapear `verbosity` → tamanho/estilo do resumo humano em `Content`.

**Pronto quando:** em `minimal`, `Content` é curto e não contém JSON completo.

### Fase 23.3 — Projeção (`fields`) e explicações opt-in

**Objetivo:** reduzir payload na fonte.

- Definir por tool quais campos suportam projeção e como validar `fields`.
- Definir por tool quando `include_explanations` é aceito e o que muda.

**Pronto quando:** projeção é determinística e validada (erro claro para campos inválidos).

### Fase 23.4 — Paginação determinística (quando aplicável)

**Objetivo:** controlar respostas grandes em `full` de forma reprodutível.

- Definir esquema de paginação determinística (ex.: `page/page_size` ou `cursor`).
- Fixar ordenação canônica e estabilidade do cursor (quando existir).

**Pronto quando:** mesmas entradas + mesmos knobs retornam o mesmo subconjunto paginado.

### Fase 23.5 — Descoberta/UX: `help` e `discover_capabilities`

**Objetivo:** reduzir tentativa/erro e tornar knobs visíveis.

- `help` explica “modo econômico” vs detalhado (mapeando para knobs).
- `discover_capabilities` declara suporte por tool (knobs, enums aceitos, defaults recomendados) sem payload excessivo.

**Pronto quando:** um leigo consegue pedir `minimal/full` com poucas tentativas.

### Fase 23.6 — Evidências e testes (contrato + determinismo + custo)

**Objetivo:** travar eficiência e determinismo por testes.

- Testes cobrindo ao menos `minimal` e `full` nas tools alvo.
- Testes garantindo: `Content` não duplica JSON; `StructuredContent` permanece canônico.
- Testes cobrindo a regra de `deterministic_hash` definida na Fase 23.1.

**Pronto quando:** regressões de duplicação/knobs/hashing quebram testes.

---

## Fases (ADR 0024 — Distribuição ZIP self-contained para MCP STDIO)

### Fase 24.1 — Fechar o contrato operacional de distribuição (sem repo)

**Objetivo:** tornar a distribuição ZIP um caminho “primeira-classe”, sem dependência do workspace/editor.

- Fixar o modo CLI mínimo do binário distribuído: `--mcp-stdio`.
- Declarar comportamento padrão do executável (sem flags) no `README.md` para evitar ambiguidade.
- Reafirmar a regra: discovery operacional vem de `tools/list` + tools meta (`help`, `discover_capabilities`) — não de descritores externos.
- Reafirmar a regra do dataset: `Dataset__DrawsSourceUri` é obrigatório; sem fallback/fixtures.

**Pronto quando:** documentação fecha “como executar via ZIP” com parâmetros, env vars e discovery, sem depender do repo.

### Fase 24.2 — Publicação self-contained por plataforma (build/publish)

**Objetivo:** gerar um executável que rode sem SDK .NET e sem código fonte.

- Definir a matriz v1 de suporte (mínimo Windows x64; demais como planejado).
- Definir o processo de publish self-contained por plataforma (RID, configuração, output).
- Garantir que o produto publicado não dependa de paths do workspace/editor.

**Pronto quando:** existe um artefato publish self-contained executável por plataforma alvo.

### Fase 24.3 — Empacotamento ZIP v1 (conteúdo e convenções)

**Objetivo:** produzir o ZIP versionado e “instalável” por extração.

- Definir conteúdo mínimo do ZIP: executável + README curto (ou link) com:
  - configuração do host MCP (ex.: Cursor) para STDIO,
  - execução em modo HTTP (quando suportado),
  - env vars obrigatórias (dataset) e exemplos.
- Definir convenção de nome (plataforma + versão).
- Definir política de dataset no ZIP (não incluir por padrão; sample apenas opt-in).

**Pronto quando:** o ZIP é gerado seguindo convenções e sem defaults ocultos.

### Fase 24.4 — Verificação “máquina limpa” (aceite da ADR 0024)

**Objetivo:** validar o cenário real do usuário final sem repositório.

- Validar que `tools/list` funciona iniciando o servidor via executável com `--mcp-stdio`.
- Validar que `help` e `discover_capabilities` funcionam sem repo.
- Validar que, sem `Dataset__DrawsSourceUri`, tools dependentes do histórico retornam `DATASET_UNAVAILABLE` (sem fallback).

**Pronto quando:** critérios de aceite da ADR 0024 são atendidos no cenário sem repo.

---

## Fases (ADR 0025 — Deploy HTTP para MCP HTTP)

### Fase 25.1 — Fechar endpoint MCP HTTP mínimo e modo de execução

**Objetivo:** remover ambiguidade do “HTTP mode” e do endpoint MCP.

- Definir no `README.md` o endpoint MCP HTTP mínimo (ex.: `/mcp`) e como iniciar o servidor em modo HTTP.
- Garantir alinhamento com a ADR 0005: MCP HTTP é protocolo MCP real (não “REST espelhado”).
- Reforçar: `Dataset__DrawsSourceUri` é obrigatório e não há fallback/fixtures.

**Pronto quando:** documentação fecha endpoint mínimo e forma de execução HTTP sem ambiguidade.

### Fase 25.2 — Artefato para deploy HTTP: Docker (quando aplicável)

**Objetivo:** padronizar deploy em ambientes com container.

- Definir (e documentar) o build de imagem Docker para rodar o servidor em modo HTTP.
- Documentar variáveis de ambiente necessárias (dataset) e o mapeamento de portas/URL do endpoint MCP.

**Pronto quando:** existe um caminho documentado e reprodutível para executar via Docker.

### Fase 25.3 — Hosting HTTP via IIS (quando aplicável)

**Objetivo:** suportar Windows Server/IIS como reverse proxy sem desviar do protocolo MCP.

- Documentar publicação/hospedagem ASP.NET Core atrás do IIS (reverse proxy).
- Garantir que o endpoint exposto é MCP real e permanece compatível com host MCP (conexão e `tools/list`/`tools/call`).

**Pronto quando:** existe um guia mínimo de IIS e a rota MCP permanece clara e testável.

### Fase 25.4 — Cloud e restrições de streaming (serverless vs container)

**Objetivo:** evitar “deploy possível” mas operacionalmente inadequado.

- Documentar critérios: quando serverless não é adequado a conexões longas/streaming do transporte MCP HTTP, preferir container/app service.
- Documentar que `Dataset__DrawsSourceUri` deve vir de config/secret do ambiente.

**Pronto quando:** documentação deixa claro o caminho preferencial por classe de ambiente e limitações.

### Fase 25.5 — Verificação de aceite (host MCP via HTTP)

**Objetivo:** validar que o deploy HTTP atende o mínimo.

- Validar conexão de um host MCP via endpoint HTTP mínimo (definido no README) com `tools/list` e `tools/call`.
- Validar que, sem `Dataset__DrawsSourceUri`, tools dependentes do histórico retornam `DATASET_UNAVAILABLE`.

**Pronto quando:** critérios de aceite da ADR 0025 são atendidos no(s) alvo(s) escolhidos (container/IIS/cloud).

