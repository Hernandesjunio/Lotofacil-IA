# Brief do projeto

**Navegação:** [README do repositório](../README.md)

Este arquivo é o **índice** da pasta `docs/`: a seção **Referências** (no fim deste arquivo) aponta para os demais documentos.

## Objetivo

Construir um sistema educacional para engenharia de IA aplicado a Lotofácil, capaz de:
- calcular indicadores estatísticos determinísticos;
- estruturar dados para consumo por IA via MCP;
- compor análises dinâmicas a partir de indicadores canônicos;
- gerar jogos candidatos por heurísticas descritivas e reproduzíveis;
- explicar como cada análise, ranking, filtro ou jogo foi produzido.

O sistema existe para apoiar estudo, análise e tomada de decisão assistida. Ele não deve prometer aumento de chance, acerto futuro ou previsão de resultado.

## Escopo

- **Dentro do escopo:**
- processamento de concursos da Lotofácil;
- cálculo de indicadores base, derivados, estruturais e de estabilidade;
- geração de séries históricas e recortes temporais;
- estruturação dos dados para consumo analítico e por IA;
- composição dinâmica de indicadores com pesos, transformações e filtros declarativos;
- análise de estabilidade, persistência, associação, divergência e faixas típicas;
- análise histórica de linhas, colunas, pares/ímpares, vizinhos, repetição, top 10 e slots;
- geração determinística de jogos candidatos com critérios escolhidos;
- filtros estruturais para excluir padrões raros ou pouco úteis para geração;
- explicação dos critérios, pesos e filtros usados em cada análise;
- comparação entre estratégias de geração de jogos.

- **Fora do escopo:**
- outras modalidades de loterias;
- recomendação comercial de apostas;
- qualquer promessa de aumento de chance de acerto;
- modelos preditivos de resultado;
- respostas opacas sem justificativa estatística ou semântica.

## Forma de consumo

O sistema deve gerar:
- séries históricas por indicador;
- séries históricas por atributo estrutural do concurso;
- agregações por janela (ex.: últimos N concursos);
- estruturas padronizadas para consumo por IA;
- análise de estabilidade, persistência e associação entre indicadores;
- resumos de padrões típicos, faixas prováveis no sentido descritivo e eventos raros;
- jogos candidatos com justificativa de geração e de exclusão.

Métricas "por concurso isolado" continuam não sendo um escopo independente de `MetricValue.scope`; quando necessário, elas aparecem como séries históricas agregadas a partir dos concursos dentro de uma janela. O contrato MCP continua usando `scope = window | series | candidate_game`.

Formato esperado:
- JSON estruturado ou equivalente;
- respostas explicáveis por ferramenta MCP;
- ausência de contexto oculto;
- rastreabilidade por `dataset_version`, `tool_version` e `deterministic_hash`.

## Exemplos de uso esperados

- "Analise os últimos 20 sorteios e identifique quais indicadores tiveram maior estabilidade estatística."
- "Quais dezenas parecem mais persistentes nos últimos 100 concursos quando combino frequência, atraso, blocos de presença e ausência?"
- "Quais correlações entre quantidade de vizinhos, repetição, entropia de linha e pares apresentaram maior coesão na janela recente?"
- "Resuma qual perfil estrutural cobriu pelo menos 80% dos últimos 20 sorteios em slots, vizinhos e top 10."
- "Gere jogos candidatos eliminando sequências longas demais, baixa entropia de linha e aderência de slot muito fraca."
- "Gere 10 jogos candidatos que satisfaçam **faixas** (ranges) de pares, vizinhos e repetição, sem fixar valores exatos, e explique quais restrições foram aplicadas (faixa explícita vs faixa típica por cobertura)."

Exemplos adicionais de prompts de teste ficam em [prompt-catalog.md](prompt-catalog.md).

## Ajuda e templates (resources) para uso com LLM

Para reduzir ambiguidade de janela e padronizar combinações de métricas por objetivo, a instância MCP expõe:

