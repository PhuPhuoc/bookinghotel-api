SHELL := C:/Users/phuoc.tran/AppData/Local/Programs/Git/bin/bash.exe
.SHELLFLAGS := -ec
SLN        = $(PREFIX).sln
PREFIX     = BookingHotel
API_DIR    = host/$(PREFIX).Api
API_PROJ   = $(API_DIR)/$(PREFIX).Api.csproj

CONTRACTS_PROJ   = src/$(PREFIX).Contracts/$(PREFIX).Contracts.csproj
APPLICATION_PROJ = src/$(PREFIX).Application/$(PREFIX).Application.csproj
DOMAIN_PROJ      = src/$(PREFIX).Domain/$(PREFIX).Domain.csproj
PERSIST_PROJ     = src/$(PREFIX).Infrastructure.Persistence/$(PREFIX).Infrastructure.Persistence.csproj

# Optional modules (not part of `make init`, added on demand)
CACHING_DIR      = src/$(PREFIX).Infrastructure.Caching
CACHING_PROJ     = $(CACHING_DIR)/$(PREFIX).Infrastructure.Caching.csproj
SEARCH_DIR       = src/$(PREFIX).Infrastructure.Search
SEARCH_PROJ      = $(SEARCH_DIR)/$(PREFIX).Infrastructure.Search.csproj
MESSAGING_DIR    = src/$(PREFIX).Infrastructure.Messaging
MESSAGING_PROJ   = $(MESSAGING_DIR)/$(PREFIX).Infrastructure.Messaging.csproj

# --------------------------------------------------------------------------
# Package versions — all pinned explicitly and kept compatible with net8.0.
# Rules of thumb used below:
#   - All Microsoft.EntityFrameworkCore.* packages (core/design/relational/
#     tools) MUST share the exact same EF_VERSION, or `dotnet build` throws
#     NU1605 downgrade / NU1107 conflict errors. This includes the EF.Design
#     package referenced from the Api project (needed by `dotnet ef`), not
#     just the one in Infrastructure.Persistence.
#   - Npgsql.EntityFrameworkCore.PostgreSQL tracks EF Core's major line but
#     is versioned independently by the Npgsql team — pin explicitly via
#     NPGSQL_EF_VERSION, don't assume it equals EF_VERSION.
#   - WolverineFx.Postgresql -> Weasel.Postgresql pulls in the raw Npgsql
#     ADO.NET driver (currently resolves to 9.0.2+) independent of the EF
#     provider. As of EF_VERSION=9.0.9 this is compatible; if you bump
#     EF_VERSION, re-check `dotnet nuget why <Persist.csproj> Npgsql`.
#   - Microsoft.CodeAnalysis.* (Common/Workspaces.Common) must be pinned to
#     CODEANALYSIS_VERSION directly in Infrastructure.Persistence to resolve
#     the conflict between EF.Design's Roslyn dependency (older) and
#     Wolverine's JasperFx.RuntimeCompiler (newer). Pinning forces NuGet to
#     use the newer version solution-wide instead of erroring out.
#   - IMPORTANT: because EF_VERSION is on the 9.x line, EF Core's own
#     dependencies force Microsoft.Extensions.Configuration.Abstractions
#     (and transitively other Microsoft.Extensions.* packages) up to 9.x
#     as well, even though the app still targets net8.0 and runs fine on
#     the net8.0 shared framework. Do NOT pin these back to 8.0.x — that
#     reintroduces the "Detected package downgrade" NU1605 error. This
#     applies to every project that references Configuration.Abstractions
#     directly, including optional modules (Redis/Elastic/Messaging).
#   - Microsoft.AspNetCore.Authentication.JwtBearer stays on 8.0.x — this
#     is an ASP.NET Core *framework* package tied to the net8.0 shared
#     framework itself, unrelated to the EF Core / Extensions.* line above.
#   - StackExchange.Redis, Serilog sinks, FluentValidation, Mapster, ErrorOr
#     version independently of EF/ASP.NET Core — safe to bump on their own.
#   - WolverineFx / DotNetCore.CAP / Elastic.Clients.Elasticsearch are 3rd
#     party and release fast. Versions below are known-good net8.0 targets
#     at time of writing — re-check nuget.org before bumping majors.
# --------------------------------------------------------------------------

# EF Core (must all match EF_VERSION exactly — core/design/relational/tools)
EF_VERSION                  = 9.0.9
NPGSQL_EF_VERSION           = 9.0.4

