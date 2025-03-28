
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
      if(release == null) release = await _githubClient.Repository.Release.GetLatest(owner, repo);

      var baseTag = release.TagName;

      var repoInfo = await _githubClient.Repository.Get(owner, repo);
      var defaultBranch = repoInfo.DefaultBranch; 

      var compare = await _githubClient.Repository.Commit.Compare(owner, repo, baseTag, defaultBranch);
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
