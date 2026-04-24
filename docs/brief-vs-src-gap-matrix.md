# Matriz de aderência `docs/brief.md` vs `src/`

## Objetivo

Este documento consolida uma verificação rastreável entre o que o `docs/brief.md` comunica ao consumidor do MCP e o que está efetivamente implementado em `src/`.

O foco aqui é:

- identificar lacunas reais de entrega;
- destacar impactos perceptíveis para quem consome o MCP;
- transformar a análise em backlog técnico acionável.

Itens já aderentes na fundação do servidor, como transporte MCP, resources, tool `help`, `dataset_version`, `tool_version` e `deterministic_hash`, não são o foco principal desta matriz, exceto quando ajudam a contextualizar um gap.

## Escala de status

- `Implementado`: o item prometido pelo brief está materializado e consumível.
- `Parcial`: existe implementação relevante, mas a entrega pública é incompleta, recortada ou inconsistente com a promessa.
- `Ausente`: o item é prometido no brief, mas não foi materializado de forma verificável em `src/`.
- `Congelado (não aprovado)`: item reconhecido como gap, mas explicitamente fora do escopo de implementação no momento (não deve bloquear a sequência dos demais).

## Escala de impacto

- `Alto`: causa frustração direta no consumidor MCP ou quebra expectativa central de uso.
- `Médio`: reduz cobertura funcional relevante, mas com algum contorno possível.
- `Baixo`: não bloqueia o uso principal, porém compromete consistência, clareza ou evolução.

## Resumo executivo

| Área | Situação |
|------|----------|
| Fundação MCP/HTTP, stdio, resources, `help`, rastreabilidade | Boa aderência |
| Cobertura de métricas do domínio documentado | Parcial |
| Disponibilidade coerente entre catálogo, templates e tools | Parcial |
| Geração de jogos com estratégias comparáveis e filtros declarativos | Parcial |
| Janela por extremos de forma uniforme | Parcial |
| Descoberta da superfície real da instância | Parcial |
| Fonte histórica CEF como entrada real | Ausente/parcial operacional |

## Matriz rastreável

