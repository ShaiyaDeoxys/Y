FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.16-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 30800/tcp

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.16-amd64 AS build

WORKDIR .
COPY ["src/Imgeneus.Login/Imgeneus.Login.csproj", "src/Imgeneus.Login/"]

RUN dotnet restore "src/Imgeneus.Login/Imgeneus.Login.csproj"
COPY . .
WORKDIR "/src/Imgeneus.Login"
RUN dotnet build "Imgeneus.Login.csproj" -c SHAIYA_US -o /app/build

FROM build AS publish
RUN dotnet publish "Imgeneus.Login.csproj" -c SHAIYA_US -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Imgeneus.Login.dll"]