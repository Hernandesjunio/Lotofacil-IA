# Templates atômicos de execução — ADRs 0023–0025 (eficiência, distribuição, deploy)

**Navegação:** [← Brief (índice)](brief.md) · [spec-driven-execution-guide.md](spec-driven-execution-guide.md) · [ADR 0023](adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md) · [ADR 0024](adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md) · [ADR 0025](adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md) · **Templates antigos (arquivado):** `docs/archived/fases-execucao-templates.md`

Este arquivo contém **somente** os templates das fases presentes no `docs/spec-driven-execution-guide.md` (enxugado para ADRs 0023–0025).

**Regra de ouro:** implemente **apenas** o recorte descrito na fase.

---

## Fase 23.1 — Fechar contrato dos knobs de economia

```md
Implemente apenas o recorte normativo da ADR 0023 no contrato:

- Definir `verbosity`: `minimal | standard | full`.
- Definir `include_explanations` onde aplicável.
- Definir `fields` (ou `response_projection`) para projeção server-side em respostas grandes.
- Fechar a regra de `deterministic_hash` quando `verbosity/include_explanations/fields` alterarem a apresentação.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Contrato documenta knobs + hashing sem ambiguidade.
```

## Fase 23.2 — Separar canais (StructuredContent vs Content) sem duplicar JSON

```md
Implemente apenas a política de canais da ADR 0023:

- `StructuredContent` permanece como JSON canônico.
- `Content` vira resumo humano curto, alinhado a `verbosity`.
- Proibir duplicar o JSON completo do `StructuredContent` dentro do `Content` por padrão.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Em `verbosity="minimal"`, `Content` não contém JSON completo e segue útil como resumo.
```

## Fase 23.3 — Projeção (`fields`) e explicações opt-in

```md
Implemente apenas projeção e explicações opt-in:

- Definir por tool quais campos suportam `fields` e como validar.
- Definir por tool quando `include_explanations` é aceito e o que muda na resposta.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Projeção é determinística e valida `fields` com erro estruturado quando inválido.
```

## Fase 23.4 — Paginação determinística (quando aplicável)

```md
Implemente apenas paginação determinística para respostas grandes em `full`:

- Definir esquema (`page/page_size` ou `cursor`) e limites.
- Fixar ordenação canônica e estabilidade do cursor (quando existir).

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Mesmas entradas + mesmos knobs retornam o mesmo subconjunto paginado.
```

## Fase 23.5 — Descoberta/UX: `help` e `discover_capabilities`

```md
Implemente apenas a descoberta/UX dos knobs:

- `help` explica como pedir "modo econômico" vs detalhado, mapeando para `verbosity/include_explanations/fields`.
- `discover_capabilities` declara quais tools suportam quais knobs, enums aceitos e defaults recomendados, sem payload excessivo.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md (alinhamento de descoberta)
- docs/adrs/0009-help-e-catalogo-de-templates-resources-v1.md (quando aplicável)

Critério de pronto:
- Um leigo consegue pedir `minimal`/`full` com poucas tentativas e sem tentativa/erro de parâmetros.
```

## Fase 23.6 — Evidências e testes (contrato + determinismo + custo)

```md
Implemente apenas as evidências e testes para travar eficiência:

- Testes de contrato cobrindo ao menos `minimal` e `full` nas tools alvo.
- Teste garantindo que `Content` não duplica JSON do `StructuredContent`.
- Testes cobrindo a regra de `deterministic_hash` definida para knobs de apresentação.

Referências obrigatórias:
- docs/adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md
- docs/mcp-tool-contract.md

Critério de pronto:
- Regressões de duplicação/knobs/hashing quebram testes.
```

---

## Fase 24.1 — Fechar o contrato operacional de distribuição (sem repo)

```md
Implemente apenas o fechamento do contrato operacional da ADR 0024 (sem repo):

- Fixar o modo CLI mínimo do binário distribuído: `--mcp-stdio`.
- Definir o comportamento padrão do executável (sem flags) no README para evitar ambiguidade.
- Reforçar que discovery operacional vem de `tools/list` + tools meta (`help`, `discover_capabilities`) — não de descritores externos.
- Reforçar que `Dataset__DrawsSourceUri` é obrigatório e não existe fallback/fixtures.

Referências obrigatórias:
- docs/adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md
- docs/adrs/0011-tool-de-discovery-de-capacidades-por-build-v1.md
- docs/adrs/0009-help-e-catalogo-de-templates-resources-v1.md
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- Documentação fecha “como executar via ZIP (STDIO)” com parâmetros, env vars e discovery sem depender do repo.
```

## Fase 24.2 — Publicação self-contained por plataforma (build/publish)

