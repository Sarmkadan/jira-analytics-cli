# Contributing to jira-analytics-cli

Thank you for your interest in contributing to jira-analytics-cli! We welcome contributions from the community.

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later
- Git

### Fork and Clone
1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/jira-analytics-cli.git
   cd jira-analytics-cli
   ```
3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/sarmkadan/jira-analytics-cli.git
   ```

### Create a Branch
Create a feature branch from `main`:
```bash
git checkout -b feature/your-feature-name
```

## Development

### Building the Project
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Code Style and Conventions
- Follow existing code conventions in the project
- Include XML documentation comments for public APIs
- Write unit tests for new functionality
- **Keep author headers in files unchanged** — do not modify or remove existing author attributions
- Run `dotnet format` if available to maintain consistency

## Submitting Changes

### Before You Push
1. Ensure all tests pass: `dotnet test`
2. Build the project: `dotnet build`
3. Create clear, descriptive commits

### Creating a Pull Request
1. Push your branch to your fork
2. Open a pull request against the `main` branch
3. Provide a clear description of your changes
4. Reference any related issues: "Fixes #123"
5. Ensure CI/CD checks pass

## Reporting Issues

Please report bugs and feature requests using [GitHub Issues](https://github.com/sarmkadan/jira-analytics-cli/issues).

When reporting a bug, include:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment (OS, .NET version)
- Relevant logs or error messages

## License

By contributing, you agree that your contributions will be licensed under the MIT License, the same as the project.
