using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Intranet;
using Relay.Api.Routes;
using Relay.Intranet.Application.Commands.UpdateUserByEmail;
using Relay.Intranet.Application.Queries.GetUserById;
using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Intranet;

[Authorize]
[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly IQueryDispatcher _queries;
    private readonly ICommandDispatcher _commands;

    public UsersController(IQueryDispatcher queries, ICommandDispatcher commands)
    {
        _queries  = queries  ?? throw new ArgumentNullException(nameof(queries));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
    }

    [HttpGet(ApiRoutes.Users.GetById)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetUserByIdQuery, UserDto?>(new GetUserByIdQuery(id), cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : NotFound();
    }

    [HttpPut(ApiRoutes.Users.UpdateByEmail)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateByEmail(
        [FromBody] UpdateUserByEmailRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _commands.SendAsync(
            new UpdateUserByEmailCommand(request.Email, request.NewDisplayName), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code == "User.NotFound"
                ? NotFound(result.Error.Description)
                : BadRequest(result.Error.Description);
        }

        return Ok(result.Value);
    }
}
