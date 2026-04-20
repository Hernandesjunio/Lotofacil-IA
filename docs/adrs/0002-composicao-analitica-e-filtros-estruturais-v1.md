# ADR 0002 — Composição analítica e filtros estruturais na V1

- **Status:** Aceito
- **Data:** 2026-04-20
- **Escopo:** `docs/brief.md`, `docs/metric-catalog.md`, `docs/mcp-tool-contract.md`, `docs/generation-strategies.md`

## Contexto

Após o fechamento semântico da V1 em `ADR 0001`, surgiram novos requisitos do domínio:

- combinar dinamicamente vários indicadores;
- cruzar frequência, ausência, blocos e slot;
- responder perguntas sobre persistência histórica sem sair do escopo não preditivo;
- analisar padrões por linha, coluna, pares/ímpares e vizinhos em séries históricas;
- correlacionar indicadores estruturais;
- eliminar, na geração, padrões muito raros ou pouco úteis.

O contrato anterior cobria bem métricas por janela, estabilidade e quatro estratégias fixas de geração, mas ainda não oferecia:

1. uma ferramenta canônica de composição dinâmica;
2. uma ferramenta explícita para associações entre indicadores;
3. uma ferramenta de resumo de padrões históricos;
4. séries estruturais por concurso para linha, coluna, vizinhos e pares;
5. filtros estruturais declarativos na geração.

## Decisões

### D1 — Séries estruturais por concurso entram na V1

**Decisão:** entram na V1 as métricas `pares_no_concurso`, `quantidade_vizinhos_por_concurso` e `sequencia_maxima_vizinhos_por_concurso`.

**Justificativa:** essas séries respondem diretamente perguntas de variação histórica sem exigir `scope = draw`.

### D2 — Distribuições por linha e coluna ganham forma histórica explícita

**Decisão:** entram na V1 `distribuicao_linha_por_concurso` e `distribuicao_coluna_por_concurso`, com `shape = series_of_count_vector[5]`.

**Justificativa:** o domínio precisa comparar concentração central, deslocamento para extremidades e correlação linha × coluna.

### D3 — Entropia e HHI históricos entram como séries canônicas

**Decisão:** entram na V1 `entropia_linha_por_concurso`, `entropia_coluna_por_concurso`, `hhi_linha_por_concurso` e `hhi_coluna_por_concurso`.

**Justificativa:** permitem resumir forma espacial típica, detectar dispersão/concentração e criar filtros estruturais de geração.

### D4 — Nova tool `compose_indicator_analysis`

**Decisão:** a V1 passa a aceitar composição dinâmica de indicadores via payload estruturado, com operador, target, componentes, transformações e pesos explícitos.

**Justificativa:** isso atende o requisito de combinar frequência, ausência, slot e demais indicadores sem criar uma tool por pergunta.

### D5 — Nova tool `analyze_indicator_associations`

**Decisão:** a V1 ganha uma tool específica para correlação entre séries compatíveis.

**Justificativa:** correlação e estabilidade da correlação não cabem semanticamente dentro de `analyze_indicator_stability`.

### D6 — Nova tool `summarize_window_patterns`

**Decisão:** a V1 ganha uma tool para responder moda, faixa típica, cobertura percentual e eventos raros.

**Justificativa:** perguntas do tipo "80% dos sorteios tiveram qual característica?" exigem um resumo de padrões, não apenas uma métrica isolada.

### D7 — Estratégia `declared_composite_profile` entra na V1

**Decisão:** a geração passa a aceitar uma estratégia composta declarativa com componentes canônicos de score e pesos explícitos.

**Justificativa:** atende o requisito de combinação dinâmica também na geração, mantendo determinismo e fechando o espaço sem permitir pesos implícitos por prompt.

### D8 — Filtros estruturais viram parte formal do contrato

**Decisão:** entram no contrato os filtros `max_consecutive_run`, `max_neighbor_count`, `min_row_entropy_norm`, `min_column_entropy_norm`, `max_hhi_linha`, `max_hhi_coluna`, `repeat_range`, `min_slot_alignment` e `max_outlier_score`.

**Justificativa:** o objetivo final do domínio inclui eliminar padrões muito raros ou pouco úteis, como sequências excessivas ou concentração espacial extrema.

### D9 — Prompt catalog e test plan passam a ser artefatos obrigatórios

**Decisão:** o repositório passa a ter `docs/prompt-catalog.md` e `docs/test-plan.md` como parte da definição funcional da V1.

**Justificativa:** o MCP será consumido por IA; logo, cobertura por prompts e cobertura de cálculo precisam ser explícitas e auditáveis.

## Consequências

### Positivas

- O MCP passa a cobrir as perguntas centrais do domínio sem linguagem ambígua.
- A geração fica mais útil para filtragem estrutural.
- O projeto ganha rastreabilidade entre requisito, prompt, contrato e plano de teste.

### Custos

- O contrato fica mais extenso.
- Ferramentas de composição e associação exigem mais validação de schema.
- A matriz de testes cresce significativamente.

## Rastreabilidade

| ID | Documento afetado | Seções tocadas |
|----|-------------------|----------------|
| D1 | `metric-catalog.md` | métricas de séries estruturais |
| D2 | `metric-catalog.md`, `mcp-tool-contract.md` | linha/coluna históricas |
| D3 | `metric-catalog.md` | entropia e HHI históricos |
| D4 | `mcp-tool-contract.md` | `compose_indicator_analysis` |
| D5 | `mcp-tool-contract.md` | `analyze_indicator_associations` |
| D6 | `mcp-tool-contract.md` | `summarize_window_patterns` |
| D7 | `generation-strategies.md`, `mcp-tool-contract.md` | `declared_composite_profile` |
| D8 | `generation-strategies.md`, `mcp-tool-contract.md` | `structural_exclusions` |
| D9 | `brief.md`, `prompt-catalog.md`, `test-plan.md` | referências e cobertura |
