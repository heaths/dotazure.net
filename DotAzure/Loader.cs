// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using dotenv.net;

namespace DotAzure;

/// <summary>
/// Load environment variables for an Azure Developer CLI project.
/// </summary>
public class Loader
{
    /// <summary>
    /// Load environment variables for an Azure Developer CLI project.
    /// </summary>
    /// <param name="options">Optional <see cref="LoadOptions"/> to customize loading.</param>
    /// <returns><code>true</code> if the <code>.env</code> file was found and loaded successfully; otherwise <code>false</code> if no <code>.env</code> file was found.</returns>
    /// <remarks>
    /// Locates the <code>.env</code> file from the default environment name if an <see href="https://aka.ms/azure-dev">Azure Developer CLI</see> project was already provisioned.
    /// </remarks>
    public static bool Load(LoadOptions? options = null)
    {
        options ??= new LoadOptions();
        options.Context ??= new AzdContextBuilder(options.fileSystem).Build();
        if (options.Context is null)
        {
            return false;
        }

        DotEnv.Load(new DotEnvOptions(
            envFilePaths: [options.Context.EnvironmentFile],
            overwriteExistingVars: options.Replace));

        return true;
    }
}

/// <summary>
/// Options to customize <see cref="Loader.Load"/>.
/// </summary>
public class LoadOptions
{
    internal readonly IFileSystem fileSystem;

    /// <summary>
    /// Creates a new instance of the <see cref="LoadOptions"/> class.
    /// </summary>
    public LoadOptions() : this(new FileSystem())
    {
    }

    internal LoadOptions(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    /// <summary>
    /// Gets or sets the <see cref="AzdContext"/> to use for discovery.
    /// </summary>
    public AzdContext? Context { get; set; }

    /// <summary>
    /// Gets or sets whether to replace environment variables that were already set.
    /// </summary>
    public bool Replace { get; set; }
}
