using System.Data.SQLite;

namespace FileMgr.Objects;

/// <summary>
///     Represents a tag in the database.
/// </summary>
public class Tag
{
    /// <summary>
    /// Tag Id.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Tag added to db time.
    /// </summary>
    public DateTime Created { get; }

    /// <summary>
    /// Tag name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Tag colour.
    /// </summary>
    public string? Colour  { get; }

    /// <summary>
    ///     Initialises a new tag object.
    /// </summary>
    /// <param name="id">Id of the tag.</param>
    /// <param name="created">Tag added to db time.</param>
    /// <param name="name">Name of the tag.</param>
    /// <param name="colour">Colour of the tag.</param>
    public Tag(long id, DateTime created, string name, string colour)
    {
        Id = id;
        Created = created;
        Name = name;
        Colour = colour;
    }

    /// <summary>
    ///     Initialises a new tag object.
    /// </summary>
    /// <param name="reader">The sqlite reader to draw data from.</param>
    public Tag(SQLiteDataReader reader)
    {
        Id = reader.GetInt64(0);
        Created = reader.GetDateTime(1);
        Name = reader.GetString(2);
        Colour = reader.GetString(3);
    }
}