FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080 443

ENV ASPNETCORE_URLS=https://+:443;http://+:8080
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="smk@377040"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["StoreAPI.csproj", "./"]
RUN dotnet restore "StoreAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "StoreAPI.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "StoreAPI.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["aspnetapp.pfx", "/https/"]
ENTRYPOINT ["dotnet", "StoreAPI.dll"]