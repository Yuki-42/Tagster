using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
using FileMgr;

namespace Cli;

internal class Program
{
    
    /// <summary>
    /// File manager instance.
    /// </summary>
    private FileManager _fileManager;
    
    public static int Main(string[] args)
    {
        // Create a new instance of the program
        Program program = new();
        
        // Process the arguments
        return Parser.Default.ParseArguments<GuiOptions, BuildOptions, ImportOptions>(args)
            .MapResult(
                (GuiOptions opts) => program.RunGui(opts),
                (BuildOptions opts) => program.Build(opts),
                (ImportOptions opts) => program.ImportFiles(opts),
                errs => 1
            );
    }
    
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

    private int RunGui(GuiOptions opts)
    {
        // Check if the GUI executable is installed
        if (!File.Exists("TagsterGui.exe"))
        {
            Console.WriteLine("TagsterGui.exe not found.");
            return 1;
        }

        // Start the GUI
        Process.Start("TagsterGui.exe");

        // Wait for the GUI to exit
        Process.GetCurrentProcess().WaitForExit();
        
        return 0;
    }

    private int Build(BuildOptions opts)
    {
        // List options
        Console.WriteLine("Options: " + opts.Mode);
        switch (opts.Mode)
        {
            // Run the appropriate build command
            case "new":
                return BuildInit();
            case "existing":
                return BuildExisting();
            default:
                Console.WriteLine("Invalid build command.");
                return 1;
        }
    }
    
    private int BuildInit()
    {
        // Return the exit code
        Console.WriteLine("Building new management system.");
        
        // Create the management system
        _fileManager.InitialiseDirectory();
        
        return 0;
    }
    
    private int BuildExisting()
    {
        // Return the exit code
        Console.WriteLine("Building management system from existing sources.");
        return 2;
    }

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
                yield return new Example("Initialize a new management system", new BuildOptions(){Mode = "new"});
                yield return new Example("Initialize a new management system from existing sources in this directory", new BuildOptions() {Mode = "existing"});
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
                yield return new Example("Import a file", new ImportOptions {File = "file.txt"});
                yield return new Example("Import multiple files from a directory", new ImportOptions {File = "directory", Bulk = true});
            }
        }
        
        /// <summary>
        /// The file/directory to import
        /// </summary>
        [Value(0, MetaName = "file", HelpText = "File to import", Required = true)]
        public required string File { get; set; }

        /// <summary>
        /// Import multiple files from a directory
        /// </summary>
        [Option('b', "bulk", HelpText = "Import multiple files from a directory", Required = false, Default = false)]
        public bool Bulk { get; set; }
    }
}