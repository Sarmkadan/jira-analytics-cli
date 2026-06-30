# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =====================================================================
# Dockerfile for Jira Analytics CLI
# Multi-stage build for production deployment
# =====================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy project file and restore dependencies
COPY JiraAnalyticsCli.csproj .
RUN dotnet restore --locked-mode

# Copy source code
COPY . .

# Build and publish as self-contained for optimal performance
RUN dotnet publish JiraAnalyticsCli.csproj -c Release -o /app \
    -r linux-x64 \
    --self-contained true \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:EnableCompressionInSingleFile=true

# Runtime stage - minimal image
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine

LABEL maintainer="Vladyslav Zaiets <rutova2@gmail.com>"
LABEL description="Jira Analytics CLI - Advanced sprint and team metrics analysis"
LABEL version="1.0.0"
LABEL com.github.actions.icon="terminal"
LABEL com.github.actions.color="blue"

WORKDIR /app

# Create non-root user for security
RUN addgroup -S analytics && adduser -S -G analytics analytics
USER analytics

# Copy published app from builder
COPY --from=builder /app /app/

# Create output directory with proper permissions
RUN mkdir -p /app/output && chown analytics:analytics /app/output

# Health check - verify CLI can execute basic commands
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD [ -x /app/JiraAnalyticsCli ] && /app/JiraAnalyticsCli --version > /dev/null 2>&1 || exit 1

# Default command - show help
ENTRYPOINT ["/app/JiraAnalyticsCli"]
CMD ["--help"]
