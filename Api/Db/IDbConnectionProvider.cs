using System.Data.Common;

namespace Api.Db;

/// <summary>
/// Basic connection provider interface.
/// </summary>
public interface IDbConnectionProvider
{
	T Get<T>() where T : DbConnection;
	Task<T> GetAsync<T>() where T : DbConnection;
}