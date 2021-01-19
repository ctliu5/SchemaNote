using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
    public class OverviewViewModel : IConnString
    {
        public List<Table> Tables { get; set; } = new List<Table>();
        public Dictionary<string, string> TablesJson {
            get {
                var d = new Dictionary<string, string>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    d.Add(t.NAME, "accordion" + i);
                });
                return d;
            }
        }

        public string DATABASE_Name { get; set; }

        [Display(Name = Common.ConnString), Required]
        public string ConnectionString { get; set; }
    }

    public class Table : DTO_Table, IProperties
    {
        public List<Column> Columns { get; set; } = new List<Column>();

        #region NAME_CHT
        string _NAME_CHT;
        [Display(Name = Common.NAME_CHT)]
        public string NAME_CHT { get { return string.IsNullOrEmpty(_NAME_CHT) ? Common.DefaultValue : _NAME_CHT; } set { _NAME_CHT = value; } }
        #endregion

        #region REMARK
        string _REMARK;
        [Display(Name = Common.REMARK)]
        public string REMARK { get { return string.IsNullOrEmpty(_REMARK) ? Common.DefaultValue : _REMARK; } set { _REMARK = value; } }
        #endregion

        [Display(Name = "物件類型")]
        public string TYPE_NAME {
            get {
                switch (TYPE)
                {
                    case "U":
                        return "資料表";
                    case "V":
                        return "檢視";
                    default:
                        return "（無法辨識類型）";
                }
            }
        }
    }

    public class Column : DTO_Column, IProperties
    {
        #region NAME_CHT
        string _NAME_CHT;
        [Display(Name = Common.NAME_CHT)]
        public string NAME_CHT { get { return string.IsNullOrEmpty(_NAME_CHT) ? Common.DefaultValue : _NAME_CHT; } set { _NAME_CHT = value; } }
        #endregion

        #region REMARK
        string _REMARK;
        [Display(Name = Common.REMARK)]
        public string REMARK { get { return string.IsNullOrEmpty(_REMARK) ? Common.DefaultValue : _REMARK; } set { _REMARK = value; } }
        #endregion

        [Display(Name = "資料型態")]
        public string TYPE { get { return TYPE_NAME + LENGTH; } }
    }
}
