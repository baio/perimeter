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