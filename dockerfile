# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala
COPY ["Ecommerce.csproj", "./"]
RUN dotnet restore "Ecommerce.csproj"

# Tüm kaynak kodları kopyala
COPY . .

# Projeyi build et
RUN dotnet build "Ecommerce.csproj" -c Release -o /app/build

# Publish aşaması
FROM build AS publish
RUN dotnet publish "Ecommerce.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Gerekli paketleri yükle
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Publish edilen dosyaları kopyala
COPY --from=publish /app/publish .

# Render'ın kullandığı PORT environment variable'ını dinle
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
ENV ASPNETCORE_ENVIRONMENT=Production

# Port'u expose et (Render dinamik port atar)
EXPOSE ${PORT:-5000}

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "Ecommerce.dll"]
