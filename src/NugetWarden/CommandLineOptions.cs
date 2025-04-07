using System.CommandLine;

namespace NugetWarden;

public class CommandLineOptions
{
    public RootCommand BuildRootCommand()
    {
        var configOption = new Option<string>(
            name: "--config",
            description: "Path to the blocked-packages.yaml file",
            getDefaultValue: () => "blocked-packages.yaml");

        var projectDirOption = new Option<string>(
            name: "--project-dir",
            description: "Directory to scan for .csproj files",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        var rootCommand = new RootCommand("nuget-warden - scans for blocked NuGet packages in project files");
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(projectDirOption);

        rootCommand.SetHandler(Run, configOption, projectDirOption);

        return rootCommand;
    }

    private static async Task Run(string configPath, string projectDir)
    {
        await new NugetWardenRunner().Run(configPath, projectDir);
    }
}