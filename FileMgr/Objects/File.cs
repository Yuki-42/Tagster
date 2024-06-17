using System.Data.SQLite;

namespace FileMgr.Objects;

/// <summary>
///     Represents a file in the database.
/// </summary>
public class DbFile
{
    /// <summary>
    /// File ID.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// File added to db time.
    /// </summary>
    public DateTime Added { get; }

    /// <summary>
    /// File creation time. (OS)
    /// </summary>
    public DateTime Created { get; }

    /// <summary>
    /// File modified time. (OS)
    /// </summary>
    public DateTime Modified { get; }

    /// <summary>
    /// File path.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// List of tags applied to file.
    /// </summary>
    public List<Tag> Tags { get; }

    /// <summary>
    ///     Initialises a new file object.
    /// </summary>
    /// <param name="id">Id of the file.</param>
    /// <param name="added">File added to db time.</param>
    /// <param name="path">Path to file.</param>
    /// <param name="tags">List of tags applied to file.</param>
    public DbFile(long id, DateTime added, string path, List<Tag> tags)
    {
        Id = id;
        Added = added;
        Created = File.GetCreationTime(path);
        Modified = File.GetLastWriteTime(path);
        Path = path;
        Tags = tags;
    }

    /// <summary>
    /// Initialises a new file object from a reader.
    /// </summary>
    /// <param name="reader">The DataReader to read file data from.</param>
    /// <param name="tags">Tags associated with this file.</param>
    public DbFile(SQLiteDataReader reader, List<Tag> tags)
    {
        Id = reader.GetInt64(0);
        Added = reader.GetDateTime(1);
        Created = File.GetCreationTime(reader.GetString(2));
        Modified = File.GetLastWriteTime(reader.GetString(2));
        Path = reader.GetString(2);
        Tags = tags;
    }
}