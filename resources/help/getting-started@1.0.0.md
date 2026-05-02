# Lotofacil-IA — Comece por aqui (Getting started)

Este guia é para quem está começando no MCP `lotofacil-ia`.

## O que é (em uma frase)
Ferramenta **educacional**: mostra **métricas e recortes** do histórico (e jogos **candidatos** só para ilustrar critérios), de forma **reproduzível** e rastreável. O modo `full` (e respostas completas) serve para **transparência e estudo** — não para “acertar mais” no sorteio.

## O que não é
Não é previsão nem dica de aposta. Evite frases como “vai sair”, “mais provável”, “aumenta a chance”, “número forte”. No sorteio oficial, **cada combinação simples tem a mesma probabilidade**; o que vês aqui são **regularidades no passado** condicionadas à janela que escolheres.

## Como começar (3 passos)
1) **Peça ajuda**: chame `help`.
2) **Escolha um caminho** (comece por um):
   - **Painel geral (recomendado)**: para ter uma visão rápida do histórico.
   - **Frequência e atraso**: para ver o que aparece mais/menos e há quanto tempo.
   - **Repetição entre concursos**: para comparar com o concurso anterior e padrões de repetição.
   - **Forma (linhas/colunas)**: para ver distribuições no volante.
3) **Escolha o período**:
   - Se você não souber qual é o último concurso, peça “pegar o mais recente” (o sistema ancora o período com uma chamada pequena).

## Se der erro (o que fazer)
- **“Métrica desconhecida”**: você pediu algo que esta versão não oferece. Volte ao índice e escolha outro caminho/template.
- **“Pedido inválido”**: faltou algum campo ou a combinação de campos ficou confusa. Ajuste o pedido e tente de novo.

## Para DEV / integração (opcional)
- O índice de templates fica em `lotofacil-ia://prompts/index@1.0.0`.
- Se for necessário ancorar no concurso mais recente, use `get_draw_window(window_size=1)` e reaproveite o `contest_id`.
- Em respostas de tools, existem campos de rastreabilidade (ex.: `dataset_version`, `tool_version`, `deterministic_hash`, `window`).

