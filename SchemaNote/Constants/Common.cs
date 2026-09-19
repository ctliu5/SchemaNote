using System.Text.Encodings.Web;
using System.Text.Json;

namespace SchemaNote.Constants
{
    public static class Common
    {
        #region JSON 序列化設定
        /// <summary>
        /// 共用的 JSON 序列化設定：camelCase 命名、寬鬆跳脫、不縮排。
        /// </summary>
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };
        #endregion
        #region 擴充屬性 Keys
        public const string MS_Desc = "MS_Description";
        public const string Remark = "REMARK";
        public const string Flags = "FLAGS";
        #endregion
        #region 表格 Headers
        public const string OBJ_Flags = "標籤";
        public const string OBJ_COL_Id = "物件識別碼";
        public const string OBJ_COL_ChineseName = "中文名稱";
        public const string OBJ_COL_Remark = "備註";
        public const string OBJ_Name = "表格名稱";
        public const string OBJ_SchemaName = "結構描述名稱";
        public const string OBJ_Type = "表格類型";
        public const string OBJ_CreateDate = "表格創建日期";
        public const string OBJ_ModifyDate = "表格修改日期";
        public const string OBJ_RowCount = "筆數";
        public const string COL_Name = "欄位名稱";
        public const string COL_Type = "資料型別";
        public const string COL_NotNull = "不為Null";
        public const string COL_DefaultVal = "預設值";
        public const string COL_SeqId = "欄位序碼";
        public const string COL_IsPersisted = "計算結果儲存";
        public const string COL_IndexType = "索引類型";
        public const string P_Key = "主鍵";
        #endregion
        #region 系統值
        public const int StrMaxLen = 4000;
        public const char FlagsSeparator = ';';
        public const int SheetNameMaxLen = 31; // Excel 工作表（Sheet）名稱限制：最多 31 字元，且不可包含下列特殊字元。
        public const string SheetNameInvalidChars = @"\/?*[]:";
        public const string CurrentVersion = "Version: 1.0.0";
        public const string DefaultValue = "null";
        public const string NoneSheetName = "-none-"; // 沒有設定標籤（或標籤值即為此字串）的 Table/View 集中放置的工作表名稱。
        public const string accordion = "accordion";
        #endregion
        #region 文字訊息
        public const string ConnStringMissing = "Your connection string is missing!";
        public const string ConnString = "Connection String";
        public const string ValidationMsg = "欄位驗證錯誤! 允許最多4000個字。";
        public const string CountTip = "注意！此為參考值，非準確值。";
        public const string ToolTip擴充屬性Excel = "相同標籤的 Table/View，會集中在同一個工作表（Sheet）內。不存在的擴充屬性呈現空白";
        public const string ToolTip擴充屬性Markdown = "相同標籤的 Table/View，會集中在同一個區塊（Accordion）內。不存在的擴充屬性呈現 null";
        public const string ToolTip擴充屬性SQL = "此SQL腳本寫入行為是「Upsert」；若物件欄位不存在，會採取跳過不報錯策略。";
        public const string ToolTip擴充屬性DropAll = "刪除所有本平台定義的所有擴充屬性";
        #endregion

        public static string OBJ_TypeDesc(string? typeId)
        {
            return (typeId ?? string.Empty) switch
            {
                "U" => "資料表",
                "V" => "檢視表",
                _ => "（無法辨識類型）",
            };
        }
    }
    #region enum
    public enum IndexType
    {
        堆積,
        叢集,
        非叢集,
        XML,
        空間,
        叢集資料行存放區索引,
        非叢集資料行存放區索引,
        非叢集雜湊索引,
    }

    [Flags]
    public enum ExceResultType
    {
        Success = 0x0,
        Failed = 0x1,
        NoData = 0x2,
        Exception = 0x4,
    }

    public enum PropVerb
    {
        undefined,
        add,
        update,
        drop,
    }

    public enum DB_tool
    {
        ADO_dot_NET,
        Dapper,
    }
    #endregion
}
