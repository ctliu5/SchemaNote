# SchemaNote 的多階段 (multi-stage) Dockerfile（ASP.NET Core Razor Pages，.NET 8）
# 此 Dockerfile 預設在 SchemaNote 專案資料夾中作為 build context 使用。
# 從 SchemaNote 專案目錄建置：
#   docker build -f Dockerfiles/Dockerfile -t schemanote:local .
# 或從方案根目錄建置（再指定上下文為 SchemaNote之作法）：
#   docker build -f SchemaNote/Dockerfiles/Dockerfile -t schemanote:local SchemaNote

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 先只複製專案檔以善用 Docker layer 快取
COPY SchemaNote.csproj ./
RUN dotnet restore "./SchemaNote.csproj"

# 複製其餘來源並進行發行 (publish)
COPY . ./
RUN dotnet publish "./SchemaNote.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.11-noble-chiseled-extra-amd64 AS runtime
WORKDIR /app

# 控制是否在映像內部降低 OpenSSL 安全性（僅在極受控的測試環境使用）
# 若想啟用，請在 docker build 命令中添加 --build-arg INSECURE=1
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
