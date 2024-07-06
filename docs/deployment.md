# Deployment Guide

This guide covers deploying Jira Analytics CLI to production environments.

## Deployment Methods

### 1. Local Installation

Best for: Single user, development, testing

```bash
# Clone and build
git clone https://github.com/sarmkadan/jira-analytics-cli.git
cd jira-analytics-cli
dotnet publish -c Release -o ./dist --self-contained -p:PublishSingleFile=true

# Install to PATH
sudo cp dist/jira-analytics-cli /usr/local/bin/
chmod +x /usr/local/bin/jira-analytics-cli

# Verify
jira-analytics-cli --version
```

### 2. Docker Container

Best for: Isolated environment, CI/CD, containers

```bash
# Build image
docker build -t jira-analytics-cli:latest .

# Run container
docker run --rm \
  -e JIRA_BASE_URL="https://your-instance.atlassian.net" \
  -e JIRA_API_TOKEN="your-token" \
  -v $(pwd)/output:/app/output \
  jira-analytics-cli analytics -p MYPROJECT -s 5 -o /app/output/report.txt

# Tag and push to registry
docker tag jira-analytics-cli:latest your-registry/jira-analytics-cli:latest
docker push your-registry/jira-analytics-cli:latest
```

### 3. Docker Compose

Best for: Local development, multi-service setups

```bash
# Create .env file
cat > .env << EOF
JIRA_BASE_URL=https://your-instance.atlassian.net
JIRA_API_TOKEN=your-token
OUTPUT_DIR=$(pwd)/output
EOF

# Run with docker-compose
docker-compose up -d
docker-compose exec app jira-analytics-cli analytics -p MYPROJECT -s 5
docker-compose down
```

### 4. Kubernetes

Best for: Enterprise, high-availability

```yaml
# jira-analytics-job.yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: jira-analytics
  namespace: analytics
spec:
  template:
    spec:
      containers:
      - name: jira-analytics
        image: jira-analytics-cli:latest
        env:
        - name: JIRA_BASE_URL
          valueFrom:
            secretKeyRef:
              name: jira-config
              key: base-url
        - name: JIRA_API_TOKEN
          valueFrom:
            secretKeyRef:
              name: jira-config
              key: api-token
        volumeMounts:
        - name: output
          mountPath: /app/output
        args: ["analytics", "-p", "MYPROJECT", "-s", "5", "-o", "/app/output/report.txt"]
      volumes:
      - name: output
        persistentVolumeClaim:
          claimName: analytics-output
      restartPolicy: Never
```

Deploy with:
```bash
kubectl apply -f jira-analytics-job.yaml
```

## Production Configuration

### Environment Variables

Set these in your deployment environment:

```bash
# Jira Configuration
export JIRA_BASE_URL="https://your-instance.atlassian.net"
export JIRA_API_TOKEN="your-api-token"  # Use secrets management!
export JIRA_DEFAULT_PROJECT="MYPROJECT"

# Caching (optimize for production)
export CACHE_EXPIRATION_MINUTES=30
export CACHE_MAX_ITEMS=5000

# Performance
export ENABLE_DETAILED_LOGGING=false
export MAX_CONCURRENT_REQUESTS=3

# Security
export TLS_PROTOCOL_VERSION="TLS1.2"
```

### Secrets Management

Never commit API tokens. Use your platform's secrets system:

**Docker Secrets**:
```bash
echo "your-api-token" | docker secret create jira_api_token -
docker run --rm --secret jira_api_token \
  -e JIRA_API_TOKEN_FILE=/run/secrets/jira_api_token \
  jira-analytics-cli
```

**Kubernetes Secrets**:
```bash
kubectl create secret generic jira-config \
  --from-literal=api-token=your-token \
  --from-literal=base-url=https://your-instance.atlassian.net

# Reference in deployment
- name: JIRA_API_TOKEN
  valueFrom:
    secretKeyRef:
      name: jira-config
      key: api-token
```

