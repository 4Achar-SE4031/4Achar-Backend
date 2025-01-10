#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 80
EXPOSE 443

  USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

USER root

COPY ["./src/Concertify.Domain/Concertify.Domain.csproj", "Concertify.Domain/"]
COPY ["./src/Concertify.Infrastructure/Concertify.Infrastructure.csproj", "Concertify.Infrastructure/"]
COPY ["./src/Concertify.Application/Concertify.Application.csproj", "Concertify.Application/"]
COPY ["./src/Concertify.API/Concertify.API.csproj", "Concertify.API/"]

RUN dotnet restore "./Concertify.API/Concertify.API.csproj"

COPY "./src/" .
WORKDIR "/src/Concertify.API"
RUN dotnet build "./Concertify.API.csproj" -c Release -o /app/build

FROM build AS develop
EXPOSE 80
EXPOSE 443

USER root

COPY ["./aspnetapp.pfx", "/https/"]

RUN mkdir ./wwwroot
RUN mkdir ./wwwroot/images
RUN mkdir ./wwwroot/images/concert
RUN chmod -R 777 /src/Concertify.API/wwwroot

CMD dotnet run --no-launch-profile

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
USER root
RUN dotnet publish "./Concertify.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
EXPOSE 80
EXPOSE 443
WORKDIR /app
COPY --from=publish /app/publish .

COPY ["./aspnetapp.pfx", "/https/"]
USER root


RUN mkdir ./wwwroot
RUN mkdir ./wwwroot/images
RUN mkdir ./wwwroot/images/concert
RUN chmod -R 777 /app/wwwroot

USER app
ENTRYPOINT ["dotnet", "Concertify.API.dll"]