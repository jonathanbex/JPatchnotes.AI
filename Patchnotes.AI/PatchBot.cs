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
      var owner = await SelectOwner();
      var repo = await SelectRepository(owner);
      var patchNoteTypeEnum = PromptPatchNoteType();
      var generateFromRelease = PromptReleaseType();
      // Now it's input
      try
      {
        var patchData = await _githubService.GeneratePatchData(owner, repo, generateFromRelease);
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

    string PromptRequired(string label, string? defaultValue = null)
    {
      Console.Write(defaultValue != null ? $"{label} [{defaultValue}]: " : $"{label}: ");
      var input = Console.ReadLine()?.Trim();
      return string.IsNullOrWhiteSpace(input) ? (defaultValue ?? "") : input;
    }

    async Task<string> SelectOwner()
    {
      var listOwners = PromptRequired("List owners (y/n)", "n");
      if (listOwners.Equals("y", StringComparison.OrdinalIgnoreCase))
      {
        var owners = await _githubService.ListOwners();

        if (owners.Count > 0)
        {
          Console.WriteLine("Owners:");
          for (int i = 0; i < owners.Count; i++)
          {
            Console.WriteLine($"{i + 1}: {owners[i]}");
          }

          Console.WriteLine();
          Console.Write("Select owner by number, or press Enter to type manually: ");
          var selection = Console.ReadLine();

          if (!string.IsNullOrWhiteSpace(selection) &&
              int.TryParse(selection, out var index) &&
              index >= 1 &&
              index <= owners.Count)
          {
            var selectedOwner = owners[index - 1];
            Console.WriteLine($"Selected: {selectedOwner}");
            return selectedOwner;
          }
        }
      }

      return PromptInput("owner");
    }

    async Task<string> SelectRepository(string owner)
    {
      var listRepos = PromptRequired("List repos (y/n)", "n");
      if (listRepos.Equals("y", StringComparison.OrdinalIgnoreCase))
      {
        var repositories = await _githubService.ListRepos(owner);

        if (repositories.Count > 0)
        {
          Console.WriteLine("Repos:");
          for (int i = 0; i < repositories.Count; i++)
          {
            Console.WriteLine($"{i + 1}: {repositories[i]}");
          }

          Console.WriteLine();
          Console.Write("Select repo by number, or press Enter to type manually: ");
          var selection = Console.ReadLine();

          if (!string.IsNullOrWhiteSpace(selection) &&
              int.TryParse(selection, out var index) &&
              index >= 1 &&
              index <= repositories.Count)
          {
            var selectedRepo = repositories[index - 1];
            Console.WriteLine($"Selected: {selectedRepo}");
            return selectedRepo;
          }
        }
      }

      return PromptInput("repository name");
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
    ReleaseType PromptReleaseType()
    {
      Console.WriteLine("Select release type:");
      Console.WriteLine("1 - From Release");
      Console.WriteLine("2 - Latest Head");
      Console.Write("Enter choice (default is 1): ");

      var input = Console.ReadLine()?.Trim();

      return input switch
      {
        "2" => ReleaseType.LatestHead,
        _ => ReleaseType.FromRelease
      };
    }
  }
}
