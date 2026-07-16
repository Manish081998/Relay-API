using Relay.Api.Models.Auth;

namespace Relay.Api.Services.Auth;

internal sealed class UserLoginService : IUserLoginService
{
    private readonly IUserAuthRepository _authRepo;
    private readonly IAdUserService _adService;
    private readonly ILogger<UserLoginService> _logger;

    public UserLoginService(
        IUserAuthRepository authRepo,
        IAdUserService adService,
        ILogger<UserLoginService> logger)
    {
        _authRepo = authRepo;
        _adService = adService;
        _logger = logger;
    }

    public async Task<(AuthenticatedUserDto? User, string? ErrorMessage)> AuthorizeAndGetUserAsync(string globalId, CancellationToken ct = default)
    {
        // 1. Check DB authorization status
        var authStatus = await _authRepo.GetAuthStatusAsync(globalId, ct);
        if (authStatus is null)
        {
            _logger.LogWarning("No auth status record found for {GlobalId}", globalId);
            return (null, "User not found.");
        }

        // 2. Fetch extended user details from AD
        var adUser = await _adService.GetUserDetailsAsync(globalId);
        if (adUser is null || string.IsNullOrEmpty(adUser.GlobalId))
        {
            _logger.LogWarning("AD lookup returned no result for {GlobalId}", globalId);
            return (null, "Please enter a valid global ID.");
        }

        // 3. Handle by status
        UserRecord? dbUser = null;
        string userType;

        if (authStatus.Status is  "Granted")
        {
            dbUser = await _authRepo.UpsertUserAsync(adUser, ct);
            userType = authStatus.Status == "Granted"
                ? (authStatus.UserType ?? "User")   // preserve existing role on re-login
                : "User";                           // new users default to User
        }
        else
        {
            _logger.LogWarning("Access denied for {GlobalId}: status={Status}", globalId, authStatus.Status);
            return (null, string.IsNullOrEmpty(authStatus.Message) ? "Access denied." : authStatus.Message);
        }

        bool isAdmin = string.Equals(userType, "Admin", StringComparison.OrdinalIgnoreCase);

        var brandInfo = await _authRepo.GetUserBrandInfoAsync(globalId, ct);

        return (new AuthenticatedUserDto
        {
            GlobalId = adUser.GlobalId,
            FirstName = adUser.FirstName,
            LastName = adUser.LastName,
            EmailId = adUser.EmailId,
            Title = adUser.Title,
            CompanyName = adUser.CompanyName,
            Department = adUser.Department,
            Office = adUser.Office,
            UserType = userType,
            BrandId = brandInfo?.BrandId,
            BrandName = brandInfo?.BrandName,
            AssociatedQueueNames = brandInfo?.AssociatedQueueNames ?? []
        }, null);
    }
}
