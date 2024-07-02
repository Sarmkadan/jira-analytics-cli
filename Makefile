# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Makefile for Jira Analytics CLI
# Provides convenient commands for building, testing, and deploying
# =============================================================================

.PHONY: help build test publish clean docker docker-push install uninstall \
        format lint restore run examples docs version

# Default target
help:
	@echo "Jira Analytics CLI - Build Commands"
	@echo ""
	@echo "Development:"
	@echo "  make build              - Build the project in Debug mode"
	@echo "  make restore            - Restore NuGet packages"
	@echo "  make clean              - Remove build artifacts"
	@echo "  make format             - Format code with dotnet format"
	@echo "  make lint               - Run code analysis"
	@echo ""
	@echo "Testing & Quality:"
	@echo "  make test               - Run unit tests"
	@echo "  make test-coverage      - Run tests with coverage report"
	@echo ""
	@echo "Release:"
	@echo "  make publish            - Publish Release build"
	@echo "  make publish-linux      - Publish for Linux x64"
	@echo "  make publish-windows    - Publish for Windows x64"
	@echo "  make publish-macos      - Publish for macOS"
	@echo "  make publish-all        - Publish for all platforms"
	@echo ""
	@echo "Docker:"
	@echo "  make docker             - Build Docker image"
	@echo "  make docker-run         - Run container locally"
	@echo "  make docker-push        - Push to registry"
	@echo "  make docker-compose     - Start with Docker Compose"
	@echo ""
	@echo "Installation:"
	@echo "  make install            - Install CLI to /usr/local/bin"
	@echo "  make uninstall          - Remove CLI from /usr/local/bin"
	@echo ""
	@echo "Utilities:"
	@echo "  make version            - Show project version"
	@echo "  make examples           - List example scripts"
	@echo "  make docs               - Generate documentation"
	@echo "  make watch              - Watch for changes (dotnet watch)"

# Variables
DOTNET := dotnet
PROJECT := JiraAnalyticsCli.csproj
DIST_DIR := dist
VERSION := $(shell grep -m 1 '<Version>' $(PROJECT) | sed -E 's/.*<Version>(.*)<\/Version>.*/\1/')
REGISTRY := ghcr.io
IMAGE_NAME := jira-analytics-cli
DOCKER_IMAGE := $(REGISTRY)/$(IMAGE_NAME):$(VERSION)

