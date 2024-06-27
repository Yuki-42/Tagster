using System.Data.SQLite;
using FileMgr.Objects;
using File = FileMgr.Objects.File;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FileMgr.Handlers;

/// <summary>
///     Handles file-related database operations.
/// </summary>
public class Files
{
    /// <summary>
    ///     Database configuration.
    /// </summary>
    private readonly ApplicationConfig _config;

    /// <summary>
    ///     Low-level database connection.
    /// </summary>
    private readonly SQLiteConnection _connection;

    /// <summary>
    ///     Relations handler.
    /// </summary>
    private Relations _relations;

    /// <summary>
    ///     Tags handler.
    /// </summary>
    private Tags _tags;

    /// <summary>
    ///     Constructor for the Files handler.
    /// </summary>
    /// <param name="connection">Database connection.</param>
    /// <param name="config">App config.</param>
    public Files(SQLiteConnection connection, ApplicationConfig config)
    {
        _connection = connection;
        _config = config;
    }

    /// <summary>
    ///     Populates handlers. This is done separately as it must be done after all handlers are initialised.
    /// </summary>
    /// <param name="handlersGroup">Handlers group.</param>
    public void Populate(HandlersGroup handlersGroup)
    {
        _relations = handlersGroup.Relations;
        _tags = handlersGroup.Tags;
    }

    /*************************************************************************************************************************************************************************************
     * Database Operations
     *************************************************************************************************************************************************************************************/

    /// <summary>
    ///     Gets a file by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>File if found.</returns>
    public File? Get(long id)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT * FROM files WHERE id = @id;", _connection);

        // Add the parameter.
        command.Parameters.AddWithValue("@id", id);

        using SQLiteDataReader reader = command.ExecuteReader();

        if (!reader.HasRows) return null;

        // Read the data.
        reader.Read();
        long fileId = reader.GetInt64(0);
        DateTime added = reader.GetDateTime(1);
        string filePath = reader.GetString(2);

        // Create a new file object and return it
        return new File(fileId, added, filePath, _relations.GetTags(fileId));
    }


    /// <summary>
    ///     Adds a file to the database.
    /// </summary>
    /// <param name="file">File to add.</param>
    /// <returns>File added.</returns>
    public File Add(FileInfo file)
    {
        // Create a new command 
        SQLiteCommand command = new("INSERT INTO files (path) VALUES (@path) RETURNING id;", _connection);
        command.Parameters.AddWithValue("@path", file.FullName);

        // Execute the command and get the ID
        return Get((long)command.ExecuteScalar())!;
    }

    /// <summary>
    ///     Adds a file to the database with tags.
    /// </summary>
    /// <param name="file">File to add.</param>
    /// <param name="tags">Tags to add.</param>
    /// <returns>File added.</returns>
    public File Add(
        FileInfo file,
        List<Tag> tags
    )
    {
        // Add the file
        File newFile = Add(file);

        // Add the tags
        foreach (Tag tag in tags) _relations.AddTag(newFile, tag);

        return newFile;
    }
}