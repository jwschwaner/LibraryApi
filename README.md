# LibraryApi

A small library API used to demonstrate **CQRS with the Mediator pattern** in ASP.NET Core
(.NET 10, controllers). Authors, books, members and loans, with a few real lending rules so
the commands have something to decide.

## Run it

Start MongoDB in Docker, then the API:

```bash
docker compose up -d
dotnet run --project LibraryApi.Api
```

Open http://localhost:5227 for the Scalar API reference. On startup the API creates the
MongoDB indexes and, in Development, seeds a few authors, books and members into an empty
`library` database. `docker compose down -v` deletes the data so you can start over.
`LibraryApi.Api/LibraryApi.Api.http` has ready-made requests.

## Structure

Clean Architecture with dependencies pointing inward:
**Domain → Application → Persistence → Api** (composition root).

| Project | Contains |
| --- | --- |
| `LibraryApi.Domain` | Entities (`Author`, `Book`, `Member`, `Loan`) with private setters, factory methods and the business rules. Throws `DomainException` with a stable code. |
| `LibraryApi.Application` | One folder per use case (`Loans/BorrowBook/…`): the command/query record, its handler and its validator side by side. Also the result types, pipeline behaviors and the DbContext interfaces. |
| `LibraryApi.Persistence` | MongoDB driver: write and read-only contexts, BSON class maps (so the domain has no MongoDB attributes), indexes, demo seeding. |
| `LibraryApi.Api` | Thin controllers that turn HTTP into a message, `ISender.Send` it and map the result to a status code. |

## How a request flows

```
POST /loans ─► LoansController ─► ISender.Send(BorrowBookCommand)
                                     │
                    LoggingBehavior ─┤  (times every message)
                 ValidationBehavior ─┤  (FluentValidation → 422 before the handler runs)
            DomainExceptionBehavior ─┤  (a broken rule → Conflict result with the rule's code)
                                     ▼
                       BorrowBookCommandHandler
                         loads state via IApplicationDbContext
                         Loan.Create(...) enforces the lending rules
                         returns CommandResult<LoanResponse>
                                     │
LoansController ◄─ result.ToCreatedResult(...)  → 201 / 404 / 409 / 422
```

Handlers don't catch `DomainException`. `DomainExceptionBehavior` turns it into a Conflict
result carrying the rule's code (`Loan.NoCopiesAvailable`), so any caller of the command gets a
result back, not an exception.

## Errors

