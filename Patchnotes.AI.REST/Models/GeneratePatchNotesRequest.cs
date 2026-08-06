using static Domain.Models.Enums.Enums;

namespace Patchnotes.AI.REST.Models
{
  public class GeneratePatchNotesRequest
  {
    public string Owner { get; set; }
    public string Repo { get; set; }
    public ReleaseType ReleaseType { get; set; } = ReleaseType.FromRelease;

  }
}
