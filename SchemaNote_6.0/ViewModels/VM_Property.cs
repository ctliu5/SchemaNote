using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
    public class VM_Property
    {
        public int COLUMN_ID { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false), StringLength(Models.Common.StrMaxLen)]
        public string MS_Description { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false), StringLength(Models.Common.StrMaxLen)]
        public string REMARK { get; set; }
    }
}
