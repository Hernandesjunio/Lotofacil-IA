# Contrato inicial de ferramentas MCP

## Objetivo

Definir um contrato inicial, explicavel e testavel para um MCP que permita:

- consultar concursos e janelas
- calcular indicadores canonicos
- analisar estabilidade de indicadores
- gerar jogos candidatos por estrategias declaradas
- explicar os resultados de forma reproduzivel

Este contrato nao assume capacidade preditiva. O foco e analise descritiva, geracao heuristica e explicabilidade.

## Decisao de escopo para V1

A V1 deve operar sobre um historico canonico de concursos da Lotofacil e expor poucas ferramentas de alto valor. O objetivo nao e "ter muitas tools", e sim garantir que cada tool tenha semantica fechada e payload estavel.

### Fora da V1

- chat livre dentro do servidor
- recomendacao comercial de apostas
- mutacao manual de pesos por prompt sem estrategia nomeada
- escrita concorrente em multiplas fontes sem regra de reconciliacao
- inferencia preditiva nao validada

## Modelo conceitual

### Entidades canonicas

#### `Draw`

Representa um concurso fechado.

Campos minimos:

- `contest_id`: identificador numerico do concurso
- `draw_date`: data do concurso
- `numbers`: array ordenado crescente com 15 dezenas validas entre 1 e 25
- `source`: origem do dado
- `ingested_at`: timestamp de ingestao

#### `Window`

Representa um recorte temporal baseado em concursos.

Campos minimos:

- `size`: quantidade de concursos
- `end_contest_id`: concurso final incluido
- `start_contest_id`: concurso inicial incluido
- `draws`: colecao ordenada por concurso

#### `MetricValue`

Representa o valor de uma metrica em um contexto.

Campos minimos:

- `metric_name`
- `scope`: `window`, `candidate_game` ou `series` (ADR 0001 D13 — `draw` reservado para evolucao futura)
- `shape`: `scalar` | `series` | `vector_by_dezena` | `count_vector[5]` | `count_matrix[25x15]` | `count_pair` | `dezena_list[10]` (ADR 0001 D10). O campo `value` e tipado conforme `shape`.
- `window`
- `value`
- `unit` — conforme coluna `Unidade` em `docs/metric-catalog.md`
- `explanation`
- `version` — SemVer da metrica, vem de `docs/metric-catalog.md`

#### `CandidateGame`

Representa um jogo gerado pelo motor.

Campos minimos:

- `numbers`: array ordenado crescente com 15 dezenas validas entre 1 e 25
- `strategy_name`
- `strategy_version`
- `seed_used`: `uint64` — propagada do input; permite reproduzir a geracao (ADR 0001 D1)
- `search_method`: `exhaustive | sampled | greedy_topk` (ADR 0001 D3)
- `n_samples_used`: inteiro (presente apenas quando `search_method = "sampled"`)
- `scores`: objeto com scores por dimensao da estrategia e `strategy_score` agregado
- `constraints_applied`
- `tie_break_rule`: string declarativa usada no desempate
- `rationale`

## Invariantes globais

1. Mesmo input canonico (incluindo `seed` quando aplicavel) + mesmo `dataset_version` deve produzir o mesmo output. Verificavel via `deterministic_hash` (ver abaixo).
2. Toda resposta analitica deve declarar a janela usada.
3. Toda resposta de geracao deve declarar `strategy_name`, `strategy_version`, `seed_used`, `search_method`, filtros aplicados, pesos efetivos e `tie_break_rule`.
4. Nenhuma tool deve depender de contexto oculto de conversa para executar corretamente.
5. Termos como `slot`, `outlier` e `estabilidade` devem ter definicao explicita no payload ou na documentacao da estrategia (`docs/generation-strategies.md`) ou do catalogo (`docs/metric-catalog.md`).
6. Ferramentas nao devem retornar conclusoes como "mais provavel de sair"; devem retornar "mais estavel", "mais aderente a estrategia" ou "mais distante do comportamento tipico".
7. `deterministic_hash = SHA256(canonical_json({input, dataset_version, tool_version}))`, com `canonical_json` seguindo RFC 8785 (JCS). E devolvido em toda resposta e permite reproducao cross-implementacao (ADR 0001 D1).
8. `dataset_version = "cef-" + YYYY-MM-DD + "-sha" + 8_hex_de_SHA256_do_arquivo_fonte` (ADR 0001 D2).

## Ferramentas propostas

### 1. `get_draw_window`

#### Finalidade

