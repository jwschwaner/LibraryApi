using LibraryApi.Api.Controllers.Base;
using LibraryApi.Api.Controllers.Members.Requests;
using LibraryApi.Api.Extensions;
using LibraryApi.Application.Members.GetMember;
using LibraryApi.Application.Members.ListMembers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Api.Controllers;

public class MembersController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListMembers(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListMembersQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMember(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMemberQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterMember([FromBody] RegisterMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedResult(member => $"/members/{member.Id}");
    }
}
