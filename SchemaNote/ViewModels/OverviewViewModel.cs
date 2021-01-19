using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
    public class OverviewViewModel : IConnString
    {
        public long ADO_dot_NET { get; set; }
        public long Dapper { get; set; }
        public long ADO_dot_NET2 { get; set; }

        public List<Table> Tables { get; set; } = new List<Table>();
        public Dictionary<string, List<string>> TableNameJson {
            get {
                var d = new Dictionary<string, List<string>>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    d.Add(Common.accordion + i, new List<string>() { t.NAME.ToUpper() });
                });
                return d;
            }
        }
        public Dictionary<string, List<string>> ColumnNameJson {
            get {
                var d = new Dictionary<string, List<string>>();
                int i = 0;
                Tables.ForEach(t =>
                {
                    i++;
                    var l = new List<string>();
                    t.Columns.ForEach(c =>
                    {
                        l.Add(c.NAME.ToUpper());
                    });
                    d.Add(Common.accordion + i, l);
                });
                return d;
            }
        }
        public Dictionary<string, List<string>> DescriptionJson {
            get {
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
                return d;
            }
        }
        public Dictionary<string, List<string>> RemarkJson {
            get {
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
        [Display(Name = Common.PropDesc)]
        public string MS_Description { get { return string.IsNullOrEmpty(_MS_Description) ? Common.DefaultValue : _MS_Description; } set { _MS_Description = value; } }
        #endregion

        #region REMARK
        string _REMARK;
        [Display(Name = Common.RropRemark)]
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
        [Display(Name = Common.PropDesc)]
        public string MS_Description { get { return string.IsNullOrEmpty(_MS_Description) ? Common.DefaultValue : _MS_Description; } set { _MS_Description = value; } }
        #endregion

        #region REMARK
        string _REMARK;
        [Display(Name = Common.RropRemark)]
        public string REMARK { get { return string.IsNullOrEmpty(_REMARK) ? Common.DefaultValue : _REMARK; } set { _REMARK = value; } }
        #endregion

        [Display(Name = "資料型態")]
        public string TYPE { get { return TYPE_NAME + LENGTH; } }
    }
}
