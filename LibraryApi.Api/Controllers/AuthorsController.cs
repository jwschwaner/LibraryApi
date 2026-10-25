using LibraryApi.Api.Controllers.Authors.Requests;
using LibraryApi.Api.Controllers.Base;
using LibraryApi.Api.Extensions;
using LibraryApi.Application.Authors.DeleteAuthor;
using LibraryApi.Application.Authors.GetAuthor;
using LibraryApi.Application.Authors.ListAuthors;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Api.Controllers;

public class AuthorsController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAuthors(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListAuthorsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAuthor(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAuthorQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedResult(author => $"/authors/{author.Id}");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] UpdateAuthorRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAuthor(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAuthorCommand(id), cancellationToken);
        return result.ToActionResult();
    }
}
