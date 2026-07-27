# syntax=docker/dockerfile:1

# ---------------------------------------------------------
# Stage 1: Build and publish the ASP.NET Core application
# ---------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY MidisSqlAi.Api.csproj ./

RUN dotnet restore MidisSqlAi.Api.csproj

COPY . ./

RUN dotnet publish MidisSqlAi.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# ---------------------------------------------------------
# Stage 2: Create the smaller runtime image
# ---------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish ./

USER app

ENTRYPOINT ["dotnet", "MidisSqlAi.Api.dll"]