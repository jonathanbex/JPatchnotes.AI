using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Enums
{
  public static class Enums
  {
    public enum PatchNotePromptType
    {
      DeveloperFriendlyPrompt = 0,
      UserFriendlyPrompt = 1,
    }
    public enum ReleaseType
    {
      FromRelease = 0,
      LatestHead = 1,
    }

  }
}
