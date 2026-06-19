namespace Api.Db.Models;

public class SharedFileDbm : BaseDbm
{
	/// <summary>
	/// Id of file source file referenced.
	/// </summary>
	public required Guid References { get; init; }
}