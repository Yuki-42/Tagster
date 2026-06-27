namespace Api.Db.Models;

/// <summary>
/// DBM For an Ingest Session.
/// </summary>
public class IngestSessionDbm : BaseDbm
{
    public required string Name { get; init; }
}

public class CreateIngestDbm
{
    public required string Name { get; init; }
}

public class IngestMediaDbm : BaseDbm
{
    public required Guid IngestId {get; init;}
    public required Guid MediaId {get; init;}
}

public class IngestTagDbm : BaseDbm
{
    public required Guid IngestId {get; init;}
    public required Guid TagId {get; init;}
}