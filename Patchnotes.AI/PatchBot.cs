using Domain.Services;
using Patchnotes.AI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patchnotes.AI
{
  public class PatchBot
  {
    GitHubService _githubService;
    OpenAIService _openAIService;
    public PatchBot(GitHubService githubService, OpenAIService openAIService)
    {
      _githubService = githubService;
      _openAIService = openAIService;
    }
    public async Task RunAsync()
    {
      Console.WriteLine("Welcome to J Patch Generator");
      Console.WriteLine("**************");
      Console.WriteLine(@"
      ╔══════════════════════════════════════════════════════╗
      ║           ▄███████████▄                              ║
      ║         ▄██▀         ▀▀██▄                           ║
      ║       ▄██   ▄████████▄  ██▄       IN THE GRIM        ║
      ║      ███   ██▀      ▀██  ███     DARKNESS OF CODE...║
      ║     ███   ███      ▄███  ███                         ║
      ║     ███   ▀██▄    ▄██▀   ███     THERE IS ONLY PATCH ║
      ║     ▀███▄   ▀▀████▀▀   ▄███▀                         ║
      ║       ▀███▄▄        ▄▄███▀                           ║
      ║          ▀▀██████████▀▀      SERVITOR PROTOCOL: ON  ║
      ╚══════════════════════════════════════════════════════╝
");

      var owner = PromptInput("owner");
      var repo = PromptInput("repository name");
      //now its input
      var patchData = await _githubService.GeneratePatchData(owner, repo);
      await _openAIService.GeneratePatchNotesAsync(patchData);

    }
    string PromptInput(string label)
    {
      Console.WriteLine($"Please input {label}:");
      var input = Console.ReadLine();
      while (string.IsNullOrWhiteSpace(input))
      {
        Console.WriteLine($"{label} cannot be empty. Please input {label}:");
        input = Console.ReadLine();
      }
      return input;
    }
  }
}
