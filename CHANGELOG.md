# Changelog

All notable changes to Jira Analytics CLI are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-15

### Added
- Initial stable release of Jira Analytics CLI
- Sprint velocity analysis across multiple sprints
- Team member productivity metrics and developer load tracking
- Cycle time tracking for issues (created → resolved duration)
- Overdue issue detection and reporting with risk scoring
- Burndown chart generation using SkiaSharp (PNG, JPEG, PDF)
- Multi-format export: PNG, JPEG, JSON, CSV, TXT
- Jira REST API v3 integration with async/await throughout
- Dependency injection container via Microsoft.Extensions.DI
- Configuration management via environment variables and appsettings.json
- In-memory caching with configurable expiration
- Structured logging with Microsoft.Extensions.Logging
- Repository pattern for data access (Issue, Sprint, Metrics)
- Service layer for business logic (Analytics, Report, Export, JiraApi)
- Comprehensive CLI using System.CommandLine 2.0
- Middleware pipeline: error handling, request logging, rate limiting
- Domain event bus foundation for webhook integration
- Background task runner for async metric sync and report generation
- Performance diagnostics and metrics collection
- Quality score and defect rate calculations
- Developer productivity ranking
- Docker and Docker Compose support
- GitHub Actions CI/CD workflow for automated builds
- Comprehensive documentation suite (Getting Started, Architecture, API Reference, Deployment, FAQ)
- Example scripts for velocity analysis, developer load, and batch exports
- Unit tests with xUnit, Moq, and FluentAssertions

### Features
- `analytics` command — sprint analysis with formatted report output
- `export` command — data export in JSON, CSV, PNG, JPEG, PDF
- `burndown` command — sprint burndown chart visualization
- Global configuration via environment variables
- Comprehensive error handling with custom exceptions and specific exit codes
- Input validation and sanitization at system boundaries
- Request logging and tracing middleware

## [0.2.0] - 2025-09-08

### Added
- Experimental burndown chart generation with SkiaSharp
- CSV export support with proper field escaping
- Preliminary velocity calculations across sprints
- Issue tracking and aggregation
- In-memory caching layer with expiration policies
- Rate limiting middleware for Jira API calls
- Developer productivity metrics (completion rate, cycle time)
- Quality score calculation based on defect rates
- Risk assessment for overdue and blocked issues

### Changed
- Refactored service layer architecture for better separation of concerns
- Improved data model validation
- More efficient memory usage for large datasets

### Fixed
- Incorrect cycle time calculations for issues resolved on the same day
- CSV special character handling for non-ASCII characters

## [0.1.0] - 2025-08-15

### Added
- Initial project setup with .NET 10 and System.CommandLine
- Basic Jira API integration using REST API v3
- Sprint retrieval and issue querying framework
- Project structure with dependency injection setup
- Domain models: JiraIssue, Sprint, Developer, SprintMetric, BurndownSnapshot
- Repository pattern stubs for Issue, Sprint, and Metrics
- Environment variable configuration support
- Structured logging with Microsoft.Extensions.Logging
- MIT license and project scaffolding

---

## Upgrade Guide

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

---

## Known Issues

### v1.0.0
- Burndown charts may not display correctly for sprints with no scope changes
- Rate limiting on Jira Cloud may require retry delays on high-volume queries
- CSV export truncates very long text fields (10 000+ characters)

---

## Future Roadmap

### Planned for v1.1.0
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

---

## Support

For issues or questions about a specific version:
- Check the [FAQ](./docs/faq.md) documentation
- Review the [Troubleshooting](./docs/faq.md#troubleshooting) section
- Open an issue on GitHub with version information

---

**Latest Version:** 1.0.0

**Release Date:** 2025-10-15

**Maintained by:** [Vladyslav Zaiets](https://sarmkadan.com)
