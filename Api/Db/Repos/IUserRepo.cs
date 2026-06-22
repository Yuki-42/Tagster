using Api.Db.Models;

namespace Api.Db.Repos;

/// <summary>
/// Users repository.
/// </summary>
public interface IUserRepo
{
	/// <summary>
	/// Get user by ID.
	/// </summary>
	/// <param name="id">User ID.</param>
	/// <returns>User if found, null if none.</returns>
	Task<UserDbm?> Get(Guid id);

	/// <summary>
	/// Get user by email address.
	/// </summary>
	/// <param name="email">User email.</param>
	/// <returns>User if found, null if none.</returns>
	Task<UserDbm?> Get(string email);

	/// <summary>
	/// Creates a user in the database.
	/// </summary>
	/// <param name="user">The user to create.</param>
	/// <returns>The created user.</returns>
	Task<UserDbm> Insert(InsertUserDbm user);
}