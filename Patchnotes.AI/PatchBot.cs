using Domain.Helpers;
using Domain.Models;
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

      while (true)
      {
        try
        {
          Console.WriteLine("Do you want to generate patch notes? (y/n to exit): ");
          var continueOption = Console.ReadLine()?.Trim();
          if (continueOption?.ToLower() == "n" || continueOption?.ToLower() == "no")
          {
            Console.WriteLine("Exiting...");
            break;
          }
          await GenerateDialogAndRun();
        }
        catch (Exception ex)
        {
          Console.WriteLine($"An error occurred: {ex.Message}");
          Console.WriteLine("Please try again.");
        }
      }
    }

    async Task GenerateDialogAndRun()
    {
      var owner = PromptInput("owner");
      var repo = PromptInput("repository name");

      // Now it's input
      try
      {
        var patchData = await _githubService.GeneratePatchData(owner, repo);
        if (patchData.DiffFiles.Count() > 50)
        {
          var patchNoteChunks = new List<string>();
          foreach (var chunk in patchData.DiffFiles.Chunk(50))
          {
            var chunkedBundle = PatchDataSplitHelper.CloneWithDiffFiles(patchData, chunk);
            var patchNotes = await _openAIService.GeneratePatchNotesAsync(chunkedBundle);
            patchNoteChunks.Add(patchNotes);
          }

          var finalNotes = await _openAIService.GeneratePatchNotesFromCombined(patchNoteChunks);
        }
        else
        {
          var patchNotes = await _openAIService.GeneratePatchNotesAsync(patchData);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error generating patch data or patch notes: {ex.Message}");
        Console.WriteLine("Please try again.");
      }
    }

    string PromptInput(string label)
    {
      Console.WriteLine($"Please input {label}:");
      var input = Console.ReadLine()?.Trim();
      while (string.IsNullOrWhiteSpace(input))
      {
        Console.WriteLine($"{label} cannot be empty. Please input {label}:");
        input = Console.ReadLine()?.Trim();
      }
      return input;
    }
  }
}
