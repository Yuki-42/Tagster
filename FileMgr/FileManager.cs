// Third-party libraries

using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using FileMgr.Handlers;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Local libraries


namespace FileMgr;

/// <summary>
///     Default configuration values.
/// </summary>
public class DefaultConfig
{
    /// <summary>
    ///     Tag delimiter in file names.
    /// </summary>
    public const char Delimiter = '&';

    /// <summary>
    ///     Database file.
    /// </summary>
    public const string DatabaseFile = "database.db";
}

/// <summary>
///     Model for the application configuration.
/// </summary>
public class ApplicationConfig
{
    /// <summary>
    ///     Tag delimiter in file names.
    /// </summary>
    public required string Delimiter { get; set; } = DefaultConfig.Delimiter.ToString();
    
    /// <summary>
    ///     Database file location.
    /// </summary>
    public required string DatabaseFile { get; set; } = DefaultConfig.DatabaseFile;
    
    /// <summary>
    ///    Managed name. This is the "friendly" name of the management system shown on the frontend.
    /// </summary>
    public required string ManagedName { get; set; } = Environment.CurrentDirectory;
}

/// <summary>
///     Stores runtime configuration for the application.
/// </summary>
/// <param name="rootPath">Root path of the application.</param>
public class RuntimeConfiguration(DirectoryInfo rootPath, ApplicationConfig config)
{
    /// <summary>
    ///     Effectively the CWD for the application.
    /// </summary>
    public DirectoryInfo RootPath { get; } = rootPath;

    /// <summary>
    ///     Location of the config file.
    /// </summary>
    public FileInfo ConfigFile { get; } = new(rootPath.FullName + "/.tagster");

    /// <summary>
    ///     Location of the database file.
    /// </summary>
    public FileInfo DatabaseFile { get; } = new(rootPath.FullName + "/" + config.DatabaseFile);

    /// <summary>
    ///     Location of the executable directory.
    /// </summary>
    public DirectoryInfo ExecutableDirectory { get; private set; } = GetExecutableDirectory();

    /// <summary>
    ///     Location of the schema file.
    /// </summary>
    public FileInfo SchemaFile { get; } = new(GetExecutableDirectory().FullName + "/schema.sql");

    private static DirectoryInfo GetExecutableDirectory()
    {
        // Get the path to the entry assembly
        string path = Assembly.GetEntryAssembly()!.Location;

        // Now do some fancy index manipulation to get the directory
        int index = path.LastIndexOf(Path.DirectorySeparatorChar);
        return new DirectoryInfo(path[..index]);
    }
}

