using Api.Dto;
using Api.Dto.Auth;

namespace Api.Db.Models;

/// <summary>
/// User database model.
/// </summary>
[MappedObject(typeof(UserDbm), typeof(UserDto))]
public class UserDbm : BaseDbm
{
	/// <summary>
	/// User's chosen username. Non-identifying. 
	/// </summary>
	public required string Username { get; init; }
	
	/// <summary>
	/// User's email address. 
	/// </summary>
	public required string Email { get; init; }
	
	/// <summary>
	/// Hashed and salted user password.
	/// </summary>
	public required string Password { get; init; }
}

public class InsertUserDbm
{
	public required string Username { get; init; }
	public required string Email { get; init; }
	public required string PassHash { get; init; }
}