using Api.Db.Models;

namespace Api.Db.Repos;

/// <summary>
/// Tags repository.
/// </summary>
public interface ITagRepo
{
	/// <summary>
	/// Gets a paginated list of tags from the database.
	/// </summary>
	/// <param name="pg">Page number.</param>
	/// <param name="count">Items per page.</param>
	/// <returns>List of all found results.</returns>
	Task<IList<TagDbm>> Get(int pg, int count);

	Task<TagDbm?> Get(string name);
	Task<TagDbm?> Get(Guid id);

	Task<TagDbm> Insert(InsertTagDbm ob);
}