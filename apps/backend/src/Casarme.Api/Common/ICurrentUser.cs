namespace Casarme.Api.Common;

public interface ICurrentUser
{
    string? Id { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}