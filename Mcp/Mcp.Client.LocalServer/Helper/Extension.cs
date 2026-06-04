using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

namespace Mcp.Client.LocalServer.Helper
{
    public static class Extension
    {
        public static async Task<IServiceCollection> AddSemanticKernelDependency(this IServiceCollection services, IConfiguration configuration)
        {
            var kernelBuilder = services.AddKernel();

            await AddLocalServerTools(kernelBuilder);
            await AddCustomServerTools(kernelBuilder);

            var apiKey = configuration.GetValue<string>("github:apiKey") ?? string.Empty;
            var model = configuration.GetValue<string>("github:model") ?? string.Empty;
            var endpoint = configuration.GetValue<string>("github:endpoint") ?? string.Empty;

            kernelBuilder.AddOpenAIChatCompletion(modelId: model, apiKey: apiKey, httpClient: new HttpClient { BaseAddress = new Uri(endpoint) });

            FunctionChoiceBehaviorOptions options = new() { AllowConcurrentInvocation = true };

            services.AddTransient<PromptExecutionSettings>(_ => new OpenAIPromptExecutionSettings()
            {
                Temperature = 0.9f,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: options)
            });

            return services;
        }

        private static async Task AddLocalServerTools(IKernelBuilder kernelBuilder)
        {
            var options = new StdioClientTransportOptions()
            {
                Name = "File Sever",
                Command = "npx",
                Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "D:\\Github\\model-context-protocol\\Mcp\\Mcp.Client.LocalServer\\Data\\"]
            };

            var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(options));

            IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

            var functions = tools.Select(x => x.AsKernelFunction());
            kernelBuilder.Plugins.AddFromFunctions("FS", functions);
        }
        private static async Task AddCustomServerTools(IKernelBuilder kernelBuilder)
        {
            var options = new StdioClientTransportOptions()
            {
                Name = "Custom Server",
                Command = "dotnet",
                Arguments = ["run", "--project", "D:\\Github\\model-context-protocol\\Mcp\\Mcp.Server\\Mcp.Server.csproj"]
            };

            var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(options));

            IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

            var functions = tools.Select(x => x.AsKernelFunction());
            kernelBuilder.Plugins.AddFromFunctions("CS", functions);
        }
    }
}