- a tool `help`, que retorna um índice básico de templates;
- um resource de onboarding curto `lotofacil-ia://help/getting-started@1.0.0` (ponto de entrada agnóstico ao host);
- resources Markdown sob `lotofacil-ia://prompts/` (incluindo `index@1.0.0`) prontos para copiar/colar no chat.

Esses templates **não** substituem tools nem mudam o contrato: são conteúdo de referência para facilitar o mapeamento NL → JSON com janelas explícitas.

## Restrições técnicas

- **Stack (implementação MCP):** C# / **.NET 10**.
- Dados históricos obtidos inicialmente via arquivo da CEF.
- Atualizações futuras via API a ser definida.
- Processamento deve ser determinístico (mesmo input => mesmo output). Isso exige: (a) `seed` explícito em toda chamada com componente estocástico, (b) `dataset_version` rastreável por hash do snapshot, (c) `deterministic_hash` canônico por resposta.
- Toda composição dinâmica deve declarar explicitamente componentes, transformações, agregações, pesos, janelas de referência e operadores; não pode haver regra implícita inferida por prompt.
- Quando a intenção do usuário não permitir inferir com segurança os parâmetros das tools MCP, o fluxo (agente/host) deve fazer **perguntas específicas** alinhadas ao contrato até obter um JSON válido; o servidor não supre defaults não documentados. Detalhes e uso opcional de **Prompts/Resources** do protocolo MCP estão em [mcp-tool-contract.md](mcp-tool-contract.md).
- A geração de jogos deve registrar critérios, pesos efetivos, filtros, janela, `seed`, `search_method`, `tie_break_rule` e versão da estratégia usada.
- O sistema deve permitir evolução da persistência e do armazenamento sem alterar a semântica das métricas.

## Premissas estatísticas

- Não assumir viés sem validação.
- Indicadores são descritivos e inferenciais, não preditivos por padrão.
- Evitar conclusões baseadas em eventos isolados.
- Considerar distribuição teórica, percentis, séries temporais e simulação quando aplicável.
- "Maior previsibilidade" deve ser tratada como "maior estabilidade estatística na janela analisada".
- "Persistência" deve ser tratada como regularidade observada no histórico, nunca como garantia de ocorrência futura.
- "Probabilidade" só deve aparecer quando vier de distribuição empírica declarada no payload e acompanhada de interpretação não preditiva.

## Critérios de sucesso

- A mesma entrada deve produzir a mesma análise e os mesmos jogos candidatos.
- Cada resultado deve informar quais métricas, transformações, pesos, filtros e referências foram usados.
- O sistema deve permitir comparar estratégias e composições dinâmicas sem reescrever o cálculo canônico.
- O contrato MCP deve ser claro o suficiente para automação por IA sem ambiguidade semântica.
- O catálogo de prompts e o plano de testes devem cobrir 100% das famílias de cálculo do domínio documentado.
- A **disponibilidade** de cada métrica em cada rota, o **pipeline mínimo** de tools (fluidez sem defaults ocultos) e a semântica de respostas parciais/erros (ex. estabilidade de associação) seguem [ADR 0006](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md), em conjunto com o contrato MCP e o [metric-catalog.md](metric-catalog.md). A **descoberta** do que uma instância expõe (vs. norma do catálogo), **resources** vs **tools**, e a **janela por concurso inicial/final** seguem [ADR 0008](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md).

## Referências

