using System.ComponentModel.DataAnnotations;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Table
    {
        [Display(Name = "物件識別碼")]
        public int OBJECT_ID { get; set; }

        [Display(Name = "資料表名")]
        public string NAME { get; set; }

        [Display(Name = "結構描述名稱")]
        public string SCHEMA_NAME { get; set; }

        [Display(Name = "物件類型")]
        public string TYPE { get; set; }

        [Display(Name = "物件創建日期")]
        public string CREATE_DATE { get; set; }

        [Display(Name = "物件修改日期")]
        public string MODIFY_DATE { get; set; }

        [Display(Name = "參考筆數")]
        public long QTY { get; set; }
    }
}
