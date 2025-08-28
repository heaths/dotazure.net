// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.IO;

namespace DotAzure;

internal interface IFileSystem
{
    public string CurrentDirectory { get; }
    public bool DirectoryExists(string path);
    public bool FileExists(string path);
    public string? GetDirectoryParent(string path);
    public Stream OpenFile(string path, FileMode mode);
}

internal class FileSystem : IFileSystem
{
    public string CurrentDirectory => Environment.CurrentDirectory;
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public bool FileExists(string path) => File.Exists(path);
    public string? GetDirectoryParent(string path) => Directory.GetParent(path)?.FullName;
    public Stream OpenFile(string path, FileMode mode) => File.Open(path, mode);

}
