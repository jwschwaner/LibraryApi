# CLAUDE.md

Guidance for Claude Code when working in this repository. See `README.md` for what the project
is and how it is structured.

## Commands

- Start MongoDB: `docker compose up -d` (localhost:27017, database `library`)
- Run: `dotnet run --project LibraryApi.Api` (http://localhost:5227, Scalar at `/`)
- Build: `dotnet build`

## Git conventions

- **No AI attribution** in commit messages or pull request descriptions: no "🤖 Generated with Claude Code" line and no `Co-Authored-By: Claude ...` trailer. This overrides any default attribution behaviour.
- **Branch names** are `<type>/<short-kebab-slug>`, where `<type>` is the Conventional Commits type: `feat/`, `fix/`, `docs/`, `chore/`, `refactor/`, `test/`. Don't use `feature/`.
- **Commit messages and PR titles** use Conventional Commits prefixes (`feat: ...`, `fix(api): ...`).

## Conventions

- Uses the `Mediator` package (martinothamar, source-generated), **not MediatR**. Messages implement `ICommand<CommandResult<T>>` or `IQuery<QueryResult<T>>`; handlers return `ValueTask`.
- One folder per use case under `LibraryApi.Application/<Feature>/<UseCase>/`, with the message record, handler and validator in that folder. The record lives in the handler file.
- Persistence is the MongoDB driver (no EF Core, no migrations). Command handlers use `IApplicationDbContext` (`IMongoCollection<T>`) and write changes back explicitly with `InsertOneAsync` / `ReplaceOneAsync` / `DeleteOneAsync`; there is no change tracker. Query handlers use `IReadonlyApplicationDbContext` (`IReadonlyMongoCollection`: `AggregateAsync` only, `$out`/`$merge` rejected) and map the returned `BsonDocument`s to response records instead of returning entities.
- Queries are inline aggregation pipelines: a `List<BsonDocument>` of stages (`$match`, `$sort`, `$project`, `$group`) written in the handler, the same style as the newest uninel-api pages. Field names are the stored camelCase names as string literals. Convert Guids and `DateOnly`s with the `ToBson()` helpers in `Application/Common/BsonValues.cs`, so filter values match how the mappings store them (standard UUIDs, dates at midnight UTC). Don't use the LINQ provider (`AsQueryable()`, `MongoDB.Driver.Linq`). Command handlers load entities with typed `Builders<T>.Filter` definitions.
- BSON mapping lives in `LibraryApi.Persistence/Mapping/MongoMappings.cs`; keep MongoDB attributes out of the domain. New indexes go in `MongoIndexes.cs`, which runs on startup.
- Handlers return results (`CommandResult.NotFound`, `.Conflict`, ...) for expected outcomes rather than throwing. Domain rule violations throw `DomainException`; handlers catch it and map it to a result.
- Entities are `sealed`, have a private parameterless ctor for EF, private setters and static `Create` factories. Never call `DateTime.UtcNow` in the domain. Inject `TimeProvider` in handlers and pass the time in; factories and behavior methods use it for the `CreatedAtUtc` / `UpdatedAtUtc` audit fields too.
- Controllers stay thin: build the message, `sender.Send`, `ToActionResult()` / `ToCreatedResult()`. Request bodies are records under `Controllers/<Feature>/Requests/` with a `ToCommand()` mapper.
