FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/BillingService/BillingService.csproj src/BillingService/
RUN dotnet restore src/BillingService/BillingService.csproj

COPY src/BillingService/ src/BillingService/
RUN dotnet publish src/BillingService/BillingService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "BillingService.dll"]