# ADR 0019 — Critérios por faixa (range) e por cobertura na geração v1

**Navegação:** [← Brief (índice)](../brief.md) · [Contrato MCP](../mcp-tool-contract.md) · [Estratégias de geração](../generation-strategies.md) · [ADR 0017](0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md) · [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

A geração atual (`generate_candidate_games`) aceita critérios e filtros que, na prática, são **limiares absolutos** (ex.: `min_*`, `max_*`), o que pode:

- reduzir o espaço de busca a ponto de tornar planos inviáveis (`STRUCTURAL_EXCLUSION_CONFLICT`);
- induzir “engessamento” (ex.: fixar exatamente um valor de repetição), quando o objetivo correto é aderir a uma **faixa típica** que cobre uma fração relevante do histórico;
- dificultar a composição de múltiplos guardrails sem colapsar a diversidade.

No sistema legado, vários parâmetros eram modelados como **range**, e alguns eram derivados do próprio histórico como “faixa que atinge ao menos X% de cobertura” (ex.: últimos 20 concursos → encontrar intervalo de repetição que cobre ≥ 80%).

O projeto (ADR 0001 e ADR 0017) exige:

- determinismo (mesmo input ⇒ mesmo output),
- sem defaults semânticos ocultos,
- rastreabilidade (eco do que foi aplicado).

## Decisão

Introduzir, na geração v1, **critérios e filtros parametrizáveis por faixa** e, quando aplicável, por **faixa derivada do histórico com cobertura-alvo**.

Em termos de contrato, isso significa permitir que itens do `plan[]` e/ou `structural_exclusions` aceitem:

1) **Modo absoluto** (como hoje): `value`, `min`, `max`.
2) **Modo faixa explícita**: `range: { min, max, inclusive? }`.
3) **Modo faixa por cobertura** (histórico): `typical_range: { metric_name, method, coverage, window_ref?, inclusive? }` que resolve deterministamente um intervalo a partir da janela (ex.: IQR, percentis).
4) **Modo discreto (multi-valor)**: `allowed_values: { values: [v1, v2, ...] }` para casos em que o consumidor quer expressar um conjunto pequeno de valores aceitáveis (ex.: vizinhos ∈ {8,9,10}).

**Semântica central:** declarar um `range` (ou `allowed_values`) **não** significa “gerar um jogo para cada valor”. Significa declarar um **conjunto válido**; o `count` do plano define quantos candidatos o servidor deve retornar que satisfaçam as restrições.

O servidor deve sempre retornar, por candidato:

- o **request original** (eco), e
- a **configuração efetivamente resolvida** (incluindo a faixa computada e quaisquer defaults), em `applied_configuration.resolved_defaults` (como já definido na ADR 0017).

## Definições

- **Critério**: condição de elegibilidade (passa/falha) baseada em um valor do perfil do jogo ou em métricas derivadas.
- **Filtro estrutural**: guardrail “duro” aplicado antes ou durante seleção (ex.: `max_neighbor_count`), com impacto direto no espaço de busca.
- **Faixa típica**: intervalo calculado deterministicamente na janela, a partir de um método fechado (ex.: IQR ⇒ \([Q1,Q3]\); percentis ⇒ \([P10,P90]\)).
- **Cobertura**: fração de observações na janela que caem dentro do intervalo.

## Especificação (v1)

### 1) Estruturas de faixa

#### `RangeSpec`

```json
{ "min": <number>, "max": <number>, "inclusive": true }
```

Regras:
- `min <= max`, caso contrário `INVALID_REQUEST`.
- `inclusive` default `true` (se ausente, o servidor deve registrá-lo em `resolved_defaults`).

#### `AllowedValuesSpec`

```json
{ "values": [<number>, <number>, ...] }
```

Regras:
- `values` deve ser não vazio.
- Duplicatas em `values` são normalizadas (ordenar e deduplicar) e o resultado deve aparecer em `resolved_defaults`.
- Quando o indicador for naturalmente inteiro (ex.: pares, vizinhos, repetição), o servidor deve validar que os valores são inteiros; caso contrário `INVALID_REQUEST`.

