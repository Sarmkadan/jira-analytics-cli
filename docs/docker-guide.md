# Docker Guide for jira-analytics-cli v2.0

This guide provides comprehensive instructions for running **jira-analytics-cli** in Docker containers, including production deployment best practices.

---

## 📋 Table of Contents

- [Quick Start with Docker](#quick-start-with-docker)
- [Docker Compose Setup](#docker-compose-setup)
- [Environment Variables Reference](#environment-variables-reference)
- [Production Deployment Checklist](#production-deployment-checklist)
- [Advanced Configuration](#advanced-configuration)
- [Volume Management](#volume-management)
- [Network Configuration](#network-configuration)
- [Security Considerations](#security-considerations)
- [Performance Optimization](#performance-optimization)
- [Troubleshooting](#troubleshooting)

---

## ⚡ Quick Start with Docker

### Prerequisites

- ✅ Docker installed ([Install Docker](https://docs.docker.com/get-docker/))
- ✅ Docker Compose (optional, for multi-container setups)
- ✅ Jira API token
- ✅ Jira instance URL

### 1. Pull the Image (or Build Locally)

```bash
# Option A: Pull from Docker Hub (if published)
docker pull sarmkadan/jira-analytics-cli:latest

# Option B: Build locally from source
git clone https://github.com/sarmkadan/jira-analytics-cli.git
cd jira-analytics-cli
docker build -t jira-analytics-cli .
```

### 2. Run Container with Environment Variables

```bash
docker run --rm \
  -e JIRA_BASE_URL="https://your-instance.atlassian.net" \
  -e JIRA_API_TOKEN="your-api-token" \
  -e JIRA_DEFAULT_PROJECT="MYPROJECT" \
  -v $(pwd)/output:/app/output \
  jira-analytics-cli \
  analytics -p MYPROJECT -s 5 -o /app/output/report.txt
```

### 3. Verify Output

```bash
# Check generated report
cat output/report.txt

# List output files
ls -la output/
```

---

## 🐳 Docker Compose Setup

### Basic Setup

Create a `docker-compose.yml` file:

```yaml
version: '3.8'

services:
  jira-analytics:
    build: .
    image: jira-analytics-cli
    container_name: jira-analytics-cli
    restart: unless-stopped
    environment:
      - JIRA_BASE_URL=https://your-instance.atlassian.net
      - JIRA_API_TOKEN=${JIRA_API_TOKEN}
      - JIRA_DEFAULT_PROJECT=MYPROJECT
      - CACHE_EXPIRATION_MINUTES=30
      - ENABLE_DETAILED_LOGGING=false
    volumes:
      - ./output:/app/output
      - ./dashboards:/app/dashboards
      - ./config:/app/config
    networks:
      - jira-network
    # Health check (optional)
    healthcheck:
      test: ["CMD", "dotnet", "--list-runtimes"]
      interval: 30s
      timeout: 10s
      retries: 3

networks:
  jira-network:
    driver: bridge
```

### Using Environment File

Create a `.env` file:

```bash
# .env
JIRA_BASE_URL=https://your-instance.atlassian.net
JIRA_API_TOKEN=your-api-token-here
JIRA_DEFAULT_PROJECT=MYPROJECT
CACHE_EXPIRATION_MINUTES=30
```

Update `docker-compose.yml`:

```yaml
services:
  jira-analytics:
    # ... other config ...
    env_file:
      - .env
```

### Run with Docker Compose

```bash
# Start the service
docker-compose up -d

# Check logs
docker-compose logs -f jira-analytics

# Execute commands
# Method 1: Use docker-compose exec
docker-compose exec jira-analytics \
  jira-analytics-cli analytics -p MYPROJECT -s 5

# Method 2: Run new container with same config
docker-compose run --rm jira-analytics \
  analytics -p MYPROJECT -s 5 -o /app/output/report.txt

# Stop the service
docker-compose down
```

---

## 🌍 Environment Variables Reference

### Required Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `JIRA_BASE_URL` | Base URL of your Jira instance | `https://your-company.atlassian.net` |
| `JIRA_API_TOKEN` | Jira API token for authentication | `ABC123...XYZ` |

### Jira Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `JIRA_DEFAULT_PROJECT` | Default project to analyze | - | `MYPROJECT` |
| `JIRA_REQUEST_TIMEOUT_SECONDS` | HTTP request timeout | `30` | `60` |
| `JIRA_MAX_RETRIES` | Maximum retry attempts | `3` | `5` |
| `JIRA_USE_SSL` | Force HTTPS (true/false) | `true` | `true` |

### Caching Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `CACHE_EXPIRATION_MINUTES` | Cache expiration time | `15` | `30` |
| `CACHE_MAX_ITEMS` | Maximum cache items | `1000` | `5000` |
| `CACHE_ENABLED` | Enable/disable caching | `true` | `true` |

### Dashboard Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `DASHBOARD_DEFAULT_LAYOUT` | Default dashboard layout | `team-overview` | `project-overview` |
| `DASHBOARD_AUTOSAVE_LAYOUT` | Auto-save dashboard changes | `true` | `true` |
| `WIDGET_VELOCITY_COLOR_SCHEME` | Velocity chart colors | `blue` | `green` |
| `WIDGET_DEVELOPER_LOAD_SHOW_RANKINGS` | Show developer rankings | `true` | `true` |

### Performance Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `MAX_CONCURRENT_REQUESTS` | Max concurrent API requests | `5` | `10` |
| `ENABLE_METRICS_COLLECTION` | Enable performance metrics | `true` | `true` |
| `ENABLE_DETAILED_LOGGING` | Enable detailed logging | `false` | `true` |

### Export Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `DEFAULT_SPRINT_COUNT` | Default sprints to analyze | `5` | `10` |
| `EXPORT_FORMAT` | Default export format | `json` | `csv` |

### Complete Example

```bash
export JIRA_BASE_URL="https://your-company.atlassian.net"
export JIRA_API_TOKEN="ATATT3xFfGF..."
export JIRA_DEFAULT_PROJECT="ENGINEERING"
export JIRA_REQUEST_TIMEOUT_SECONDS=60
export CACHE_EXPIRATION_MINUTES=30

export DASHBOARD_DEFAULT_LAYOUT="team-overview"
export MAX_CONCURRENT_REQUESTS=10

export ENABLE_DETAILED_LOGGING=false
```

---

## 🏭 Production Deployment Checklist

### 1. Security Hardening

- [ ] ✅ Use HTTPS for Jira URL
- [ ] ✅ Store API token in secrets manager (not in .env files)
- [ ] ✅ Set appropriate file permissions on volumes
- [ ] ✅ Use non-root user in Docker container
- [ ] ✅ Enable Docker content trust
- [ ] ✅ Regularly rotate API tokens

### 2. Resource Configuration

- [ ] ✅ Set CPU limits: `--cpus=2`
- [ ] ✅ Set memory limits: `--memory=2g`
- [ ] ✅ Configure swap appropriately
- [ ] ✅ Set restart policy: `--restart=unless-stopped`

### 3. Data Persistence

- [ ] ✅ Configure output volume for reports
- [ ] ✅ Configure dashboard volume for saved layouts
- [ ] ✅ Configure config volume for custom settings
- [ ] ✅ Set appropriate backup strategy for volumes

### 4. Monitoring & Logging

- [ ] ✅ Configure centralized logging (ELK, Loki, etc.)
- [ ] ✅ Set up health checks
- [ ] ✅ Configure log rotation
- [ ] ✅ Set up monitoring for:
  - Container restarts
  - CPU/Memory usage
  - API response times
  - Cache hit/miss ratios

### 5. Network Configuration

- [ ] ✅ Configure internal Docker network
- [ ] ✅ Set up proper DNS resolution
- [ ] ✅ Configure proxy settings if behind corporate firewall
- [ ] ✅ Set up network policies if using Kubernetes

### 6. Scheduling & Automation

- [ ] ✅ Set up cron jobs or scheduled tasks
- [ ] ✅ Configure proper error handling and retries
- [ ] ✅ Set up alerting for failures
- [ ] ✅ Configure notification system (email, Slack, etc.)

### 7. Backup & Disaster Recovery

- [ ] ✅ Regular volume backups
- [ ] ✅ Test restore procedures
- [ ] ✅ Document recovery steps
- [ ] ✅ Set up multi-region deployment if needed

---

## 🔧 Advanced Configuration

### Multi-Stage Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Create non-root user
RUN adduser -D appuser
USER appuser

# Set entry point
ENTRYPOINT ["dotnet", "jira-analytics-cli.dll"]
```

### Custom Entrypoint Script

Create `entrypoint.sh`:

```bash
#!/bin/sh
set -e

# Wait for Jira to be available (if needed)
if [ -n "$JIRA_WAIT_FOR" ]; then
  echo "Waiting for Jira at $JIRA_WAIT_FOR..."
  while ! curl -s -o /dev/null -I -w "%{http_code}" "$JIRA_WAIT_FOR" | grep -q "200"; do
    sleep 5
  done
echo "Jira is available!"
fi

# Run the actual command
exec "$@"
```

Update Dockerfile:

```dockerfile
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh
ENTRYPOINT ["/app/entrypoint.sh"]
CMD ["dotnet", "jira-analytics-cli.dll"]
```

### Using Custom Configuration Files

```yaml
# docker-compose.yml
services:
  jira-analytics:
    # ... other config ...
    volumes:
      - ./config/appsettings.json:/app/appsettings.json:ro
      - ./config/appsettings.Production.json:/app/appsettings.Production.json
```

Create `config/appsettings.Production.json`:

```json
{
  "jiraConfiguration": {
    "baseUrl": "https://your-instance.atlassian.net",
    "apiToken": "${JIRA_API_TOKEN}",
    "defaultProject": "MYPROJECT",
    "requestTimeoutSeconds": 60,
    "maxRetries": 5
  },
  "caching": {
    "expirationMinutes": 30,
    "maxItems": 5000
  },
  "dashboard": {
    "defaultLayout": "team-overview",
    "autoSaveLayout": true
  }
}
```

### Health Check Configuration

```yaml
# docker-compose.yml
services:
  jira-analytics:
    # ... other config ...
    healthcheck:
      test: ["CMD", "dotnet", "--list-runtimes"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Resource Limits

```bash
docker run --rm \
  --cpus=2 \
  --memory=2g \
  --memory-swap=2g \
  --cpus-quota=50000 \
  --cpus-period=100000 \
  jira-analytics-cli \
  analytics -p MYPROJECT -s 5
```

---

## 💾 Volume Management

### Output Volume

```yaml
services:
  jira-analytics:
    volumes:
      - ./output:/app/output
```

**Best Practices:**
- Use named volumes for production: `volume_name:/app/output`
- Set appropriate permissions: `chmod 755 output`
- Configure log rotation for output files
- Set up automated cleanup for old reports

### Dashboard Volume

```yaml
services:
  jira-analytics:
    volumes:
      - dashboards:/app/dashboards
```

**Best Practices:**
- Use named volumes for dashboards
- Backup dashboard configurations regularly
- Set appropriate retention policies
- Share dashboards across team members

### Config Volume

```yaml
services:
  jira-analytics:
    volumes:
      - ./config:/app/config
```

**Best Practices:**
- Use read-only mounts for production: `:ro`
- Separate configs by environment
- Use secrets management for sensitive data
- Version control configuration files

---

## 🌐 Network Configuration

### Internal Docker Network

```yaml
version: '3.8'

services:
  jira-analytics:
    # ... config ...
    networks:
      - jira-network

networks:
  jira-network:
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16
```

### External Network Access

```bash
# Allow host network access
docker run --rm --network=host jira-analytics-cli analytics -p MYPROJECT
```

### Custom DNS Configuration

```yaml
services:
  jira-analytics:
    dns:
      - 8.8.8.8
      - 8.8.4.4
    dns_search:
      - your-company.local
```

### Proxy Configuration

```bash
docker run --rm \
  -e HTTP_PROXY="http://proxy.your-company.com:8080" \
  -e HTTPS_PROXY="http://proxy.your-company.com:8080" \
  -e NO_PROXY="localhost,127.0.0.1,.your-company.local" \
  jira-analytics-cli \
  analytics -p MYPROJECT
```

---

## 🔒 Security Considerations

### Secrets Management

**Never store API tokens in Dockerfiles or plain text files!**

**Option 1: Docker Secrets (Swarm Mode)**

```bash
# Create secret
echo "your-api-token" | docker secret create jira_api_token -

# Use in compose
services:
  jira-analytics:
    secrets:
      - jira_api_token

secrets:
  jira_api_token:
    external: true
```

**Option 2: Environment Files with Restricted Permissions**

```bash
# Set strict permissions
chmod 600 .env
```

**Option 3: Kubernetes Secrets (if using K8s)**

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: jira-secrets
stringData:
  api-token: "your-api-token"
```

### File Permissions

```bash
# Set appropriate permissions
chmod 750 output/
chown 1000:1000 output/  # For non-root user
```

### Network Security

```yaml
services:
  jira-analytics:
    networks:
      - internal-network
    # Don't expose ports to host unless necessary
    # ports:
    #   - "8080:80"  # Only if running a web server
```

### Regular Updates

```bash
# Update base images regularly
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/aspnet:10.0

# Or use watchtower for automatic updates
```

---

## ⚡ Performance Optimization

### CPU Optimization

```bash
# Limit CPU usage
docker run --rm --cpus=2 jira-analytics-cli analytics -p MYPROJECT

# Set CPU quota (50% of CPU)
docker run --rm --cpus=0.5 jira-analytics-cli analytics -p MYPROJECT
```

### Memory Optimization

```bash
# Set memory limits
docker run --rm --memory=2g --memory-swap=2g jira-analytics-cli analytics -p MYPROJECT

# Enable memory swappiness control
docker run --rm --memory-swappiness=10 jira-analytics-cli analytics -p MYPROJECT
```

### Parallel Processing

```bash
# Increase concurrent requests
export MAX_CONCURRENT_REQUESTS=10

docker run --rm -e MAX_CONCURRENT_REQUESTS=10 jira-analytics-cli analytics -p MYPROJECT
```

### Cache Optimization

```bash
# Increase cache size for large projects
export CACHE_MAX_ITEMS=10000
export CACHE_EXPIRATION_MINUTES=60

docker run --rm \
  -e CACHE_MAX_ITEMS=10000 \
  -e CACHE_EXPIRATION_MINUTES=60 \
  jira-analytics-cli analytics -p MYPROJECT
```

### Network Optimization

```bash
# Use host network for better performance
docker run --rm --network=host jira-analytics-cli analytics -p MYPROJECT

# Or use macvlan for dedicated IP
```

---

## 🐛 Troubleshooting

### Container Won't Start

**Problem:** Container exits immediately with error

**Solutions:**
1. Check logs: `docker logs <container-id>`
2. Verify environment variables are set correctly
3. Check Jira URL and API token
4. Test with minimal configuration


```bash
# Debug with shell access
docker run --rm -it --entrypoint sh jira-analytics-cli
```


### Authentication Errors


**Problem:** 401 Unauthorized errors


**Solutions:**
1. Verify API token is correct and not expired
2. Check JIRA_BASE_URL format (must include https://)
3. Ensure token has proper permissions in Jira
4. Try regenerating the token

### Connection Issues

**Problem:** Unable to connect to Jira


**Solutions:**
1. Check network connectivity: `ping your-jira-instance.com`
2. Verify DNS resolution: `nslookup your-jira-instance.com`
3. Check proxy settings if behind corporate firewall
4. Test with curl: `curl -v https://your-jira-instance.com`


### Permission Denied Errors


**Problem:** Can't write to output directory


**Solutions:**
1. Check volume permissions: `chmod 755 output`
2. Set correct ownership: `chown -R 1000:1000 output/`
3. Use named volumes instead of host mounts
4. Run as root temporarily to test (not recommended for production)


### Out of Memory Errors


**Problem:** Container crashes with OOM errors


**Solutions:**
1. Increase memory limit: `--memory=4g`
2. Reduce cache size: `CACHE_MAX_ITEMS=5000`
3. Decrease sprint count: `-s 3` instead of `-s 10`
4. Optimize dashboard complexity


### Slow Performance

**Problem:** Commands take too long to execute

**Solutions:**
1. Increase CPU allocation: `--cpus=2`
2. Increase cache expiration: `CACHE_EXPIRATION_MINUTES=60`
3. Reduce sprint count: `-s 3` instead of `-s 10`
4. Disable detailed logging: `ENABLE_DETAILED_LOGGING=false`
5. Check Jira API response times


---

## 📊 Monitoring & Logging


### Docker Logs

```bash
# View logs
docker logs <container-id>

# Follow logs
docker logs -f <container-id>


# View last 100 lines
docker logs --tail=100 <container-id>


# View logs with timestamps
docker logs -t <container-id>
```

### Docker Events

```bash
# Monitor events
docker events --filter 'event=die' --filter 'event=start'
```

### Resource Usage

```bash
# View resource usage
docker stats <container-id>

# View all containers
docker stats -a
```

### Health Status

```bash
# Check health status
docker inspect --format='{{json .State.Health}}' <container-id>

# Check container status
docker ps --filter "health=unhealthy"
```

---

## 🔄 Scheduling & Automation


### Cron Job Example

```bash
# Run daily at 8 AM
0 8 * * * /usr/bin/docker run --rm jira-analytics-cli analytics -p MYPROJECT -s 10 -o /reports/daily-$(date +\%Y\%m\%d).txt

# Run hourly
0 * * * * /usr/bin/docker run --rm jira-analytics-cli dashboard update -p MYPROJECT -n team-overview
```

### Systemd Service Example

Create `/etc/systemd/system/jira-analytics.service`:

```ini
[Unit]
Description=Jira Analytics CLI Service
After=network.target

[Service]
Type=simple
User=appuser
Group=appuser
WorkingDirectory=/app
ExecStart=/usr/bin/docker run --rm \
  --name=%n \
  --cpus=2 \
  --memory=2g \
  -e JIRA_BASE_URL=https://your-instance.atlassian.net \
  -e JIRA_API_TOKEN=${JIRA_API_TOKEN} \
  -e JIRA_DEFAULT_PROJECT=MYPROJECT \
  -v /var/lib/jira-analytics/output:/app/output \
  jira-analytics-cli \
  analytics -p MYPROJECT -s 5 -o /app/output/report.txt
Restart=unless-stopped
RestartSec=30

[Install]
WantedBy=multi-user.target
```

Then:
```bash
# Enable and start service
sudo systemctl enable jira-analytics.service
sudo systemctl start jira-analytics.service

# Check status
sudo systemctl status jira-analytics.service

# View logs
journalctl -u jira-analytics.service -f
```

---

## 📚 Additional Resources

- [Official Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Jira REST API Documentation](https://developer.atlassian.com/cloud/jira/platform/rest/v3/intro/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Security Best Practices](https://docs.docker.com/engine/security/)

---

## 🆘 Need Help?


- Check the [FAQ](../docs/faq.md) for common issues
- Review the [examples](../examples/docker-deployment/) for working configurations
- Open an issue on GitHub for bugs or feature requests
- Check Docker community forums for container-specific questions


---

**Happy Containerizing!** 🐳


This guide covers production-ready Docker deployment for jira-analytics-cli v2.0. For questions or issues, consult the project documentation or community resources.
