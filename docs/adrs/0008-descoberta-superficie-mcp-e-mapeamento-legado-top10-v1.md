# ADR 0008 — Descoberta de superfície para consumidores (tools vs resources), resolução de janela por extremos e mapeamento do indicador legado “Top 10” (V1)

**Navegação:** [← Brief (índice)](../brief.md) · [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md) · [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) · [ADR 0007](0007-agregados-canonicos-de-janela-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Aceito.

**Data:** 2026-04-23

## Contexto

1. **Descoberta:** Consumidores (agentes, IDEs, integradores) precisam saber **o que podem obter** desta instância: quais *tools* existem, quais `metric_name@version` a rota `compute_window_metrics` **aceita nesta build**, quais `aggregate_type` existem em `summarize_window_aggregates`, e como interpretar nomes do [metric-catalog.md](../metric-catalog.md). O protocolo MCP já oferece `tools/list`; isso **não** substitui uma visão alinhada ao **catálogo semântico** vs **disponibilidade por build** ([ADR 0006 D1](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)).
2. **Onde publicar “o que o servidor sabe”:** A especificação MCP distingue **Tools** (execução, argumentos, resultados) de **Resources** (dados *read-only* referenciáveis por URI) e **Prompts** (templates com argumentos declarados). O [mcp-tool-contract.md](../mcp-tool-contract.md) trata de Prompts/Resources como opcionais, sem fechar a política de produto para descoberta.
3. **Janela explícita:** O contrato histórico usa `window_size` e `end_contest_id` para ancorar uma janela contígua. Muitos consumidores (e o legado documentado no repositório) expressam a intenção como **concurso inicial e final (inclusivos)**. Sem regra de equivalência normativa, cada cliente reimplementa a conversão de forma divergente.
4. **Indicador de export legado `HistoricoTop10MaisSorteados`:** A UI legado descrita no projeto (ex.: *PopularGraficoQtdTop10*) usava regras temporais ad hoc (p.ex. amostra curta e lookback) **diferentes** da métrica canónica `top10_mais_sorteados@1.0.0` (um ranking sobre **uma** janela declarada). A equipa definiu alinhar o **caso de substituição** ao modelo canónico: **janela não fixa; o chamador declara o intervalo de concursos** usado no cálculo.
5. **Cobertura incompleta entre artefactos:** O ficheiro `indicadores.json` (export de painel) e o `mapeamento.md` (controller + view) **não** listam o mesmo conjunto de informações; há subcampos no JSON sem métrica canónica dedicada e há endpoints legados **sem** chave homónima no export.
6. **Apresentação vs núcleo:** O legado devolve por vezes **percentuais** ou *strings* agregadas (`|02|01|…|`) para gráficos; o catálogo MCP fixa **contagens, séries e estruturas** tipadas — a camada de apresentação (%, labels) permanece no cliente salvo outra decisão explícita.

## Decisões

### D1 — Modelo híbrido de descoberta: *superfície de execução* (tools) + *norma* (resources)

**Decisão:** a descoberta para o consumidor deve combinar:

| Camada | Meio | Conteúdo | Porquê |
|--------|------|----------|--------|
| **A — Instância / build** | *Preferencialmente* respostas de **tool** (ou, no mínimo, erros enriquecidos) que listem o **subconjunto** aceite por esta build (`allowed_metrics`, enums de `aggregate_type`, *tool_version*). | O que **este binário** expõe, alinhado a [ADR 0006 D1](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) e ao contrato. |
| **B — Semântica estável** | **MCP Resources** (URIs versionadas) ou injeção externa dos mesmos `docs/` | Trechos de [metric-glossary.md](../metric-glossary.md), [metric-catalog.md](../metric-catalog.md) e, quando aplicável, *links* a ADRs — conteúdo **read-only**, reutilizável, sem lógica de cálculo. |
| **C — Ergonomia de orquestração** | **MCP Prompts** (opcionais) | Argumentos com nomes alinhados aos schemas das tools, reduzindo campos omissos, sem *defaults* semânticos invisíveis (rem [mcp-tool-contract.md](../mcp-tool-contract.md), secção de Prompts). |

**Justificativa técnica:** Resources não substituem a allowlist de métricas da build (evitam *drift* “doc diz X, servidor aceita Y”). Resources evitam inflar o JSON de toda resposta e servem a hosts que preferem *attach* de documentação. Tools de superfície (ou erros com `details.allowed_metrics`) produzem sinal **falsificável** por teste de contrato e **hashes** alinhados ao *dataset*.

**Não decisão (explicitamente fora do escopo desta ADR):** nome exato e schema JSON de uma tool `list_mcp_surface` (ou equivalente); a implementação escolhe o identificador desde que a semântica de D1 e [ADR 0006 D1](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) sejam respeitadas e o contrato seja actualizado em conjunto.

### D2 — Janela por concurso inicial e final (inclusivos) como forma equivalente

**Decisão:** Quando o consumidor declarar `start_contest_id` e `end_contest_id` (ambos **inclusivos** e contíguos no dataset canónico, sem buracos fora do modelo de janela), a resolução é **equivalente** à já normativa com:

- `end_contest_id` = o valor declarado `end_contest_id` (o extremo *mais recente* da análise);
- `window_size` = `end_contest_id - start_contest_id + 1` (número de concursos na janela contígua).

O servidor pode aceitar *apenas* a forma `window_size` + `end_contest_id` e documentar a conversão no [mcp-tool-contract.md](../mcp-tool-contract.md); *ou* estender o request com campos opcionais `start_contest_id` / `end_contest_id` desde que a janela resultante seja a mesma e **nunca** implícita.

**Justificativa:** reprodutibilidade, um único mapeamento no contrato, e alinhamento a pedidos de UI legado (“da edição *a* à *b*”).

**Arquivos:** [mcp-tool-contract.md](../mcp-tool-contract.md) (entidade `Window` e tools que recebem janela); [metric-catalog.md](../metric-catalog.md) (coluna *Janela* onde fizer sentido); testes de contrato.

### D3 — Mapeamento normativo: `HistoricoTop10MaisSorteados` (export legado) → `top10_mais_sorteados`

**Decisão:** Para substituir a semântica do *export* de UI cujo rótulo sugere “histórico de top 10” por a métrica canónica do catálogo:

- O conteúdo a expor no ecossistema MCP, quando a pergunta for **“quais as dez dezenas mais sorteadas neste recorte”**, é **`top10_mais_sorteados@1.0.0`** (ver [metric-catalog.md](../metric-catalog.md)), calculado **unicamente** sobre a janela resolvida por D2.
- A série *rolling* (top-K condicionada a lookbacks móveis) **não** reutiliza este rótulo legado; se for necessária, deve entrar no catálogo com **outro** `Nome`, parâmetros explícitos (`lookback`, etc.) e versão própria.

**Justificativa:** evita duas definições para o mesmo *nome* de métrica; separa o gráfico antigo (regras múltiplas) da análise declarativa do contrato.

**Arquivos:** [metric-catalog.md](../metric-catalog.md) (nota e Tabela 2, se ainda implícito); [metric-glossary.md](../metric-glossary.md) (interpretação de `top10_mais_sorteados` e distinção de legado).

### D4 — *Defaults* temporais do legado (“últimos 10”, “últimos 20”) não são norma MCP

**Decisão:** Qualquer recorte que no legado aparecia como **N fixo** (p.ex. últimos 10 concursos num gráfico, ou `fim - 20` na view) deve, no MCP, tornar-se **`window_size` + `end_contest_id`** (ou par equivalente por D2) **declarados pelo chamador**. O servidor **não** reproduz N mágico herdado da UI antiga.

**Justificativa:** alinha a [proibição de defaults semânticos ocultos](../mcp-tool-contract.md) e a rastreabilidade da janela nas respostas.

### D5 — Respostas “painel único” (*RecuperarUltimoResultado*, *Gerados*, *LotoDTO*) vs `MetricValue`

**Decisão:** Campos agregados num único JSON (p.ex. `QtdLinha1..5`, `QtPares`, `QtdVizinhos` sobre **um** sorteio ou **um** jogo gerado) são **vistas derivadas**; no domínio canónico decompõem-se em:

- **Sorteio oficial:** `get_draw_window` com `window_size = 1` e `end_contest_id` alvo, mais `compute_window_metrics` com métricas aplicáveis (p.ex. `distribuicao_linha_por_concurso` na janela unitária; séries escalarizadas quando a pergunta for temporal).
- **Jogo gerado:** [generation-strategies.md](../generation-strategies.md), `generate_candidate_games`, `explain_candidate_games` — **não** duplicar semântica de geração nesta ADR.

**Justificativa:** evita “DTO monolítico” como contrato MCP; mantém a descomposição em métricas nomeadas e versionadas.

### D6 — `AnaliseSlots` (análise qualitativa) e `QTDTudoOk` em tabelas de slot

**Decisão:** Texto qualitativo ou flags de UI **sem** fórmula fechada no [metric-catalog.md](../metric-catalog.md) **não** são métricas canónicas até entrada própria (nome, versão, teste). Até lá, o consumidor MCP usa `matriz_numero_slot`, `analise_slot` e `surpresa_slot` onde couber; *gaps* são trabalho de produto/catálogo, não sinónimo silencioso.

## Anexo A — Pente fino: chaves de `indicadores.json` → métricas canónicas (além do Top 10)

| Chave no export | Conteúdo essencial | Métrica(s) / tool(s) canónica(s) | Ressalva |
|-----------------|-------------------|-----------------------------------|----------|
| `PadroesLinha` | `Dados`, `PercentualExistente`, `Ultimos200PadroesSorteados`, `ListaComparacaoHistorica` | `distribuicao_linha_por_concurso`; agregados via `summarize_window_aggregates` ([ADR 0007](0007-agregados-canonicos-de-janela-v1.md)) | `ListaComparacaoHistorica` e regra exacta de `PercentualExistente` **não** têm entrada dedicada na Tabela 1 até especificação; não inferir sem ADR/catálogo. |
| `QtdFrequencia` | vector 1..25 (contagens na janela) | `frequencia_por_dezena@1.0.0` | **Fechado** no [metric-catalog.md](../metric-catalog.md) (*Rótulo de export legado `QtdFrequencia`*). *Não* é `atraso_por_dezena`. O *controller* legado pode ainda servir outro significado noutro ecrã; a norma MCP para o export de referência é frequência. |
| `HistoricoFrequenciaAusencia` | séries por dezena + `ultimoConcurso` | `frequencia_por_dezena`, `atraso_por_dezena`, `frequencia_blocos` / `ausencia_blocos`, `estado_atual_dezena` + `get_draw_window` | Formato `Valores` em *string* é **apresentação**; núcleo = séries/vectores tipados. |
| `HistoricoSlotsOcorrenciaPorColunas` | matriz dezena × slot (raiz JSON é **array**) | `matriz_numero_slot` | Nome sugere “colunas”; a estrutura segue slots 1..15 após ordenação das dezenas (catálogo). |
| `QtdOcorrenciaNumerosColunas` / `QtdOcorrenciaNumerosLinhas` | distribuição agregada no intervalo | `distribuicao_coluna_por_concurso` / `distribuicao_linha_por_concurso` + agregados | Legado pode expor **%**; MCP fixa contagens — ver Contexto, ponto 6. |
| `QuantidadeVizinhosPorJogo`, `QuantidadeMaximaVizinhosSequenciais`, `QtdparesImpares`, `QtdJogosAnteriores` | séries nos últimos N no gráfico legado | `quantidade_vizinhos_por_concurso`, `sequencia_maxima_vizinhos_por_concurso`, `pares_no_concurso`, `repeticao_concurso_anterior` | O **N** do gráfico antigo vira janela explícita (D4). |
| `HistoricoTop10MaisSorteados` | ver D3 | `top10_mais_sorteados` | — |

## Anexo B — Endpoints em `mapeamento.md` **sem** chave homónima em `indicadores.json`

| Endpoint / artefacto | Papel | Onde normatizar no ecossistema MCP |
|------------------------|-------|-----------------------------------|
| `PopularGraficoGerados`, `Gerados` | distribuição por linhas de **jogo gerado** + critérios `LotoDTO` | [generation-strategies.md](../generation-strategies.md), `generate_candidate_games`, `explain_candidate_games`; métricas `distribuicao_linha` sobre o candidato. |
| `RecuperarUltimoResultado` | último sorteio + *snapshot* de métricas estruturais | `get_draw_window` + `compute_window_metrics` (janela 1) ou decomposição explícita; ver D5. |
| `GerarJogos` | geração com *ranges* por linha/coluna/top10/etc. | [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md), [generation-strategies.md](../generation-strategies.md); **não** é extensão desta ADR. |
| `AnalisarJogos` (ViewBag) | monta `PadroesLinha` no servidor | Mesmo Anexo A; *export* pode omitir campos que a view preenche. |
| `PopularGraficoFrequencia` | gráfico por concurso / recorte | `frequencia_por_dezena` para o mesmo mapeamento que o export `QtdFrequencia` (frequência na janela). `atraso_por_dezena` é métrica distinta se a *feature* for “frio” — ver Tabela 2. |

## Anexo C — Métricas canónicas úteis **sem** painel explícito no legado mapeado

Exemplos (lista não exaustiva): `top10_menos_sorteados`, `entropia_linha_por_concurso`, `entropia_coluna_por_concurso`, `hhi_*_por_concurso`, `intersecoes_multiplas`, `divergencia_kl`, métricas de estabilidade (`madn_janela`, …). A **descoberta** (D1) deve permitir ao consumidor saber quais estão **implementadas** na build, ainda que não exista gráfico legado homónimo.

## Alternativas consideradas

1. **Apenas Resources** com o *metric-catalog* completo embebido no servidor — **rejeitado** como *única* solução: o catálogo normativo é mais largo do que a allowlist da build; sem tool ou erro rico, o consumidor **não** sabe a verdade de execução (contradiz [ADR 0006 D1](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)).
2. **Aumentar descrições em `tools/list`** com listas de métricas — **rejeitado** como *substituição completa* (descrições curtas, fáceis de desactualizar, sem paginação nem *hash* coerente com a política de versão de dados).
3. **Misturar no [ADR 0007](0007-agregados-canonicos-de-janela-v1.md)** a política de descoberta e o Top 10 — **rejeitado** para manter 0007 focada em *agregados* e `summarize_window_aggregates` (separação de eixos de decisão).

## Consequências

### Positivas

- Uma linha de raciocínio clara no `docs/`: *norma* (catálogo) vs *instância* (build) vs *pedagogia* (glossário/resources).
- Mapeamento explícito janela por extremos, útil a integradores e a painéis que citam concurso inicial/final.
- Fim de ambiguidade entre o rótulo de *export* “HistoricoTop10MaisSorteados” e a métrica canónica (sob a decisão D3).

### Custos

- Manter **sincronização** entre resources (se houver) e o repositório `docs/`, *ou* gerar resources a partir do mesmo *source* (recomendado no contrato).
- Se forem adicionados campos `start_contest_id` / `end_contest_id` ao request, validação cruzada e testes *golden* adicionais.

## Critérios de verificação

1. [mcp-tool-contract.md](../mcp-tool-contract.md) e [metric-catalog.md](../metric-catalog.md) referenciam explícitamente a ADR 0008 onde a janela por extremos e o mapeamento Top 10 forem normativos.
2. Nenhum documento reutiliza a ADR 0007 para *descoberta* ou *Top 10* sem cross-link a esta ADR.
3. Testes de contrato (quando a implementação existir) cobrem: (a) equivalência `start`/`end` ↔ `window_size`+`end_contest_id` quando suportada; (b) recusa com `INVALID_REQUEST` quando a combinação for ambígua; (c) `top10_mais_sorteados` alinhado à Tabela 2 do catálogo.
4. Mapeamento normativo do export `QtdFrequencia` → `frequencia_por_dezena` consta do [metric-catalog.md](../metric-catalog.md) e do Anexo A; a ambiguidade de texto no `mapeamento.md` do *controller* está alinhada por essa fonte, não o contrário.
5. Qualquer métrica nova necessária para `ListaComparacaoHistorica`, `PercentualExistente` ou `AnaliseSlots` entra no [metric-catalog.md](../metric-catalog.md) com versão e teste, **ou** permanece explicitamente fora de escopo num comentário de produto ligado a este anexo.

## Referências internas

- [mcp-tool-contract.md](../mcp-tool-contract.md) — Primitivas Prompts/Resources; entidade `Window`
- [metric-catalog.md](../metric-catalog.md) — `top10_mais_sorteados`, coluna *Janela*
- [metric-glossary.md](../metric-glossary.md) — definição pedagógica
- [ADR 0005](0005-transporte-mcp-e-superficie-tools-v1.md) — `tools/list` e transporte MCP
- [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) — `allowed_metrics` e *GAPS*
- [ADR 0007](0007-agregados-canonicos-de-janela-v1.md) — agregados; **não** compete com D1–D6 desta ADR
- [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md) — composição e filtros estruturais (geração; Anexo B)
- [generation-strategies.md](../generation-strategies.md) — geração e critérios de jogo (Anexo B)
- [mapeamento.md](../../mapeamento.md) (raiz do repositório) — inventário legado HTTP/UI; **não** é fonte normativa de métricas, apenas input de mapeamento
