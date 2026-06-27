using System.Data.Common;

namespace Api.Db;

/// <summary>
/// Basic connection provider interface.
/// </summary>
public interface IDbConnectionProvider
{
	/// <summary>
	/// Get a database connection of generic type.
	/// </summary>
	/// <remarks>
	///	This syntax was used to allow for flexibility in the database repo implementations in DI while using the same syntax. In reality the implementation of this method will
	/// likely only be able to return the connection it is designed for.
	/// </remarks>
	/// <typeparam name="T">Type of DB connection.</typeparam>
	/// <returns>Opened database connection.</returns>
	Task<T> Get<T>() where T : DbConnection;
	
	/// <inheritdoc cref="IDbConnectionProvider.Get"/>
	T GetSync<T>() where T : DbConnection;
}