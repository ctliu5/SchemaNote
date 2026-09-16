using System.ComponentModel.DataAnnotations;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Table
    {
        [Display(Name = "物件識別碼")]
        public int OBJECT_ID { get; set; }

        [Display(Name = Common.ObjName)]
        public string? NAME { get; set; }

        [Display(Name = Common.SchemaName)]
        public string? SCHEMA_NAME { get; set; }

        [Display(Name = Common.ObjType)]
        public string? TYPE { get; set; }

        [Display(Name = Common.ObjCreateDate)]
        public string? CREATE_DATE { get; set; }

        [Display(Name = Common.ObjModifyDate)]
        public string? MODIFY_DATE { get; set; }

        [Display(Name = "筆數")]
        public long QTY { get; set; }
    }
}
