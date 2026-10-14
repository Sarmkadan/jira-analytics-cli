## [Unreleased]
### Added
- `--check-config` root flag: checks each Jira setting, then runs an authenticated `GET /rest/api/3/myself`, printing OK/FAIL/SKIP per item
- Output option validation before any Jira call: `--format` must be a supported value (listed in the error), `--sprints` must be 1-50, and `--output`/`--output-dir` must have an existing or creatable directory
### Changed
- `export --format` default is now `json`; `pdf` was listed in help but is not supported and is no longer advertised

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