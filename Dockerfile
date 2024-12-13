#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update \
  && apt-get install --no-install-recommends -y ca-certificates fonts-liberation libasound2 \
  libatk-bridge2.0-0 libatk1.0-0 libc6 libcairo2 libcups2 libdbus-1-3 libexpat1 libfontconfig1 \
  libgbm1 libgcc1 libglib2.0-0 libgtk-3-0 libnspr4 libnss3 libpango-1.0-0 libpangocairo-1.0-0 \
  libstdc++6 libx11-6 libx11-xcb1 libxcb1 libxcomposite1 libxcursor1 libxdamage1 libxext6 \
  libxfixes3 libxi6 libxrandr2 libxrender1 libxss1 libxtst6 lsb-release wget xdg-utils curl \
  && apt-get clean

  USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["./src/Concertify.Domain/Concertify.Domain.csproj", "Concertify.Domain/"]
COPY ["./src/Concertify.Infrastructure/Concertify.Infrastructure.csproj", "Concertify.Infrastructure/"]
COPY ["./src/Concertify.Scraper/Concertify.Scraper.csproj", "Concertify.Scraper/"]
COPY ["./src/Concertify.Application/Concertify.Application.csproj", "Concertify.Application/"]
COPY ["./src/Concertify.API/Concertify.API.csproj", "Concertify.API/"]

RUN dotnet restore "./Concertify.API/Concertify.API.csproj"

COPY "./src/" .
WORKDIR "/src/Concertify.API"
RUN dotnet build "./Concertify.API.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Concertify.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
EXPOSE 80
EXPOSE 443
WORKDIR /app
COPY --from=publish /app/publish .

COPY ["./aspnetapp.pfx", "/https/"]
USER root
RUN chmod +x /app/selenium-manager/linux/selenium-manager
USER app
ENTRYPOINT ["dotnet", "Concertify.API.dll"]