$serviceName = "aurora.services.issues-service"

$output = docker container list -a --filter "NAME=$($serviceName)"
if ($output -match $serviceName) {
    docker container remove $serviceName
}

docker build -t "$($serviceName):latest" .
docker run --name $serviceName -d --env AURORA_ISSUES_DB_PATH "$($serviceName):latest" .