param(
    [string]$ApiUrl = "http://localhost:5000/api/tickets",
    [int]$TotalRequests = 50
)

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " CivicConnect M3 Performance & Load Test" -ForegroundColor Cyan
Write-Host " Target Operation : GET $ApiUrl"
Write-Host " Workload         : $TotalRequests sequential/burst requests"
Write-Host " Environment      : Simulated Staging (Local SQLite)"
Write-Host "====================================================`n" -ForegroundColor Cyan

$responseTimes = @()
$successCount = 0
$errorCount = 0
$suiteTimer = [System.Diagnostics.Stopwatch]::StartNew()

for ($i = 1; $i -le $TotalRequests; $i++) {
    $reqTimer = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri $ApiUrl -Method Get -UseBasicParsing -TimeoutSec 10
        $reqTimer.Stop()
        if ($response.StatusCode -eq 200) {
            $successCount++
            $responseTimes += $reqTimer.Elapsed.TotalMilliseconds
        } else {
            $errorCount++
        }
    } catch {
        $reqTimer.Stop()
        $errorCount++
    }
}

$suiteTimer.Stop()

if ($responseTimes.Count -gt 0) {
    $sorted = $responseTimes | Sort-Object
    $avg = ($responseTimes | Measure-Object -Average).Average
    $min = $sorted[0]
    $max = $sorted[-1]
    $p95Index = [Math]::Floor($sorted.Count * 0.95) - 1
    if ($p95Index -lt 0) { $p95Index = 0 }
    $p95 = $sorted[$p95Index]
    $throughput = $TotalRequests / $suiteTimer.Elapsed.TotalSeconds

    Write-Host "Test Execution Complete!" -ForegroundColor Green
    Write-Host "----------------------------------------------------"
    Write-Host ("Total Duration    : {0:N2} seconds" -f $suiteTimer.Elapsed.TotalSeconds)
    Write-Host ("Successful (200)  : $successCount / $TotalRequests ({0:N1}%)" -f (($successCount / $TotalRequests) * 100))
    Write-Host ("Failed Errors     : $errorCount")
    Write-Host ("Throughput        : {0:N2} requests/sec" -f $throughput)
    Write-Host ("Avg Response Time : {0:N2} ms" -f $avg)
    Write-Host ("Min Response Time : {0:N2} ms" -f $min)
    Write-Host ("Max Response Time : {0:N2} ms" -f $max)
    Write-Host ("95th Percentile   : {0:N2} ms" -f $p95)
    Write-Host "----------------------------------------------------"
} else {
    Write-Host "All requests failed. Check that CivicConnect.Api is running and verify the port in -ApiUrl." -ForegroundColor Red
}