**GitHub Actions Secrets**:
```yaml
- name: Run Analytics
  env:
    JIRA_BASE_URL: ${{ secrets.JIRA_BASE_URL }}
    JIRA_API_TOKEN: ${{ secrets.JIRA_API_TOKEN }}
  run: jira-analytics-cli analytics -p MYPROJECT -s 5
```

## Scheduled Execution

### Cron Jobs (Linux/macOS)

```bash
# Edit crontab
crontab -e

# Run analytics every Friday at 9 AM
0 9 * * 5 /usr/local/bin/jira-analytics-cli analytics -p MYPROJECT -s 5 -o /var/reports/weekly.txt

# Daily at 6 AM with email notification
0 6 * * * /usr/local/bin/jira-analytics-cli analytics -p MYPROJECT -s 1 -o /tmp/daily.txt && mail -s "Daily Jira Report" team@example.com < /tmp/daily.txt
```

### GitHub Actions Workflow

```yaml
# .github/workflows/jira-analytics.yml
name: Jira Analytics

on:
  schedule:
    - cron: '0 9 * * 1'  # Every Monday at 9 AM UTC
  workflow_dispatch:     # Manual trigger

jobs:
  analytics:
    runs-on: ubuntu-latest
    steps:
      - name: Run Analytics
        env:
          JIRA_BASE_URL: ${{ secrets.JIRA_BASE_URL }}
          JIRA_API_TOKEN: ${{ secrets.JIRA_API_TOKEN }}
        run: |
          # Download latest release
          wget https://github.com/sarmkadan/jira-analytics-cli/releases/download/v1.2.0/jira-analytics-cli
          chmod +x jira-analytics-cli
          ./jira-analytics-cli analytics -p MYPROJECT -s 5 -o report.txt
      
      - name: Upload Report
        uses: actions/upload-artifact@v3
        with:
          name: jira-report
          path: report.txt
```

### Azure Pipelines

```yaml
# azure-pipelines.yml
trigger:
  - none

schedules:
  - cron: "0 9 * * 1"
    displayName: Weekly Analysis
    branches:
      include:
      - main

jobs:
  - job: JiraAnalytics
    pool:
      vmImage: 'ubuntu-latest'
    steps:
      - script: |
          dotnet tool install -g jira-analytics-cli
          jira-analytics-cli analytics -p MYPROJECT -s 5 -o $(Build.ArtifactStagingDirectory)/report.txt
        env:
          JIRA_BASE_URL: $(JIRA_BASE_URL)
          JIRA_API_TOKEN: $(JIRA_API_TOKEN)
      - publish: $(Build.ArtifactStagingDirectory)
        artifact: reports
```

### Kubernetes CronJob

```yaml
# k8s-cronjob.yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: jira-analytics-weekly
spec:
  schedule: "0 9 * * 1"  # Every Monday at 9 AM
  jobTemplate:
    spec:
      template:
        spec:
          serviceAccountName: analytics
          containers:
          - name: analytics
            image: your-registry/jira-analytics-cli:latest
            env:
            - name: JIRA_BASE_URL
              valueFrom:
                secretKeyRef:
                  name: jira-secrets
                  key: base-url
            - name: JIRA_API_TOKEN
              valueFrom:
                secretKeyRef:
                  name: jira-secrets
                  key: api-token
            args: ["analytics", "-p", "MYPROJECT", "-s", "5", "-o", "/reports/weekly.txt"]
            volumeMounts:
            - name: reports
              mountPath: /reports
          volumes:
          - name: reports
            persistentVolumeClaim:
              claimName: reports-pvc
          restartPolicy: OnFailure
```

## Monitoring & Logging

### Log Collection

```bash
# View application logs
docker logs <container-id>

# Stream logs in real-time
docker logs -f <container-id>

# Save logs to file
docker logs <container-id> > analytics.log 2>&1
```

### Health Checks

Create a wrapper script to verify functionality:

```bash
#!/bin/bash
# health-check.sh

TIMEOUT=30
PROJECT="MYPROJECT"

timeout $TIMEOUT jira-analytics-cli analytics -p "$PROJECT" -s 1 -o /tmp/test.txt

if [ $? -eq 0 ]; then
    echo "✓ Analytics CLI is healthy"
    rm -f /tmp/test.txt
    exit 0
else
    echo "✗ Analytics CLI health check failed"
    exit 1
fi
```

