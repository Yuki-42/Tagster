using System.ComponentModel.DataAnnotations;

namespace Api.Dto.Media.Ingest;

/// <summary>
/// Model for a media ingest session.
/// </summary>
public class IngestDto
{
    /// <summary>
    /// Ingest session ID.
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Ingest session name. Must be unique.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Tags created in this ingest.
    /// </summary>
    public required IList<Guid> CreatedTags { get; init; }
    
    /// <summary>
    /// Media items created in this ingest.
    /// </summary>
    public required IList<Guid> CreatedMedia { get; init; }
}

/// <summary>
/// Model for creating/extending a media ingest session.
/// </summary>
public class CreateIngestDto
{
    /// <summary>
    /// ID of an existing ingest session that is being extended.
    /// </summary>
    public Guid? ExtendingId {get; init;}
    
    /// <inheritdoc cref="IngestDto.Name"/>
    [Required] public required string Name { get; init; }

    /// <summary>
    /// New tags required to be created before media items can be added.
    /// </summary>
    [Required] public required IList<CreateTagDto> NewTags { get; init; }
    
    /// <summary>
    /// Initial batch of media items.
    /// </summary>
    [Required] public required IList<IngestMediaItemDto> Media { get; init; }
}

/// <summary>
/// Individual media item included in an ingest. 
/// </summary>
public class IngestMediaItemDto
{
    /// <inheritdoc cref="MediaDto.Id"/>
    [Required] public required Guid Id { get; init; }

    /// <inheritdoc cref="MediaDto.Captured"/>
    [Required] public required DateOnly Captured { get; init; }

    /// <inheritdoc cref="MediaDto.MediaType"/>
    [Required] public required MediaType MediaType { get; init; }

    /// <inheritdoc cref="MediaDto.TimeCaptured"/>
    public TimeOnly? TimeCaptured { get; init; }

    /// <inheritdoc cref="MediaDto.FileInfo"/>
    [Required] public required MediaFileInfo FileInfo { get; init; }
    
    /// <summary>
    /// Tags attached to this image. In this case tags are identified by their name instead of ID,
    /// as they may or may not exist within the DB already, and as such their IDs are unknowable at import time.
    /// </summary>
    [Required] public required IList<string> Tags { get; init; }
}
