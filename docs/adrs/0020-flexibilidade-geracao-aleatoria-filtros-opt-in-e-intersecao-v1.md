# ADR 0020 — Flexibilidade de geração: aleatoriedade explícita, filtros opt-in por faixas e interseção de comportamentos

**Navegação:** [← Brief (índice)](../brief.md) · [Contrato MCP](../mcp-tool-contract.md) · [Estratégias de geração](../generation-strategies.md) · [ADR 0017](0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md) · [ADR 0019](0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md) · [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md) · [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

Feedback de utilização e de desenho do MCP (incluindo experiência com `generate_candidate_games`) apontou que:

1. **Rigidez percebida:** omitir filtros ou exclusões estruturais **não** significa “sem regras” para o utilizador leigo — o servidor pode aplicar **defaults conservadores** (ex.: limites de vizinhos, entropia mínima, alinhamento de slot), o que conflita com a intenção “só quero números candidatos sem filtro”.
2. **Orçamento de busca:** estratégias amostradas combinam `unique_games` com um **teto de tentativas** por pedido; pedidos grandes (ex.: 100 jogos únicos) podem exigir **várias chamadas** ou aumento explícito de orçamento, o que parece “trabalhoso” sem orientação no contrato.
3. **Volume e erros de escala:** um pedido mal formado ou copiado (ex.: `count` astronómico) pode **sobrecarregar** o servidor e a UI; é desejável um **limite duro** por pedido, forçando **nova rodada** explícita para lotes maiores.
4. **Objetivo do produto (reafirmado):** o sistema **não** visa prever o resultado do sorteio; visa **descrever comportamentos** no histórico, permitir que o utilizador **restrinja** candidatos a regiões do espaço de jogos compatíveis com esses comportamentos (muitas vezes via **faixas** de valores para não fixar um único limiar), e **gerar** candidatos **condicionados** a essas escolhas.
5. **Composição:** o utilizador pretende combinar **vários** critérios; a semântica desejada é a de **interseção** — um candidato só é válido se satisfizer **todas** as restrições declaradas (salvo onde o contrato definir explicitamente um modo alternativo, ex.: perfis nomeados com semântica própria).
6. **`seed` e rigidez percebida:** exigir `seed` **só** para cumprir determinismo estocástico pode **chocar** com o desejo de “gerar sem cerimónia”; o contrato deve clarificar **quando** a reprodutibilidade bit-a-bit é prometida e **quando** não o é, sem obrigar o utilizador a gerir estado de RNG se não lhe interessa.

Os ADRs [0017](0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md) e [0019](0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md) já encaminham geração declarativa e faixas; falta **fechar a política de flexibilidade** face ao utilizador: caminho **óbvio** para “aleatório sem filtro” vs “filtrado por comportamento”, e a semântica de **combinação**.

## Decisão

### D1 — Dois modos normativos de geração (explícitos no request)

O contrato de `generate_candidate_games` deve suportar, de forma **discriminável e sem ambiguidade**, pelo menos:

| Modo | Intenção do utilizador | Comportamento esperado (normativo) |
|------|------------------------|-------------------------------------|
| **`random_unrestricted`** (nome canónico a fechar no contrato MCP) | Gerar candidatos **sem** aplicar filtros estruturais nem critérios de aderência a padrões históricos | Amostragem (ou processo equivalente documentado) sobre o espaço de jogos válidos da Lotofácil (15 dezenas distintas, ordenação canónica), **sem** aplicar os defaults atuais de `structural_exclusions` **a menos que** o utilizador os declare |
| **`behavior_filtered`** (nome canónico a fechar) | Gerar candidatos **condicionados** a comportamentos declarados | Aplica-se **somente** o que foi declarado: critérios, filtros, faixas (`range`, `typical_range`, `allowed_values` conforme [ADR 0019](0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md)), pesos e estratégias; **não** introduzir guardrails não solicitados |

**Regra de transparência (alinhada à ADR 0001 / 0017):** qualquer campo não enviado que o servidor resolver por política interna deve aparecer em `applied_configuration.resolved_defaults` **incluindo** o modo efetivo (`random_unrestricted` vs `behavior_filtered`) e os limites numéricos efetivos.

### D2 — Filtros e critérios são **opt-in**

- **Defaults conservadores** (ex.: `max_neighbor_count`, `min_row_entropy_norm`, etc.) aplicam-se **apenas** no modo `behavior_filtered` **ou** quando o utilizador **omitir** o modo mas **declarar** explicitamente um subconjunto de exclusões estruturais (política exata a fechar: ver *Abertos* abaixo).
- No modo `random_unrestricted`, **proibido** aplicar filtros estruturais não solicitados; se o utilizador quiser “só um” filtro (ex.: só teto de vizinhos), isso é um terceiro recorte explícito no contrato (“subset mínimo declarado”) — a documentar como variante de `behavior_filtered` com lista mínima.

### D3 — Faixas (`range`) para maior abrangência

- Critérios e filtros numéricos devem continuar a privilegiar **faixas** ([ADR 0019](0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md)) em vez de limiares únicos sempre que o utilizador quiser **largura** admissível.
- A resolução determinística da faixa (e o eco em `resolved_defaults`) permanece obrigatória.

### D4 — Interseção de comportamentos

- Sejam \(S_1,\ldots,S_k\) os conjuntos de jogos definidos pelas restrições declaradas (cada uma pode ser um intervalo em métrica/perfil, um filtro estrutural, um critério hard, etc., conforme contrato).
- O conjunto admissível padrão é \(S = \bigcap_{i=1}^{k} S_i\) **salvo** indicação explícita de outra álgebra (ex.: estratégia nomeada que documente união ou ponderação distinta).
- O `count` do plano é sempre relativamente a **candidatos distintos em \(S\)** (quando `unique_games` for verdadeiro), salvo modo documentado de amostragem com reposição.

### D5 — Separação semântica: filtrar comportamento ≠ prever resultado

- Documentação, `help` e respostas devem deixar claro: interseção e faixas **restringem** candidatos a regiões **descritivas** do espaço; **não** implicam maior probabilidade de acerto no sorteio oficial.
- Métricas e critérios permanecem condicionados à **janela** declarada.

### D6 — Limite máximo de **1000** jogos por pedido

- Define-se um teto normativo: a **soma** dos `count` em todos os itens de `plan[]` de um único `generate_candidate_games` **não pode exceder 1000**.
- Pedidos acima desse limite são **`INVALID_REQUEST`** (ou código dedicado documentado), com mensagem que indica **dividir em várias rodadas** (vários requests) se o utilizador precisar de mais volume.
- O limite protege **carga de CPU/memória**, **tempo de resposta** e **UI** (evita erros de escala tipo “1 milhão de jogos” por distracção ou prompt mal supervisionado).

### D7 — `seed` opcional; sem promessa global de replay estocástico

- O campo `seed` deixa de ser **obrigatório** para todos os caminhos de geração no contrato alvo desta ADR.
- **Com `seed` presente** (e demais inputs iguais, mesmo `dataset_version` / versão de tool): o servidor **garante** a reprodutibilidade da **porção estocástica** da geração — isto é, o mesmo pedido canónico produz o **mesmo** conjunto ordenado de candidatos (sujeito ainda a `unique_games` e a regras declaradas).
- **Com `seed` ausente** e existir estocástica na estratégia / método de busca: o servidor **não** garante que uma segunda invocação com o mesmo JSON devolva os mesmos candidatos; a resposta deve indicar explicitamente que a geração foi **não replayável** (nome de campo a fechar no contrato, ex.: `stochastic_episode: true` ou `replay_guaranteed: false`) e documentar o significado de `deterministic_hash` nesse modo (ex.: hash **apenas** dos inputs não aleatórios — janela, modos, critérios resolvidos — **excluindo** a sequência concreta de candidatos).
- **Nota de implementação:** ausência de `seed` no contrato **não** elimina aleatoriedade interna; elimina a **obrigação** para o cliente e a **promessa** de determinismo bit-a-bit, alinhando o produto ao utilizador que prioriza **flexibilidade** face ao replay académico opcional.

## Consequências

### Positivas

- Reduz fricção para o caso de uso “**só quero candidatos aleatórios**” (com ou sem replay explícito via `seed`).
- O teto de **1k** por pedido reduz risco operacional e erros de escala; volumes maiores ficam **explícitos** em múltiplas rodadas.
- Alinha a superfície MCP à intenção pedagógica do [brief](../brief.md): composição, explicação, **sem** promessa preditiva.
- Interseção torna a composição de múltiplos critérios **previsível** para consumidores (UI, agentes, notebooks).

### Negativas / custos

- Exige evolução coordenada de **contrato MCP**, **implementação**, **testes de contrato** e **generation-strategies.md**.
- “Aleatório uniforme no espaço \(\binom{25}{15}\)” pode ser **caro** em alguns métodos; pode ser necessário documentar algoritmo aceite (ex.: rejeição com teto) e o papel de `generation_budget`.
- **Replay opcional:** testes de regressão e demos “golden” que dependam de candidatos idênticos devem **passar a incluir `seed`** ou fixar o modo replayável; há **trade-off** explícito com [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md) na **periferia** desta rota (ver Relações).

## Abertos (para fechamento na mesma linha spec → testes → código)

1. **Nomes finais** dos modos no JSON (`random_unrestricted` / `behavior_filtered` ou equivalente) e onde encaixar (`global_constraints` vs campo novo de topo).
2. **Transição** a partir do comportamento atual: compatibilidade retroativa (requests antigos = equivalente a qual modo?).
3. **Semântica exacta** de amostragem uniforme vs amostragem ponderada apenas no modo filtrado.
4. **Política** quando o utilizador omite o modo mas envia `structural_exclusions` parcial: defaults preenchem só campos ausentes ou o request é inválido até completar?
5. **Código de erro** e texto normativo para violação do teto de 1000 jogos.
6. **Campo canónico** para “geração não replayável” quando `seed` ausente (nome e interacção com `deterministic_hash`).

## Relações

- Refina [ADR 0017](0017-geracao-declarativa-de-candidatos-filtros-e-estrategias-v1.md) (superfície e rastreabilidade) e [ADR 0019](0019-criterios-por-faixa-e-cobertura-na-geracao-v1.md) (faixas).
- **Afinamento face a [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md):** o núcleo do projeto mantém determinismo onde for normativo; esta ADR **qualifica** a rota `generate_candidate_games` para permitir **episódios estocásticos sem `seed`**, **sem** afirmar que todo o ecossistema MCP deixa de ser determinístico. Onde o utilizador **quiser** replay, continua a valer **fornecer `seed`**.
- Composição analítica de filtros no domínio mais amplo: [ADR 0002](0002-composicao-analitica-e-filtros-estruturais-v1.md).

---

*Última intenção deste ADR:* traduzir o **feedback de flexibilidade** do utilizador em decisões normativas — **aleatoriedade explícita**, **filtros só quando pedidos**, **faixas para abrangência**, **interseção** como semântica de combinação, **teto de 1000 jogos por pedido**, e **`seed` opcional** com promessa de replay **apenas** quando declarado — sem confundir com predição de resultado.
