param(
    [string]$ApiBase = "http://localhost:5293/api",
    [string]$Username = $env:DIGITAL_QA_TEST_USERNAME,
    [string]$Password = $env:DIGITAL_QA_TEST_PASSWORD,
    [switch]$RunWriteFlow,
    [string]$CourseId,
    [string]$PdfPath
)

$ErrorActionPreference = 'Stop'

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "FAILED: $Message" }
    Write-Host "PASS: $Message" -ForegroundColor Green
}

function Invoke-Api([string]$Path, [string]$Method = 'GET', $Body = $null, [string]$Token = $null) {
    $headers = @{}
    if ($Token) { $headers.Authorization = "Bearer $Token" }
    $params = @{ Uri = "$ApiBase$Path"; Method = $Method; Headers = $headers; ErrorAction = 'Stop' }
    if ($null -ne $Body) { $params.ContentType = 'application/json'; $params.Body = ($Body | ConvertTo-Json -Depth 8) }
    Invoke-RestMethod @params
}

function Invoke-MultipartUpload([string]$MaterialId, [string]$FilePath, [string]$Token, [string]$Remark) {
    $client = [System.Net.Http.HttpClient]::new()
    $form = [System.Net.Http.MultipartFormDataContent]::new()
    $stream = $null
    try {
        $client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $Token)
        $form.Add([System.Net.Http.StringContent]::new($MaterialId), 'learningMaterialId')
        $form.Add([System.Net.Http.StringContent]::new($Remark), 'remark')
        $stream = [System.IO.File]::OpenRead((Resolve-Path -LiteralPath $FilePath))
        $fileContent = [System.Net.Http.StreamContent]::new($stream)
        $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse('application/pdf')
        $form.Add($fileContent, 'file', [System.IO.Path]::GetFileName($FilePath))
        $response = $client.PostAsync("$ApiBase/material-versions/upload", $form).GetAwaiter().GetResult()
        $payload = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) { throw "Upload failed ($($response.StatusCode)): $payload" }
        return $payload | ConvertFrom-Json
    }
    finally {
        if ($stream) { $stream.Dispose() }
        $form.Dispose()
        $client.Dispose()
    }
}

Write-Host "DigitalLearningQA API smoke tests: $ApiBase" -ForegroundColor Cyan

try { Invoke-Api '/learning-materials' | Out-Null; throw 'Endpoint accepted an anonymous request.' }
catch {
    if ($_.Exception.Message -match 'Endpoint accepted') { throw }
    $status = $null
    if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
    Assert-Condition ($status -in @(401, 403)) 'API blocks anonymous material access'
}

if ([string]::IsNullOrWhiteSpace($Username) -or [string]::IsNullOrWhiteSpace($Password)) {
    Write-Warning 'Set DIGITAL_QA_TEST_USERNAME and DIGITAL_QA_TEST_PASSWORD to test login, authorization and notifications.'
    exit 0
}

$session = Invoke-Api '/auth/login' 'POST' @{ username = $Username; password = $Password }
Assert-Condition (-not [string]::IsNullOrWhiteSpace($session.accessToken)) 'Login returns an access token'
$me = Invoke-Api '/auth/me' 'GET' $null $session.accessToken
Assert-Condition ($me.username -eq $Username) 'JWT identifies the authenticated account'
$notifications = Invoke-Api '/notifications' 'GET' $null $session.accessToken
Assert-Condition ($null -ne $notifications) 'Current user can retrieve notifications'

if ($me.role -ne 'Admin') {
    try { Invoke-Api '/users' 'GET' $null $session.accessToken | Out-Null; throw 'Non-admin accessed user management.' }
    catch { Assert-Condition ($_.Exception.Message -notmatch 'Non-admin accessed') 'Role authorization blocks user management' }
}

if (-not $RunWriteFlow) {
    Write-Host 'Skipping upload/evaluation/resubmission. Use -RunWriteFlow -CourseId <GUID> -PdfPath <PDF> for the write flow.' -ForegroundColor Yellow
    exit 0
}

Assert-Condition ($me.role -in @('Lecturer','CCD')) 'The account may submit learning materials'
$parsedCourseId = [Guid]::Empty
Assert-Condition ([Guid]::TryParse($CourseId, [ref]$parsedCourseId)) 'CourseId is a valid GUID'
Assert-Condition (Test-Path -LiteralPath $PdfPath) 'The PDF test file exists'
$stamp = Get-Date -Format 'yyyyMMddHHmmss'
$material = Invoke-Api '/learning-materials' 'POST' @{ code = "SMOKE-$stamp"; courseId = $CourseId; title = "Smoke test material $stamp"; description = 'CLO, content, activities, questions, answers, rubric and OBE mapping.'; materialType = 0 } $session.accessToken
Assert-Condition ($null -ne $material.id) 'Creates a test material'
$upload = Invoke-MultipartUpload $material.id $PdfPath $session.accessToken 'Automated test version 1.'
Assert-Condition ($null -ne $upload.id) 'Uploads a material version'
$test = Invoke-Api '/auto-tests' 'POST' @{ materialVersionId = $upload.id } $session.accessToken
Assert-Condition ($null -ne $test.id) 'Starts automatic evaluation'
$report = Invoke-Api "/auto-tests/$($test.id)/report" 'GET' $null $session.accessToken
Assert-Condition ($null -ne $report.overallScore) 'Returns an evaluation report'
$resubmission = Invoke-MultipartUpload $material.id $PdfPath $session.accessToken 'Automated test resubmission.'
Assert-Condition ($resubmission.id -ne $upload.id) 'Resubmission creates a new version'
$retest = Invoke-Api '/auto-tests' 'POST' @{ materialVersionId = $resubmission.id } $session.accessToken
$retestReport = Invoke-Api "/auto-tests/$($retest.id)/report" 'GET' $null $session.accessToken
Assert-Condition ($null -ne $retestReport.overallScore) 'Resubmission is evaluated again'
Write-Host 'Completed: upload -> evaluate -> resubmit -> evaluate again.' -ForegroundColor Green
