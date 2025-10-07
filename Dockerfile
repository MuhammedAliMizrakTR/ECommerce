# 1. Build aşaması (.NET SDK ile)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Proje dosyalarını kopyala ve restore et
COPY ./ECommerce/*.csproj ./
RUN dotnet restore

# Tüm kaynakları kopyala ve publish et
COPY ./ECommerce/. ./
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime aşaması (.NET ASP.NET Core)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Yayınlanan dosyaları kopyala
COPY --from=build /app/publish ./

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "ECommerce.dll"]

EXPOSE 3000
