<#
  ファイル名 / File: setup.ps1
  概要 / Description: Publishes mdEditor and registers it as a Windows Markdown app
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'mdEditor.csproj'
$publishPath = Join-Path $env:LOCALAPPDATA 'Programs\mdEditor'
$applicationPath = Join-Path $publishPath 'mdEditor.exe'

# 起動中の mdEditor は exe を使用中にして上書きを妨げるため、発行の前に閉じる。
# CloseMainWindow は通常の終了と同じで、未保存の文書があれば mdEditor が保存を確認する。
$running = @(Get-Process -Name 'mdEditor' -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -eq $applicationPath })
if ($running.Count -gt 0) {
    Write-Host "インストール済みの mdEditor が $($running.Count) 個起動しています。更新するには閉じる必要があります。"
    $answer = Read-Host '閉じてよろしいですか？未保存の文書があれば、保存するか確認されます (Y/N)'
    if ($answer -notmatch '^[Yy]') {
        throw 'mdEditor を閉じてから、もう一度実行してください。'
    }
    foreach ($process in $running) {
        [void]$process.CloseMainWindow()
    }
    Write-Host '保存の確認が表示された場合は、各ウィンドウで応答してください。'
    foreach ($process in $running) {
        if (-not $process.WaitForExit(60000)) {
            throw 'mdEditor が終了しませんでした。保存の確認に応答してから、もう一度実行してください。'
        }
    }
}

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
