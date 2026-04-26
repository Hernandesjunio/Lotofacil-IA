# ADR 0022 — Fonte de dados configurável (`Dataset:DrawsSourceUri`) + metadados de ganhadores (15 acertos) por concurso

**Navegação:** [← Brief (índice)](../brief.md) · [Contrato MCP](../mcp-tool-contract.md) · [Fatia V0](../vertical-slice.md) · [Guia spec-driven](../spec-driven-execution-guide.md)

## Status

Proposto.

**Data:** 2026-04-25

## Contexto

O repositório começou com fixtures em `tests/fixtures/` para fechar a V0 (dado bruto → `Draw` canônico → janela → 1 métrica). Para evoluir o sistema, precisamos:

1. Permitir que a origem dos concursos seja configurável, podendo ser:
   - arquivo local (CSV ou JSON);
   - URL HTTP/HTTPS (CSV ou JSON).
2. Manter as invariantes centrais do projeto:
   - **stateless por request**;
   - **sem defaults semânticos ocultos** (o servidor não “adivinha” fonte nem janela);
   - **rastreabilidade e determinismo** via `dataset_version`, `tool_version`, `deterministic_hash` (ver [mcp-tool-contract.md](../mcp-tool-contract.md)).
3. Incorporar um insight funcional: para cada concurso, é útil retornar se houve ganhador de **15 acertos**, e quantos, para suportar consultas do tipo:
   - “últimos concursos em que houve ganhadores”;
   - “últimos concursos em que **não** houve ganhadores”;
   - “últimos concursos em que houve exatamente 4 ganhadores”.

Essa informação é **factual do concurso histórico**, não uma métrica derivada de janela; portanto deve viver como **metadado do `Draw`** (e não como `MetricValue`).

## Decisão

### D1 — Configuração única para a fonte de dados

Introduzir a configuração obrigatória:

- `Dataset:DrawsSourceUri` (via env var `Dataset__DrawsSourceUri` em .NET)

Semântica: string que identifica a origem do dataset de concursos, em um dos formatos abaixo:

- **Arquivo local (caminho absoluto Windows)**: `C:\dados\lotofacil.csv` / `C:\dados\lotofacil.json`
- **URI `file://`**: `file:///C:/dados/lotofacil.csv` / `file:///C:/dados/lotofacil.json`
- **URL HTTP/HTTPS**: `https://exemplo.com/lotofacil.csv`

O formato (CSV vs JSON) é determinado por:

- extensão do path (`.csv` / `.json`), e/ou
- `Content-Type` em HTTP (quando disponível).

Ambiguidade não resolvida (ex.: sem extensão e `Content-Type` genérico) deve falhar com `DATASET_UNAVAILABLE` (ver D4).

#### Resolução de caminho/URI (normativa; sem “busca implícita”)

Para evitar ambiguidades e comportamento dependente do diretório atual, a resolução deve ser determinística:

- Se `Dataset:DrawsSourceUri` for **URI `file://`**, o servidor deve convertê-la para um **path local** (Windows) e usar esse path.
- Se for um **path absoluto**, usar diretamente.
- Se for um **path relativo**, resolver **apenas** contra a raiz de conteúdo do servidor (ex.: `ContentRootPath`) — sem subir diretórios em busca de “um arquivo que exista”.
- Se, após resolução, o arquivo não existir, falhar com `DATASET_UNAVAILABLE` (`reason="unreachable"`), incluindo `source` e `examples` em `details`.

#### Obrigatoriedade (sem fallback)

Esta configuração é **obrigatória** e **não deve ter fallback silencioso** (ex.: “usar fixture default” quando ausente).

- Se `Dataset__DrawsSourceUri` não for fornecida no ambiente/configuração do processo, a build deve falhar de forma explícita (ver D4), sem tentar “adivinhar” um arquivo local padrão.
- Isso vale mesmo em V0/fixture: a fixture continua existindo como **arquivo**, mas o caminho/URI para ela deve ser informado via `Dataset:DrawsSourceUri` para remover ambiguidade operacional entre docs e runtime.

