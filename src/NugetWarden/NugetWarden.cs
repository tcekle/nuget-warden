using NuGet.Versioning;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace NugetWarden;

public class NugetWardenRunner
{
    private List<BlockedPackage>? _blockedPackages;
    
    public async Task Run(string configPath, string projectDir)
    {
        _blockedPackages = await LoadBlockedPackages(configPath);
        
        if (_blockedPackages.Count == 0)
        {
            Console.WriteLine("No blocked packages defined.");
            return;
        }
        
        var csprojFiles = Directory.GetFiles(projectDir, "*.csproj", SearchOption.AllDirectories);
        bool blockedFound = false;

        foreach (var csproj in csprojFiles)
        {
            blockedFound |= await ScanProject(csproj);
        }
        
        if (blockedFound)
        {
            await Console.Error.WriteLineAsync("🚫 One or more blocked direct dependencies found. Build failed.");
            Environment.Exit(1);
        }
        else
        {
            Console.WriteLine("✅ No blocked direct dependencies detected.");
        }
    }

    private async Task<bool> ScanProject(string project)
    {
        Console.WriteLine($"🔍 Scanning {project}...");
        bool blockedFound = false;
        
        var doc = XDocument.Load(project);
        var packageRefs = doc.Descendants("PackageReference")
            .Select(e => new
            {
                Id = e.Attribute("Include")?.Value,
                Version = e.Attribute("Version")?.Value ?? e.Element("Version")?.Value
            })
            .Where(p => !string.IsNullOrEmpty(p.Id) && !string.IsNullOrEmpty(p.Version));
            
        foreach (var pkg in packageRefs)
        {
            foreach (var blocked in _blockedPackages ?? [])
            {
                if (!string.Equals(pkg.Id, blocked.Id, StringComparison.OrdinalIgnoreCase)) continue;

                var version = NuGetVersion.Parse(pkg.Version);
                var range = VersionRange.Parse(blocked.Version);

                if (range.Satisfies(version))
                {
                    await Console.Error.WriteLineAsync($"❌ Blocked package: {pkg.Id} {version} in '{Path.GetFileName(project)}' (matches '{blocked.Version}')");
                    blockedFound = true;
                }
            }
        }

        return blockedFound;
    }
    
    private static async Task<List<BlockedPackage>> LoadBlockedPackages(string path)
    {
        if (!File.Exists(path)) return new();

        var yaml = await File.ReadAllTextAsync(path);
        var deserializer = new DeserializerBuilder().Build();
        var config = deserializer.Deserialize<BlockedPackagesList>(yaml);

        return config.Packages;
    }
}