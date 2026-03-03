# ============================================
# Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/Alfred.Core.Domain/Alfred.Core.Domain.csproj", "src/Alfred.Core.Domain/"]
COPY ["src/Alfred.Core.Application/Alfred.Core.Application.csproj", "src/Alfred.Core.Application/"]
COPY ["src/Alfred.Core.Infrastructure/Alfred.Core.Infrastructure.csproj", "src/Alfred.Core.Infrastructure/"]
COPY ["src/Alfred.Core.WebApi/Alfred.Core.WebApi.csproj", "src/Alfred.Core.WebApi/"]
COPY ["src/Alfred.Core.Cli/Alfred.Core.Cli.csproj", "src/Alfred.Core.Cli/"]

RUN --mount=type=cache,id=nuget-core,target=/root/.nuget/packages \
    dotnet restore "src/Alfred.Core.WebApi/Alfred.Core.WebApi.csproj" && \
    dotnet restore "src/Alfred.Core.Cli/Alfred.Core.Cli.csproj"

COPY . .

# ============================================
# Publish Stage
# ============================================
FROM build AS publish
RUN --mount=type=cache,id=nuget-core,target=/root/.nuget/packages \
    dotnet publish "src/Alfred.Core.WebApi/Alfred.Core.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false && \
    dotnet publish "src/Alfred.Core.Cli/Alfred.Core.Cli.csproj" -c Release -o /app/publish/cli /p:UseAppHost=false

# ============================================
# Final Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN apk --no-cache add curl libgcc libstdc++ icu-libs

RUN addgroup -S -g 1001 alfred && adduser -S -u 1001 -G alfred -H alfred

COPY --from=publish --chown=alfred:alfred /app/publish .
COPY --from=publish --chown=alfred:alfred /app/publish/cli ./cli

USER alfred

EXPOSE 8200

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8200/health || exit 1

ENTRYPOINT ["dotnet", "Alfred.Core.WebApi.dll"]
