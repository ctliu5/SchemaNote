using System;

namespace SchemaNote.Models
{
    public static class Common
    {
        public const string CurrentVersion = "Version: 0.1.2";
        public const string DefaultValue = "null";
        public const string ConnString = "Connection String";
        public const string PropDesc = "欄位說明";
        public const string RropRemark = "備註";
        public const string ConnStringMissing = "Your connection string is missing!";
        public const string ValidationMsg = "欄位驗證錯誤! 允許最多4000個字。";
        public const int StrMaxLen = 4000;
        public const string MS_Desc = "MS_Description";
        public const string Remark = "REMARK";
        public const string CountTip = "注意！此為參考值，非準確值。";
        public const string accordion = "accordion";
    }

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
}
