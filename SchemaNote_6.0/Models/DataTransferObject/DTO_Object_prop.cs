using System;

namespace SchemaNote.Models.DataTransferObject
{
    public class DTO_Object_prop
    {
        /// <summary>
        /// 結構描述名稱
        /// </summary>
        public string SCHEMA_NAME { get; set; }
        /// <summary>
        /// 物件識別碼
        /// </summary>
        public int OBJECT_ID { get; set; }
        /// <summary>
        /// 資料表名
        /// </summary>
        public string NAME { get; set; }
        /// <summary>
        /// 物件類型
        /// </summary>
        public string TYPE { get; set; }
        /// <summary>
        /// 欄位序碼
        /// </summary>  
        public int COLUMN_ID { get; set; }
        /// <summary>
        /// 欄位名稱
        /// </summary>  
        public string COLUMN_NAME { get; set; }
        /// <summary>
        /// 項目識別碼
        /// </summary>
        public int MAJOR_ID { get; set; }
        /// <summary>
        /// 次要識別碼
        /// </summary>
        public int MINOR_ID { get; set; }
        /// <summary>
        /// 擴充屬性的名稱
        /// </summary>
        public string PROP_NAME { get; set; }
        /// <summary>
        /// 擴充屬性的值
        /// </summary>
        public Object PROP_VALUE { get; set; }
        /// <summary>
        /// 可識別內容所在的項目類別
        /// </summary>
        public byte CLASS { get; set; }
    }
}
