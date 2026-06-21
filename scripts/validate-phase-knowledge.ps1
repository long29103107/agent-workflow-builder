[CmdletBinding()]
param(
    [string]$Root = "docs/knowledge/phases"
)

$ErrorActionPreference = "Stop"
$rootPath = (Resolve-Path -LiteralPath $Root).Path
$errors = [Collections.Generic.List[string]]::new()
$allowedStatuses = @("planned", "in progress", "blocked", "done")
$taskRecords = @{}
$phaseResults = @{}

function Read-Frontmatter([string]$Path) {
    $content = Get-Content -LiteralPath $Path -Raw
    $match = [regex]::Match($content, "(?s)^---\r?\n(?<yaml>.*?)\r?\n---")
    $values = @{}
    if (-not $match.Success) { return $values }
    foreach ($line in $match.Groups["yaml"].Value -split "\r?\n") {
        if ($line -match "^(?<key>[a-z_]+):\s*(?<value>.*)$") {
            $values[$Matches["key"]] = $Matches["value"].Trim().Trim('"')
        }
    }
    return $values
}

function Normalize-Status([string]$Status) {
    if ($null -eq $Status) { return "" }
    return $Status.Trim().ToLowerInvariant().Replace("_", " ")
}

function Read-Dependencies([string]$Value) {
    if ([string]::IsNullOrWhiteSpace($Value) -or $Value -eq "none") { return @() }
    return @($Value -split "," | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}

$indexPath = Join-Path $rootPath "README.md"
$indexMeta = Read-Frontmatter $indexPath
$activePhase = $indexMeta["active_phase"]
$nextTaskOverride = $indexMeta["next_task_override"]
if ([string]::IsNullOrWhiteSpace($activePhase)) {
    $errors.Add("Phase index is missing active_phase metadata.")
}

$phaseFolders = @(Get-ChildItem -LiteralPath $rootPath -Directory | Sort-Object Name)
foreach ($phaseFolder in $phaseFolders) {
    $summaryPath = Join-Path $phaseFolder.FullName "PHASE_SUMMARY.md"
    if (-not (Test-Path -LiteralPath $summaryPath)) {
        $errors.Add("Missing PHASE_SUMMARY.md in $($phaseFolder.Name).")
        continue
    }

    $summaryMeta = Read-Frontmatter $summaryPath
    $summaryStatus = Normalize-Status $summaryMeta["status"]
    $summaryContent = Get-Content -LiteralPath $summaryPath -Raw
    $summaryRows = @([regex]::Matches($summaryContent, '(?m)^\| \[(?<id>\d{3}_\d{3})\]\(\./(?<file>[^)]+\.md)\) \| .*? \| `(?<status>[^`]+)` \| (?<done>\d+)/(?<total>\d+) \|\r?$'))
    $taskFiles = @(Get-ChildItem -LiteralPath $phaseFolder.FullName -File | Where-Object Name -NE "PHASE_SUMMARY.md" | Sort-Object Name)

    if ($summaryRows.Count -ne $taskFiles.Count) {
        $errors.Add("$($phaseFolder.Name) summary has $($summaryRows.Count) rows for $($taskFiles.Count) task files.")
    }

    $doneTasks = 0
    foreach ($taskFile in $taskFiles) {
        $meta = Read-Frontmatter $taskFile.FullName
        $taskId = $meta["task_id"]
        $taskStatus = Normalize-Status $meta["status"]
        $schemaVersion = $meta["schema_version"]
        if ([string]::IsNullOrWhiteSpace($taskId) -or $taskFile.BaseName -notmatch "^$([regex]::Escape($taskId))-" ) {
            $errors.Add("Task metadata/file mismatch: $($taskFile.FullName).")
            continue
        }
        if ($taskRecords.ContainsKey($taskId)) {
            $errors.Add("Duplicate task ID: $taskId.")
            continue
        }
        if ($allowedStatuses -notcontains $taskStatus) {
            $errors.Add("Task $taskId has unsupported status '$taskStatus'.")
        }

        $taskContent = Get-Content -LiteralPath $taskFile.FullName -Raw
        if ($schemaVersion -eq "1") {
            if ($meta["updated_at"] -ne "unknown" -or $meta["verification"] -ne "not_recorded" -or $meta["outcome"] -ne "migrated_from_legacy_phase_status") {
                $errors.Add("Legacy task $taskId must use truthful schema version 1 migration metadata.")
            }
        } elseif ($schemaVersion -eq "2") {
            if ([string]::IsNullOrWhiteSpace($meta["updated_at"]) -or $meta["updated_at"] -eq "unknown") {
                $errors.Add("Schema version 2 task $taskId requires updated_at.")
            }
            if ($taskContent -notmatch '(?m)^## Verification\s*$' -or $taskContent -notmatch '(?m)^## Outcome\s*$') {
                $errors.Add("Schema version 2 task $taskId requires Verification and Outcome sections.")
            }
            if ($taskStatus -eq "done" -and ($taskContent -match '(?s)## Verification\s+Pending\.' -or $taskContent -match '(?s)## Outcome\s+Pending\.')) {
                $errors.Add("Done schema version 2 task $taskId still has pending verification or outcome.")
            }
        } else {
            $errors.Add("Task $taskId has unsupported or missing schema_version '$schemaVersion'.")
        }

        $dependencies = Read-Dependencies $meta["depends_on"]
        $taskRecords[$taskId] = @{
            Id = $taskId
            Status = $taskStatus
            Phase = $phaseFolder.Name
            Dependencies = $dependencies
        }

        $checked = [regex]::Matches($taskContent, "(?m)^- \[x\] ", "IgnoreCase").Count
        $unchecked = [regex]::Matches($taskContent, "(?m)^- \[ \] ").Count
        $total = $checked + $unchecked
        $summaryRow = @($summaryRows | Where-Object { $_.Groups["id"].Value -eq $taskId })
        if ($summaryRow.Count -ne 1) {
            $errors.Add("Task $taskId must have exactly one summary row.")
            continue
        }
        if (-not (Test-Path -LiteralPath (Join-Path $phaseFolder.FullName $summaryRow[0].Groups["file"].Value))) {
            $errors.Add("Broken summary link for task $taskId.")
        }
        if ((Normalize-Status $summaryRow[0].Groups["status"].Value) -ne $taskStatus) {
            $errors.Add("Status mismatch for task $taskId.")
        }
        if ([int]$summaryRow[0].Groups["done"].Value -ne $checked -or [int]$summaryRow[0].Groups["total"].Value -ne $total) {
            $errors.Add("Checklist count mismatch for task $taskId.")
        }
        if ($taskStatus -eq "done") {
            $doneTasks++
            if ($unchecked -gt 0) { $errors.Add("Done task $taskId still has unchecked items.") }
        }
    }

    $hasStartedTask = @($taskRecords.Values | Where-Object { $_.Phase -eq $phaseFolder.Name -and $_.Status -in @("in progress", "blocked", "done") }).Count -gt 0
    $expectedPhaseStatus = if ($doneTasks -eq $taskFiles.Count) { "done" } elseif ($hasStartedTask) { "in progress" } else { "planned" }
    if ($summaryStatus -ne $expectedPhaseStatus) {
        $errors.Add("Phase $($phaseFolder.Name) status is '$summaryStatus', expected '$expectedPhaseStatus'.")
    }
    if ($summaryContent -notmatch "Progress: ``$doneTasks/$($taskFiles.Count)`` tasks done\.") {
        $errors.Add("Phase $($phaseFolder.Name) progress count is stale.")
    }
    $phaseResults[$phaseFolder.Name] = @{ Status = $expectedPhaseStatus; Done = $doneTasks; Total = $taskFiles.Count }
}

foreach ($task in $taskRecords.Values) {
    foreach ($dependency in $task.Dependencies) {
        if (-not $taskRecords.ContainsKey($dependency)) {
            $errors.Add("Task $($task.Id) depends on unknown task '$dependency'.")
        }
    }
}

$indexContent = Get-Content -LiteralPath $indexPath -Raw
$indexRows = @([regex]::Matches($indexContent, '(?m)^\| \[(?<phase>\d{3}-[^]]+)\]\(\./[^)]+/PHASE_SUMMARY\.md\) \| .*? \| `(?<status>[^`]+)` \| (?<done>\d+)/(?<total>\d+) \|\r?$'))
if ($indexRows.Count -ne $phaseFolders.Count) {
    $errors.Add("Phase index has $($indexRows.Count) rows for $($phaseFolders.Count) phase folders.")
}
foreach ($row in $indexRows) {
    $phaseName = $row.Groups["phase"].Value
    if (-not $phaseResults.ContainsKey($phaseName)) {
        $errors.Add("Phase index references unknown phase '$phaseName'.")
        continue
    }
    $result = $phaseResults[$phaseName]
    if ((Normalize-Status $row.Groups["status"].Value) -ne $result.Status -or [int]$row.Groups["done"].Value -ne $result.Done -or [int]$row.Groups["total"].Value -ne $result.Total) {
        $errors.Add("Phase index progress is stale for '$phaseName'.")
    }
}

if ($activePhase -and -not $phaseResults.ContainsKey($activePhase)) {
    $errors.Add("active_phase '$activePhase' does not exist.")
}

$eligibleTasks = @($taskRecords.Values | Where-Object {
    $_.Phase -eq $activePhase -and $_.Status -in @("in progress", "planned") -and
    @($_.Dependencies | Where-Object { -not $taskRecords.ContainsKey($_) -or $taskRecords[$_].Status -ne "done" }).Count -eq 0
} | Sort-Object @{ Expression = { if ($_.Status -eq "in progress") { 0 } else { 1 } } }, @{ Expression = { $_.Id } })
$derivedNextTask = if ($eligibleTasks.Count -gt 0) { $eligibleTasks[0].Id } else { $null }

if ($nextTaskOverride) {
    if (-not $taskRecords.ContainsKey($nextTaskOverride)) {
        $errors.Add("next_task_override '$nextTaskOverride' does not exist.")
    } elseif (@($eligibleTasks | Where-Object Id -EQ $nextTaskOverride).Count -ne 1) {
        $errors.Add("next_task_override '$nextTaskOverride' is not eligible in active_phase '$activePhase'.")
    } else {
        $derivedNextTask = $nextTaskOverride
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

$nextDisplay = if ($derivedNextTask) { $derivedNextTask } else { "none" }
Write-Output "Phase knowledge valid: $($phaseFolders.Count) phases, $($taskRecords.Count) tasks, active=$activePhase, next=$nextDisplay."
