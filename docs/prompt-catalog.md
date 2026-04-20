# Catálogo de prompts cobertos pelo MCP

Este documento lista prompts de referência para testes funcionais, E2E e validação por IA. Cada prompt deve poder ser respondido apenas com as tools documentadas em `docs/mcp-tool-contract.md`.

## Regras de uso

- Todo prompt deve declarar a janela quando o domínio exigir.
- Prompts com composição dinâmica devem declarar a combinação desejada em linguagem natural suficiente para o agente mapear para o payload estruturado.
- Respostas devem manter linguagem descritiva, não preditiva.

## 1. Estabilidade de indicadores

1. "Quais indicadores estão mais estáveis com menor variação entre os últimos 20 resultados?"
   - Tools esperadas: `analyze_indicator_stability`
2. "Compare estabilidade de repetição, vizinhos, pares e entropia de linha nos últimos 50 concursos."
   - Tools esperadas: `analyze_indicator_stability`
3. "Quais componentes da distribuição por linha estão mais estáveis na janela de 100 concursos?"
   - Tools esperadas: `analyze_indicator_stability`

## 2. Composição dinâmica por dezena

4. "Quais dezenas parecem mais persistentes nos últimos 100 concursos combinando frequência, atraso e blocos de ausência?"
   - Tools esperadas: `compose_indicator_analysis`
5. "Cruze frequência com ausência e slot para ranquear as dezenas mais aderentes ao padrão recente."
   - Tools esperadas: `compose_indicator_analysis`
6. "Mostre um ranking composto de dezenas usando frequência 40%, atraso invertido 30% e assimetria de blocos 30%."
   - Tools esperadas: `compose_indicator_analysis`

## 3. Associações entre indicadores

7. "Quais correlações entre repetição, vizinhos, pares e entropia de linha foram mais estáveis nos últimos 100 concursos?"
   - Tools esperadas: `analyze_indicator_associations`
8. "Existe associação entre quantidade de ocorrência por linha e por coluna?"
   - Tools esperadas: `analyze_indicator_associations`
9. "Compare a correlação entre pares e vizinhos com a correlação entre repetição e entropia."
   - Tools esperadas: `analyze_indicator_associations`

## 4. Padrões históricos

10. "Nos últimos 20 resultados, 80% dos sorteios tiveram qual característica padrão em vizinhos, pares e entropia de linha?"
    - Tools esperadas: `summarize_window_patterns`
11. "Qual a faixa típica de repetição contra o concurso anterior nos últimos 100 concursos?"
    - Tools esperadas: `summarize_window_patterns`
12. "Quantos por cento dos últimos 20 concursos ficaram concentrados nas linhas 2, 3 e 4?"
    - Tools esperadas: `summarize_window_patterns`, `compute_window_metrics`
13. "Quais deslocamentos raros para linha 1 e linha 5 apareceram na janela recente?"
    - Tools esperadas: `summarize_window_patterns`
14. "Quais padrões de coluna cobriram pelo menos 80% dos sorteios?"
    - Tools esperadas: `summarize_window_patterns`

## 5. Slots

15. "Mostre a matriz de número por slot dos últimos 20 concursos."
    - Tools esperadas: `compute_window_metrics`
16. "Qual é o padrão mais aderente de slots na janela recente?"
    - Tools esperadas: `compute_window_metrics`, `summarize_window_patterns`
17. "Qual percentual de jogos segue os slots dominantes observados nos últimos 20 concursos?"
    - Tools esperadas: `summarize_window_patterns`, `compute_window_metrics`

## 6. Top-k e conjuntos de referência

18. "Quais são os top 10 mais sorteados dos últimos 50 concursos?"
    - Tools esperadas: `compute_window_metrics`
19. "Quais são os top 10 menos sorteados da janela de 100 concursos?"
    - Tools esperadas: `compute_window_metrics`
20. "Quantos números do concurso atual estavam no top 10 da janela anterior?"
    - Tools esperadas: `compute_window_metrics`, `compose_indicator_analysis` ou `summarize_window_patterns`

## 7. Repetição e interseções

21. "Quantos números do jogo atual saíram no jogo passado e qual é a variância histórica disso nos últimos 100 concursos?"
    - Tools esperadas: `compute_window_metrics`, `summarize_window_patterns`
22. "Me dê a média, MAD, desvio padrão e range típico da repetição entre concursos consecutivos."
    - Tools esperadas: `summarize_window_patterns`
23. "Analise interseções com lag 2 e lag 3 nos últimos 60 concursos."
    - Tools esperadas: `compute_window_metrics`

## 8. Pares, ímpares e vizinhos

24. "Qual é o ponto de equilíbrio descritivo entre pares e ímpares nos últimos 100 concursos?"
    - Tools esperadas: `summarize_window_patterns`
25. "Mostre a variação histórica da quantidade de vizinhos nos últimos 80 concursos."
    - Tools esperadas: `compute_window_metrics`, `summarize_window_patterns`
26. "Existe correlação entre pares e quantidade de vizinhos?"
    - Tools esperadas: `analyze_indicator_associations`

## 9. Linha, coluna e entropia

27. "Analise a quantidade de ocorrência por linha e por coluna e me diga quais perfis são mais comuns."
    - Tools esperadas: `compute_window_metrics`, `summarize_window_patterns`
28. "Use entropia de linha e HHI para identificar jogos estrutururalmente raros na janela recente."
    - Tools esperadas: `summarize_window_patterns`, `compose_indicator_analysis`
29. "Quais indicadores espaciais parecem mais estáveis: entropia de linha, entropia de coluna, HHI de linha ou HHI de coluna?"
    - Tools esperadas: `analyze_indicator_stability`

## 10. Geração de jogos

30. "Gere um jogo com critérios de menor variação."
    - Tools esperadas: `generate_candidate_games` com `declared_composite_profile`
31. "Gere um jogo com correlação de indicadores mais estáveis."
    - Tools esperadas: `analyze_indicator_associations`, `generate_candidate_games`
32. "Gere jogos eliminando sequências muito longas, baixa entropia e baixa aderência de slot."
    - Tools esperadas: `generate_candidate_games`
33. "Gere um jogo combinando frequência, slot, equilíbrio de pares e vizinhos."
    - Tools esperadas: `generate_candidate_games`
34. "Explique por que esse jogo foi aceito e quais filtros estruturais ele respeitou."
    - Tools esperadas: `explain_candidate_games`

## 11. Prompts negativos

35. "Qual número certamente vai continuar saindo?"
    - Resultado esperado: recusa semântica ou reformulação para persistência histórica
36. "Gere o jogo com maior chance matemática de sair."
    - Resultado esperado: recusa semântica
37. "Combine qualquer peso que você achar melhor."
    - Resultado esperado: exigir pesos explícitos no payload ou usar estratégia nomeada
38. "Ignore a janela e use o contexto do chat para decidir."
    - Resultado esperado: recusa por violar invariantes do contrato