# Roslyn (forced to align with Wolverine's JasperFx.RuntimeCompiler)
CODEANALYSIS_VERSION        = 4.14.0

# Microsoft.Extensions.* family — forced to 9.x by EF_VERSION=9.0.9, still
# net8.0-runtime compatible. Keep these in lockstep with EF_VERSION.
CONFIG_ABSTRACTIONS_VERSION = 9.0.9
HOSTING_VERSION             = 9.0.9
DI_ABSTRACTIONS_VERSION     = 9.0.9
OPTIONS_CONFIG_VERSION      = 9.0.9

# ASP.NET Core auth (framework-specific, stays on 8.0.x — see note above)
JWT_VERSION                 = 8.0.11

# Logging
SERILOG_ASPNET_VERSION      = 8.0.3
SERILOG_CONSOLE_VERSION     = 6.0.0

# Application layer building blocks
WOLVERINE_VERSION           = 5.0.0     # replaces MediatR — mediator + messaging
WOLVERINE_EF_VERSION        = 5.0.0     # WolverineFx.EntityFrameworkCore, keep == WOLVERINE_VERSION
WOLVERINE_POSTGRES_VERSION  = 5.0.0
FLUENTVALIDATION_VERSION    = 11.9.2
ERROROR_VERSION             = 2.0.1
MAPSTER_VERSION             = 7.4.0
MAPSTER_DI_VERSION          = 1.0.1

# Optional module: Redis
REDIS_VERSION               = 2.8.24    # StackExchange.Redis, versions independently
REDIS_CACHING_VERSION       = 9.0.9     # Microsoft.Extensions.Caching.StackExchangeRedis, track EF_VERSION line

# Optional module: Elasticsearch
ELASTIC_VERSION             = 8.15.10   # Elastic.Clients.Elasticsearch (new official client, not NEST)

# Optional module: Messaging (CAP, used as outbox + event bus over RabbitMQ)
CAP_VERSION                 = 8.3.5     # DotNetCore.CAP core + .RabbitMQ + .PostgreSql must match

.DEFAULT_GOAL := run

# --------------------------------------------------------------------------
# Project layout:
#   host/  -> BookingHotel.Api                        (Presentation layer)
#   src/   -> BookingHotel.Contracts                  (Request/Response DTOs)
#             BookingHotel.Application                (Use Cases, CQRS via Wolverine, Validation)
#             BookingHotel.Domain                     (Entities, Aggregates, Interfaces)
#             BookingHotel.Infrastructure.Persistence (EF Core, Repositories, Migrations)
#
#   Optional (add on demand, not part of `make init`):
#             BookingHotel.Infrastructure.Caching     (Redis)
#             BookingHotel.Infrastructure.Search      (Elasticsearch)
#             BookingHotel.Infrastructure.Messaging   (CAP: outbox + RabbitMQ event bus)
# --------------------------------------------------------------------------

# --------------------------------------------------------------------------
# Solution & Project scaffolding
# Run once: make init
# --------------------------------------------------------------------------

# Project reference graph (Clean Architecture), base solution:
#
#   Domain  <--  Application  <--  Infrastructure.Persistence
#
#   Contracts <---- Api
#                   |
#                   +---- Application
#                   +---- Infrastructure.Persistence
#
# Contracts:              plain POCOs, no references.
# Domain:                 no references.
# Application:            references Domain only.
# Infrastructure.Persistence: references Application + Domain.
# Api:                    references Application + Infrastructure.Persistence + Contracts.
#
# Optional modules (Caching / Search / Messaging), once added:
#   Application <- Infrastructure.<Module>
#   Domain      <- Infrastructure.<Module>
#   Api         -> Infrastructure.<Module>
#

.PHONY: sdk-check
sdk-check:
	@V=$$(dotnet --version); \
	case "$$V" in \
	  8.*) echo "dotnet SDK $$V OK (net8.0)";; \
	  *) echo "WARNING: dotnet SDK is $$V, this Makefile targets net8.0. Package versions may not resolve.";; \
	esac

.PHONY: new-sln
new-sln:
	dotnet new sln --name $(PREFIX) --output .
	@echo "Created solution $(SLN)"

