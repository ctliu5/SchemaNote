using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Index
    {
        [Display(Name = "物件識別碼")]
        public int OBJECT_ID { get; set; }

        [Display(Name = "索引序碼")]
        public int INDEX_ID { get; set; }

        [Display(Name = "索引名稱")]
        public string NAME { get; set; }

        [Display(Name = "欄位序碼")]
        public int COLUMN_ID { get; set; }

        [Display(Name = "索引類型")]
        public byte TYPE { get; set; }

        [Display(Name = "類型描述")]
        public string TYPE_DESC { get; set; }

        [Display(Name = "唯一")]
        public bool IS_UNIQUE { get; set; }

        [Display(Name = "主鍵")]
        public bool IS_PK { get; set; }

        [Display(Name = "填滿因子")]
        public byte FILL_FACTOR { get; set; }

    }
}
