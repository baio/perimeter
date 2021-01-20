## Run env

```
docker run --name local-mongo -v c:/dev/data/mongo:/data/db -d --rm -p 27017:27017 mongo
```

## Migrations


```
dotnet ef migrations remove --project Apps/PRR/Data/PRR.Data.DataContextMigrations
dotnet ef migrations add Initial --project Apps/PRR/Data/PRR.Data.DataContextMigrations
dotnet ef database update --project Apps/PRR/Data/PRR.Data.DataContextMigrations
```

## Infra

AzureStorageEmulator for akka persistence 


## Tests

Run tests with Test configuration !
```
dotnet test --filter "FullyQualifiedName~sign-in-api" -c Test
```

# Observability

```
# tracing with jaeger
docker run --name jaeger -d --restart unless-stopped -p 6831:6831/udp -p 6832:6832/udp -p 16686:16686 jaegertracing/all-in-one:1.7 --log-level=debug
# logging with seq
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
# merics with prometheus
# WARNING ! Current directoty file ref https://stackoverflow.com/questions/41485217/mount-current-directory-as-a-volume-in-docker-on-windows-10
# WARNING Connect localhost from dockercontainer https://stackoverflow.com/questions/24319662/from-inside-of-a-docker-container-how-do-i-connect-to-the-localhost-of-the-mach
docker run --name prometheus --add-host prr-api:192.168.0.104 -v ${pwd}/prometheus.local.yml:/etc/prometheus/prometheus.yml -d --restart unless-stopped -p 9090:9090 prom/prometheus                        
```

