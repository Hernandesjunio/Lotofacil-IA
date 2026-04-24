# Índice de templates (resources) — Lotofacil-IA

Este resource lista templates Markdown prontos para **copiar/colar no chat** ao usar o MCP `lotofacil-ia`.

- **Importante**: isto é conteúdo de referência (resource). O cálculo determinístico é feito via **tools** (`compute_window_metrics`, `summarize_window_*`, etc.).
- **Janela sempre explícita**: o servidor não aplica “últimos N” escondido. Você decide `window_size` + `end_contest_id` (ou `start`/`end` equivalente).

## Comece por aqui (escolha 1 opção)

Se você não sabe por onde começar, escolha uma:

1) **Painel geral (recomendado para primeiro uso)**  
   Abra: `lotofacil-ia://prompts/dashboard-essentials@1.0.0`

2) **Frequência e atraso (por dezena)**  
   Abra: `lotofacil-ia://prompts/frequency-vs-delay@1.0.0`

3) **Repetição entre concursos**  
   Abra: `lotofacil-ia://prompts/repetition-overlap@1.0.0`

4) **Forma (linhas/colunas)**  
   Abra: `lotofacil-ia://prompts/shape-lines-columns@1.0.0`

## Opção 1 — Escrever seu próprio prompt (mini-template)

Copie/cole e ajuste:

```text
Quero montar indicadores via MCP `lotofacil-ia` com objetivo: <descrever>.

Preferência de exibição:
- display_mode = simple | advanced | both (se não declarar, assumir both)

Como alternar (recomendação para o cliente):
- mostre um seletor persistente (toggle) com: Simples | Avançado | Ambos
- o usuário pode trocar a qualquer momento; o cliente só muda o `display_mode` no prompt do LLM
- se o usuário quiser “override” pontual, ele pode escrever explicitamente no texto: `display_mode=advanced` (ou `simple`/`both`)

Janela(s):
- RECENTE: window_size=<ex. 20>, end_contest_id=<último ou id>
- RANKING: window_size=<ex. 100>, end_contest_id=<id>
- BASELINE (opcional): window_size=<ex. 300-500>, end_contest_id=<id>

Regras:
- resposta descritiva (sem promessa/previsão)
- incluir dataset_version, tool_version, deterministic_hash e window em cada chamada

Por favor proponha as chamadas MCP (payloads JSON) e explique como usar o output no UI.
```

Sugestão de default (onboarding):
- no primeiro uso, usar `display_mode=both` (o leigo lê o “Resumo simples”, o expert pula para “Detalhes avançados”)
- depois, lembrar a última escolha do usuário (preferência do cliente)

## Opção 2 — Usar templates pré-programados

Escolha um item abaixo e abra o resource correspondente.

### Templates disponíveis (10)

1) **Dashboard essencial (20/100/500)**  
   - **Resource**: `lotofacil-ia://prompts/dashboard-essentials@1.0.0`  
   - **Quando usar**: painel geral (recente + ranking por dezena) com rastreabilidade e sem linguagem preditiva.

2) **Repetição / sobreposição entre concursos**  
   - **Resource**: `lotofacil-ia://prompts/repetition-overlap@1.0.0`  
   - **Quando usar**: entender repetição contra o concurso anterior e comportamento repetitivo no curto prazo.

3) **Vizinhos e runs no tempo**  
   - **Resource**: `lotofacil-ia://prompts/neighbors-runs@1.0.0`  
   - **Quando usar**: analisar repetição de adjacências (vizinhos) e tamanho de sequências ao longo da janela.

4) **Forma no volante (linhas/colunas)**  
   - **Resource**: `lotofacil-ia://prompts/shape-lines-columns@1.0.0`  
   - **Quando usar**: repetição de forma (distribuição por linha/coluna), dispersão (entropia) e concentração (HHI).

5) **Frequência vs atraso (por dezena)**  
   - **Resource**: `lotofacil-ia://prompts/frequency-vs-delay@1.0.0`  
   - **Quando usar**: painel por dezena com frequência, top10, atraso e estado atual.

6) **Blocos de presença/ausência (satélite)**  
   - **Resource**: `lotofacil-ia://prompts/blocks-presence-absence@1.0.0`  
   - **Quando usar**: investigar repetição em sequências (blocos) por dezena; pode exigir opt-ins dependendo da build.

7) **Estabilidade do ranking (janela média)**  
   - **Resource**: `lotofacil-ia://prompts/ranking-stability@1.0.0`  
   - **Quando usar**: medir se a ordem relativa do ranking por dezena “mudou muito” entre sub-janelas.

8) **Mudança de regime (comparar janelas)**  
   - **Resource**: `lotofacil-ia://prompts/regime-shift@1.0.0`  
   - **Quando usar**: comparar duas janelas (ex.: recente vs anterior) e descrever deslocamentos.

9) **Associações (Spearman) — sanity checks**  
   - **Resource**: `lotofacil-ia://prompts/associations-sanity@1.0.0`  
   - **Quando usar**: co-movimento descritivo entre séries (sem inferir causalidade).

10) **Triagem candidato × histórico**  
   - **Resource**: `lotofacil-ia://prompts/candidate-vs-history-screening@1.0.0`  
   - **Quando usar**: explicar/ranquear jogos candidatos com base em métricas de forma + slot + outlier.

## Observação final (para uso com LLM)
- Se você não souber `end_contest_id`, peça ao agente para chamar `get_draw_window(window_size=1)` e ancorar as janelas no concurso mais recente.