| ID | Item do brief | Status | Impacto | Evidência em `src/` | Justificativa da lacuna | Sugestão de implementação |
|----|---------------|--------|---------|---------------------|-------------------------|---------------------------|
| B01 | Cálculo de indicadores base, derivados, estruturais e de estabilidade em cobertura ampla do domínio | Parcial | Alto | `src/LotofacilMcp.Domain/Metrics/WindowMetricDispatcher.cs` implementa um subconjunto de métricas; `src/LotofacilMcp.Application/Validation/MetricAvailabilityCatalog.cs` anuncia um catálogo muito maior | O brief comunica um domínio mais amplo do que a build realmente entrega. O consumidor encontra métricas documentadas ou sugeridas pelo catálogo que não podem ser calculadas | Criar uma única fonte de verdade para métricas com campos `implemented`, `exposed_in_compute_window_metrics`, `allowed_in_aggregates`, `allowed_in_associations`; só expor no MCP o que estiver realmente entregue |
| B02 | Análise histórica de linhas, colunas, pares/ímpares, vizinhos, repetição, top 10 e slots | Parcial | Alto | Há implementação para linhas, colunas, vizinhos, repetição e top 10 em `WindowMetricDispatcher`, mas não para famílias como `pares_impares`, `matriz_numero_slot`, `analise_slot` e `surpresa_slot`, apesar de aparecerem no catálogo nominal em `MetricAvailabilityCatalog.cs` | O usuário espera cobertura homogênea desse bloco do brief, mas a superfície pública é desigual e incompleta, especialmente em slots e pares/ímpares | Implementar primeiro as famílias mais citadas no brief e nos templates: `pares_impares`, `matriz_numero_slot`, `analise_slot`, `surpresa_slot`; depois alinhar catálogo, testes e resources |
| B03 | Estruturação dos dados para consumo analítico e por IA sem inconsistência de superfície | Parcial | Alto | `MetricAvailabilityCatalog.cs` contém métricas "conhecidas" que não estão no dispatcher nem sempre estão expostas em `compute_window_metrics`; `SummarizeWindowAggregatesUseCase.cs` valida nomes conhecidos, mas pode falhar ao despachar | Existe divergência entre o que parece suportado e o que realmente funciona. Isso cria sensação de contrato instável | Substituir listas manuais por um registro central de capacidades da instância e derivar dele validações, help, prompts e descoberta |
| B04 | Séries históricas e agregações por janela para as famílias documentadas | Parcial | Alto | `compute_window_metrics`, `summarize_window_patterns` e `summarize_window_aggregates` existem, mas `summarize_window_patterns` está limitado a `pares_no_concurso` em `src/LotofacilMcp.Application/Validation/V0CrossFieldValidator.cs` | O brief sugere uma cobertura analítica mais ampla de séries e resumos de janela do que o recorte atual permite | Expandir `summarize_window_patterns` para séries escalares já disponíveis, como vizinhos, repetição, entropia e HHI; adicionar cobertura contratual por família |
| B05 | Resumos de padrões típicos, faixas prováveis no sentido descritivo e eventos raros | Parcial | Médio | `src/LotofacilMcp.Application/UseCases/SummarizeWindowPatternsUseCase.cs` implementa apenas resumo IQR em uma feature escalar; não há cobertura ampla de eventos raros por família | A promessa do brief é mais rica do que o que hoje é entregável sem composições ad hoc do consumidor | Introduzir uma camada de resumos descritivos por tipo de série: faixa típica, outliers, raridade relativa e cobertura observada |
| B06 | Composição dinâmica com pesos, transformações e filtros declarativos | Parcial | Médio | `src/LotofacilMcp.Application/UseCases/ComposeIndicatorAnalysisUseCase.cs` e `src/LotofacilMcp.Application/Validation/V0CrossFieldValidator.cs` limitam a composição a `target = dezena` e `operator = weighted_rank` | Há uma implementação válida, mas ainda restrita diante da promessa mais geral do brief | Evoluir a composição em fases: mais operadores, mais targets, eco da configuração aplicada na resposta e testes por combinação suportada |
| B07 | Explicar como cada análise, ranking, filtro ou jogo foi produzido | Parcial | Alto | Algumas tools retornam explicações textuais, mas respostas como `ComposeIndicatorAnalysisResponse` e `GenerateCandidateGamesResponse` em `src/LotofacilMcp.Server/Tools/V0Tools.cs` não devolvem de forma completa os componentes, pesos, filtros e critérios aplicados | O brief promete explicabilidade forte. Hoje parte da explicação existe, mas nem sempre com rastreabilidade suficiente para auditoria do payload | Adicionar um bloco padrão `applied_configuration` a todas as respostas analíticas e de geração |
| B08 | Análise de estabilidade, persistência, associação e divergência | Parcial | Alto | Há `AnalyzeIndicatorStabilityUseCase.cs` e `AnalyzeIndicatorAssociationsUseCase.cs`, mas não há implementação pública de várias métricas do catálogo ligadas a estabilidade/divergência, como `estabilidade_ranking`, `divergencia_kl` e `persistencia_atraso_extremo` | O consumidor vê o tema coberto no brief e em prompts, mas encontra uma oferta parcial na prática | Priorizar as métricas de estabilidade/divergência citadas no catálogo e conectá-las às tools existentes |
| B09 | Correlações entre indicadores com robustez/estabilidade em subjanelas | Parcial | Alto | `stability_check` é aceito no contrato MCP, mas rejeitado em `src/LotofacilMcp.Server/Tools/V0Tools.cs`; `AssociationStability` sai sempre `null` em `src/LotofacilMcp.Application/UseCases/AnalyzeIndicatorAssociationsUseCase.cs` | O contrato sugere uma capacidade que ainda não existe, causando frustração direta em quem tenta usá-la | Implementar análise por subjanelas para associações (honrando `stability_check`) e, até a entrega completa, manter erro canônico/determinístico + publicar via discovery o status e as limitações desta build |
| B10 | Análise histórica de top 10 e slots integrada aos exemplos de uso | Parcial | Alto | `top10_mais_sorteados` existe no dispatcher, mas slots aparecem mais como heurística interna em `src/LotofacilMcp.Application/UseCases/ExplainCandidateGamesUseCase.cs` do que como métricas canônicas MCP | O brief usa slots como elemento de consumo esperado, mas o MCP ainda não oferece essa família como métrica pública consistente | Materializar o bloco de slots como métricas canônicas independentes, com versões e explicações próprias |
| B11 | Geração determinística de jogos candidatos com critérios escolhidos | Parcial | Alto | `src/LotofacilMcp.Application/UseCases/GenerateCandidateGamesUseCase.cs` gera candidatos a partir do ranking de `frequencia_por_dezena`, com uma única estratégia pública `common_repetition_frequency` | A geração existe, mas o espaço de critérios realmente configuráveis é bem menor do que o brief sugere | Evoluir `generate_candidate_games` para aceitar critérios declarativos, pesos e filtros explícitos em vez de depender só de `strategy_name` |
| B12 | Filtros estruturais para excluir padrões raros ou pouco úteis para geração | Parcial | Alto | `src/LotofacilMcp.Application/UseCases/ExplainCandidateGamesUseCase.cs` possui `exclusion_breakdown`, mas esses filtros não entram como configuração formal em `GenerateCandidateGamesRequest` | O sistema explica exclusões depois, mas não permite ao consumidor controlar a exclusão na geração de forma declarativa | Levar filtros como `max_consecutive_run`, `max_neighbor_count`, `min_row_entropy_norm`, `max_hhi_linha` e `min_slot_alignment` para o request de geração |
| B13 | Comparação entre estratégias de geração de jogos | Parcial | Alto | `V0CrossFieldValidator.cs` aceita apenas `common_repetition_frequency` como estratégia de geração; outras "estratégias" aparecem apenas como explicações heurísticas em `ExplainCandidateGamesUseCase.cs` | O brief promete comparação entre estratégias, mas a geração pública tem só uma estratégia real | Implementar pelo menos 2 ou 3 estratégias reais com o mesmo contrato e retornar resultados agrupados por estratégia |
| B14 | Registro de critérios, pesos efetivos, filtros, janela, `seed`, `search_method`, `tie_break_rule` e versão da estratégia usada | Parcial | Médio | `GenerateCandidateGamesResponse` em `src/LotofacilMcp.Server/Tools/V0Tools.cs` retorna `strategy_name`, `strategy_version`, `search_method`, `tie_break_rule` e `seed_used`, mas não devolve critérios, pesos ou filtros aplicados | A rastreabilidade da geração fica incompleta frente ao que o brief exige | Estender o envelope do candidato com `criteria`, `weights`, `filters` e `window_reference` efetivamente usados |
| B15 | Janela por concurso inicial/final como semântica consistente da superfície MCP | Parcial | Alto | `start_contest_id` está disponível em `get_draw_window` e `compute_window_metrics` via `src/LotofacilMcp.Server/Tools/V0McpTools.cs`, mas não foi propagado de forma uniforme para as outras tools de análise | O consumidor precisa alternar entre dois modelos de janela, o que reduz previsibilidade e aumenta ambiguidade operacional | Criar um DTO comum de janela para todas as tools orientadas a histórico e resolver a janela sempre no mesmo ponto |
| B16 | Ausência de contexto oculto e de defaults não documentados no servidor | Parcial | Alto | `AnalyzeIndicatorStabilityUseCase.cs` assume `madn` quando o campo não vem; `GenerateCandidateGamesUseCase.cs` e `V0CrossFieldValidator.cs` assumem `greedy_topk` quando `search_method` não é informado | O brief afirma que parâmetros não inferíveis devem ser explicitados, mas a build ainda injeta defaults relevantes | Tornar parâmetros semânticos obrigatórios ou devolver explicitamente `resolved_defaults` no payload, com documentação visível |
| B17 | `allow_pending` como opt-in real para métricas pendentes | Ausente funcionalmente | Alto | `allow_pending` aparece no request e no hash em `src/LotofacilMcp.Server/Tools/V0Tools.cs` e `ComputeWindowMetricsUseCase.cs`, mas não altera a validação nem a execução | O consumidor pode acreditar que habilitou um modo de métricas pendentes, mas nada muda na prática | Implementar a semântica do opt-in ou remover o parâmetro até existir comportamento verificável |
| B18 | Dados históricos obtidos inicialmente via arquivo da CEF | Congelado (não aprovado) | Médio | `src/LotofacilMcp.Infrastructure/Providers/SyntheticFixtureProvider.cs` lê fixture JSON canônica, não um arquivo CEF real | Gap reconhecido, mas **explicitamente fora do escopo por enquanto** para não interferir na sequência de implementação dos demais pontos | **Não implementar agora.** Manter como dívida registrada; retomar quando o backlog de métricas/contrato estiver estável |
| B19 | Evolução da persistência e armazenamento sem alterar semântica das métricas | Parcial | Baixo | Existe boa separação entre camadas, mas a fonte real está acoplada ao `SyntheticFixtureProvider` em vários use cases | A arquitetura aponta na direção correta, mas a abstração de fonte ainda não está plenamente consolidada | Introduzir uma interface de provider de histórico e injetar implementações por infraestrutura |
| B20 | Descoberta do que a instância expõe, versus o que a norma documenta | Parcial | Alto | `help` em `src/LotofacilMcp.Server/Tools/V0Tools.cs` lista onboarding e templates, mas não publica uma matriz formal de métricas, agregados, filtros e estratégias realmente suportados nesta build | O consumidor aprende por tentativa e erro o que esta instância aceita, o que conflita com a expectativa de descoberta clara | **Adicionar uma tool dedicada de discovery** (ex.: `discover_capabilities`) retornando: tools, schemas/fields relevantes, métricas implementadas/expostas por rota, agregados suportados, estratégias de geração e versões; manter `help` como onboarding e atalhos para templates |
| B21 | Catálogo de prompts e resources cobrindo 100% das famílias de cálculo do domínio documentado | Parcial | Alto | `src/LotofacilMcp.Server/Prompting/PromptCatalog.cs` descreve temas como blocos, estabilidade de ranking, slot, outlier e regime, mas parte dessas famílias não está disponível como superfície canônica MCP | Os templates elevam a expectativa do usuário acima da cobertura real da build | Gerar o catálogo de prompts a partir das capacidades implementadas ou separar claramente prompts suportados de prompts futuros |
| B22 | Perfil estrutural cobrindo slots, vizinhos e top 10 em exemplos de uso esperados | Parcial | Médio | O exemplo do brief depende de uma combinação que hoje exigiria contornos manuais porque `summarize_window_patterns` não cobre slots/top10 e slots não são métricas MCP públicas completas | Os exemplos do brief parecem executáveis, mas não são plenamente reproduzíveis com as tools atuais | Reescrever exemplos para o recorte real da build ou implementar as famílias faltantes até que o exemplo seja executável de ponta a ponta |
| B23 | Métricas por concurso isolado aparecendo como séries históricas agregadas conforme `MetricValue.scope` | Parcial | Médio | A tipagem de `scope` existe em `src/LotofacilMcp.Server/Tools/V0Tools.cs`, mas a cobertura real de métricas por família ainda é insuficiente para sustentar o modelo em todo o domínio documentado | O modelo conceitual está certo, porém a superfície ainda não cobre todas as famílias que deveriam obedecer a essa semântica | Completar a implementação das métricas faltantes usando o mesmo modelo de `scope`, evitando exceções ad hoc por família |
| B24 | Comparação entre estratégias e composições dinâmicas sem reescrever cálculo canônico | Parcial | Médio | Há reaproveitamento parcial do dispatcher e dos use cases, mas composição, geração e explicação usam caminhos diferentes e nem sempre compartilham métricas canônicas de primeira classe | A evolução futura tende a multiplicar lógica paralela se não houver um núcleo canônico único | Consolidar um catálogo central de métricas e um pipeline de composição/regras reutilizado por análise, agregação, geração e explicação |

