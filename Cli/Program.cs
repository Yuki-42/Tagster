using System.Diagnostics;
using CommandLine;

namespace Cli;

internal class Program
{
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
        // Return the exit code
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
    }

    
    [Verb("import", HelpText = "Import files")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImportOptions
    {
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