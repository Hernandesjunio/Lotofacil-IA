FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["LotofacilMcp.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/LotofacilMcp.Domain/LotofacilMcp.Domain.csproj", "src/LotofacilMcp.Domain/"]
COPY ["src/LotofacilMcp.Application/LotofacilMcp.Application.csproj", "src/LotofacilMcp.Application/"]
COPY ["src/LotofacilMcp.Infrastructure/LotofacilMcp.Infrastructure.csproj", "src/LotofacilMcp.Infrastructure/"]
COPY ["src/LotofacilMcp.Server/LotofacilMcp.Server.csproj", "src/LotofacilMcp.Server/"]

RUN dotnet restore "src/LotofacilMcp.Server/LotofacilMcp.Server.csproj"

COPY . .

RUN dotnet publish "src/LotofacilMcp.Server/LotofacilMcp.Server.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LotofacilMcp.Server.dll"]
