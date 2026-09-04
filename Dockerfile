# Multi-stage Dockerfile for SchemaNote (ASP.NET Core Razor Pages, .NET 8)
# Place this file at repository root and build with: docker build -t schemanote:local .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY *.sln .
COPY SchemaNote/SchemaNote.csproj SchemaNote/

RUN dotnet restore

# Copy everything and publish
COPY . .
WORKDIR /src/SchemaNote
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 控制是否在映像內部降低 OpenSSL 安全性（僅在極受控的測試環境使用）
ARG INSECURE=0
RUN if [ "$INSECURE" = "1" ]; then \
    echo "*** WARNING: applying insecure OpenSSL settings (for testing only) ***"; \
    cp /etc/ssl/openssl.cnf /etc/ssl/openssl.cnf.bak && \
    printf 'openssl_conf = default_conf\n\n[default_conf]\nssl_conf = ssl_sect\n\n[ssl_sect]\nsystem_default = system_default_sect\n\n[system_default_sect]\nMinProtocol = TLSv1\nCipherString = DEFAULT:@SECLEVEL=0\nOptions = UnsafeLegacyRenegotiation\n' > /etc/ssl/openssl.cnf && \
    cat /etc/ssl/openssl.cnf; \
fi

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "SchemaNote.dll"]
