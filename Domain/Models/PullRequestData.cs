
namespace Domain.Models
{
  public class PullRequestData
  {
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public List<string> Comments { get; set; } = new();
    public List<FileChangeSummary> Files { get; set; } = new();
  }
}
