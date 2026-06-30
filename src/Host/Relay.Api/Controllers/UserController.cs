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

        [HttpPut(ApiRoutes.Users.UpdateUser)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request, CancellationToken ct = default)
        {
            var adUser = new AdUserDetails
            {
                GlobalId = request.GlobalId,
                BrandId = request.BrandId,
                QueueId = request.QueueId,
                RoleId = request.RoleId
            };

            var result = await _authRepo.UpsertUserAsync(adUser, ct);
            return result is null ? BadRequest("Failed to update user.") : Ok(result);
        }

        [HttpDelete(ApiRoutes.Users.DeleteUser)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] string globalId,
            [FromQuery] string createdBy,
            CancellationToken ct = default)
        {
            var deleted = await _authRepo.DeleteUserAsync(globalId, createdBy, ct);
            return deleted ? Ok() : NotFound();
        }

    }
}
