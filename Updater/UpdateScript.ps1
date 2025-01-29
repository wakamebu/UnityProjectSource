param(
    [string]$UpdateUrl,  # $args[0]
    [string]$InstallDir, # $args[1]
    [string]$GameExe     # $args[2]
)

Write-Host "=== PowerShell Updater ==="
Write-Host "UpdateUrl   = $UpdateUrl"
Write-Host "InstallDir  = $InstallDir"
Write-Host "GameExe     = $GameExe"
Write-Host ""

# プロセス名 (拡張子 .exe を除去)
$gameProcessName = [System.IO.Path]::GetFileNameWithoutExtension($GameExe)

# -----------------------------------------------------------
# (A) まずゲームが起動していないか確認し、起動中なら待機する
# -----------------------------------------------------------
while ($true) {
    # 指定したプロセスが動いているかチェック (見つからなければ $null)
    $proc = Get-Process -Name $gameProcessName -ErrorAction SilentlyContinue

    if ($proc) {
        Write-Host "ゲームが起動しています。終了してください。"
        Write-Host "5秒後に再チェックします..."
        Start-Sleep -Seconds 5
    }
    else {
        Write-Host "ゲームは起動していません。アップデートを続行できます。"
        break
    }
}

# -----------------------------------------------------------
# (B) ユーザーにアップデート実行の意思を確認
# -----------------------------------------------------------
$answer = Read-Host "アップデートを実行しますか？ (y/n)"
if ($answer -ne "y") {
    Write-Host "キャンセルしました。何かキーを押すと終了します。"
    [Console]::ReadKey() | Out-Null
    return
}

# -----------------------------------------------------------
# (C) ダウンロード＆解凍
# -----------------------------------------------------------
Write-Host "`nダウンロード中..."

$TempZipPath = Join-Path $env:TEMP "update.zip"
if (Test-Path $TempZipPath) {
    Remove-Item $TempZipPath -Force
}


$UpdateUrl = $UpdateUrl -replace '^https://', 'http://'

Invoke-WebRequest -Uri $UpdateUrl -OutFile $TempZipPath

Write-Host "ダウンロード完了: $TempZipPath"

Write-Host "解凍して上書きします: $InstallDir"
Expand-Archive -Path $TempZipPath -DestinationPath $InstallDir -Force
Write-Host "完了しました。"
Write-Host ""

# -----------------------------------------------------------
# (D) ゲームを再起動するかを尋ねる
# -----------------------------------------------------------
$reanswer = Read-Host "ゲームを再起動しますか？ (y/n)"
if ($reanswer -eq "y") {
    $exePath = Join-Path $InstallDir $GameExe
    Write-Host "ゲームを起動します: $exePath"
    Start-Process $exePath
}

Write-Host "処理が完了しました。何かキーを押すとこのウィンドウは閉じます。"
[Console]::ReadKey() | Out-Null
