using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Mcp.Client.LocalServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class McpClientController : ControllerBase
    {
        [HttpGet("stream")]
        public async Task<string?> GetChatAsync([FromQuery] string query,
                                                [FromServices] Kernel kernel,
                                                [FromServices] PromptExecutionSettings settings)
        {
            var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
            var response = await chatCompletion.GetChatMessageContentAsync(query, settings, kernel);
            return response.Content;
        }
    }
}