.PHONY: new-projects
new-projects:
	dotnet new webapi   -n $(PREFIX).Api                        -o $(API_DIR)                        --no-openapi
	dotnet new classlib -n $(PREFIX).Contracts                  -o src/$(PREFIX).Contracts
	dotnet new classlib -n $(PREFIX).Application                -o src/$(PREFIX).Application
	dotnet new classlib -n $(PREFIX).Domain                     -o src/$(PREFIX).Domain
	dotnet new classlib -n $(PREFIX).Infrastructure.Persistence -o src/$(PREFIX).Infrastructure.Persistence
	@echo "Base projects created (Api + Contracts + Application + Domain + Infrastructure.Persistence)"

.PHONY: sln-add
sln-add:
	dotnet sln $(SLN) add $(API_PROJ)
	dotnet sln $(SLN) add $(CONTRACTS_PROJ)
	dotnet sln $(SLN) add $(APPLICATION_PROJ)
	dotnet sln $(SLN) add $(DOMAIN_PROJ)
	dotnet sln $(SLN) add $(PERSIST_PROJ)
	@echo "All base projects registered in $(SLN)"

.PHONY: ref
ref:
	@echo "Setting up project references..."
	dotnet add $(APPLICATION_PROJ) reference $(DOMAIN_PROJ)
	dotnet add $(PERSIST_PROJ) reference $(APPLICATION_PROJ)
	dotnet add $(PERSIST_PROJ) reference $(DOMAIN_PROJ)
	dotnet add $(API_PROJ) reference $(APPLICATION_PROJ)
	dotnet add $(API_PROJ) reference $(PERSIST_PROJ)
	dotnet add $(API_PROJ) reference $(CONTRACTS_PROJ)
	@echo "Done. Dependency graph:"
	@echo "  Domain <- Application <- Infrastructure.Persistence"
	@echo "  Contracts <- Api <- Application"
	@echo "               Api <- Infrastructure.Persistence"
	@echo ""
	@echo "Optional (run on demand):"
	@echo "  make add-redis        Redis caching module"
	@echo "  make add-elastic      Elasticsearch search module"
	@echo "  make add-messaging    CAP (outbox + RabbitMQ event bus) module"

# Install default packages for the 4 base layers + minimal Api needs.
# All versions pinned via the variables above, all net8.0 compatible.
.PHONY: add-packages
add-packages:
	@echo "Installing Domain packages..."
	dotnet add $(DOMAIN_PROJ) package ErrorOr --version $(ERROROR_VERSION)
	@echo "Installing Application packages..."
	dotnet add $(APPLICATION_PROJ) package WolverineFx --version $(WOLVERINE_VERSION)
	dotnet add $(APPLICATION_PROJ) package FluentValidation.DependencyInjectionExtensions --version $(FLUENTVALIDATION_VERSION)
	dotnet add $(APPLICATION_PROJ) package ErrorOr --version $(ERROROR_VERSION)
	dotnet add $(APPLICATION_PROJ) package Mapster --version $(MAPSTER_VERSION)
	dotnet add $(APPLICATION_PROJ) package Mapster.DependencyInjection --version $(MAPSTER_DI_VERSION)
	@echo "Installing Infrastructure.Persistence packages..."
	dotnet add $(PERSIST_PROJ) package Microsoft.CodeAnalysis.Common --version $(CODEANALYSIS_VERSION)
	dotnet add $(PERSIST_PROJ) package Microsoft.CodeAnalysis.Workspaces.Common --version $(CODEANALYSIS_VERSION)
	dotnet add $(PERSIST_PROJ) package Microsoft.EntityFrameworkCore --version $(EF_VERSION)
	dotnet add $(PERSIST_PROJ) package Microsoft.EntityFrameworkCore.Relational --version $(EF_VERSION)
	dotnet add $(PERSIST_PROJ) package Npgsql.EntityFrameworkCore.PostgreSQL --version $(NPGSQL_EF_VERSION)
	dotnet add $(PERSIST_PROJ) package Microsoft.EntityFrameworkCore.Tools --version $(EF_VERSION)
	dotnet add $(PERSIST_PROJ) package Microsoft.Extensions.Configuration.Abstractions --version $(CONFIG_ABSTRACTIONS_VERSION)
	dotnet add $(PERSIST_PROJ) package Microsoft.Extensions.Hosting.Abstractions --version $(HOSTING_VERSION)
	dotnet add $(PERSIST_PROJ) package WolverineFx.EntityFrameworkCore --version $(WOLVERINE_EF_VERSION)
	@echo "Installing Api packages..."
	dotnet add $(API_PROJ) package Swashbuckle.AspNetCore
	dotnet add $(API_PROJ) package Serilog.AspNetCore --version $(SERILOG_ASPNET_VERSION)
	dotnet add $(API_PROJ) package Serilog.Sinks.Console --version $(SERILOG_CONSOLE_VERSION)
	dotnet add $(API_PROJ) package Microsoft.AspNetCore.Authentication.JwtBearer --version $(JWT_VERSION)
	dotnet add $(API_PROJ) package Microsoft.EntityFrameworkCore.Design --version $(EF_VERSION)
	dotnet add $(API_PROJ) package Microsoft.Extensions.Configuration.Abstractions --version $(CONFIG_ABSTRACTIONS_VERSION)
	dotnet add $(API_PROJ) package WolverineFx.Postgresql --version $(WOLVERINE_POSTGRES_VERSION)
	@echo "All base packages installed"

