using System.ComponentModel;

namespace Janet.Tools;

public static class FileTools
{
    [Description("Read the contents of a file")]
    public static string ReadFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");
        
        return File.ReadAllText(path);
    }

    [Description("Write content to a file")]
    public static void WriteFile(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        
        File.WriteAllText(path, content);
    }

    [Description("Check if a file exists")]
    public static bool FileExists(string path) => File.Exists(path);

    [Description("List files in a directory matching a pattern")]
    public static string[] Glob(string pattern)
    {
        var dir = Path.GetDirectoryName(pattern);
        if (string.IsNullOrEmpty(dir))
            dir = ".";
        
        var search = Path.GetFileName(pattern);
        
        if (string.IsNullOrEmpty(search))
            search = "*";
        
        if (search.Contains('*') || search.Contains('?'))
            return Directory.GetFiles(dir, search, SearchOption.AllDirectories);
        
        return Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
            .Where(f => Path.GetFileName(f).Contains(search))
            .ToArray();
    }
}
