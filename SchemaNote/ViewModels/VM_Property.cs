using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
    public class VM_Property
    {
        public int COLUMN_ID { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NAME_CHT { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string REMARK { get; set; }
    }
}
