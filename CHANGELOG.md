## [2.0.2] - 2026-05-20
### Fixed
- Fix pagination returning duplicate issues when issues are modified during fetch
- Added regression test for the fix

## [2.0.0] - 2026-05-18
### Added
- HTML report command with self-contained output
- JQL query command with pagination support
- Team comparison command (side-by-side multi-project metrics)
- Docker support with multi-stage builds
### Changed
- Upgraded to .NET 10.0
- SprintComparisonService extracted from analytics layer
### Fixed
- Various edge cases found through testing