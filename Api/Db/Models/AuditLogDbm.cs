namespace Api.Db.Models;

/// <summary>
/// Audit log row.
/// </summary>
public class AuditLogDbm : BaseDbm
{
	/// <summary>
	/// Action timestamp.
	/// </summary>
	public required DateTime Timestamp { get; init; }

	/// <summary>
	/// Effected table name.
	/// </summary>
	public required string TableName { get; init; }

	/// <summary>
	/// Type of action performed.
	/// </summary>
	public required AuditActionType ActionType { get; init; }

	/// <summary>
	/// Row ID effected.
	/// </summary>
	public required Guid RowId { get; init; }

	/// <summary>
	/// User performing action. Only used for authenticated actions.
	/// </summary>
	public Guid UserId { get; init; }

	/// <summary>
	/// Previous row state dump (used for important data)
	/// </summary>
	public string? PrevState { get; init; } = null;

	/// <summary>
	/// If the action successfully went through, or was blocked/failed for any other reason.
	/// </summary>
	public bool? Effected { get; init; } = null;
}

/// <summary>
/// Model for creating an audit log entry.
/// </summary>
public class CreateAuditLogDbo
{
	/// <summary>
	/// Effected table name.
	/// </summary>
	public required string TableName { get; init; }

	/// <summary>
	/// Type of action performed.
	/// </summary>
	public required AuditActionType ActionType { get; init; }

	/// <summary>
	/// Row ID effected.
	/// </summary>
	public required Guid RowId { get; init; }

	/// <summary>
	/// User performing action.
	/// </summary>
	public required Guid UserId { get; init; }

	/// <summary>
	/// Previous row state dump (used for important data)
	/// </summary>
	public string? PrevState { get; init; } = null;

	/// <summary>
	/// If the action successfully went through, or was blocked/failed for any other reason.
	/// </summary>
	public bool? Affected { get; init; } = null;
}

/// <summary>
/// Audit log action type.
/// </summary>
public enum AuditActionType
{
	/// <summary>
	/// Resource created.
	/// </summary>
	Create,

	/// <summary>
	/// Row edited.
	/// </summary>
	Edit,

	/// <summary>
	/// Row deleted.
	/// </summary>
	Delete,

	/// <summary>
	/// Resource read.
	/// </summary>
	Access
}