# Full setup from scratch
.PHONY: init
init: sdk-check new-sln new-projects sln-add ref add-packages restore
	@echo ""
	@echo "========================================"
	@echo "  $(PREFIX) solution is ready!"
	@echo "========================================"
	@echo "Base layers: Domain, Contracts, Application, Infrastructure.Persistence (+ Api host)"
	@echo "Next: make run"
	@echo "Optional: make add-redis | make add-elastic | make add-messaging"

# --------------------------------------------------------------------------
# Optional module: Redis caching
# Usage: make add-redis      -> create project, wire references, install packages
#        make remove-redis   -> remove references + sln entry + delete project
# --------------------------------------------------------------------------

.PHONY: add-redis
add-redis:
	@echo "Adding Infrastructure.Caching (Redis) project..."
	@if [ ! -f "$(CACHING_PROJ)" ]; then \
	    dotnet new classlib -n $(PREFIX).Infrastructure.Caching -o $(CACHING_DIR); \
	    echo "Created $(PREFIX).Infrastructure.Caching"; \
	else \
	    echo "$(PREFIX).Infrastructure.Caching already exists. Skipping project creation."; \
	fi
	@dotnet sln $(SLN) list | grep -q "$(PREFIX).Infrastructure.Caching.csproj" || \
	    dotnet sln $(SLN) add $(CACHING_PROJ)
	@dotnet list $(CACHING_PROJ) reference | grep -q "$(PREFIX).Application.csproj" || \
	    dotnet add $(CACHING_PROJ) reference $(APPLICATION_PROJ)
	@dotnet list $(CACHING_PROJ) reference | grep -q "$(PREFIX).Domain.csproj" || \
	    dotnet add $(CACHING_PROJ) reference $(DOMAIN_PROJ)
	@dotnet list $(API_PROJ) reference | grep -q "$(PREFIX).Infrastructure.Caching.csproj" || \
	    dotnet add $(API_PROJ) reference $(CACHING_PROJ)
	@echo "Installing Redis packages..."
	@dotnet list $(CACHING_PROJ) package | grep -q "StackExchange.Redis" || \
	    dotnet add $(CACHING_PROJ) package StackExchange.Redis --version $(REDIS_VERSION)
	@dotnet list $(CACHING_PROJ) package | grep -q "Microsoft.Extensions.Caching.StackExchangeRedis" || \
	    dotnet add $(CACHING_PROJ) package Microsoft.Extensions.Caching.StackExchangeRedis --version $(REDIS_CACHING_VERSION)
	@dotnet list $(CACHING_PROJ) package | grep -q "Microsoft.Extensions.Configuration.Abstractions" || \
	    dotnet add $(CACHING_PROJ) package Microsoft.Extensions.Configuration.Abstractions --version $(CONFIG_ABSTRACTIONS_VERSION)
	@dotnet list $(CACHING_PROJ) package | grep -q "Microsoft.Extensions.Options.ConfigurationExtensions" || \
	    dotnet add $(CACHING_PROJ) package Microsoft.Extensions.Options.ConfigurationExtensions --version $(OPTIONS_CONFIG_VERSION)
	@dotnet restore
	@dotnet build
	@echo "Redis caching module added. Dependency graph added:"
	@echo "  Application <- Infrastructure.Caching, Domain <- Infrastructure.Caching, Api -> Infrastructure.Caching"

