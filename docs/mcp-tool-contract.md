# Contrato das ferramentas MCP

## Objetivo

Definir um contrato explicável, testável e determinístico para um MCP que permita:

- consultar concursos e janelas;
- calcular métricas canônicas;
- analisar estabilidade, composição, associação e padrões históricos;
- gerar jogos candidatos por estratégias nomeadas ou perfil composto declarado;
- explicar resultados de forma reproduzível.

Este contrato não assume capacidade preditiva. O foco é análise descritiva, geração heurística e explicabilidade.

## Decisão de escopo para V1

A V1 expandida deve operar sobre um histórico canônico da Lotofácil e expor poucas tools de alto valor, cada uma com semântica fechada e payload estável.

### Fora da V1

- chat livre dentro do servidor;
- recomendação comercial de apostas;
- linguagem de "jogo provável" ou "chance de sair";
- pesos implícitos inferidos por prompt;
- escrita concorrente em múltiplas fontes sem reconciliação;
- inferência preditiva não validada.

## Modelo conceitual

### Entidades canônicas

#### `Draw`

- `contest_id`
- `draw_date`
- `numbers`: array ordenado crescente com 15 dezenas válidas entre 1 e 25
- `source`
- `ingested_at`

#### `Window`

- `size`
- `start_contest_id`
- `end_contest_id`
- `draws`

#### `MetricRequest`

- `name`
- `params?`: objeto com parâmetros explícitos da métrica, quando necessário
- `aggregation?`: obrigatório quando o `shape` não for escalar e a tool exigir redução
- `component_index?`: usado em métricas vetoriais ou séries vetoriais quando a seleção do componente for explícita

#### `MetricValue`

- `metric_name`
- `scope`: `window | series | candidate_game`
- `shape`: `scalar | series | vector_by_dezena | count_vector[5] | series_of_count_vector[5] | count_matrix[25x15] | count_pair | dezena_list[10] | count_list_by_dezena | dimensionless_pair`
- `window`
- `value`
- `unit`
- `explanation`
- `version`

#### `CandidateGame`

- `numbers`
- `strategy_name`
- `strategy_version`
- `seed_used`
- `search_method`
- `n_samples_used?`
- `scores`
- `constraints_applied`
- `tie_break_rule`
- `rationale`

## Invariantes globais

1. Mesmo input canônico + mesmo `dataset_version` deve produzir o mesmo output.
2. Toda resposta analítica declara a janela usada.
3. Toda resposta de geração declara estratégia, versão, filtros, pesos, `search_method`, `tie_break_rule` e `seed_used` quando aplicável.
4. Nenhuma tool depende de contexto oculto de conversa.
5. Termos como `slot`, `outlier`, `persistência`, `equilíbrio`, `faixa típica` e `correlação` devem ter definição explícita no payload ou na documentação.
6. Ferramentas não devem concluir "mais provável de sair"; devem concluir "mais estável", "mais aderente", "mais persistente no histórico declarado" ou "mais raro".
7. `deterministic_hash = SHA256(canonical_json({input, dataset_version, tool_version}))`.
8. Toda composição dinâmica deve declarar componentes, transformações, agregações, pesos e operador.
9. Toda exclusão estrutural usada na geração deve ser reportada no output.

## Ferramentas propostas

### 1. `get_draw_window`

#### Finalidade

Retornar um recorte canônico de concursos.

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
- Se `end_contest_id` for omitido, usar o concurso mais recente.
- Os concursos devem ser retornados em ordem crescente.

### 2. `compute_window_metrics`

#### Finalidade

