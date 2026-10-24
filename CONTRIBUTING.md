# Contributing

Thanks for your interest in the Jira Analytics CLI. This guide covers how to set up, build, test, and submit changes.

## Prerequisites

- .NET SDK 10.0.100 or later in the 10.0 line. `global.json` pins `10.0.100` with `rollForward: latestMinor`.
- Git.
- A Jira instance and an API token, only if you want to run the CLI against real data.

## Build

The repository root contains both `jira-analytics-cli.sln` and `jira-analytics-cli.slnx`, so a bare `dotnet build` fails with MSB1011. Pass the solution file explicitly:

```bash
dotnet restore jira-analytics-cli.sln
dotnet build jira-analytics-cli.sln -c Debug
```

`make build` does the same. For a Release build, use `-c Release` or `make release`.

## Test

```bash
dotnet test jira-analytics-cli.sln
```

or `make test`. Tests use xUnit, FluentAssertions, and Moq.

The test project does not reference the main project. It includes source files explicitly. When you add a new class that has tests, add a matching `<Compile Include="...">` entry to `tests/jira-analytics-cli.Tests/jira-analytics-cli.Tests.csproj`.

## Run the CLI locally

Set the connection settings as environment variables:

```bash
export JIRA_URL="https://your-company.atlassian.net"
export JIRA_API_TOKEN="your-api-token"
```

Check the setup, then run a command:

```bash
dotnet run -- --check-config
dotnet run -- analytics --project MYPROJ --sprints 3
```

See the README for all settings and commands. Do not commit real tokens or `appsettings.local.json`.

## Coding conventions

- Follow `.editorconfig`. It sets 4-space indentation and the naming rules: PascalCase for public members, camelCase for locals.
- Nullable reference types are enabled. Do not add new nullable warnings.
- Use file-scoped namespaces. Namespaces match folder names.
- Each class in `Services/`, `Repositories/`, and similar folders has an `I`-prefixed interface. Register both in `ServiceFactory.cs`.
- Validate arguments with `ArgumentNullException.ThrowIfNull` or `ValidationHelpers`, and cover those throws in tests.
- Keep the build at zero warnings. Run `make lint` (a Release build with `EnforceCodeStyleInBuild=true`) before you open a PR.
- Run `dotnet format` (or `make format`) to apply formatting.

## Branches and commits

Create a branch from `main` for each change. Use a short prefix that matches the kind of change, followed by a descriptive slug:

- `feat/export-csv-headers`
- `fix/burndown-empty-sprint`
- `docs/contributing-guide`

Write commit messages in the conventional style used in this repository: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, or `chore:`. Use the imperative mood, keep the subject line under about 72 characters, and add a body when the reason for the change is not obvious from the diff.

Do not commit generated or local files: `bin/`, `obj/`, `TestResults/`, `.aider*`, or `appsettings.local.json`.

## Pull request process

1. For a large change, such as a new command, a new output format, or a change to the public API, open an issue first and agree on the approach.
2. Keep each PR small and focused on one change. Split unrelated work into separate PRs.
3. Make sure `dotnet build` and `dotnet test` pass locally. CI runs restore, a Release build, and the test suite on every PR to `main`, and it must pass before merge.
4. In the PR description, explain what changed and why. Describe the testing you did: the commands you ran, the tests you added, and any manual checks against Jira.
5. Respond to review comments by pushing new commits. Squash only if a reviewer asks.

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