## Gaps mais críticos para o consumidor MCP

### Prioridade 1

- `B01`: catálogo de métricas maior do que a implementação real.
- `B02`: famílias importantes do brief não estão materializadas na superfície pública.
- `B09`: `stability_check` aparece no contrato, mas não funciona.
- `B11`: geração existe, porém com recorte bem menor do que o brief sugere.
- `B15`: janela por extremos não é uniforme em todas as tools.
- `B17`: `allow_pending` não tem efeito real.
- `B20`: descoberta da superfície real da instância ainda é insuficiente.
- `B21`: prompts/resources sugerem capacidades acima da cobertura real.

### Prioridade 2

- `B04`: resumos de janela ainda muito restritos.
- `B06`: composição dinâmica ainda em recorte pequeno.
- `B12`: filtros estruturais não entram como configuração declarativa da geração.
- `B14`: rastreabilidade da geração ainda incompleta.
- `B18`: ingestão CEF real não implementada (**congelado / não aprovado**).

## Recomendações de execução

### Fase 1: alinhar expectativa pública com a build atual

- Consolidar um registro único de capacidades reais da instância.
- Derivar desse registro:
  - validação de requests;
  - `help`;
  - discovery;
  - catálogo de prompts suportados;
  - disponibilidade por tool.
- **Não remover contratos públicos**: manter o contrato e **implementar as capacidades prometidas**. Enquanto houver gaps, garantir erro canônico e determinístico (com código/diagnóstico) e publicar via discovery o status real por build.

