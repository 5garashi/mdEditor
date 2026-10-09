<#
  File: setup.ps1
  Description: Publishes mdEditor and registers it as a Windows Markdown app
  Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  Created: 2026-10-10
  License: MIT
  SPDX-License-Identifier: MIT
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'mdEditor.csproj'
$publishPath = Join-Path $env:LOCALAPPDATA 'Programs\mdEditor'
$applicationPath = Join-Path $publishPath 'mdEditor.exe'

dotnet publish $projectPath --configuration Release --runtime win-x64 --self-contained true --output $publishPath -p:PublishSingleFile=true
if ($LASTEXITCODE -ne 0) {
    throw 'mdEditor の発行に失敗しました。'
}
if (-not (Test-Path $applicationPath)) {
    throw "発行後のアプリが見つかりません: $applicationPath"
}

$programId = 'mdEditor.MarkdownFile'
$openCommand = '"{0}" "%1"' -f $applicationPath
$extensionPaths = @(
    'Software\Classes\.md',
    'Software\Classes\.markdown'
)

New-Item -Path "HKCU:\Software\Classes\$programId\shell\open\command" -Force | Out-Null
Set-Item -Path "HKCU:\Software\Classes\$programId" -Value 'Markdown document'
New-Item -Path "HKCU:\Software\Classes\$programId\DefaultIcon" -Force | Out-Null
Set-Item -Path "HKCU:\Software\Classes\$programId\DefaultIcon" -Value "`"$applicationPath`",0"
Set-Item -Path "HKCU:\Software\Classes\$programId\shell\open\command" -Value $openCommand

foreach ($extensionPath in $extensionPaths) {
    $openWithPath = "HKCU:\$extensionPath\OpenWithProgids"
    New-Item -Path $openWithPath -Force | Out-Null
    New-ItemProperty -Path $openWithPath -Name $programId -Value '' -PropertyType String -Force | Out-Null
}

$capabilitiesPath = 'HKCU:\Software\mdEditor\Capabilities'
New-Item -Path $capabilitiesPath -Force | Out-Null
Set-ItemProperty -Path $capabilitiesPath -Name ApplicationName -Value 'mdEditor'
Set-ItemProperty -Path $capabilitiesPath -Name ApplicationDescription -Value 'Markdown を編集し、表示します。'
Set-ItemProperty -Path $capabilitiesPath -Name ApplicationIcon -Value "`"$applicationPath`",0"
New-Item -Path "$capabilitiesPath\FileAssociations" -Force | Out-Null
Set-ItemProperty -Path "$capabilitiesPath\FileAssociations" -Name '.md' -Value $programId
Set-ItemProperty -Path "$capabilitiesPath\FileAssociations" -Name '.markdown' -Value $programId
New-Item -Path 'HKCU:\Software\RegisteredApplications' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\RegisteredApplications' -Name 'mdEditor' -Value 'Software\mdEditor\Capabilities'

Write-Host 'mdEditor を登録しました。Windows の「既定のアプリ」で .md ファイルの既定アプリに選択してください。'
Start-Process 'ms-settings:defaultapps?registeredAppUser=mdEditor'
