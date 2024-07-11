## [2.0.2] - 2026-05-20
### Fixed
- Fix pagination returning duplicate issues when issues are modified during fetch
- Added regression test for the fix

## [2.0.0] - 2026-05-18
### Added
- Add custom dashboard builder with drag-drop widgets and saved layouts
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x
### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency
### Fixed
- Various edge cases found through testing