Retornar um recorte canonico de concursos para servir de base a calculos posteriores.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "include_metadata": true
}
```

#### Regras

- `window_size` deve ser inteiro positivo.
- Se `end_contest_id` for omitido, usar o concurso mais recente disponivel.
- O servidor deve retornar os concursos em ordem crescente.

#### Output

```json
{
  "window": {
    "size": 20,
    "start_contest_id": 3381,
    "end_contest_id": 3400
  },
  "draws": [
    {
      "contest_id": 3381,
      "draw_date": "2026-03-01",
      "numbers": [1,2,3,5,7,8,9,11,13,15,17,18,20,22,25],
      "source": "cef-file-v1"
    }
  ],
  "metadata": {
    "draw_count": 20,
    "dataset_version": "cef-2026-04-20-sha7c3a91b2",
    "tool_version": "1.0.0",
    "deterministic_hash": "f1c2…"
  }
}
```

#### Viabilidade

Alta. E a tool mais simples e a base para qualquer outra.

---

### 2. `compute_window_metrics`

#### Finalidade

Calcular metricas canonicas para uma janela de concursos.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "metrics": [
    "frequencia_por_dezena",
    "top10_mais_sorteados",
    "repeticao_concurso_anterior",
    "atraso_por_dezena",
    "matriz_numero_slot"
  ]
}
```

#### Regras

- `metrics` e **obrigatorio** (ADR 0001 D14). Omissao retorna `INVALID_REQUEST` com `missing_field: "metrics"`. Nao existe conjunto default implicito.
- Metrica desconhecida retorna `UNKNOWN_METRIC`.
- Metricas com status `pendente de detalhamento` no catalogo exigem flag explicita `allow_pending: true`; sem a flag, retorna `UNKNOWN_METRIC` para forcar decisao consciente.

#### Output

```json
{
  "window": {
    "size": 20,
    "start_contest_id": 3381,
    "end_contest_id": 3400
  },
  "metrics": [
    {
      "metric_name": "repeticao_concurso_anterior",
      "scope": "series",
      "shape": "series",
      "unit": "count",
      "value": [9, 8, 10, 9, 11],
      "summary": {
        "mean": 9.1,
        "std_dev": 1.2
      },
      "version": "1.0.0"
    },
    {
      "metric_name": "frequencia_por_dezena",
      "scope": "window",
      "shape": "count_vector[25]",
      "unit": "count",
      "value": [12, 10, 11, 9, 13, 8, 11, 10, 12, 9, 10, 11, 12, 9, 10, 11, 10, 12, 9, 11, 10, 11, 9, 12, 10],
      "version": "1.0.0"
    },
    {
      "metric_name": "matriz_numero_slot",
      "scope": "window",
      "shape": "count_matrix[25x15]",
      "unit": "count",
      "value": [[0,1,0,0,0,1,0,0,0,0,0,0,0,0,0], "..."],
      "version": "1.0.0"
    }
  ],
  "metadata": {
    "dataset_version": "cef-2026-04-20-sha7c3a91b2",
    "tool_version": "1.0.0",
    "deterministic_hash": "…"
  }
}
```

#### Observacoes

- A resposta declara `scope` e `shape` explicitos por metrica (ADR 0001 D10). Consumidor nao precisa inferir shape do `metric_name`.
- Metricas de unidade e versao vem do catalogo (`docs/metric-catalog.md`).
- Metricas pendentes de detalhamento nao entram por padrao; exigem `allow_pending: true`.

#### Viabilidade

Alta para metricas canonicas. Apos o fechamento do ADR 0001 (`analise_slot`, `surpresa_slot`, `outlier_score`, `divergencia_kl` com smoothing), o conjunto canonico V1 esta completo.

---

### 3. `analyze_indicator_stability`

#### Finalidade

