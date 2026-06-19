namespace Api.Db.Models;


/// <summary>
/// Base database model inherited by all database models.
/// </summary>
public class BaseDbm
{
	public required Guid Id { get; init; }
}