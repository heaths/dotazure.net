// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotAzure;

/// <summary>
/// Project information for the Azure Development CLI.
/// </summary>
public class AzdContext
{
    internal static readonly string ConfigurationFileName = "config.json";
    internal static readonly string EnvironmentDirectoryName = ".azure";
    internal static readonly string EnvironmentFileName = ".env";
    internal static readonly string ProjectFileName = "azure.yaml";

    private readonly string projectDirectory;
    private readonly string environmentName;

    internal AzdContext(string projectDirectory, string environmentName)
    {
        this.projectDirectory = projectDirectory;
        this.environmentName = environmentName;
    }

    /// <summary>
    /// Gets the directory containing the <code>azure.yaml</code> project file.
    /// </summary>
    public string ProjectDirectory => projectDirectory;

    /// <summary>
    /// Gets the path to the <code>azure.yaml</code> project file.
    /// </summary>
    public string ProjectPath => Path.Combine(ProjectDirectory, ProjectFileName);

    /// <summary>
    /// Gets the path to the <code>.azure</code> directory.
    /// </summary>
    public string EnvironmentDirectory => Path.Combine(ProjectDirectory, EnvironmentDirectoryName);

    /// <summary>
    /// Gets the name of the environment.
    /// </summary>
    public string EnvironmentName => environmentName;

    /// <summary>
    /// Gets the path to the environment directory under <see cref="EnvironmentDirectory"/>.
    /// </summary>
    public string EnvironmentRoot => Path.Combine(EnvironmentDirectory, environmentName);

    /// <summary>
    /// Gets the path to the <code>.env</code> file under <see cref="EnvironmentRoot"/>.
    /// </summary>
    public string EnvironmentFile => Path.Combine(EnvironmentRoot, EnvironmentFileName);

    /// <summary>
    /// Creates a new <see cref="AzdContextBuilder"/> to build an <see cref="AzdContext"/>.
    /// </summary>
    /// <returns>A new <see cref="AzdContextBuilder"/>.</returns>
    public static AzdContextBuilder Builder() => new AzdContextBuilder();
}

/// <summary>
/// A builder to construct an <see cref="AzdContext"/>.
/// </summary>
public class AzdContextBuilder
{
    private readonly IFileSystem fileSystem;
    private string? currentDirectory;
    private string? environmentName;

    /// <summary>
    /// Creates a new instance of <see cref="AzdContextBuilder"/>.
    /// </summary>
    public AzdContextBuilder() : this(new FileSystem())
    {
    }

    internal AzdContextBuilder(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Sets the current directory.
    /// </summary>
    /// <param name="path">The directory to set as the current directory.</param>
    /// <returns>The <see cref="AzdContextBuilder"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> cannot be empty or white space.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null.</exception>
    /// <exception cref="DirectoryNotFoundException">The <paramref name="path"/> does not exist.</exception>
    /// <remarks>
    /// The default is <see cref="Environment.CurrentDirectory"/>.
    /// </remarks>
    public AzdContextBuilder WithCurrentDirectory(string path)
    {
        ArgumentHelpers.ThrowIfNullOrWhiteSpace(path);

        if (!fileSystem.DirectoryExists(path))
        {
            throw new DirectoryNotFoundException();
        }

        currentDirectory = path;

        return this;
    }

    /// <summary>
    /// Sets the environment name.
    /// </summary>
    /// <param name="name">The environment name to set.</param>
    /// <returns>the <see cref="AzdContextBuilder"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> cannot be empty or white space.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> cannot be null.</exception>
    /// <remarks>
    /// The default comes from the <code>AZURE_ENV_NAME</code> environment variable or from the <code>.azure/config.json</code> file.
    /// </remarks>
    public AzdContextBuilder WithEnvironmentName(string name)
    {
        ArgumentHelpers.ThrowIfNullOrWhiteSpace(name);

        environmentName = name;

        return this;
    }

    /// <summary>
    /// Builds an <see cref="AzdContext"/>.
    /// </summary>
    /// <returns>An <see cref="AzdContext"/> or <code>null</code> if an environment cannot be found.</returns>
    /// <exception cref="FileNotFoundException">A project file exists but does not define a <code>defaultEnvironment</code>.</exception>
    public AzdContext? Build()
    {
        var projectDirectory = this.currentDirectory ?? fileSystem.CurrentDirectory;
        while (projectDirectory != null)
        {
            var path = Path.Combine(projectDirectory, AzdContext.ProjectFileName);
            if (fileSystem.FileExists(path))
            {
                break;
            }

            projectDirectory = fileSystem.GetDirectoryParent(projectDirectory);
        }

        if (projectDirectory is null)
        {
            return null;
        }

        var environmentName = this.environmentName;
        if (environmentName is null)
        {
            var projectPath = Path.Combine(projectDirectory, AzdContext.EnvironmentDirectoryName, AzdContext.ConfigurationFileName);
            if (!fileSystem.FileExists(projectPath))
            {
                return null;
            }

            using var file = fileSystem.OpenFile(projectPath, FileMode.Open);
            var configuration = JsonSerializer.Deserialize<Configuration>(file);
            environmentName = configuration?.DefaultEnvironment ?? throw new FileNotFoundException($"{projectPath} does not default 'defaultEnvironment'");
        }

        return new AzdContext(projectDirectory, environmentName);
    }
}

internal class Configuration
{
    [JsonPropertyName("defaultEnvironment")]
    public string? DefaultEnvironment { get; set; }
}
