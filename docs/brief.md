# Brief do projeto

## Objetivo

Construir um sistema educacional para engenharia de IA aplicado a Lotofácil, capaz de:
- calcular indicadores estatísticos determinísticos
- estruturar dados para consumo por IA via MCP
- gerar jogos candidatos por heurísticas descritivas e reproduzíveis
- explicar como cada análise ou jogo foi produzido

O sistema existe para apoiar estudo, análise e tomada de decisão assistida. Ele não deve prometer aumento de chance, acerto futuro ou previsão de resultado.

## Escopo

- **Dentro do escopo:** 
- processamento de concursos da lotofacil
- cálculo de indicadores base e avançados
- Geração de séries históricas e recortes temporais
- Estruturação dos dados para consumo analítico e por IA
- geração determinística de jogos candidatos com critérios escolhidos
- análise de estabilidade de indicadores em janelas configuráveis
- explicação dos critérios, pesos e filtros usados em cada análise
- comparação entre estratégias de geração de jogos

- **Fora do escopo:** 
- outras modalidades de loterias
- recomendação comercial de apostas
- qualquer promessa de aumento de chance de acerto
- modelos preditivos de resultado
- respostas opacas sem justificativa estatística ou semântica

## Forma de consumo

o sistema deve gerar:
- séries históricas por indicador
- agregações por janela (ex: últimos N concursos)
- estrutura padronizada para consumo por IA
- análise de estabilidade dos indicadores em uma janela
- jogos candidatos com justificativa de geração

Métricas "por concurso isolado" não são um escopo ativo da V1; todo indicador opera sobre janela, série ou jogo candidato (ver `docs/mcp-tool-contract.md`, seção `MetricValue.scope`, e ADR 0001 D13).

Formato esperado:
- JSON estruturado ou equivalente
- respostas explicáveis por ferramenta MCP

Exemplos de uso esperados:
- "Analise os últimos 20 sorteios e identifique quais indicadores tiveram maior estabilidade estatística."
- "Gere 10 jogos candidatos, sendo 3 com foco em repetição e frequência, 3 com foco em slot, 3 com foco em peso, distribuição e entropia de linhas, e 1 com critério de outlier."

## Restrições Técnicas

- Dados históricos obtidos inicialmente via arquivo da CEF
- Atualizações futuras via API a ser definida
- Processamento deve ser determinístico (mesmo input => mesmo output). Isso exige: (a) `seed` explícito em toda chamada com componente estocástico, (b) `dataset_version` rastreável por hash do snapshot, (c) `deterministic_hash` canônico por resposta — ver ADR 0001 D1 e D2.
- A geração de jogos deve registrar critérios, pesos, janela, `seed`, `search_method` e versão da estratégia usada
- O sistema deve permitir evolução da persistência sem alterar a semântica das métricas

## Premissas Estatísticas
- Não assumir viés sem validação
- Indicadores são descritivos e inferenciais
- Evitar conclusões baseadas em eventos isolados
- Considerar distribuição teórica e simulação quando aplicável
- "Maior previsibilidade" deve ser tratada como "maior estabilidade estatística na janela analisada"

## Critérios de sucesso

- A mesma entrada deve produzir a mesma análise e os mesmos jogos candidatos
- Cada resultado deve informar quais métricas e pesos foram usados
- O sistema deve permitir comparar estratégias sem reescrever o cálculo canônico
- O contrato MCP deve ser claro o suficiente para automação por IA sem ambiguidade semântica

## Referências

- `docs/metric-catalog.md` — métricas utilizadas
- `docs/mcp-tool-contract.md` — contrato inicial de ferramentas MCP
- `docs/generation-strategies.md` — estratégias iniciais de geração
- `docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md` — decisões de fechamento da V1
