# ADR 0009 — Help e catálogo de templates como Resources (Markdown) v1

**Navegação:** [← Brief (índice)](../brief.md)

## Contexto

Consumidores do MCP (ex.: Cursor, UI web, CLI) têm dificuldade em:

- decidir **janelas** (ex.: 20/100/500) de forma consistente;
- escolher **combinações de métricas** por objetivo (painel geral, repetição, forma, ranking etc.);
- descobrir rapidamente “o que pedir” sem ler todo `docs/`.

Além disso, há um problema recorrente de **experiência de entrada (onboarding)**:

- o usuário pede “getting started” e recebe texto **técnico demais**, com jargões (tools, envelopes, campos) antes de valor prático;
- referências internas (ex.: “ADR 0008”) aparecem em contexto de onboarding, o que é **ruído** para iniciantes;
- pedidos como “liste ajuda” retornam **catálogo grande** (tools + templates) e deixam a pessoa sem saber “qual é o próximo passo”.

Esta ADR existe para reduzir esse atrito: quando a pessoa pede ajuda, ela precisa de um **caminho simples e curto** (menu) e só depois de detalhes.

O contrato atual já reconhece as primitivas opcionais do protocolo MCP:

- **Resources** como *norma estável* (conteúdo read-only, Camada B);
- **Prompts MCP** como ergonomia (templates com argumentos, Camada C);

sem substituir tools determinísticas nem violar a proibição de defaults semânticos no servidor (janela e parâmetros sempre explícitos).

Referências: [docs/mcp-tool-contract.md](../mcp-tool-contract.md) e [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) (Camadas A/B/C).

## Decisão

1) Publicar **10 templates** de uso (Markdown) como **MCP Resources** sob um namespace estável.
2) Publicar um **resource índice** com descrições curtas, janelas sugeridas e um mini-template para “prompt livre”.
3) Publicar um **resource de onboarding curto** (*getting started*) que responda perguntas básicas ("por onde começo", "quais opções", "como usar") de forma **agnóstica ao host**, apontando para `help`, para o índice e para o pipeline mínimo.
3) Adicionar a tool **`help`** que retorna:
   - o `index_markdown` (para UX imediata), e
   - um array `templates[]` (somente descrições/metadados), e
   - o `index_resource_uri` (para clientes que preferem consumir via resources).

4) Padronizar, em todos os templates, uma preferência de exibição **orientada ao cliente/LLM**:
   - `display_mode = simple | advanced | both` (default recomendado: `both` quando não declarado).

5) Recomendar explicitamente uma UX de alternância no cliente:
   - um seletor persistente (Simples | Avançado | Ambos) que controla apenas o `display_mode` no prompt do LLM;
   - o usuário pode alternar a qualquer momento;
   - o template pode aceitar override pontual (ex.: o usuário escrever `display_mode=advanced` no texto).

6) **Priorizar linguagem simples e progressão por camadas (progressive disclosure)** no onboarding e na “ajuda”:
   - primeiro: objetivo do produto + **3 passos** (“por onde começo?”);
   - depois: um **menu curto** (2–4 caminhos comuns) com rótulos humanos (“Painel geral”, “Frequência e atraso”, “Repetição entre concursos”…);
   - por último: detalhes técnicos (nomes canônicos, campos, invariantes), em secção separada “Para integração/DEV”.

7) **Evitar referências internas** (ADRs, nomes de decisões, códigos de fase) nos textos voltados a iniciantes. Se necessário, manter apenas numa secção final “Notas para DEV”.

### Por que Resource e não Tool (para os templates)

- Template é **conteúdo** read-only; não é cálculo nem ação → Resource é a primitiva correta.
- Versionamento e auditoria: templates precisam ser estáveis e rastreáveis.
- “Copiar/colar no chat” funciona melhor com Markdown retornado como resource.

### Por que a tool `help` existe mesmo com resources

