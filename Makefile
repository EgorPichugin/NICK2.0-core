DOTNET ?= $(shell if command -v dotnet >/dev/null 2>&1; then printf dotnet; elif command -v cmd.exe >/dev/null 2>&1; then printf 'cmd.exe /C dotnet'; else printf dotnet; fi)

.PHONY: local-run db-up db-down db-reset db-logs build test

local-run: db-up
	$(DOTNET) run --launch-profile http

db-up:
	docker compose up -d --wait postgres

db-down:
	docker compose down

db-reset:
	docker compose down -v

db-logs:
	docker compose logs -f postgres

build:
	$(DOTNET) build --no-restore

test:
	$(DOTNET) test NICK2.0-core.Tests/NICK2.0-core.Tests.csproj --no-restore --verbosity minimal
