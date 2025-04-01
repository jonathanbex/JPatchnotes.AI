using Domain.Models;

namespace Domain.Helpers
{
  public static class PatchDataSplitHelper
  {
    public static ReleasePatchNoteBundle CloneWithDiffFiles(ReleasePatchNoteBundle original, IEnumerable<FileChangeSummary> newDiffFiles)
    {
      return new ReleasePatchNoteBundle
      {
        RepoName = original.RepoName,
        RepoDescription = original.RepoDescription,
        BaseTag = original.BaseTag,
        HeadTag = original.HeadTag,
        PullRequests = original.PullRequests,
        AuthorCodeHistories = original.AuthorCodeHistories,
        DiffFiles = newDiffFiles.ToList()
      };
    }
  }
}