### D2 — Modelo canônico permanece `Draw` (ingestão é detalhe)

Independente do formato de entrada, o servidor materializa um dataset canônico composto por `Draw`:

- `contest_id` (int, chave única)
- `draw_date` (data ISO-8601)
- `numbers` (15 dezenas distintas em `[1..25]`)
- `winners_15` (int 0..99999) — **obrigatório no dataset**
- `has_winner_15` (bool) — **derivado, sempre coerente com `winners_15`**

Invariantes:

- `has_winner_15 == (winners_15 > 0)`
- `winners_15 = 0` ⇒ `has_winner_15 = false`
- `winners_15 ≥ 1` ⇒ `has_winner_15 = true`

Se a entrada trouxer ambos os campos e eles forem inconsistentes, é erro de dados (ver D4).

### D3 — Schema de importação (CSV e JSON)

#### CSV (recomendado para ingestão e alinhamento com CEF)

Regras:

- encoding: UTF-8
- separador: um dentre `,` (vírgula), `;` (ponto e vírgula) ou tab (`\t`).
- header: **opcional** (o servidor deve aceitar com ou sem título).

Detecção do separador (normativa):

- O servidor deve detectar o separador a partir da **primeira linha do arquivo** (header quando existir; caso contrário, primeira linha de dados), aceitando apenas os três valores acima.
- Se houver ambiguidade (ex.: linha sem nenhum dos separadores aceitos, ou mistura), falhar com `DATASET_UNAVAILABLE` (`reason="invalid_format"`).

Detecção de header (normativa):

- Se a primeira linha contiver claramente os rótulos CEF (abaixo) ou os rótulos canônicos (`contest_id`, `draw_date`, `b1..b15`, `winners_15`), ela é tratada como header.
- Caso contrário, a primeira linha é tratada como dados e o parsing assume **schema posicional** (ver “Sem header (posicional)” abaixo).

Header aceito — padrão CEF (colunas com espaços):

- `Concurso`
- `Data Sorteio`
- `Bola1`..`Bola15`
- `Ganhadores 15 acertos`

Header aceito — padrão canônico (sem espaços):

- `contest_id`
- `draw_date`
- `b1`..`b15`
- `winners_15`

Mapeamento (header → canônico):

- `Concurso` → `contest_id`
- `Data Sorteio` → `draw_date`
- `BolaN` → `bN` (N=1..15)
- `Ganhadores 15 acertos` → `winners_15`

Colunas **obrigatórias**:

- `contest_id` (int)
- `draw_date` (`YYYY-MM-DD`)
- `b1`..`b15` (int 1..25)
- `winners_15` (int 0..99999)

Observações:

- A ordem dos `b1..b15` não precisa estar crescente; o servidor canoniza `numbers` em ordem crescente.
- `has_winner_15` não precisa existir no CSV (é derivado).

Sem header (posicional):

- Quando não houver header, o servidor deve aceitar exatamente 18 colunas, nesta ordem:
  1) `contest_id`
  2) `draw_date`
  3..17) `b1..b15`
  18) `winners_15`

Se o número de colunas não bater com o esperado, falhar com `DATASET_UNAVAILABLE` (`reason="invalid_format"`).

#### JSON (preferido para fixtures e testes)

Forma canônica (arquivo com objeto e lista `draws`):

```json
{
  "draws": [
    {
      "contest_id": 1001,
      "draw_date": "2024-01-01",
      "numbers": [1,2,3,6,7,8,11,12,13,16,17,18,21,22,23],
      "winners_15": 0,
      "has_winner_15": false
    }
  ]
}
```

Regras:

- `winners_15` é obrigatório.
- `has_winner_15` pode estar presente por redundância, mas deve ser validado contra `winners_15`.

### D4 — Falhas e mensagens canônicas (ENV ausente / fonte inválida)

