# Changelog

All notable changes to Jira Analytics CLI are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-01-15

### Added
- Comprehensive documentation suite (Getting Started, Architecture, API Reference, Deployment, FAQ)
- Example scripts for common use cases (velocity analysis, developer load, batch exports)
- Docker and Docker Compose support for containerized deployments
- GitHub Actions CI/CD workflow for automated builds and releases
- Makefile for common development tasks
- .editorconfig for consistent code formatting across editors
- Support for multiple export formats with improved error handling
- Health check endpoint for container deployments
- Detailed logging configuration options
- Performance metrics collection capability
- Rate limiting middleware for API calls
- Caching policy customization
- Burndown chart visual enhancements

### Changed
- Refactored repository layer for better testability
- Improved error messages with actionable advice
- Optimized Jira API queries with JQL improvements
- Enhanced CSV export with proper field escaping
- Better handling of concurrent API requests
- Updated dependencies to latest stable versions

### Fixed
- Rate limiting handling on Jira Cloud
- Memory leaks in cache implementation
- Timezone handling in burndown charts
- UTF-8 character handling in exports
- Null reference exceptions in sparse data scenarios

### Security
- Added HTTPS requirement for Jira API communication
- Sensitive data filtering from logs
- API token never logged or output
- User isolation in container deployments
- Read-only filesystem support in Docker

## [1.1.0] - 2025-12-01

### Added
- Developer productivity ranking system
- Quality score calculation based on defect rates
- Risk assessment for overdue and blocked issues
- Bulk export functionality for multiple projects
- JSON export with nested structure support
- Configuration file support (appsettings.json)
- Support for Jira Server installations
- Comprehensive error handling with specific exit codes
- Async/await throughout for better performance
- Cache expiration policies
- Event bus foundation for webhooks

### Changed
- Refactored analytics calculations for accuracy
- Improved sprint metric aggregation
- Better handling of incomplete sprints
- More efficient memory usage for large datasets
- Simplified CLI argument parsing

### Fixed
- Incorrect cycle time calculations
- Velocity fluctuations from scope changes
- Missing developer assignments
- CSV special character handling

## [1.0.0] - 2025-10-15

### Added
- Initial release of Jira Analytics CLI
- Core features:
  * Sprint velocity analysis across multiple sprints
  * Team member productivity metrics
  * Cycle time tracking for issues
  * Overdue issue detection and reporting
  * Burndown chart generation using SkiaSharp
  * Multi-format export (PNG, JPEG, JSON, CSV, TXT)
  * Jira REST API v3 integration
  * Dependency injection container setup
  * Configuration management with environment variables
  * In-memory caching with expiration
  * Structured logging with Microsoft.Extensions.Logging
  * Repository pattern for data access
  * Service layer for business logic
  * Comprehensive CLI using System.CommandLine
  * Support for Jira Cloud instances
  
### Features
- Analytics command for sprint analysis
- Export command for data export in multiple formats
- Burndown command for sprint burndown visualization
- Global configuration via environment variables
- Error handling with custom exceptions
- Input validation and sanitization
- Performance diagnostics
- Request logging and tracing

## [0.2.0] - 2025-09-01

### Added
- Experimental burndown chart generation
- CSV export support
- Preliminary velocity calculations
- Issue tracking and aggregation

### Changed
- Refactored service layer architecture
- Improved data model validation

## [0.1.0] - 2025-08-15

### Added
- Initial project setup
- Basic Jira API integration
- Sprint retrieval functionality
- Issue querying framework
- Project structure and dependency injection setup

---

## Upgrade Guide

### From 1.1.0 to 1.2.0

No breaking changes. Update by replacing the binary:

```bash
dotnet publish -c Release -o ./dist --self-contained -p:PublishSingleFile=true
```

New environment variables available (optional):
- `ENABLE_METRICS_COLLECTION=true` for diagnostics
- `MAX_CONCURRENT_REQUESTS=5` for performance tuning

### From 1.0.0 to 1.1.0

Configuration file format unchanged, but new options available:

```json
{
  "features": {
    "enableDetailedLogging": false  // New option
  }
}
```

### From 0.x to 1.0.0

Version 1.0.0 includes several breaking changes:

1. Command structure changed:
   ```bash
   # Old
   jira-analytics-cli --project PROJ --analyze
   
   # New
   jira-analytics-cli analytics -p PROJ
   ```

2. Configuration keys updated:
   ```bash
   # Old
   export JIRA_INSTANCE_URL="..."
   
   # New
   export JIRA_BASE_URL="..."
   ```

3. Export command introduced:
   ```bash
   # New in 1.0.0
   jira-analytics-cli export -p PROJ -f json -o output.json
   ```

See [Migration Guide](./docs/migration.md) for detailed upgrade instructions.

---

## Known Issues

### v1.2.0
- Burndown charts may not display correctly for sprints with no scope changes
- Rate limiting on Jira Cloud may require retry delays
- CSV export truncates very long text fields (10000+ characters)

### v1.1.0
- Memory usage increases with projects having 10000+ issues
- Concurrent requests may exceed API rate limits on smaller plans

---

## Future Roadmap

### Planned for v1.3.0
- Database persistence with Entity Framework Core
- PostgreSQL and SQL Server support
- Webhook real-time metric synchronization
- Custom metric definitions
- Multi-team support

### Planned for v2.0.0
- Web API with ASP.NET Core
- Web dashboard and visualizations
- Advanced forecasting with ML predictions
- GraphQL support
- Mobile app integration

---

## Support

For issues or questions about a specific version:
- Check the [FAQ](./docs/faq.md) documentation
- Review the [Troubleshooting](./docs/faq.md#troubleshooting) section
- Open an issue on GitHub with version information

---

**Latest Version:** 1.2.0

**Release Date:** 2026-01-15

**Maintained by:** [Vladyslav Zaiets](https://sarmkadan.com)
