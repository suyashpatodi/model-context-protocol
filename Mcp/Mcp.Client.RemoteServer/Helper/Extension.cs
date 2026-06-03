using Mcp.Client.RemoteServer.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

namespace Mcp.Client.RemoteServer.Helper
{
    public static class Extension
    {
        public static async Task<IServiceCollection> AddSemanticKernelDependency(this IServiceCollection services, IConfiguration configuration)
        {
            var kernelBuilder = services.AddKernel();

            var apiKey = configuration.GetValue<string>("github:apiKey") ?? string.Empty;
            var model = configuration.GetValue<string>("github:model") ?? string.Empty;
            var endpoint = configuration.GetValue<string>("github:endpoint") ?? string.Empty;

            var mcpClient = await CreateMcpClient(apiKey);

            services.AddSingleton<McpClient>(mcpClient);
            kernelBuilder.Plugins.AddFromType<McpResource>();

            await AddRemoteServerTools(kernelBuilder, mcpClient);

            kernelBuilder.AddOpenAIChatCompletion(modelId: model, apiKey: apiKey, httpClient: new HttpClient { BaseAddress = new Uri(endpoint) });

            FunctionChoiceBehaviorOptions options = new() { AllowConcurrentInvocation = true };

            services.AddTransient<PromptExecutionSettings>(_ => new OpenAIPromptExecutionSettings()
            {
                Temperature = 0.9f,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: options)
            });

            return services;
        }

        private static async Task<McpClient> CreateMcpClient(string apiKey)
        {
            var options = new HttpClientTransportOptions()
            {
                Name = "Github",
                Endpoint = new Uri("https://api.githubcopilot.com/mcp/"),
                AdditionalHeaders = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {apiKey}"
                }
            };

            var mcpClient = await McpClient.CreateAsync(new HttpClientTransport(options));
            return mcpClient;
        }

        private static async Task AddRemoteServerTools(IKernelBuilder kernelBuilder, McpClient mcpClient)
        {
            IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

            var allowedTools = new[] { "issue_write", "list_issues" };
            var filteredTools = tools
                .Where(t => allowedTools.Contains(t.Name))
                .Select(t => t.AsKernelFunction());

            kernelBuilder.Plugins.AddFromFunctions("GS", filteredTools);
        }

        public record McpResourceMenu(string MenuText);
    }
}
