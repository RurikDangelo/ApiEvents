$ErrorActionPreference = 'Stop'
$base = 'https://localhost:7164'

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

function B64Url([byte[]]$b) {
    [Convert]::ToBase64String($b).TrimEnd('=').Replace('+','-').Replace('/','_')
}

# 1) JWT manual (HS256)
$secret  = 'dev-secret-key-only-for-local-testing'
$header  = '{"alg":"HS256","typ":"JWT"}'
$payload = '{"sub":"22222222-2222-2222-2222-222222222222","school_id":"11111111-1111-1111-1111-111111111111","role":"school","exp":9999999999}'
$hP = B64Url ([Text.Encoding]::UTF8.GetBytes($header))
$pP = B64Url ([Text.Encoding]::UTF8.GetBytes($payload))
$sign = "$hP.$pP"
$hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($secret))
$sig = B64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($sign)))
$token = "$sign.$sig"
$h = @{ Authorization = "Bearer $token" }
Write-Host "Token gerado (len=$($token.Length))"

# 2) GET /event/all
Write-Host "`n[1] GET /event/all"
$events = Invoke-RestMethod -Method GET -Uri "$base/api/v1/event/all" -Headers $h
Write-Host "    eventos=$($events.Count)"
$evt = $events | Where-Object { $_.id -eq 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' } | Select-Object -First 1
if (-not $evt) { $evt = $events | Where-Object { $_.status -eq 1 } | Select-Object -First 1 }
if (-not $evt) { throw 'Nenhum evento com status=EnrollmentOpen.' }
$eventId = $evt.id
Write-Host "    eventId=$eventId  status=$($evt.status)  title=$($evt.title)"

# 3) GET segmentos disponiveis
Write-Host "`n[2] GET segmentos"
$segs = Invoke-RestMethod -Method GET -Uri "$base/api/v1/event-enrollment/segment/$eventId" -Headers $h
Write-Host "    segmentos=$($segs.Count)"
$pick = $null
foreach ($s in $segs) {
    foreach ($c in $s.categories) {
        if ($c.isAvailable) {
            $pick = [pscustomobject]@{ S=$s.id; C=$c.id; Sn=$s.name; Cn=$c.name }
            break
        }
    }
    if ($pick) { break }
}
if (-not $pick) { throw 'Nenhuma combinacao Segmento/Categoria disponivel.' }
Write-Host "    segmento='$($pick.Sn)' (id=$($pick.S))"
Write-Host "    categoria='$($pick.Cn)' (id=$($pick.C))"

# 4) POST /event-enrollment
Write-Host "`n[3] POST /event-enrollment"
$body = @{
    eventId                  = $eventId
    segmentId                = $pick.S
    categoryId               = $pick.C
    projectName              = "Projeto Demo $(Get-Date -Format HHmmss)"
    responsibleName          = 'Responsavel Demo'
    managementRepresentative = 'Representante Demo'
} | ConvertTo-Json
$enr = Invoke-RestMethod -Method POST -Uri "$base/api/v1/event-enrollment" -Headers $h -ContentType 'application/json' -Body $body
$enrId = $enr.id
Write-Host "    EnrollmentID=$enrId"
Write-Host "    projectName=$($enr.projectName)"

# 5) Upload (multipart manual, compativel com PS 5.1)
Write-Host "`n[4] POST upload"
$tmp = Join-Path $env:TEMP "demo-$(Get-Date -Format HHmmss).png"
[IO.File]::WriteAllBytes($tmp, [byte[]](137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,1,0,0,0,1,8,6,0,0,0,31,21,196,137,0,0,0,13,73,68,65,84,120,156,99,248,255,255,63,0,5,254,2,254,167,181,129,193,0,0,0,0,73,69,78,68,174,66,96,130))

$boundary = [Guid]::NewGuid().ToString()
$LF = "`r`n"
$fileBytes = [IO.File]::ReadAllBytes($tmp)
$fileName = [IO.Path]::GetFileName($tmp)
$head = "--$boundary$LF" +
        "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"$LF" +
        "Content-Type: image/png$LF$LF"
$tail = "$LF--$boundary--$LF"
$ms = New-Object IO.MemoryStream
$enc = [Text.Encoding]::UTF8
$ms.Write($enc.GetBytes($head), 0, $enc.GetByteCount($head))
$ms.Write($fileBytes, 0, $fileBytes.Length)
$ms.Write($enc.GetBytes($tail), 0, $enc.GetByteCount($tail))
$payloadBytes = $ms.ToArray()
$ms.Dispose()

$req = [System.Net.HttpWebRequest]::Create("$base/api/v1/event-enrollment/$enrId/files")
$req.Method = 'POST'
$req.ContentType = "multipart/form-data; boundary=$boundary"
$req.Headers.Add('Authorization', "Bearer $token")
$req.ContentLength = $payloadBytes.Length
$rs = $req.GetRequestStream()
$rs.Write($payloadBytes, 0, $payloadBytes.Length)
$rs.Close()
try {
    $resp = $req.GetResponse()
    $sr = New-Object IO.StreamReader($resp.GetResponseStream())
    $respBody = $sr.ReadToEnd() | ConvertFrom-Json
    Write-Host "    FileId=$($respBody.id)  type=$($respBody.fileType)  size=$($respBody.sizeInBytes)b"
} catch [System.Net.WebException] {
    $errResp = $_.Exception.Response
    if ($errResp) {
        $sr = New-Object IO.StreamReader($errResp.GetResponseStream())
        Write-Host "    Upload falhou: $($sr.ReadToEnd())"
    } else {
        throw
    }
}

# 6) Evidencia disco
Write-Host "`n[5] Evidencia disco"
$dir = "C:\uploads\apieventsr\$enrId"
if (Test-Path $dir) {
    Get-ChildItem $dir | ForEach-Object { Write-Host "    $($_.FullName) ($($_.Length)b)" }
} else {
    Write-Warning "    pasta nao existe: $dir"
}

# 7) Confirmar GET /event-enrollment/all
Write-Host "`n[6] GET /event-enrollment/all"
$list = Invoke-RestMethod -Method GET -Uri "$base/api/v1/event-enrollment/all" -Headers $h
Write-Host "    inscricoes=$($list.Count)"
$list | ForEach-Object {
    Write-Host "    - $($_.projectName) | canEdit=$($_.canEdit) canDelete=$($_.canDelete) canUpload=$($_.canUpload)"
}

Write-Host "`n=== SUCESSO ==="
Write-Host "EnrollmentID = $enrId"
Write-Host "EventId      = $eventId"
