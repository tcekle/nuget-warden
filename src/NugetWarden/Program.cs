using NugetWarden;
using System.CommandLine;

CommandLineOptions commandLineOptions = new CommandLineOptions();
await commandLineOptions.BuildRootCommand().InvokeAsync(args);
