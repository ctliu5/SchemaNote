using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchemaNote.ViewModels
{
    public class DetailsViewModel : DTO_Table, IProperties, IConnString
    {
        public List<ColumnDetail> Columns { get; set; } = new List<ColumnDetail>();

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
        public string ConnectionString { get; set; }
    }

    public class ColumnDetail : Column
    {
        public List<Index> Indexes { get; set; }
    }

    public class Index : DTO_Index
    {
        [Display(Name = "索引類型")]
        public string TYPE_NAME {
            get {
                switch (TYPE)
                {
                    case 0:
                        return "堆積";
                    case 1:
                        return "叢集";
                    case 2:
                        return "非叢集";
                    case 3:
                        return "XML";
                    case 4:
                        return "空間";
                    case 5:
                        return "叢集資料行存放區索引";
                    case 6:
                        return "非叢集資料行存放區索引";
                    case 7:
                        return "非叢集雜湊索引";
                    default:
                        return "（無法辨識類型）";
                }
            }
        }
    }
}
