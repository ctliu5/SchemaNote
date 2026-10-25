//#define 測試效能
using Microsoft.Data.SqlClient;
using SchemaNote.Constants;
using SchemaNote.DataAccess.DB_Tools;
using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using SchemaNote.ViewModels;
using System.Reflection;
using System.Text;

namespace SchemaNote.DataAccess;

public static class DB_Access
{
    internal static OverviewViewModel GetTables_Columns(string ConnectionString, DB_tool db_Tool)
    {
        List<DTO_Column> cols = [];
        List<DTO_Table> tbls = [];
        List<DTO_Extended_prop> props = [];
#if 測試效能
        System.Diagnostics.Stopwatch sw = new();
        long ADO_dot_NET = 0, Dapper = 0, ADO_dot_NET2 = 0, ADO_dot_NET3 = 0, ADO_dot_NET4 = 0;
#endif

        try
        {
            switch (db_Tool)
            {
                case DB_tool.Dapper:
                    ORM_Dapper dapper = new(ConnectionString);
                    dapper.GetColumns(ref cols);
                    dapper.GetTables(ref tbls);
                    dapper.GetExtended_prop(ref props);
                    break;
                default:
                    ADO_dot_NET ADO = new(ConnectionString);
                    ADO.GetColumns(ref cols);
                    ADO.GetTables(ref tbls);
                    ADO.GetExtended_prop(ref props);
                    break;
            }
#if 測試效能
            for (int i = 0; i < 200; i++)
            {
                sw.Start();
                ADO_dot_NET4 ADO4 = new(ConnectionString);
                ADO4.GetColumns(ref cols);
                ADO4.GetTables(ref tbls);
                ADO4.GetExtended_prop(ref props);
                sw.Stop();
                ADO_dot_NET4 += sw.ElapsedMilliseconds;
                sw.Reset();

                sw.Start();
                ADO_dot_NET3 ADO3 = new(ConnectionString);
                ADO3.GetColumns(ref cols);
                ADO3.GetTables(ref tbls);
                ADO3.GetExtended_prop(ref props);
                sw.Stop();
                ADO_dot_NET3 += sw.ElapsedMilliseconds;
                sw.Reset();

                sw.Start();
                ADO_dot_NET2 ADO2 = new(ConnectionString);
                ADO2.GetColumns(ref cols);
                ADO2.GetTables(ref tbls);
                ADO2.GetExtended_prop(ref props);
                sw.Stop();
                ADO_dot_NET2 += sw.ElapsedMilliseconds;
                sw.Reset();

                sw.Start();
                ADO_dot_NET ADO = new(ConnectionString);
                ADO.GetColumns(ref cols);
                ADO.GetTables(ref tbls);
                ADO.GetExtended_prop(ref props);
                sw.Stop();
                ADO_dot_NET += sw.ElapsedMilliseconds;
                sw.Reset();

                sw.Start();
                ORM_Dapper dapper = new(ConnectionString);
                dapper.GetColumns(ref cols);
                dapper.GetTables(ref tbls);
                dapper.GetExtended_prop(ref props);
                sw.Stop();
                Dapper += sw.ElapsedMilliseconds;
                sw.Reset();
            }
#endif
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }

        OverviewViewModel result = new()
        {
            Tables = [.. tbls.Select(t =>
            {
                var pObj = props.Where(p => p.MAJOR_ID == t.OBJECT_ID);
                IEnumerable<DTO_Extended_prop> pT = pObj.Where(p => p.MINOR_ID == 0);
                return new Table
                {
                    OBJECT_ID = t.OBJECT_ID,
                    NAME = t.NAME,
                    SCHEMA_NAME = t.SCHEMA_NAME,
                    TYPE = t.TYPE,
                    CREATE_DATE = t.CREATE_DATE,
                    MODIFY_DATE = t.MODIFY_DATE,
                    QTY = t.QTY,
                    MS_Description = (pT.FirstOrDefault(p => p.NAME?.Equals(Common.MS_Desc, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object ms_Description ? ms_Description.ToString() is string ms_Description_str ? ms_Description_str : string.Empty : string.Empty,
                    REMARK = (pT.FirstOrDefault(p => p.NAME?.Equals(Common.Remark, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object remark ? remark.ToString() is string remark_str ? remark_str : string.Empty : string.Empty,
                    FLAGS = (pT.FirstOrDefault(p => p.NAME?.Equals(Common.Flags, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object flags ? flags.ToString() is string flags_str ? flags_str : string.Empty : string.Empty,
                    Columns = [.. cols.Where(c => c.OBJECT_ID == t.OBJECT_ID).Select(c =>
                    {
                        var pC = pObj.Where(p => p.MINOR_ID == c.COLUMN_ID);
                        return new Column
                        {
                            OBJECT_ID = c.OBJECT_ID,
                            COLUMN_ID = c.COLUMN_ID,
                            NAME = c.NAME,
                            TYPE_NAME = c.TYPE_NAME,
                            LENGTH = c.LENGTH,
                            IS_PK = c.IS_PK,
                            DISALLOW_NULL = c.DISALLOW_NULL,
                            DEFUALT = c.DEFUALT,
                            MS_Description = (pC.FirstOrDefault(p => p.NAME?.Equals(Common.MS_Desc, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object ms_Description ? ms_Description.ToString() is string ms_Description_str ? ms_Description_str : string.Empty : string.Empty,
                            REMARK = (pC.FirstOrDefault(p => p.NAME?.Equals(Common.Remark, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object remark ? remark.ToString() is string remark_str ? remark_str : string.Empty : string.Empty,
                        };
                    })]
                };
            })]
        };
#if 測試效能
        result.ADO_dot_NET = ADO_dot_NET;
        result.Dapper = Dapper;
        result.ADO_dot_NET2 = ADO_dot_NET2;
        result.ADO_dot_NET3 = ADO_dot_NET3;
        result.ADO_dot_NET4 = ADO_dot_NET4;
#endif
        return result;
    }

    internal static DetailsViewModel GetTable_Columns(string ConnectionString, int _OBJECT_ID, DB_tool db_Tool)
    {
        List<DTO_Extended_prop> props = [];
        List<DTO_Index> indexes = [];
        DTO_Table tbl = new();
        List<DTO_Column> cols = [];

        try
        {
            switch (db_Tool)
            {
                case DB_tool.Dapper:
                    ORM_Dapper dapper = new(ConnectionString);
                    cols = dapper.GetColumnsByOBJECT_ID(_OBJECT_ID);
                    var table1 = dapper.GetTablesByOBJECT_ID(_OBJECT_ID);
                    if (table1.Count < 1)
                    {
                        throw new InvalidOperationException("找不到資料表，（OBJECT_ID為：" + _OBJECT_ID + "）");
                    }
                    tbl = table1.First();
                    dapper.GetExtended_prop(ref props);
                    dapper.GetIndexes(ref indexes);
                    break;
                default:
                    ADO_dot_NET ADO = new(ConnectionString);
                    cols = ADO.GetColumnsByOBJECT_ID(_OBJECT_ID);
                    var table2 = ADO.GetTablesByOBJECT_ID(_OBJECT_ID);
                    if (table2.Count < 1)
                    {
                        throw new InvalidOperationException("找不到資料表，（OBJECT_ID為：" + _OBJECT_ID + "）");
                    }
                    tbl = table2.First();
                    ADO.GetExtended_prop(ref props);
                    ADO.GetIndexes(ref indexes);
                    break;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }

        var pObj = props.Where(p => p.MAJOR_ID == tbl.OBJECT_ID);
        var pT = pObj.Where(p => p.MINOR_ID == 0);
        var iObj = indexes.Where(i => i.OBJECT_ID == tbl.OBJECT_ID);
        int SortNum = 0;
        return new DetailsViewModel
        {
            OBJECT_ID = tbl.OBJECT_ID,
            NAME = tbl.NAME,
            SCHEMA_NAME = tbl.SCHEMA_NAME,
            TYPE = tbl.TYPE,
            CREATE_DATE = tbl.CREATE_DATE,
            MODIFY_DATE = tbl.MODIFY_DATE,
            QTY = tbl.QTY,
            MS_Description = (pT.FirstOrDefault(p => p.NAME?.Equals(Common.MS_Desc, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object ms_Description ? ms_Description.ToString() is string ms_Description_str ? ms_Description_str : string.Empty : string.Empty,
            REMARK = (pT.FirstOrDefault(p => p.NAME?.Equals(Common.Remark, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object remark ? remark.ToString() is string remark_str ? remark_str : string.Empty : string.Empty,
            FLAGS = (pT.FirstOrDefault(p => p.NAME?.Equals(Common.Flags, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object flags ? flags.ToString() is string flags_str ? flags_str : string.Empty : string.Empty,
            Columns = [.. cols.Select(c =>
            {
                var pC = pObj.Where(p => p.MINOR_ID == c.COLUMN_ID);
                return new ColumnDetail()
                {
                    SortNum = ++SortNum,
                    OBJECT_ID = c.OBJECT_ID,
                    COLUMN_ID = c.COLUMN_ID,
                    NAME = c.NAME,
                    TYPE_NAME = c.TYPE_NAME,
                    LENGTH = c.LENGTH,
                    IS_PK = c.IS_PK,
                    DISALLOW_NULL = c.DISALLOW_NULL,
                    DEFUALT = c.DEFUALT,
                    IS_COMPUTED = c.IS_COMPUTED,
                    IS_PERSISTED = c.IS_PERSISTED,
                    COMPUTED_DEFINITION = c.COMPUTED_DEFINITION,
                    MS_Description = (pC.FirstOrDefault(p => p.NAME?.Equals(Common.MS_Desc, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object ms_Description ? ms_Description.ToString() is string ms_Description_str ? ms_Description_str : string.Empty : string.Empty,
                    REMARK = (pC.FirstOrDefault(p => p.NAME?.Equals(Common.Remark, StringComparison.OrdinalIgnoreCase) ?? false)?.VALUE) is object remark ? remark.ToString() is string remark_str ? remark_str : string.Empty : string.Empty,
                    Indexes = [.. iObj.Where(i => i.COLUMN_ID == c.COLUMN_ID).Select(i => new IndexDetail()
                    {
                        OBJECT_ID = i.OBJECT_ID,
                        INDEX_ID = i.INDEX_ID,
                        NAME = i.NAME,
                        COLUMN_ID = i.COLUMN_ID,
                        TYPE = i.TYPE,
                        TYPE_DESC = i.TYPE_DESC,
                        IS_UNIQUE = i.IS_UNIQUE,
                        IS_PK = i.IS_PK,
                        FILL_FACTOR = i.FILL_FACTOR,
                    })]
                };
            })],
            // 彙整資料庫中所有物件出現過的標籤（去重、排序），供標籤輸入框下拉建議
            AllFlags = [.. props
                .Where(p => p.MINOR_ID == 0 && (p.NAME?.Equals(Common.Flags, StringComparison.OrdinalIgnoreCase) ?? false))
                .SelectMany(p => (p.VALUE?.ToString() ?? string.Empty).Split(Common.FlagsSeparator))
                .Where(f => f.Length > 0)
                .Distinct()
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)]
        };
    }

    internal static int SaveProperties(string ConnectionString, int id, ICollection<VM_Property> model, DB_tool db_Tool)
    {
        #region 取得物件的所有自訂屬性
        var props = new List<DTO_Extended_prop>();
        try
        {
            switch (db_Tool)
            {
                case DB_tool.Dapper:
                    ORM_Dapper dapper = new(ConnectionString);
                    dapper.GetExtended_prop(ref props);
                    return dapper.SaveProperties(id, Categorized(model, [.. props.Where(p => p.MAJOR_ID == id)]));
                default:
                    ADO_dot_NET ADO = new(ConnectionString);
                    ADO.GetExtended_prop(ref props);
                    return ADO.SaveProperties(id, Categorized(model, [.. props.Where(p => p.MAJOR_ID == id)]));
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
        #endregion
    }

    /// <summary>
    /// 整理出要新刪修的屬性
    /// </summary>
    /// <param name="model"></param>
    /// <param name="curr_props"></param>
    /// <returns></returns>
    static List<DTO_prop> Categorized(ICollection<VM_Property> model, List<DTO_Extended_prop> curr_props)
    {
        List<DTO_prop> dto_prop = [];

        foreach (VM_Property vm in model)
        {
            if (vm.MS_Description != null)
                dto_prop.Add(
                new DTO_prop
                {
                    COLUMN_ID = vm.COLUMN_ID,
                    NAME = Common.MS_Desc,
                    VALUE = vm.MS_Description,
                });

            if (vm.REMARK != null)
                dto_prop.Add(
                new DTO_prop
                {
                    COLUMN_ID = vm.COLUMN_ID,
                    NAME = Common.Remark,
                    VALUE = vm.REMARK,
                });

            if (vm.FLAGS != null)
                dto_prop.Add(
                new DTO_prop
                {
                    COLUMN_ID = vm.COLUMN_ID,
                    NAME = Common.Flags,
                    VALUE = vm.FLAGS,
                });
        }

        IEnumerable<DTO_prop> UnionProp = dto_prop.GroupJoin(
             curr_props,
             d => new { d.COLUMN_ID, NAME = d.NAME?.ToUpper() },
             p => new { COLUMN_ID = p.MINOR_ID, NAME = p.NAME?.ToUpper() },
             (d, p) => new DTO_prop
             {
                 COLUMN_ID = d.COLUMN_ID,
                 NAME = d.NAME,
                 VALUE = d.VALUE?.Trim(),
                 Original_VALUE = p.Any()
                    ? (p.First().VALUE?.ToString()?.Trim() ?? string.Empty)
                    : null,
             });

        List<DTO_prop> Properties = [];

        foreach (DTO_prop p in UnionProp)
        {
            //屬性值若一致，便不動作
            if (p.VALUE == p.Original_VALUE) continue;

            if (p.Original_VALUE != null)
            {
                if (p.VALUE == Common.DefaultValue || p.VALUE == string.Empty)
                {
                    //原始屬性有值，填寫的屬性為空字串或空白值，代表要刪除屬性
                    p.Verb = PropVerb.drop;
                    Properties.Add(p); continue;
                }
                else
                {
                    //原始屬性有值，代表要更新屬性
                    p.Verb = PropVerb.update;
                    Properties.Add(p); continue;
                }
            }
            else if (p.VALUE != Common.DefaultValue && p.VALUE != string.Empty)
            {
                //原始屬性無值，填寫的屬性非為空字串或空白值，代表要新增屬性
                p.Verb = PropVerb.add;
                Properties.Add(p); continue;
            }
        }

        return Properties;
    }

    internal static StringBuilder ExportPropertiesScript(string ConnectionString, DB_tool db_Tool, bool ForDeleteEmptyData = false, int[]? objectIds = null)
    {
        List<DTO_Object_prop> object_props = [];
        string scriptTemp = string.Empty;
        StringBuilder stringBuilder = new(
        """
        ------ 變數宣告 BEGIN ------
        DECLARE
        @id int,
        @col_id int,
        @name sysname,
        @value sql_variant,
        @level0name sysname,
        @level1type sysname,
        @level1name sysname,
        @level2name sysname,
        @propQty int;

        DECLARE @props TABLE
        (
        [name] sysname,
        [value] sql_variant,
        [level0name] sysname,
        [level1type] sysname,
        [level1name] sysname,
        [level2name] sysname
        );
        ------ 變數宣告 END ------

        ------ 擴充屬性資料 BEGIN ------

        """);

        #region 匯出擴充屬性資料
        try
        {
            if (ForDeleteEmptyData)
            {
                scriptTemp = SQLScripts.DeleteScript_Extended_prop;
                switch (db_Tool)
                {
                    case DB_tool.Dapper:
                        ORM_Dapper dapper = new(ConnectionString);
                        dapper.GetObjectExtendedProp_emptyValue(ref object_props);
                        break;
                    default:
                        ADO_dot_NET ADO = new(ConnectionString);
                        ADO.GetObjectExtendedProp_emptyValue(ref object_props);
                        break;
                }
            }
            else
            {
                scriptTemp = SQLScripts.SavingScript_Extended_prop;
                switch (db_Tool)
                {
                    case DB_tool.Dapper:
                        ORM_Dapper dapper = new(ConnectionString);
                        dapper.GetObjectExtendedProp_NotEmpty(ref object_props);
                        break;
                    default:
                        ADO_dot_NET ADO = new(ConnectionString);
                        ADO.GetObjectExtendedProp_NotEmpty(ref object_props);
                        break;
                }
            }
            if (object_props.Count < 1)
            {
                throw new InvalidOperationException("找不到擴充屬性");
            }

            // 依前端傳入的 OBJECT_ID 清單（搜尋/標籤過濾後仍顯示的項目）僅匯出對應物件的擴充屬性。
            if (objectIds is not null && objectIds.Length > 0)
            {
                var idSet = new HashSet<int>(objectIds);
                object_props = [.. object_props.Where(op => idSet.Contains(op.OBJECT_ID))];
                if (object_props.Count < 1)
                {
                    throw new InvalidOperationException("找不到擴充屬性");
                }
            }

            foreach (var op in object_props)
            {
                stringBuilder.Append(
                string.Format("INSERT INTO @props VALUES (N'{0}', N'{1}', N'{2}', N'{3}', N'{4}', N'{5}');{6}",
                    op.PROP_NAME?.Replace("'", "''"),
                    op.PROP_VALUE?.ToString()?.Replace("'", "''") ?? string.Empty,
                    op.SCHEMA_NAME?.Replace("'", "''"),
                    op.TYPE,
                    op.NAME?.Replace("'", "''"),
                    op.COLUMN_NAME?.Replace("'", "''"),
                    Environment.NewLine));
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("------ 擴充屬性資料 END ------");
            stringBuilder.AppendLine();
            stringBuilder.Append(scriptTemp);
            return stringBuilder;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
        #endregion
    }

    internal static int DropAllProperties(string ConnectionString, DB_tool db_Tool, int[]? objectIds = null, string? propName = null)
    {
        List<DTO_Object_prop> object_props = [];
        try
        {
            switch (db_Tool)
            {
                case DB_tool.Dapper:
                    ORM_Dapper dapper = new(ConnectionString);
                    dapper.GetObjectExtendedProp(ref object_props);
                    break;
                default:
                    ADO_dot_NET ADO = new(ConnectionString);
                    ADO.GetObjectExtendedProp(ref object_props);
                    break;
            }

            // 依前端傳入的 OBJECT_ID 清單（搜尋/標籤過濾後仍顯示的項目）僅刪除對應物件的擴充屬性。
            if (objectIds is not null && objectIds.Length > 0)
            {
                var idSet = new HashSet<int>(objectIds);
                object_props = [.. object_props.Where(op => idSet.Contains(op.OBJECT_ID))];
            }

            // 若指定屬性名稱（例如僅刪除 FLAGS 標籤），僅保留該名稱的擴充屬性。
            if (!string.IsNullOrEmpty(propName))
            {
                object_props = [.. object_props.Where(op => string.Equals(op.PROP_NAME, propName, StringComparison.OrdinalIgnoreCase))];
            }

            if (object_props.Count < 1)
            {
                throw new InvalidOperationException("找不到擴充屬性");
            }

            switch (db_Tool)
            {
                case DB_tool.Dapper:
                    ORM_Dapper dapper = new(ConnectionString);
                    return dapper.DropAllProperties(object_props);
                default:
                    ADO_dot_NET ADO = new(ConnectionString);
                    return ADO.DropAllProperties(object_props);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}
