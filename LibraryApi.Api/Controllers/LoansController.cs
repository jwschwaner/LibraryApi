using LibraryApi.Api.Controllers.Base;
using LibraryApi.Api.Controllers.Loans.Requests;
using LibraryApi.Api.Extensions;
using LibraryApi.Application.Loans.OverdueLoans;
using LibraryApi.Application.Loans.ReturnBook;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Api.Controllers;

public class LoansController(ISender sender) : ApiControllerBase
{
    [HttpGet("overdue")]
    public async Task<IActionResult> OverdueLoans(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new OverdueLoansQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> BorrowBook([FromBody] BorrowBookRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedResult(loan => $"/loans/{loan.Id}");
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> ReturnBook(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReturnBookCommand(id), cancellationToken);
        return result.ToActionResult();
    }
}
