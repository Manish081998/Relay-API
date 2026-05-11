using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.Documentum.Application.Commands.AddUser;
using Relay.Documentum.Application.Commands.UpdateUser;
using Relay.Documentum.Application.Queries.GetAllUsers;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class DocumentumUsersController : ControllerBase
{
    private readonly IQueryDispatcher _queries;
    private readonly ICommandDispatcher _commands;

    public DocumentumUsersController(IQueryDispatcher queries, ICommandDispatcher commands)
    {
        _queries  = queries  ?? throw new ArgumentNullException(nameof(queries));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet(ApiRoutes.DocumentumUsers.GetAll)]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetAllUsersQuery, IReadOnlyList<UserDto>>(
            new GetAllUsersQuery(), cancellationToken);

        return Ok(result.Value);
    }

    [HttpPost(ApiRoutes.DocumentumUsers.Add)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AddUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _commands.SendAsync(
            new AddUserCommand(
                request.GlobalId,
                request.Password,
                request.FirstName,
                request.LastName,
                request.EmailId,
                request.BrandId,
                request.IsActive,
                request.CreatedBy,
                request.ModifiedBy),
            cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : BadRequest(result.Error.Description);
    }

    /// <summary>
    /// Update User records
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut(ApiRoutes.DocumentumUsers.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _commands.SendAsync(
            new UpdateUserCommand(
                request.userId,
                request.BrandId,
                request.IsActive,
                request.ModifiedBy),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code == "User.NotFound"
                ? NotFound(result.Error.Description)
                : BadRequest(result.Error.Description);
        }

        return Ok(result.Value);
    }
}
