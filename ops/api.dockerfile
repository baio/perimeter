FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY Apps/PRR/Data/PRR.Data.DataContext/PRR.Data.DataContext.csproj Apps/PRR/Data/PRR.Data.DataContext/
COPY Apps/PRR/Data/PRR.Data.DataContextMigrations/PRR.Data.DataContextMigrations.csproj Apps/PRR/Data/PRR.Data.DataContextMigrations/
COPY Apps/PRR/Data/PRR.Data.Entities/PRR.Data.Entities.csproj Apps/PRR/Data/PRR.Data.Entities/
COPY Apps/PRR/Domain/PRR.Domain.Auth/PRR.Domain.Auth.fsproj Apps/PRR/Domain/PRR.Domain.Auth/
COPY Apps/PRR/Domain/PRR.Domain.Tenant/PRR.Domain.Tenant.fsproj Apps/PRR/Domain/PRR.Domain.Tenant/
COPY Apps/PRR/PRR.API/PRR.API.fsproj Apps/PRR/PRR.API/
COPY Apps/PRR/PRR.API.Tests/PRR.API.Tests.fsproj Apps/PRR/PRR.API.Tests/
COPY Apps/PRR/System/PRR.System/PRR.System.fsproj Apps/PRR/System/PRR.System/
COPY Apps/PRR/System/PRR.System.Models/PRR.System.Models.fsproj Apps/PRR/System/PRR.System.Models/
COPY Common/Common.Utils/Common.Utils.fsproj Common/Common.Utils/
COPY Common/Common.Test.Utils/Common.Test.Utils.fsproj Common/Common.Test.Utils/
COPY Common/Domain/Common.Domain.Giraffe/Common.Domain.Giraffe.fsproj Common/Domain/Common.Domain.Giraffe/
COPY Common/Domain/Common.Domain.Models/Common.Domain.Models.fsproj Common/Domain/Common.Domain.Models/
COPY Common/Domain/Common.Domain.Utils/Common.Domain.Utils.fsproj Common/Domain/Common.Domain.Utils/

RUN dotnet restore Perimeter.sln

# copy everything else and build app
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "PRR.API.dll"]