#### `TypicalRangeSpec`

```json
{
  "metric_name": "<nome_canônico>",
  "method": "iqr" | "percentile",
  "coverage": 0.8,
  "params": { ... }
}
```

Regras:
- `coverage` em \([0,1]\).
- `method="iqr"` não requer params; `method="percentile"` requer `p_low` e `p_high` (ex.: 0.10 e 0.90).
- A resolução usa **a mesma janela** do request, exceto se a estratégia declarar explicitamente `window_ref` (neste caso, deve ser declarado no request e refletido no hash determinístico).

**Nota crítica (para evitar ambiguidade):** `typical_range` deve resolver um intervalo por um método **fechado**. “Faixa que atinge X%” precisa de definição operacional. Nesta v1, as opções permitidas são:
- `iqr`: intervalo \([Q1,Q3]\); `coverage_observed` é reportado (não é garantido atingir `coverage`).
- `percentile`: intervalo \([P_low,P_high]\) com percentis declarados em `params`; `coverage_observed` é reportado.

Output da resolução (sempre ecoado):

```json
{
  "resolved_range": { "min": <number>, "max": <number>, "inclusive": true },
  "coverage_observed": <number>,
  "method_version": "1.0.0"
}
```

### 2) Onde ranges podem ser usados

#### 2.1 `plan[].criteria[]`

Hoje: `{ "name": string, "value": number }`.

Evolução proposta (retrocompatível):

- **ou** `{ "name": string, "value": number }` (modo atual)
- **ou** `{ "name": string, "range": RangeSpec }`
- **ou** `{ "name": string, "typical_range": TypicalRangeSpec }`
- **ou** `{ "name": string, "allowed_values": AllowedValuesSpec }`

O servidor rejeita payloads que misturem modos no mesmo item (ex.: `value` e `range`) com `INVALID_REQUEST`.

#### 2.1.1 `mode`: hard vs soft (para evitar colapso por composição)

Para suportar combinação de muitos indicadores sem inviabilizar o plano, critérios/filtros podem declarar `mode`:

- `mode = "hard"`: fora da faixa/conjunto ⇒ candidato rejeitado.
- `mode = "soft"`: fora da faixa/conjunto ⇒ candidato não é rejeitado, mas sofre penalidade determinística no score (a estratégia deve declarar como a penalidade é aplicada, ou usar uma penalidade canônica versionada).

Se `mode` for omitido, o default é `hard`, e isso deve ser registrado em `resolved_defaults`.

#### 2.2 `plan[].filters[]` e `structural_exclusions`

Os filtros canônicos listados em `generation-strategies.md` que são naturalmente “faixa” devem aceitar `range` quando fizer sentido (ex.: `repeat_range` já é um range; estender para `neighbor_count_range`, `pairs_range`, etc., conforme disponibilidade de métricas do perfil).

Para filtros já modelados como `min_*`/`max_*`, a evolução é permitir:

- manter `min`/`max` (modo atual),
- ou aceitar `range` (semântica equivalente),
- e quando aplicável, aceitar `typical_range`.
- Para filtros discretos, aceitar `allowed_values` quando fizer sentido (ex.: pares ∈ {7,8,9}).

### 3) Métricas candidatas a suportar range

Esta ADR define a intenção “sempre que fizer sentido e houver métrica no perfil”.

No recorte V1 imediato (prioridade):

- **Repetição**: `repeat_range` (já existe), estender para também aceitar `typical_range` baseado em `repeticao_concurso_anterior` na janela.
- **Vizinhos**: range em `neighbor_count` e/ou `max_consecutive_run`.
- **Pares**: range em `pares(jogo)` usando histórico de `pares_no_concurso`.
- **Entropia/HHI**: ranges para `row_entropy_norm`, `column_entropy_norm`, `hhi_linha`, `hhi_coluna`.
- **Slot**: range em `slot_alignment` quando a estratégia usa `analise_slot`.
- **Top10 overlap (derivado)**: permitir range/set para `top10_overlap_count(game)` (quantidade de dezenas do jogo presentes no `top10_mais_sorteados` da janela).  
  **Nota:** `top10_mais_sorteados` é lista; o range aplica-se a um **escalar derivado** (contagem ou razão), para evitar ambiguidade.