### Fase 2: corrigir os gaps que mais frustram o consumidor

- Implementar `stability_check` (sem remover do contrato).
- Dar semântica real a `allow_pending` (sem remover do contrato).
- Unificar a semântica de janela por extremos em todas as tools.
- Expor como métricas públicas as famílias mais prometidas no brief e mais citadas nos templates:
  - slots;
  - pares/ímpares;
  - blocos;
  - ranking/estabilidade;
  - divergência;
  - outlier.

### Fase 3: amadurecer geração e explicabilidade

- Tornar `generate_candidate_games` declarativa em critérios, pesos e filtros.
- Implementar múltiplas estratégias reais de geração.
- Padronizar `applied_configuration` em todas as tools.
- Garantir que `explain_candidate_games` reflita decisões realmente configuráveis na geração.

## Conclusão

O projeto já entrega uma base técnica consistente de MCP, mas o `brief` ainda descreve um domínio mais completo do que o que está consumível hoje pela instância.

O principal problema de consistência não é a ausência da infraestrutura do MCP, e sim a diferença entre:

- o que o brief, os templates e o catálogo sugerem;
- e o que a implementação realmente aceita, calcula e retorna.

Enquanto essa diferença existir, a sensação para o consumidor continuará sendo de promessa maior do que a entrega real.