Comparar indicadores em uma janela e identificar quais apresentaram menor volatilidade relativa.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "indicators": [
    { "name": "repeticao_concurso_anterior" },
    { "name": "frequencia_por_dezena", "aggregation": "mean" },
    { "name": "atraso_por_dezena", "aggregation": "max" }
  ],
  "normalization_method": "madn",
  "top_k": 5,
  "min_history": 20
}
```

#### Regras

- `indicators` e uma lista de objetos `{ name, aggregation? }` (ADR 0001 D10). Para indicadores com `shape` vetorial (`count_vector[25]`, `count_matrix[25x15]`), `aggregation` e **obrigatorio** e deve ser um de `"mean" | "max" | "l2_norm" | "per_component"`. Ausencia retorna `UNSUPPORTED_AGGREGATION` antes de calcular.
- `normalization_method` default: `madn` (ADR 0001 D4). Valores aceitos: `"madn" | "coefficient_of_variation" | "iqr_over_median"`.
- `coefficient_of_variation` e aceito apenas para series estritamente positivas. Em series com `min(x) <= 0` ou `median(x) <= ε_cv`, retorna `UNSUPPORTED_NORMALIZATION_METHOD` com razao; o consumidor deve escolher `madn` ou `iqr_over_median`.
- `top_k` limita o tamanho do ranking retornado (nao a quantidade de indicadores processados).
- `min_history ≥ window_size`; caso contrario, `INSUFFICIENT_HISTORY`.
- O ranking compara apenas series compativeis; se um indicador nao puder ser comparado de forma valida, e excluido com justificativa.

#### Output

```json
{
  "window": {
    "size": 20,
    "start_contest_id": 3381,
    "end_contest_id": 3400
  },
  "normalization_method": "madn",
  "stability_ranking": [
    {
      "indicator": "repeticao_concurso_anterior",
      "aggregation": null,
      "mean": 9.1,
      "median": 9.0,
      "std_dev": 1.2,
      "mad": 1.0,
      "madn": 0.111,
      "trend": 0.05,
      "stability_score": 0.84,
      "interpretation": "Baixa dispersao robusta relativa na janela; comportamento descritivo estavel (nao implica previsibilidade)."
    }
  ],
  "excluded_indicators": [],
  "metadata": {
    "dataset_version": "cef-2026-04-20-sha7c3a91b2",
    "tool_version": "1.0.0",
    "deterministic_hash": "…"
  }
}
```

#### Viabilidade

Alta apos ADR 0001:

- agregacao de vetoriais fechada no input (`aggregation` obrigatoria).
- `madn` como metodo canonico.
- `coefficient_of_variation` disponivel mas restrito a series positivas.

Semantica de "estabilidade descritiva" reforcada no campo `interpretation`, alinhada ao invariante 6.

---

### 4. `generate_candidate_games`

#### Finalidade

Gerar jogos candidatos a partir de estrategias nomeadas e restricoes declaradas.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "seed": 424242,
  "plan": [
    { "strategy_name": "common_repetition_frequency", "count": 3 },
    { "strategy_name": "slot_weighted", "count": 3 },
    { "strategy_name": "row_entropy_balance", "count": 3 },
    { "strategy_name": "outlier_candidate", "count": 1 }
  ],
  "global_constraints": {
    "unique_games": true,
    "sorted_numbers": true
  }
}
```

#### Regras

- `seed: uint64` e **obrigatoria** (ADR 0001 D1). Propagada para cada estrategia com `search_method != "exhaustive"`. Sem `seed`, retorna `INVALID_REQUEST`.
- `plan` deve explicitar quantidade por estrategia.
- `MAX_COUNT_PER_STRATEGY = 100` e `MAX_TOTAL_COUNT = 250` (ADR 0001 D11). Violacao: `PLAN_BUDGET_EXCEEDED`.
- Cada estrategia deve estar listada em `docs/generation-strategies.md` com versao canonica; estrategia desconhecida retorna `UNKNOWN_STRATEGY`.
- A tool retorna o score por estrategia, as restricoes aplicadas e a `tie_break_rule` em cada jogo.
- O servidor nao aceita "pesos soltos" sem estrategia nomeada na V1.

#### Output

```json
{
  "window": {
    "size": 20,
    "start_contest_id": 3381,
    "end_contest_id": 3400
  },
  "games": [
    {
      "numbers": [1,3,4,5,7,8,10,11,13,15,17,18,20,22,24],
      "strategy_name": "common_repetition_frequency",
      "strategy_version": "1.0.0",
      "seed_used": 424242,
      "search_method": "greedy_topk",
      "scores": {
        "strategy_score": 0.82,
        "freq_alignment": 0.79,
        "repeat_alignment": 0.88
      },
      "constraints_applied": {
        "unique_games": true,
        "sorted_numbers": true,
        "min_top10_overlap": 6,
        "repeat_target_range": [8, 10]
      },
      "tie_break_rule": "min(hhi_linha); then lex asc",
      "rationale": [
        "Aderencia a dezenas frequentes da janela (8 do top10).",
        "Repeticao prevista 9 no intervalo [Q1=8, Q3=10] da janela."
      ]
    }
  ],
  "summary": {
    "requested_count": 10,
    "generated_count": 10,
    "strategies": [
      { "name": "common_repetition_frequency", "version": "1.0.0", "search_method": "greedy_topk" },
      { "name": "slot_weighted", "version": "1.0.0", "search_method": "greedy_topk" },
      { "name": "row_entropy_balance", "version": "1.0.0", "search_method": "sampled", "n_samples_used": 2000 },
      { "name": "outlier_candidate", "version": "1.0.0", "search_method": "sampled", "n_samples_used": 5000 }
    ]
  },
  "metadata": {
    "dataset_version": "cef-2026-04-20-sha7c3a91b2",
    "tool_version": "1.0.0",
    "deterministic_hash": "…"
  }
}
```

