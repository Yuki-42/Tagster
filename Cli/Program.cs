using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
using FileMgr;

namespace Cli;

internal class Program
{
    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Return code.</returns>
    public static int Main(string[] args)
    {
        // Create a new instance of the program
        Program program = new();

        // Process the arguments
        return Parser.Default.ParseArguments<GuiOptions, BuildOptions, ImportOptions>(args)
            .MapResult(
                (GuiOptions opts) => RunGui(opts),
                (BuildOptions opts) => program.Build(opts),
                (ImportOptions opts) => program.ImportFiles(opts),
                errs => 1
            );
    }

    /// <summary>
    ///     File manager instance.
    /// </summary>
    private readonly FileManager _fileManager;

    /// <summary>
    ///    Create a new instance of the program.
    /// </summary>
    private Program()
    {
        // Initialize the file manager with the current directory from which the program was run
        try
        {
            _fileManager = new FileManager(Directory.GetCurrentDirectory());
        }
        catch (MissingFileException e)
        {
            Console.WriteLine(e.Message);
            Environment.Exit(1);
        }
    }


    /// <summary>
    /// Runs the gui executable.
    /// </summary>
    /// <param name="opts">Options to pass to the GUI.</param>
    /// <returns>Return code of the GUI Process.</returns>
    private static int RunGui(GuiOptions opts)
    {
        // Check if the GUI executable is installed
        if (!File.Exists(@"TagsterGui.exe"))
        {
            Console.WriteLine(Resources.GuiExecutableNotFound);
            return 1;
        }

        // Start the GUI
        Process process = Process.Start("TagsterGui.exe", opts.ToString() ?? string.Empty);

        // Wait for the GUI to exit
        process.WaitForExit();

        return process.ExitCode;
    }

    private int Build(BuildOptions opts)
    {
        // Work out which build command to run
        switch (opts.Mode)
        {
            // Run the appropriate build command
            case "new":
                return BuildNew();
            case "existing":
                return BuildExisting();
            default:
                Console.WriteLine("Invalid build command.");
                return 1;
        }
    }

    private int BuildNew()
    {
        // Return the exit code
        Console.WriteLine(Resources.BuildNewStart);

        // Create the management system
        try
        {
            _fileManager.InitialiseDirectory();
        }
        catch (AlreadyInitialisedDatabaseException)
        {
            Console.WriteLine(Resources.BuildNewFailAlreadyInitialised);
        }
        return 0;
    }

    /// <summary>
    /// Build a management system from existing sources in the current directory.
    /// </summary>
    /// <returns>Return status of subsystem.</returns>
    private int BuildExisting()
    {
        // Return the exit code
        Console.WriteLine(Resources.BuildExistingStart);

        return 2;
    }

    /// <summary>
    /// Import files.
    /// </summary>
    /// <param name="opts"></param>
    /// <returns></returns>
    private int ImportFiles(ImportOptions opts)
    {
        // Return the exit code
        return 2;
    }


    [Verb("gui", HelpText = "Run the GUI")]
    // ReSharper disable once ClassNeverInstantiated.Local
    private class GuiOptions
    {
    }

    [Verb("build", false, ["init"], HelpText = "Build a new management system in the current directory")]
    // ReSharper disable once ClassNeverInstantiated.Local
    private class BuildOptions
    {
        [Usage(ApplicationAlias = "tagster")]
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Initialize a new management system", new BuildOptions { Mode = "new" });
                yield return new Example("Initialize a new management system from existing sources in this directory", new BuildOptions { Mode = "existing" });
            }
        }

        [Option('m', "mode", HelpText = "The build mode", Required = true)]
        public required string Mode { get; set; }
    }


    [Verb("import", HelpText = "Import files")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImportOptions
    {
        [Usage(ApplicationAlias = "tagster")]
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Import a file", new ImportOptions { File = "file.txt" });
                yield return new Example("Import multiple files from a directory", new ImportOptions { File = "directory", Bulk = true });
            }
        }

        /// <summary>
        ///     The file/directory to import
        /// </summary>
        [Value(0, MetaName = "file", HelpText = "File to import", Required = true)]
        public required string File { get; set; }

        /// <summary>
        ///     Import multiple files from a directory
        /// </summary>
        [Option('b', "bulk", HelpText = "Import multiple files from a directory", Required = false, Default = false)]
        public bool Bulk { get; set; }
    }
}