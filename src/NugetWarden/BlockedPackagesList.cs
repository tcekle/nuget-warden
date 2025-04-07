namespace NugetWarden;

/// <summary>
/// Represents a list of blocked packages.
/// </summary>
public class BlockedPackagesList
{
    /// <summary>
    /// Gets or sets the collection of blocked packages.
    /// </summary>
    public List<BlockedPackage> Packages { get; set; } = new List<BlockedPackage>();
}