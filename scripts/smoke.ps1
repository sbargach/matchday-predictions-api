$ErrorActionPreference = 'Stop'
$baseUrl = 'http://localhost:8081'

function Send-Request {
    param(
        [string]$Method,
        [string]$Path,
        $Body = $null,
        [string]$Token = ''
    )

    $request = @{
        Method = $Method
        Uri = "$baseUrl$Path"
        SkipHttpErrorCheck = $true
    }
    if ($Token) {
        $request.Headers = @{ Authorization = "Bearer $Token" }
    }
    if ($null -ne $Body) {
        $request.ContentType = 'application/json'
        $request.Body = ConvertTo-Json -InputObject $Body -Compress
    }

    Invoke-WebRequest @request
}

function Expect-Status {
    param($Response, [int]$Status, [string]$Step)

    if ([int]$Response.StatusCode -ne $Status) {
        throw "$Step returned $([int]$Response.StatusCode), expected $Status. $($Response.Content)"
    }
    Write-Host "OK  $Step ($Status)"
}

Expect-Status (Send-Request GET '/health') 200 'health'
Expect-Status (Send-Request GET '/metrics') 200 'metrics'
Expect-Status (Send-Request GET '/swagger/index.html') 200 'Swagger'

$login = Send-Request POST '/api/v1/auth/login' @{ username = 'demo'; password = 'Demo12345' }
Expect-Status $login 200 'demo login'
$token = ($login.Content | ConvertFrom-Json).token

$me = Send-Request GET '/api/v1/users/me' -Token $token
Expect-Status $me 200 'current user'
$userId = ($me.Content | ConvertFrom-Json).id

$leagues = Send-Request GET '/api/v1/leagues' -Token $token
Expect-Status $leagues 200 'list leagues'
$league = @($leagues.Content | ConvertFrom-Json)[0]
if (-not $league) { throw 'No demo league found' }

Expect-Status (Send-Request GET "/api/v1/leagues/$($league.leagueId)" -Token $token) 200 'league details'

$matches = Send-Request GET "/api/v1/matches?leagueId=$($league.leagueId)" -Token $token
Expect-Status $matches 200 'list matches'
if (-not @($matches.Content | ConvertFrom-Json)[0]) { throw 'No demo match found' }

$match = Send-Request POST '/api/v1/matches' @{
    leagueId = $league.leagueId
    homeTeam = 'Ajax Demo'
    awayTeam = 'PSV Demo'
    kickoffUtc = [DateTime]::UtcNow.AddDays(1).ToString('o')
} -Token $token
Expect-Status $match 201 'create match'
$matchId = ($match.Content | ConvertFrom-Json).matchId
if ($matchId -le 0 -or $match.Headers.Location -notmatch "/matches/$matchId$") {
    throw "Create match returned an invalid ID or Location: $($match.Content)"
}

Expect-Status (Send-Request GET "/api/v1/matches/$matchId" -Token $token) 200 'match details'

$prediction = @{
    matchId = $matchId
    userId = $userId
    homeGoals = 2
    awayGoals = 1
}
Expect-Status (Send-Request POST '/api/v1/predictions' $prediction $token) 201 'create prediction'
Expect-Status (Send-Request GET "/api/v1/predictions/$matchId`?userId=$userId" -Token $token) 200 'read prediction'

$prediction.homeGoals = 3
Expect-Status (Send-Request POST '/api/v1/predictions' $prediction $token) 200 'update prediction'
$saved = Send-Request GET "/api/v1/predictions/$matchId`?userId=$userId" -Token $token
Expect-Status $saved 200 'read updated prediction'
if (($saved.Content | ConvertFrom-Json).homeGoals -ne 3) { throw 'Prediction update was not saved' }

$newUsername = "smoke$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
$register = Send-Request POST '/api/v1/users' @{
    username = $newUsername
    displayName = 'Smoke Test'
    email = "$newUsername@example.local"
    password = 'Smoke12345'
}
Expect-Status $register 201 'register user'
if ($register.Content -match 'Smoke12345') { throw 'Registration response exposed the password' }
Expect-Status (Send-Request POST '/api/v1/users' @{
    username = $newUsername
    displayName = 'Smoke Test'
    email = "$newUsername@example.local"
    password = 'Smoke12345'
}) 409 'duplicate registration'

$newLogin = Send-Request POST '/api/v1/auth/login' @{ username = $newUsername; password = 'Smoke12345' }
Expect-Status $newLogin 200 'new user login'
$newToken = ($newLogin.Content | ConvertFrom-Json).token
$newMe = Send-Request GET '/api/v1/users/me' -Token $newToken
Expect-Status $newMe 200 'new user profile'
$newUserId = ($newMe.Content | ConvertFrom-Json).id

$prediction.userId = $newUserId
Expect-Status (Send-Request POST '/api/v1/predictions' $prediction $newToken) 403 'league membership guard'
Expect-Status (Send-Request GET "/api/v1/predictions/$matchId`?userId=$userId" -Token $newToken) 403 'cross-user guard'

Write-Host 'Smoke test passed'