Every error is a [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) body, built by
ASP.NET Core's `ProblemDetailsFactory` in `ResultExtensions`. Our own errors therefore look the
same as the framework's (for example a 400 for a body that isn't JSON):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "All copies of this book are on loan",
  "traceId": "00-3ed36a12...",
  "code": "Loan.NoCopiesAvailable"
}
```

A 422 is a `ValidationProblemDetails` with the messages per field under `errors`. `code` is only
present when a domain rule refused the request. Unhandled exceptions become a 500
ProblemDetails through `UnhandledExceptionHandler`, without exception details.

## The CQRS split

- **Commands** (`ICommand<CommandResult<T>>`) change state. Handlers use
  `IApplicationDbContext`, which exposes the `IMongoCollection<T>`s: load an entity, call a
  domain method, then insert, replace or delete the document explicitly.
- **Queries** (`IQuery<QueryResult<T>>`) only read. Handlers use
  `IReadonlyApplicationDbContext`, whose collections only run typed aggregation pipelines
  (and reject `$out` and `$merge`), so a query has no way to write. They project in the
  database straight to response records, never to entities. A response that needs another
  collection (author names, copies on loan) runs one extra query per collection here; a
  `$lookup` would be allowed too.

Both sides use the driver's typesafe builders: `Builders<T>.Filter` and `.Sort`, and pipelines
built from `new EmptyPipelineDefinition<T>()` with `.Match`, `.Sort`, `.Project` and `.Group`.
Field names, Guids and dates go through the BSON mappings, so a renamed property is a compile
error rather than a query that silently matches nothing. A raw `BsonDocument` stage is only
for something the builders can't express. `AsQueryable()` isn't used.

Both sides share one database. The split is about code paths and intent, not separate
stores.

## Lending rules

Enforced in `Loan.Create` and `Loan.Return`, surfaced as `409 Conflict` with the rule's code
from `LoanErrors`:

- A book can't be borrowed when all copies are on loan.
- A member can have at most 3 books at a time.
- A member with an overdue book can't borrow more (loan period is 14 days).
- A loan can only be returned once.
- A book's copy count can't drop below the copies currently on loan.

The rules are checked against counts read just before the insert, without a transaction, so
two simultaneous requests for the last copy could both succeed. Fine for a demo; a real
system would use an atomic conditional update or a transaction (needs a replica set).

## Tests

xUnit throughout, in three projects:

| Project | Contains |
| --- | --- |
| `LibraryApi.UnitTests` | The domain rules (`Loan`, `Book`, `Member`). No mocks, no Docker. |
| `LibraryApi.IntegrationTests` | Boots the real API with `WebApplicationFactory<Program>` against a MongoDB container, one folder per feature (`Loans/BorrowBookTests.cs`, ...). |
| `LibraryApi.Tests.Common` | Shared test infrastructure, not a test project (`IsTestProject=false`): the MongoDB fixture, builders, `ApiResponse<T>` and `CommonAssertions`. |

```bash
dotnet test                                  # everything; needs Docker running
dotnet test LibraryApi.UnitTests             # fast loop without Docker
dotnet test --filter "FullyQualifiedName~BorrowBookTests"
```

**Integration tests are where query handlers are tested.** The builders catch wrong field names
at compile time, but only a real database shows that a pipeline translates and returns the right
data (expression projections and any raw `BsonDocument` stage most of all). `MongoDbFixture` starts one
`mongo:7.0` container per run through Testcontainers, shared by every class in the `MongoDb`
xUnit collection. `IntegrationTestBase` drops the database and starts a fresh API host before
each test, and gives you `PersistAsync(...)` (writes entities through the API's own
`IApplicationDbContext`, so the real BSON mappings apply) and `GetAsync` / `PostAsync` /
`PutAsync` / `DeleteAsync`, which return an `ApiResponse<T>` with the status code, raw body,
deserialized body and, for an error, the ProblemDetails fields as an `ApiProblem`.

**Time is frozen.** The API's `TimeProvider` is replaced by a `FakeTimeProvider` set to
`TestConstants.NowUtc`, and the builders stamp entities with the same instant, so due dates and
"overdue" are exact.

**Test data comes from the builders** in `Tests.Common/Builders`: `Simple()` for the least that's
valid, `Typical()` for a fully populated entity, `WithX()` setters, then `Build()`, which goes
through the entity's `Create` factory so test data obeys the domain rules. ISBNs and emails are
unique per builder, because both have unique indexes. `LoanBuilder.Of(book, member)` takes
`.Overdue(days)` and `.Returned()`.

**Conventions**

- Test names are `Given_..._When_..._Then_...`, with `// Arrange`, `// Act` and `// Assert`
  sections.
- Integration test classes are one per endpoint, `sealed`, marked
  `[Collection(MongoDbFixture.CollectionName)]` and derived from `IntegrationTestBase`.
- Unit test classes nest one class per method under test (`LoanTests.Create`, `LoanTests.Return`).
- Assert status codes with `CommonAssertions`. `IsConflict(response, message, code)` also checks
  which rule refused the request (pass the code from `LoanErrors` / `BookErrors` for a domain
  rule), and `IsValidationError(response, "Field", ...)` checks which fields failed.

## Libraries

- [Mediator](https://github.com/martinothamar/Mediator) (source-generated, MIT). Not MediatR.
- [MongoDB.Driver](https://github.com/mongodb/mongo-csharp-driver) (official, Apache-2.0).
- FluentValidation, Scalar for the API reference.
- Tests: xUnit, [Testcontainers](https://dotnet.testcontainers.org/) for MongoDB,
  `Microsoft.AspNetCore.Mvc.Testing` and `Microsoft.Extensions.TimeProvider.Testing`.
