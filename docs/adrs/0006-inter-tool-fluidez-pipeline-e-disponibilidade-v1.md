# ADR 0006 — Inter-tool fluidez, pipeline de pedidos, disponibilidade de métricas e performance documentada (V1)

**Navegação:** [← Brief (índice)](../brief.md) · [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md) · [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md) · [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md)

## Status

Aceito — estende a forma como consumidores e testes leem o contrato sem reabrir o núcleo semântico de [0001](0001-fechamento-semantico-e-determinismo-v1.md) nem a superfície de [0005](0005-transporte-mcp-e-superficie-tools-v1.md).

**Data:** 2026-04-22

## Contexto

Validações e uso real com hosts MCP (ex.: Cursor) mostraram o seguinte:

1. **O catálogo** ([metric-catalog.md](../metric-catalog.md)) define métricas canónicas, mas **nem toda rota** precisa, na mesma build, oferecer o mesmo recorte. O mínimo da fatia [vertical V0](../vertical-slice.md) pedia `compute_window_metrics` com uma métrica; o catálogo e as estratégias referem muitas outras. Isto cria a **expectativa de consumidor** de “pedir tudo o que o glossário/estratégia cita” numa única rota, quando a política da build pode ainda restringir a allowlist.
2. **`analyze_indicator_associations`** consegue devolver magnitude (Spearman/Pearson) global na janela, mas a **estabilidade em subjanelas** (`stability_check`) exige carga extra e, em alguns recortes, o campo fica vazio. Sem decisão, `null` torna-se ambíguo (dado faltando vs. não suportado).
3. A **repetição e geração** (`generation-strategies.md`, `explain_candidate_games`) reutilizam métricas (ex. `repeticao_concurso_anterior`); a **fluidez** (menos *round-trips*, menos divergência entre o que a geração *usa* e o que `compute_window_metrics` *expõe*) exige padrão documentado de **promoção** e de **fluxo mínimo** (janela → métricas/associação → geração/explicar) sem defaults ocultos, respeitando o [brief.md](../brief.md) e o [mcp-tool-contract.md](../mcp-tool-contract.md).
4. O **desempenho** não adiciona requisito de latência de rede neste ADR, mas proíbe **caches implícitos** incompatíveis com o [project-guide.md](../project-guide.md) e alinha a documentação a **lotes de métricas** em um único request, quando a tool já o suporta, para reduzir *round-trips*.

## Decisões

### D1 — Matriz normativa: catálogo vs. `compute_window_metrics` por fase

**Decisão:** o [metric-catalog.md](../metric-catalog.md) permanece a fonte de **nomes, versões, formas e fórmulas**. A seção *Disponibilidade normativa (catálogo × `compute_window_metrics`)* no catálogo indica, para o **recorte mínimo** documentado em [vertical-slice.md](../vertical-slice.md) e o **alvo** V1, quais entradas uma build que ainda cumpre apenas a V0 mínima pode recusar com `UNKNOWN_METRIC` apesar de o nome constar do catálogo, desde que a resposta de erro inclua `details.metric_name` (e, quando o servidor tiver, `details.allowed_metrics` em conjunto fechado) conforme o contrato.

**Justificativa:** evita contradizer `UNKNOWN_METRIC` “métrica desconhecida” no cliente com “é conhecida no doc mas não no servidor” — a ambiguidade fica resolvida por pistas de `details` e pela matriz de disponibilidade.

**Arquivos:** [metric-catalog.md](../metric-catalog.md), [mcp-tool-contract.md](../mcp-tool-contract.md) (secção de `compute_window_metrics`).

### D2 — Erro e semântica para `stability_check` não implementado

**Decisão:** se o request inclui `stability_check` e a build **não** implementa a instabilidade/estatística de subjanela, a tool deve responder com o código de erro `UNSUPPORTED_STABILITY_CHECK` (e não com sucesso e `null` nesse caso). Quando a implementação **omite** a camada de estabilidade de forma conhecida, mas ainda responde sucesso sem `stability_check` no request, os campos de estabilidade podem ser omitidos; se a forma JSON fixar `null` com semântica **“não computado com sucesso nesta resposta”**, a documentação do contrato liga o `null` a ausência de input `stability_check` ou a limitação documentada da build.

**Justificativa:** distingue *cliente pediu subjanela e o servidor ainda não entregou* de *cliente não pediu estabilidade*.

**Arquivos:** [mcp-tool-contract.md](../mcp-tool-contract.md) (tabela de erros; `analyze_indicator_associations`).

### D3 — Pipeline mínimo recomendado (fluidez, sem lógica escondida)

**Decisão:** o padrão descritivo para análise “forma do volante” (pares, entropia, vizinhos, repetição) e relação com candidatos de jogo, sem *defaults* invisíveis, é a sequência **reproduzível e declarada** abaixo. Cada seta implica o mesmo `window_size` e `end_contest_id` a menos que o `dataset_version` exija reprocesso:

1. `get_draw_window` (ou implícito nas seguintes) →  
2. `compute_window_metrics` (com lista explícita de `metrics`) *ou* tools analíticas subsequentes, conforme a pergunta;  
3. `analyze_indicator_stability` / `analyze_indicator_associations` / `summarize_window_patterns` *conforme necessidade*;  
4. `generate_candidate_games` (com `seed` e `plan` explícitos onde o contrato exigir) → `explain_candidate_games` para o mesmo *window* e jogos.  