Docker healthcheck:
```dockerfile
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD bash /app/health-check.sh || exit 1
```

## Scaling Considerations

### Multiple Projects

```bash
#!/bin/bash
# Analyze multiple projects in parallel

projects=("BACKEND" "FRONTEND" "DEVOPS")

for project in "${projects[@]}"; do
    jira-analytics-cli analytics -p "$project" -s 5 -o "reports/${project}.txt" &
done

wait  # Wait for all background jobs
echo "All analyses complete"
```

### Large Datasets

For projects with 1000+ issues:

```bash
# Increase timeouts and reduce concurrency
export JIRA_REQUEST_TIMEOUT_SECONDS=60
export MAX_CONCURRENT_REQUESTS=2
export CACHE_EXPIRATION_MINUTES=60

jira-analytics-cli analytics -p LARGEPRO -s 3 -o report.txt
```

## Backup & Recovery

### Backup Configuration

```bash
#!/bin/bash
# backup-config.sh

BACKUP_DIR="backups/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

# Backup configuration files
cp appsettings.json "$BACKUP_DIR/"
cp -r reports "$BACKUP_DIR/" 2>/dev/null

echo "Backup created at $BACKUP_DIR"
```

### Export for Archival

```bash
# Export all historical data
jira-analytics-cli export -p MYPROJECT -f json -o "archive_$(date +%Y%m%d).json"
gzip "archive_$(date +%Y%m%d).json"
```

## Security Best Practices

### API Token Security

1. **Never commit tokens**
   ```bash
   # Add to .gitignore
   echo "appsettings.local.json" >> .gitignore
   echo ".env" >> .gitignore
   ```

2. **Use environment variables**
   ```bash
   # Never in command line
   # ✗ jira-analytics-cli --token "abc123"
   # ✓ export JIRA_API_TOKEN="abc123"
   ```

3. **Rotate tokens regularly**
   - Set a reminder to rotate quarterly
   - Invalidate old tokens in Jira settings

4. **Audit access**
   - Check Jira API token access logs
   - Monitor for unusual API usage

### Network Security

```bash
# Use HTTPS only
export JIRA_BASE_URL="https://your-instance.atlassian.net"  # ✓ Correct
# NOT: export JIRA_BASE_URL="http://..."  # ✗ Insecure

# Behind proxy?
export HTTP_PROXY="http://proxy.example.com:3128"
export HTTPS_PROXY="https://proxy.example.com:3128"
```

### Container Security

```dockerfile
# Run as non-root user
RUN useradd -m analytics
USER analytics

# Read-only filesystem
RUN chmod 444 /app/config.json

# Security scanning
RUN apt-get update && apt-get install -y ca-certificates
```

## Troubleshooting Deployment

### Issue: Connection timeout

```bash
# Check network connectivity
curl -I "$JIRA_BASE_URL"

# Check DNS
nslookup your-instance.atlassian.net

# Check firewall rules
telnet your-instance.atlassian.net 443
```

### Issue: Out of memory

```bash
# Increase JVM heap if needed
export _JAVA_OPTIONS="-Xmx512m"

# Or reduce cache size
export CACHE_MAX_ITEMS=500
```

### Issue: Rate limiting

```bash
# Reduce request concurrency
export MAX_CONCURRENT_REQUESTS=1

# Increase cache expiration
export CACHE_EXPIRATION_MINUTES=60

# Or reduce sprint count
jira-analytics-cli analytics -p MYPROJECT -s 2
```

## Support & Maintenance

- Monitor disk space for reports
- Regularly review and delete old reports
- Keep .NET runtime updated for security patches
- Check for new releases: https://github.com/sarmkadan/jira-analytics-cli/releases

---

For additional help, see:
- [Getting Started Guide](./getting-started.md)
- [Configuration Reference](../README.md#configuration)
- [Troubleshooting FAQ](./faq.md)
