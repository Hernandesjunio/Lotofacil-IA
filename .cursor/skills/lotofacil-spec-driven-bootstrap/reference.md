# Reference — Lotofacil Spec-Driven Bootstrap

This file is intentionally more detailed than `SKILL.md`. Read this only when the user asks for deeper guidance, checklists, or “what next” sequencing.

## Critical distinctions (avoid drift)

- **Norma vs instância**: `docs/*` is normative. A running build may expose only a subset (allowlists). Discovery must never be faked by reading source.
- **MCP protocol vs REST mirror**: “MCP” means `tools/list`, `tools/call`, etc. REST endpoints that mimic tools are **not** MCP.
- **Human presentation vs JSON contract**: ADR 0021 controls **how to present to humans** (tables A/B, D5). It does not change the MCP envelope by itself.

## Default deliverable for a new repo (recommended)

Create a “project kit” that includes:

- `README.md`: narrative entry point (“what it is / what it isn’t”, how to run, safety language).
- `AGENTS.md`: map of truth sources and non-negotiables.
- `docs/brief.md`: scope, constraints, determinism, “no prediction”, consumption model.
- `docs/project-guide.md`: folder layout + boundaries (Domain/Application/Infrastructure/Server).
- `docs/metric-catalog.md`: metric names, versions, scope/shape/unit, formulas (closed semantics).
- `docs/metric-glossary.md`: human wording, usage examples, ADR 0021 texts for tables.
- `docs/mcp-tool-contract.md`: tool schemas, error codes, envelopes, determinism_hash policy.
- `docs/vertical-slice.md`: V0 definition (data → canonical model → 1 metric → tools).
- `docs/test-plan.md`: coverage meaning (“100%” definition) and matrix.
- `docs/contract-test-plan.md`: goldens, contract tests ordering, special phases (B.1/B.2).
- `docs/spec-driven-execution-guide.md`: the practical sequence spec → test → code.
- `docs/fases-execucao-templates.md`: copy-ready atomic templates (“Implemente apenas…”).
- `docs/adrs/*.md`: only for structural decisions; avoid ADR theater.

## V0 “vertical slice” minimal checklist (spec-first)

The V0 must include (minimum):

- **Dataset/fixture**: a synthetic window fixture for hand-verifiable expected values.
- **Canonical domain model**: `Draw`, `Window`, normalization barrier.
- **Window resolution rules**: explicit, testable, no magic defaults.
- **One canonical metric**: simplest base metric (e.g., `frequencia_por_dezena@1.0.0`).
- **Two tools**:
  - `get_draw_window`
  - `compute_window_metrics`
- **Contract envelope**: `dataset_version`, `tool_version`, `deterministic_hash`.
- **Negative contract cases**: missing required fields, unknown metric, invalid window.

## What to do when starting a new project

If the user says “start a new project”, do:

1. **Write docs skeleton first** (minimal but coherent)
2. **Write V0 tests** (domain + contract) that fail for the right reason
3. **Implement only what’s needed**
4. **Keep outputs deterministic and hashable**

## Anti-patterns to reject

- “Implement V1” as a single step (too big; violates atomicity).
- Adding many projects/layers “because clean architecture” (structure theater).
- Inventing metric meaning from intuition (must be in catalog/ADR).
- Server inferring window defaults silently (“last N” without explicit request).
- Using predictive language (“increase chance”, “predict next draw”).

## If the user asks “Skill or not?”

Use this decision rule:

- Choose a **Rule** when: always-on writing norms and safety language (e.g., ADR 0021 for human summaries).
- Choose a **Skill** when: a repeatable **workflow** is needed (bootstrap, vertical-slice execution, contract+tests loops).
- Choose **both** when: you need permanent guardrails (Rules) + a step-by-step execution recipe (Skills).

