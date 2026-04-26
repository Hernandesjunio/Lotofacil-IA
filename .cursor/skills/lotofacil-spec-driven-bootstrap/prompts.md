# Prompt Pack — Bootstrap & V0 (copy/paste)

Use these as “atomic” requests to an AI coding agent. Replace `<...>` with your repo paths.

## Bootstrap 0 — Freeze baseline (no feature)

```md
Implemente apenas um checkpoint de alinhamento normativo da base (sem codar feature), registrando conformidade com arquitetura, stack, determinismo e contrato.

Referências obrigatórias:
- docs/brief.md
- docs/spec-driven-execution-guide.md (Fase 0)
- docs/project-guide.md
- docs/mcp-tool-contract.md

Arquivos esperados:
- AGENTS.md (se necessário)
- docs/ (nota curta de alinhamento, se necessário)

Regras:
- não implementar novas funcionalidades nesta fase;
- apontar explicitamente qualquer contradição estrutural pendente.

Critério de pronto:
- estado normativo vigente está explícito e rastreável;
- nenhuma pendência estrutural crítica fica escondida.
```

## Bootstrap 1 — Docs skeleton (minimal, coherent)

```md
Implemente apenas o esqueleto mínimo de documentação do repositório para operar spec-driven (sem codar servidor): brief, guia de projeto, contrato MCP, catálogo de métricas (placeholders), plano de testes e uma definição de V0 (vertical slice).

Referências obrigatórias:
- docs/spec-driven-execution-guide.md (definição do fluxo)
- docs/project-guide.md (estrutura)

Arquivos esperados:
- docs/brief.md
- docs/project-guide.md
- docs/mcp-tool-contract.md
- docs/metric-catalog.md
- docs/test-plan.md
- docs/vertical-slice.md
- docs/contract-test-plan.md
- docs/spec-driven-execution-guide.md
- docs/fases-execucao-templates.md

Regras:
- manter linguagem descritiva (sem promessa de acerto);
- fechar invariantes de determinismo e ausência de defaults ocultos.

Critério de pronto:
- existe um “mapa de verdade” claro;
- existe uma V0 escrita que pode virar testes.
```

## V0 2 — Fixture + testes vermelhos (domínio + contrato)

```md
Implemente apenas a fixture mínima e os testes vermelhos da V0 (domínio + contrato) antes de qualquer implementação funcional.

Referências obrigatórias:
- docs/vertical-slice.md
- docs/contract-test-plan.md
- docs/test-plan.md
- docs/mcp-tool-contract.md
- docs/metric-catalog.md

Arquivos esperados:
- tests/fixtures/<synthetic_min_window>.json
- tests/<Domain.Tests>/
- tests/<ContractTests>/

Regras:
- TDD: testes devem falhar pelo motivo esperado;
- cobrir normalização canônica, resolução de janela, 1 métrica base e erros mínimos.

Critério de pronto:
- existe teste explícito da barreira de normalização;
- existe teste de fórmula para 1 métrica base;
- existem testes negativos mínimos (`metrics` ausente, métrica desconhecida).
```

## V0 3 — Implementar núcleo canônico mínimo (após testes)

```md
Implemente apenas o núcleo de domínio mínimo da V0: modelos canônicos, normalização, resolução de janela e 1 métrica base conforme catálogo, até os testes do domínio passarem.

Referências obrigatórias:
- docs/metric-catalog.md
- docs/spec-driven-execution-guide.md (Fase 3)
- docs/project-guide.md

Arquivos esperados:
- src/<Domain>/Models/
- src/<Domain>/Normalization/
- src/<Domain>/Windows/
- src/<Domain>/Metrics/

Regras:
- não mover códigos de contrato/transporte para o domínio;
- manter determinismo forte.

Critério de pronto:
- testes de normalização, janela e fórmula passam sem depender de HTTP/MCP.
```