.PHONY: remove-redis
remove-redis:
	@if [ ! -f "$(CACHING_PROJ)" ]; then \
	    echo "Infrastructure.Caching not found. Nothing to remove."; exit 0; \
	fi
	@echo "Removing Redis caching module..."
	-dotnet remove $(API_PROJ) reference $(CACHING_PROJ)
	-dotnet sln $(SLN) remove $(CACHING_PROJ)
	rm -rf $(CACHING_DIR)
	@dotnet restore
	@dotnet build
	@echo "Infrastructure.Caching removed cleanly (Api reference, sln entry, and project files)."

# --------------------------------------------------------------------------
# Optional module: Elasticsearch search
# Usage: make add-elastic    -> create project, wire references, install packages
#        make remove-elastic -> remove references + sln entry + delete project
# --------------------------------------------------------------------------

.PHONY: add-elastic
add-elastic:
	@echo "Adding Infrastructure.Search (Elasticsearch) project..."
	@if [ ! -f "$(SEARCH_PROJ)" ]; then \
	    dotnet new classlib -n $(PREFIX).Infrastructure.Search -o $(SEARCH_DIR); \
	    echo "Created $(PREFIX).Infrastructure.Search"; \
	else \
	    echo "$(PREFIX).Infrastructure.Search already exists. Skipping project creation."; \
	fi
	@dotnet sln $(SLN) list | grep -q "$(PREFIX).Infrastructure.Search.csproj" || \
	    dotnet sln $(SLN) add $(SEARCH_PROJ)
	@dotnet list $(SEARCH_PROJ) reference | grep -q "$(PREFIX).Application.csproj" || \
	    dotnet add $(SEARCH_PROJ) reference $(APPLICATION_PROJ)
	@dotnet list $(SEARCH_PROJ) reference | grep -q "$(PREFIX).Domain.csproj" || \
	    dotnet add $(SEARCH_PROJ) reference $(DOMAIN_PROJ)
	@dotnet list $(API_PROJ) reference | grep -q "$(PREFIX).Infrastructure.Search.csproj" || \
	    dotnet add $(API_PROJ) reference $(SEARCH_PROJ)
	@echo "Installing Elasticsearch packages..."
	@dotnet list $(SEARCH_PROJ) package | grep -q "Elastic.Clients.Elasticsearch" || \
	    dotnet add $(SEARCH_PROJ) package Elastic.Clients.Elasticsearch --version $(ELASTIC_VERSION)
	@dotnet list $(SEARCH_PROJ) package | grep -q "Microsoft.Extensions.Configuration.Abstractions" || \
	    dotnet add $(SEARCH_PROJ) package Microsoft.Extensions.Configuration.Abstractions --version $(CONFIG_ABSTRACTIONS_VERSION)
	@dotnet restore
	@dotnet build
	@echo "Elasticsearch search module added. Dependency graph added:"
	@echo "  Application <- Infrastructure.Search, Domain <- Infrastructure.Search, Api -> Infrastructure.Search"

.PHONY: remove-elastic
remove-elastic:
	@if [ ! -f "$(SEARCH_PROJ)" ]; then \
	    echo "Infrastructure.Search not found. Nothing to remove."; exit 0; \
	fi
	@echo "Removing Elasticsearch search module..."
	-dotnet remove $(API_PROJ) reference $(SEARCH_PROJ)
	-dotnet sln $(SLN) remove $(SEARCH_PROJ)
	rm -rf $(SEARCH_DIR)
	@dotnet restore
	@dotnet build
	@echo "Infrastructure.Search removed cleanly (Api reference, sln entry, and project files)."

# --------------------------------------------------------------------------
# Optional module: Messaging (CAP — outbox pattern + RabbitMQ event bus)
# Usage: make add-messaging    -> create project, wire references, install packages
#        make remove-messaging -> remove references + sln entry + delete project
# Note: CAP creates/manages its own outbox tables automatically at startup,
#       no EF migration needed for CAP itself.
# --------------------------------------------------------------------------

