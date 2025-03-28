namespace Domain.Models
{
  public class ReleasePatchNoteBundle
  {
    public string BaseTag { get; set; } = "";
    public string HeadTag { get; set; } = "";
    public List<PullRequestData> PullRequests { get; set; } = new();
    public List<FileChangeSummary> DiffFiles { get; set; } = new();
  }
}
