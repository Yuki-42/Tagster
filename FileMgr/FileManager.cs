// Third-party libraries: Newtonsoft.Json, System.Data.SQLite
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;

// Local libraries
using FileMgr.Handlers;
using FileMgr.Objects;
using FileMgr.Exceptions;
using File = FileMgr.Objects.File;

namespace FileMgr;

/// <summary>
/// Model for the application configuration.
/// </summary>
public class ApplicationConfig
{
    /// <summary>
    /// Tag delimiter in file names.
    /// </summary>
    public required string Delimiter { get; set; }
}

/// <summary>
/// Stores runtime configuration for the application.
/// </summary>
/// <param name="rootPath">Root path of the application.</param>
/// <param name="configFile">Location of config file.</param>
/// <param name="databaseFile">Location of database file.</param>
public class RuntimeConfiguration(DirectoryInfo rootPath, FileInfo configFile, FileInfo databaseFile)
{
    /// <summary>
    /// Effectively the CWD for the application.
    /// </summary>
    public DirectoryInfo RootPath { get; private set; } = rootPath;

    /// <summary>
    /// Location of the config file.
    /// </summary>
    public FileInfo ConfigFile { get; private set; } = configFile;

    /// <summary>
    /// Location of the database file.
    /// </summary>
    public FileInfo DatabaseFile { get; private set; } = databaseFile;
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
    ///     Stores persistent application configuration.
    /// </summary>
    private ApplicationConfig _config;

    /// <summary>
    ///     Runtime configuration.
    /// </summary>
    private RuntimeConfiguration _runtimeConfiguration;

    /// <summary>
    /// Tags handler.
    /// </summary>
    public Tags Tags { get; private set; }

    /// <summary>
    /// Files handler.
    /// </summary>
    public Files Files { get; private set; }

    /// <summary>
    /// Relations handler.
    /// </summary>
    public Relations Relations { get; private set; }

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Miscellaneous Methods
     */

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileManager" /> class.
    /// </summary>
    /// <param name="filePath">The path to the data directory.</param>
    /// <param name="actionCode">
    /// The action code to perform.
    ///
    /// 1: Connect to an existing db.
    /// 2: Initialise a new db from scratch.
    /// 3: Initialise a new db from existing sources. (Rebuild)
    /// </param>
    public FileManager(
        DirectoryInfo filePath,
        int actionCode
        )
    {
        // Create service provider
        IServiceCollection services = new ServiceCollection();

        // Do runtime configuration
        _runtimeConfiguration = new RuntimeConfiguration(
            filePath,
            new FileInfo(filePath.FullName + "/.tagster"),
            new FileInfo(filePath.FullName + "/database.db")
        );
        
        // Do the registry checks
        DoRegistryChecks();

        switch (actionCode)
        {
            case 1:
                Connect();
                break;
            case 2:
                InitialiseNew();
                break;
            case 3:
                InitialiseExisting();
                break;
            default:
                throw new InvalidOperationException("Invalid action code.");
        }

        // Create an sqlite connection
        SQLiteConnection connection = new("Data Source=" + _runtimeConfiguration.RootPath.FullName + ";Version=3;");
        connection.Open();

        // Create the handlers
        Tags = new Tags(connection, _config);
        Files = new Files(connection, _config);
        Relations = new Relations(connection, _config);

        // Close the connection
        connection.Close();
        connection.Dispose();

        // Register services
        services.AddSingleton(Tags);
        services.AddSingleton(Files);
        services.AddSingleton(Relations);

        services.AddSingleton(_config);
        services.AddSingleton(_runtimeConfiguration);
    }

    // Overload for string path
    /// <inheritdoc cref="FileManager"/>
    public FileManager(
        string filePath,
        int actionCode
        ) : this(new DirectoryInfo(filePath), actionCode)
    {
    }