# Color output
RED := \033[0;31m
GREEN := \033[0;32m
YELLOW := \033[0;33m
NC := \033[0m

# Build targets
restore:
	@echo "$(GREEN)Restoring packages...$(NC)"
	$(DOTNET) restore

build: restore
	@echo "$(GREEN)Building project...$(NC)"
	$(DOTNET) build -c Debug

release: restore
	@echo "$(GREEN)Building Release...$(NC)"
	$(DOTNET) build -c Release --no-restore

clean:
	@echo "$(GREEN)Cleaning up...$(NC)"
	rm -rf bin obj $(DIST_DIR) *.log
	find . -name '*.tmp' -delete

# Code quality
format: restore
	@echo "$(GREEN)Formatting code...$(NC)"
	$(DOTNET) format

lint: restore
	@echo "$(GREEN)Running code analysis...$(NC)"
	$(DOTNET) build -c Release --no-restore /p:EnforceCodeStyleInBuild=true

# Testing
test: build
	@echo "$(GREEN)Running tests...$(NC)"
	$(DOTNET) test -c Debug --no-build --verbosity normal

test-coverage: build
	@echo "$(GREEN)Running tests with coverage...$(NC)"
	$(DOTNET) test -c Debug --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=opencover || true
	@echo "Coverage report available in TestResults/"

# Publishing
publish: release
	@echo "$(GREEN)Publishing Release build...$(NC)"
	$(DOTNET) publish -c Release -o $(DIST_DIR)/release \
		--self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true

publish-linux: release
	@echo "$(GREEN)Publishing for Linux x64...$(NC)"
	mkdir -p $(DIST_DIR)
	$(DOTNET) publish -c Release -o $(DIST_DIR)/linux-x64 \
		--self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -r linux-x64

publish-windows: release
	@echo "$(GREEN)Publishing for Windows x64...$(NC)"
	mkdir -p $(DIST_DIR)
	$(DOTNET) publish -c Release -o $(DIST_DIR)/windows-x64 \
		--self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -r win-x64

publish-macos: release
	@echo "$(GREEN)Publishing for macOS...$(NC)"
	mkdir -p $(DIST_DIR)
	$(DOTNET) publish -c Release -o $(DIST_DIR)/macos-x64 \
		--self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -r osx-x64
	$(DOTNET) publish -c Release -o $(DIST_DIR)/macos-arm64 \
		--self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -r osx-arm64

publish-all: publish-linux publish-windows publish-macos
	@echo "$(GREEN)Creating archives...$(NC)"
	cd $(DIST_DIR) && \
		tar -czf jira-analytics-cli-linux-x64.tar.gz linux-x64/ && \
		zip -r jira-analytics-cli-windows-x64.zip windows-x64/ && \
		tar -czf jira-analytics-cli-macos-x64.tar.gz macos-x64/ && \
		tar -czf jira-analytics-cli-macos-arm64.tar.gz macos-arm64/
	@echo "$(GREEN)✓ Archives created in $(DIST_DIR)$(NC)"

# Docker targets
docker:
	@echo "$(GREEN)Building Docker image...$(NC)"
	docker build -t $(DOCKER_IMAGE) .
	@echo "$(GREEN)✓ Image built: $(DOCKER_IMAGE)$(NC)"

docker-run: docker
	@echo "$(GREEN)Running Docker container...$(NC)"
	docker run --rm -it \
		-e JIRA_BASE_URL="https://your-instance.atlassian.net" \
		-e JIRA_API_TOKEN="your-token" \
		-v $(PWD)/output:/app/output \
		$(DOCKER_IMAGE) analytics -p MYPROJECT -s 5

docker-push: docker
	@echo "$(GREEN)Pushing Docker image...$(NC)"
	docker push $(DOCKER_IMAGE)
	@echo "$(GREEN)✓ Image pushed to $(DOCKER_IMAGE)$(NC)"

docker-compose:
	@echo "$(GREEN)Starting Docker Compose...$(NC)"
	docker-compose up -d
	@echo "$(GREEN)✓ Services started. View with: docker-compose logs -f$(NC)"

docker-compose-down:
	@echo "$(GREEN)Stopping Docker Compose...$(NC)"
	docker-compose down

# Installation targets
install: publish
	@echo "$(GREEN)Installing CLI...$(NC)"
	sudo cp $(DIST_DIR)/release/jira-analytics-cli /usr/local/bin/
	sudo chmod +x /usr/local/bin/jira-analytics-cli
	@echo "$(GREEN)✓ Installed to /usr/local/bin/jira-analytics-cli$(NC)"

uninstall:
	@echo "$(GREEN)Uninstalling CLI...$(NC)"
	sudo rm -f /usr/local/bin/jira-analytics-cli
	@echo "$(GREEN)✓ Uninstalled$(NC)"

# Utility targets
version:
	@echo "Jira Analytics CLI v$(VERSION)"

examples:
	@echo "$(GREEN)Example Scripts:$(NC)"
	@echo ""
	@ls -lh examples/*.sh 2>/dev/null | awk '{print "  " $$NF}' || echo "  No shell examples found"
	@ls -lh examples/*.cs 2>/dev/null | awk '{print "  " $$NF}' || echo "  No C# examples found"
	@echo ""
	@echo "Run examples:"
	@echo "  bash examples/analyze-recent-sprints.sh MYPROJECT"
	@echo "  bash examples/velocity-report.sh BACKEND FRONTEND"

docs:
	@echo "$(GREEN)Documentation:$(NC)"
	@echo ""
	@ls -lh docs/*.md 2>/dev/null | awk '{print "  " $$NF}' || echo "  No docs found"

watch:
	@echo "$(GREEN)Watching for changes...$(NC)"
	$(DOTNET) watch run

# Development utilities
dev: restore build
	@echo "$(GREEN)Development build complete$(NC)"
	@echo "Run with: dotnet run -- analytics -p MYPROJECT -s 1"

run:
	@echo "$(GREEN)Running CLI...$(NC)"
	$(DOTNET) run -- --help

# Common workflows
all: clean restore build test lint format publish-all
	@echo "$(GREEN)✓ Full build complete$(NC)"

ci: clean restore build test lint
	@echo "$(GREEN)✓ CI build passed$(NC)"

# Help for specific goals
.PHONY: help-docker help-publish help-ci

help-docker:
	@echo "Docker Commands:"
	@echo "  make docker             - Build Docker image"
	@echo "  make docker-run         - Run container interactively"
	@echo "  make docker-push        - Push to registry"
	@echo ""
	@echo "Docker Compose:"
	@echo "  make docker-compose     - Start services"
	@echo "  make docker-compose-down - Stop services"

help-publish:
	@echo "Publishing for Different Platforms:"
	@echo "  make publish            - Publish for current platform"
	@echo "  make publish-all        - Build for all platforms (Linux, Windows, macOS)"
	@echo ""
	@echo "Outputs to: $(DIST_DIR)/"

help-ci:
	@echo "CI/CD Pipeline:"
	@echo "  make ci                 - Full CI build (test, lint, etc.)"
	@echo "  make all                - Everything (build, test, publish)"
	@echo ""
	@echo "GitHub Actions configured in: .github/workflows/"
