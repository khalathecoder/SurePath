# kill-dev.ps1
# Run this whenever VS throws "file is locked by SurePath.XYZ (PID)"
# Safe to run even if processes aren't running — errors are suppressed.

$processes = @(
    "SurePath.Core",
    "SurePath.Markets",
    "SurePath.Security",
    "SurePath.Commerce",
    "SurePath.API",
    "SurePath.Identity"
)

foreach ($name in $processes) {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($procs) {
        $procs | ForEach-Object {
            Write-Host "Killing $name (PID $($_.Id))..." -ForegroundColor Yellow
            Stop-Process -Id $_.Id -Force
        }
    }
}

Write-Host "`nDone. Safe to build now." -ForegroundColor Green