- Requisito de produto: “usuário pede help e o MCP responde com o index”.
- Nem todo cliente oferece uma UI boa de `resources/list` + `resources/read`.
- `help` é um **atalho de ergonomia**; não cria defaults, não executa métricas.

## Especificação (v1)

### Resources publicados

- Base URI: `lotofacil-ia://prompts/`
- Base URI (help/onboarding): `lotofacil-ia://help/`
- Índice:
  - `lotofacil-ia://prompts/index@1.0.0`
- Getting started (onboarding curto):
  - `lotofacil-ia://help/getting-started@1.0.0`
- Templates (10):
  - `lotofacil-ia://prompts/dashboard-essentials@1.0.0`
  - `lotofacil-ia://prompts/repetition-overlap@1.0.0`
  - `lotofacil-ia://prompts/neighbors-runs@1.0.0`
  - `lotofacil-ia://prompts/shape-lines-columns@1.0.0`
  - `lotofacil-ia://prompts/frequency-vs-delay@1.0.0`
  - `lotofacil-ia://prompts/blocks-presence-absence@1.0.0`
  - `lotofacil-ia://prompts/ranking-stability@1.0.0`
  - `lotofacil-ia://prompts/regime-shift@1.0.0`
  - `lotofacil-ia://prompts/associations-sanity@1.0.0`
  - `lotofacil-ia://prompts/candidate-vs-history-screening@1.0.0`

Cada resource tem `mimeType = text/markdown`.

### Conteúdo mínimo de cada template

- Objetivo (descritivo, não preditivo)
- Preferência de exibição (`display_mode`) com regras curtas para:
  - **simple** (linguagem simples, sem jargão; poucas linhas)
  - **advanced** (nomes canônicos, janelas, números-chave e limitações)
  - **both** (primeiro simples, depois avançado)
- Janelas sugeridas + racional
- Lista de métricas e tools envolvidas
- Checklist de rastreabilidade (`dataset_version`, `tool_version`, `deterministic_hash`, `window`)
- Instruções de fallback quando a build não expõe determinada métrica (`UNKNOWN_METRIC`)

### Resource `index`

Deve conter:

- duas opções de UX: “prompt livre” e “usar templates”;
- para cada template: título + 1–2 linhas de descrição + janelas sugeridas + URI;
- mini-template de “prompt livre” incluindo `display_mode`;
- instruções práticas para alternar `display_mode` no cliente (toggle persistente + override textual);
- lembrete: janela explícita e sem defaults de UI legado;
- recomendação para ancorar `end_contest_id` via `get_draw_window(window_size=1)` quando necessário.

### Resource `getting-started` (onboarding curto)

Deve conter (Markdown, curto e copiável):

- resposta direta para perguntas do tipo:
  - "por onde eu começo?"
  - "como eu começo?"
  - "qual é o primeiro passo?"
  - "o que eu posso pedir aqui?"
  - "o que este MCP faz?"
  - "como funciona este MCP?"
  - "como usar este MCP?"
  - "como usar via MCP STDIO?"
  - "quais tools existem?"
  - "quais recursos (resources) existem?"
  - "tem exemplos?"
  - "me dá um exemplo rápido"
  - "qual template eu devo usar?"
  - "qual template serve para (X)?"
  - "qual janela devo usar? 20/100/500?"
  - "preciso informar end_contest_id?"
  - "como eu descubro o contest_id mais recente?"
  - "como eu vejo dataset_version/tool_version?"
  - "como eu valido que está determinístico?"
  - "o que significa deterministic_hash?"
  - "por que deu UNKNOWN_METRIC?"
  - "por que deu INVALID_REQUEST?"
  - "quais são as opções?"
  - "o que você consegue fazer?"
  - "quais caminhos existem?"
  - "consegue me ajudar?"
  - "pode me guiar?"
- ordem recomendada de descoberta (agnóstica ao host):
  1. chamar a tool `help`;
  2. ler `lotofacil-ia://prompts/index@1.0.0`;
  3. escolher um template (ou "prompt livre") e executar o pipeline mínimo de tools.