    /// <summary>
    /// Checks if the filesystem has been initialised for a tagster management system.
    /// </summary>
    /// <returns>
    /// The initialisation status.
    ///
    /// 0: Initialised
    /// 10: Uninitialised
    /// 2x: Name schema present but no db
    /// 3x: Bad DB
    /// 4x: Bad config
    ///
    /// x0: Missing object. (e.g. db, config)
    /// x1: Misconfigured or corrupt object. Means a failure in the lower-level libraries used in the project.
    /// </returns>
    public int CheckFilesystemInitialised()
    {
        // Work backwards through errors to find the first one.
        if (!_configFile.Exists) return 40;

        // Attempt to read the config file.
        try
        {
            JToken.Parse(_configFile.OpenText().ReadToEnd());
        }
        catch (JsonReaderException)
        {
            return 41;
        }

        return 0;
    }


    /// <summary>
    /// Performs runtime checks to ensure the program can run. Uses CheckFilesystemInitialised.
    /// </summary>
    /// <param name="skips">Codes to skip the error raising for.</param>
    private void DoRuntimeExceptions(List<int>? skips = null)
    {
        // Check the filesystem initialisation status.
        int status = CheckFilesystemInitialised();

        if (status == 0) return;
        if (skips != null && skips.Contains(status)) return;

        // Raise an exception based on the status.
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
                Console.WriteLine(e.Message);  // TODO: Remove this once the program is stable.
                Environment.Exit(1);
            }
        }

        // Now add a registry key for this app to mark this action as done 
        applicationKeys.SetValue("LongPathsEnabled", 1, RegistryValueKind.DWord);
    }


    /// <summary>
    /// Tries to connect to an existing DB in the current working directory.
    /// </summary>
    private void Connect()
    {
        // Do the runtime exceptions before connecting.
        DoRuntimeExceptions();

        // We know that everything is present and correct, so we can read in the config
        _config = JsonConvert.DeserializeObject<ApplicationConfig>(_configFile.OpenText().ReadToEnd())!;
    }

    /// <summary>
    ///     Called by the frontend to initialise the directory by creating the database and indexing the files.
    /// </summary>
    public void InitialiseDirectory(bool fromExistingSources = false)
    {
        // Do the runtime exceptions before connecting.
        DoRuntimeExceptions([]);

        // Create a new .tagster file.
        System.IO.File.WriteAllText(_configFile.FullName, JsonConvert.SerializeObject(_config));

        // Get the config object to pass to the database.
        _config = JsonConvert.DeserializeObject<ApplicationConfig>(_configFile.OpenText().ReadToEnd())!;

        // Now add all files in the directory to the database.
        AddFromDirectory(_filePath);

    }

    /// <summary>
    /// Tiny helper method to initialise a new directory.
    /// </summary>
    public void InitialiseNew()
    {
        InitialiseDirectory(true);
    }

    public void InitialiseExisting()
    {
        InitialiseDirectory(false);
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
}

internal class Database
{


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
    public File AddFile(FileInfo file, bool tagsFromFileName = true)
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
        foreach (string sTag in tags)
        {
            // Add the tag if it does not exist.
            Tag? tag = this.Tags.Get(sTag) ?? new Tag(this.Tags.New(sTag).Id, DateTime.Now, sTag, "");

            // Add the tag to the file.
            AddTagToFile(tagId, tag.Id);
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



    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Tag Methods
     */


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

    public List<Tag?> GetTagsForFile(long fileId)
    {
        // Create a new command.
        SQLiteCommand command = new("SELECT tag_id FROM file_tags WHERE file_id = @fileId;", _connection);

        // Add the parameter.
        command.Parameters.AddWithValue("@fileId", fileId);

        // Execute the command and get the reader.
        SQLiteDataReader reader = command.ExecuteReader();

        // Create a new list of tags.
        List<Tag?> tags = [];

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
        List<File?> files = [];

        // Read the reader.
        while (reader.Read()) files.Add(GetFile(reader.GetInt64(0)));

        // Close the reader and return the files.
        reader.Close();
        return files;
    }
}
