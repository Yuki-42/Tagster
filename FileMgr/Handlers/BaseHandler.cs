using System.Data.SQLite;

namespace FileMgr.Handlers;

/// <summary>
///     Base Handler for database interactions.
/// </summary>
public class BaseHandler
{
    /*
     * Methods needed:
     * - Close (void)
     * - Populate (void)
     * - Init (void) (constructor)
     */

    /// <summary>
    ///     Application configuration.
    /// </summary>
    protected readonly ApplicationConfig Config;

    /// <summary>
    ///     Connection to the database.
    /// </summary>
    protected readonly SQLiteConnection Connection;

    /// <summary>
    ///     Handlers group.
    /// </summary>
    protected HandlersGroup HandlersGroup = null!;

    /// <summary>
    ///     Initialises a new handler.
    /// </summary>
    /// <param name="connection">Connection to use.</param>
    /// <param name="config">Config to use.</param>
    protected BaseHandler(SQLiteConnection connection, ApplicationConfig config)
    {
        Connection = connection;
        Config = config;
    }

    /// <summary>
    ///     Populates the handlers group.
    /// </summary>
    /// <param name="handlersGroup"></param>
    public void Populate(HandlersGroup handlersGroup)
    {
        HandlersGroup = handlersGroup;
    }
}