DOTNET ?= $(shell if command -v dotnet >/dev/null 2>&1; then printf dotnet; elif command -v cmd.exe >/dev/null 2>&1; then printf 'cmd.exe /C dotnet'; else printf dotnet; fi)
API_PROJECT := NICK2.0-core.csproj
BOT_PROJECT := NICK2.0-core.NickTeleBot/NICK2.0-core.NickTeleBot.csproj

.PHONY: local-run api-run bot-run db-up db-down db-reset db-logs build test

local-run: db-up
	@trap 'kill 0' INT TERM EXIT; \
	$(DOTNET) run --project $(API_PROJECT) --launch-profile http & \
	api_pid=$$!; \
	$(DOTNET) run --project $(BOT_PROJECT) --launch-profile NICK2._0_core.NickTeleBot & \
	bot_pid=$$!; \
	wait $$api_pid $$bot_pid

api-run:
	$(DOTNET) run --project $(API_PROJECT) --launch-profile http

bot-run:
	$(DOTNET) run --project $(BOT_PROJECT) --launch-profile NICK2._0_core.NickTeleBot

db-up:
	docker compose up -d --wait postgres

db-down:
	docker compose down

db-reset:
	docker compose down -v

db-logs:
	docker compose logs -f postgres

build:
	$(DOTNET) build --no-restore /m:1

test:
	$(DOTNET) test NICK2.0-core.Tests/NICK2.0-core.Tests.csproj --no-restore /m:1 --verbosity minimal