.PHONY: add-messaging
add-messaging:
	@echo "Adding Infrastructure.Messaging (CAP) project..."
	@if [ ! -f "$(MESSAGING_PROJ)" ]; then \
	    dotnet new classlib -n $(PREFIX).Infrastructure.Messaging -o $(MESSAGING_DIR); \
	    echo "Created $(PREFIX).Infrastructure.Messaging"; \
	else \
	    echo "$(PREFIX).Infrastructure.Messaging already exists. Skipping project creation."; \
	fi
	@dotnet sln $(SLN) list | grep -q "$(PREFIX).Infrastructure.Messaging.csproj" || \
	    dotnet sln $(SLN) add $(MESSAGING_PROJ)
	@dotnet list $(MESSAGING_PROJ) reference | grep -q "$(PREFIX).Application.csproj" || \
	    dotnet add $(MESSAGING_PROJ) reference $(APPLICATION_PROJ)
	@dotnet list $(MESSAGING_PROJ) reference | grep -q "$(PREFIX).Domain.csproj" || \
	    dotnet add $(MESSAGING_PROJ) reference $(DOMAIN_PROJ)
	@dotnet list $(API_PROJ) reference | grep -q "$(PREFIX).Infrastructure.Messaging.csproj" || \
	    dotnet add $(API_PROJ) reference $(MESSAGING_PROJ)
	@echo "Installing CAP packages (core + RabbitMQ transport + Postgres outbox storage)..."
	@dotnet list $(MESSAGING_PROJ) package | grep -q "DotNetCore.CAP\b" || \
	    dotnet add $(MESSAGING_PROJ) package DotNetCore.CAP --version $(CAP_VERSION)
	@dotnet list $(MESSAGING_PROJ) package | grep -q "DotNetCore.CAP.RabbitMQ" || \
	    dotnet add $(MESSAGING_PROJ) package DotNetCore.CAP.RabbitMQ --version $(CAP_VERSION)
	@dotnet list $(MESSAGING_PROJ) package | grep -q "DotNetCore.CAP.PostgreSql" || \
	    dotnet add $(MESSAGING_PROJ) package DotNetCore.CAP.PostgreSql --version $(CAP_VERSION)
	@dotnet list $(MESSAGING_PROJ) package | grep -q "Microsoft.Extensions.Configuration.Abstractions" || \
	    dotnet add $(MESSAGING_PROJ) package Microsoft.Extensions.Configuration.Abstractions --version $(CONFIG_ABSTRACTIONS_VERSION)
	@dotnet restore
	@dotnet build
	@echo ""
	@echo "Infrastructure.Messaging (CAP) added successfully."
	@echo "Dependency graph added:"
	@echo "  Application <- Infrastructure.Messaging"
	@echo "  Domain      <- Infrastructure.Messaging"
	@echo "  Api         -> Infrastructure.Messaging"
	@echo "Remember to wire x.UseRabbitMQ(...) / x.UsePostgreSql(...) in your composition root."

.PHONY: remove-messaging
remove-messaging:
	@if [ ! -f "$(MESSAGING_PROJ)" ]; then \
	    echo "Infrastructure.Messaging not found. Nothing to remove."; exit 0; \
	fi
	@echo "Removing Messaging (CAP) module..."
	-dotnet remove $(API_PROJ) reference $(MESSAGING_PROJ)
	-dotnet sln $(SLN) remove $(MESSAGING_PROJ)
	rm -rf $(MESSAGING_DIR)
	@dotnet restore
	@dotnet build
	@echo "Infrastructure.Messaging removed cleanly (Api reference, sln entry, and project files)."

# --------------------------------------------------------------------------
# Build & Run
# --------------------------------------------------------------------------

.PHONY: build
build:
	dotnet build

.PHONY: run
run:
	dotnet run --project $(API_PROJ) --launch-profile https

.PHONY: watch
watch:
	dotnet watch run --project $(API_PROJ) --launch-profile https

.PHONY: clean
clean:
	dotnet clean
	@powershell -Command "Get-ChildItem -Recurse -Include bin,obj | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue"
	@echo "Cleaned bin and obj directories"

.PHONY: restore
restore:
	dotnet restore

.PHONY: kill
kill:
	@powershell -Command "Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force"
	@echo "Stopped all running dotnet processes"

# --------------------------------------------------------------------------
# NuGet package management
# Usage: make add pkg=Newtonsoft.Json to=Application
#        make add pkg=SomePackage to=Application ver=1.2.3
#        make remove pkg=Newtonsoft.Json to=Application
# Layers:
#   Api | Contracts | Application | Domain
#   Infrastructure.Persistence | Infrastructure.Caching | Infrastructure.Search | Infrastructure.Messaging
# Note: Api lives under host/, all others live under src/
#       Optional layers (Caching/Search/Messaging) must exist first
#       (see make add-redis / add-elastic / add-messaging).
# --------------------------------------------------------------------------

