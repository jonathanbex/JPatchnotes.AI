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

Be professional, but feel free to include a touch of humor or light sarcasm if the situation calls for it, memes are also good. Think like a friendly developer writing patchnotes for other developers.

If nothing major changed, say so — but you can do it with a wink.

Use markdown formatting with sections like:

- Features
- Improvements
- Fixes
- Internal
- Other
- Areas to Watch

Do **not** make things up. Base everything on the actual content provided.

For the section Potential Bugs list changes that can potentially cause bugs. I.e wrong percentage calculations etc.

Keep it fun but informative, use smileys on every section

In the end make a summary using the author data using FilesChanged,Additions and Deletions.Stack them like this and remove + and -
Author (some quick summary of them)
Files changed : Number
Additions : number
Deletions : number
Make a lil fun and harmless description about every author.

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


    public async Task<string> GeneratePatchNotesFromCombined(List<string> patchNotes, CancellationToken cancellationToken = default)
    {
      var options = new ChatCompletionOptions
      {
        EndUserId = "release-bot"
      };
      var instructionHeader = """
You are a patchnote summarizer. Generate a final, clean, markdown-formatted summary from several partial patchnotes.

Preserve their tone and style (humorous, friendly, memes if included), but remove duplicates, group similar items, and make it feel like one consistent release note.

Use these sections:
- Features 😊
- Improvements 🔧
- Fixes 🐛
- Internal 🏗️
- Other 🤷
- Areas to Watch 🕵️

Dont combine Author changes just pick one from a Summary

At the end, include a fun summary of contributors if provided in the original texts. 
""";

      var sbPrompt = new StringBuilder();
      for (int i = 0; i < patchNotes.Count; i++)
      {
        sbPrompt.AppendLine($"--- Summary Index: {i + 1} ---");
        sbPrompt.AppendLine(patchNotes[i]);
        sbPrompt.AppendLine();
      }

      var userMessage = UserChatMessage.CreateUserMessage(new[]
      {
    ChatMessageContentPart.CreateTextPart(instructionHeader + "\n\n" + sbPrompt.ToString())
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
      sb.AppendLine($"Project `{bundle.RepoName}`, - Project description : {bundle.RepoDescription}`.\n");
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
          sb.AppendLine(Truncate(file.Patch, 1000));
          sb.AppendLine("```");
        }
      }
      sb.AppendLine("\n### Authors:");

      foreach (var author in bundle.AuthorCodeHistories.OrderByDescending(x => x.Additions - x.Deletions))
      {
        sb.AppendLine($"#### {author.Name} (Files changed : {author.FilesChanged}, {author.Additions}+ / {author.Deletions}-)");
        
      }
      return sb.ToString();
    }


    private string Sanitize(string input) =>
        input.Replace("\r", "").Replace("\n", " ").Trim();

    private string Truncate(string input, int maxLength) =>
        input.Length <= maxLength ? input : input[..maxLength] + "\n// (truncated)";
  }
}
