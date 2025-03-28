using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
  public class AuthorCodeHistory
  {
    public string Name { get; set; } = "";
    public int FilesChanged { get; set; } = 0;
    public int Additions { get; set; } = 0;
    public int Deletions { get; set; } = 0;
  }
}
