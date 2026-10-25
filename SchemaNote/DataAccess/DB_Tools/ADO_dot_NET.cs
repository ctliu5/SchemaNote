using Microsoft.Data.SqlClient;
using SchemaNote.Constants;
using SchemaNote.Models.DataTransferObject;
using SchemaNote.Models.Extensions;
using System.Transactions;

namespace SchemaNote.DataAccess.DB_Tools;
/// <summary>
/// <see cref="https://stackoverflow.com/questions/4439409/open-close-sqlconnection-or-keep-open"/>
/// </summary>
public class ADO_dot_NET(string _ConnectionString)
{
    protected string ConnectionString { get; set; } = _ConnectionString;

    public virtual List<T> ExecSqlDataReader<T>(SqlCommand comm) where T : new()
    {
        using SqlDataReader dr = comm.ExecuteReader();
        if (dr.HasRows)
            return dr.ReadAll<T>();
        else
            return [];
    }

    internal void GetColumns(ref List<DTO_Column> cols)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new(SQLScripts.GetColumns, conn);
        cols = ExecSqlDataReader<DTO_Column>(comm);
    }

    internal void GetTables(ref List<DTO_Table> tbls)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new(SQLScripts.GetTables, conn);
        tbls = ExecSqlDataReader<DTO_Table>(comm);
    }

    internal void GetExtended_prop(ref List<DTO_Extended_prop> props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new(SQLScripts.GetExtended_prop, conn);
        props = ExecSqlDataReader<DTO_Extended_prop>(comm);
    }

    internal void GetIndexes(ref List<DTO_Index> indexes)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new(SQLScripts.GetIndexes, conn);
        indexes = ExecSqlDataReader<DTO_Index>(comm);
    }

    internal List<DTO_Column> GetColumnsByOBJECT_ID(int _OBJECT_ID)
    {
        SqlParameter para = new()
        {
            ParameterName = "OBJECT_ID",
            SqlDbType = System.Data.SqlDbType.Int,
            Value = _OBJECT_ID
        };

        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using (SqlCommand comm = new(SQLScripts.GetColumnsByObject_id, conn))
        {
            comm.Parameters.Add(para);
            List<DTO_Column> result = ExecSqlDataReader<DTO_Column>(comm);
            if (result.Count < 1)
            {
                throw new InvalidOperationException("找不到資料欄位，（OBJECT_ID為：" + _OBJECT_ID + "）");
            }
            return result;
        }
    }

    internal List<DTO_Table> GetTablesByOBJECT_ID(int _OBJECT_ID)
    {
        SqlParameter para = new()
        {
            ParameterName = "OBJECT_ID",
            SqlDbType = System.Data.SqlDbType.Int,
            Value = _OBJECT_ID
        };

        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using (SqlCommand comm = new(SQLScripts.GetTablesByObject_id, conn))
        {
            comm.Parameters.Add(para);
            List<DTO_Table> result = ExecSqlDataReader<DTO_Table>(comm);
            if (result.Count < 1)
            {
                throw new InvalidOperationException("找不到資料表，（OBJECT_ID為：" + _OBJECT_ID + "）");
            }
            return result;
        }
    }

    internal int SaveProperties(int _OBJECT_ID, List<DTO_prop> props)
    {
        SqlParameter para_OBJECT_ID = new()
        {
            ParameterName = "OBJECT_ID",
            SqlDbType = System.Data.SqlDbType.Int,
            Value = _OBJECT_ID
        };

        string SCHEMA_NAME = "", OBJECT_NAME = "", TYPE = "", NewLine = Environment.NewLine;

        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new()
        {
            Connection = conn
        };

        comm.Parameters.Add(para_OBJECT_ID);

        #region GetSchema By OBJECT_ID
        comm.CommandText = SQLScripts.GetSchema_ByObject_id;
        using (SqlDataReader dr = comm.ExecuteReader())
        {
            while (dr.Read())
            {
                OBJECT_NAME = dr["OBJECT_NAME"]?.ToString() ?? string.Empty;
                SCHEMA_NAME = dr["SCHEMA_NAME"]?.ToString() ?? string.Empty;
                TYPE = dr["TYPE"]?.ToString()?.Trim() ?? string.Empty;
            }
        }

        if (string.IsNullOrEmpty(OBJECT_NAME))
        {
            throw new InvalidOperationException("找不到OBJECT_NAME，（OBJECT_ID為：" + _OBJECT_ID + "）");
        }
        if (string.IsNullOrEmpty(SCHEMA_NAME))
        {
            throw new InvalidOperationException("找不到SCHEMA_NAME，（OBJECT_ID為：" + _OBJECT_ID + "）");
        }
        switch (TYPE)
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

        #region Add/Update/Drop Prop
        SqlParameter[] paras =
        [
            new()
            {
                ParameterName = "SCHEMA_NAME",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Value = SCHEMA_NAME
            },
            new()
            {
                ParameterName = "OBJECT_NAME",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Value = OBJECT_NAME
            },
            new()
            {
                ParameterName = "TYPE",
                SqlDbType = System.Data.SqlDbType.Char,
                Value = TYPE
            },
            para_OBJECT_ID
        ];

        using TransactionScope transaction = new();
        int affectedRows = 0;
        foreach (DTO_prop prop in props)
        {
            comm.Parameters.Clear();
            comm.Parameters.AddRange(paras);
            switch (prop.Verb)
            {
                case PropVerb.add:
                    comm.Parameters.AddRange(
                    [
                        new()
                        {
                            ParameterName = "COLUMN_ID",
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = prop.COLUMN_ID
                        },
                        new()
                        {
                            ParameterName = "PROP_NAME",
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = prop.NAME
                        },
                        new()
                        {
                            ParameterName = "PROP_VALUE",
                            SqlDbType = System.Data.SqlDbType.Variant,
                            Value = prop.VALUE
                        },
                    ]);
                    comm.CommandText = SQLScripts.Addextendedproperty;
                    affectedRows += comm.ExecuteNonQuery();
                    if (affectedRows == 0)
                    {
                        throw new InvalidOperationException("第" + prop.COLUMN_ID + "欄，擴充屬性新增失敗" + NewLine +
                            "內容為：\"" + prop.VALUE + "\"");
                    }
                    break;
                case PropVerb.update:
                    comm.Parameters.AddRange(
                    [
                        new()
                        {
                            ParameterName = "COLUMN_ID",
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = prop.COLUMN_ID
                        },
                        new()
                        {
                            ParameterName = "PROP_NAME",
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = prop.NAME
                        },
                        new()
                        {
                            ParameterName = "PROP_VALUE",
                            SqlDbType = System.Data.SqlDbType.Variant,
                            Value = prop.VALUE
                        },
                    ]);
                    comm.CommandText = SQLScripts.Updateextendedproperty;
                    affectedRows += comm.ExecuteNonQuery();
                    if (affectedRows == 0)
                    {
                        throw new InvalidOperationException("第" + prop.COLUMN_ID + "欄，擴充屬性更新失敗" + NewLine +
                            "內容為：\"" + prop.VALUE + "\"");
                    }
                    break;
                case PropVerb.drop:
                    comm.Parameters.AddRange(
                    [
                        new()
                        {
                            ParameterName = "COLUMN_ID",
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = prop.COLUMN_ID
                        },
                        new()
                        {
                            ParameterName = "PROP_NAME",
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Value = prop.NAME
                        },
                    ]);
                    comm.CommandText = SQLScripts.Dropextendedproperty;
                    affectedRows += comm.ExecuteNonQuery();
                    if (affectedRows == 0)
                    {
                        throw new InvalidOperationException("第" + prop.COLUMN_ID + "欄，擴充屬性移除失敗");
                    }
                    break;
            }
        }
        transaction.Complete();
        #endregion
        return affectedRows;
    }

    internal void GetObjectExtendedProp(ref List<DTO_Object_prop> object_props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new(SQLScripts.GetObject_Extended_prop, conn);
        object_props = ExecSqlDataReader<DTO_Object_prop>(comm);
    }

    internal void GetObjectExtendedProp_NotEmpty(ref List<DTO_Object_prop> object_props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new(SQLScripts.GetObject_Extended_prop_NotEmpty, conn);
        object_props = ExecSqlDataReader<DTO_Object_prop>(comm);
    }

    internal void GetObjectExtendedProp_emptyValue(ref List<DTO_Object_prop> object_props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new(SQLScripts.GetObject_Extended_prop_emptyValue, conn);
        object_props = ExecSqlDataReader<DTO_Object_prop>(comm);
    }

    internal int DropAllProperties(List<DTO_Object_prop> object_props)
    {
        using SqlConnection conn = new(ConnectionString);
        conn.Open();

        using SqlCommand comm = new()
        {
            Connection = conn,
            CommandText = SQLScripts.Dropextendedproperty
        };

        using TransactionScope transaction = new();
        int affectedRows = 0;
        foreach (DTO_Object_prop prop in object_props)
        {
            string type = prop.TYPE?.Trim() switch
            {
                "U" => "TABLE",
                "V" => "VIEW",
                _ => prop.TYPE?.Trim() ?? string.Empty
            };

            comm.Parameters.Clear();
            comm.Parameters.AddRange(
            [
                new SqlParameter
                {
                    ParameterName = "OBJECT_ID",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Value = prop.OBJECT_ID
                },
                new SqlParameter
                {
                    ParameterName = "COLUMN_ID",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Value = prop.COLUMN_ID
                },
                new SqlParameter
                {
                    ParameterName = "PROP_NAME",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Value = prop.PROP_NAME
                },
                new SqlParameter
                {
                    ParameterName = "SCHEMA_NAME",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Value = prop.SCHEMA_NAME
                },
                new SqlParameter
                {
                    ParameterName = "TYPE",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Value = type
                },
                new SqlParameter
                {
                    ParameterName = "OBJECT_NAME",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Value = prop.NAME
                },
            ]);
            affectedRows += comm.ExecuteNonQuery();
        }
        transaction.Complete();
        return affectedRows;
    }
}

public class ADO_dot_NET2(string _ConnectionString) : ADO_dot_NET(_ConnectionString)
{
    public override List<T> ExecSqlDataReader<T>(SqlCommand comm)
    {
        using SqlDataReader dr = comm.ExecuteReader();
        if (dr.HasRows)
            return dr.ReadAll2<T>();
        else
            return [];
    }
}
public class ADO_dot_NET3(string _ConnectionString) : ADO_dot_NET(_ConnectionString)
{
    public override List<T> ExecSqlDataReader<T>(SqlCommand comm)
    {
        using SqlDataReader dr = comm.ExecuteReader();
        if (dr.HasRows)
            return dr.ReadAll3<T>();
        else
            return [];
    }
}
public class ADO_dot_NET4(string _ConnectionString) : ADO_dot_NET(_ConnectionString)
{
    public override List<T> ExecSqlDataReader<T>(SqlCommand comm)
    {
        using SqlDataReader dr = comm.ExecuteReader();
        if (dr.HasRows)
            return dr.ReadAll4<T>();
        else
            return [];
    }
}
