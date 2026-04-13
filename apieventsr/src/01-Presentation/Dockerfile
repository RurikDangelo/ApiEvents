#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["01-Presentation/apieventsr.Api.csproj", "01-Presentation/"]
COPY ["04-Infra/4.1-CrossCutting/IoC/apieventsr.IoC.csproj", "04-Infra/4.1-CrossCutting/IoC/"]
COPY ["02-Application/apieventsr.Application.csproj", "02-Application/"]
COPY ["03-Domain/apieventsr.Domain.csproj", "03-Domain/"]
COPY ["04-Infra/4.2-Data/apieventsr.Data.csproj", "04-Infra/4.2-Data/"]
COPY ["04-Infra/4.1-CrossCutting/ControllerHandler/apieventsr.ControllerHandler.csproj", "04-Infra/4.1-CrossCutting/ControllerHandler/"]
RUN dotnet restore "./01-Presentation/apieventsr.Api.csproj"
COPY . .
WORKDIR "/src/01-Presentation"
RUN dotnet build "./apieventsr.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./apieventsr.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "apieventsr.Api.dll"]