```md
Implemente apenas o recorte de publish self-contained da ADR 0024:

- Declarar matriz v1 de suporte (mínimo Windows x64; demais como planejado).
- Definir o processo de publish self-contained por plataforma (RID/config/output) como artefato executável sem SDK.
- Garantir que o publicado não dependa de paths do workspace/editor.

Referências obrigatórias:
- docs/adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md

Critério de pronto:
- Existe um artefato publish self-contained executável por plataforma alvo.
```

## Fase 24.3 — Empacotamento ZIP v1 (conteúdo e convenções)

```md
Implemente apenas o empacotamento ZIP v1 da ADR 0024:

- Definir conteúdo mínimo do ZIP: executável + README curto (ou link) com:
  - configuração do host MCP (ex.: Cursor) para STDIO,
  - execução em modo HTTP (quando suportado),
  - env vars obrigatórias (dataset) e exemplos.
- Definir convenção de nome do ZIP (plataforma + versão).
- Definir política de dataset no ZIP (não incluir por padrão; sample apenas opt-in).

Referências obrigatórias:
- docs/adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- O ZIP é gerado seguindo convenções e sem defaults ocultos.
```

## Fase 24.4 — Verificação “máquina limpa” (aceite da ADR 0024)

```md
Implemente apenas a verificação de aceite da ADR 0024 no cenário sem repo:

- Validar que `tools/list` funciona iniciando o servidor via executável com `--mcp-stdio`.
- Validar que `help` e `discover_capabilities` funcionam sem repo.
- Validar que, sem `Dataset__DrawsSourceUri`, tools dependentes do histórico retornam `DATASET_UNAVAILABLE` (sem fallback).

Referências obrigatórias:
- docs/adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- Critérios de aceite da ADR 0024 são atendidos no cenário sem repositório.
```

---

## Fase 25.1 — Fechar endpoint MCP HTTP mínimo e modo de execução

```md
Implemente apenas o fechamento do endpoint MCP HTTP mínimo (ADR 0025):

- Definir no README o endpoint MCP HTTP mínimo (ex.: `/mcp`) e como iniciar o servidor em modo HTTP.
- Garantir alinhamento com ADR 0005: MCP HTTP é protocolo MCP real (não REST espelhado).
- Reforçar: `Dataset__DrawsSourceUri` é obrigatório e não existe fallback/fixtures.

Referências obrigatórias:
- docs/adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- Documentação fecha endpoint mínimo e forma de execução HTTP sem ambiguidade.
```

## Fase 25.2 — Artefato para deploy HTTP: Docker (quando aplicável)

```md
Implemente apenas o recorte Docker da ADR 0025:

- Definir e documentar o build de imagem Docker para rodar o servidor em modo HTTP.
- Documentar env vars necessárias (dataset) e o mapeamento de portas/URL do endpoint MCP.

Referências obrigatórias:
- docs/adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- Existe um caminho documentado e reprodutível para executar via Docker.
```

## Fase 25.3 — Hosting HTTP via IIS (quando aplicável)

```md
Implemente apenas o recorte IIS da ADR 0025:

- Documentar publicação/hospedagem ASP.NET Core atrás do IIS (reverse proxy).
- Garantir que o endpoint exposto é MCP real e permanece compatível com host MCP (conexão e `tools/list`/`tools/call`).

Referências obrigatórias:
- docs/adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md
- docs/adrs/0005-transporte-mcp-e-superficie-tools-v1.md

Critério de pronto:
- Existe um guia mínimo de IIS e a rota MCP permanece clara e testável.
```

## Fase 25.4 — Cloud e restrições de streaming (serverless vs container)

```md
Implemente apenas o recorte de decisão operacional da ADR 0025 para cloud:

- Documentar critérios: quando serverless não é adequado a conexões longas/streaming do transporte MCP HTTP, preferir container/app service.
- Documentar que `Dataset__DrawsSourceUri` deve vir de config/secret do ambiente.

Referências obrigatórias:
- docs/adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- Documentação deixa claro o caminho preferencial por classe de ambiente e limitações.
```

## Fase 25.5 — Verificação de aceite (host MCP via HTTP)

```md
Implemente apenas a verificação de aceite da ADR 0025:

- Validar conexão de um host MCP via endpoint HTTP mínimo (definido no README) com `tools/list` e `tools/call`.
- Validar que, sem `Dataset__DrawsSourceUri`, tools dependentes do histórico retornam `DATASET_UNAVAILABLE`.

Referências obrigatórias:
- docs/adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md
- docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md

Critério de pronto:
- Critérios de aceite da ADR 0025 são atendidos no(s) alvo(s) escolhidos (container/IIS/cloud).
```

