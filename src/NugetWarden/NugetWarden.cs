using NuGet.Versioning;
using System.Xml.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NugetWarden;

/// <summary>
/// Represents the main runner for the NuGet Warden application.
/// </summary>
public class NugetWardenRunner
{
    private List<BlockedPackage>? _blockedPackages;
    
    /// <summary>
    /// Executes the NuGet Warden logic with the provided configuration path, project directory, and mode.
    /// </summary>
    /// <param name="configPath">The path to the configuration file.</param>
    /// <param name="projectDir">The directory to scan for project files.</param>
    /// <param name="mode">The scan mode to use ('direct' or 'central').</param>
    public async Task Run(string configPath, string projectDir, string mode)
    {
        _blockedPackages = await LoadBlockedPackages(configPath);
        
        if (_blockedPackages.Count == 0)
        {
            Console.WriteLine("No blocked packages defined.");
            return;
        }

        bool blockedFound = mode switch
        {
            "direct" => await RunDirectMode(projectDir),
            "central" => await RunCentralMode(projectDir, _blockedPackages),
            _ => throw new ArgumentException("Invalid mode specified. Use 'direct' or 'central'."),
        };
        
        if (blockedFound)
        {
            await Console.Error.WriteLineAsync("[FAIL] One or more blocked direct dependencies found. Build failed.");
            Environment.Exit(1);
        }
        else
        {
            Console.WriteLine("[OK] No blocked direct dependencies detected.");
        }
    }
    
    /// <summary>
    /// Runs the central mode scan for blocked packages.
    /// </summary>
    /// <param name="projectDir">The directory containing the central configuration file.</param>
    /// <param name="blockedPackages">The list of blocked packages to check against.</param>
    /// <returns>True if blocked packages are found; otherwise, false.</returns>
    private async Task<bool> RunCentralMode(string projectDir, List<BlockedPackage> blockedPackages)
    {
        var propsPath = Path.Combine(projectDir, "Directory.Packages.props");
        if (!File.Exists(propsPath))
        {
            await Console.Error.WriteLineAsync("[WARN] Directory.Packages.props not found.");
            Environment.Exit(1);
        }

        var doc = XDocument.Load(propsPath);
        var packages = doc.Descendants("PackageVersion")
            .Select(e => new BlockedPackage
            {
                Id = e.Attribute("Include")?.Value,
                Version = e.Attribute("Version")?.Value
            })
            .Where(p => !string.IsNullOrEmpty(p.Id) && !string.IsNullOrEmpty(p.Version));

        return await CheckPackages(blockedPackages, packages);
    }

    /// <summary>
    /// Runs the direct mode scan for blocked packages.
    /// </summary>
    /// <param name="projectDir">The directory containing project files.</param>
    /// <returns>True if blocked packages are found; otherwise, false.</returns>
    private async Task<bool> RunDirectMode(string projectDir)
    {
        var csprojFiles = Directory.GetFiles(projectDir, "*.csproj", SearchOption.AllDirectories);
        bool blockedFound = false;

        foreach (var csproj in csprojFiles)
        {
            blockedFound |= await ScanProject(csproj);
        }

        return blockedFound;
    }

    /// <summary>
    /// Scans a single project file for blocked packages.
    /// </summary>
    /// <param name="project">The path to the project file.</param>
    /// <returns>True if blocked packages are found; otherwise, false.</returns>
    private async Task<bool> ScanProject(string project)
    {
        Console.WriteLine($"[SCAN] Scanning {project}...");
        
        var doc = XDocument.Load(project);
        var packageRefs = doc.Descendants("PackageReference")
            .Select(e => new BlockedPackage
            {
                Id = e.Attribute("Include")?.Value,
                Version = e.Attribute("Version")?.Value ?? e.Element("Version")?.Value
            })
            .Where(p => !string.IsNullOrEmpty(p.Id) && !string.IsNullOrEmpty(p.Version));

        return await CheckPackages(_blockedPackages!, packageRefs);
    }
    
    /// <summary>
    /// Loads the list of blocked packages from the specified configuration file.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <returns>A list of blocked packages.</returns>
    private static async Task<List<BlockedPackage>> LoadBlockedPackages(string path)
    {
        if (!File.Exists(path)) return new();

        var yaml = await File.ReadAllTextAsync(path);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var config = deserializer.Deserialize<BlockedPackagesList>(yaml);

        return config.Packages;
    }
    
    /// <summary>
    /// Checks the provided packages against the list of blocked packages.
    /// </summary>
    /// <param name="blockedPackages">The list of blocked packages.</param>
    /// <param name="packages">The packages to check.</param>
    /// <returns>True if blocked packages are found; otherwise, false.</returns>
    private static async Task<bool> CheckPackages(List<BlockedPackage> blockedPackages,  IEnumerable<BlockedPackage> packages)
    {
        bool blockedFound = false;
        foreach (var pkg in packages)
        {
            foreach (var blocked in blockedPackages)
            {
                if (!string.Equals(pkg.Id, blocked.Id, StringComparison.OrdinalIgnoreCase)) continue;

                var version = NuGetVersion.Parse(pkg.Version!);
                var range = VersionRange.Parse(blocked.Version!);

                if (range.Satisfies(version))
                {
                    await Console.Error.WriteLineAsync($"[BLOCKED] {pkg.Id} {version} from central config (matches '{blocked.Version}')");
                    blockedFound = true;
                }
            }
        }

        return blockedFound;
    }
}