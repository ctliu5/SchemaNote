using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
    public class OverviewViewModel : IConnString
    {
        public List<Table> Tables { get; set; } = new List<Table>();
        public Dictionary<string, string> TableNameJson {
            get {
                var d = new Dictionary<string, string>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    d.Add(Common.accordion + i, t.NAME.ToUpper());
                });
                return d;
            }
        }
        public Dictionary<string, string> ColumnNameJson {
            get {
                var d = new Dictionary<string, string>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    string Column = string.Empty;
                    t.Columns.ForEach(c => Column += c.NAME.ToUpper());
                    d.Add(Common.accordion + i, Column);
                });
                return d;
            }
        }
        public Dictionary<string, string> DescriptionJson {
            get {
                var d = new Dictionary<string, string>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    string Description = string.Empty;
                    t.Columns.ForEach(c => Description += c.MS_Description.ToUpper());
                    d.Add(Common.accordion + i, t.MS_Description.ToUpper() + Description);
                });
                return d;
            }
        }
        public Dictionary<string, string> RemarkJson {
            get {
                var d = new Dictionary<string, string>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    string Remark = string.Empty;
                    t.Columns.ForEach(c => Remark += c.REMARK.ToUpper());
                    d.Add(Common.accordion + i, t.REMARK.ToUpper() + Remark);
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

        #region MS_Description
        string _MS_Description;
        [Display(Name = Common.Name_zh)]
        public string MS_Description { get { return string.IsNullOrEmpty(_MS_Description) ? Common.DefaultValue : _MS_Description; } set { _MS_Description = value; } }
        #endregion

        #region REMARK
        string _REMARK;
        [Display(Name = Common.Remark_zh)]
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
        #region MS_Description
        string _MS_Description;
        [Display(Name = Common.Name_zh)]
        public string MS_Description { get { return string.IsNullOrEmpty(_MS_Description) ? Common.DefaultValue : _MS_Description; } set { _MS_Description = value; } }
        #endregion

        #region REMARK
        string _REMARK;
        [Display(Name = Common.Remark_zh)]
        public string REMARK { get { return string.IsNullOrEmpty(_REMARK) ? Common.DefaultValue : _REMARK; } set { _REMARK = value; } }
        #endregion

        [Display(Name = "資料型態")]
        public string TYPE { get { return TYPE_NAME + LENGTH; } }
    }
}
