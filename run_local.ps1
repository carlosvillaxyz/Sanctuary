# Starts the local asset server and the three Sanctuary servers (SQLite) in their own windows, then launches the client.
# Usage:  .\run_local.ps1            -> servers + client (login flow, account test/testtest)
#         .\run_local.ps1 -NoClient  -> servers only
param([switch]$NoClient)

$repo = $PSScriptRoot
$src  = Join-Path $repo "src"

$env:Database__Provider         = "Sqlite"
$env:Database__ConnectionString = "Data Source=$repo\sanctuary.db"
$env:DOTNET_ENVIRONMENT         = "Development"
$env:ASPNETCORE_ENVIRONMENT     = "Development"
$env:DOTNET_ROLL_FORWARD        = "Major"   # lets net9.0 apps run on the .NET 10 runtime if the 9.0 runtime is missing

dotnet build "$src\Sanctuary.slnx" -c Debug --nologo -v q
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed"; exit 1 }

# Local asset-delivery server (caches streamed client assets, serves overrides). See tools\asset-server\README.md
$assetServer = Join-Path $repo "tools\asset-server\server.py"
Start-Process python -ArgumentList "`"$assetServer`"" -WorkingDirectory $repo -WindowStyle Normal

foreach ($p in "Login", "Gateway", "WebAPI") {
    $dll = "$src\Sanctuary.$p\bin\Debug\net9.0\Sanctuary.$p.dll"
    Start-Process dotnet -ArgumentList "`"$dll`"" -WorkingDirectory $src -WindowStyle Normal
    Start-Sleep -Seconds 4   # Login must be up before Gateway connects to it
}

if (-not $NoClient) {
    Start-Sleep -Seconds 4
    python "$repo\run_client.py"
}
