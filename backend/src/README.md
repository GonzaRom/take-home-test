# Loan Management Backend MVP

This backend implements the Loan Management MVP API. The core flow supports creating loans, listing loans, viewing loan details, and registering payments against active loans.

This is an MVP for a take-home sprint deliverable, not a full banking platform.

## Tech stack

- ASP.NET Core / .NET 6
- Entity Framework Core
- SQL Server
- xUnit
- Docker / Docker Compose

## Project structure

```text
backend/src
├── src.sln
├── Fundo.Applications.WebApi      # ASP.NET Core API host, controllers, Swagger, CORS and startup configuration
├── Fundo.Application              # Loan application services, DTOs and service contracts
├── Fundo.Domain                   # Loan and payment domain model and business rules
├── Fundo.Infrastructure           # EF Core DbContext, repository and migrations
└── Fundo.Services.Tests           # xUnit domain and API integration tests
```

## API endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/loans` | Create a loan and return the persisted loan details, including payment history. |
| `GET` | `/loans` | List available loan summaries without payment history. |
| `GET` | `/loans/{id:guid}` | Retrieve one loan with payment history. |
| `POST` | `/loans/{id:guid}/payment` | Register a payment and return the updated loan with payment history. |

Swagger is available only when the API runs in `Development`.

## Running locally

Prerequisites:

- .NET 6 SDK
- SQL Server or LocalDB for non-Docker database runs
- EF Core CLI if applying migrations from the host

From the repository root:

```powershell
dotnet restore .\backend\src\src.sln
dotnet build .\backend\src\src.sln --no-restore
dotnet run --project .\backend\src\Fundo.Applications.WebApi\Fundo.Applications.WebApi.csproj
```

The default non-Docker connection string in `Fundo.Applications.WebApi/appsettings.json` points to LocalDB:

```text
Server=(localdb)\mssqllocaldb;Database=FundoLoans;Trusted_Connection=True;TrustServerCertificate=True
```

Swagger URL depends on the active local launch profile or hosting URL. In Docker with default ports, it is available at `http://localhost:5080/swagger`.

## Database

The backend uses EF Core with SQL Server persistence. The initial migration creates `Loans` and `Payments`, including a foreign key from payments to loans and seed data for three active loans.

Apply migrations manually from the repository root:

```powershell
dotnet ef database update --project .\backend\src\Fundo.Infrastructure\Fundo.Infrastructure.csproj --startup-project .\backend\src\Fundo.Applications.WebApi\Fundo.Applications.WebApi.csproj
```

`appsettings.json` keeps `Database:AutoMigrate` set to `false` by default. Automatic startup migrations are intended only for local Docker review, where `docker-compose.yml` sets `ASPNETCORE_ENVIRONMENT=Development` and `Database__AutoMigrate=true` for the API container.

Migration retry behavior is configured with `Database:MigrationMaxAttempts` and `Database:MigrationRetryDelaySeconds`.

## Docker

Docker Compose is defined at the repository root. The stack runs:

- `niuro-sqlserver`: SQL Server 2022 Developer edition with a persistent project volume.
- `niuro-api`: ASP.NET Core API built from `backend/src/Fundo.Applications.WebApi/Dockerfile`.

Build and start the stack from the repository root:

```powershell
docker compose -p niuro-loan-mvp build
docker compose -p niuro-loan-mvp up -d
```

Stop the stack without deleting the SQL Server volume:

```powershell
docker compose -p niuro-loan-mvp down
```

Default Docker URLs and ports:

- API base URL: `http://localhost:5080`
- Swagger UI: `http://localhost:5080/swagger`
- SQL Server host endpoint: `127.0.0.1,14333`

The API container listens on port `8080` and maps to host `${API_HTTP_PORT:-5080}`. SQL Server maps to host `${SQLSERVER_HOST_PORT:-14333}`.

The compose file also supports these local-development overrides:

- `API_HTTP_PORT`: host port for the API.
- `SQLSERVER_HOST_PORT`: host port for SQL Server.
- `SQLSERVER_DATABASE`: database name. Default: `FundoLoans`.
- `MSSQL_SA_PASSWORD`: local SQL Server SA password. Default: `NiuroLocal#2026!`.
- `FRONTEND_ORIGIN`: allowed Angular development origin. Default: `http://localhost:4200`.

Inside the Docker network, the API connects to SQL Server at `niuro-sqlserver,1433`. When Docker starts the API in `Development` with `Database__AutoMigrate=true`, pending EF Core migrations run during startup and seed the configured loan data.

If the SQL Server password is changed after the project volume exists, SQL Server keeps the original password stored in that volume. To intentionally discard only this project database volume:

```powershell
docker compose -p niuro-loan-mvp down
docker volume rm niuro-loan-mvp_niuro_sqlserver_data
```

Do not use global Docker prune commands for this project cleanup.

## Testing

Run the backend test project from the repository root:

```powershell
dotnet test .\backend\src\Fundo.Services.Tests\Fundo.Services.Tests.csproj --no-restore
```

The test project includes domain unit tests for loan rules and API integration tests for `LoanManagementController`. Integration tests use `WebApplicationFactory` with EF Core InMemory.

Useful filters:

```powershell
dotnet test .\backend\src\Fundo.Services.Tests\Fundo.Services.Tests.csproj --filter "FullyQualifiedName~LoanTests"
dotnet test .\backend\src\Fundo.Services.Tests\Fundo.Services.Tests.csproj --filter "FullyQualifiedName~LoanManagementControllerTests"
```

## Design notes and tradeoffs

- The backend uses a lightweight layered structure: WebApi, Application, Domain and Infrastructure.
- Controllers stay thin and delegate loan workflows to application services.
- The list contract uses `LoanSummaryDto` and intentionally omits payment history; `LoanDetailsDto` is returned by detail, create and payment flows when payment history is needed.
- Loan creation and payment rules live outside controllers in the domain/application flow.
- EF Core handles SQL Server persistence and migrations.
- Authentication and authorization are not implemented in this MVP.
- Payment registration reduces the current balance directly; there is no amortization engine yet.
- There is no payment provider integration.
- Advanced concurrency handling for simultaneous payments is not implemented yet.

## Future improvements

- Add authentication and authorization.
- Add pagination and filtering for loan lists.
- Add optimistic concurrency for payments.
- Add an audit trail for loan and payment changes.
- Add an amortization schedule.
