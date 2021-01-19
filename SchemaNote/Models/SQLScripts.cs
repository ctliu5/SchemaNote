using System.IO;
using System.Reflection;
using System.Text;

namespace SchemaNote.Models
{
    /// <summary>
    /// SQL腳本文件
    /// <see cref="https://docs.microsoft.com/zh-tw/visualstudio/msbuild/common-msbuild-project-items?view=vs-2017#embeddedresource"/>
    /// 為了在.Net Core中直接使用.sql檔案格式的SQL Script，於是在專案檔
    /// SchemaNote.csproj中，加入「內嵌資源」（EmbeddedResource）項目 ：
    /// <code>
    /// 
    /// <ItemGroup>
    ///   <EmbeddedResource Include="sql\*.sql" />
    /// </ItemGroup>
    /// 
    /// </code>
    /// 上述使用星號 (*) 萬用字元，接受根目錄中的sql資料夾下的所有副檔名為.sql檔案作為內嵌資源！
    /// </summary>
    /// <remarks>
    /// 在.Net中取得資源的寫法：
    /// 藉由取得當前組件，將組件的方法Assembly.GetManifestResourceStream(string name)，來取得資源檔(Manifest Resource)串流，
    /// 需要提供區分大小寫的資源全名（namespace.myfolder1.myfile.fileExtension）
    /// </remarks>
    public static class SQLScripts
    {
        static Assembly _Assembly { get { return Assembly.GetExecutingAssembly(); } }
        static string GetScript(string _name)
        {
            var resourceStream = _Assembly.GetManifestResourceStream(_name);

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }

        }
        public static string GetTables {
            get {
                return GetScript("SchemaNote.sql.getTables.sql");
            }
        }
        public static string GetColumns {
            get {
                return GetScript("SchemaNote.sql.getColumns.sql");
            }
        }
        public static string GetExtended_prop {
            get {
                return GetScript("SchemaNote.sql.getExtended_prop.sql");
            }
        }
        public static string GetIndexes {
            get {
                return GetScript("SchemaNote.sql.getIndexes.sql");
            }
        }
        public static string GetColumnsByObject_id {
            get {
                return GetScript("SchemaNote.sql.getColumns_ByObject_id.sql");
            }
        }
        public static string GetTablesByObject_id {
            get {
                return GetScript("SchemaNote.sql.getTables_ByObject_id.sql");
            }
        }
        public static string GetSchema_ByObject_id {
            get {
                return GetScript("SchemaNote.sql.getSchema_ByObject_id.sql");
            }
        }
        public static string Addextendedproperty {
            get {
                return GetScript("SchemaNote.sql.addextendedproperty.sql");
            }
        }
        public static string Updateextendedproperty {
            get {
                return GetScript("SchemaNote.sql.updateextendedproperty.sql");
            }
        }
        public static string Dropextendedproperty {
            get {
                return GetScript("SchemaNote.sql.dropextendedproperty.sql");
            }
        }
        public static string GetObject_Extended_prop {
            get {
                return GetScript("SchemaNote.sql.getObject_Extended_prop.sql");
            }
        }
        public static string SavingScript_Extended_prop {
            get {
                return GetScript("SchemaNote.sql.savingScript_Extended_prop.sql");
            }
        }
    }
}
