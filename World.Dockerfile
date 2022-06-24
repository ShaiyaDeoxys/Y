FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.16-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 30810/tcp

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.16-amd64 AS build

WORKDIR .
COPY ["src/Imgeneus.World/Imgeneus.World.csproj", "src/Imgeneus.World/"]

RUN dotnet restore "src/Imgeneus.World/Imgeneus.World.csproj"
COPY . .
WORKDIR "/src/Imgeneus.World"
RUN dotnet build "Imgeneus.World.csproj" -c SHAIYA_US -o /app/build

FROM build AS publish
RUN dotnet publish "Imgeneus.World.csproj" -c SHAIYA_US -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Imgeneus.World.dll"]