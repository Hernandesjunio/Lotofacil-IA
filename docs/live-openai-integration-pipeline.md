# Integração real com OpenAI (ChatGPT) e esteira GitHub

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Este documento define **um tipo específico de teste de integração**: execução **automatizada** em que um **modelo da OpenAI** (API compatível com ChatGPT / Chat Completions ou Responses, conforme implementação) recebe **prompts em linguagem natural**, decide **quais tools MCP** invocar e com **quais argumentos**, e o teste **valida** que o fluxo ponta a ponta permanece **funcional** e **alinhado ao contrato**.

Ele existe para complementar testes determinísticos (fórmula, contrato, propriedades) descritos em [test-plan.md](test-plan.md), **sem** substituí-los.

---

## 1. Objetivo

- Garantir, com **custo controlado**, que o projeto continua **operável de ponta a ponta** quando exposto a um **agente real** (modelo + tool calling).
- Detectar regressões que só aparecem com **roteamento por LLM** (escolha errada de tool, parâmetros omitidos, recusa indevida ou aceite indevido de prompt).
- Manter essa validação **fora** da esteira principal de PR/commit, para **não** pagar API nem introduzir flakiness em cada push.

## 2. Definições (para evitar ambiguidade)

| Termo | Significado neste repositório |
|--------|-------------------------------|
| **Integração real (OpenAI)** | Teste que **chama a API da OpenAI** com credencial de produção ou de projeto (nunca commitada) e executa o fluxo agente → tools MCP → asserções. |
| **Esteira dedicada** | Workflow do GitHub Actions **separado** dos workflows de CI “rápida” (build/test sem API paga). |
| **Suíte mínima** | Conjunto **obrigatório** de **cinco** cenários documentados na [seção 6](#6-suíte-mínima-obrigatória-cinco-cenários) (mapeados ao [prompt-catalog.md](prompt-catalog.md)). |
| **Funcional (neste contexto)** | O cenário passa se as **tool calls** observadas e os **payloads** (ou erros esperados) respeitam o [mcp-tool-contract.md](mcp-tool-contract.md) e os critérios da tabela de cenários; **não** se exige texto natural idêntico entre execuções. |

## 3. O que este tipo de teste cobre e o que não cobre

**Cobre:**

- Seleção de **tool(s)** adequada(s) a partir de prompt natural (com janela explícita quando o domínio exige).
- Montagem de **JSON válido** para entrada da tool, quando o modelo usa tool calling.
- Caminho feliz **ponta a ponta** com dados de fixture ou snapshot conforme [contract-test-plan.md](contract-test-plan.md).

**Não substitui:**

- Prova matemática de métricas (camada **fórmula**).
- Validação exaustiva de schema (**contrato**) — continua obrigatória sem API.
- Prova de determinismo do servidor com mesma entrada (**propriedades** / `deterministic_hash`) — o LLM **não** é determinístico; o servidor continua sendo validado nos testes sem modelo.

## 4. Política de custo e frequência

- **Não** executar esta suíte em todo `push` ou em todo PR por padrão.
- Execução esperada: **manual** (`workflow_dispatch`), ou gatilhos **esporádicos** acordados pelo time (ex.: **diário** em `main`, **semanal**, ou **após release**). Qualquer aumento de frequência deve ser **explícito** no workflow e neste documento.
- Definir **teto** de tokens por execução na implementação dos testes (modelo mais barato adequado ao tool calling, prompts curtos, limite de rodadas do agente).

## 5. Segredos, variáveis de ambiente e modelo

### 5.1 Secret obrigatório no GitHub

| Secret | Obrigatório | Uso |
|--------|-------------|-----|
| `OPENAI_API_KEY` | Sim, para a esteira dedicada | Autenticação na API OpenAI. |

**Proibições explícitas:**

- Não commitar chave em repositório, fork público ou logs.
- Não reutilizar chave pessoal em CI compartilhada sem política de rotação; preferir **chave de projeto** com limite de gasto na OpenAI.

### 5.2 Variáveis opcionais (recomendadas)

Definir como **variáveis** do repositório ou do ambiente do workflow (não como texto no código):

| Variável | Exemplo | Finalidade |
|----------|---------|------------|
| `OPENAI_MODEL` | `gpt-4o-mini` | Modelo usado nos testes (fixar para comparações e custo previsível). |
| `OPENAI_BASE_URL` | *(vazio)* | Só preencher se usar **proxy compatível** com a API OpenAI; caso contrário, omitir. |
| `LIVE_OPENAI_MAX_ROUNDS` | `8` | Limite de mensagens/tool rounds por cenário (evita loop de custo). |

### 5.3 Execução local (desenvolvedor)

- Fornecer a mesma `OPENAI_API_KEY` via ambiente do usuário ou arquivo **não versionado** (ex.: `.env.local` no `.gitignore` quando existir política de projeto).
- O [README](../README.md) e o [project-guide.md](project-guide.md) podem detalhar o comando exato quando o projeto `LotofacilMcp.sln` existir; até lá, vale o contrato deste documento: **mesmo filtro de testes** da CI dedicada.

## 6. Suíte mínima obrigatória (cinco cenários)

Cada linha é **um** teste de integração real independente. Os textos dos prompts são os do catálogo (copiar literalmente na implementação para evitar deriva).

| ID | Prompt (referência [prompt-catalog.md](prompt-catalog.md)) | Tools esperadas (mínimo) | O que o teste deve validar |
|----|------------------------------------------------------------|---------------------------|----------------------------|
| **L1** | §1 item 1 — estabilidade nos últimos 20 | `analyze_indicator_stability` | Tool correta; parâmetros de janela coerentes; resposta utilizável sem erro de contrato. |
| **L2** | §2 item 4 — composição 40/30/30 (frequência, atraso invertido, blocos de ausência) | `compose_indicator_analysis` | Pesos declarados mapeados para payload; nenhuma tool com argumentos inventados fora do contrato. |
| **L3** | §3 item 7 — Spearman entre indicadores nos últimos 100 | `analyze_indicator_associations` | Método e janela explícitos; séries compatíveis conforme contrato. |
| **L4** | §4 item 10 — padrão em vizinhos, pares e entropia (últimos 20) | `summarize_window_patterns` | Agregações permitidas; escopo de janela respeitado. |
| **L5** | §10 item 30 — geração com `declared_composite_profile`, `greedy_topk`, `seed` 424242 | `generate_candidate_games` | `seed` e estratégia respeitados; perfil composto com pesos que somam 1; saída explicável. |

Cenário **L6 (recomendado, não substitui o mínimo de cinco acima):** [prompt-catalog.md](prompt-catalog.md) §3 item 10 (Spearman entre `pares_no_concurso` e `entropia_linha_por_concurso` na mesma janela, leitura não causal) com `analyze_indicator_associations` — alinhado a [ADR 0006 D5](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) e [test-plan.md](test-plan.md) secção *Cenário canónico*. A esteira **pode** adicionar L6 quando a implementação e os goldens cobrirem o caso; a falha de L6 não define regressão de “suíte mínima” até a equipa promover o cenário a bloqueador.

**Prompts negativos (catálogo §12):** não fazem parte desta **suíte mínima de cinco** positivos. O [test-plan.md](test-plan.md) exige cobertura E2E separada para os prompts 37–41; a implementação pode incluir **um ou mais** testes live adicionais para negativos, mas o **custo** e a **estabilidade** devem ser avaliados — preferir testes determinísticos para negativos quando possível.

## 7. Critérios de aceite por execução

Para a esteira dedicada ser considerada **verde**:

1. Os **cinco** cenários **L1–L5** concluem sem exceção não tratada.
2. Para cada cenário, as **tool calls** observadas incluem **pelo menos** as tools listadas (ordem pode variar se o contrato permitir múltiplas chamadas).
3. Nenhuma resposta final viola a regra de **linguagem não preditiva** indevida (alinhado ao [test-plan.md](test-plan.md), seção E2E).
4. Erros retornados pelo servidor, se esperados, têm **`code`**, **`message`** e **`details`** conforme [mcp-tool-contract.md](mcp-tool-contract.md).

**Flakiness:** se um cenário falhar por instabilidade do modelo, o procedimento é: (a) fixar `OPENAI_MODEL` e limites de rodada; (b) repetir **manualmente**; (c) se persistir, tratar como **bug** de prompt de sistema ou de exposto de tools — não “piscar” o critério sem ADR ou atualização de contrato.

## 8. Implementação nos testes (.NET)

Convenção sugerida (ajustar ao framework quando `tests/LotofacilMcp.Tests/` existir):

- Marcar testes desta suíte com **categoria** `LiveOpenAI` (ex.: xUnit: `[Trait("Category", "LiveOpenAI")]`).
- Na CI dedicada, executar apenas esses testes, por exemplo:  
  `dotnet test --configuration Release --filter "Category=LiveOpenAI"`

Testes **sem** essa categoria **não** devem chamar a API OpenAI na esteira principal.

## 9. Workflow GitHub Actions dedicado

Arquivo: [`.github/workflows/live-openai-integration.yml`](../.github/workflows/live-openai-integration.yml).

| Propriedade | Valor |
|-------------|--------|
| **Nome sugerido** | “Live OpenAI integration” |
| **Gatilho padrão** | Apenas `workflow_dispatch` (execução manual na aba Actions). |
| **Secrets** | `OPENAI_API_KEY` obrigatório para sucesso real. |
| **Isolamento** | Não é dependência do workflow de PR; não bloqueia merge por padrão. |

**Quando o job é ignorado:** se `LotofacilMcp.sln` ainda não existir na raiz, o job de testes é **pulado** propositalmente até a solução estar no repositório (o workflow permanece como contrato da esteira).

## 10. Relação com os demais documentos

| Documento | Papel |
|-----------|--------|
| [test-plan.md](test-plan.md) | Matriz geral de cobertura; esta suíte **reforça** integração real sem dispensar as demais camadas. |
| [prompt-catalog.md](prompt-catalog.md) | Fonte dos enunciados e tools esperadas. |
| [mcp-tool-contract.md](mcp-tool-contract.md) | Autoridade para parâmetros, erros e semântica. |
| [contract-test-plan.md](contract-test-plan.md) | Fixtures e ordem de testes de contrato; dados usados pelos testes live devem ser **compatíveis** com as fixtures acordadas. |

---

## Checklist rápido (operacional)

- [ ] Secret `OPENAI_API_KEY` configurado no GitHub (ambiente ou repositório).
- [ ] Variável `OPENAI_MODEL` fixada para previsibilidade.
- [ ] Workflow `live-openai-integration.yml` disparado manualmente após mudanças em tools ou prompts.
- [ ] Cinco cenários **L1–L5** implementados e marcados com categoria `LiveOpenAI`.
- [ ] Esteira principal de PR **sem** chamadas à API OpenAI.
