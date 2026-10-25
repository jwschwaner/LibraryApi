# LibraryApi

A small library API used to demonstrate **CQRS with the Mediator pattern** in ASP.NET Core
(.NET 10, controllers). Authors, books, members and loans, with a few real lending rules so
the commands have something to decide.

## Run it

```bash
dotnet run --project LibraryApi.Api
```

Open http://localhost:5227 for the Scalar API reference. On startup the API applies EF Core
migrations to a local SQLite file (`library.db`) and, in Development, seeds a few authors,
books and members. Delete `library.db` to start over.
`LibraryApi.Api/LibraryApi.Api.http` has ready-made requests.

## Structure

Clean Architecture with dependencies pointing inward:
**Domain → Application → Persistence → Api** (composition root).

| Project | Contains |
| --- | --- |
| `LibraryApi.Domain` | Entities (`Author`, `Book`, `Member`, `Loan`) with private setters, factory methods and the business rules. Throws `DomainException` with a stable code. |
| `LibraryApi.Application` | One folder per use case (`Loans/BorrowBook/…`): the command/query record, its handler and its validator side by side. Also the result types, pipeline behaviors and the DbContext interfaces. |
| `LibraryApi.Persistence` | EF Core (SQLite): write and read-only contexts, entity configurations, audit interceptor, migrations, demo seeding. |
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
  `IApplicationDbContext`: tracked entities, domain methods, `SaveChangesAsync`.
- **Queries** (`IQuery<QueryResult<T>>`) only read. Handlers use
  `IReadonlyApplicationDbContext`: `IQueryable`s on a no-tracking context whose
  `SaveChanges` throws, projected straight to response records in SQL.

Both sides share one database. The split is about code paths and intent, not separate
stores.

## Lending rules

Enforced in `Loan.Create` and `Loan.Return`, surfaced as `409 Conflict`:

- A book can't be borrowed when all copies are on loan.
- A member can have at most 3 books at a time.
- A member with an overdue book can't borrow more (loan period is 14 days).
- A loan can only be returned once.
- A book's copy count can't drop below the copies currently on loan.

## Libraries

- [Mediator](https://github.com/martinothamar/Mediator) (source-generated, MIT). Not MediatR.
- FluentValidation, EF Core 10 with SQLite, Scalar for the API reference.
