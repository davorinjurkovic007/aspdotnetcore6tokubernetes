#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["frontend/frontend.csproj", "frontend/"]
RUN dotnet restore "frontend/frontend.csproj"
COPY . .
WORKDIR "/src/frontend"
RUN dotnet build "frontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "frontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "frontend.dll"]