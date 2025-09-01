// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DotAzure;

[TestClass]
public sealed class AzdContextBuildsTests
{
    [TestMethod]
    public void TestWithCurrentDirectoryArgumentExceptions()
    {
        var sut = new AzdContextBuilder(Mock.Of<IFileSystem>());
        Assert.ThrowsException<ArgumentNullException>(() => sut.WithCurrentDirectory(null!));
        Assert.ThrowsException<ArgumentException>(() => sut.WithCurrentDirectory(string.Empty));
        Assert.ThrowsException<ArgumentException>(() => sut.WithCurrentDirectory(" "));
    }

    [TestMethod]
    public void TestWithCurrentDirectoryNotExists()
    {
        var fileSystem = new Mock<IFileSystem>();
        fileSystem
            .Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .Throws<DirectoryNotFoundException>(() => throw new DirectoryNotFoundException());

        var sut = new AzdContextBuilder(fileSystem.Object);
        Assert.ThrowsException<DirectoryNotFoundException>(() => sut.WithCurrentDirectory("ShouldNotExist"));
    }

    [TestMethod]
    public void TestWithEnvironmentNameArgumentExceptions()
    {
        var sut = new AzdContextBuilder(Mock.Of<IFileSystem>());
        Assert.ThrowsException<ArgumentNullException>(() => sut.WithEnvironmentName(null!));
        Assert.ThrowsException<ArgumentException>(() => sut.WithEnvironmentName(string.Empty));
        Assert.ThrowsException<ArgumentException>(() => sut.WithEnvironmentName(" "));
    }

    [TestMethod]
    public void TestBuild()
    {
        var fileSystem = new Mock<IFileSystem>();
        fileSystem.SetupGet(x => x.CurrentDirectory).Returns("~/src/project");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/project/azure.yaml", PathComparer.Default))).Returns(false);
        fileSystem.Setup(x => x.GetDirectoryParent("~/src/project")).Returns("~/src");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/azure.yaml", PathComparer.Default))).Returns(true);
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/.azure/config.json", PathComparer.Default))).Returns(true);
        fileSystem
            .Setup(x => x.OpenFile(It.Is("~/src/.azure/config.json", PathComparer.Default), FileMode.Open))
            .Returns(() => """{"defaultEnvironment":"test"}"""u8.ToStream());

        var sut = new AzdContextBuilder(fileSystem.Object).Build();
        Assert.AreEqual("~/src/.azure/test/.env", sut?.EnvironmentFile, PathComparer.Default);
    }

    [TestMethod]
    public void TestBuildOverride()
    {
        var fileSystem = new Mock<IFileSystem>();

        // Default
        fileSystem.SetupGet(x => x.CurrentDirectory).Returns("~/src/project");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/project/azure.yaml", PathComparer.Default))).Returns(false);
        fileSystem.Setup(x => x.GetDirectoryParent("~/src/project")).Returns("~/src");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/azure.yaml", PathComparer.Default))).Returns(true);
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/.azure/config.json", PathComparer.Default))).Returns(true);
        fileSystem
            .Setup(x => x.OpenFile(It.Is("~/src/.azure/config.json", PathComparer.Default), FileMode.Open))
            .Returns(() => """{"defaultEnvironment":"test"}"""u8.ToStream());

        // Override
        fileSystem.Setup(x => x.DirectoryExists(It.Is("~/src/solution/project", PathComparer.Default))).Returns(true);
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/solution/project/azure.yaml", PathComparer.Default))).Returns(false);
        fileSystem.Setup(x => x.GetDirectoryParent("~/src/solution/project")).Returns("~/src/solution");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/solution/azure.yaml", PathComparer.Default))).Returns(false);
        fileSystem.Setup(x => x.GetDirectoryParent("~/src/solution")).Returns("~/src");

        var sut = new AzdContextBuilder(fileSystem.Object)
            .WithCurrentDirectory("~/src/solution/project")
            .WithEnvironmentName("prod")
            .Build();
        Assert.AreEqual("~/src/.azure/prod/.env", sut?.EnvironmentFile, PathComparer.Default);
    }

    [TestMethod]
    public void TestBuildBadConfiguration()
    {
        var fileSystem = new Mock<IFileSystem>();
        fileSystem.SetupGet(x => x.CurrentDirectory).Returns("~/src/project");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/project/azure.yaml", PathComparer.Default))).Returns(false);
        fileSystem.Setup(x => x.GetDirectoryParent("~/src/project")).Returns("~/src");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/azure.yaml", PathComparer.Default))).Returns(true);
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/.azure/config.json", PathComparer.Default))).Returns(true);
        fileSystem
            .Setup(x => x.OpenFile(It.Is("~/src/.azure/config.json", PathComparer.Default), FileMode.Open))
            .Returns(() => """{}"""u8.ToStream());

        Assert.ThrowsException<FileNotFoundException>(() => new AzdContextBuilder(fileSystem.Object).Build());
    }

    [TestMethod]
    public void TestAzdContext()
    {
        var fileSystem = new Mock<IFileSystem>();
        fileSystem.SetupGet(x => x.CurrentDirectory).Returns("~/src/project");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/project/azure.yaml", PathComparer.Default))).Returns(false);
        fileSystem.Setup(x => x.GetDirectoryParent("~/src/project")).Returns("~/src");
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/azure.yaml", PathComparer.Default))).Returns(true);
        fileSystem.Setup(x => x.FileExists(It.Is("~/src/.azure/config.json", PathComparer.Default))).Returns(true);
        fileSystem
            .Setup(x => x.OpenFile(It.Is("~/src/.azure/config.json", PathComparer.Default), FileMode.Open))
            .Returns(() => """{"defaultEnvironment":"test"}"""u8.ToStream());

        var sut = new AzdContextBuilder(fileSystem.Object).Build();
        Assert.IsNotNull(sut);
        Assert.AreEqual("~/src", sut.ProjectDirectory.NormalizePath());
        Assert.AreEqual("~/src/azure.yaml", sut.ProjectPath.NormalizePath());
        Assert.AreEqual("~/src/.azure", sut.EnvironmentDirectory.NormalizePath());
        Assert.AreEqual("test", sut.EnvironmentName);
        Assert.AreEqual("~/src/.azure/test", sut.EnvironmentRoot);
        Assert.AreEqual("~/src/.azure/test/.env", sut.EnvironmentFile);
    }
}
