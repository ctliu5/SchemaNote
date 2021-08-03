using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchemaNote.Models.myAttribute
{
  public class ExcelAttribute:Attribute
  {
    public int Rank { get; set; }
    public ExcelAttribute(int _rank)
    {
      Rank = _rank;
    }
  }
}
