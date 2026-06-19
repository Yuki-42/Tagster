using Api.Dto;
using Api.Dto.Auth;

namespace Api.Db.Models;

/// <summary>
/// User database model.
/// </summary>
[MappedObject(typeof(UserDbm), typeof(UserDto))]
public class UserDbm : BaseDbm
{
	public required string Username { get; init; }
	public required string Email { get; init; }
	public required string Password { get; init; }
}

public class InsertUserDbm
{
	public required string Username { get; init; }
	public required string Email { get; init; }
	public required string PassHash { get; init; }
}