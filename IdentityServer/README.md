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
docker run --name jaeger -d --restart unless-stopped -p 6831:6831/udp -p 6832:6832/udp -p 16686:16686 jaegertracing/all-in-one:1.7 --log-level=debug
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

```

