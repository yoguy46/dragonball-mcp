FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj, para que la restauración de paquetes se guarde en caché
COPY Modelo/Modelo.csproj Modelo/
COPY Repositorio/Repositorio.csproj Repositorio/
COPY BackendController/BackendController.csproj BackendController/
RUN dotnet restore BackendController/BackendController.csproj

COPY Modelo/ Modelo/
COPY Repositorio/ Repositorio/
COPY BackendController/ BackendController/
RUN dotnet publish BackendController/BackendController.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BackendController.dll"]
