using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.Api.Services.Auth;
using Relay.Documentum.Application.Commands.AddUser;
using Relay.Documentum.Application.Commands.UpdateUser;
using Relay.Documentum.Application.Queries.GetAllUsers;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;
namespace Relay.Api.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IQueryDispatcher _queries;
        private readonly ICommandDispatcher _commands;
        private readonly IAdUserService _adService;
        private readonly IUserAuthRepository _authRepo;
        public UserController(IQueryDispatcher queries, ICommandDispatcher commands, IAdUserService adService, IUserAuthRepository authRepo)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _adService = adService;
            _authRepo = authRepo;
        }

        [HttpGet(ApiRoutes.Users.GetAll)]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _queries.SendAsync<GetAllUsersQuery, IReadOnlyList<UserDto>>(
                new GetAllUsersQuery(), cancellationToken);

            return Ok(result.Value);
        }

        [HttpPost(ApiRoutes.Users.Add)]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromBody] AddUserRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _commands.SendAsync(
                new AddUserCommand(
                    request.GlobalId,
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

        [HttpPut(ApiRoutes.Users.Update)]
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


        [HttpGet(ApiRoutes.Users.GetByGlobalId)]
        public async Task<IActionResult> GetUserByGlobalId([FromRoute] string globalId, CancellationToken ct = default)
        {
            var result = await _adService.GetUserDetailsAsync(globalId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost(ApiRoutes.Users.CreateUser)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct = default)
        {
            var adUser = new AdUserDetails
            {
                GlobalId = request.GlobalId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailId = request.EmailId,
                BrandId = request.BrandId,
                QueueId = request.QueueId,
                RoleId = request.RoleId,
                CreatedBy = request.CreatedBy
            };

            var result = await _authRepo.UpsertUserAsync(adUser, ct);
            return result is null ? BadRequest("Failed to create user.") : Ok(result);
        }



    }
}
