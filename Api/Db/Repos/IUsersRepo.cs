using Api.Db.Models;

namespace Api.Db.Repos;

/// <summary>
/// Users repository.
/// </summary>
public interface IUsersRepo
{
	/// <summary>
	/// Get user by ID.
	/// </summary>
	/// <param name="id">User ID.</param>
	/// <returns>User if found, null if none.</returns>
	Task<UserDbm?> Get(Guid id);
}