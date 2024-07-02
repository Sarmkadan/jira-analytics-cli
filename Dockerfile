# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Dockerfile for Jira Analytics CLI
# Builds a lightweight container image with the CLI tool
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy project file and restore dependencies
COPY JiraAnalyticsCli.csproj .
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish -c Release -o /app --self-contained false

# Runtime image - small and lightweight
FROM mcr.microsoft.com/dotnet/runtime:10.0

LABEL maintainer="Vladyslav Zaiets <rutova2@gmail.com>"
LABEL description="Jira Analytics CLI - Advanced sprint and team metrics analysis"

WORKDIR /app

# Create non-root user for security
RUN useradd -m -u 1000 analytics
USER analytics

# Copy published app from builder
COPY --from=builder /app .

# Create output directory
RUN mkdir -p /app/output

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD dotnet JiraAnalyticsCli.dll analytics -p TEST -s 1 > /dev/null 2>&1 || exit 1

# Default command - show help
ENTRYPOINT ["dotnet", "JiraAnalyticsCli.dll"]
CMD ["--help"]
