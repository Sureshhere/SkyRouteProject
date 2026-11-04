
# ============================================
# SkyRoute - Prometheus Business Traffic Generator
# ============================================

$BaseUrl = "http://localhost:5235"

# Paste the SESSION_ID cookie value from your browser.
$AuthToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3ZGViNzBmNy0zOWZmLTRjNWMtOWQ0ZS0xMjE3NGQzYTI2NjMiLCJlbWFpbCI6InN1cmVzaEBlcGFtLmNvbSIsImZ1bGxOYW1lIjoiU3VyZXNoIiwianRpIjoiNGJlMWZkYmMtNDZlYi00ZTI4LTljNTgtMDc5ZTIyOWRkNDBjIiwiaWF0IjoxNzkzNjg4MjY0LCJuYmYiOjE3OTM2ODgyNjQsImV4cCI6MTc5MzY5MTg2NCwiaXNzIjoiU2t5Um91dGUiLCJhdWQiOiJTa3lSb3V0ZSJ9.vqazn6FQHa7OBXrXaf3Uibs3nqQzINmbA8J-NTpyF8k"

# Number of search iterations.
$Iterations = 30

# Booking attempts are limited to avoid excessive test data.
$MaxBookingAttempts = 5

$SearchUrl = "$BaseUrl/api/flights/search"
$BookingUrl = "$BaseUrl/api/bookings"

$Headers = @{
    Cookie = "auth_token=$AuthToken"
}

# Search variations.
$Routes = @(
    @{ Origin = "BOM"; Destination = "DEL" }
)

$CabinClasses = @(1)

$SearchResults = @()
$BookingAttempts = 0

$SearchStats = @{
    Success = 0
    NoResults = 0
    Failed = 0
}

$BookingStats = @{
    Success = 0
    Failed = 0
}

Write-Host "`nStarting SkyRoute traffic generation..." -ForegroundColor Cyan

for ($i = 1; $i -le $Iterations; $i++) {

    $Route = $Routes[0]
    $Cabin = 1
    $Passengers = 1
    $DepartureDate = "2026-11-03"

    # Occasionally use an invalid route to exercise validation/failure paths.
    if ($i % 7 -eq 0) {
        $Route = @{
            Origin = "XXX"
            Destination = "YYY"
        }
    }

    $SearchRequest = @{
    originAirportCode = $Route.Origin
    destinationAirportCode = $Route.Destination
    departureDate = $DepartureDate
    numberOfPassengers = $Passengers
    cabinClass = $Cabin
    } | ConvertTo-Json

    Write-Host "`n[$i/$Iterations] Searching $($Route.Origin) -> $($Route.Destination)" -ForegroundColor Yellow

    try {
        $Response = Invoke-RestMethod `
            -Uri $SearchUrl `
            -Method POST `
            -ContentType "application/json" `
            -Body $SearchRequest `
            -TimeoutSec 15

        if ($Response.flights.Count -gt 0) {

            $SearchStats.Success++

            Write-Host "Search successful: $($Response.flights.Count) flights" -ForegroundColor Green

            foreach ($Flight in $Response.flights) {
                $SearchResults += $Flight
            }
        }
        else {
            $SearchStats.NoResults++
            Write-Host "No flights found." -ForegroundColor DarkYellow
        }
    }
    catch {
        $SearchStats.Failed++

        Write-Host "`nSearch failed on iteration $i" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

        $Response = $_.Exception.Response

        if ($Response) {
            Write-Host "HTTP Status: $([int]$Response.StatusCode)" -ForegroundColor Yellow

            $Reader = [System.IO.StreamReader]::new(
                $Response.GetResponseStream()
            )

            $ErrorBody = $Reader.ReadToEnd()
            $Reader.Dispose()

            Write-Host "Response Body: $ErrorBody" -ForegroundColor Yellow
        }
    }

    # Booking attempts use actual IDs returned by the search API.
    if (
        $BookingAttempts -lt $MaxBookingAttempts -and
        $SearchResults.Count -gt 0 -and
        $i % 5 -eq 0
    ) {
        $Flight = Get-Random -InputObject $SearchResults

        $BookingRequest = @{
            flightId = $Flight.id
            departureDate = $DepartureDate
            passengers = @(
                @{
                    fullName = "SkyRoute Test User $i"
                    email = "skyroute.test$i@example.com"
                    documentNumber = "TESTDOC$i"
                    seatNumber = "1A"
                }
            )
        } | ConvertTo-Json -Depth 5

        $BookingAttempts++

        Write-Host "Attempting booking for $($Flight.airlineName)..." -ForegroundColor Cyan

        try {
            $BookingResponse = Invoke-RestMethod `
                -Uri $BookingUrl `
                -Method POST `
                -Headers $Headers `
                -ContentType "application/json" `
                -Body $BookingRequest `
                -TimeoutSec 15

            $BookingStats.Success++
            Write-Host "Booking successful." -ForegroundColor Green
        }
        catch {
            $BookingStats.Failed++
            Write-Host "Booking failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Small delay to avoid overwhelming the local API.
    Start-Sleep -Milliseconds (Get-Random -Minimum 300 -Maximum 1200)
}

Write-Host "`n========== TRAFFIC SUMMARY ==========" -ForegroundColor Cyan

Write-Host "`nFlight searches:"
Write-Host "Successful: $($SearchStats.Success)"
Write-Host "No results: $($SearchStats.NoResults)"
Write-Host "Failed: $($SearchStats.Failed)"

Write-Host "`nBookings:"
Write-Host "Successful: $($BookingStats.Success)"
Write-Host "Failed: $($BookingStats.Failed)"

Write-Host "`nTotal booking attempts: $BookingAttempts"
Write-Host "======================================" -ForegroundColor Cyan
