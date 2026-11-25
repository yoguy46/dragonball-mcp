FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Modelo/Modelo.csproj Modelo/
COPY MCPServer/MCPServer.csproj MCPServer/
RUN dotnet restore MCPServer/MCPServer.csproj

COPY Modelo/ Modelo/
COPY MCPServer/ MCPServer/
RUN dotnet publish MCPServer/MCPServer.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MCPServer.dll"]