**Justificativa:** alinha a documentação ao objetivo de **correlação e interação** entre métricas (ex.: pares e entropia) sem misturar semânticas; reduz cadeias *ad hoc* de prompts. O [prompt-catalog.md](../prompt-catalog.md) reforça exemplos.

**Não** é obrigatório o servidor oferecer “uma tool única” que unifique 2–4, salvo outra entrega; o padrão é informativo e testável (multi-tool).

**Arquivos:** [mcp-tool-contract.md](../mcp-tool-contract.md) (secção *Pipeline mínimo recomendado*), [prompt-catalog.md](../prompt-catalog.md), [test-plan.md](../test-plan.md), [contract-test-plan.md](../contract-test-plan.md).

### D4 — Validação `min_history` em `analyze_indicator_stability`

**Decisão:** o contrato reforça que, se `min_history` for **maior** que o tamanho efetivo da janela (quantidade de concursos disponíveis resolvida), a tool deve recusar com `INSUFFICIENT_HISTORY` (ou, se a implementação ainda devolver *ranking* parcial, isso fica *proibido* — preferir erro claro) e `details` com `min_history` e o tamanho observado, conforme já alinhado em [mcp-tool-contract.md](../mcp-tool-contract.md) e reforçado no plano de testes.

**Justificativa:** a documentação padrão usa exemplos com `min_history: 20`; janela curta com default não ajustado foi fonte de surpresa em testes reais.

**Arquivos:** [mcp-tool-contract.md](../mcp-tool-contract.md), [test-plan.md](../test-plan.md).

### D5 — Cenário canónico pares × entropia (associação, não causalidade)

**Decisão:** a pergunta *“em concursos com mais pares, a entropia de linha tende a subir ou descer?”* é mapeada para `analyze_indicator_associations` com a mesma janela, `items` = `[ { "name": "pares_no_concurso" }, { "name": "entropia_linha_por_concurso" } ]`, e `method: "spearman"`. A interpretação é **co-movimento descritivo** na janela; a causalidade fica fora, como em [0002](0002-composicao-analitica-e-filtros-estruturais-v1.md) e [mcp-tool-contract.md](../mcp-tool-contract.md) (glossário “correlação”).

**Justificativa:** cria bateria de teste replicável para “GAPS” e para exploração de *interação entre métricas*; fixa a escolha de pares/entropia *por concurso* (séries alinhadas) em vez de misturar com `entropia_linha` de jogo candidato.

**Arquivos:** [test-plan.md](../test-plan.md), [contract-test-plan.md](../contract-test-plan.md), [prompt-catalog.md](../prompt-catalog.md).

### D6 — Geração e alinhamento ao subconjunto implementado

**Decisão:** enquanto `generate_candidate_games` e `explain_candidate_games` forem alimentados por [generation-strategies.md](../generation-strategies.md) com *subconjunto* de estratégias ativas na build, a versão e o inventário de estratégias efetivas devem rastrearse via resposta (metadados / `tool_version` / tabela de sumário) e, quando fizer diferença para o consumidor, a documentação *deste ADR* reenvia a D1: não atribuir ao `UNKNOWN_METRIC` de outra rota o mesmo significado que a `UNKNOWN_STRATEGY` na geração.

**Arquivos:** [generation-strategies.md](../generation-strategies.md) (nota de alinhamento), [mcp-tool-contract.md](../mcp-tool-contract.md).

## Consequências

- `contract-test-plan.md` e `test-plan.md` ganham cenários de **GAPS** (erro congelado, coerência explain/compute) e o de **pares–entropia** (D5).
- Novos códigos de erro documentados: `UNSUPPORTED_STABILITY_CHECK` (D2) — a implementação deve acompanhar na mesma PR ou tarefa que fixar a semântica.
- Evoluções que **promovam** métricas a `compute_window_metrics` devem atualizar a matriz em [metric-catalog.md](../metric-catalog.md) e, se a política de erro mudar, os goldens.
- Não altera os limites de orçamento de [0001 D11](0001-fechamento-semantico-e-determinismo-v1.md) nem a estrutura de processos de [0004](0004-estrutura-arquitetural-inicial-mcp-dotnet10.md).
- Os pedidos atômicos de implementação correspondentes estão em [fases-execucao-templates.md](../fases-execucao-templates.md) (Fase 21).

## Critérios de verificação

1. O contrato MCP cita o pipeline mínimo e o erro `UNSUPPORTED_STABILITY_CHECK` com semântica única.  
2. O catálogo exibe a matriz (ou tabela) D1.  
3. O plano de testes e o contract-test-plan incluem os GAPS e o D5.  
4. Nenhum texto promete *latência* fixa; fluidez é *menos idas* com **parametrização explícita** e tabelas de disponibilidade.

## Referências internas

- [brief.md](../brief.md)  
- [mcp-tool-contract.md](../mcp-tool-contract.md)  
- [metric-catalog.md](../metric-catalog.md)  
- [vertical-slice.md](../vertical-slice.md)  
- [generation-strategies.md](../generation-strategies.md)  
- [test-plan.md](../test-plan.md)  
- [contract-test-plan.md](../contract-test-plan.md)  
- [spec-driven-execution-guide.md](../spec-driven-execution-guide.md)  
- [fases-execucao-templates.md](../fases-execucao-templates.md) (Fase 21 — templates 21.1 a 21.5)  
- [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md)  