- [metric-catalog.md](metric-catalog.md) — métricas utilizadas (tipagem, fórmulas, consumidores)
- [metric-glossary.md](metric-glossary.md) — definição em linguagem simples, o que cada métrica observa e exemplos de uso (complemento pedagógico)
- [mcp-tool-contract.md](mcp-tool-contract.md) — contrato das ferramentas MCP
- [generation-strategies.md](generation-strategies.md) — estratégias e filtros de geração
- [prompt-catalog.md](prompt-catalog.md) — prompts cobertos pelo MCP para testes
- [test-plan.md](test-plan.md) — matriz de cobertura de testes do domínio
- [brief-vs-src-gap-matrix.md](brief-vs-src-gap-matrix.md) — matriz de aderência do brief vs implementação e backlog técnico (GAPs B01–B24)
- [spec-driven-execution-guide.md](spec-driven-execution-guide.md) — ordem prática de execução, passos atômicos e uso dos specs na implementação (inclui **Fase 25**: fechamento sistemático dos GAPs do brief)
- [fases-execucao-templates.md](fases-execucao-templates.md) — pedidos atômicos por fase (0–20 do guia e extensões; inclui **Fase 25**: ADR 0010–0018)
- [live-openai-integration-pipeline.md](live-openai-integration-pipeline.md) — integração real com API OpenAI, suíte mínima L1–L5 e esteira GitHub dedicada
- [vertical-slice.md](vertical-slice.md) — primeira fatia de implementação (dados → métrica única → MCP)
- [contract-test-plan.md](contract-test-plan.md) — ordem de execução, fixtures douradas e matriz de testes de contrato
- [adrs/0001-fechamento-semantico-e-determinismo-v1.md](adrs/0001-fechamento-semantico-e-determinismo-v1.md) — decisões de fechamento base da V1
- [adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md](adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md) — ampliação da V1 para composições dinâmicas, correlações e filtros
- [adrs/0003-processo-desenvolvimento-bmad-vs-spec-driven.md](adrs/0003-processo-desenvolvimento-bmad-vs-spec-driven.md) — spec-driven como padrão; BMAD opcional
- [adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md](adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md) — estrutura arquitetural inicial congelada para V0/V1 em .NET 10
- [adrs/0005-transporte-mcp-e-superficie-tools-v1.md](adrs/0005-transporte-mcp-e-superficie-tools-v1.md) — transporte MCP, convivência com HTTP, rollout de tools
- [adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md](adrs/0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) — inter-tool, disponibilidade de métricas, pipeline, fluidez, testes de GAPS e pares–entropia
- [adrs/0007-agregados-canonicos-de-janela-v1.md](adrs/0007-agregados-canonicos-de-janela-v1.md) — agregados canônicos (histogramas, padrões e matrizes) via `summarize_window_aggregates`
- [adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md](adrs/0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) — descoberta para consumidores (tools vs resources), janela por extremos, mapeamento `HistoricoTop10MaisSorteados` → `top10_mais_sorteados`
- [adrs/0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md](adrs/0010-plano-de-fechamento-de-gaps-brief-vs-src-v1.md) — governança e plano por clusters para fechar GAPs do brief vs `src/`
- [adrs/0011-tool-de-discovery-de-capacidades-por-build-v1.md](adrs/0011-tool-de-discovery-de-capacidades-por-build-v1.md) — tool `discover_capabilities` (surface discovery por build)
- [adrs/0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md](adrs/0012-registro-unico-de-metricas-e-disponibilidade-por-rota-v1.md) — registro único de métricas/capacidades e allowlists por rota
- [adrs/0013-janela-uniforme-por-extremos-em-todas-as-tools-v1.md](adrs/0013-janela-uniforme-por-extremos-em-todas-as-tools-v1.md) — janela por extremos em toda a superfície
- [adrs/0014-semantica-real-de-allow-pending-v1.md](adrs/0014-semantica-real-de-allow-pending-v1.md) — semântica observável de `allow_pending`
- [adrs/0015-estabilidade-em-subjanelas-para-associacoes-stability-check-v1.md](adrs/0015-estabilidade-em-subjanelas-para-associacoes-stability-check-v1.md) — implementar `stability_check`/`association_stability`
- [adrs/0016-expansao-de-resumos-de-janela-e-padroes-v1.md](adrs/0016-expansao-de-resumos-de-janela-e-padroes-v1.md) — expansão de `summarize_window_patterns`
- [adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md](adrs/0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md) — geração declarativa (critérios/filtros/estratégias) e rastreabilidade
- [adrs/0018-pacote-de-metricas-prioritarias-slots-pares-blocos-outliers-v1.md](adrs/0018-pacote-de-metricas-prioritarias-slots-pares-blocos-outliers-v1.md) — pacote de métricas prioritárias para fechar “missing metrics”
