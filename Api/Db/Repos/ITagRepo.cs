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

	/// <summary>
	/// Get tag by name.
	/// </summary>
	/// <returns>Tag if found.</returns>
	Task<TagDbm?> Get(string name);
	
	/// <summary>
	/// Get tag by ID.
	/// </summary>
	/// <returns>Tag if found.</returns>
	Task<TagDbm?> Get(Guid id);

	/// <summary>
	/// Insert a new tag.
	/// </summary>
	/// <param name="ob">New tag item.</param>
	/// <returns>Created tag.</returns>
	Task<TagDbm> Insert(InsertTagDbm ob);
	
	/// <summary>
	/// Deletes a tag.
	/// </summary>
	Task Delete(Guid id);
}