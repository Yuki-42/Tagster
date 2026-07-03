namespace Api.Db.Models.Ingest;

/// <summary>
/// DBM For an Ingest Session.
/// </summary>
public class IngestSessionDbm : BaseDbm
{
    /// <summary>
    /// Ingest name.
    /// </summary>
    public required string Name { get; set; }
}

/// <summary>
///	DBM for creating an ingest session.
/// </summary>
public class InsertIngestSessionDbm
{
	/// <inheritdoc cref="IngestSessionDbm.Name"/>
	public required string Name { get; init; }
}

/// <summary>
/// Used as a helper DBM for easily creating an ingest session with tags and media items included.
/// </summary>
public class InsertIngestDbm
{
	/// <inheritdoc cref="IngestSessionDbm.Name"/>
	public required string Name { get; set; }

	/// <summary>
	/// Tags that are included in this ingest.
	/// </summary>
	public required IList<InsertTagDbm> Tags { get; init; }

	/// <summary>
	/// Media items that are included in this ingest.
	/// </summary>
	public required IList<IngestBulkMediaItem> MediaItems { get; init; }

	/// <summary>
	/// Superclass of <see cref="InsertIngestMediaItem"/> adding tags to properties.
	/// </summary>
	public class IngestBulkMediaItem : InsertMediaDbm
	{
		public IEnumerable<string> Tags { get; init; }
	}
}

/// <summary>
/// Used to bulk update an ingest with new/more data.
/// </summary>
public class UpdateIngestDbm : InsertIngestDbm
{
	public Guid Id { get; init; }
}

