## Booking API Test Scripts

### Generate Flight-Search Traffic

Run this PowerShell script with the local API running to send 30 flight-search requests across randomly selected routes, pausing 500 milliseconds between requests.

```powershell
$uri = "http://localhost:5235/api/flights/search"

$routes = @(
    @{ Origin = "BOM"; Destination = "DEL" },
    @{ Origin = "JFK"; Destination = "LHR" },
    @{ Origin = "LAX"; Destination = "ORD" },
    @{ Origin = "DEL"; Destination = "LHR" }
)

1..30 | ForEach-Object {
    $route = $routes | Get-Random

    $body = @{
        originAirportCode = $route.Origin
        destinationAirportCode = $route.Destination
        departureDate = "2026-11-03"
        numberOfPassengers = 1
        cabinClass = 1
    } | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri $uri -Method POST `
            -ContentType "application/json" -Body $body | Out-Null

        Write-Host "Search $_ completed: $($route.Origin) -> $($route.Destination)"
    }
    catch {
        Write-Host "Search $_ failed"
    }

    Start-Sleep -Milliseconds 500
}
```

These PowerShell scripts exercise booking failure and success scenarios against the local API.

### Prerequisites

- Start the API at `http://localhost:5235`.
- Provide a valid `auth_token` cookie when prompted.

### Booking Failure Case

This request uses an invalid flight ID to verify that the API handles a failed booking request.

```powershell
$BaseUrl = "http://localhost:5235"
$AuthToken = (Read-Host "Paste your current auth_token cookie value only").Trim()

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$cookie = New-Object System.Net.Cookie
$cookie.Name = "auth_token"
$cookie.Value = $AuthToken
$cookie.Path = "/"
$session.Cookies.Add([Uri]$BaseUrl, $cookie)

$body = @{
    flightId = "ffffffff-ffff-ffff-ffff-ffffffffffff"
    departureDate = "2026-11-03"
    passengers = @(
        @{
            fullName = "Monitoring Test"
            email = "monitoring-test@example.com"
            documentNumber = "TEST123456"
            seatNumber = "9Z"
        }
    )
} | ConvertTo-Json -Depth 5

try {
    Invoke-RestMethod `
        -Uri "$BaseUrl/api/bookings" `
        -Method POST `
        -WebSession $session `
        -MaximumRedirection 0 `
        -ContentType "application/json" `
        -Body $body `
        -ErrorAction Stop
}
catch {
    Write-Host "Request failed:"
    Write-Host $_.Exception.Message
    if ($_.ErrorDetails.Message) {
        Write-Host $_.ErrorDetails.Message
    }
}
```

### Booking Success Case

The script searches for available flights, prompts for a flight and seat, then submits a booking.

```powershell
$BaseUrl = "http://localhost:5235"

# Authentication
$AuthToken = (Read-Host "Paste your current auth_token cookie value").Trim()

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$cookie = New-Object System.Net.Cookie
$cookie.Name = "auth_token"
$cookie.Value = $AuthToken
$cookie.Path = "/"
$session.Cookies.Add([Uri]$BaseUrl, $cookie)

# Search for real flights
$searchBody = @{
    originAirportCode = "BOM"
    destinationAirportCode = "DEL"
    departureDate = "2026-11-03"
    numberOfPassengers = 1
    cabinClass = 1
} | ConvertTo-Json

try {
    $searchResult = Invoke-RestMethod `
        -Uri "$BaseUrl/api/flights/search" `
        -Method POST `
        -WebSession $session `
        -ContentType "application/json" `
        -Body $searchBody `
        -ErrorAction Stop

    $flights = @($searchResult.flights)

    if ($flights.Count -eq 0) {
        Write-Host "No flights available."
        return
    }

    Write-Host "`nAvailable flights:"

    for ($i = 0; $i -lt $flights.Count; $i++) {
        Write-Host "[$i] $($flights[$i].airlineName) - $($flights[$i].flightNumber) | ID: $($flights[$i].id) | Departure: $($flights[$i].departureTime)"
    }

    # Select a real flight
    $index = [int](Read-Host "`nEnter the flight index to book")

    if ($index -lt 0 -or $index -ge $flights.Count) {
        Write-Host "Invalid flight selection."
        return
    }

    $selectedFlight = $flights[$index]

    # Use the actual departure date from the selected flight
    $departureDate = ([datetime]$selectedFlight.departureTime).ToString("yyyy-MM-dd")

    $seatNumber = Read-Host "Enter an available seat number"

    $bookingBody = @{
        flightId = $selectedFlight.id
        departureDate = $departureDate
        passengers = @(
            @{
                fullName = "Monitoring Test"
                email = "monitoring-test@example.com"
                documentNumber = "TEST123456"
                seatNumber = $seatNumber
            }
        )
    } | ConvertTo-Json -Depth 5

    Write-Host "`nSubmitting booking..."

    $bookingResult = Invoke-RestMethod `
        -Uri "$BaseUrl/api/bookings" `
        -Method POST `
        -WebSession $session `
        -ContentType "application/json" `
        -Body $bookingBody `
        -ErrorAction Stop

    Write-Host "`nBOOKING SUCCESSFUL"
    $bookingResult | ConvertTo-Json -Depth 6
}
catch {
    Write-Host "`nREQUEST FAILED:"
    Write-Host $_.Exception.Message

    if ($_.ErrorDetails.Message) {
        Write-Host $_.ErrorDetails.Message
    }
}
```