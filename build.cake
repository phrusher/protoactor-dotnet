#tool "nuget:?package=xunit.runner.console&version=2.1.0"

var target = Argument("target", "Default");
var configuration = Argument<string>("configuration", "Debug");

var solutions = GetFiles("./*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());


Setup(context => 
{
    
});

// Clean solution directories.
Task("Clean")
    .Does(() =>
{
    foreach(var path in solutionPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "./src/**/bin/" + configuration);
        CleanDirectories(path + "./src/**/obj/" + configuration);
    }
});

// Restore all NuGet packages.
Task("Restore")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Restoring {0}...", solution);
        NuGetRestore(solution);
    }
});

// Build all solutions.
Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);
        MSBuild(solution, settings =>
            settings.SetPlatformTarget(PlatformTarget.MSIL)
                //.WithProperty("TreatWarningsAsErrors","true")
                //.SetVerbosity(Verbosity.Diagnostic)
                .SetVerbosity(Verbosity.Minimal)
                .WithTarget("Build")
                .SetConfiguration(configuration));
    }
});

// Run Unit tests
Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        XUnit2("./tests/*Tests/bin/" + configuration + "/*.Tests.dll");
    });

Task("Default")
    .IsDependentOn("Run-Unit-Tests");


RunTarget(target);
