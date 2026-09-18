using SchemaNote.Constants;
using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SchemaNote.ViewModels
{
    public class DetailsViewModel : DTO_Table, IProperties, IConnString
    {
        public List<ColumnDetail> Columns { get; set; } = [];

        #region MS_Description
        string? _MS_Description;
        [Display(Name = Common.OBJ_COL_ChineseName)]
        public string MS_Description { get { return string.IsNullOrEmpty(_MS_Description) ? Common.DefaultValue : _MS_Description; } set { _MS_Description = value; } }
        #endregion

        #region REMARK
        string? _REMARK;
        [Display(Name = Common.OBJ_COL_Remark)]
        public string REMARK { get { return string.IsNullOrEmpty(_REMARK) ? Common.DefaultValue : _REMARK; } set { _REMARK = value; } }
        #endregion

        #region FLAGS
        [Display(Name = Common.OBJ_Flags)]
        public string FLAGS { get; set; } = string.Empty;

        /// <summary>
        /// 依半形分號切分的標籤清單，保留標籤前後空白，僅濾除完全空字串。
        /// </summary>
        public List<string> FlagList =>
            string.IsNullOrEmpty(FLAGS)
                ? []
                : [.. FLAGS.Split(Common.FlagsSeparator).Where(f => f.Length > 0)];

        /// <summary>
        /// 資料庫中所有物件出現過的標籤（去重、依名稱排序），供標籤輸入框的下拉建議清單使用。
        /// </summary>
        public List<string> AllFlags { get; set; } = [];

        public string AllFlagsJson => JsonSerializer.Serialize(AllFlags, Common.JsonOptions);
        #endregion

        [Display(Name = Common.OBJ_Type)]
        public string TYPE_NAME { get => Common.OBJ_TypeDesc(TYPE); }
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class ColumnDetail : Column
    {
        public int SortNum { get; set; }
        public IndexDetail[] Indexes { get; set; } = [];

        [Display(Name = Common.COL_IsPersisted)]
        public string IS_PERSISTED_DESC { get { return IS_PERSISTED ? "是" : "否"; } }
    }

    public class IndexDetail : DTO_Index
    {
        [Display(Name = Common.COL_IndexType)]
        public string TYPE_NAME
        {
            get
            {
                return TYPE.ToString();
            }
        }
    }
}
