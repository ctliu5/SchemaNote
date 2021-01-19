using System;

namespace SchemaNote.Models
{
    public static class Common
    {
        public const string CurrentVersion = "Version: 0.1.0";
        public const string DefaultValue = "(空白)";
        public const string ConnString = "Connection String";
        public const string NAME_CHT = "中文名稱";
        public const string REMARK = "備註";
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
}
