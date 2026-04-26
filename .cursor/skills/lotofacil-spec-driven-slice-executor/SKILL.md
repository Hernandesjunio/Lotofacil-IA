---
name: lotofacil-spec-driven-slice-executor
description: Executes one spec-driven vertical slice at a time (metric/tool/contract change) using the Lotofacil-IA workflow: spec → tests (domain+contract) → minimal implementation → determinism checks → evidence. Use when implementing a new metric, adding/updating an MCP tool, fixing spec↔code drift, or expanding availability/pipeline per ADRs.
---

# Lotofacil Spec-Driven Slice Executor

## Goal

Turn a **single, explicit recorte** into code with proof:

- spec closed (docs)
- tests written first (domain and/or contract)
- minimal implementation
- deterministic behavior verified
- docs/tests/code aligned

## When to use

Use this skill when the user asks to:

- implement a metric from `docs/metric-catalog.md`
- add or change an MCP tool payload/behavior
- add contract tests / goldens
- address ADR-driven behavior (availability, discovery, window semantics, aggregates)
- fix drift between docs and the implementation

## Hard constraints

- **One recorte at a time**: do not “bundle” multiple tools/metrics unless the spec defines them as a single inseparable unit.
- **No invented semantics**: if a rule is not in the catalog/contract/ADR, treat it as unknown and drive resolution via spec updates first.
- **Determinism**: repeated identical canonical requests must produce the same `deterministic_hash` where the contract requires it.

## Execution loop (always)

1. **Identify the governing spec**:
   - metric semantics: `docs/metric-catalog.md`
   - tool contract/errors/envelopes: `docs/mcp-tool-contract.md`
   - test expectations: `docs/test-plan.md` + `docs/contract-test-plan.md`
   - process sequencing: `docs/spec-driven-execution-guide.md` + `docs/fases-execucao-templates.md`
2. **Write tests first**:
   - domain formula tests (small synthetic fixtures)
   - contract tests (schemas, error codes, determinism, paridade MCP↔HTTP if applicable)
3. **Implement minimally**:
   - respect project boundaries (`Domain`/`Application`/`Infrastructure`/`Server`)
4. **Verify and lock evidence**:
   - run tests
   - freeze goldens only when payload is stable and intended
5. **Update docs only if needed**:
   - if the change is semantic, docs must be updated in the same change set

## ADR routing shortcuts (pick the right playbook)

- **Availability/pipeline/GAPS/stability_check**: ADR 0006
- **Canonical aggregates (`summarize_window_aggregates`)**: ADR 0007
- **Discovery vs norma; window by extremes; legacy Top10 mapping**: ADR 0008
- **Human presentation of window summaries**: ADR 0021 (usually a Rule, not a tool contract change)

## Copy-ready atomic request

Use:

```md
Implemente apenas <passo único>.

Referências obrigatórias:
- <docs/...>

Arquivos esperados:
- <...>

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- seguir contrato MCP e catálogo de métricas;
- manter determinismo.

Critério de pronto:
- <testes passam>
```

## Additional resources

- Slice checklists and “done” gates: see [reference.md](reference.md)
