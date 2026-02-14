using System.ComponentModel;
using System.Diagnostics;

namespace Janet.Tools;

public static class ProcessTools
{
    [Description("Run a shell command and return its output")]
    public static string RunCommand(string command, string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/bash",
            Arguments = OperatingSystem.IsWindows() ? $"/c {command}" : $"-c \"{command}\"",
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
            throw new InvalidOperationException("Failed to start process");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        
        process.WaitForExit(30000);

        return string.IsNullOrEmpty(error) 
            ? output 
            : $"STDOUT:\n{output}\n\nSTDERR:\n{error}";
    }

    [Description("Check if a command exists")]
    public static bool CommandExists(string command)
    {
        var checkCmd = OperatingSystem.IsWindows() 
            ? $"where {command}" 
            : $"which {command}";
        
        try
        {
            return RunCommand(checkCmd).Trim().Length > 0;
        }
        catch
        {
            return false;
        }
    }
}