Calcular métricas canônicas para uma janela.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "metrics": [
    { "name": "frequencia_por_dezena" },
    { "name": "repeticao_concurso_anterior" },
    { "name": "distribuicao_linha_por_concurso" },
    { "name": "entropia_linha_por_concurso" }
  ]
}
```

#### Regras

- `metrics` é obrigatório.
- Cada item de `metrics` é um objeto; nomes soltos deixam de ser aceitos na V1 expandida.
- Métrica desconhecida retorna `UNKNOWN_METRIC`.
- Métricas `pendente de detalhamento` exigem `allow_pending: true`.
- Parâmetros de métrica devem ser explícitos em `params`; o servidor não infere defaults semânticos escondidos.

#### Observações

- `scope`, `shape`, `unit` e `version` são sempre explícitos.
- A tool cobre tanto métricas clássicas quanto séries estruturais por concurso.

### 3. `analyze_indicator_stability`

#### Finalidade

Comparar indicadores em uma janela e identificar quais apresentam menor volatilidade relativa.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "indicators": [
    { "name": "repeticao_concurso_anterior" },
    { "name": "frequencia_por_dezena", "aggregation": "mean" },
    { "name": "distribuicao_linha_por_concurso", "aggregation": "per_component" }
  ],
  "normalization_method": "madn",
  "top_k": 5,
  "min_history": 20
}
```

#### Regras

- Vetores e séries vetoriais exigem `aggregation`.
- Agregações aceitas: `mean | max | l2_norm | per_component`.
- `per_component` retorna múltiplas entradas no ranking, uma por componente.
- `normalization_method` default: `madn`.
- `coefficient_of_variation` só é aceito para séries positivas.

### 4. `compose_indicator_analysis`

#### Finalidade

Executar composições dinâmicas e declarativas entre indicadores para produzir ranking, filtragem, score composto ou perfil conjunto.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "target": "dezena",
  "operator": "weighted_rank",
  "components": [
    {
      "metric_name": "frequencia_por_dezena",
      "transform": "normalize_max",
      "weight": 0.4
    },
    {
      "metric_name": "atraso_por_dezena",
      "transform": "invert_normalize_max",
      "weight": 0.3
    },
    {
      "metric_name": "assimetria_blocos",
      "transform": "shift_scale_unit_interval",
      "weight": 0.3
    }
  ],
  "top_k": 10
}
```

#### Regras

- `target` aceito: `dezena | draw | candidate_game | indicator`.
- `operator` aceito: `weighted_rank | threshold_filter | joint_profile | stability_rank`.
- Pesos são obrigatórios em `weighted_rank` e devem somar `1.0 ± 1e-9`.
- Transformações aceitas: `normalize_max`, `invert_normalize_max`, `rank_percentile`, `identity_unit_interval`, `one_minus_unit_interval`, `shift_scale_unit_interval`.
- Componentes incompatíveis com o `target` retornam `INCOMPATIBLE_COMPOSITION`.
- A tool não aceita fórmulas livres em texto.

#### Uso esperado

- cruzar frequência com ausência e blocos;
- ranquear dezenas persistentes;
- combinar slot, frequência e equilíbrio estrutural;
- produzir score composto reprodutível.

### 5. `analyze_indicator_associations`

#### Finalidade

Medir associações entre séries de indicadores compatíveis.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "items": [
    { "name": "repeticao_concurso_anterior" },
    { "name": "quantidade_vizinhos_por_concurso" },
    { "name": "pares_no_concurso" },
    { "name": "entropia_linha_por_concurso" }
  ],
  "method": "spearman",
  "top_k": 5,
  "stability_check": {
    "method": "rolling_window",
    "subwindow_size": 20
  }
}
```

#### Regras

- Métodos aceitos: `spearman | pearson`.
- Séries vetoriais exigem `aggregation` antes da associação.
- O output deve separar magnitude da associação e estabilidade da associação.
- Interpretação jamais deve afirmar causalidade.

### 6. `summarize_window_patterns`

#### Finalidade

Resumir padrões dominantes, faixas típicas, cobertura percentual e eventos raros em uma janela.

#### Input

```json
{
  "window_size": 20,
  "end_contest_id": 3400,
  "features": [
    { "metric_name": "quantidade_vizinhos_por_concurso" },
    { "metric_name": "sequencia_maxima_vizinhos_por_concurso" },
    { "metric_name": "pares_no_concurso" },
    { "metric_name": "entropia_linha_por_concurso" }
  ],
  "coverage_threshold": 0.8,
  "range_method": "iqr"
}
```