.PHONY: add
add:
	@if [ "$(pkg)" = "" ]; then \
	    echo "Error: missing 'pkg'. Example: make add pkg=Newtonsoft.Json to=Application"; exit 1; fi
	@if [ "$(to)" = "" ]; then \
	    echo "Error: missing 'to' (Api | Contracts | Application | Domain | Infrastructure.Persistence | Infrastructure.Caching | Infrastructure.Search | Infrastructure.Messaging)"; exit 1; fi
	@if [ "$(to)" = "Api" ]; then \
	    dotnet add $(API_PROJ) package $(pkg) $(if $(ver),--version $(ver),); \
	else \
	    dotnet add src/$(PREFIX).$(to)/$(PREFIX).$(to).csproj package $(pkg) $(if $(ver),--version $(ver),); \
	fi

.PHONY: remove
remove:
	@if [ "$(pkg)" = "" ]; then \
	    echo "Error: missing 'pkg'. Example: make remove pkg=Newtonsoft.Json to=Application"; exit 1; fi
	@if [ "$(to)" = "" ]; then \
	    echo "Error: missing 'to' (Api | Contracts | Application | Domain | Infrastructure.Persistence | Infrastructure.Caching | Infrastructure.Search | Infrastructure.Messaging)"; exit 1; fi
	@if [ "$(to)" = "Api" ]; then \
	    dotnet remove $(API_PROJ) package $(pkg); \
	else \
	    dotnet remove src/$(PREFIX).$(to)/$(PREFIX).$(to).csproj package $(pkg); \
	fi

.PHONY: list-outdated
list-outdated:
	dotnet list package --outdated

# --------------------------------------------------------------------------
# EF Core migrations (targets Infrastructure.Persistence)
# Usage: make migration name=InitialCreate
#        make db-update
#        make db-rollback name=<target migration>
#        make migration-remove    <- removes last unapplied migration
# --------------------------------------------------------------------------

.PHONY: migration
migration:
ifeq ($(strip $(name)),)
	$(error missing migration name. Example: make migration name=InitialCreate)
endif
	dotnet ef migrations add $(name) \
	    --project $(PERSIST_PROJ) \
	    --startup-project $(API_PROJ)

.PHONY: db-update
db-update:
	dotnet ef database update \
	    --project $(PERSIST_PROJ) \
	    --startup-project $(API_PROJ)

.PHONY: db-rollback
db-rollback:
	@if [ "$(name)" = "" ]; then \
	    echo "Error: specify target migration. Example: make db-rollback name=InitialCreate"; exit 1; fi
	dotnet ef database update $(name) \
	    --project $(PERSIST_PROJ) \
	    --startup-project $(API_PROJ)

.PHONY: migration-remove
migration-remove:
	dotnet ef migrations remove \
	    --project $(PERSIST_PROJ) \
	    --startup-project $(API_PROJ)

.PHONY: db-drop
db-drop:
	dotnet ef database drop --force \
	    --project $(PERSIST_PROJ) \
	    --startup-project $(API_PROJ)

# --------------------------------------------------------------------------
# Utilities
# --------------------------------------------------------------------------

.PHONY: list-refs
list-refs:
	@echo "=== Project References ==="
	@echo "-- Api --"
	@dotnet list $(API_PROJ) reference
	@echo "-- Application --"
	@dotnet list $(APPLICATION_PROJ) reference
	@echo "-- Infrastructure.Persistence --"
	@dotnet list $(PERSIST_PROJ) reference
	@echo "-- Infrastructure.Caching --"
	@if [ -f "$(CACHING_PROJ)" ]; then dotnet list $(CACHING_PROJ) reference; else echo "Not added. Run: make add-redis"; fi
	@echo "-- Infrastructure.Search --"
	@if [ -f "$(SEARCH_PROJ)" ]; then dotnet list $(SEARCH_PROJ) reference; else echo "Not added. Run: make add-elastic"; fi
	@echo "-- Infrastructure.Messaging --"
	@if [ -f "$(MESSAGING_PROJ)" ]; then dotnet list $(MESSAGING_PROJ) reference; else echo "Not added. Run: make add-messaging"; fi
	@echo "-- Contracts (should be empty) --"
	@dotnet list $(CONTRACTS_PROJ) reference
	@echo "-- Domain (should be empty) --"
	@dotnet list $(DOMAIN_PROJ) reference

