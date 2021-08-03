using SchemaNote.Models;
using SchemaNote.Models.myAttribute;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
  public interface IProperties
  {
    string MS_Description { get; set; }
    string REMARK { get; set; }
  }

  public interface IConnString
  {
    [Display(Name = Common.ConnString), Required]
    string ConnectionString { get; set; }
  }
}
