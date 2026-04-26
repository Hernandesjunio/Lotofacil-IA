import csv
import json
import os
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_RESULTS_CSV = REPO_ROOT / "tests" / "resultados.csv"
FIXTURES_DIR = REPO_ROOT / "tests" / "fixtures"


def _sniff_delimiter(sample: str) -> str:
    # Deterministic allowlist: only these separators are accepted by spec.
    candidates = [",", ";", "\t"]
    counts = {d: sample.count(d) for d in candidates}
    best = max(counts.items(), key=lambda kv: kv[1])
    if best[1] == 0:
        raise ValueError("Could not detect delimiter (no , ; or tab found).")
    # If more than one delimiter appears in sample, it's ambiguous for our spec.
    nonzero = [d for d, c in counts.items() if c > 0]
    if len(nonzero) > 1:
        raise ValueError(f"Ambiguous delimiter in sample (found {nonzero}).")
    return best[0]


def load_winners_map(results_csv_path: Path) -> dict[int, int]:
    text = results_csv_path.read_text(encoding="utf-8")
    # Use first non-empty line as delimiter sniff base
    first_non_empty = next((ln for ln in text.splitlines() if ln.strip()), "")
    if not first_non_empty:
        raise ValueError("CSV file is empty.")

    delimiter = _sniff_delimiter(first_non_empty)

    # Header detection: first field numeric => no header (CEF downloads sometimes omit it)
    first_field = first_non_empty.split(delimiter, 1)[0].strip()
    has_header = not first_field.isdigit()

    winners_by_contest: dict[int, int] = {}

    reader = csv.reader(text.splitlines(), delimiter=delimiter)
    if has_header:
        header = next(reader)
        normalized = [h.strip().lower() for h in header]
        try:
            contest_idx = normalized.index("concurso")
        except ValueError:
            try:
                contest_idx = normalized.index("contest_id")
            except ValueError as e:
                raise ValueError("CSV header missing 'Concurso'/'contest_id'.") from e

        # Support both CEF label and canonical label.
        winner_labels = {"ganhadores 15 acertos", "winners_15"}
        winner_idx = None
        for i, h in enumerate(normalized):
            if h in winner_labels:
                winner_idx = i
                break
        if winner_idx is None:
            raise ValueError(
                "CSV header does not include winners column "
                "('Ganhadores 15 acertos' or 'winners_15')."
            )

        for row in reader:
            if not row or all(not c.strip() for c in row):
                continue
            contest_id = int(row[contest_idx].strip())
            winners_15 = int(row[winner_idx].strip())
            winners_by_contest[contest_id] = winners_15
        return winners_by_contest

    # No header => positional. We only proceed if winners_15 column exists (18 columns).
    # Expected positional (no header):
    # 0 contest_id, 1 draw_date, 2..16 balls, 17 winners_15
    for row in reader:
        if not row or all(not c.strip() for c in row):
            continue
        if len(row) == 17:
            # contest_id + draw_date + 15 balls only => no winners info in this file.
            raise ValueError(
                f"{results_csv_path} appears to have 17 columns (no winners_15). "
                "Provide a CSV that includes winners_15 as the 18th column, or a header with "
                "'Ganhadores 15 acertos'."
            )
        if len(row) != 18:
            raise ValueError(
                f"Unexpected column count {len(row)} in positional CSV row; expected 18."
            )
        contest_id = int(row[0].strip())
        winners_15 = int(row[17].strip())
        winners_by_contest[contest_id] = winners_15

    return winners_by_contest


def update_fixture_file(path: Path, winners_by_contest: dict[int, int]) -> bool:
    data = json.loads(path.read_text(encoding="utf-8"))

    draws = data.get("draws")
    if not isinstance(draws, list):
        return False

    changed = False
    for d in draws:
        if not isinstance(d, dict):
            continue
        if "contest_id" not in d:
            continue
        contest_id = int(d["contest_id"])
        # Some fixtures are synthetic (e.g. tie-heavy) and may use contest ids outside
        # the real dataset. In that case, default to 0 winners to preserve invariants
        # without changing draws/numbers.
        winners_15 = int(winners_by_contest.get(contest_id, 0))
        has_winner_15 = winners_15 > 0
        if d.get("winners_15") != winners_15:
            d["winners_15"] = winners_15
            changed = True
        if d.get("has_winner_15") != has_winner_15:
            d["has_winner_15"] = has_winner_15
            changed = True

    if changed:
        path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return changed


def main() -> int:
    results_csv = Path(os.environ.get("RESULTS_CSV", DEFAULT_RESULTS_CSV))
    winners_by_contest = load_winners_map(results_csv)

    updated = 0
    scanned = 0
    for p in sorted(FIXTURES_DIR.rglob("*.json")):
        scanned += 1
        try:
            did = update_fixture_file(p, winners_by_contest)
        except json.JSONDecodeError:
            continue
        if did:
            updated += 1

    print(f"Scanned {scanned} json files under {FIXTURES_DIR}")
    print(f"Updated {updated} fixture file(s) with winners_15/has_winner_15")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

