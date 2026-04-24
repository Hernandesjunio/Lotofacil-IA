# ADR 0010 — Plano de fechamento dos GAPs (brief vs src) e governança de execução v1

**Navegação:** [← Brief (índice)](../brief.md) · [Matriz brief vs src](../brief-vs-src-gap-matrix.md) · [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md) · [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md) · [Contrato MCP](../mcp-tool-contract.md)

## Status

Proposto.

**Data:** 2026-04-24

## Contexto

A análise de consistência entre `docs/brief.md` e `src/` foi consolidada em `docs/brief-vs-src-gap-matrix.md`.

O problema reportado por consumidores do MCP é recorrente:

- o brief (e os templates/resources) comunicam uma cobertura funcional maior do que a build atual entrega;
- métricas “citadas” não funcionam quando pedidas (ou funcionam só em partes da superfície);
- alguns parâmetros aparecem no contrato e no payload, mas não produzem efeito na execução.

Este ADR existe para fixar **governança de fechamento** e **critérios de aceite** para os GAPs sem criar uma “mega ADR” para todas as decisões técnicas.

## Decisão

1) O fechamento de GAPs será realizado via **ADRs pequenas por cluster** (0011+), cada uma contendo:

- decisão e motivação,
- especificação mínima (inputs/outputs/códigos de erro),
- critérios de verificação (contrato/testes),
- migração/rollout quando necessário.

2) Este ADR 0010 é **guarda‑chuva**: define a forma de trabalho e o mapa de dependências, sem substituir os ADRs específicos.

3) Política de contrato público:

- **Não remover contratos públicos**: a estratégia é **implementar o que foi exposto**.
- Enquanto um gap existir, o servidor deve responder com **erro canônico e determinístico**, e a superfície deve permitir **descoberta do status real por build** (ver ADR 0008 D1; reforço no ADR 0011).

4) Item explicitamente congelado:

- **B18 (ingestão CEF real)** permanece **Congelado (não aprovado)** por decisão de produto/execução. Nenhum ADR 0011+ deve depender de B18 para avançar (ver matriz).

## Pacotes (clusters) e ADRs derivadas

Os clusters abaixo correspondem aos blocos mais críticos da matriz:

- **ADR 0011** — Tool de discovery de capacidades por build (B20 + suporte a B21).
- **ADR 0012** — Registro único de métricas + disponibilidade por rota (B01/B03 + efeito em B21; alinhado ao ADR 0006 D1).
- **ADR 0013** — Janela uniforme por extremos em todas as tools (B15; alinhado ao ADR 0008 D2).
- **ADR 0014** — Semântica real de `allow_pending` (B17).
- **ADR 0015** — Implementação de `stability_check` em associações (B09; alinhado ao ADR 0006 D2).
- **ADR 0016** — Expansão de resumos de janela/padrões (B04/B05).
- **ADR 0017** — Geração declarativa (critérios/filtros/estratégias) e rastreabilidade (B11–B14).
- **ADR 0018** — Pacote de métricas canônicas prioritárias (slots, pares/ímpares, blocos, ranking/estabilidade, divergência, outlier) (B02/B08/B10).

## Critérios de “gap fechado” (definição operacional)

Um GAP do tipo “métrica não implementada / inconsistência de superfície” só é considerado fechado quando:

1) **Código**: implementação existe em `Domain` + orquestração em `Application` + exposição correta em `Server` (quando aplicável).
2) **Contrato**: `docs/mcp-tool-contract.md` e/ou `docs/metric-catalog.md` refletem a mudança, com versão e semântica explícitas.
3) **Descoberta**: a tool de discovery (ADR 0011) reporta o status e a disponibilidade por rota da build.
4) **Testes**: há teste de contrato (ou suíte mínima equivalente) cobrindo caso feliz e caso de erro canônico.

## Consequências

### Positivas

- Evita “mudança invisível”: cada cluster vira uma decisão rastreável.
- Reduz frustração no consumidor: discovery + erros canônicos minimizam trial-and-error.
- Permite roadmap técnico incremental sem travar em B18.

### Trade-offs

- Mais arquivos ADR para manter; porém com escopo pequeno e revisão mais fácil.

## Referências internas

- [brief-vs-src-gap-matrix.md](../brief-vs-src-gap-matrix.md)
- [ADR 0006](0006-inter-tool-fluidez-pipeline-e-disponibilidade-v1.md)
- [ADR 0008](0008-descoberta-superficie-mcp-e-mapeamento-legado-top10-v1.md)
- [mcp-tool-contract.md](../mcp-tool-contract.md)
