using Azure;
using Azure.AI.OpenAI;
using Domain.Helpers;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.Text;
using static Domain.Models.Enums.Enums;

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

    public async Task<PatchNoteGeneratedResult> GeneratePatchNotesAsync(ReleasePatchNoteBundle bundle, PatchNotePromptType patchNotePromptType = PatchNotePromptType.DeveloperFriendlyPrompt, CancellationToken cancellationToken = default)
    {
      var options = new ChatCompletionOptions
      {
        EndUserId = "release-bot"
      };

      var instructionHeader = GetPrompt(patchNotePromptType, false);

      var userMessage = UserChatMessage.CreateUserMessage(new[]
      {
    ChatMessageContentPart.CreateTextPart(instructionHeader + "\n\n" + BuildPrompt(bundle))
});

      var messages = new List<ChatMessage> { userMessage };

      var sb = new StringBuilder();

      var totalCostForJob = 0m;

      await foreach (var update in _chatClient.CompleteChatStreamingAsync(messages, options, cancellationToken))
      {
        var inputTokens = update.Usage?.InputTokenCount;
        var outputTokens = update.Usage?.OutputTokenCount;

        var cost = CalculateCost(inputTokens, outputTokens);
        totalCostForJob += cost;

        foreach (var part in update.ContentUpdate?.ToList() ?? Enumerable.Empty<ChatMessageContentPart>())
        {

          sb.Append(part.Text);

        }
      }

      var totalMessage = sb.ToString();
      Console.WriteLine("\n\n[Total Patch Notes]");
      Console.WriteLine(totalMessage);
      return new PatchNoteGeneratedResult {  PatchNotes = totalMessage, Cost = totalCostForJob};

    }


    public async Task<PatchNoteGeneratedResult> GeneratePatchNotesFromCombined(List<PatchNoteGeneratedResult> patchNotes, PatchNotePromptType patchNotePromptType = PatchNotePromptType.DeveloperFriendlyPrompt, CancellationToken cancellationToken = default)
    {
      var options = new ChatCompletionOptions
      {
        EndUserId = "release-bot"
      };
      var instructionHeader = GetPrompt(patchNotePromptType, true);

      var sbPrompt = new StringBuilder();
      for (int i = 0; i < patchNotes.Count; i++)
      {
        sbPrompt.AppendLine($"--- Summary Index: {i + 1} ---");
        sbPrompt.AppendLine(patchNotes[i].PatchNotes);
        sbPrompt.AppendLine();
      }

      var userMessage = UserChatMessage.CreateUserMessage(new[]
      {
    ChatMessageContentPart.CreateTextPart(instructionHeader + "\n\n" + sbPrompt.ToString())
});

      var messages = new List<ChatMessage> { userMessage };

      var sb = new StringBuilder();

      var totalCostForJob = patchNotes.Sum(x=>x.Cost);


      await foreach (var update in _chatClient.CompleteChatStreamingAsync(messages, options, cancellationToken))
      {
        var inputTokens = update.Usage?.InputTokenCount;
        var outputTokens = update.Usage?.OutputTokenCount;

        var cost = CalculateCost(inputTokens, outputTokens);
        totalCostForJob += cost;
        foreach (var part in update.ContentUpdate?.ToList() ?? Enumerable.Empty<ChatMessageContentPart>())
        {

          sb.Append(part.Text);

        }
      }

      var totalMessage = sb.ToString();
      Console.WriteLine("\n\n[Total Patch Notes]");
      Console.WriteLine(totalMessage);
      return new PatchNoteGeneratedResult { PatchNotes = totalMessage, Cost = totalCostForJob };

    }

    private decimal CalculateCost(int? inputTokens, int? outputTokens)
    {
      const decimal inputRate = 0.15m / 1000000;  // $0.15 per million input tokens
      const decimal outputRate = 0.60m / 1000000; // $0.60 per million output tokens

      var inputCost = (inputTokens ?? 0) * inputRate;
      var outputCost = (outputTokens ?? 0) * outputRate;

      return Math.Round(inputCost + outputCost, 4);
    }

    private string BuildPrompt(ReleasePatchNoteBundle bundle)
    {
      var sb = new System.Text.StringBuilder();
      sb.AppendLine($"Project `{bundle.RepoName}`, - Project description : {bundle.RepoDescription}`.\n");
      sb.AppendLine($"Release notes for version `{bundle.HeadTag}`, changes since `{bundle.BaseTag}`.\n");
      sb.AppendLine($"Release Date: {DateTime.UtcNow:yyyy-MM-dd}\n");
      sb.AppendLine("### Pull Requests:");
      foreach (var pr in bundle.PullRequests)
      {
        sb.AppendLine($"- [#{pr.Number}] {pr.Title}");
        if (!string.IsNullOrWhiteSpace(pr.Body))
        {
          sb.AppendLine($"  > {Sanitize(pr.Body)}");
        }
      }

      sb.AppendLine("### Commit Messages contained in release:");
      foreach (var commitMessage in bundle.CommitMessages)
      {
        sb.AppendLine($"{commitMessage}");
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

    private string GetPrompt(PatchNotePromptType promptType, bool summary = false)
    {
      return (promptType, summary) switch
      {
        (PatchNotePromptType.DeveloperFriendlyPrompt, false) => PromptHelper.DeveloperPrompt,
        (PatchNotePromptType.DeveloperFriendlyPrompt, true) => PromptHelper.DeveloperCombinedPrompt,
        (PatchNotePromptType.UserFriendlyPrompt, false) => PromptHelper.UserPrompt,
        (PatchNotePromptType.UserFriendlyPrompt, true) => PromptHelper.UserCombinedPrompt,
        _ => throw new NotSupportedException("Unknown patch note prompt type or mode")
      };
    }


  }
}
