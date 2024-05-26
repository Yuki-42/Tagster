using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Text.Json;
using Microsoft.Win32;

namespace FileMgr;

internal class ApplicationConfig
{
    public required string Delimiter { get; set; }
}

/// <summary>
///     Public exposed class for file management using tags and file paths.
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class FileManager
{
    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Class Variables
     */

    /// <summary>
    ///     The most important class variable. This is the path to the data directory and the parent path for all operations.
    /// </summary>
    private readonly DirectoryInfo _filePath;

    /// <summary>
    ///     Stores persistent application configuration.
    /// </summary>
    private ApplicationConfig _config;

    /// <summary>
    ///     The database connection for the program.
    /// </summary>
    private Database _database;

    /// <summary>
    ///     Stores whether the file manager has been initialised.
    /// </summary>
    private bool _initialised;


    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Miscellaneous Methods
     */

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileManager" /> class.
    /// </summary>
    /// <param name="filePath">The path to the data directory.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FileManager(DirectoryInfo filePath)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        // Set the file path
        _filePath = filePath;

        // Do the registry checks
        DoRegistryChecks();
    }

    // Overload for string path
    public FileManager(string filePath) : this(new DirectoryInfo(filePath))
    {
    }

    /// <summary>
    ///     Ensures that the program can run by checking the registry for the LongPathsEnabled key.
    /// </summary>
    private void DoRegistryChecks()
    {
        // Check the state of the HKEY_LOCAL_MACHINE\SOFTWARE\tagster\LongPathsEnabled registry key
        RegistryKey applicationKeys = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\tagster", true) ?? Registry.CurrentUser.CreateSubKey(@"SOFTWARE\tagster");

        // Check if the key exists or is not set to 1.
        if (
            applicationKeys.GetValue("LongPathsEnabled") is null ||
            (int)(applicationKeys.GetValue("LongPathsEnabled") ?? 0) != 1
        )
        {
            // The key does not exist, so continue with the checks.
        }
        else
        {
            // The key exists and is set to 1, so we can continue.
            return;
        }

        // Get current user
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            // Create a new process with admin privileges
            ProcessStartInfo processStartInfo = new("RegistryPatcher.exe");
            processStartInfo.Verb = "runas";
            try
            {
                Process.Start(processStartInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        // Now add a registry key for this app to mark this action as done 
        applicationKeys.SetValue("LongPathsEnabled", 1, RegistryValueKind.DWord);
    }

    public void Connect()
    {
        // Check if a .tagster file exists in the directory.
        if (!File.Exists(_filePath.FullName + "/.tagster")) throw new MissingFileException("The .tagster file does not exist.", 0);

        // Read the contents as a json object.
        string json = File.ReadAllText(".tagster");
        _config = JsonSerializer.Deserialize<ApplicationConfig>(json) ?? throw new InvalidOperationException("Json error in .tagster file.");

        // Check if the database exists.
        if (!File.Exists(_filePath.FullName + "/database.db"))
            // Throw an exception if the database does not exist.
            throw new UninitialisedDatabaseException("The database does not exist.");
        // The database exists, so we can connect to it.
        _database = new Database(_filePath.FullName + "/database.db");
        _initialised = true;
    }

    /// <summary>
    ///     Called by the frontend to initialise the directory by creating the database and indexing the files.
    /// </summary>
    public void InitialiseDirectory(bool fromExistingSources = false)
    {
        _database = Database.InitialiseNew(_filePath);

        // Now enumerate through the files in the directory and add them to the database.
        foreach (FileInfo file in _filePath.EnumerateFiles()) _database.AddFile(file, fromExistingSources);

        // Set the initialised flag to true.
        _initialised = true;

        // Create a new .tagster file.
        File.WriteAllText(_filePath.FullName + "/.tagster", JsonSerializer.Serialize(_config));
    }

    public void Dispose()
    {
        _config = null!;
        if (_initialised) _database.Dispose();
    }

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Database exposed methods
     */

    public DbFile AddFile(FileInfo file, bool tagsFromFileName = false)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.AddFile(file, tagsFromFileName, _config.Delimiter);
    }

    public DbFile AddFile(string path, bool tagsFromFileName = false)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.AddFile(path, tagsFromFileName, _config.Delimiter);
    }

    public DbFile GetFile(int id)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.GetFile(id);
    }

    public DbTag AddTag(string tag, string colour = "")
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.AddTag(tag, colour);
    }

    public DbTag GetTag(int id)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.GetTag(id)!;
    }

    public DbTag GetTag(string tagName)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.GetTag(tagName)!;
    }

    public List<DbTag?> GetSimilarTags(string tag)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.GetSimilarTags(tag);
    }

    public void EditTag(int id, string? tag = null, string? colour = null)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        _database.EditTag(id, tag, colour);
    }

    public void AddTagToFile(int fileId, int tagId)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        _database.AddTagToFile(fileId, tagId);
    }

    public void RemoveTagFromFile(int fileId, int tagId)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        _database.RemoveTagFromFile(fileId, tagId);
    }

    public List<DbTag?> GetTagsForFile(int fileId)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.GetTagsForFile(fileId);
    }

    public List<DbFile?> GetFilesWithTag(int tagId)
    {
        if (!_initialised) throw new UninitialisedDatabaseException("The database has not been initialised.");
        return _database.GetFilesWithTag(tagId);
    }
}

