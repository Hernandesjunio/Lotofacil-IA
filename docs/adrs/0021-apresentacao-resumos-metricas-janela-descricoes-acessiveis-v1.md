# ADR 0021 — Apresentação de resumos de janela: tabelas legíveis, descrições breves e vocabulário acessível

**Navegação:** [← Brief (índice)](../brief.md) · [Glossário de métricas (pedagógico)](../metric-glossary.md) · [Catálogo técnico](../metric-catalog.md) · [ADR 0009 (help / templates)](0009-help-e-catalogo-de-templates-resources-v1.md) · [ADR 0020](0020-flexibilidade-geracao-aleatoria-filtros-opt-in-e-intersecao-v1.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

1. **Respostas técnicas demais para leitura humana:** ao resumir saídas de `compute_window_metrics` (e análises equivalentes), costuma-se montar tabelas com colunas de **shape**, **unidade** e **scope** alinhadas ao [metric-catalog.md](../metric-catalog.md). Isso é correto para **contrato e implementação**, mas **atrapalha** quem só quer entender o que o número *significa* no contexto do volante e do tempo.
2. **Escalares sem narrativa:** métricas como `estabilidade_ranking` chegam como um **único número** em \([0,1]\) sem, no mesmo bloco, uma frase que diga *o que* esse score mede no produto (persistência de ordem entre sub-janelas, não previsão).
3. **Séries com rótulo vazio ou técnico demais:** tabelas do tipo “Métrica | mín | máx | (nota)” deixam a coluna de contexto vaga; termos como **entropia** (bits) e **HHI** assumem familiaridade com teoria da informação ou concentração industrial — vocabulário difícil sem uma **linha de comportamento** (“o que sobe ou desce quando o sorteio fica mais/menos concentrado”).
4. **Duplicação de papel:** o [metric-glossary.md](../metric-glossary.md) já descreve definição e *“O que observa”*; falta **norma de apresentação** que ligue o glossário ao **formato** de tabelas em documentação, ajuda (`help` / resources) e respostas assistidas, sem alterar o **JSON** do MCP.
5. **Profundidade *vs.* custo (tokens):** colunas A/B com **frases fixas** reduzem variação e tamanho da resposta, mas **não** proíbem que o consumidor (ou o pedido do utilizador) peça **interpretação mais rica** (comparar séries, comentar faixa min–máx, explicar em função do valor observado). Isso **aumenta** o uso de contexto e de tokens; a decisão de quando ir além do resumo fica com **pedido explícito** e com política do *host* (por exemplo, “explica o comportamento” *vs.* “só tabela”).

## Decisão

### D1 — Dois *templates* de tabela para “painel resumido” (não normativos do payload JSON)

Estes modelos aplicam-se a **texto voltado a humanos** (briefings, explicações em chat, anexos pedagógicos, seções de resources). O **envelope** `MetricValue` no [mcp-tool-contract.md](../mcp-tool-contract.md) **não** muda; apenas a **apresentação** segue o template.

| Uso | Colunas obrigatórias | Colunas a **omitir** neste modo |
|-----|----------------------|---------------------------------|
| **A — Escalares e listas curtas** (vetores pequenos, listas 10, um escalar) | **Métrica** · **Valor** (ou sub-tabela por dezena quando fizer sentido) · **Descrição** — breve texto do *comportamento* observado naquela janela ou do que o número *mede* | **Forma / unidade** (ex.: `vector_by_dezena` + `count`); quem precisar de tipagem exata usa o **JSON** ou o catálogo. |
| **B — Séries** (um valor por concurso ao longo da janela) | **Métrica** · **Mín.** · **Máx.** · **O que esta série indica (linguagem acessível)** — 1–2 frases, foco no *comportamento*, não no nome isolado do indicador | Coluna genérica **(nota)** ou equivalente; substituir integralmente por **D2**. |

**Requisito de conteúdo (A):** para `estabilidade_ranking` (e qualquer escalar opaco), a coluna **Descrição** deve conter, no mínimo, a ideia de **comparação de rankings de frequência entre sub-janelas consecutivas** e o intervalo \([0,1]\), em linguagem próxima à do glossário (ver *Apêndice* abaixo), **sem** promessa preditiva.

**Requisito de conteúdo (B):** a última coluna **não** é opcional: deve explicar **em concreto** o que varia concurso a concurso (ex.: “como o sorteio se reparte pelas linhas do volante”), e só depois, se necessário, mencionar a unidade técnica entre parênteses.

### D2 — Vocabulário acessível mínimo (obrigatório quando a métrica aparecer na tabela B)

- **`entropia_*_por_concurso`:** descrever como **grau de mistura** das 15 dezenas pelas linhas ou colunas do volante; **mais alto** ≈ mais **espalhado**; **mais baixo** ≈ mais **concentrado** em poucas linhas/colunas. A unidade “bits” pode aparecer **depois** da frase, se útil.
- **`hhi_*_por_concurso`:** descrever como **concentração** da distribuição: valor **mais alto** ≈ mais dezenas nas **mesmas** linhas/colunas; **mais baixo** ≈ repartição mais **uniforme** (HHI é o nome técnico; a frase em português leigo vem antes).
- **`pares_no_concurso`:** quantas dezenas **pares** (2,4,…) entre as 15 sorteadas naquele concurso.
- **`quantidade_vizinhos_por_concurso` / `sequencia_maxima_vizinhos_por_concurso`:** pares de números consecutivos no jogo (ex. 7 e 8) e, na segunda, o **maior** bloco desses no sorteio.

Outras séries do catálogo: reutilizar ou condensar o bloco *“O que observa”* do [metric-glossary.md](../metric-glossary.md).

### D3 — Fonte normativa de texto: glossário + apêndice desta ADR

- A **redação canónica** para copiar/ajustar fica no **[metric-glossary.md](../metric-glossary.md)** (já com `estabilidade_ranking` e demais entradas).
- Esta ADR inclui um **Apêndice** com **frases modelo** (PT) para tabelas A e B, até implementação de secção dedicada no glossário (ver *Consequências*).

### D4 — Relação com ADR 0009 (help e templates)

Quando o conteúdo for **onboarding** ou **template** de prompt ([ADR 0009](0009-help-e-catalogo-de-templates-resources-v1.md)), as tabelas A/B **preferem** descrição a jargão; a camada “contrato/JSON” permanece nas tools.

### D5 — Dois modos de profundidade (resumo *vs.* interpretação; agente / *host*)

A ADR distingue o **papel normativo** das tabelas e frases (D1, D2, Apêndice) do **grau de texto** que a sessão gera, sem mudar o servidor.

| Modo | Quando | O que acontece | Tokens (ordem de grandeza) |
|------|--------|----------------|-----------------------------|
| **Resumo padrão** | Pedido geral, painel, primeira resposta, *help* com espaço curto | Colunas A e B com 1–2 frases; preferir **Apêndice** (ajustar à janela) + *“O que observa”* do glossário, sem improvisar definição contratual. | **Menor** — texto previsível e encurtado. |
| **Interpretação explícita** | Utilizador (ou regra do cliente) pede *explicar o comportamento*, *o que dizer desse min/máx*, *como ler isto com os outros números*, etc. | O agente **pode** interpretar: relacionar **valores concretos** devolvidos pelo MCP, comparar séries, clarificar padrão **descritivo** na janela. Ainda **proibido:** prometer resultado futuro ou afirmar semântica inexistente no [metric-catalog.md](../metric-catalog.md) / glossário. | **Maior** — mais raciocínio, mais parágrafos, mais contexto de dados. |

**Regras para o modo interpretação:**

- **Ancoragem:** toda conclusão sobre “o que a métrica *é*” remete ao **catálogo** + **glossário**; exemplos e analogias servem *explicar* a norma, não a substituir.
- **Dados:** leitura da **janela e valores** reais (incl. min/máx, amostra de série) pode ser usada para descrever *o que aconteceu naquele recorte* — descrição histórica, não previsão.
- **Custo consciente:** o utilizador (e o *host*) podem tratar a interpretação longa como **opt-in** explícito (“explica com mais detalhe”, “o que isso implica *nesta* janela”, …), aceitando mais tokens.

Nada no servidor MCP obriga a interpretação longa; a ADR **autoriza** a camada de UI/agente a fazê-lo quando fizer sentido, sem conflito com o template resumido.

## Consequências

- **Positivo:** alinhamento entre *o que o código calcula* (catálogo) e *o que a documentação e o leigo leem* (glossário + tabelas); redução de confusão entre colunas técnicas e colunas de significado.
- **Repositório:** adicionar em [metric-glossary.md](../metric-glossary.md) uma subsecção **“Textos de resumo para tabelas (ADR 0021)”** com as frases do Apêndice, mantendo o catálogo como fonte de fórmulas; opcional: referência cruzada no [fases-execucao-templates.md](../fases-execucao-templates.md) em fases que descrevam resumos de janela.
- **Positivo (D5):** o agente fica com **governança clara**: resumo curto por defeito; **interpretação condicional** sobre pedido ou política do *host*, com consciência de **custo em tokens** e sem alterar a norma dos números.
- **Não efeita:** `compute_window_metrics`, `DeterministicHash`, `MetricValue` shapes, `discover_capabilities` — a menos que, noutro ADR, se altere semântica de métrica.

## Abertos

- [ ] Inserir no `metric-glossary` a subsecção D3 com textos do Apêndice (pode ser PR separado) — *templates* de execução: [Fase 27.1 no guia de fases](../fases-execucao-templates.md#fase-27---adr-0021-apresentacao-de-resumos-de-janela-tabelas-a-b-glossario-d5) ([spec-driven-execution-guide.md — Fase 27](../spec-driven-execution-guide.md#fase-27---apresentacao-de-resumos-de-janela-adr-0021)).
- [ ] Revisar resources em `resources/help` e `resources/prompts` para aderência voluntária ao template A/B onde houver tabelas exemplo (template 27.4 no mesmo bloco de fases).

---

## Apêndice — Frases modelo (PT) para tabelas

*Base para o **modo resumo padrão** (D1, D5): coluna **Descrição** (A) ou **O que esta série indica** (B). Ajustar o número de concursos ao contexto (“nesta janela de N concursos…”).*  
*No **modo interpretação explícita** (D5), o agente **não** fica preso a repetir literalmente o texto abaixo; usa estas frases como piso mínimo de fidelidade à norma, podendo alargar com o pedido do utilizador e com os **valores** reais, ao custo de mais tokens.*

### Tabela A (escalares e listas curtas)

| Métrica (exemplo) | Frase sugerida |
|-------------------|----------------|
| `estabilidade_ranking` | Mede, entre **sub-janelas consecutivas** do mesmo recorte, se a **ordem** das dezenas por **frequência** tende a manter-se parecida (0 = muito instável, 1 = muito estável). **Não** mede acerto futuro; só padrão no histórico analisado. |
| `frequencia_por_dezena` | Número de vezes em que cada dezena (1 a 25) **saiu** nos concursos **desta janela** (cada concurso conta no máximo uma vez por dezena). |
| `atraso_por_dezena` | Quantos **concursos à frente** do fim desta janela desde a **última** vez em que a dezena saiu; 0 = saiu no **último** sorteio do recorte. |
| `top10_mais_sorteados` / `top10_menos_sorteados` | Lista compacta das dezenas com mais (ou menos) **frequência** de **saída** na janela declarada. |

### Tabela B (séries por concurso)

| Métrica (exemplo) | Frase sugerida |
|-------------------|----------------|
| `entropia_linha_por_concurso` | Indica o quanto o sorteio **mistura** dezenas pelas **5 linhas** do volante. Valor mais alto: mais “espalhado” entre linhas. Mais baixo: mais **concentrado** em poucas linhas. (Unidade técnica: bits de Shannon.) |
| `entropia_coluna_por_concurso` | Igual à ideia da entropia de **linha**, mas para as **5 colunas** do volante. |
| `hhi_linha_por_concurso` | Mede **concentração**: se as dezenas do sorteio se concentram nas **mesmas linhas**. HHI **mais alto** = mais concentrado; **mais baixo** = repartição mais **uniforme** entre linhas. |
| `hhi_coluna_por_concurso` | O mesmo que o HHI de **linha**, para **colunas**. |
| `repeticao_concurso_anterior` | Quantas dezenas **repetem** em relação ao concurso **imediatamente anterior** na janela. |
| `pares_no_concurso` | Quantas das 15 dezenas sorteadas são **números pares** (2, 4, 6, …) naquele concurso. |
| `quantidade_vizinhos_por_concurso` | Conta pares de dezenas **consecutivas** no jogo (ex.: 7 e 8) após ordenar o sorteio. |
| `sequencia_maxima_vizinhos_por_concurso` | Comprimento do **maior** bloco de dezenas consecutivas (diferença 1) naquele concurso. |
| `distribuicao_linha_por_concurso` / `distribuicao_coluna_por_concurso` | Cinco inteiros por sorteio: **quantas** dezenas em cada **linha** (ou coluna) do volante, somando sempre 15. |

---

*Última revisão desta ADR:* alinhar com [metric-glossary.md](../metric-glossary.md) sempre que a definição canónica de uma métrica mudar (bump de versão de métrica / catálogo).
