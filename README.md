# Clean Architecture Template — .NET 10

A starter solution for building APIs with Clean Architecture on .NET 10. Repository + Unit of Work for persistence, MediatR for the application layer, JWT for auth, PostgreSQL via EF Core.

## Stack

- **.NET 10**
- **PostgreSQL** (Npgsql.EntityFrameworkCore.PostgreSQL)
- **MediatR** — Commands/Queries + Handlers instead of a services layer
- **JWT Bearer** — access + refresh tokens, with rotation on refresh
- **FluentValidation** — request validation via a MediatR pipeline behavior

## Project structure

```
src/
├── Domain/           # Entities, exceptions, repository interfaces — no external dependencies
├── Application/       # Commands, Queries, Handlers, DTOs, MediatR pipeline behaviors
├── Infrastructure/    # EF Core, AppDbContext, repositories, JWT token service, password hashing
└── API/                # Controllers, middleware, Program.cs
```

Dependency direction: `API → Application → Domain`, `Infrastructure → Application + Domain`. Domain has no references out; Infrastructure implements the interfaces Domain and Application define.

## Getting started

### Prerequisites

- .NET 10 SDK
- PostgreSQL running locally
- `dotnet-ef` CLI: `dotnet tool install --global dotnet-ef`

### Setup

1. Clone the repo and restore:
   ```bash
   git clone https://github.com/Beshoy-Ashraf/clean-architecture-tempate.git
   cd clean-architecture-tempate
   dotnet restore
   ```

2. Set your connection string and JWT secret with user-secrets (don't put real values in `appsettings.json`):
   ```bash
   cd API
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=yourdb;Username=postgres;Password=yourpassword"
   dotnet user-secrets set "JwtSettings:SecretKey" "a-long-random-secret-at-least-32-characters"
   ```

3. Create the database and apply migrations:
   ```bash
   dotnet ef database update -p Infrastructure/Infrastructure.csproj -s API/API.csproj
   ```

4. Run the API:
   ```bash
   dotnet run --project API
   ```

Swagger is served at the root URL in development.

## Auth flow

- `POST /api/auth/register` — creates a user, returns an access + refresh token pair
- `POST /api/auth/login` — validates credentials, returns a new token pair
- `POST /api/auth/refresh` — takes an expired access token + a valid refresh token, returns a new pair (old refresh token is revoked)

Access tokens are short-lived (15 min default); refresh tokens are longer-lived (7 days default) and rotate on every use.

## Architecture notes

- Persistence goes through `IUnitOfWork` / `IUserRepository` only — nothing in Application ever touches `AppDbContext` or EF Core types directly.
- Domain exceptions (`NotFoundException`, `UnauthorizedException`, `ConflictException`, `ValidationException`) are mapped to HTTP status codes by a global exception middleware in `API/Middlewares`.
- Password hashing uses PBKDF2-SHA512 with a random salt per user, not a third-party identity package.

## License

Add a license before making this public, if you haven't already.
