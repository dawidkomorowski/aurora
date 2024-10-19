if (-not $env:AURORA_STORAGE_PATH) {
    Write-Error "Environment variable AURORA_STORAGE_PATH is not defined."
    return
}

docker compose up --build -d