internal class Database
{
    private readonly SQLiteConnection _connection;

    /// <summary>
    ///     Initialises a new database connection.
    /// </summary>
    /// <param name="path">The path for the database file.</param>
    public Database(string path)
    {
        _connection = new SQLiteConnection("Data Source=" + path + ";Version=3;");
        _connection.Open();
    }

    /// <summary>
    ///     Initialises a new database in the specified directory.
    /// </summary>
    /// <param name="path"></param>
    public static Database InitialiseNew(DirectoryInfo path)
    {
        // First create the database file.
        SQLiteConnection.CreateFile(path.FullName + "/database.db");

        Database database = new(path.FullName + "/database.db");

        // Read in schema.sql and execute it.
        SQLiteCommand command = new(resources.DbSchema, database._connection);
        command.ExecuteNonQuery();

        // Enumerate through the files in the directory and add them to the database.
        foreach (FileInfo file in path.EnumerateFiles()) database.AddFile(file);

        return database;
    }

    public void Dispose()
    {
        _connection.Close();
    }

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * File Methods
     */

    /// <summary>
    ///     Adds a file to the database.
    /// </summary>
    /// <param name="file">The file to add.</param>
    /// <param name="tagsFromFileName">Whether to extract the tags from the file name.</param>
    /// <param name="delimiter">The tag delimiter character.</param>
    public DbFile AddFile(FileInfo file, bool tagsFromFileName = true, string delimiter = "&")
    {
        if (!file.Exists) throw new FileNotFoundException("The file does not exist.", file.FullName);


        // Create a new command.
        SQLiteCommand command = new("INSERT INTO files (path) VALUES (@path) RETURNING id;", _connection);

        // Add the parameters.
        command.Parameters.AddWithValue("@path", file.FullName);

        // Execute the command and return the id.
        int tagId = (int)command.ExecuteScalar();

        if (!tagsFromFileName) return GetFile(tagId);

        // Get any existing tags for the file.
        string tagGroup = file.FullName.Split(".")[0]; // The tags are stored in the file name before the first period.

        // Split the tags into an array at &
        string[] tags = tagGroup.Split(delimiter);

        // Add the tags to the database.
        foreach (string tag in tags)
        {
            // Add the tag if it does not exist.
            DbTag? dbTag = GetTag(tag) ?? new DbTag(AddTag(tag).Id, DateTime.Now, tag, "");

            // Add the tag to the file.
            AddTagToFile(tagId, dbTag.Id);
        }

        return GetFile(tagId);
    }

    /// <summary>
    ///     Adds a file to the database.
    /// </summary>
    /// <param name="path">Full path to file to add.</param>
    /// <param name="tagsFromFileName">Whether to extract the tags from the file name.</param>
    /// <param name="delimiter">The tag delimiter character.</param>
    public DbFile AddFile(string path, bool tagsFromFileName = true, string delimiter = "&")
    {
        return AddFile(new FileInfo(path), tagsFromFileName, delimiter);
    }

