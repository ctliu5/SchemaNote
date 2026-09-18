using SchemaNote.Constants;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Column
    {
        [Display(Name = Common.OBJ_COL_Id)]
        public int OBJECT_ID { get; set; }

        [Display(Name = Common.COL_SeqId)]
        public int COLUMN_ID { get; set; }

        [Display(Name = Common.COL_Name)]
        public string? NAME { get; set; }

        [Display(Name = Common.COL_Type)]
        public string? TYPE_NAME { get; set; }

        [Display(Name = "資料長度")]
        public string? LENGTH { get; set; }

        [Display(Name = Common.P_Key)]
        public bool IS_PK { get; set; }

        [Display(Name = Common.COL_NotNull)]
        public bool DISALLOW_NULL { get; set; }

        [Display(Name = Common.COL_DefaultVal)]
        public string? DEFUALT { get; set; }

        public bool IS_COMPUTED { get; set; }

        [Display(Name = Common.COL_IsPersisted)]
        public bool IS_PERSISTED { get; set; }

        [Display(Name = "計算欄位公式")]
        public string? COMPUTED_DEFINITION { get; set; }
    }
}
