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