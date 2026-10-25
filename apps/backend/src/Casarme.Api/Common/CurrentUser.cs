using System.Security.Claims;

namespace Casarme.Api.Common;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public string? Id => User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");

    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