Quando `Dataset__DrawsSourceUri` estiver ausente, inválida, inacessível, ou o dataset falhar validação (schema/duplicatas/invariantes), as tools dependentes do dataset devem falhar com:

- `DATASET_UNAVAILABLE`

Com `details` estruturado o suficiente para o host/agente guiar o usuário, sem inferência silenciosa, por exemplo:

- `missing_env: "Dataset__DrawsSourceUri"` (quando ausente)
- `source: "<valor atual>"` (quando presente mas falha)
- `accepted_schemes: ["file", "http", "https"]`
- `accepted_formats: ["csv", "json"]`
- `examples: [...]` (mínimo 2 exemplos)
- `reason: "missing_env" | "unreachable" | "invalid_format" | "invalid_data"`

#### Nota de transição (V0 → leitura por URI)

Enquanto a implementação ainda estiver baseada em fixture JSON local, `Dataset:DrawsSourceUri` pode apontar para o arquivo de fixture via:

- path relativo ao repositório (resolvido pelo servidor de forma determinística contra sua raiz de conteúdo), ex.: `tests/fixtures/synthetic_min_window.json`
- ou URI `file://`, ex.: `file:///C:/_projeto/Lotofacil-IA/tests/fixtures/synthetic_min_window.json`

O requisito normativo aqui é **unificar a chave** e permitir `file://`; a expansão para HTTP/CSV deve manter o mesmo contrato de erro e rastreabilidade.

### D5 — Winners como metadado do `Draw` + filtro explícito em `get_draw_window`

`winners_15` e `has_winner_15` são metadados do concurso, e devem aparecer no output de `get_draw_window` quando explicitamente solicitados (evitar payload mais “pesado” por padrão):

- `include_winners_15` (bool) no request, default `false`.

Além disso, `get_draw_window` pode suportar filtros explícitos por winners para consultas do tipo “últimos N concursos com/sem ganhador” ou “com exatamente X ganhadores”, sem criar tool nova:

- `draw_filters.has_winner_15` (bool)
- `draw_filters.winners_15` com predicados `{eq, min, max}`

Semântica de `window_size` quando filtros são usados:

- `window_size` passa a representar **quantos concursos retornar após aplicar filtros** (top‑N filtrado), sempre em ordem canônica por `contest_id`.
- Se não houver concursos suficientes após filtrar: `INSUFFICIENT_HISTORY` com `details.requested` e `details.available_after_filter`.

## Consequências

### Positivas

- Sai do “fixture hard-coded” sem perder rastreabilidade e determinismo.
- CSV vira o formato prático para pipeline real; JSON continua ótimo para testes.
- Metadados de winners habilitam consultas e UX sem reabrir o catálogo de métricas.

### Custos / trade-offs

- Fonte HTTP precisa ser tratada como **snapshot versionado**: `dataset_version` deve refletir o conteúdo efetivo lido (para auditoria e cache).
- O contrato precisa explicitar claramente o erro `DATASET_UNAVAILABLE` com `details` ricos, para evitar que o host tente “adivinhar” fonte.
- `get_draw_window` com filtros altera a leitura ingênua de `window_size`; isso precisa estar normatizado no contrato para não surpreender consumidores.

## Relações

- `Draw` e invariantes de janela/erro: [mcp-tool-contract.md](../mcp-tool-contract.md)
- V0 e evolução de providers/infra: [vertical-slice.md](../vertical-slice.md), [spec-driven-execution-guide.md](../spec-driven-execution-guide.md)
- “Sem defaults ocultos” e determinismo: [ADR 0001](0001-fechamento-semantico-e-determinismo-v1.md)
- Help/resources (mensagens de onboarding e “se der erro”): [ADR 0009](0009-help-e-catalogo-de-templates-resources-v1.md)

---

*Última intenção deste ADR:* permitir fonte de dados configurável (local/HTTP, CSV/JSON) preservando determinismo e rastreabilidade, e incorporar metadados de winners (15 acertos) como fato histórico do `Draw`, com filtro explícito em `get_draw_window`, sem transformar isso em métrica.

