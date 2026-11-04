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
                                     ▼
                       BorrowBookCommandHandler
                         loads state via IApplicationDbContext
                         Loan.Create(...) enforces the lending rules
                         returns CommandResult<LoanResponse>
                                     │
LoansController ◄─ result.ToCreatedResult(...)  → 201 / 404 / 409 / 422
```

## The CQRS split

- **Commands** (`ICommand<CommandResult<T>>`) change state. Handlers use
  `IApplicationDbContext`, which exposes the `IMongoCollection<T>`s: load an entity, call a
  domain method, then insert, replace or delete the document explicitly.
- **Queries** (`IQuery<QueryResult<T>>`) only read. Handlers use
  `IReadonlyApplicationDbContext`, which exposes nothing but LINQ `IQueryable<T>`s, so a
  query has no way to write. They project in the database and never return entities.
  MongoDB doesn't join collections in a plain query, so a response that needs another
  collection (author names, copies on loan) runs one extra query per collection.

Both sides share one database. The split is about code paths and intent, not separate
stores.

## Lending rules

Enforced in `Loan.Create` and `Loan.Return`, surfaced as `409 Conflict`:

- A book can't be borrowed when all copies are on loan.
- A member can have at most 3 books at a time.
- A member with an overdue book can't borrow more (loan period is 14 days).
- A loan can only be returned once.
- A book's copy count can't drop below the copies currently on loan.

The rules are checked against counts read just before the insert, without a transaction, so
two simultaneous requests for the last copy could both succeed. Fine for a demo; a real
system would use an atomic conditional update or a transaction (needs a replica set).

## Libraries

- [Mediator](https://github.com/martinothamar/Mediator) (source-generated, MIT). Not MediatR.
- [MongoDB.Driver](https://github.com/mongodb/mongo-csharp-driver) (official, Apache-2.0).
- FluentValidation, Scalar for the API reference.