- pipeline mínimo (sem defaults ocultos):
  - `get_draw_window` (opcional, quando precisar do recorte bruto ou ancorar extremos)
  - `compute_window_metrics` (batch)
  - tools analíticas conforme a pergunta (`analyze_indicator_stability`, `summarize_window_patterns`, `analyze_indicator_associations`, etc.)

#### Regras de linguagem e estrutura (onboarding para leigos)

- O conteúdo principal deve ser **leigo-first**:
  - usar termos humanos (“período”, “último concurso”, “painel”) e só depois (se necessário) mencionar campos técnicos entre parênteses;
  - evitar jargões como “ADR”, “allowlist”, “envelope” no texto principal.
- Deve começar com um **guia de 3 passos**, com CTA claro (ex.: “Peça ajuda”, “Escolha um caminho”, “Escolha o período”).
- Deve oferecer um **menu curto** (2–4 opções) de “o que fazer agora”, por exemplo:
  - “Quero um painel geral (recomendado para começar)”
  - “Quero ver frequência e atraso”
  - “Quero entender repetição entre concursos”
  - “Quero analisar forma (linhas/colunas)”
- Deve ter uma secção “Se der erro” com explicação humana do que fazer, sem despejar códigos.
- Os lembretes normativos podem existir, mas preferir forma simples:
  - janela/período sempre informado pelo usuário (sem “últimos N” escondido);
  - evitar linguagem preditiva (“vai sair”, “garantia”);
  - rastreabilidade/determinismo podem ficar como “Detalhes (para DEV)” se o público-alvo for iniciante.

### Tool `help`

- Nome: `help`
- Input: nenhum (v1)
- Output (envelope simples):
  - `tool_version`
  - `getting_started_resource_uri` (opcional; recomendado quando o resource existir)
  - `index_resource_uri`
  - `index_markdown`
  - `templates[]`: `{ id, resource_uri, title, description, suggested_windows }`

#### Extensão recomendada (não-breaking) para UX (v1.x)

Para reduzir confusão no primeiro contato, `help` pode incluir campos opcionais adicionais (sem quebrar clientes existentes), por exemplo:

- `quick_start_markdown`: “o que fazer agora” em 3 passos + menu curto (2–4 caminhos);
- `recommended_entrypoints[]`: lista curta de templates com rótulo “comece aqui”.

**Invariantes:**

- `help` não executa cálculo de métricas.
- `help` não decide janela.
- `help` não deve conter linguagem de previsão.

## Consequências

### Positivas

- onboarding rápido (help como ponto de entrada)
- reduz erro de requests por janela implícita
- templates prontos para copiar/colar em chats com LLM
- resources são consumíveis também por UIs não-LLM

### Trade-offs

- manutenção editorial: os 10 templates e o índice precisam acompanhar mudanças de contrato/métricas
- versionamento: alterações relevantes exigem bump e preservação de URIs antigos quando necessário

## Alternativas consideradas

1) Apenas resources, sem tool `help`:
- mais “puro”, mas UX pior em clientes sem UI de resources.

2) Uma tool JSON (menu) em vez de markdown:
- melhor para UI, pior para “copiar/colar”.

3) Prompts MCP (`prompts/list`, `prompts/get`) como substituto:
- útil quando há argumentos formais; aqui o objetivo é template human-friendly em Markdown.

## Plano spec-driven (documentação e testes)

- Atualizar `docs/brief.md` para mencionar `help` e o catálogo de resources.
- Atualizar `docs/prompt-catalog.md` com prompts de discovery/ajuda (mapeando para `help`).
- Atualizar `docs/test-plan.md`:
  - incluir cobertura de tool `help`;
  - incluir cobertura de `resources/list` e `resources/read` para o índice e ao menos 1 template.
- Atualizar `docs/spec-driven-execution-guide.md` e `docs/fases-execucao-templates.md` com uma fase atômica nova para “Help + Resources”.

