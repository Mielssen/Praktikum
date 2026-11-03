# Запуск усіх комбінацій k6: навантаження (small/medium/large) x розмір обраного (10/100/1000).
# Приклад:
#   .\run_k6.ps1 -Email test@test.com -Password "Pass123" -FirstTourId 101
# Окремий запуск:
#   .\run_k6.ps1 -Email ... -Password ... -FirstTourId 101 -FavCounts 100 -Loads medium
param(
    [Parameter(Mandatory = $true)][string]$Email,
    [Parameter(Mandatory = $true)][string]$Password,
    [Parameter(Mandatory = $true)][int]$FirstTourId,
    [string]$BaseUrl = "http://localhost:5265",
    [int[]]$FavCounts = @(10, 100, 1000),
    [string[]]$Loads = @("small", "medium", "large")
)

Set-Location $PSScriptRoot
New-Item -ItemType Directory -Force -Path results | Out-Null

$lines = @()
$lines += "| Навантаження | VU | Обране | iter/s | GET avg / p95 / p99 (мс) | POST avg / p95 / p99 (мс) | DELETE avg / p95 / p99 (мс) | Помилки | Пороги |"
$lines += "|---|---|---|---|---|---|---|---|---|"

function Fmt($e) {
    if ($null -eq $e.avg) { return "—" }
    return "{0} / {1} / {2}" -f [math]::Round($e.avg, 1), [math]::Round($e.p95, 1), [math]::Round($e.p99, 1)
}

foreach ($fav in $FavCounts) {
    foreach ($load in $Loads) {
        Write-Host "=== LOAD=$load  FAV_COUNT=$fav ===" -ForegroundColor Cyan

        k6 run `
            -e BASE_URL=$BaseUrl `
            -e PERF_EMAIL=$Email `
            -e PERF_PASSWORD=$Password `
            -e FIRST_TOUR_ID=$FirstTourId `
            -e FAV_COUNT=$fav `
            -e LOAD=$load `
            k6/favorites_load.js
        $code = $LASTEXITCODE

        $file = "results/k6_${load}_fav${fav}.json"
        if (Test-Path $file) {
            $j = Get-Content $file -Raw | ConvertFrom-Json
            $err = if ($null -eq $j.failedRate) { "—" } else { "{0:P2}" -f $j.failedRate }
            $ok = if ($code -eq 0) { "✅" } else { "⚠️ (код $code)" }
            $lines += "| $load | $($j.vus) | $fav | {0:N1} | $(Fmt $j.endpoints.get) | $(Fmt $j.endpoints.post) | $(Fmt $j.endpoints.delete) | $err | $ok |" -f $j.iterationsPerSec
        }
        else {
            $lines += "| $load | — | $fav | — | — | — | — | — | ❌ тест не завершився |"
        }

        Start-Sleep -Seconds 5
    }
}

$lines | Set-Content results/k6_report.md -Encoding UTF8
Write-Host "`nГотово: results/k6_report.md" -ForegroundColor Green
