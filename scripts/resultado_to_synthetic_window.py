#!/usr/bin/env python3
"""
Converte linhas no formato CEF (TSV):
  concurso \\t DD/MM/AAAA \\t 15 dezenas
para JSON no esquema de tests/fixtures/synthetic_min_window.json.

Exemplo de linha:
  1\\t29/09/2003\\t2\\t3\\t5\\t6\\t9\\t10\\t11\\t13\\t14\\t16\\t18\\t20\\t23\\t24\\t25

Uso:
  python scripts/resultado_to_synthetic_window.py tests/fixtures/resultado.txt -o tests/fixtures/synthetic_min_window.json
  type resultado.txt | python scripts/resultado_to_synthetic_window.py -o out.json
"""

from __future__ import annotations

import argparse
import json
import sys
from datetime import datetime
from pathlib import Path


def parse_line(line: str) -> dict | None:
    line = line.strip()
    if not line or line.startswith("#"):
        return None

    parts = line.split("\t")
    if len(parts) < 17:
        parts = line.split()
    if len(parts) < 17:
        raise ValueError(f"Esperadas 17 colunas (concurso, data, 15 dezenas), obtido {len(parts)}: {line!r}")

    contest_id = int(parts[0])
    date_str = parts[1]
    dt = datetime.strptime(date_str.strip(), "%d/%m/%Y")
    draw_date = dt.strftime("%Y-%m-%d")
    numbers = sorted(int(x) for x in parts[2:17])

    return {
        "contest_id": contest_id,
        "draw_date": draw_date,
        "numbers": numbers,
    }


def load_draws(text: str) -> list[dict]:
    draws: list[dict] = []
    for i, line in enumerate(text.splitlines(), start=1):
        try:
            row = parse_line(line)
        except ValueError as e:
            raise SystemExit(f"Linha {i}: {e}") from e
        if row is not None:
            draws.append(row)
    return draws


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "input",
        nargs="?",
        type=Path,
        help="Arquivo de entrada (stdin se omitido)",
    )
    parser.add_argument(
        "-o",
        "--output",
        type=Path,
        help="Arquivo JSON de saída (stdout se omitido)",
    )
    parser.add_argument(
        "--indent",
        type=int,
        default=2,
        help="Indentação JSON (padrão: 2; use 0 para compacto)",
    )
    args = parser.parse_args()

    if args.input is None:
        text = sys.stdin.read()
    else:
        text = args.input.read_text(encoding="utf-8")

    payload = {"draws": load_draws(text)}
    if args.indent > 0:
        out = json.dumps(payload, ensure_ascii=False, indent=args.indent)
    else:
        out = json.dumps(payload, ensure_ascii=False, separators=(",", ":"))

    if args.output is None:
        sys.stdout.write(out + "\n")
    else:
        args.output.write_text(out + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
