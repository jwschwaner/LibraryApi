# CLAUDE.md

Guidance for Claude Code when working in this repository. See `README.md` for what the project
is and how it is structured.

## Commands

- Start MongoDB: `docker compose up -d` (localhost:27017, database `library`)
- Run: `dotnet run --project LibraryApi.Api` (http://localhost:5227, Scalar at `/`)
- Build: `dotnet build`
- Test: `dotnet test` (needs Docker for the integration tests); `dotnet test LibraryApi.UnitTests` for the fast loop

## Git conventions

- **No AI attribution** in commit messages or pull request descriptions: no "🤖 Generated with Claude Code" line and no `Co-Authored-By: Claude ...` trailer. This overrides any default attribution behaviour.
- **Branch names** are `<type>/<short-kebab-slug>`, where `<type>` is the Conventional Commits type: `feat/`, `fix/`, `docs/`, `chore/`, `refactor/`, `test/`. Don't use `feature/`.
- **Commit messages and PR titles** use Conventional Commits prefixes (`feat: ...`, `fix(api): ...`).

## Backend guidelines

This project is the reference implementation of the team's `kamu:backend-guidelines` skill
(from the `kamu-claude-skills` repo), and the skill's code snippets are taken from it. New code
follows the skill. Load it before adding a use case, endpoint, entity or integration test.
Keep the two in step: when a pattern here changes, the skill needs the same change, and the
other way round.

## Conventions

- Uses the `Mediator` package (martinothamar, source-generated), **not MediatR**. Messages implement `ICommand<CommandResult<T>>` or `IQuery<QueryResult<T>>`; handlers return `ValueTask`.
- One folder per use case under `LibraryApi.Application/<Feature>/<UseCase>/`, with the message record, handler and validator in that folder. The record lives in the handler file.
- Persistence is the MongoDB driver (no EF Core, no migrations). Command handlers use `IApplicationDbContext` (`IMongoCollection<T>`) and write changes back explicitly with `InsertOneAsync` / `ReplaceOneAsync` / `DeleteOneAsync`; there is no change tracker. Query handlers use `IReadonlyApplicationDbContext` (`IReadonlyMongoCollection`: `AggregateAsync` only, `$out`/`$merge` rejected) and map the returned `BsonDocument`s to response records instead of returning entities.
- Queries are inline aggregation pipelines: a `List<BsonDocument>` of stages (`$match`, `$sort`, `$project`, `$group`) written in the handler. Field names are the stored camelCase names as string literals. Convert Guids and `DateOnly`s with the `ToBson()` helpers in `Application/Common/BsonValues.cs`, so filter values match how the mappings store them (standard UUIDs, dates at midnight UTC). Don't use the LINQ provider (`AsQueryable()`, `MongoDB.Driver.Linq`). Command handlers load entities with typed `Builders<T>.Filter` definitions.
- BSON mapping lives in `LibraryApi.Persistence/Mapping/MongoMappings.cs`; keep MongoDB attributes out of the domain. New indexes go in `MongoIndexes.cs`, which runs on startup.
- Handlers return results (`CommandResult.NotFound`, `.Conflict`, ...) for expected outcomes rather than throwing. Domain rule violations throw `DomainException`; handlers catch it and map it to a result.
- Entities are `sealed`, have a private parameterless ctor so the MongoDB driver can materialise them, private setters and static `Create` factories. Never call `DateTime.UtcNow` in the domain. Inject `TimeProvider` in handlers and pass the time in; factories and behavior methods use it for the `CreatedAtUtc` / `UpdatedAtUtc` audit fields too.
- Controllers stay thin: build the message, `sender.Send`, `ToActionResult()` / `ToCreatedResult()`. Request bodies are records under `Controllers/<Feature>/Requests/` with a `ToCommand()` mapper.
- Every new endpoint gets integration tests in `LibraryApi.IntegrationTests/<Feature>/<UseCase>Tests.cs`: they are the only thing that checks query pipelines (string field names) against a real database. Domain rules get unit tests in `LibraryApi.UnitTests`, without mocks. Seed data with the builders in `LibraryApi.Tests.Common/Builders` (add a `WithX()` rather than constructing entities by hand), name tests `Given_..._When_..._Then_...` with Arrange/Act/Assert sections, and assert HTTP outcomes with `CommonAssertions`. See the Tests section of README.md.
