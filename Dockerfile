# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copia o .csproj especificando a pasta de destino com a barra no final
COPY ApiFestaJulina/ApiFestaJulina.csproj ./ApiFestaJulina/

# 2. CORREÇÃO: Adicionada a barra '/' após o nome da pasta para separar do arquivo
RUN dotnet restore ApiFestaJulina/ApiFestaJulina.csproj

COPY . .

# 3. CORREÇÃO: Adicionada a barra '/' aqui também para o publish encontrar o arquivo correto
RUN dotnet publish ApiFestaJulina/ApiFestaJulina.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiFestaJulina.dll"]