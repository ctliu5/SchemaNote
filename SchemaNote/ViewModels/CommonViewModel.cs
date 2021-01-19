using SchemaNote.Models;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
    public interface IProperties
    {
        string NAME_CHT { get; set; }
        string REMARK { get; set; }
    }

    public interface IConnString
    {
        [Display(Name = Common.ConnString), Required]
        string ConnectionString { get; set; }
    }
}