    public DbFile GetFile(int id)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT * FROM files WHERE id = @id;", _connection);

        // Add the parameter.
        command.Parameters.AddWithValue("@id", id);

        // Execute the command and get the reader.
        SQLiteDataReader reader = command.ExecuteReader();

        // Read the data.
        reader.Read();
        int fileId = reader.GetInt32(0);
        DateTime added = reader.GetDateTime(2);
        string filePath = reader.GetString(1);
        reader.Close();

        // Create a new file object and return it
        return new DbFile(fileId, added, filePath, GetTagsForFile(fileId));
    }

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Tag Methods
     */

    /// <summary>
    ///     Adds a tag to the database.
    /// </summary>
    /// <param name="tag">The tag name to add.</param>
    /// <param name="colour"></param>
    public DbTag AddTag(string tag, string colour = "")
    {
        // Create a query.
        SQLiteCommand command = new("INSERT INTO tags (tag, colour) VALUES (@tag, @colour) RETURNING id;", _connection);
        command.Parameters.AddWithValue("@tag", tag);
        command.Parameters.AddWithValue("@colour", colour);

        // Execute the command and return the id.
        return GetTag((int)command.ExecuteScalar())!;
    }

    /// <summary>
    ///     Edits a tag in the database.
    /// </summary>
    /// <param name="id">The id of the tag to edit.</param>
    /// <param name="tag">The new tag name.</param>
    /// <param name="colour">The new tag colour</param>
    public void EditTag(int id, string? tag = null, string? colour = null)
    {
        // First get the tag
        DbTag dbTag = GetTag(id) ?? throw new MissingTagException("The tag does not exist.", id);

        // Create a new query.
        SQLiteCommand command = new("UPDATE tags SET tag = @tag, colour = @colour WHERE id = @id;", _connection);
        command.Parameters.AddWithValue("@tag", tag ?? dbTag.Name);
        command.Parameters.AddWithValue("@colour", colour ?? dbTag.Colour);
        command.Parameters.AddWithValue("@id", id);

        // Execute the command.
        command.ExecuteNonQuery();
    }

    public DbTag? GetTag(int id)
    {
        // Create the query.
        SQLiteCommand command = new("SELECT * FROM tags WHERE id = @id;", _connection);
        command.Parameters.AddWithValue("@id", id);

        // Get the data.
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();

        // Create a new tag object.
        DbTag tag = new(reader.GetInt32(0), reader.GetDateTime(1), reader.GetString(2), reader.GetString(3));

        // Close the reader and return the tag.
        reader.Close();
        return tag;
    }

    public DbTag? GetTag(string tagName)
    {
        // Create query
        SQLiteCommand command = new("SELECT * FROM tags WHERE tag = @tag;", _connection);
        command.Parameters.AddWithValue("@tag", tagName);

        // Get the data 
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();

        if (!reader.HasRows)
        {
            reader.Close();
            return null;
        }

        // Create a new tag object.
        DbTag tag = new(reader.GetInt32(0), reader.GetDateTime(1), reader.GetString(2), reader.GetString(3));

        // Close the reader and return the tag.
        reader.Close();
        return tag;
    }

    /// <summary>
    ///     Gets all tags with similar names.
    /// </summary>
    /// <param name="tag">The tag name to get similar tags to.</param>
    /// <returns>List of similar tags.</returns>
    public List<DbTag?> GetSimilarTags(string tag)
    {
        // Create query
        SQLiteCommand command = new("SELECT * FROM tags WHERE tag LIKE %@tag%;", _connection);
        command.Parameters.AddWithValue("@tag", tag);

        // Get the tags.
        SQLiteDataReader reader = command.ExecuteReader();
        List<DbTag?> tags = [];
        while (reader.Read()) tags.Add(new DbTag(reader.GetInt32(0), reader.GetDateTime(1), reader.GetString(2), reader.GetString(3)));

        reader.Close();
        return tags;
    }

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * File Tag Methods
     */

    public void AddTagToFile(int fileId, int tagId)
    {
        // Create a new command.
        SQLiteCommand command = new("INSERT INTO file_tags (file_id, tag_id) VALUES (@fileId, @tagId);", _connection);

        // Add the parameters.
        command.Parameters.AddWithValue("@fileId", fileId);
        command.Parameters.AddWithValue("@tagId", tagId);

        // Execute the command.
        command.ExecuteNonQuery();
    }

    public void RemoveTagFromFile(int fileId, int tagId)
    {
        // Create a new command.
        SQLiteCommand command = new("DELETE FROM file_tags WHERE file_id = @fileId AND tag_id = @tagId;", _connection);

        // Add the parameters.
        command.Parameters.AddWithValue("@fileId", fileId);
        command.Parameters.AddWithValue("@tagId", tagId);

        // Execute the command.
        command.ExecuteNonQuery();
    }

    public List<DbTag?> GetTagsForFile(int fileId)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT tag_id FROM file_tags WHERE file_id = @fileId;", _connection);

        // Add the parameter.
        command.Parameters.AddWithValue("@fileId", fileId);

        // Execute the command and get the reader.
        SQLiteDataReader reader = command.ExecuteReader();

        // Create a new list of tags.
        List<DbTag?> tags = [];

        // Read the reader.
        while (reader.Read()) tags.Add(GetTag(reader.GetInt32(0))!);

        // Close the reader and return the tags.
        reader.Close();
        return tags;
    }

    /// <summary>
    ///     Gets all files with a specific tag.
    /// </summary>
    /// <param name="tagId">The id of the tag in which to get files for.</param>
    /// <returns></returns>
    public List<DbFile?> GetFilesWithTag(int tagId)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT file_id FROM file_tags WHERE tag_id = @tagId;", _connection);

        // Add the parameter.
        command.Parameters.AddWithValue("@tagId", tagId);

        // Execute the command and get the reader.
        SQLiteDataReader reader = command.ExecuteReader();

        // Create a new list of files.
        List<DbFile?> files = [];

        // Read the reader.
        while (reader.Read()) files.Add(GetFile(reader.GetInt32(0)));

        // Close the reader and return the files.
        reader.Close();
        return files;
    }
}