### 3.1 Indicadores derivados (para evitar ambiguidades de shape)

Alguns indicadores canônicos são listas/vetores; a flexibilidade na geração deve incidir sobre **features derivadas escalares** do jogo. Exemplos:

- `top10_overlap_count(game)` e `top10_overlap_ratio(game)` a partir de `top10_mais_sorteados`.
- `mean_frequency_alignment(game)` (já existe como `freq_alignment`) a partir de `frequencia_por_dezena`.

Essas features derivadas devem ter nomes canônicos estáveis no contexto de geração/explicação (estratégias), e devem aparecer em `metric_breakdown` e/ou `resolved_defaults` quando usadas como restrição.

### 4) Determinismo e hash

O `deterministic_hash` deve incluir:

- o payload original (`range`/`typical_range`),
- e, quando `typical_range` for usado, os parâmetros e método, além de qualquer `window_ref`.

O servidor deve registrar em `applied_configuration.resolved_defaults`:

- os defaults de `inclusive`,
- normalização de `allowed_values`,
- default de `mode`,
- o `resolved_range` calculado,
- `coverage_observed`,
- e versão do método de resolução.

### 4.1 Orçamento (budget) e escalabilidade de `count`

Para tornar eficiente a geração com múltiplas constraints (ranges/sets) e permitir `count` alto sem enumerar combinações no cliente, a geração deve expor um orçamento determinístico, por exemplo:

- `generation_budget.max_attempts` (quantidade máxima de candidatos propostos/avaliados)
- e/ou `generation_budget.pool_multiplier` (escala o pool proporcionalmente a `count`)

Esses parâmetros (quando presentes) fazem parte do determinismo e devem aparecer em `applied_configuration.resolved_defaults` junto de:

- `attempts_used`
- `accepted_count`
- `rejected_count_by_reason` (agregado determinístico)

### 5) Erros

- `INVALID_REQUEST`: range inválido, modos mistos, `coverage` fora do intervalo, percentis inválidos.
- `UNKNOWN_METRIC`: `typical_range.metric_name` desconhecida.
- `INCOMPATIBLE_COMPOSITION` (ou equivalente): `typical_range` aponta para métrica sem shape/escopo compatível com o cálculo da faixa.
- `STRUCTURAL_EXCLUSION_CONFLICT`: continua quando o plano é inviável; deve incluir `available_count` e, quando aplicável, indicar qual faixa/filtro colapsou o espaço.

## Consequências

### Positivas

- Reduz “engessamento” de critérios absolutos.
- Melhora viabilidade de planos com múltiplos guardrails.
- Facilita reproduzir comportamento do legado de forma auditável.

### Trade-offs / Custos

- Aumenta complexidade de validação e de eco de configuração.
- Requer rastreabilidade rigorosa da resolução de faixas (para não virar default oculto).
- Pode exigir ajustes no motor de busca (poolSize / orçamento) para aproveitar o espaço ampliado.

## Plano de implementação (sugestão)

1) Atualizar contrato de request/validator para aceitar `range`/`typical_range` (mantendo modo atual).
2) Implementar resolvedor determinístico de `typical_range` (IQR e percentis) operando sobre métricas de janela/series já existentes.
3) Atualizar estratégias `common_repetition_frequency` e `declared_composite_profile` para aceitar ranges nos componentes/guardrails mais críticos (repetição, vizinhos, pares).
4) Atualizar `explain_candidate_games` para ecoar a resolução e indicar pass/fail contra faixa.
5) Adicionar testes:
   - determinismo: mesma janela+seed+spec ⇒ mesmo `deterministic_hash`;
   - retrocompatibilidade: requests antigos continuam válidos;
   - range semantics: `range` vs (`min`,`max`) equivalente quando aplicável;
   - typical_range: resolução consistente e ecoada em `resolved_defaults`.

## Referências internas

- [ADR 0017](0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md)
- [generation-strategies.md](../generation-strategies.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
- [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md)

