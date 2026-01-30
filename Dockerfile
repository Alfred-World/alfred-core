# ============================================
# Build Stage - Compile ứng dụng
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (tận dụng Docker layer caching)
COPY ["src/Alfred.Core.Domain/Alfred.Core.Domain.csproj", "src/Alfred.Core.Domain/"]
COPY ["src/Alfred.Core.Application/Alfred.Core.Application.csproj", "src/Alfred.Core.Application/"]
COPY ["src/Alfred.Core.Infrastructure/Alfred.Core.Infrastructure.csproj", "src/Alfred.Core.Infrastructure/"]
COPY ["src/Alfred.Core.WebApi/Alfred.Core.WebApi.csproj", "src/Alfred.Core.WebApi/"]
COPY ["src/Alfred.Core.Cli/Alfred.Core.Cli.csproj", "src/Alfred.Core.Cli/"]

# Restore dependencies for each project (skip test projects)
RUN dotnet restore "src/Alfred.Core.WebApi/Alfred.Core.WebApi.csproj"
RUN dotnet restore "src/Alfred.Core.Cli/Alfred.Core.Cli.csproj"

# Copy toàn bộ source code (excluding tests via .dockerignore)
COPY . .

# Build ứng dụng
WORKDIR "/src/src/Alfred.Core.WebApi"
RUN dotnet build "Alfred.Core.WebApi.csproj" -c Release -o /app/build

# Build CLI tool
WORKDIR "/src/src/Alfred.Core.Cli"
RUN dotnet build "Alfred.Core.Cli.csproj" -c Release -o /app/cli

# ============================================
# Publish Stage - Tạo artifact để deploy
# ============================================
FROM build AS publish
WORKDIR "/src/src/Alfred.Core.WebApi"
RUN dotnet publish "Alfred.Core.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

WORKDIR "/src/src/Alfred.Core.Cli"
RUN dotnet publish "Alfred.Core.Cli.csproj" -c Release -o /app/publish/cli /p:UseAppHost=false

# ============================================
# Final Stage - Image runtime siêu nhẹ
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Tạo non-root user để bảo mật
RUN addgroup --system --gid 1001 alfred && \
    adduser --system --uid 1001 --ingroup alfred alfred

# Copy artifact từ publish stage
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/cli ./cli

# Đổi ownership cho user alfred
RUN chown -R alfred:alfred /app

# Switch sang user alfred (không dùng root)
USER alfred

# Expose port
EXPOSE 8200

# Health check using wget (available by default in aspnet image)
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8200/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Alfred.Core.WebApi.dll"]