/// <summary>
///     Represents a tag in the database.
/// </summary>
/// <param name="id">Id of the tag.</param>
/// <param name="name">Name of the tag.</param>
/// <param name="colour">Colour of the tag.</param>
public class DbTag(int id, DateTime created, string name, string colour)
{
    public int Id { get; } = id;
    public DateTime Created { get; } = created;
    public string Name { get; } = name;
    public string Colour { get; } = colour;
}

/// <summary>
///     Represents a file in the database.
/// </summary>
/// <param name="id">Id of the file.</param>
/// <param name="path">Path to the file.</param>
/// <param name="tags">List of tags.</param>
public class DbFile(int id, DateTime added, string path, List<DbTag> tags)
{
    public int Id { get; } = id;
    public DateTime Added { get; } = added;
    public DateTime Created { get; } = File.GetCreationTime(path);
    public DateTime Modified { get; } = File.GetLastWriteTime(path);
    public string Path { get; } = path;
    public List<DbTag> Tags { get; } = tags;
}

/// <summary>
///     Used when a tag is not found.
/// </summary>
/// <param name="message">Error message</param>
/// <param name="id">The id of the tag</param>
public class MissingTagException(string message, int id) : Exception(message)
{
    public int TagId { get; } = id;
}

/// <summary>
///     Used when a file is not found.
/// </summary>
/// <param name="message">Error message</param>
/// <param name="id">The id of the file</param>
public class MissingFileException(string message, int id) : Exception(message)
{
    public int FileId { get; } = id;
}

/// <summary>
///     Used when tag collisions are detected.
/// </summary>
/// <param name="message">Error message</param>
public class DuplicateTagsException(string message) : Exception(message);

/// <summary>
///     Used when the database has not been initialised.
/// </summary>
/// <param name="message">Error message </param>
public class UninitialisedDatabaseException(string message) : Exception(message);