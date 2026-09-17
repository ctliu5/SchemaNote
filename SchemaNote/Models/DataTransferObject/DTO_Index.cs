using System.ComponentModel.DataAnnotations;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Index
    {
        [Display(Name = Common.OBJ_COL_Id)]
        public int OBJECT_ID { get; set; }

        [Display(Name = "索引序碼")]
        public int INDEX_ID { get; set; }

        [Display(Name = "索引名稱")]
        public string? NAME { get; set; }

        [Display(Name = Common.COL_SeqId)]
        public int COLUMN_ID { get; set; }

        [Display(Name = Common.COL_IndexType)]
        public IndexType TYPE { get; set; }

        [Display(Name = "類型描述")]
        public string? TYPE_DESC { get; set; }

        [Display(Name = "唯一")]
        public bool IS_UNIQUE { get; set; }

        [Display(Name = Common.P_Key)]
        public bool IS_PK { get; set; }

        [Display(Name = "填滿因子")]
        public byte FILL_FACTOR { get; set; }

    }
}
