using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class EchoClass
{
    [McpServerTool, Description("Echoes the message back to the client")]
    public static string Echo(string message) => $"Hello! I am messaging from echo: {message}";
}
