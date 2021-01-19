using System.ComponentModel.DataAnnotations;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Column
    {
        [Display(Name = "物件識別碼")]
        public int OBJECT_ID { get; set; }

        [Display(Name = "欄位序碼")]
        public int COLUMN_ID { get; set; }

        [Display(Name = "欄位名稱")]
        public string NAME { get; set; }

        [Display(Name = "資料型別")]
        public string TYPE_NAME { get; set; }

        [Display(Name = "資料長度")]
        public string LENGTH { get; set; }

        [Display(Name = "主鍵")]
        public bool IS_PK { get; set; }

        [Display(Name = "不為Null")]
        public bool DISALLOW_NULL { get; set; }

        [Display(Name = "預設值")]
        public string DEFUALT { get; set; }
    }
}