.PHONY: list-pkgs
list-pkgs:
	dotnet list package

.PHONY: format
format:
	dotnet format

# --------------------------------------------------------------------------
# Infrastructure (Podman Compose)
# --------------------------------------------------------------------------
PODMAN_COMPOSE := podman compose
# Nếu bạn dùng công cụ podman-compose (Python), hãy đổi thành:
# PODMAN_COMPOSE := podman-compose

.PHONY: infra-up
infra-up:
	$(PODMAN_COMPOSE) up -d
	@echo "Infrastructure services started via Podman."

.PHONY: infra-down
infra-down:
	$(PODMAN_COMPOSE) down
	@echo "Infrastructure services stopped and removed."

.PHONY: infra-logs
infra-logs:
	$(PODMAN_COMPOSE) logs -f

.PHONY: infra-status
infra-status:
	$(PODMAN_COMPOSE) ps

.PHONY: help
help:
	@echo ""
	@echo "================================================================"
	@echo "  $(PREFIX) - Makefile Commands"
	@echo "================================================================"
	@echo "  SCAFFOLDING (run once)"
	@echo "    make init                    Full setup: Domain, Contracts, Application,"
	@echo "                                  Infrastructure.Persistence (+ Api host)"
	@echo "    -- or step by step --"
	@echo "    make sdk-check                Verify dotnet SDK is 8.x"
	@echo "    make new-sln                  Create .sln file"
	@echo "    make new-projects             Create base 5 projects (host/ + src/)"
	@echo "    make sln-add                  Register projects in .sln"
	@echo "    make ref                      Set up project references"
	@echo "    make add-packages             Install default packages (incl. Wolverine)"
	@echo "----------------------------------------------------------------"
	@echo "  OPTIONAL MODULES (add/remove independently, on demand)"
	@echo "    make add-redis                Add Infrastructure.Caching (Redis)"
	@echo "    make remove-redis             Remove it cleanly (refs + sln + files)"
	@echo "    make add-elastic              Add Infrastructure.Search (Elasticsearch)"
	@echo "    make remove-elastic           Remove it cleanly (refs + sln + files)"
	@echo "    make add-messaging            Add Infrastructure.Messaging (CAP + RabbitMQ)"
	@echo "    make remove-messaging         Remove it cleanly (refs + sln + files)"
	@echo "----------------------------------------------------------------"
	@echo "  INFRASTRUCTURE (PODMAN)"
	@echo "    make infra-up                Start Postgres, Redis, RabbitMQ, etc."
	@echo "    make infra-down              Stop and remove containers"
	@echo "    make infra-logs              Stream container logs"
	@echo "    make infra-status            Check container status"
	@echo "----------------------------------------------------------------"
	@echo "  BUILD & RUN"
	@echo "    make run                     Run API (https profile)"
	@echo "    make watch                   Hot reload"
	@echo "    make build                   Build solution"
	@echo "    make clean                   Remove bin/obj"
	@echo "    make kill                    Kill stray dotnet processes"
	@echo "----------------------------------------------------------------"
	@echo "  PACKAGES"
	@echo "    make add pkg=X to=Y          Add package to layer Y"
	@echo "    make add pkg=X to=Y ver=Z    Add specific version"
	@echo "    make remove pkg=X to=Y       Remove package from layer Y"
	@echo "    make list-pkgs               List all packages"
	@echo "    make list-outdated           Check for outdated packages"
	@echo "  Layers: Api | Contracts | Application | Domain"
	@echo "          Infrastructure.Persistence | Infrastructure.Caching |"
	@echo "          Infrastructure.Search | Infrastructure.Messaging"
	@echo "----------------------------------------------------------------"
	@echo "  DATABASE / EF CORE"
	@echo "    make migration name=X        Create a new migration"
	@echo "    make db-update                Apply pending migrations"
	@echo "    make db-rollback name=X      Roll back to migration X"
	@echo "    make migration-remove        Remove last unapplied migration"
	@echo "    make db-drop                  Drop the database"
	@echo "----------------------------------------------------------------"
	@echo "  UTILITIES"
	@echo "    make list-refs                Show all project references"
	@echo "    make format                   Run dotnet format"
	@echo "    make help                     Show this menu"
	@echo "================================================================"
	@echo ""