/// <summary>
///     Public exposed class for file management using tags and file paths.
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class FileManager
{
    private readonly SQLiteConnection _connection;

    /// <summary>
    ///     Runtime configuration.
    /// </summary>
    private readonly RuntimeConfiguration _runtimeConfiguration;
    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Class Variables
     */

    /// <summary>
    ///     Stores persistent application configuration.
    /// </summary>
    private ApplicationConfig _config;

    /// <summary>
    ///     Id of the instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <inheritdoc cref="ApplicationConfig.ManagedName"/>
    public string FriendlyName => _config.ManagedName;
    
    /// <summary>
    /// Path to the root directory.
    /// </summary>
    public string RootPath => _runtimeConfiguration.RootPath.FullName;

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * Miscellaneous Methods
     */

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileManager" /> class.
    /// </summary>
    /// <param name="filePath">The path to the data directory.</param>
    /// <param name="actionCode">
    ///     The action code to perform.
    ///     1: Connect to an existing db.
    ///     2: Initialise a new db from scratch.
    ///     3: Initialise a new db from existing sources. (Rebuild)
    /// </param>
    public FileManager(
        DirectoryInfo filePath,
        int actionCode
    )
    {
        // Load configuration
        _runtimeConfiguration = LoadConfigAndRuntime(filePath);

        // Do the registry checks
        DoRegistryChecks();

        // Execute the correct action code
        switch (actionCode)
        {
            case 1: // Connect to an existing database
                // Do the runtime exceptions before connecting.
                DoRuntimeExceptions();
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
        _connection = new SQLiteConnection("Data Source=" + _runtimeConfiguration.DatabaseFile.FullName + ";Version=3;");
        _connection.Open();

        // Create the handlers
        Debug.Assert(_config != null, nameof(_config) + " != null");
        Tags = new Tags(_connection, _config);
        Files = new Files(_connection, _config);
        Relations = new Relations(_connection, _config);

        // Create handlers group
        HandlersGroup handlersGroup = new(Files, Tags, Relations);

        // Populate the handlers
        Tags.Populate(handlersGroup);
        Files.Populate(handlersGroup);
        Relations.Populate(handlersGroup);

        // Add files to database if we are initialising a new database
        if (actionCode == 2) AddFromDirectory(filePath);
    }

    // Overload for string path
    /// <inheritdoc cref="FileManager" />
    public FileManager(
        string filePath,
        int actionCode
    ) : this(new DirectoryInfo(filePath), actionCode)
    {
    }

    /// <summary>
    ///     Tags handler.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Tags Tags { get; }

    /// <summary>
    ///     Files handler.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Files Files { get; }

    /// <summary>
    ///     Relations handler.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Relations Relations { get; }

    /// <summary>
    ///     Checks if the filesystem has been initialised for a tagster management system.
    /// </summary>
    /// <returns>
    ///     The initialisation status.
    ///     0: Initialised
    ///     10: Uninitialised
    ///     2x: Name schema present but no db
    ///     3x: Bad DB
    ///     4x: Bad config
    ///     x0: Missing object. (e.g. db, config)
    ///     x1: Misconfigured or corrupt object. Means a failure in the lower-level libraries used in the project.
    /// </returns>
    public int CheckFilesystemInitialised()
    {
        // Work backwards through errors to find the first one.
        if (!_runtimeConfiguration.ConfigFile.Exists) return 40;

        // Attempt to read the config file.
        try
        {
            JToken.Parse(_runtimeConfiguration.ConfigFile.OpenText().ReadToEnd());
        }
        catch (JsonReaderException)
        {
            return 41;
        }

        return 0;
    }


    /// <summary>
    ///     Performs runtime checks to ensure the program can run. Uses CheckFilesystemInitialised.
    /// </summary>
    /// <param name="skips">Codes to skip the error raising for.</param>
    private void DoRuntimeExceptions(ICollection<int>? skips = null)
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
    private static void DoRegistryChecks()
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
                Console.WriteLine(e.Message); // TODO: Remove this once the program is stable.
                Environment.Exit(1);
            }
        }

        // Now add a registry key for this app to mark this action as done 
        applicationKeys.SetValue("LongPathsEnabled", 1, RegistryValueKind.DWord);
    }


    /// <summary>
    ///     Called by the frontend to initialise the directory by creating the database and indexing the files.
    /// </summary>
    public void InitialiseDirectory(bool fromExistingSources = false)
    {
        // Do the runtime exceptions before connecting.
        DoRuntimeExceptions([]);

        // Create a new .tagster file.
        File.WriteAllText(_runtimeConfiguration.ConfigFile.FullName, JsonConvert.SerializeObject(new DefaultConfig()));

        // Get the config object to pass to the database.
        _config = JsonConvert.DeserializeObject<ApplicationConfig>(_runtimeConfiguration.ConfigFile.OpenText().ReadToEnd())!;

        // Create an sqlite connection
        SQLiteConnection connection = new("Data Source=" + _runtimeConfiguration.DatabaseFile.FullName + ";Version=3;");
        connection.Open();

        // Execute schema creation
        string schema = File.ReadAllText(_runtimeConfiguration.SchemaFile.FullName);
        SQLiteCommand command = new(schema, connection); // This might break as we are passing several commands at once.
        command.ExecuteNonQuery();
    }

    /// <summary>
    ///     Initialise a new directory from existing sources.
    /// </summary>
    public void InitialiseNew()
    {
        InitialiseDirectory(true);
    }

    /// <summary>
    /// </summary>
    public void InitialiseExisting()
    {
        InitialiseDirectory();
    }

    /// <summary>
    ///     Adds all files in the directory to the database. If it encounters a subdirectory, it will recursively add all files
    ///     in that directory.
    /// </summary>
    /// <param name="directory"></param>
    private void AddFromDirectory(DirectoryInfo directory)
    {
        foreach (FileInfo file in directory.EnumerateFiles())
        {
            Console.WriteLine(file.Exists);
            Console.WriteLine(file.FullName);
            Files.Add(file);
        }

        foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories()) AddFromDirectory(subDirectory);
    }

    /// <summary>
    ///     Loads the configuration file into _config.
    /// </summary>
    private RuntimeConfiguration LoadConfigAndRuntime(DirectoryInfo path)
    {
        FileInfo configFile = new("./.tagster");
        
        // Check if the config file exists
        if (!configFile.Exists)
        {
            // Create a new config file
            File.WriteAllText(configFile.FullName, JsonConvert.SerializeObject(new DefaultConfig()));
        }
        
        // Read the config file
        _config = JsonConvert.DeserializeObject<ApplicationConfig>(configFile.OpenText().ReadToEnd())!;
        
        // Now that the config is read in, we can create the runtime configuration
        return new RuntimeConfiguration(path, _config);
    }

    /// <summary>
    ///     Exits the instance cleanly by closing the database connections and saving any in-memory changes.
    /// </summary>
    public void Exit()
    {
        // Close the database connections
        _connection.Close();
    }
}