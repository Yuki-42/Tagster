using Api.Db.Models;

namespace Api.Db.Repos;

/// <summary>
/// Ingests repo.
/// </summary>
public interface IIngestRepo
{
    Task<IngestSessionDbm> Create(CreateIngestDbm ingestDbm);
    Task AttachMedia(Guid ingestId, Guid mediaId);
    Task AttachTag(Guid ingestId, Guid tagId);
}