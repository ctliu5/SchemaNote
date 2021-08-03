using SchemaNote.Models.myAttribute;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.Models.DataTransferObject
{
  public class DTO_Table
  {
    [Display(Name = "物件識別碼")]
    [Excel(1)]
    public int OBJECT_ID { get; set; }


    [Display(Name = "資料表名")]
    [Excel(2)]
    public string NAME { get; set; }

    [Display(Name = "結構描述名稱")]
    [Excel(3)]
    public string SCHEMA_NAME { get; set; }

    [Display(Name = "物件類型")]
    [Excel(4)]
    public string TYPE { get; set; }

    [Display(Name = "物件創建日期")]
    [Excel(5)]
    public string CREATE_DATE { get; set; }

    [Display(Name = "物件修改日期")]
    [Excel(6)]
    public string MODIFY_DATE { get; set; }

    [Display(Name = "筆數")]
    [Excel(7)]
    public long QTY { get; set; }
  }
}
