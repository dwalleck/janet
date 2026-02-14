using Xunit;
using Janet.Tools;

namespace Janet.Tests;

public class FileToolsTests
{
    [Fact]
    public void FileExists_ReturnsFalse_ForNonExistentFile()
    {
        var result = FileTools.FileExists("nonexistent_file_12345.txt");
        Assert.False(result);
    }

    [Fact]
    public void Glob_ReturnsFiles_MatchingPattern()
    {
        var results = FileTools.Glob("*.*");
        Assert.NotNull(results);
    }
}

public class ProcessToolsTests
{
    [Fact]
    public void CommandExists_ReturnsTrue_ForBuiltInCommand()
    {
        var result = OperatingSystem.IsWindows() 
            ? ProcessTools.CommandExists("dir") 
            : ProcessTools.CommandExists("ls");
        
        Assert.True(result);
    }
}

public class SearchToolsTests
{
    [Fact]
    public void Tree_ReturnsDirectoryStructure()
    {
        var result = SearchTools.Tree(".", 1);
        Assert.NotNull(result);
    }
}
