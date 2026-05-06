# Issue — `compute_window_metrics` em `verbosity=standard` (multi-métrica) não preenche `Content` e aparece como `JsonElement`

**Estado:** aberto  
**Data:** 2026-05-06  
**Tipo:** bug / DX (MCP) — ajuste de apresentação/transportes, **sem** mudança de semântica das métricas

**Navegação:** [← Brief (índice)](../brief.md) · [Contrato MCP](../mcp-tool-contract.md) · [ADR 0023](../adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)

---

## Resumo

Ao chamar a tool `compute_window_metrics` com:

- `verbosity = "standard"`
- **2+ métricas** em `metrics[]`
- `include_explanations = false` (explicitamente; o default no schema é `true`)

o comportamento observado no host/agente foi:

- nenhum resumo textual útil foi emitido no canal `Content`; e
- o retorno apareceu como **`OK: JsonElement`** (ou seja: **neste host/agente**, o payload estruturado não foi exibido/serializado como JSON legível e o `Content` também não trouxe o resumo esperado).

Isso quebra a experiência esperada descrita no próprio schema das tools (controle de verbosidade “do resumo humano no canal Content”) e no espírito do [ADR 0023](../adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md): `Content` deve trazer pelo menos o “fato saliente” em `standard`, enquanto o payload canônico fica no canal estruturado.

---

## Repro (observado em 2026-05-06)

### Contexto

- Build reportada por `discover_capabilities`: `build=v0`, `tool_version=1.2.0`.
- “Último concurso” ancorado com `get_draw_window { window_size: 1 }` retornou janela `3674..3674`.
- Observação importante: o sintoma **`OK: JsonElement`** é o que **este host/agente** exibiu ao receber a resposta. A issue não assume, sem evidência adicional, que o servidor “não retornou JSON” — pode ser um problema de serialização/apresentação no cliente **ou** um `Content` não preenchido no servidor.

### Caso A — 1 métrica (funciona)

`compute_window_metrics` com 1 métrica e `verbosity="standard"` **emite** resumo no `Content`, por exemplo:

- `frequencia_por_dezena`, `atraso_por_dezena`, `top10_mais_sorteados`, etc.

### Caso B — 2+ métricas (problema)

Chamada (exemplo mínimo):

```json
{
  "window_size": 1,
  "metrics": [
    { "name": "frequencia_por_dezena" },
    { "name": "atraso_por_dezena" }
  ],
  "verbosity": "standard",
  "include_explanations": false
}
```

**Resultado observado no host/agente:** `OK: JsonElement` (sem resumo no `Content`).

Outro exemplo (5 métricas, também reproduz):

```json
{
  "window_size": 1,
  "metrics": [
    { "name": "pares_no_concurso" },
    { "name": "quantidade_vizinhos_por_concurso" },
    { "name": "sequencia_maxima_vizinhos_por_concurso" },
    { "name": "distribuicao_linha_por_concurso" },
    { "name": "distribuicao_coluna_por_concurso" }
  ],
  "verbosity": "standard",
  "include_explanations": false
}
```

**Resultado observado no host/agente:** `OK: JsonElement` (sem resumo no `Content`).

> Nota: o mesmo request com `verbosity="full"` + `include_explanations=true` retorna um `Content` preenchido (ex.: “Window 1 (3674..3674): ...”), então o problema parece estar no caminho “standard + multi + sem explicações”.

---

## Esperado

- `verbosity="standard"` deve preencher o canal `Content` com um **resumo mínimo** mesmo em requests multi-métrica (alinhado à descrição de `verbosity` no schema da tool e às intenções do ADR 0023), por exemplo:
  - identificar a janela (`start..end`);
  - listar cada `metric_name` e um “highlight” do `value` (ex.: valores escalares completos; `top10` completo; para vetores grandes, “top5” + indicação de truncamento).
- `include_explanations=false` deve **apenas remover explicações**, não zerar o resumo humano.
- O payload estruturado deve continuar retornando normalmente (independente do conteúdo textual).

---

## Atual

- Em requests com 2+ métricas (pelo menos nas combinações acima), o host/agente não recebeu texto em `Content` e exibiu apenas `OK: JsonElement`.

---

## Impacto

- Dificulta uso por agentes/hosts que dependem do `Content` para feedback rápido em `standard`, conforme o propósito do `verbosity`.
- Aumenta a chance de “tentativa-e-erro” e de UX inconsistente: 1 métrica funciona, 2+ “vira JsonElement”.

---

## Hipótese técnica (não normativa)

Pode haver um caminho de resposta em que:

- o `StructuredContent` é retornado, mas o host/serializer imprime apenas o tipo (`JsonElement`) ao invés do conteúdo; e
- o builder do `Content` é omitido/curto-circuitado quando `metrics.Count > 1` e `include_explanations=false` (ou quando há projeção/fields).

Alternativamente (ou adicionalmente), pode existir uma regra não desejada do lado do servidor que trate `include_explanations=false` como “suprimir todo texto”, o que conflita com a expectativa de utilidade mínima do `Content` em `verbosity="standard"`.

---

## Proposta de correção (sem alterar semântica)

- Garantir que `compute_window_metrics` sempre emita um `Content` mínimo em `verbosity="standard"` para **qualquer cardinalidade** de `metrics[]`.
- Adicionar teste(s) de contrato para:
  - `verbosity="standard"` + 2 métricas + `include_explanations=false` ⇒ `Content` não-vazio e contendo pelo menos `window` + nomes das métricas.
  - (Opcional) `verbosity="standard"` + N métricas ⇒ `Content` apresenta todos os nomes e highlights determinísticos (com truncamento explícito quando necessário).

---

## Referências

- Schema MCP da tool: `mcps/user-lotofacil-ia/tools/compute_window_metrics.json`
- [ADR 0023](../adrs/0023-controle-de-verbosidade-projecao-e-canais-mcp-para-eficiencia-v1.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md) (seções de `verbosity`, canais e utilidade mínima de `Content`)

