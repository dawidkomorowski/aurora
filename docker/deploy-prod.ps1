if (-not $env:AURORA_STORAGE_PATH) {
    Write-Error "Environment variable AURORA_STORAGE_PATH is not defined."
    return
}

docker compose -f compose.prod.yaml up --build -d