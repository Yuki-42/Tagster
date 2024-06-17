namespace FileMgr.Exceptions;

/// <summary>
///     Used when a tag is not found.
/// </summary>
/// <param name="message">Error message</param>
/// <param name="id">The id of the tag</param>
public class MissingTagException(string message, long id) : Exception(message)
{
    /// <summary>
    /// The tag id that was not found.
    /// </summary>
    public long TagId { get; } = id;
}

/// <summary>
///     Used when a file is not found.
/// </summary>
/// <param name="message">Error message</param>
/// <param name="id">The id of the file</param>
public class MissingFileException(string message, long id) : Exception(message)
{
    /// <summary>
    /// The file id that was not found.+
    /// </summary>
    public long FileId { get; } = id;
}

/// <summary>
///     Used when tag collisions are detected.
/// </summary>
/// <param name="message">Error message</param>
public class DuplicateTagsException(string message) : Exception(message);

/// <summary>
///     Used when the database has not been initialised.
/// </summary>
/// <param name="message">Error message</param>
public class UninitialisedDatabaseException(string message) : Exception(message);


/// <summary>
///     Used when the database has already been initialised.
/// </summary>
/// <param name="message">Error message</param>
public class AlreadyInitialisedDatabaseException(string message) : Exception(message);