namespace Janet.Configuration;

public class Config
{
    public string Model { get; set; } = "llama3.1";
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 4096;
    public string SystemPrompt { get; set; } = @"You are Janet, a helpful coding assistant.
You have access to tools for reading files, writing files, running commands, and searching code.
Use these tools when the user asks you to perform tasks that require them.
Always be precise and concise in your responses.";
}
