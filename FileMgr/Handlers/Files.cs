using System.Data.SQLite;

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
}