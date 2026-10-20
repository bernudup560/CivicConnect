$api = "http://localhost:5080/api/tickets"

foreach ($cat in 1..5) {
  foreach ($emergency in $false, $true) {
    $body = @{
      title             = "Test ticket cat $cat"
      description       = "Testing priority output"
      categoryId        = $cat
      submittedByUserId = 1
      isEmergency       = $emergency
    } | ConvertTo-Json

    try {
      $r = Invoke-RestMethod -Uri $api -Method Post -Body $body -ContentType "application/json"
      "{0,-3} emergency={1,-5} -> {2}" -f $cat, $emergency, $r.priority
    } catch {
      "{0,-3} emergency={1,-5} -> ERROR: {2}" -f $cat, $emergency, $_.Exception.Message
    }
  }
}