using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
    public class DetailsViewModel : DTO_Table, IProperties, IConnString
    {
        public List<ColumnDetail> Columns { get; set; } = [];

        #region MS_Description
        string? _MS_Description;
        [Display(Name = Common.PropDesc)]
        public string MS_Description { get { return string.IsNullOrEmpty(_MS_Description) ? Common.DefaultValue : _MS_Description; } set { _MS_Description = value; } }
        #endregion

        #region REMARK
        string? _REMARK;
        [Display(Name = Common.RropRemark)]
        public string REMARK { get { return string.IsNullOrEmpty(_REMARK) ? Common.DefaultValue : _REMARK; } set { _REMARK = value; } }
        #endregion

        [Display(Name = "物件類型")]
        public string TYPE_NAME
        {
            get
            {
                return TYPE switch
                {
                    "U" => "資料表",
                    "V" => "檢視",
                    _ => "（無法辨識類型）",
                };
            }
        }
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class ColumnDetail : Column
    {
        public int SortNum { get; set; }
        public IndexDetail[] Indexes { get; set; } = [];
    }

    public class IndexDetail : DTO_Index
    {
        [Display(Name = "索引類型")]
        public string TYPE_NAME
        {
            get
            {
                return TYPE.ToString();
            }
        }
    }
}
