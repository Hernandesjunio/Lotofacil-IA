---
name: lotofacil-spec-driven-bootstrap
description: Bootstraps a new spec-driven MCP/HTTP .NET project using the Lotofacil-IA methodology (docs-first, deterministic metrics, MCP contract, TDD, vertical slices). Use when starting a new project/repo, creating the initial docs skeleton, defining a V0 vertical slice, or setting up a repeatable AI-driven workflow.
---

# Lotofacil Spec-Driven Bootstrap

## Goal

Provide a **repeatable AI-driven development workflow** for a new project that follows the Lotofacil-IA methodology:

- **Spec-first**: semantics start in `docs/`
- **TDD / contract-first**: tests prove each recorte
- **Determinism**: same canonical input ⇒ same canonical output (except generation without `seed`)
- **No hidden defaults on server**: client/host must be explicit per contract
- **Small vertical slices**: V0 first, then expand one tool/metric at a time

## When to use

Use this skill when the user asks for any of the following:

- Start a new repository “like Lotofacil-IA”
- Create a “boilerplate” to run a spec-driven framework with AI
- Set up docs + test plan + execution guide + templates for atomic steps
- Plan the first vertical slice (V0) and enforce spec → test → code

## What to produce (outputs)

Produce one of these, depending on the request:

1. **Project kit** (recommended default):
   - Minimal `docs/` skeleton (brief, contract, catalog placeholders, test-plan)
   - A `spec-driven-execution-guide.md` adapted to the repo
   - `fases-execucao-templates.md` with atomic “Implemente apenas…” templates
2. **V0 plan**:
   - The exact next atomic step to execute, with references + expected files + done criteria
3. **Guardrails**:
   - A short “non-goals / prohibited behaviors” section (no prediction, no hidden defaults, etc.)

## Operating rules (non-negotiable)

- **Always anchor to specs**: cite which `docs/*` governs the change.
- **Do not invent formulas or metric semantics** outside `metric-catalog.md` / glossary / ADRs.
- **Prefer 1 atomic recorte per PR**: one goal, one test proof, minimal files.
- **If semantics change**: update **docs + tests + code** together.
- **If a choice is ambiguous**: ask contract-aligned questions in the host/client flow; server does not guess.

## Default source-of-truth order (for new projects)

Use this order as the project’s “map” (copy/adapt to your repo):

- `docs/brief.md`
- `docs/vertical-slice.md` (V0)
- `docs/mcp-tool-contract.md`
- `docs/metric-catalog.md`
- `docs/contract-test-plan.md`
- `docs/spec-driven-execution-guide.md`
- `docs/fases-execucao-templates.md`
- `docs/test-plan.md`
- `docs/project-guide.md`
- `docs/metric-glossary.md` (human wording + ADR 0021 alignment)
- `docs/adrs/*` (structural decisions)

## Workflow (bootstrap)

1. **Freeze baseline**: confirm stack, architecture boundaries, deterministic policy.
2. **Draft minimal docs skeleton**: brief + contract + catalog + test plan + vertical slice.
3. **Define V0**: one fixture, one canonical model, one metric, two tools.
4. **Write tests first**: domain formula + contract envelopes + negative cases.
5. **Implement minimal code**: only to pass the tests, respecting boundaries.
6. **Record evidence**: run suites, capture deterministic outputs/goldens where applicable.

## Quick template: atomic request (copy/paste)

Use this exact structure for requests to the agent:

```md
Implemente apenas <passo único>.

Referências obrigatórias:
- <spec 1>
- <spec 2>

Arquivos esperados:
- <arquivo A>
- <arquivo B>

Regras:
- não extrapolar além do recorte citado;
- manter TDD;
- respeitar fronteiras de arquitetura;
- seguir nomes canônicos do catálogo/contrato;
- sem defaults ocultos no servidor.

Critério de pronto:
- <teste X passa>
- <erro Y é emitido>
- <payload Z contém campos obrigatórios>
```

## Additional resources

- Detailed guidance and checklists: see [reference.md](reference.md)
- Copy-ready atomic prompts by phase: see [prompts.md](prompts.md)
