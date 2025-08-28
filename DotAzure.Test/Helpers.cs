// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Unicode;

namespace DotAzure;

internal static class Helpers
{
    public static Stream ToStream(this ReadOnlySpan<byte> buffer) => new MemoryStream(buffer.ToArray());
}

internal class PathComparer : IEqualityComparer<string>
{
    public static IEqualityComparer<string> Default { get; } = new PathComparer();

    public bool Equals(string? x, string? y) =>
        StringComparer.Ordinal.Equals(Replace(x), Replace(y));

    public int GetHashCode(string obj) =>
        Replace(obj)!.GetHashCode();

    private static string? Replace(string? s) => s?.Replace(@"\", "/");
}
