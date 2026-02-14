using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Janet.Tools;

public static class SearchTools
{
    [Description("Search for text in files")]
    public static string Grep(string pattern, string? path = null)
    {
        var searchPath = path ?? ".";
        var results = new List<string>();
        
        foreach (var file in Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories))
        {
            if (ShouldSkip(file))
                continue;
            
            try
            {
                var content = File.ReadAllText(file);
                var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
                
                if (matches.Count > 0)
                {
                    results.Add($"{file}: {matches.Count} match(es)");
                    foreach (var match in matches.Take(3))
                    {
                        var line = GetLine(content, match.Index);
                        results.Add($"  Line {line}: ...{Truncate(match.Value, 50)}...");
                    }
                }
            }
            catch { }
        }
        
        return results.Count > 0 
            ? string.Join("\n", results) 
            : "No matches found.";
    }

    [Description("Get directory structure")]
    public static string Tree(string? path = null, int maxDepth = 3)
    {
        return TreeWalk(path ?? ".", 0, maxDepth);
    }

    private static string TreeWalk(string path, int depth, int maxDepth)
    {
        if (depth > maxDepth)
            return "";
        
        var indent = new string(' ', depth * 2);
        var lines = new List<string>();
        
        foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d))
        {
            var name = Path.GetFileName(dir);
            if (name.StartsWith('.') || name == "node_modules" || name == "bin" || name == "obj")
                continue;
            
            lines.Add($"{indent}{Path.DirectorySeparatorChar}{name}");
            lines.Add(TreeWalk(dir, depth + 1, maxDepth));
        }
        
        foreach (var file in Directory.GetFiles(path).OrderBy(f => f))
        {
            var name = Path.GetFileName(file);
            if (name.StartsWith('.'))
                continue;
            
            lines.Add($"{indent}{Path.GetFileName(file)}");
        }
        
        return string.Join("\n", lines.Where(l => !string.IsNullOrEmpty(l)));
    }

    private static bool ShouldSkip(string path) =>
        path.Contains(".git") || 
        path.Contains("node_modules") ||
        path.Contains("bin") ||
        path.Contains("obj");

    private static int GetLine(string content, int index)
    {
        return content.Substring(0, index).Count(c => c == '\n') + 1;
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s.Substring(0, max) + "...";
}
