using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Mcp.Client.RemoteServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class McpRemoteController : ControllerBase
    {

        [HttpGet("stream")]
        public async Task<string?> GetChatAsync([FromQuery] string query,
                                               [FromServices] Kernel kernel,
                                               [FromServices] PromptExecutionSettings settings)
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(@"You are a helpful assistant with access to local project data. 
                                           If the user asks about project files, rules, guidelines, or internal data, use the 'list-all-available-resources' tool to discover what resources are available to you.");

            chatHistory.AddUserMessage(query);

            var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
            var response = await chatCompletion.GetChatMessageContentAsync(chatHistory, settings, kernel);
            return response.Content;
        }
    }
}
