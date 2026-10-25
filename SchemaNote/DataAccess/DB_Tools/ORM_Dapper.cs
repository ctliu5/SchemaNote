using Dapper;
using Microsoft.Data.SqlClient;
using SchemaNote.Constants;
using SchemaNote.Models.DataTransferObject;
using System.Data;

namespace SchemaNote.DataAccess.DB_Tools;

public class ORM_Dapper(string _ConnectionString)
{
    string ConnectionString { get; set; } = _ConnectionString;

    internal void GetColumns(ref List<DTO_Column> cols)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        cols = [.. conn.Query<DTO_Column>(SQLScripts.GetColumns)];
    }

    internal void GetTables(ref List<DTO_Table> tbls)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        tbls = [.. conn.Query<DTO_Table>(SQLScripts.GetTables)];
    }

    internal void GetExtended_prop(ref List<DTO_Extended_prop> props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        props = [.. conn.Query<DTO_Extended_prop>(SQLScripts.GetExtended_prop)];
    }

    internal void GetIndexes(ref List<DTO_Index> indexes)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        indexes = [.. conn.Query<DTO_Index>(SQLScripts.GetIndexes)];
    }

    internal List<DTO_Column> GetColumnsByOBJECT_ID(int _OBJECT_ID)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        List<DTO_Column> result = [.. conn.Query<DTO_Column>(SQLScripts.GetColumnsByObject_id, new { OBJECT_ID = _OBJECT_ID })];
        if (result.Count < 1)
        {
            throw new InvalidOperationException("找不到資料欄位，（OBJECT_ID為：" + _OBJECT_ID + "）");
        }
        return result;
    }

    internal List<DTO_Table> GetTablesByOBJECT_ID(int _OBJECT_ID)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        List<DTO_Table> result = [.. conn.Query<DTO_Table>(SQLScripts.GetTablesByObject_id, new { OBJECT_ID = _OBJECT_ID })];
        if (result.Count < 1)
        {
            throw new InvalidOperationException("找不到資料表，（OBJECT_ID為：" + _OBJECT_ID + "）");
        }
        return result;
    }

    internal int SaveProperties(int _OBJECT_ID, List<DTO_prop> props)
    {
        string TYPE, NewLine = Environment.NewLine;
        int affectedRows = 0;

        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        using SqlTransaction transaction = conn.BeginTransaction();
        try
        {
            #region GetSchema By OBJECT_ID
            var schemaInfo = conn.QueryFirst<dynamic>(SQLScripts.GetSchema_ByObject_id, new { OBJECT_ID = _OBJECT_ID }, transaction, 3, CommandType.Text);
            if (schemaInfo.OBJECT_NAME is not string OBJECT_NAME || string.IsNullOrEmpty(OBJECT_NAME))
            {
                throw new InvalidOperationException("找不到OBJECT_NAME，（OBJECT_ID為：" + _OBJECT_ID + "）");
            }
            if (schemaInfo.SCHEMA_NAME is not string SCHEMA_NAME || string.IsNullOrEmpty(SCHEMA_NAME))
            {
                throw new InvalidOperationException("找不到SCHEMA_NAME，（OBJECT_ID為：" + _OBJECT_ID + "）");
            }
            switch ((schemaInfo.TYPE as string)?.Trim())
            {
                case "U":
                    TYPE = "TABLE";
                    break;
                case "V":
                    TYPE = "VIEW";
                    break;
                default:
                    throw new InvalidOperationException("找不到TYPE，（OBJECT_ID為：" + _OBJECT_ID + "）");
            }
            #endregion

            #region  Add/Update/Drop Prop
            foreach (DTO_prop prop in props)
            {
                switch (prop.Verb)
                {
                    case PropVerb.add:
                        affectedRows += conn.Execute(SQLScripts.Addextendedproperty, new
                        {
                            OBJECT_ID = _OBJECT_ID,
                            SCHEMA_NAME,
                            OBJECT_NAME,
                            TYPE,
                            prop.COLUMN_ID,
                            PROP_NAME = prop.NAME,
                            PROP_VALUE = prop.VALUE
                        }, transaction, 3, CommandType.Text);
                        if (affectedRows == 0)
                        {
                            transaction.Rollback();
                            throw new InvalidOperationException("第" + prop.COLUMN_ID + "欄，擴充屬性新增失敗" + NewLine +
                                "內容為：\"" + prop.VALUE + "\"");
                        }
                        break;
                    case PropVerb.update:
                        affectedRows += conn.Execute(SQLScripts.Updateextendedproperty, new
                        {
                            OBJECT_ID = _OBJECT_ID,
                            SCHEMA_NAME,
                            OBJECT_NAME,
                            TYPE,
                            prop.COLUMN_ID,
                            PROP_NAME = prop.NAME,
                            PROP_VALUE = prop.VALUE
                        }, transaction, 3, CommandType.Text);
                        if (affectedRows == 0)
                        {
                            transaction.Rollback();
                            throw new InvalidOperationException("第" + prop.COLUMN_ID + "欄，擴充屬性更新失敗" + NewLine +
                                "內容為：\"" + prop.VALUE + "\"");
                        }
                        break;
                    case PropVerb.drop:
                        affectedRows += conn.Execute(SQLScripts.Dropextendedproperty, new
                        {
                            OBJECT_ID = _OBJECT_ID,
                            SCHEMA_NAME,
                            OBJECT_NAME,
                            TYPE,
                            prop.COLUMN_ID,
                            PROP_NAME = prop.NAME,
                        }, transaction, 3, CommandType.Text);
                        if (affectedRows == 0)
                        {
                            transaction.Rollback();
                            throw new InvalidOperationException("第" + prop.COLUMN_ID + "欄，擴充屬性移除失敗" + NewLine +
                                "內容為：\"" + prop.VALUE + "\"");
                        }
                        break;
                }
            }
            #endregion
            transaction.Commit();
            return affectedRows;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    internal void GetObjectExtendedProp(ref List<DTO_Object_prop> object_props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        object_props = [.. conn.Query<DTO_Object_prop>(SQLScripts.GetObject_Extended_prop)];
    }

    internal void GetObjectExtendedProp_NotEmpty(ref List<DTO_Object_prop> object_props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        object_props = [.. conn.Query<DTO_Object_prop>(SQLScripts.GetObject_Extended_prop_NotEmpty)];
    }

    internal void GetObjectExtendedProp_emptyValue(ref List<DTO_Object_prop> object_props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        object_props = [.. conn.Query<DTO_Object_prop>(SQLScripts.GetObject_Extended_prop_emptyValue)];
    }

    internal int DropAllProperties(List<DTO_Object_prop> object_props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();
        using SqlTransaction transaction = conn.BeginTransaction();
        try
        {
            int affectedRows = 0;
            foreach (DTO_Object_prop prop in object_props)
            {
                string type = prop.TYPE?.Trim() switch
                {
                    "U" => "TABLE",
                    "V" => "VIEW",
                    _ => prop.TYPE?.Trim() ?? string.Empty
                };

                affectedRows += conn.Execute(SQLScripts.Dropextendedproperty, new
                {
                    prop.OBJECT_ID,
                    prop.COLUMN_ID,
                    prop.PROP_NAME,
                    prop.SCHEMA_NAME,
                    TYPE = type,
                    OBJECT_NAME = prop.NAME
                }, transaction, 3, CommandType.Text);
            }
            transaction.Commit();
            return affectedRows;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
