using System;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Extended_prop
    {
        /// <summary>
        /// 項目識別碼
        /// </summary>
        public int MAJOR_ID { get; set; }
        /// <summary>
        /// 次要識別碼
        /// </summary>
        public int MINOR_ID { get; set; }
        /// <summary>
        /// 內容名稱
        /// </summary>
        public string NAME { get; set; }
        /// <summary>
        /// 擴充屬性的值
        /// </summary>
        public Object VALUE { get; set; }
        /// <summary>
        /// 可識別內容所在的項目類別
        /// </summary>
        public byte CLASS { get; set; }
    }

    public class DTO_prop
    {
        public int COLUMN_ID { get; set; }
        public string NAME { get; set; }
        public string VALUE { get; set; }
        public string Original_VALUE { get; set; }
        public PropVerb Verb { get; set; }
    }
}
