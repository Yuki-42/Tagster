// Third-party libraries
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Local libraries
using FileMgr.Handlers;
using FileMgr.Objects;

// Overload for FileInfo
using File = FileMgr.Objects.File;

namespace FileMgr;

/// <summary>
///     Model for the application configuration.
/// </summary>
public class ApplicationConfig
{
    /// <summary>
    ///     Tag delimiter in file names.
    /// </summary>
    public required string Delimiter { get; set; }
}

/// <summary>
///     Stores runtime configuration for the application.
/// </summary>
/// <param name="rootPath">Root path of the application.</param>
/// <param name="configFile">Location of config file.</param>
/// <param name="databaseFile">Location of database file.</param>
public class RuntimeConfiguration(DirectoryInfo rootPath, FileInfo configFile, FileInfo databaseFile)
{
    /// <summary>
    ///     Effectively the CWD for the application.
    /// </summary>
    public DirectoryInfo RootPath { get; } = rootPath;

    /// <summary>
    ///     Location of the config file.
    /// </summary>
    public FileInfo ConfigFile { get; private set; } = configFile;

    /// <summary>
    ///     Location of the database file.
    /// </summary>
    public FileInfo DatabaseFile { get; private set; } = databaseFile;
}

/// <summary>
///     Public exposed class for file management using tags and file paths.
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class FileManager
{
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

        // Create handlers group
        HandlersGroup handlersGroup = new(Files, Tags, Relations);
        
        // Populate the handlers
        Tags.Populate(handlersGroup);
        Files.Populate(handlersGroup);
        Relations.Populate(handlersGroup);

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
                Console.WriteLine(e.Message); // TODO: Remove this once the program is stable.
                Environment.Exit(1);
            }
        }

        // Now add a registry key for this app to mark this action as done 
        applicationKeys.SetValue("LongPathsEnabled", 1, RegistryValueKind.DWord);
    }


    /// <summary>
    ///     Tries to connect to an existing DB in the current working directory.
    /// </summary>
    private void Connect()
    {
        // Do the runtime exceptions before connecting.
        DoRuntimeExceptions();

        // We know that everything is present and correct, so we can read in the config
        _config = JsonConvert.DeserializeObject<ApplicationConfig>(_runtimeConfiguration.ConfigFile.OpenText().ReadToEnd())!;
    }

    /// <summary>
    ///     Called by the frontend to initialise the directory by creating the database and indexing the files.
    /// </summary>
    public void InitialiseDirectory(bool fromExistingSources = false)
    {
        // Do the runtime exceptions before connecting.
        DoRuntimeExceptions([]);

        // Create a new .tagster file.
        System.IO.File.WriteAllText(_runtimeConfiguration.ConfigFile.FullName, JsonConvert.SerializeObject(_config));

        // Get the config object to pass to the database.
        _config = JsonConvert.DeserializeObject<ApplicationConfig>(_runtimeConfiguration.ConfigFile.OpenText().ReadToEnd())!;

        // Now add all files in the directory to the database.
        AddFromDirectory(_runtimeConfiguration.RootPath);
    }

    /// <summary>
    ///     Initialise a new directory from existing sources.
    /// </summary>
    public void InitialiseNew()
    {
        InitialiseDirectory(true);
    }
    
    /// <summary>
    /// 
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
}