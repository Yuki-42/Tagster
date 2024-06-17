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
                (BuildOptions opts) => program.BuildNew(opts),
                (RebuildOptions opts) => program.BuildExisting(opts),
                (ImportOptions opts) => program.ImportFiles(opts),
                errs => 1
            );
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

    private int BuildNew(BuildOptions opts)
    {
        // Create the management system
        FileManager fileManager = new(Environment.CurrentDirectory, 2);
        
        return 0;
    }

    /// <summary>
    /// Build a management system from existing sources in the current directory.
    /// </summary>
    /// <returns>Return status of subsystem.</returns>
    private int BuildExisting(RebuildOptions ops)
    {
        // Create the management system
        FileManager fileManager = new(Environment.CurrentDirectory, 3);

        return 0;
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
        
    }
    
    [Verb("rebuild", false, [], HelpText = "Rebuild a management system in the current directory")]
    private class RebuildOptions
    {
        
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