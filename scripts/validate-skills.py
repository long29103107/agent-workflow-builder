from __future__ import annotations

from pathlib import Path
import re
import sys

import yaml


ROOT = Path(__file__).resolve().parents[1]
SKILLS_ROOT = ROOT / ".codex" / "skills"
ALLOWED_FRONTMATTER = {"name", "description"}


def read_yaml_frontmatter(path: Path) -> dict[str, object]:
    text = path.read_text(encoding="utf-8")
    match = re.match(r"^---\r?\n(.*?)\r?\n---", text, re.DOTALL)
    if not match:
        raise ValueError("missing YAML frontmatter")
    value = yaml.safe_load(match.group(1))
    if not isinstance(value, dict):
        raise ValueError("frontmatter must be a mapping")
    return value


def validate_skill(skill_dir: Path) -> list[str]:
    errors: list[str] = []
    skill_file = skill_dir / "SKILL.md"
    try:
        frontmatter = read_yaml_frontmatter(skill_file)
    except (OSError, ValueError, yaml.YAMLError) as exc:
        return [f"{skill_file}: {exc}"]

    unexpected = set(frontmatter) - ALLOWED_FRONTMATTER
    if unexpected:
        errors.append(f"{skill_file}: unsupported frontmatter keys: {sorted(unexpected)}")
    if frontmatter.get("name") != skill_dir.name:
        errors.append(f"{skill_file}: name must match folder '{skill_dir.name}'")
    description = frontmatter.get("description")
    if not isinstance(description, str) or not description.strip():
        errors.append(f"{skill_file}: description is required")

    openai_file = skill_dir / "agents" / "openai.yaml"
    if not openai_file.exists():
        return errors + [f"{openai_file}: missing skill UI metadata"]
    try:
        metadata = yaml.safe_load(openai_file.read_text(encoding="utf-8"))
    except (OSError, yaml.YAMLError) as exc:
        return errors + [f"{openai_file}: {exc}"]

    interface = metadata.get("interface", {}) if isinstance(metadata, dict) else {}
    short_description = interface.get("short_description")
    if not isinstance(short_description, str) or not 25 <= len(short_description) <= 64:
        errors.append(f"{openai_file}: short_description must be 25-64 characters")
    default_prompt = interface.get("default_prompt")
    if not isinstance(default_prompt, str) or f"${skill_dir.name}" not in default_prompt:
        errors.append(f"{openai_file}: default_prompt must mention ${skill_dir.name}")
    return errors


def main() -> int:
    skill_dirs = sorted(path.parent for path in SKILLS_ROOT.glob("*/SKILL.md"))
    errors = [error for skill_dir in skill_dirs for error in validate_skill(skill_dir)]
    if errors:
        print("\n".join(errors), file=sys.stderr)
        return 1
    print(f"Repo-local skills valid: {len(skill_dirs)} skills.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
