using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json;

namespace Mcp.Client.RemoteServer.Plugins
{
    public class McpResource
    {
        private readonly McpClient _client;
        private readonly ConcurrentDictionary<string, (string Content, DateTime Expiry)> _resourceCached =
                new ConcurrentDictionary<string, (string, DateTime)>();

        public McpResource(McpClient client) { _client = client; }

        [KernelFunction("list-all-available-resources")]
        [Description("Lists the names and resourceUri paths of all available project documentation, rules, guidelines, and files.")]
        public async Task<string> ListAllAvailableResources()
        {
            var resources = await _client.ListResourcesAsync();
            var resourceMenu = resources.Select(r => new
            {
                ResourceName = r.Name,
                ResourceUri = r.Uri.ToString(),
                Description = r.Description ?? "No description provided."
            });

            // Return as a clean JSON string
            return JsonSerializer.Serialize(resourceMenu);
        }

        [KernelFunction("read-resource-content")]
        [Description("Reads and retrieves the actual text content inside a specific file or document.")]
        public async Task<string> ReadResourceContent(
        [Description("The unique resourceUri identifier of the file to read (discovered via list-all-available-resources).")] string resourceUri)
        {
            if (_resourceCached.TryGetValue(resourceUri, out var cached) && cached.Expiry > DateTime.UtcNow)
            {
                return cached.Content;
            }

            var result = await _client.ReadResourceAsync(new Uri(resourceUri));
            IList<AIContent> contents = result.Contents.ToAIContents();
            string contextData = contents.FirstOrDefault()?.ToString() ?? "Empty";

            _resourceCached[resourceUri] = (contextData, DateTime.UtcNow.AddMinutes(2));
            return contextData;
        }
    }
}
