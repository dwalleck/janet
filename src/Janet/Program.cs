using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;
using Janet.Agent;
using Janet.Configuration;
using Janet.Tools;
using Spectre.Console;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

services.AddSingleton<Config>(sp =>
{
    var config = new Config();

    var modelArg = args.FirstOrDefault(a => a.StartsWith("--model="));
    if (modelArg != null)
        config.Model = modelArg.Substring("--model=".Length);

    var urlArg = args.FirstOrDefault(a => a.StartsWith("--url="));
    if (urlArg != null)
        config.BaseUrl = urlArg.Substring("--url=".Length);

    return config;
});

services.AddSingleton<IChatClient>(sp =>
{
    var config = sp.GetRequiredService<Config>();

    var openAiClient = new OpenAIClient(
        new ApiKeyCredential("no-key"),
        new OpenAIClientOptions { Endpoint = new Uri(config.BaseUrl) });

    return openAiClient
        .GetChatClient(config.Model)
        .AsIChatClient()
        .AsBuilder()
        .UseFunctionInvocation()
        .Build();
});

services.AddTransient<Agent>();

var provider = services.BuildServiceProvider();

var config = provider.GetRequiredService<Config>();
var agent = provider.GetRequiredService<Agent>();

AnsiConsole.MarkupLine("[bold cyan]Janet[/] - AI Coding Assistant");
AnsiConsole.MarkupLine($"[dim]Connected to {config.BaseUrl} with model {config.Model}[/]\n");

var tools = new List<AITool>
{
    AIFunctionFactory.Create(FileTools.ReadFile),
    AIFunctionFactory.Create(FileTools.WriteFile),
    AIFunctionFactory.Create(FileTools.FileExists),
    AIFunctionFactory.Create(FileTools.Glob),
    AIFunctionFactory.Create(ProcessTools.RunCommand),
    AIFunctionFactory.Create(ProcessTools.CommandExists),
    AIFunctionFactory.Create(SearchTools.Grep),
    AIFunctionFactory.Create(SearchTools.Tree),
};

AnsiConsole.MarkupLine("[dim]Available tools:[/] read_file, write_file, file_exists, glob, run_command, command_exists, grep, tree\n");

while (true)
{
    var userInput = AnsiConsole.Ask<string>("[bold green]>?[/] ");

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
        break;
    }

    if (userInput.Equals("clear", StringComparison.OrdinalIgnoreCase))
    {
        agent.ClearHistory();
        AnsiConsole.MarkupLine("[dim]Conversation cleared.[/]\n");
        continue;
    }

    AnsiConsole.MarkupLine("[dim]Thinking...[/]");

    try
    {
        var response = await agent.RunAsync(userInput, tools);
        AnsiConsole.MarkupLine($"\n[white]{Markup.Escape(response)}[/]\n");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"\n[red]Error: {Markup.Escape(ex.Message)}[/]\n");
    }
}
