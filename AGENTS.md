# Orientação para agentes de IA

Este arquivo resume **intenção, limites e fontes normativas** do repositório **Lotofacil-IA**, para outro agente (ou a mesma sessão noutro contexto) orientar-se **sem ler toda a pasta `docs/`**. Não substitui os specs: quando a tarefa alterar semântica, contrato ou métricas, abra os documentos indicados e alinhe código + testes + documentação em conjunto.

**Humanos:** o ponto de entrada narrativo continua no [README.md](README.md).

---

## O que é o projeto

Sistema **educacional** para engenharia de IA aplicada à **Lotofácil**: indicadores estatísticos **determinísticos**, exposição via **MCP/HTTP** em JSON, composição declarativa de análises e geração **reproduzível** de jogos candidatos, com **explicabilidade** (critérios, janela, pesos, filtros, versões).

**Não é:** previsão de resultados, promessa de maior chance, recomendação comercial de apostas, nem “IA embarcada” no servidor que interprete linguagem natural em substituição do contrato.

---

## Fontes de verdade (ordem sugerida)

| Prioridade | Arquivo | Uso |
|------------|----------|-----|
| 1 | [docs/brief.md](docs/brief.md) | Escopo, consumo, restrições técnicas, premissas estatísticas, índice de `docs/`. |
| 2 | [docs/vertical-slice.md](docs/vertical-slice.md) | **V0:** primeira fatia obrigatória (dados → modelo canônico → uma métrica → tools). |
| 3 | [docs/mcp-tool-contract.md](docs/mcp-tool-contract.md) | Contrato das ferramentas MCP, erros, envelopes e invariantes. |
| 4 | [docs/metric-catalog.md](docs/metric-catalog.md) | Nomes, versões, fórmulas e `MetricValue` (incl. `scope`). |
| 5 | [docs/contract-test-plan.md](docs/contract-test-plan.md) | Fixtures douradas, ordem de testes de contrato. |
| 6 | [docs/spec-driven-execution-guide.md](docs/spec-driven-execution-guide.md) | Passos atómicos e ordem prática spec → teste → código. |

Complementos frequentes: [docs/generation-strategies.md](docs/generation-strategies.md), [docs/test-plan.md](docs/test-plan.md), [docs/project-guide.md](docs/project-guide.md), [docs/prompt-catalog.md](docs/prompt-catalog.md).

**ADRs (decisões arquiteturais e de processo):**

- [docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md](docs/adrs/0001-fechamento-semantico-e-determinismo-v1.md)
- [docs/adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md](docs/adrs/0002-composicao-analitica-e-filtros-estruturais-v1.md)
- [docs/adrs/0003-processo-desenvolvimento-bmad-vs-spec-driven.md](docs/adrs/0003-processo-desenvolvimento-bmad-vs-spec-driven.md)
- [docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md](docs/adrs/0004-estrutura-arquitetural-inicial-mcp-dotnet10.md)

---

## Stack e forma de entrega

- **Implementação alvo:** C# / **.NET 10**, servidor **stateless**, **sem LLM** no servidor para cumprir o contrato.
- **Fronteira pública:** HTTP + tools MCP; respostas JSON com rastreabilidade (`dataset_version`, `tool_version`, `deterministic_hash` conforme contrato).
- **Dados:** histórico inicialmente por arquivo (CEF); evolução futura de fontes não deve mudar a semântica das métricas documentadas.

A documentação descreve camadas alvo (`LotofacilMcp.Domain`, `Application`, `Infrastructure`, `Server`). O código em `src/` pode estar em fase de materialização; **a estrutura e os nomes devem convergir com** [docs/project-guide.md](docs/project-guide.md) e **ADR 0004**, não o contrário.

---

## Como deve ser o trabalho (obrigatório moral do repo)

1. **Spec-driven:** semântica nasce em `docs/`; cada entrega é um **recorte** explícito com **testes** correspondentes.
2. **TDD / contrato primeiro:** erros, formas de `MetricValue`, determinismo e validação estrutural importam tanto quanto “feature”.
3. **Fatias verticais pequenas:** começar pela **V0** ([vertical-slice.md](docs/vertical-slice.md)) antes de expandir o catálogo.
4. **Determinismo:** mesmo input válido ⇒ mesmo output canônico; componente estocástico exige `seed` explícito documentado.
5. **Sem defaults ocultos no servidor:** parâmetros incertos devem ser clarificados no **cliente/host** conforme contrato; ver [mcp-tool-contract.md](docs/mcp-tool-contract.md).
6. **Mudança coordenada:** alterar semântica ⇒ atualizar **docs + testes + código** na mesma linha de raciocínio.

---

## Mapa rápido de pastas

| Caminho | Papel |
|---------|--------|
| `docs/` | Especificação normativa, planos de teste, ADRs. |
| `docs/brief.md` | Índice semântico do projeto. |
| `src/` | Código (evoluir para a estrutura em camadas do guia/ADR 0004). |
| `tests/` | Testes de domínio, contrato, integração (quando existirem); fixtures em `tests/fixtures/`. |

---

## Atalhos por tipo de tarefa

| Tarefa | Abrir primeiro |
|--------|----------------|
| Implementar ou alterar uma métrica | [metric-catalog.md](docs/metric-catalog.md), [mcp-tool-contract.md](docs/mcp-tool-contract.md), [test-plan.md](docs/test-plan.md) |
| Nova tool ou mudança de payload MCP | [mcp-tool-contract.md](docs/mcp-tool-contract.md), [contract-test-plan.md](docs/contract-test-plan.md) |
| Geração de jogos / filtros | [generation-strategies.md](docs/generation-strategies.md), [metric-catalog.md](docs/metric-catalog.md) |
| Ordem de implementação / “o que fazer a seguir” | [spec-driven-execution-guide.md](docs/spec-driven-execution-guide.md), [vertical-slice.md](docs/vertical-slice.md) |
| Prompts para validação manual ou automática | [prompt-catalog.md](docs/prompt-catalog.md) |

---

## Lembrete de linguagem e produto

Em texto voltado ao utilizador final, evitar linguagem de **garantia de acerto** ou **previsão**. Indicadores são **descritivos** e **condicionados à janela**; “persistência” e “estabilidade” referem-se a padrões no histórico analisado, não a promessa futura.

---

*Última intenção deste arquivo:* dar um **mapa cognitivo mínimo**. Se a sua alteração tornar qualquer seção deste resumo falsa, atualize **este arquivo** ou o [README.md](README.md) no mesmo conjunto de mudanças.
