# 🛡️ nuget-warden

**nuget-warden** is a cross-platform .NET CLI tool that scans your `.csproj` files for **blocked NuGet packages**, helping enforce security, licensing, and versioning policies in your builds.

It supports:
- ✅ Direct `PackageReference` detection
- ✅ NuGet-style version ranges (`>=`, `<`, `[1.0.0]`, etc.)
- ✅ YAML-based configuration
- ✅ Fast execution (no `dotnet restore` required)
- ✅ Easy integration into CI pipelines
- ✅ Supports both traditional `<PackageReference>` and centralized `<PackageVersion>` via `Directory.Packages.props`.

---

## 📦 Installation

Pack and install as a global tool:

```bash
dotnet pack -c Release
dotnet tool install --global --add-source ./bin/Release nuget-warden
```

Or reference it locally in your repo as a tool if preferred.

## 🚀 Usage

```bash
nuget-warden --help
```

```bash
Usage:
  nuget-warden [options]

Options:
  --config <path>         Path to blocked-packages.yaml file [default: blocked-packages.yaml]
  --project-dir <path>    Root directory to scan [default: current directory]
  --mode <direct|central> Scan mode: direct .csproj or central Directory.Packages.props [default: direct]
  --help                  Show help and usage information
```

### Common examples:

```bash
# Default behavior (current directory and default config)
nuget-warden

# Scan a subfolder
nuget-warden --project-dir ./src

# Use a custom config file
nuget-warden --config ./configs/security.yaml
```

## 🔧 Configuration (blocked-packages.yaml)

Define packages and allowed version ranges using NuGet-style syntax:

```yaml
packages:
  - id: "Moq"
    version: "[4.20.0,)"         # same as >= 4.20.0
  - id: "MassTransit"
    version: "[9.0.0,)"          # same as >= 9.0.0
```

You can use any valid [NuGet version range syntax](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning#version-ranges).

## 🛠 How It Works

1. Scans .csproj files in the specified directory.
2. Parses direct <PackageReference> elements.
3. Compares each package ID and version against the blocklist.
4. Fails the build with helpful messages if blocked packages are found.

> **Note: It does not scan transitive dependencies — only top-level ones declared in the project file.**

### 🔀 Scan Modes
You can control how nuget-warden scans dependencies using the --mode option:

```bash
--mode <direct|central>
```

`direct` (*default*) – scans all `.csproj` files for direct `<PackageReference>` declarations.

`central` – scans a `Directory.Packages.props` file (for projects using Central Package Management).

**Examples:**

```bash
# Scan direct references in all .csproj files
nuget-warden --mode direct

# Scan central package versions from Directory.Packages.props
nuget-warden --mode central --project-dir ./src
```

## ✅ Example Output

```bash
🔍 Scanning src/MyProject/MyProject.csproj...
❌ Blocked: Moq 4.20.1 in 'MyProject.csproj' (matches '>=4.20.0')
❌ Blocked: MassTransit 9.1.0 in 'MyProject.csproj' (matches '>=9.0.0')
🚫 One or more blocked packages found.
```

## 🧪 Recommended Usage in CI

Add to your build scripts to enforce dependency policies:

```bash
nuget-warden --config ./blocked-packages.yaml --project-dir .
dotnet build
```

## 🤝 Contributing

Pull requests are welcome! If you have suggestions for improvements, feel free to open an issue or PR.

## 🧾 License

MIT License