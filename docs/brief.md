# Brief do projeto

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
- análise histórica de linhas, colunas, pares/ímpares, vizinhos, repetição, top-k e slots;
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

Métricas "por concurso isolado" continuam não sendo um escopo independente de `MetricValue.scope`; quando necessário, elas aparecem como séries históricas derivadas de concursos dentro de uma janela. O contrato MCP continua usando `scope = window | series | candidate_game`.

Formato esperado:
- JSON estruturado ou equivalente;
- respostas explicáveis por ferramenta MCP;
- ausência de contexto oculto;
- rastreabilidade por `dataset_version`, `tool_version` e `deterministic_hash`.

## Exemplos de uso esperados

- "Analise os últimos 20 sorteios e identifique quais indicadores tiveram maior estabilidade estatística."
- "Quais dezenas parecem mais persistentes nos últimos 100 concursos quando combino frequência, atraso, blocos de presença e ausência?"
- "Quais correlações entre quantidade de vizinhos, repetição, entropia de linha e pares apresentaram maior coesão na janela recente?"
- "Resuma qual perfil estrutural cobriu pelo menos 80% dos últimos 20 sorteios em slots, vizinhos e top-k."
- "Gere jogos candidatos eliminando sequências longas demais, baixa entropia de linha e aderência de slot muito fraca."

Exemplos adicionais de prompts de teste ficam em `docs/prompt-catalog.md`.

## Restrições técnicas

- Dados históricos obtidos inicialmente via arquivo da CEF.
- Atualizações futuras via API a ser definida.
- Processamento deve ser determinístico (mesmo input => mesmo output). Isso exige: (a) `seed` explícito em toda chamada com componente estocástico, (b) `dataset_version` rastreável por hash do snapshot, (c) `deterministic_hash` canônico por resposta.
- Toda composição dinâmica deve declarar explicitamente componentes, transformações, agregações, pesos, janelas de referência e operadores; não pode haver regra implícita inferida por prompt.
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

## Referências

- `docs/metric-catalog.md` — métricas utilizadas
- `docs/mcp-tool-contract.md` — contrato das ferramentas MCP
- `docs/generation-strategies.md` — estratégias e filtros de geração
- `docs/prompt-catalog.md` — prompts cobertos pelo MCP para testes
- `docs/test-plan.md` — matriz de cobertura de testes do domínio
- `docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md` — decisões de fechamento base da V1
- `docs/adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md` — ampliação da V1 para composições dinâmicas, correlações e filtros
