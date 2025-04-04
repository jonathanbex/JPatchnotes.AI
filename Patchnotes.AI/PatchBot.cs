using Domain.Helpers;
using Domain.Models;
using Domain.Services;
using Microsoft.Extensions.Hosting;
using Patchnotes.AI.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Models.Enums.Enums;

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
      var patchNoteTypeEnum = PromptPatchNoteType();
      // Now it's input
      try
      {
        var patchData = await _githubService.GeneratePatchData(owner, repo);
        if (patchData.DiffFiles.Count() > 50)
        {
          var patchNoteChunks = new List<PatchNoteGeneratedResult>();
          foreach (var chunk in patchData.DiffFiles.Chunk(50))
          {
            var chunkedBundle = PatchDataSplitHelper.CloneWithDiffFiles(patchData, chunk);
            var patchNotes = await _openAIService.GeneratePatchNotesAsync(chunkedBundle, patchNoteTypeEnum);
            patchNoteChunks.Add(patchNotes);
          }

          var finalNotes = await _openAIService.GeneratePatchNotesFromCombined(patchNoteChunks, patchNoteTypeEnum);
          LogCosts(finalNotes);
        }
        else
        {
          var patchNote = await _openAIService.GeneratePatchNotesAsync(patchData, patchNoteTypeEnum);
          LogCosts(patchNote);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error generating patch data or patch notes: {ex.Message}");
        Console.WriteLine("Please try again.");
      }
    }

    void LogCosts(PatchNoteGeneratedResult patchNote)
    {
      Console.WriteLine();
      Console.WriteLine("🎉 Patchnote generation complete!");
      Console.WriteLine($"🧾 Total cost: {patchNote.Cost.ToString("C3", CultureInfo.CreateSpecificCulture("en-US"))} USD");
      Console.WriteLine();
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
    PatchNotePromptType PromptPatchNoteType()
    {
      Console.WriteLine("Select patch note type:");
      Console.WriteLine("1 - Developer Friendly");
      Console.WriteLine("2 - Customer Friendly");
      Console.Write("Enter choice (default is 1): ");

      var input = Console.ReadLine()?.Trim();

      return input switch
      {
        "2" => PatchNotePromptType.UserFriendlyPrompt,
        _ => PatchNotePromptType.DeveloperFriendlyPrompt
      };
    }
  }
}
