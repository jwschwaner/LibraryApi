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
- Persistence is the MongoDB driver (no EF Core, no migrations). Command handlers use `IApplicationDbContext` (`IMongoCollection<T>`) and write changes back explicitly with `InsertOneAsync` / `ReplaceOneAsync` / `DeleteOneAsync`; there is no change tracker. Query handlers use `IReadonlyApplicationDbContext` (`IReadonlyMongoCollection<T>`: `AggregateAsync` only, `$out`/`$merge` rejected) and project to response records instead of returning entities.
- Queries and commands use the driver's typesafe builders: `Builders<T>.Filter` / `.Sort` / `.Update`, and typed pipelines built from `new EmptyPipelineDefinition<T>()` with `.Match`, `.Sort`, `.Project`, `.Group`, `.Lookup`. Expression projections (`.Project(b => new BookResponse(...))`) are fine. Use a raw `BsonDocument` only for a stage or expression the builders can't express, as one stage inside the typed pipeline (`.AppendStage<T, T, TOut>(new BsonDocument(...))`), with a comment saying why. Don't use `AsQueryable()`. Joins with `$lookup` are allowed when a query needs them.
- BSON mapping lives in `LibraryApi.Persistence/Mapping/MongoMappings.cs`; keep MongoDB attributes out of the domain. New indexes go in `MongoIndexes.cs`, which runs on startup.
- Handlers return results (`CommandResult.NotFound`, `.Conflict`, ...) for expected outcomes rather than throwing. Domain rule violations throw `DomainException`; handlers don't catch it. `DomainExceptionBehavior` turns it into a Conflict result with the rule's code, so call domain methods before writing anything.
- Errors are ProblemDetails, built by `ProblemDetailsFactory` in `ResultExtensions`: `detail` for the message, `code` for a broken domain rule, `errors` per field for a 422. Don't write a custom error body.
- Entities are `sealed`, have a private parameterless ctor so the MongoDB driver can materialise them, private setters and static `Create` factories. Never call `DateTime.UtcNow` in the domain. Inject `TimeProvider` in handlers and pass the time in; factories and behavior methods use it for the `CreatedAtUtc` / `UpdatedAtUtc` audit fields too.
- Controllers stay thin: build the message, `sender.Send`, `ToActionResult()` / `ToCreatedResult()`. Request bodies are records under `Controllers/<Feature>/Requests/` with a `ToCommand()` mapper.
- Every new endpoint gets integration tests in `LibraryApi.IntegrationTests/<Feature>/<UseCase>Tests.cs`: they are what checks that a pipeline actually translates and returns the right data against a real database. Domain rules get unit tests in `LibraryApi.UnitTests`, without mocks. Seed data with the builders in `LibraryApi.Tests.Common/Builders` (add a `WithX()` rather than constructing entities by hand), name tests `Given_..._When_..._Then_...` with Arrange/Act/Assert sections, and assert HTTP outcomes with `CommonAssertions`. See the Tests section of README.md.
