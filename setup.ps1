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

# 開ける形式をすべて「プログラムから開く」と「既定のアプリ」の候補に出します（Microsoft Store 版の
# store/AppxManifest.template.xml と同じ組み合わせ）。Markdown とテキストはそのまま開き、ほかの形式
# （HTML を含む）は Markdown に変換した新しい文書として開きます。
# 既定のアプリにはしません。拡張子の OpenWithProgids に入れると、ほかに開けるアプリがない拡張子
# （.ipynb など）では Windows がそれを既定として使ってしまうため、候補は Applications\mdEditor.exe の
# SupportedTypes だけで示します（「プログラムから開く」の一覧に出るが、ダブルクリックでは使われない）。
$openCommand = '"{0}" "%1"' -f $applicationPath
$fileTypes = @(
    @{ ProgramId = 'mdEditor.MarkdownFile'; Name = 'Markdown document'; Extensions = '.md', '.markdown' },
    @{ ProgramId = 'mdEditor.TextFile'; Name = 'Text document'; Extensions = @('.txt') },
    @{ ProgramId = 'mdEditor.HtmlFile'; Name = 'HTML document'; Extensions = '.html', '.htm' },
    @{ ProgramId = 'mdEditor.WordFile'; Name = 'Word document'; Extensions = @('.docx') },
    @{ ProgramId = 'mdEditor.EmailFile'; Name = 'E-mail message'; Extensions = @('.eml') },
    @{ ProgramId = 'mdEditor.TableFile'; Name = 'CSV / TSV table'; Extensions = '.csv', '.tsv' },
    @{ ProgramId = 'mdEditor.DataFile'; Name = 'JSON / XML data'; Extensions = '.json', '.xml' },
    @{ ProgramId = 'mdEditor.NotebookFile'; Name = 'Jupyter Notebook'; Extensions = @('.ipynb') },
    @{ ProgramId = 'mdEditor.ImageFile'; Name = 'Image'; Extensions = '.png', '.jpg', '.jpeg', '.gif', '.bmp', '.webp' }
)

foreach ($fileType in $fileTypes) {
    $classPath = "HKCU:\Software\Classes\$($fileType.ProgramId)"
    New-Item -Path "$classPath\shell\open\command" -Force | Out-Null
    Set-Item -Path $classPath -Value $fileType.Name
    New-Item -Path "$classPath\DefaultIcon" -Force | Out-Null
    Set-Item -Path "$classPath\DefaultIcon" -Value "`"$applicationPath`",0"
    Set-Item -Path "$classPath\shell\open\command" -Value $openCommand
}

$applicationKey = 'HKCU:\Software\Classes\Applications\mdEditor.exe'
New-Item -Path "$applicationKey\shell\open\command" -Force | Out-Null
Set-Item -Path "$applicationKey\shell\open\command" -Value $openCommand
Set-ItemProperty -Path $applicationKey -Name FriendlyAppName -Value 'mdEditor'
Remove-Item -Path "$applicationKey\SupportedTypes" -Recurse -ErrorAction SilentlyContinue
New-Item -Path "$applicationKey\SupportedTypes" -Force | Out-Null
foreach ($fileType in $fileTypes) {
    foreach ($extension in $fileType.Extensions) {
        New-ItemProperty -Path "$applicationKey\SupportedTypes" -Name $extension -Value '' -PropertyType String -Force | Out-Null
        # 以前の版が入れた OpenWithProgids の値は、既定のように使われてしまうので取り除きます。
        Remove-ItemProperty -Path "HKCU:\Software\Classes\$extension\OpenWithProgids" -Name $fileType.ProgramId -ErrorAction SilentlyContinue
    }
}

$capabilitiesPath = 'HKCU:\Software\mdEditor\Capabilities'
New-Item -Path $capabilitiesPath -Force | Out-Null
Set-ItemProperty -Path $capabilitiesPath -Name ApplicationName -Value 'mdEditor'
Set-ItemProperty -Path $capabilitiesPath -Name ApplicationDescription -Value 'Markdown を編集し、表示します。'
Set-ItemProperty -Path $capabilitiesPath -Name ApplicationIcon -Value "`"$applicationPath`",0"
# 作り直して、登録しなくなった形式が残らないようにします。
Remove-Item -Path "$capabilitiesPath\FileAssociations" -Recurse -ErrorAction SilentlyContinue
New-Item -Path "$capabilitiesPath\FileAssociations" -Force | Out-Null
foreach ($fileType in $fileTypes) {
    foreach ($extension in $fileType.Extensions) {
        Set-ItemProperty -Path "$capabilitiesPath\FileAssociations" -Name $extension -Value $fileType.ProgramId
    }
}
New-Item -Path 'HKCU:\Software\RegisteredApplications' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\RegisteredApplications' -Name 'mdEditor' -Value 'Software\mdEditor\Capabilities'

Write-Host 'mdEditor を登録しました。Windows の「既定のアプリ」で .md ファイルの既定アプリに選択してください。'
Start-Process 'ms-settings:defaultapps?registeredAppUser=mdEditor'