#### Viabilidade

Alta apos ADR 0001:

1. Todas as 4 estrategias V1 tem score fechado (ver `docs/generation-strategies.md`).
2. `search_method` declarado por estrategia.
3. `seed` propagada garante determinismo.
4. Orcamento por plano previne DoS.

---

### 5. `explain_candidate_games`

#### Finalidade

Explicar por que os jogos foram gerados e quais metricas sustentam cada decisao.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "games": [
    [1,3,4,5,7,8,10,11,13,15,17,18,20,22,24]
  ],
  "include_metric_breakdown": true
}
```

#### Output

```json
{
  "explanations": [
    {
      "numbers": [1,3,4,5,7,8,10,11,13,15,17,18,20,22,24],
      "candidate_strategies": [
        { "strategy_name": "common_repetition_frequency", "strategy_version": "1.0.0", "score": 0.82 },
        { "strategy_name": "row_entropy_balance", "strategy_version": "1.0.0", "score": 0.71 },
        { "strategy_name": "slot_weighted", "strategy_version": "1.0.0", "score": 0.54 },
        { "strategy_name": "outlier_candidate", "strategy_version": "1.0.0", "score": 0.08 }
      ],
      "metric_breakdown": [
        {
          "metric_name": "top10_mais_sorteados",
          "metric_version": "1.0.0",
          "contribution": 0.44,
          "explanation": "8 dezenas pertencem ao top 10 da janela."
        }
      ],
      "natural_language_summary": "Jogo candidato com maior aderencia ao perfil central de frequencia e repeticao da janela. Consulte `candidate_strategies` para o ranking completo."
    }
  ],
  "metadata": {
    "dataset_version": "cef-2026-04-20-sha7c3a91b2",
    "tool_version": "1.0.0",
    "deterministic_hash": "…"
  }
}
```

#### Observacoes

- `candidate_strategies` e ordenado decrescente por `score` (ADR 0001 D15). O consumidor decide se usa o topo, limiar minimo ou mostra todos. Isso elimina tie-break arbitrario no servidor.
- `metric_breakdown` traz `metric_version` para permitir auditoria cross-versao.

#### Viabilidade

Alta se o motor de geracao ja devolver scores intermediarios. Baixa se a explicacao tiver de ser reconstruida depois.

## Estrategias V1

Definicao canonica completa em `docs/generation-strategies.md`. Apos ADR 0001, as quatro estrategias estao em V1 com score, `search_method` e `tie_break_rule` fechados:

- `common_repetition_frequency@1.0.0`
- `row_entropy_balance@1.0.0`
- `slot_weighted@1.0.0`
- `outlier_candidate@1.0.0`

## Erros de contrato

Formato sugerido:

```json
{
  "error": {
    "code": "INVALID_WINDOW_SIZE",
    "message": "window_size deve ser maior que zero.",
    "details": {
      "window_size": 0
    }
  }
}
```

Codigos iniciais (ADR 0001 D12):

| Codigo | Descricao | Ferramentas que podem emitir |
|--------|-----------|------------------------------|
| `INVALID_REQUEST` | Schema invalido (campo obrigatorio ausente, tipo errado) | todas |
| `INVALID_WINDOW_SIZE` | `window_size` nao-positivo | todas as que aceitam janela |
| `INVALID_CONTEST_ID` | `end_contest_id` ausente do dataset | todas as que aceitam janela |
| `UNKNOWN_METRIC` | Metrica nao listada em `docs/metric-catalog.md` ou pendente sem `allow_pending` | `compute_window_metrics`, `analyze_indicator_stability` |
| `UNSUPPORTED_NORMALIZATION_METHOD` | Metodo incompatible com a serie (ex.: CV em serie com valores ≤ 0) | `analyze_indicator_stability` |
| `UNSUPPORTED_AGGREGATION` | Indicador vetorial sem `aggregation` ou com valor desconhecido | `analyze_indicator_stability` |
| `UNKNOWN_STRATEGY` | Estrategia nao listada em `docs/generation-strategies.md` | `generate_candidate_games` |
| `INCOMPATIBLE_INDICATOR_FOR_STABILITY` | Indicador sem `shape` compativel com ranking global | `analyze_indicator_stability` |
| `INSUFFICIENT_HISTORY` | `min_history > dataset_size` ou janela nao cabe no historico | todas as que aceitam janela |
| `PLAN_BUDGET_EXCEEDED` | `count > MAX_COUNT_PER_STRATEGY` ou soma > `MAX_TOTAL_COUNT` | `generate_candidate_games` |
| `NON_DETERMINISTIC_CONFIGURATION` | Configuracao impede reproducao (ex.: `seed` ausente em estrategia com `sampled`) | `generate_candidate_games` |
| `UNAUTHORIZED` | API key invalida ou ausente | todas |
| `RATE_LIMITED` | Throttling por janela de tempo | todas |
| `QUOTA_EXCEEDED` | Quota total do consumidor excedida | todas |
| `DATASET_UNAVAILABLE` | Snapshot da CEF nao disponivel (indisponibilidade de provider) | todas |
| `INTERNAL_ERROR` | Erro nao-classificado; deve ser raro e rastreavel por hash do request | todas |

## Requisitos de persistencia e cache

O contrato nao exige uma tecnologia especifica, mas exige estas garantias:

1. Leitura canonica consistente por concurso e por janela.
2. Capacidade de recalcular metricas a partir do historico bruto.
3. Rastreabilidade da versao de dados usada em cada resposta.
4. Invalidacao ou versionamento explicito ao mudar o dataset.

### Implicacao tecnica

Isso torna viavel usar:

- arquivo canonico local na V1
- cache local para leituras repetidas
- Table Storage como evolucao operacional
- snapshot em Blob como artefato de distribuicao ou recuperacao

Nao torna viavel, na V1, usar multiplas fontes de verdade sem estrategia clara de reconciliacao.

## Testes minimos para considerar o contrato viavel

1. Mesmo `window_size`, `end_contest_id`, `seed` (quando aplicavel) e `dataset_version` retornam o mesmo `deterministic_hash` em 100% das execucoes.
2. `compute_window_metrics` retorna valores identicos em execucoes repetidas, com `shape` explicito em cada `MetricValue`.
3. `analyze_indicator_stability`:
   - rejeita com `UNSUPPORTED_AGGREGATION` indicador vetorial sem `aggregation`;
   - rejeita com `UNSUPPORTED_NORMALIZATION_METHOD` uso de `coefficient_of_variation` em serie com `min(x) <= 0`;
   - usa `madn` quando `normalization_method` e omitido.
4. `generate_candidate_games`:
   - respeita a composicao `3 + 3 + 3 + 1`;
   - rejeita `count > 100` com `PLAN_BUDGET_EXCEEDED`;
   - rejeita ausencia de `seed` com `INVALID_REQUEST`;
   - dois requests com mesmo `seed` produzem mesmo `deterministic_hash`.
5. `explain_candidate_games` retorna `candidate_strategies` ordenado decrescente por score, com ao menos 2 entradas quando ha ambiguidade real.
6. `divergencia_kl` nunca retorna `+∞` ou `NaN` para janelas com `N ≥ 5` (prova de que o smoothing esta ativo).
7. `row_entropy_balance` com mesmo `seed` e diferentes `n_samples` produz conjuntos consistentes (amostragem determinstica); com `seed` diferentes, produz jogos distintos.

## Avaliacao de viabilidade (pos-ADR 0001)

### Viavel em V1

- `get_draw_window`
- `compute_window_metrics` para o conjunto canonico (todas as metricas com status `canonica` em `metric-catalog.md`)
- `analyze_indicator_stability` com `madn` como default e `aggregation` obrigatoria em vetoriais
- `generate_candidate_games` com as 4 estrategias V1 completas (score, `search_method`, `seed`, `tie_break_rule`)
- `explain_candidate_games` com `candidate_strategies` ranqueadas

### Nao entra na V1

- pesos arbitrarios fornecidos livremente por prompt sem estrategia nomeada
- escrita sincronizada em `Table + Blob + cache local` desde o primeiro dia
- metricas com status `pendente de detalhamento` (hoje: `estabilidade_ranking`) como decisao principal
- `scope: draw` em `MetricValue` (reservado)

## Recomendacao tecnica

Apos o fechamento semantico e de determinismo (ADR 0001), a V1 completa e implementavel com:

1. historico canonico local (arquivo CEF com `dataset_version` derivado)
2. `get_draw_window`
3. `compute_window_metrics`
4. `analyze_indicator_stability` com `madn` default
5. `generate_candidate_games` com as 4 estrategias V1
6. `explain_candidate_games`

Se houver pressao de escopo, a menor V1 testavel possivel e 1+2+3 + uma estrategia (`common_repetition_frequency`), que ja exercita todos os invariantes.
