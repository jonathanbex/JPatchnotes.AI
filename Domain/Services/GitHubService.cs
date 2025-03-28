
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
  public class GitHubService
  {
    IConfiguration _configuration;
    GitHubClient _githubClient;
    public GitHubService(IConfiguration configuration)
    {
      _configuration = configuration;
      var githubToken = _configuration.GetValue<string>("Github:Token");
      if (string.IsNullOrWhiteSpace(githubToken)) throw new InvalidOperationException("Missign Github Token");

      _githubClient = new GitHubClient(new ProductHeaderValue("JPatchnotes.AI"))
      {
        Credentials = new Credentials(githubToken)
      };
    }

    public async Task<ReleasePatchNoteBundle> GeneratePatchData(string owner, string repo, string? releaseId = null)
    {
      Release? release = null;
      if (!string.IsNullOrWhiteSpace(releaseId))
      {
        if (Int64.TryParse(releaseId, out long parsedReleaseId)) release = await _githubClient.Repository.Release.Get(owner, repo, parsedReleaseId);
      }
      if (release == null)
      {
        try
        {
          release = await _githubClient.Repository.Release.GetLatest(owner, repo);
        }
        catch (Exception)
        {
          //repo desont have any releases 
          release = null;
        }
      }
      var repoInfo = await _githubClient.Repository.Get(owner, repo);

      var defaultBranch = repoInfo.DefaultBranch;

      var headTag = release?.TagName;


      var previousReleases = await _githubClient.Repository.Release.GetAll(owner, repo);
      var previousRelease = previousReleases
         .Where(r => !r.TagName.Equals(headTag, StringComparison.OrdinalIgnoreCase))
         .OrderByDescending(r => r.PublishedAt)
         .FirstOrDefault();

      //previous release if it exists
      bool dataFromFirstCommit = false;

      var baseTag = previousRelease?.TagName;
      if (string.IsNullOrEmpty(baseTag))
      {
        //if it doesnt exist fetch first commit
        var commits = await _githubClient.Repository.Commit.GetAll(owner, repo, new CommitRequest { Sha = defaultBranch });
        var firstCommit = commits.Last();  // The first commit is at the end of the list
        baseTag = firstCommit.Sha;
        dataFromFirstCommit = true;
      }


      var compare = await _githubClient.Repository.Commit.Compare(owner, repo, baseTag, headTag);
      var commitShas = compare.Commits.Select(c => c.Sha).ToHashSet();

      var allPrs = await _githubClient.Repository.PullRequest.GetAllForRepository(owner, repo,
          new PullRequestRequest { State = ItemStateFilter.Closed });

      var prList = new List<PullRequestData>();

      foreach (var pr in allPrs)
      {
        if (pr.Merged == true &&
            !string.IsNullOrWhiteSpace(pr.MergeCommitSha) &&
            commitShas.Contains(pr.MergeCommitSha))
        {
          var prData = new PullRequestData
          {
            Number = pr.Number,
            Title = pr.Title,
            Body = pr.Body
          };

          // Get PR comments
          var comments = await _githubClient.Issue.Comment.GetAllForIssue(owner, repo, pr.Number);
          foreach (var comment in comments)
          {
            prData.Comments.Add(comment.Body ?? "");
          }

          // Get file changes in PR
          var files = await _githubClient.PullRequest.Files(owner, repo, pr.Number);
          foreach (var file in files)
          {
            prData.Files.Add(new FileChangeSummary
            {
              FileName = file.FileName,
              Status = file.Status,
              Additions = file.Additions,
              Deletions = file.Deletions,
              Patch = file.Patch
            });
          }

          prList.Add(prData);
        }
      }

      // Get all file diffs between baseTag and main (raw code changes)
      var diffFiles = compare.Files
          .Where(f => !string.IsNullOrWhiteSpace(f.Patch))
          .Select(f => new FileChangeSummary
          {
            FileName = f.Filename,
            Status = f.Status,
            Additions = f.Additions,
            Deletions = f.Deletions,
            Patch = f.Patch
          })
          .ToList();

      if (dataFromFirstCommit)
      {
        var firstCommitPatch = await _githubClient.Repository.Commit.Get(owner, repo, baseTag);  // Fetch the patch for the first commit
        var firstCommitFiles = firstCommitPatch.Files.Select(f => new FileChangeSummary
        {
          FileName = f.Filename,
          Status = "Added", 
          Additions = f.Additions,
          Deletions = f.Deletions,
          Patch = f.Patch
        }).ToList();

     
        diffFiles.InsertRange(0, firstCommitFiles); 
      }

      return new ReleasePatchNoteBundle
      {
        BaseTag = baseTag,
        HeadTag = defaultBranch,
        PullRequests = prList,
        DiffFiles = diffFiles
      };
    }

  }

}
