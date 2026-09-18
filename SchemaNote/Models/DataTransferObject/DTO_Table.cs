using SchemaNote.Constants;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Table
    {
        [Display(Name = Common.OBJ_COL_Id)]
        public int OBJECT_ID { get; set; }

        [Display(Name = Common.OBJ_Name)]
        public string? NAME { get; set; }

        [Display(Name = Common.OBJ_SchemaName)]
        public string? SCHEMA_NAME { get; set; }

        [Display(Name = Common.OBJ_Type)]
        public string? TYPE { get; set; }

        [Display(Name = Common.OBJ_CreateDate)]
        public string? CREATE_DATE { get; set; }

        [Display(Name = Common.OBJ_ModifyDate)]
        public string? MODIFY_DATE { get; set; }

        [Display(Name = Common.OBJ_RowCount)]
        public long QTY { get; set; }
    }
}
