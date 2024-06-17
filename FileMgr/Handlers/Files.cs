using System.Data.SQLite;
using FileMgr.Objects;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FileMgr.Handlers;

/// <summary>
/// Handles file-related database operations.
/// </summary>
public class Files
{
    /// <summary>
    /// Low-level database connection.
    /// </summary>
    private readonly SQLiteConnection _connection;

    /// <summary>
    /// Database configuration.
    /// </summary>
    private readonly ApplicationConfig _config;

    /// <summary>
    /// Constructor for the Files handler.
    /// </summary>
    /// <param name="connection">Database connection.</param>
    /// <param name="config">App config.</param>
    public Files(SQLiteConnection connection, ApplicationConfig config)
    {
        _connection = connection;
        _config = config;
    }

    /// <summary>
    /// Gets a file by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Objects.File? Get(long id)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT * FROM files WHERE id = @id;", _connection);

        // Add the parameter.
        command.Parameters.AddWithValue("@id", id);

        // Execute the command and get the reader.
        SQLiteDataReader reader = command.ExecuteReader();

        if (!reader.HasRows)
        {
            reader.Close();
            return null;
        }

        // Read the data.
        reader.Read();
        long fileId = reader.GetInt64(0);
        DateTime added = reader.GetDateTime(1);
        string filePath = reader.GetString(2);
        reader.Close();

        // Create a new file object and return it
        return new Objects.File(fileId, added, filePath, GetTags(fileId));
    }



    public Objects.File Add(FileInfo file)
    {

    }

    public Objects.File Add(
        FileInfo file,
        List<Tag> tags
        )
    {

    }

}