namespace Api.Db.Models.Ingest;

/// <summary>
/// Ingest tag link DBM.
/// </summary>
public class IngestTagDbm : BaseDbm
{
	/// <summary>
	/// Tag ID.
	/// </summary>
	public required Guid TagId { get; init; }

	/// <summary>
	/// Ingest ID this tag is related to.
	/// </summary>
	public required Guid SessionId { get; init; }
}

/// <summary>
/// DBM for creating an ingest-tag link.
/// </summary>
public class CreateIngestTagDbm
{
	/// <inheritdoc cref="IngestTagDbm.TagId"/>
	public required Guid TagId { get; init; }

	/// <inheritdoc cref="IngestTagDbm.SessionId"/>
	public required Guid SessionId { get; init; }
}