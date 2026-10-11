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
    # Store 提出では末尾の桁を 0 にします（例: 1.0.0.0）。
    [string]$Version = '1.0.0.0',
    [string]$OutputDir
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = (Resolve-Path (Join-Path $here '..')).Path
if (-not $OutputDir) { $OutputDir = Join-Path $root 'artifacts' }
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
$OutputDir = (Resolve-Path $OutputDir).Path
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

# Store の必須サイズのアイコンを、既存のマークから生成します。
Add-Type -AssemblyName System.Drawing
$iconBytes = [IO.File]::ReadAllBytes((Join-Path $root 'wwwroot\assets\5garashi_mark_simple.ico'))
$best = 0; $bestSize = 0
for ($k = 0; $k -lt [BitConverter]::ToUInt16($iconBytes, 4); $k++) {
    $size = [BitConverter]::ToUInt32($iconBytes, 6 + 16 * $k + 8)
    if ($size -gt $bestSize) { $best = $k; $bestSize = $size }
}
$offset = [BitConverter]::ToUInt32($iconBytes, 6 + 16 * $best + 12)
# 最大の画像は PNG 形式で格納されているので、そのまま読み込みます。
$source = [System.Drawing.Bitmap]::new([IO.MemoryStream]::new($iconBytes[$offset..($offset + $bestSize - 1)]))
function New-Logo([int]$Width, [int]$Height, [string]$Name) {
    $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.InterpolationMode = 'HighQualityBicubic'
    $graphics.SmoothingMode = 'HighQuality'
    $size = [int]([Math]::Min($Width, $Height) * 0.8)
    $graphics.DrawImage($source, [int](($Width - $size) / 2), [int](($Height - $size) / 2), $size, $size)
    $graphics.Dispose()
    $bitmap.Save((Join-Path "$staging\Assets" $Name), [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}
New-Logo 50 50 'StoreLogo.png'
New-Logo 44 44 'Square44x44Logo.png'
New-Logo 150 150 'Square150x150Logo.png'
$source.Dispose()

$manifest = Get-Content (Join-Path $here 'AppxManifest.template.xml') -Raw -Encoding utf8
$values = @{
    IDENTITY_NAME = $IdentityName; PUBLISHER = $Publisher; VERSION = $Version
    DISPLAY_NAME = $DisplayName; PUBLISHER_DISPLAY_NAME = $PublisherDisplayName
}
foreach ($key in $values.Keys) {
    $manifest = $manifest.Replace("@$key@", [System.Security.SecurityElement]::Escape($values[$key]))
}
[IO.File]::WriteAllText((Join-Path $staging 'AppxManifest.xml'), $manifest, (New-Object System.Text.UTF8Encoding($false)))

& $makeappx.FullName pack /d $staging /p $package /o
if ($LASTEXITCODE -ne 0) { throw 'MSIX の作成に失敗しました。' }
Write-Host "作成しました: $package"
