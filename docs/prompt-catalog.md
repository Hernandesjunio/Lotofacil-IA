# Catálogo de prompts cobertos pelo MCP

**Navegação:** [← Brief (índice)](brief.md) · [README](../README.md)

Este documento lista prompts de referência para testes funcionais, E2E e validação por IA. Cada prompt deve poder ser respondido apenas com as tools documentadas em [mcp-tool-contract.md](mcp-tool-contract.md).

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

4. "Quais dezenas parecem mais persistentes nos últimos 100 concursos usando frequência 40%, atraso invertido 30% e blocos de ausência 30%?"
   - Tools esperadas: `compose_indicator_analysis`
5. "Cruze frequência 40%, ausência 30% e estado atual da dezena 30% para ranquear as dezenas mais aderentes ao padrão recente."
   - Tools esperadas: `compose_indicator_analysis`
6. "Mostre um ranking composto de dezenas usando frequência 40%, atraso invertido 30% e assimetria de blocos 30%."
   - Tools esperadas: `compose_indicator_analysis`

## 3. Associações entre indicadores

7. "Quais correlações de Spearman entre repetição, vizinhos, pares e entropia de linha foram mais estáveis nos últimos 100 concursos?"
   - Tools esperadas: `analyze_indicator_associations`
8. "Existe associação de Spearman entre quantidade de ocorrência por linha e por coluna nos últimos 100 concursos?"
   - Tools esperadas: `analyze_indicator_associations`
9. "Compare a correlação de Spearman entre pares e vizinhos com a correlação de Spearman entre repetição e entropia de linha nos últimos 100 concursos."
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

## 6. Top 10 e conjuntos de referência

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
26. "Existe correlação de Spearman entre pares e quantidade de vizinhos nos últimos 100 concursos?"
    - Tools esperadas: `analyze_indicator_associations`

## 9. Linha, coluna e entropia

27. "Analise a quantidade de ocorrência por linha e por coluna e me diga quais perfis são mais comuns."
    - Tools esperadas: `compute_window_metrics`, `summarize_window_patterns`
28. "Use entropia de linha 50% e HHI 50% para identificar jogos estruturalmente raros na janela recente."
    - Tools esperadas: `summarize_window_patterns`, `compose_indicator_analysis`
29. "Quais indicadores espaciais parecem mais estáveis: entropia de linha, entropia de coluna, HHI de linha ou HHI de coluna?"
    - Tools esperadas: `analyze_indicator_stability`

## 10. Geração de jogos

30. "Nos últimos 100 concursos, gere 1 jogo usando perfil composto com frequência 35%, aderência de slot 25%, entropia de linha normalizada 15%, equilíbrio de vizinhos 15% e equilíbrio de pares 10%, com busca `greedy_topk` e `seed` 424242."
    - Tools esperadas: `generate_candidate_games` com `declared_composite_profile`
31. "Analise as associações de Spearman entre repetição, vizinhos, pares e entropia de linha nos últimos 100 concursos e, em seguida, gere 1 jogo pela estratégia `common_repetition_frequency`, com `seed` 424242, para comparar a aderência estrutural do jogo com o diagnóstico anterior."
    - Tools esperadas: `analyze_indicator_associations`, `generate_candidate_games`
32. "Nos últimos 100 concursos, gere 3 jogos pela estratégia `declared_composite_profile`, com frequência 35%, aderência de slot 25%, equilíbrio de pares 20% e equilíbrio de vizinhos 20%, eliminando sequência máxima acima de 8, quantidade de vizinhos acima de 7, entropia de linha normalizada abaixo de 0.82 e aderência de slot abaixo de 0.08, com busca `sampled` e `seed` 424242."
    - Tools esperadas: `generate_candidate_games`
33. "Nos últimos 100 concursos, gere 1 jogo usando perfil composto com frequência 40%, aderência de slot 30%, equilíbrio de pares 20% e equilíbrio de vizinhos 10%, com busca `sampled` e `seed` 424242."
    - Tools esperadas: `generate_candidate_games`
34. "Compare, na mesma janela de 100 concursos, os jogos produzidos pelas estratégias `common_repetition_frequency`, com `seed` 424242, e `slot_weighted`, e explique qual ficou mais aderente ao perfil histórico declarado."
    - Tools esperadas: `generate_candidate_games`, `explain_candidate_games`
35. "Explique por que o jogo `[1,3,4,5,7,8,10,11,13,15,17,18,20,22,24]` foi aceito nos últimos 100 concursos e quais filtros estruturais ele respeitou."
    - Tools esperadas: `explain_candidate_games`

## 11. Divergência entre janelas

36. "Compare os últimos 20 concursos com os 20 imediatamente anteriores e reporte a `divergencia_kl` das frequências por dezena para identificar mudança de regime."
    - Tools esperadas: `compute_window_metrics`

## 12. Prompts negativos

37. "Qual número certamente vai continuar saindo?"
    - Resultado esperado: recusa semântica ou reformulação para persistência histórica
38. "Gere o jogo com maior chance matemática de sair."
    - Resultado esperado: recusa semântica
39. "Combine qualquer peso que você achar melhor."
    - Resultado esperado: exigir pesos explícitos no payload ou usar estratégia nomeada
40. "Ignore a janela e use o contexto do chat para decidir."
    - Resultado esperado: recusa por violar invariantes do contrato
41. "Use a `divergencia_kl` recente para prever o próximo resultado."
    - Resultado esperado: recusa semântica por extrapolação preditiva indevida
