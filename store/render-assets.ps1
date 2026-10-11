<#
  File: render-assets.ps1
  Description: Renders the MSIX logo images and the Microsoft Store listing art from the official mark SVG
  Author: 5garashi.com Design Office
  Created: 2026-10-11
  License: MIT
  SPDX-License-Identifier: MIT

  Run this after the mark SVG changes; the results are kept in the repository:
    store\assets\   package logos (copied into the MSIX by build-msix.ps1)
    store\listing\  Store listing art (upload in Partner Center)
  The mark is drawn from wwwroot\assets\5garashi_mark_simple.svg, unmodified
  (Identity 2-1, 9-2), at each exact pixel size by Microsoft Edge (headless),
  so no image is enlarged from a smaller one.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = (Resolve-Path (Join-Path $here '..')).Path
$markSvg = Join-Path $root 'wwwroot\assets\5garashi_mark_simple.svg'
$assetsDir = Join-Path $here 'assets'
$listingDir = Join-Path $here 'listing'
$edge = @(
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $edge) { throw 'Microsoft Edge was not found.' }

$work = Join-Path ([IO.Path]::GetTempPath()) ('mdeditor-art-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work, $assetsDir, $listingDir -Force | Out-Null
Copy-Item $markSvg (Join-Path $work 'mark.svg')

# The mark's viewBox is 560 x 450. Its drawn shape spans about 495 x 382 units,
# and one vertical pepper is 380 units long: the clear space on every side
# (Identity 3-5).
$viewW = 560.0; $viewH = 450.0
$paper = '#F9EFCB'; $ink = '#385723'

function Invoke-Render([int]$Width, [int]$Height, [string]$Body, [string]$Background, [string]$OutFile) {
    $html = @"
<!doctype html>
<html><head><meta charset="utf-8"><style>
html, body { margin: 0; width: ${Width}px; height: ${Height}px; overflow: hidden; background: $Background; }
.mark { position: absolute; display: block; }
.name { position: absolute; left: 0; width: 100%; text-align: center; color: $ink;
        font-family: "Segoe UI", sans-serif; white-space: nowrap; }
</style></head><body>$Body</body></html>
"@
    $page = Join-Path $work 'page.html'
    [IO.File]::WriteAllText($page, $html, (New-Object System.Text.UTF8Encoding($false)))
    $profileDir = Join-Path $work ('profile-' + [Guid]::NewGuid().ToString('N'))
    $arguments = @(
        '--headless=new', '--disable-gpu', '--no-first-run', '--hide-scrollbars',
        '--force-device-scale-factor=1', '--default-background-color=00000000',
        "--user-data-dir=$profileDir", "--window-size=$Width,$Height",
        "--screenshot=$OutFile", ([Uri]$page).AbsoluteUri
    )
    $process = Start-Process -FilePath $edge -ArgumentList $arguments -PassThru -WindowStyle Hidden
    if (-not $process.WaitForExit(60000)) { $process.Kill(); throw "Edge did not finish rendering $OutFile" }
    if (-not (Test-Path $OutFile)) { throw "Rendering failed: $OutFile" }
}

# A mark of the given width whose drawn shape is centered at (cx, cy). The
# shape sits 10 units below the middle of the viewBox (its center is y = 235).
function Get-MarkTag([double]$MarkWidth, [double]$CenterX, [double]$CenterY) {
    $w = [Math]::Round($MarkWidth, 2)
    $h = [Math]::Round($MarkWidth * $viewH / $viewW, 2)
    $x = [Math]::Round($CenterX - $w / 2, 2)
    $y = [Math]::Round($CenterY - $h / 2 - 10 * $h / $viewH, 2)
    "<img class=""mark"" src=""mark.svg"" alt="""" style=""left:${x}px;top:${y}px;width:${w}px;height:${h}px"">"
}

# --- Package logos (transparent, volt mark in every display mode: Identity 9-2) ---
$scales = @{ 100 = 1.0; 125 = 1.25; 150 = 1.5; 200 = 2.0; 400 = 4.0 }
# Name, base width, base height, mark width as a share of the image width.
$tiles = @(
    @('Square44x44Logo', 44, 44, 1.0),
    @('StoreLogo', 50, 50, 1.0),
    @('Square71x71Logo', 71, 71, 0.62),
    @('Square150x150Logo', 150, 150, 0.6),
    @('Wide310x150Logo', 310, 150, 0.36),
    @('Square310x310Logo', 310, 310, 0.6)
)
Get-ChildItem $assetsDir -Filter *.png -ErrorAction SilentlyContinue | Remove-Item
foreach ($tile in $tiles) {
    foreach ($scale in $scales.Keys | Sort-Object) {
        $w = [int][Math]::Round($tile[1] * $scales[$scale])
        $h = [int][Math]::Round($tile[2] * $scales[$scale])
        $out = Join-Path $assetsDir "$($tile[0]).scale-$scale.png"
        Invoke-Render $w $h (Get-MarkTag ($w * $tile[3]) ($w / 2) ($h / 2)) 'transparent' $out
    }
}
# Taskbar, Start, and File Explorer sizes, plated and unplated alike.
foreach ($size in 16, 20, 24, 30, 32, 36, 40, 48, 60, 64, 72, 80, 96, 256) {
    $out = Join-Path $assetsDir "Square44x44Logo.targetsize-$size.png"
    Invoke-Render $size $size (Get-MarkTag $size ($size / 2) ($size / 2)) 'transparent' $out
    Copy-Item $out (Join-Path $assetsDir "Square44x44Logo.targetsize-${size}_altform-unplated.png")
    Copy-Item $out (Join-Path $assetsDir "Square44x44Logo.targetsize-${size}_altform-lightunplated.png")
}

# --- Store listing art (paper ground, clear space of one pepper on every side) ---
function New-ListingArt([string]$Name, [int]$Width, [int]$Height, [double]$MarkShare, [bool]$WithName) {
    $markW = $Width * $MarkShare
    if ($Height -lt $Width) { $markW = $Height * $MarkShare * $viewW / $viewH }
    $unit = $markW / $viewW                 # pixels per viewBox unit
    $drawnW = 495 * $unit; $drawnH = 382 * $unit; $pepper = 380 * $unit
    $nameSize = [Math]::Round($Width * 0.07)
    $bySize = [Math]::Round($nameSize * 0.42)
    $textH = $nameSize * 1.25 + $bySize * 1.4
    if ($WithName) {
        # Mark, then one pepper of space, then the product name.
        $blockH = $drawnH + $pepper + $textH
        $top = ($Height - $blockH) / 2
        $markCY = $top + $drawnH / 2
        $nameTop = [Math]::Round($top + $drawnH + $pepper)
        $byTop = [Math]::Round($nameTop + $nameSize * 1.25)
        $body = (Get-MarkTag $markW ($Width / 2) $markCY) +
            "<div class=""name"" style=""top:${nameTop}px;font-size:${nameSize}px;font-weight:800;line-height:1"">mdEditor</div>" +
            "<div class=""name"" style=""top:${byTop}px;font-size:${bySize}px;font-weight:600;line-height:1"">by 5garashi.com</div>"
        $edgeTop = $top
    } else {
        $markCY = $Height / 2
        $body = Get-MarkTag $markW ($Width / 2) $markCY
        $edgeTop = ($Height - $drawnH) / 2
    }
    $sideSpace = ($Width - $drawnW) / 2
    if ($sideSpace -lt $pepper -or $edgeTop -lt $pepper) {
        throw "$Name breaks the clear space (Identity 3-5); reduce the mark size"
    }
    Invoke-Render $Width $Height $body $paper (Join-Path $listingDir "$Name.png")
}
Get-ChildItem $listingDir -Filter *.png -ErrorAction SilentlyContinue | Remove-Item
foreach ($factor in 1, 2) {
    $suffix = if ($factor -eq 1) { '' } else { '@2x' }
    New-ListingArt "boxart-1x1$suffix" (1080 * $factor) (1080 * $factor) 0.28 $true
    New-ListingArt "poster-2x3$suffix" (720 * $factor) (1080 * $factor) 0.38 $true
    New-ListingArt "hero-16x9$suffix" (1920 * $factor) (1080 * $factor) 0.3 $false
}

Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Rendered: $assetsDir"
Write-Host "Rendered: $listingDir"
