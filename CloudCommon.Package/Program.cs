using AngryMonkey.CloudMate;
using Microsoft.Extensions.Configuration;

// Packs and publishes every CloudCommon package in one run. The version and the
// shared metadata below are read from CloudCommon.Package.csproj and written into
// each target project, so all four packages always ship as a matched set.
//
// Order matters: dependencies are packed before the projects that depend on them.
// The asset builder, demo and tests are not packages and are deliberately absent.

ConfigurationBuilder builder = new();

builder
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appconfig.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

IConfigurationRoot configuration = builder.Build();
string? apiKey = configuration["NuGetApiKey"];

await new CloudPack(new CloudPackConfig() { NugetApiKey = apiKey })
{
    MetadataProperies =
    [
        "PropertyGroup/Authors",
        "PropertyGroup/Company",
        "PropertyGroup/AssemblyVersion",
        "PropertyGroup/FileVersion",
        "PropertyGroup/PackageIcon"
    ],
    Projects =
    [
        new CloudPackProject("CloudCommon.Models"),
        new CloudPackProject("CloudCommon.Geography"),
        new CloudPackProject("CloudCommon.Theming"),
        new CloudPackProject("CloudCommon.Theming.Blazor"),
    ]
}.Pack();
