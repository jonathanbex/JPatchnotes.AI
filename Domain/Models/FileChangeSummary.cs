namespace Domain.Models
{
  public class FileChangeSummary
  {
    public string FileName { get; set; } = "";
    public string Status { get; set; } = "";
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public string? Patch { get; set; }
  }
}
