using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SchemaNote.ViewModels
{
    public class OverviewViewModel : IConnString
    {
        public long ADO_dot_NET { get; set; }
        public long Dapper { get; set; }
        public long ADO_dot_NET2 { get; set; }
        public long ADO_dot_NET3 { get; set; }
        public long ADO_dot_NET4 { get; set; }

        public List<Table> Tables { get; set; } = [];
        public string TableNameJson
        {
            get
            {
                var d = new Dictionary<string, List<string>>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    d.Add(Common.accordion + i, [t.NAME?.ToUpper() ?? string.Empty]);
                });
                return JsonSerializer.Serialize(d, Common.JsonOptions);
            }
        }
        public string ColumnNameJson
        {
            get
            {
                var d = new Dictionary<string, List<string>>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    var l = new List<string>();
                    t.Columns.ForEach(c =>
                    {
                        l.Add(c.NAME?.ToUpper() ?? string.Empty);
                    });
                    d.Add(Common.accordion + i, l);
                });
                return JsonSerializer.Serialize(d, Common.JsonOptions);
            }
        }
        public string DescriptionJson
        {
            get
            {
                var d = new Dictionary<string, List<string>>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    var l = new List<string>
                    {
                        t.MS_Description.ToUpper()
                    };
                    t.Columns.ForEach(c =>
                    {
                        l.Add(c.MS_Description.ToUpper());
                    });
                    d.Add(Common.accordion + i, l);
                });
                return JsonSerializer.Serialize(d, Common.JsonOptions);
            }
        }
        public string RemarkJson
        {
            get
            {
                var d = new Dictionary<string, List<string>>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    var l = new List<string>()
                    {
                        t.REMARK.ToUpper()
                    };
                    t.Columns.ForEach(c =>
                    {
                        l.Add(c.REMARK.ToUpper());
                    });
                    d.Add(Common.accordion + i, l);
                });
                return JsonSerializer.Serialize(d, Common.JsonOptions);
            }
        }

        /// <summary>
        /// 每個 accordion 對應的標籤陣列（原始大小寫），供前端標籤過濾比對。
        /// </summary>
        public string FlagsJson
        {
            get
            {
                var d = new Dictionary<string, List<string>>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    d.Add(Common.accordion + i, t.FlagList);
                });
                return JsonSerializer.Serialize(d, Common.JsonOptions);
            }
        }

        /// <summary>
        /// 所有物件出現過的標籤（去重、依名稱排序），供過濾下拉選單顯示。
        /// </summary>
        public string AllFlagsJson
        {
            get
            {
                var all = Tables
                    .SelectMany(t => t.FlagList)
                    .Distinct()
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return JsonSerializer.Serialize(all, Common.JsonOptions);
            }
        }

        public string? DATABASE_Name { get; set; }

        [Display(Name = Common.ConnString), Required]
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class Table : DTO_Table, IProperties
    {
        public List<Column> Columns { get; set; } = [];

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
        #endregion

        [Display(Name = Common.OBJ_Type)]
        public string TYPE_NAME { get => Common.OBJ_TypeDesc(TYPE); }
    }

    public class Column : DTO_Column, IProperties
    {
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

        [Display(Name = "資料型別")]
        public string TYPE { get { return TYPE_NAME + LENGTH; } }
    }
}
