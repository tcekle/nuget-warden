using NugetWarden;
using System.CommandLine;

await CommandLineOptions.BuildRootCommand().InvokeAsync(args);
