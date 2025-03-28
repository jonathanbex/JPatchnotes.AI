using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
  public class OpenAIService
  {
    IConfiguration _configuration;
    private readonly ChatClient _chatClient;

    public OpenAIService(IConfiguration configuration)
    {
      _configuration = configuration;
      var endpoint = configuration.GetValue<string>("OpenAI:Endpoint");         // e.g. https://your-resource.openai.azure.com/
      var deployment = configuration.GetValue<string>("OpenAI:DeploymentName"); 
      var apiKey = configuration.GetValue<string>("OpenAI:APIKey");

      var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

      _chatClient = azureClient.GetChatClient(deployment);
    }

    public async Task<string> GeneratePatchNotesAsync(ReleasePatchNoteBundle bundle, CancellationToken cancellationToken = default)
    {
      var options = new ChatCompletionOptions
      {
        EndUserId = "release-bot"
      };
      var instructionHeader = """
You are a patchnote summarizer. Generate markdown-formatted, categorized release notes based on pull requests and code diffs.

Be professional, but feel free to include a touch of humor or light sarcasm if the situation calls for it. Think like a friendly developer writing patchnotes for other developers.

If nothing major changed, say so — but you can do it with a wink.

Use markdown formatting with sections like:

- Features
- Improvements
- Fixes
- Internal
- Other

Do **not** make things up. Base everything on the actual content provided.

Keep it fun but informative. Avoid emojis unless absolutely necessary (and even then, be tasteful).
""";
      var userMessage = UserChatMessage.CreateUserMessage(new[]
      {
    ChatMessageContentPart.CreateTextPart(instructionHeader + "\n\n" + BuildPrompt(bundle))
});

      var messages = new List<ChatMessage> { userMessage };

      var sb = new StringBuilder();

      await foreach (var update in _chatClient.CompleteChatStreamingAsync(messages, options, cancellationToken))
      {
        foreach (var part in update.ContentUpdate?.ToList() ?? Enumerable.Empty<ChatMessageContentPart>())
        {
   
            sb.Append(part.Text);
          
        }
      }
      var totalMessage = sb.ToString();
      Console.WriteLine("\n\n[Total Patch Notes]");
      Console.WriteLine(totalMessage);
      return totalMessage;

    }

    private string BuildPrompt(ReleasePatchNoteBundle bundle)
    {
      var sb = new System.Text.StringBuilder();
      sb.AppendLine($"Release notes for version `{bundle.HeadTag}`, changes since `{bundle.BaseTag}`.\n");

      sb.AppendLine("### Pull Requests:");
      foreach (var pr in bundle.PullRequests)
      {
        sb.AppendLine($"- [#{pr.Number}] {pr.Title}");
        if (!string.IsNullOrWhiteSpace(pr.Body))
        {
          sb.AppendLine($"  > {Sanitize(pr.Body)}");
        }
      }

      sb.AppendLine("\n### Code Changes:");
      foreach (var file in bundle.DiffFiles)
      {
        sb.AppendLine($"#### {file.FileName} ({file.Status}, {file.Additions}+ / {file.Deletions}-)");
        if (!string.IsNullOrWhiteSpace(file.Patch))
        {
          sb.AppendLine("```diff");
          sb.AppendLine(Truncate(file.Patch, 500));
          sb.AppendLine("```");
        }
      }

      return sb.ToString();
    }

    private string Sanitize(string input) =>
        input.Replace("\r", "").Replace("\n", " ").Trim();

    private string Truncate(string input, int maxLength) =>
        input.Length <= maxLength ? input : input[..maxLength] + "\n// (truncated)";
  }
}
