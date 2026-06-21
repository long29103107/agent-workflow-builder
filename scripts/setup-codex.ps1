[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $repoRoot ".codex/skills-manifest.json"
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$codexHome = if ($env:CODEX_HOME) { $env:CODEX_HOME } else { Join-Path $HOME ".codex" }
$skillsRoot = Join-Path $codexHome "skills"
$installer = Join-Path $skillsRoot ".system/skill-installer/scripts/install-skill-from-github.py"

if (-not (Test-Path -LiteralPath $installer)) {
    throw "Codex skill installer not found at '$installer'."
}

foreach ($skill in $manifest.skills) {
    $destination = Join-Path $skillsRoot $skill
    if (Test-Path -LiteralPath $destination) {
        if (-not $Force) {
            Write-Output "Skill '$skill' already installed; skipped."
            continue
        }

        $resolvedDestination = (Resolve-Path -LiteralPath $destination).Path
        $resolvedSkillsRoot = (Resolve-Path -LiteralPath $skillsRoot).Path
        if (-not $resolvedDestination.StartsWith($resolvedSkillsRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to replace skill outside '$resolvedSkillsRoot'."
        }
        Remove-Item -LiteralPath $resolvedDestination -Recurse -Force
    }

    & python $installer --repo $manifest.repository --ref $manifest.ref --path "skills/.curated/$skill"
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install skill '$skill'."
    }
}

Write-Output "Codex skill setup complete. Restart Codex to reload installed skills."
