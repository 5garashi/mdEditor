<#
  ファイル名 / File: build-msix.ps1
  概要 / Description: Builds an unsigned MSIX package for Microsoft Store submission
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-11
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT

  Partner Center の「製品の ID」にある値を指定します。省略した値は仮のものなので、
  そのままでは提出できません。Store へ提出する場合、署名は Store が行います。
#>
[CmdletBinding()]
param(
    [string]$IdentityName = 'REPLACE.mdEditor',
    [string]$Publisher = 'CN=REPLACE-WITH-PARTNER-CENTER-PUBLISHER-ID',
    [string]$PublisherDisplayName = '5garashi.com Design Office',
    [string]$DisplayName = 'mdEditor by 5garashi.com',
    # 省略すると VERSION ファイルの番号に .0 を付けます（例: 1.0.3 → 1.0.3.0）。
    [string]$Version,
    [string]$OutputDir
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = (Resolve-Path (Join-Path $here '..')).Path
if (-not $OutputDir) { $OutputDir = Join-Path $root 'artifacts' }
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
$OutputDir = (Resolve-Path $OutputDir).Path
$appVersion = (Get-Content (Join-Path $root 'VERSION') -Raw).Trim()
if (-not $Version) { $Version = "$appVersion.0" }
if ($Version -ne "$appVersion.0") {
    throw "Version ($Version) が VERSION ファイル ($appVersion) と合いません。VERSION を書き換えてください。"
}
$staging = Join-Path $OutputDir 'msix-layout'
$package = Join-Path $OutputDir "mdEditor_${Version}_x64.msix"

if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') { throw 'Version は 1.0.0.0 の形式で指定してください。' }

$makeappx = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\makeappx.exe" -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending | Select-Object -First 1
if (-not $makeappx) { throw 'makeappx.exe が見つかりません。Windows 10/11 SDK をインストールしてください。' }

if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging, "$staging\Assets" -Force | Out-Null

dotnet publish (Join-Path $root 'mdEditor.csproj') --configuration Release --runtime win-x64 --self-contained true --output $staging -p:PublishSingleFile=true
if ($LASTEXITCODE -ne 0) { throw '発行に失敗しました。' }

# ロゴは store\assets の画像（render-assets.ps1 でマークの SVG から描いたもの）を使います。
# 表示倍率ごと（scale-100〜400）とタスクバーの大きさごと（targetsize）の画像があります。
$assets = Join-Path $here 'assets'
if (-not (Get-ChildItem $assets -Filter '*.png' -ErrorAction SilentlyContinue)) {
    throw 'store\assets にロゴがありません。先に store\render-assets.ps1 を実行してください。'
}
Copy-Item (Join-Path $assets '*.png') "$staging\Assets"

$manifest = Get-Content (Join-Path $here 'AppxManifest.template.xml') -Raw -Encoding utf8
$values = @{
    IDENTITY_NAME = $IdentityName; PUBLISHER = $Publisher; VERSION = $Version
    DISPLAY_NAME = $DisplayName; PUBLISHER_DISPLAY_NAME = $PublisherDisplayName
}
foreach ($key in $values.Keys) {
    $manifest = $manifest.Replace("@$key@", [System.Security.SecurityElement]::Escape($values[$key]))
}
[IO.File]::WriteAllText((Join-Path $staging 'AppxManifest.xml'), $manifest, (New-Object System.Text.UTF8Encoding($false)))

# resources.pri があると、Windows が表示倍率や大きさに合ったロゴを選びます。
$makepri = Join-Path (Split-Path $makeappx.FullName) 'makepri.exe'
if (-not (Test-Path $makepri)) { throw 'makepri.exe が見つかりません。Windows 10/11 SDK をインストールしてください。' }
$priConfig = Join-Path $OutputDir 'priconfig.xml'
& $makepri createconfig /cf $priConfig /dq en-US /pv 10.0.0 /o | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'resources.pri の設定を作成できませんでした。' }
# 既定の設定は倍率ごとの画像を別のリソース パッケージに分けるため、1つの MSIX にすべて入れます。
$config = [xml](Get-Content $priConfig -Raw)
$config.SelectNodes('//packaging') | ForEach-Object { [void]$_.ParentNode.RemoveChild($_) }
$config.Save($priConfig)
& $makepri new /pr $staging /cf $priConfig /mn (Join-Path $staging 'AppxManifest.xml') /of (Join-Path $staging 'resources.pri') /o | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'resources.pri を作成できませんでした。' }
Remove-Item $priConfig

& $makeappx.FullName pack /d $staging /p $package /o
if ($LASTEXITCODE -ne 0) { throw 'MSIX の作成に失敗しました。' }
Write-Host "作成しました: $package"
