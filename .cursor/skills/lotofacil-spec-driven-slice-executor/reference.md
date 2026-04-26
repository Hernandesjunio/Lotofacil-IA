# Reference — Slice Executor

## “Recorte” definition (must be explicit)

A valid slice has:

- **Single objective** (one metric, one tool, one contract behavior, one drift fix)
- **Named spec references** (docs paths + ADR when applicable)
- **Proof** (tests that fail before implementation, pass after)
- **Done criteria** (observable, not subjective)

If any of those is missing, the request is not ready; rewrite it using the atomic template.

## Choose the slice type

### A) New metric (or metric change)

Use when implementing a metric from `docs/metric-catalog.md`.

Checklist:

- Confirm **metric name + version + scope + shape + unit** exist in the catalog.
- Write a **formula test** against a small fixture.
- Add at least one **property test** (sum invariants, bounds, finitude, tie-break stability).
- If exposed via `compute_window_metrics`, add/adjust **contract tests** for `MetricValue`.

### B) New tool / tool contract change

Use when adding/changing a tool described in `docs/mcp-tool-contract.md`.

Checklist:

- Update the contract doc first (schemas + errors + invariants).
- Write contract tests:
  - success case(s)
  - negative case(s) with correct error codes and `details`
  - determinism of payload (and hash where required)
- Implement minimal server binding + validation + mapping to use cases.

### C) Drift fix (spec ↔ code)

Use when behavior exists but conflicts with docs.

Checklist:

- Classify drift: semantic vs transport/surface vs structural vs evidence.
- Choose: reconverge code to spec **or** revise spec (never implicitly both).
- Add regression tests to prevent drift recurrence.

## Determinism gates (common)

For deterministic tools:

- Same canonical request run multiple times ⇒ same JSON + same `deterministic_hash`.
- Tie-break rules must be explicit and stable.

For generation:

- With `seed` present: replay must hold for the same request.
- Without `seed`: results may vary, but must remain explainable and properly labeled.

