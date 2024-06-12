using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
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
    /// <inheritdoc />
    public FileManager(string filePath) : this(new DirectoryInfo(filePath))
    {
    }

    /// <summary>
    ///     Ensures that the program can run by checking the registry for the LongPathsEnabled key.
    /// </summary>
    private void DoRegistryChecks()
    {
        // Check current platform. If it is not windows, return
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

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
            ProcessStartInfo processStartInfo = new("RegistryPatcher.exe") // This can be an .exe as it is only for windows
            {
                Verb = "runas"
            };
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


    /// <summary>
    /// Tries to connect to an existing DB in the current working directory.
    /// </summary>
    /// <exception cref="MissingFileException">Thrown when a .tagster file cannot be found in the current directory.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the JSON format in a .tagster file is non-compliant.</exception>
    /// <exception cref="UninitialisedDatabaseException">Thrown when there is no database in the current directory.</exception>
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
        _database = new Database(_filePath.FullName + "/database.db", _config);
        _initialised = true;
    }

    /// <summary>
    ///     Called by the frontend to initialise the directory by creating the database and indexing the files.
    /// </summary>
    public void InitialiseDirectory(bool fromExistingSources = false)
    {
        // Check if the directory has already been initialised by checking for the presence of a .tagster file.
        if (File.Exists(_filePath.FullName + "/.tagster"))
        {
            // The directory has already been initialised, so connect to the database.
            throw new AlreadyInitialisedDatabaseException("The directory has already been initialised.");
        }

        // Create a new .tagster file.
        File.WriteAllText(_filePath.FullName + "/.tagster", JsonSerializer.Serialize(_config));

        // Get the config object to pass to the database.
        _config = JsonSerializer.Deserialize<ApplicationConfig>(File.ReadAllText(_filePath.FullName + "/.tagster"))!;

        _database = Database.InitialiseNew(_filePath, _config);

        // Now add all files in the directory to the database.
        AddFromDirectory(_filePath);

        // Set the initialised flag to true.
        _initialised = true;
    }

    /// <summary>
    /// Adds all files in the directory to the database. If it encounters a subdirectory, it will recursively add all files in that directory.
    /// </summary>
    /// <param name="directory"></param>
    private void AddFromDirectory(DirectoryInfo directory)
    {
        foreach (FileInfo file in directory.EnumerateFiles()) _database.AddFile(file);
        foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories()) AddFromDirectory(subDirectory);
    }


    /// <summary>
    /// GC Method.
    /// </summary>
    public void Dispose()
    {
        _config = null!;
        if (_initialised) _database.Dispose();
    }

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Database exposed methods
     */
}

