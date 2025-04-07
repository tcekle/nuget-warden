namespace NugetWarden;

/// <summary>
/// Represents a package that is blocked from being used.
/// </summary>
public class BlockedPackage
{
    /// <summary>
    /// Gets or sets the ID of the blocked package.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the version of the blocked package.
    /// </summary>
    public string? Version { get; set; }
}