#### Regras

- A tool produz moda, percentis, cobertura, faixa típica, outliers e texto explicativo.
- Para `distribuicao_linha_por_concurso` e `distribuicao_coluna_por_concurso`, o payload deve declarar `aggregation = per_component` ou `aggregation = mode_vector`.
- Pode responder perguntas do tipo "80% dos sorteios tiveram qual característica?".

### 7. `generate_candidate_games`

#### Finalidade

Gerar jogos candidatos a partir de estratégias nomeadas e filtros declarados.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "seed": 424242,
  "plan": [
    { "strategy_name": "common_repetition_frequency", "count": 3 },
    {
      "strategy_name": "declared_composite_profile",
      "count": 3,
      "profile": {
        "components": [
          { "name": "freq_alignment", "weight": 0.35 },
          { "name": "slot_alignment", "weight": 0.25 },
          { "name": "row_entropy_norm", "weight": 0.15 },
          { "name": "neighbors_balance_score", "weight": 0.15 },
          { "name": "pairs_balance_score", "weight": 0.10 }
        ]
      }
    }
  ],
  "global_constraints": {
    "unique_games": true,
    "sorted_numbers": true
  },
  "structural_exclusions": {
    "max_consecutive_run": 8,
    "max_neighbor_count": 7,
    "min_row_entropy_norm": 0.82,
    "max_hhi_linha": 0.30,
    "min_slot_alignment": 0.08
  }
}
```

#### Regras

- `seed` é obrigatória sempre que houver qualquer estratégia `sampled` ou `greedy_topk`.
- `MAX_COUNT_PER_STRATEGY = 100`; `MAX_TOTAL_COUNT = 250`.
- Estratégia desconhecida retorna `UNKNOWN_STRATEGY`.
- `declared_composite_profile` só aceita componentes listados em `docs/generation-strategies.md`.
- `structural_exclusions` são opcionais, mas quando presentes tornam-se parte do determinismo do request.
- O servidor continua não aceitando "pesos soltos" fora de um schema explícito.

### 8. `explain_candidate_games`

#### Finalidade

Explicar por que os jogos foram gerados e por que outros perfis foram descartados.

#### Input

```json
{
  "window_size": 100,
  "end_contest_id": 3400,
  "games": [
    [1,3,4,5,7,8,10,11,13,15,17,18,20,22,24]
  ],
  "include_metric_breakdown": true,
  "include_exclusion_breakdown": true
}
```

#### Regras

- `candidate_strategies` é ordenado por score decrescente.
- Quando houver exclusões estruturais, a tool deve informar quais filtros o jogo respeitou.
- `metric_breakdown` sempre traz `metric_version`.

## Estratégias V1

Definição canônica em `docs/generation-strategies.md`.

Estratégias V1:

- `common_repetition_frequency@1.0.0`
- `row_entropy_balance@1.0.0`
- `slot_weighted@1.0.0`
- `outlier_candidate@1.0.0`
- `declared_composite_profile@1.0.0`

## Erros de contrato

Formato sugerido:

```json
{
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Campo obrigatório ausente.",
    "details": {
      "missing_field": "metrics"
    }
  }
}
```

| Código | Descrição | Ferramentas que podem emitir |
|--------|-----------|------------------------------|
| `INVALID_REQUEST` | Schema inválido, campo ausente ou tipo errado | todas |
| `INVALID_WINDOW_SIZE` | `window_size` não positivo | tools com janela |
| `INVALID_CONTEST_ID` | `end_contest_id` ausente do dataset | tools com janela |
| `INVALID_REFERENCE_WINDOW` | janela de referência incompatível | composição, padrões, associações |
| `UNKNOWN_METRIC` | métrica não listada no catálogo | métricas, estabilidade, composição, associações |
| `UNKNOWN_STRATEGY` | estratégia não listada em `docs/generation-strategies.md` | geração |
| `UNSUPPORTED_AGGREGATION` | agregação obrigatória ausente ou inválida | estabilidade, composição, associações, padrões |
| `UNSUPPORTED_TRANSFORM` | transformação de composição não suportada | composição, geração |
| `UNSUPPORTED_NORMALIZATION_METHOD` | método incompatível com a série | estabilidade |
| `UNSUPPORTED_ASSOCIATION_METHOD` | método de associação não suportado | associações |
| `UNSUPPORTED_PATTERN_FEATURE` | feature não suportada em resumo de padrões | padrões |
| `INCOMPATIBLE_INDICATOR_FOR_STABILITY` | indicador sem shape compatível para ranking | estabilidade |
| `INCOMPATIBLE_COMPOSITION` | componentes incompatíveis entre si ou com o target | composição, geração |
| `STRUCTURAL_EXCLUSION_CONFLICT` | exclusões tornam o plano inviável ou contraditório | geração |
| `INSUFFICIENT_HISTORY` | histórico insuficiente para a janela pedida | tools com janela |
| `PLAN_BUDGET_EXCEEDED` | `count` acima do orçamento permitido | geração |
| `NON_DETERMINISTIC_CONFIGURATION` | configuração impede reprodução | geração |
| `UNAUTHORIZED` | credencial ausente ou inválida | todas |
| `RATE_LIMITED` | throttling | todas |
| `QUOTA_EXCEEDED` | quota excedida | todas |
| `DATASET_UNAVAILABLE` | dataset indisponível | todas |
| `INTERNAL_ERROR` | erro raro e rastreável por hash | todas |

## Requisitos de persistência e cache

O contrato exige:

1. leitura canônica consistente por concurso e por janela;
2. capacidade de recalcular métricas a partir do histórico bruto;
3. rastreabilidade da versão de dados usada em cada resposta;
4. invalidação ou versionamento explícito ao mudar o dataset.

## Testes mínimos para considerar o contrato viável

1. Mesmo input + mesmo `dataset_version` retornam o mesmo `deterministic_hash`.
2. `compute_window_metrics` retorna valores idênticos em execuções repetidas, com `shape` explícito.
3. `analyze_indicator_stability` rejeita vetoriais sem `aggregation` e usa `madn` por default.
4. `compose_indicator_analysis` rejeita pesos que não somam 1 e componentes incompatíveis.
5. `analyze_indicator_associations` rejeita associação sem redução explícita de série vetorial.
6. `summarize_window_patterns` calcula cobertura, moda e faixa típica de forma determinística.
7. `generate_candidate_games` respeita orçamento, seed, filtros estruturais e estratégia composta declarada.
8. `explain_candidate_games` retorna ranking de estratégias e detalhamento de exclusões.
9. `divergencia_kl` nunca retorna `+∞` ou `NaN` para janelas `N >= 5`.
10. Toda família de prompt documentada em `docs/prompt-catalog.md` deve ter ao menos um teste positivo e um negativo em `docs/test-plan.md`.

## Avaliação de viabilidade

### Viável em V1

- `get_draw_window`
- `compute_window_metrics`
- `analyze_indicator_stability`
- `compose_indicator_analysis`
- `analyze_indicator_associations`
- `summarize_window_patterns`
- `generate_candidate_games`
- `explain_candidate_games`

### Não entra na V1

- pesos textuais livres sem schema;
- `scope = draw` em `MetricValue`;
- narrativa preditiva não validada;
- fórmulas de composição não declaradas no payload.

## Recomendação técnica

A V1 completa e testável fica composta por:

1. histórico canônico local com `dataset_version` derivado;
2. as 8 tools acima;
3. catálogo de métricas fechado;
4. estratégias e filtros estruturais fechados;
5. catálogo de prompts de teste;
6. plano de testes cobrindo métricas, tools, composições, filtros, erros e prompts.
