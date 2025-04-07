using System.CommandLine;

namespace NugetWarden;

/// <summary>
/// Represents the command-line options and root command for the application.
/// </summary>
public static class CommandLineOptions
{
    /// <summary>
    /// Builds and configures the root command for the application.
    /// </summary>
    /// <returns>The configured <see cref="RootCommand"/>.</returns>
    public static RootCommand BuildRootCommand()
    {
        var configOption = new Option<string>(
            name: "--config",
            description: "Path to the blocked-packages.yaml file",
            getDefaultValue: () => "blocked-packages.yaml");

        var projectDirOption = new Option<string>(
            name: "--project-dir",
            description: "Directory to scan for .csproj files",
            getDefaultValue: () => Directory.GetCurrentDirectory());
        
        var modeOption = new Option<string>(
            name: "--mode",
            description: "Scan mode: 'direct' for .csproj PackageReference, 'central' for Directory.Packages.props",
            getDefaultValue: () => "direct"
        ).FromAmong("direct", "central");

        var rootCommand = new RootCommand("nuget-warden - scans for blocked NuGet packages in project files");
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(projectDirOption);
        rootCommand.AddOption(modeOption);

        rootCommand.SetHandler(Run, configOption, projectDirOption, modeOption);

        return rootCommand;
    }

    /// <summary>
    /// Executes the application logic with the provided command-line arguments.
    /// </summary>
    /// <param name="configPath">The path to the configuration file.</param>
    /// <param name="projectDir">The directory to scan for project files.</param>
    /// <param name="mode">The scan mode to use.</param>
    private static async Task Run(string configPath, string projectDir, string mode)
    {
        await new NugetWardenRunner().Run(configPath, projectDir, mode);
    }
}