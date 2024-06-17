namespace FileMgr.Objects;

/// <summary>
/// Editable tag object.
/// </summary>
public class ETag
{
    /// <summary>
    /// Tag Id.
    /// </summary>
    public readonly long Id;

    /// <summary>
    /// Tag added to db time.
    /// </summary>
    public readonly DateTime Created;

    /// <summary>
    /// Tag name.
    /// </summary>
    public string Name;

    /// <summary>
    /// Tag colour.
    /// </summary>
    public string? Colour;

    /// <summary>
    /// Initialises a new editable tag object.
    /// </summary>
    /// <param name="tag">Parent object.</param>
    public ETag(Tag tag)
    {
        Id = tag.Id;
        Created = tag.Created;
        Name = tag.Name;
        Colour = tag.Colour;
    }
}