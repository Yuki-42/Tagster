namespace Api.Db.Models;

/// <summary>
/// User database model.
/// </summary>
public class UserDbm : BaseDbm
{
	public required string Username { get; init; }
	public required string Email { get; init; }
	public required string PassHash { get; init; }
}