internal class Database
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
    ///     Initialises a new database connection.
    /// </summary>
    /// <param name="path">The path for the database file.</param>
    /// <param name="config">The configuration object.</param>
    public Database(string path, ApplicationConfig config)
    {
        _connection = new SQLiteConnection("Data Source=" + path + ";Version=3;");
        _connection.Open();
        _config = config;
    }

    /// <summary>
    ///     Initialises a new database in the specified directory.
    /// </summary>
    /// <param name="path">Path to the directory.</param>
    /// <param name="config"></param>
    public static Database InitialiseNew(DirectoryInfo path, ApplicationConfig config)
    {
        // First create the database file.
        SQLiteConnection.CreateFile(path.FullName + "/database.db");

        Database database = new(path.FullName + "/database.db", config);

        // Read in schema.sql and execute it.
        string schema = File.ReadAllText("schema.sql");
        SQLiteCommand command = new(schema, database._connection);
        command.ExecuteNonQuery();

        return database;
    }

    /// <summary>
    /// GC Method.
    /// </summary>
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
    public DbFile AddFile(FileInfo file, bool tagsFromFileName = true)
    {
        if (!file.Exists) throw new FileNotFoundException("The file does not exist.", file.FullName);

        // Create a new command.
        SQLiteCommand command = new("INSERT INTO files (path) VALUES (@path) RETURNING id;", _connection);

        // Add the parameters.
        command.Parameters.AddWithValue("@path", file.FullName);

        // Execute the command and return the id.
        long tagId = (long)command.ExecuteScalar();

        if (!tagsFromFileName) return GetFile(tagId)!;

        // Get any existing tags for the file.
        string tagGroup = file.FullName.Split(".")[0];  // The tags are stored in the file name before the first period.

        // Split the tags into an array at &
        string[] tags = tagGroup.Split(_config.Delimiter);

        // Add the tags to the database.
        foreach (string tag in tags)
        {
            // Add the tag if it does not exist.
            DbTag? dbTag = GetTag(tag) ?? new DbTag(AddTag(tag).Id, DateTime.Now, tag, "");

            // Add the tag to the file.
            AddTagToFile(tagId, dbTag.Id);
        }

        return GetFile(tagId)!;
    }

    /// <summary>
    ///     Adds a file to the database.
    /// </summary>
    /// <param name="path">Full path to file to add.</param>
    /// <param name="tagsFromFileName">Whether to extract the tags from the file name.</param>
    public DbFile AddFile(string path, bool tagsFromFileName = true)
    {
        return AddFile(new FileInfo(path), tagsFromFileName);
    }

    /// <summary>
    ///     Gets a file from the database.
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>Matching file, null if not found.</returns>
    public DbFile? GetFile(long id)
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
        return new DbFile(fileId, added, filePath, GetTagsForFile(fileId)!);
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
        SQLiteCommand command = new("INSERT INTO tags (name, colour) VALUES (@tag, @colour) RETURNING id;", _connection);
        command.Parameters.AddWithValue("@tag", tag);
        command.Parameters.AddWithValue("@colour", colour);

        // Execute the command and return the id.
        return GetTag((long)command.ExecuteScalar())!;
    }

    /// <summary>
    ///     Edits a tag in the database.
    /// </summary>
    /// <param name="id">The id of the tag to edit.</param>
    /// <param name="tag">The new tag name.</param>
    /// <param name="colour">The new tag colour</param>
    public void EditTag(long id, string? tag = null, string? colour = null)
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

    public DbTag? GetTag(long id)
    {
        // Create the query.
        SQLiteCommand command = new("SELECT * FROM tags WHERE id = @id;", _connection);
        command.Parameters.AddWithValue("@id", id);

        // Get the data.
        SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();

        if (!reader.HasRows)
        {
            reader.Close();
            return null;
        }

        // Create a new tag object.
        DbTag tag = new(reader);

        // Close the reader and return the tag.
        reader.Close();
        return tag;
    }

    public DbTag? GetTag(string tagName)
    {
        // Create query
        SQLiteCommand command = new("SELECT * FROM tags WHERE name = @tag;", _connection);
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
        DbTag tag = new(reader);

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
        SQLiteCommand command = new("SELECT * FROM tags WHERE name LIKE %@tag%;", _connection);
        command.Parameters.AddWithValue("@tag", tag);

        // Get the tags.
        SQLiteDataReader reader = command.ExecuteReader();
        List<DbTag?> tags = [];
        while (reader.Read()) tags.Add(new DbTag(reader));

        reader.Close();
        return tags;
    }

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * File Tag Methods
     */

    public void AddTagToFile(long fileId, long tagId)
    {
        // Create a new command.
        SQLiteCommand command = new("INSERT INTO file_tags (file_id, tag_id) VALUES (@fileId, @tagId);", _connection);

        // Add the parameters.
        command.Parameters.AddWithValue("@fileId", fileId);
        command.Parameters.AddWithValue("@tagId", tagId);

        // Execute the command.
        command.ExecuteNonQuery();
    }

    public void RemoveTagFromFile(long fileId, long tagId)
    {
        // Create a new command.
        SQLiteCommand command = new("DELETE FROM file_tags WHERE file_id = @fileId AND tag_id = @tagId;", _connection);

        // Add the parameters.
        command.Parameters.AddWithValue("@fileId", fileId);
        command.Parameters.AddWithValue("@tagId", tagId);

        // Execute the command.
        command.ExecuteNonQuery();
    }

    public List<DbTag?> GetTagsForFile(long fileId)
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
        while (reader.Read()) tags.Add(GetTag(reader.GetInt64(0))!);

        // Close the reader and return the tags.
        reader.Close();
        return tags;
    }

    /// <summary>
    ///     Gets all files with a specific tag.
    /// </summary>
    /// <param name="tagId">The id of the tag in which to get files for.</param>
    /// <returns></returns>
    public List<DbFile?> GetFilesWithTag(long tagId)
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
        while (reader.Read()) files.Add(GetFile(reader.GetInt64(0)));

        // Close the reader and return the files.
        reader.Close();
        return files;
    }
}

/// <summary>
///     Represents a tag in the database.
/// </summary>
public class DbTag
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
    public DbTag(long id, DateTime created, string name, string colour)
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
    public DbTag(SQLiteDataReader reader)
    {
        Id = reader.GetInt64(0);
        Created = reader.GetDateTime(1);
        Name = reader.GetString(2);
        Colour = reader.GetString(3);
    }

}

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
    public List<DbTag> Tags { get; }

    /// <summary>
    ///     Initialises a new file object.
    /// </summary>
    /// <param name="id">Id of the file.</param>
    /// <param name="added">File added to db time.</param>
    /// <param name="path">Path to file.</param>
    /// <param name="tags">List of tags applied to file.</param>
    public DbFile(long id, DateTime added, string path, List<DbTag> tags)
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
    public DbFile(SQLiteDataReader reader, List<DbTag> tags)
    {
        Id = reader.GetInt64(0);
        Added = reader.GetDateTime(1);
        Created = File.GetCreationTime(reader.GetString(2));
        Modified = File.GetLastWriteTime(reader.GetString(2));
        Path = reader.GetString(2);
        Tags = tags;
    }
}

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
