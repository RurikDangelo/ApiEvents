param(
    [string]$ApiBaseUrl = "https://localhost:7164",
    [string]$StoragePath = "C:\uploads\apieventsr",
    [string]$SchoolId = "11111111-1111-1111-1111-111111111111",
    [string]$UserId = "22222222-2222-2222-2222-222222222222",
    [string]$Role = "school"
)

$ErrorActionPreference = "Stop"
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

function New-DevJwtToken {
    param(
        [string]$SchoolIdParam,
        [string]$UserIdParam,
        [string]$RoleParam,
        [int]$HoursValid = 12
    )

    $secret = "dev-secret-key-only-for-local-testing"
    $headerJson = '{"alg":"HS256","typ":"JWT"}'
    $exp = [DateTimeOffset]::UtcNow.AddHours($HoursValid).ToUnixTimeSeconds()
    $payloadJson = (@{
        sub = $UserIdParam
        school_id = $SchoolIdParam
        role = $RoleParam
        exp = $exp
    } | ConvertTo-Json -Compress)

    function ConvertTo-Base64Url([byte[]]$bytes) {
        return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
    }

    $header = ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($headerJson))
    $payload = ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($payloadJson))
    $toSign = "$header.$payload"

    $hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($secret))
    $signature = ConvertTo-Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($toSign)))

    return "$toSign.$signature"
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Token,
        [object]$Body
    )

    if ($Body -ne $null) {
        $tmpBody = Join-Path $env:TEMP "api-body-$(Get-Date -Format yyyyMMddHHmmssfff).json"
        ($Body | ConvertTo-Json -Depth 8 -Compress) | Set-Content -Path $tmpBody -Encoding utf8
        $raw = & curl.exe -k -sS -X $Method -H "Authorization: Bearer $Token" -H "Content-Type: application/json" --data-binary "@$tmpBody" $Url
    }
    else {
        $raw = & curl.exe -k -sS -X $Method -H "Authorization: Bearer $Token" $Url
    }

    return $raw | ConvertFrom-Json
}

Write-Host "1) Gerando token local de desenvolvimento..."
$token = New-DevJwtToken -SchoolIdParam $SchoolId -UserIdParam $UserId -RoleParam $Role

Write-Host "2) Listando eventos..."
$events = Invoke-Api -Method GET -Url "$ApiBaseUrl/api/v1/event/all" -Token $token -Body $null
if (-not $events -or $events.Count -eq 0) {
    throw "Nenhum evento retornado por /event/all. Rode seed e valide migrations."
}

$selectedEvent = $events | Where-Object { $_.status -eq 1 } | Select-Object -First 1
if (-not $selectedEvent) {
    $selectedEvent = $events | Select-Object -First 1
}

$eventId = $selectedEvent.id
Write-Host "EventId selecionado: $eventId"

Write-Host "3) Buscando segmentos/categorias disponíveis..."
$segments = Invoke-Api -Method GET -Url "$ApiBaseUrl/api/v1/event-enrollment/segment/$eventId" -Token $token -Body $null

$firstAvailable = $null
foreach ($segment in $segments) {
    foreach ($category in $segment.categories) {
        if ($category.isAvailable -eq $true) {
            $firstAvailable = [pscustomobject]@{
                SegmentId = $segment.id
                CategoryId = $category.id
            }
            break
        }
    }
    if ($firstAvailable -ne $null) { break }
}

if ($firstAvailable -eq $null) {
    throw "Nenhuma combinação Segment/Categoria disponível encontrada."
}

Write-Host "SegmentId selecionado: $($firstAvailable.SegmentId)"
Write-Host "CategoryId selecionado: $($firstAvailable.CategoryId)"

Write-Host "4) Criando inscrição (captura EnrollmentID)..."
$createBody = @{
    eventId = $eventId
    segmentId = $firstAvailable.SegmentId
    categoryId = $firstAvailable.CategoryId
    projectName = "Projeto Demo API $(Get-Date -Format yyyyMMddHHmmss)"
    responsibleName = "Responsavel Demo"
    managementRepresentative = "Representante Demo"
}
$createdEnrollment = Invoke-Api -Method POST -Url "$ApiBaseUrl/api/v1/event-enrollment" -Token $token -Body $createBody
$enrollmentId = $createdEnrollment.id
if (-not $enrollmentId) {
    throw "Falha ao criar inscrição: $($createdEnrollment | ConvertTo-Json -Depth 8 -Compress)"
}
Write-Host "EnrollmentID capturado: $enrollmentId"

Write-Host "5) Executando upload de arquivo..."
$tmpFile = Join-Path $env:TEMP "demo-upload-$(Get-Date -Format yyyyMMddHHmmss).png"
[IO.File]::WriteAllBytes($tmpFile, [byte[]](137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,1,0,0,0,1,8,6,0,0,0,31,21,196,137,0,0,0,13,73,68,65,84,120,156,99,248,255,255,63,0,5,254,2,254,167,181,129,193,0,0,0,0,73,69,78,68,174,66,96,130))

$uploadUri = "$ApiBaseUrl/api/v1/event-enrollment/$enrollmentId/files"
$uploadRaw = & curl.exe -k -sS -H "Authorization: Bearer $token" -F "file=@$tmpFile;type=image/png" $uploadUri
$uploadResponse = $uploadRaw | ConvertFrom-Json
Write-Host "Upload concluído. FileId: $($uploadResponse.id)"

Write-Host "6) Validando evidência no storage..."
if (-not (Test-Path $StoragePath)) {
    Write-Warning "Storage path '$StoragePath' não existe."
} else {
    $storedFiles = Get-ChildItem -Path $StoragePath -Recurse -File | Sort-Object LastWriteTime -Descending | Select-Object -First 5
    Write-Host "Arquivos recentes no storage:"
    $storedFiles | ForEach-Object { Write-Host " - $($_.FullName)" }
}

Write-Host ""
Write-Host "SUCESSO:"
Write-Host "EnrollmentID = $enrollmentId"
Write-Host "EventId      = $eventId"
Write-Host "SegmentId    = $($firstAvailable.SegmentId)"
Write-Host "CategoryId   = $($firstAvailable.CategoryId)"

