using SchemaNote.Models.DataTransferObject;
using SchemaNote.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Transactions;

namespace SchemaNote.Models.DB_Tools
{
    /// <summary>
    /// <see cref="https://stackoverflow.com/questions/4439409/open-close-sqlconnection-or-keep-open"/>
    /// </summary>
    public class ADO_dot_NET
    {
        protected string ConnectionString { get; set; }

        public ADO_dot_NET(string _ConnectionString)
        {
            ConnectionString = _ConnectionString;
        }

        public virtual List<T> ExecSqlDataReader<T>(SqlCommand comm) where T : new()
        {
            using (SqlDataReader dr = comm.ExecuteReader())
            {
                if (dr.HasRows)
                    return dr.ReadAll<T>();
                else
                    return new List<T>();
            }
        }

        internal void GetColumns(ref List<DTO_Column> cols)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetColumns, conn))
                {
                    cols = ExecSqlDataReader<DTO_Column>(comm);
                }
            }
        }

        internal void GetTables(ref List<DTO_Table> tbls)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetTables, conn))
                {
                    tbls = ExecSqlDataReader<DTO_Table>(comm);
                }
            }
        }

        internal void GetExtended_prop(ref List<DTO_Extended_prop> props)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetExtended_prop, conn))
                {
                    props = ExecSqlDataReader<DTO_Extended_prop>(comm);
                }
            }
        }

        internal void GetIndexes(ref List<DTO_Index> indexes)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetIndexes, conn))
                {
                    indexes = ExecSqlDataReader<DTO_Index>(comm);
                }
            }
        }

        internal DTO_Flag<List<DTO_Column>> GetColumnsByOBJECT_ID(int _OBJECT_ID)
        {
            var ObjFlag = new DTO_Flag<List<DTO_Column>>(MethodBase.GetCurrentMethod().Name);
            SqlParameter para = new SqlParameter()
            {
                ParameterName = "OBJECT_ID",
                SqlDbType = System.Data.SqlDbType.Int,
                Value = _OBJECT_ID
            };

            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetColumnsByObject_id, conn))
                {
                    comm.Parameters.Add(para);
                    ObjFlag.OBJ = ExecSqlDataReader<DTO_Column>(comm);
                }
                if (ObjFlag.OBJ.Count < 1)
                {
                    ObjFlag.ErrorMessages.Append("找不到資料欄位，（OBJECT_ID為：" + _OBJECT_ID + "）");
                    ObjFlag.ResultType |= ExceResultType.Failed;
                    return ObjFlag;
                }
            }
            catch (SqlException ex)
            {
                ObjFlag.SetError(ex);
            }
            catch (Exception ex)
            {
                ObjFlag.SetError(ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return ObjFlag;
        }

        internal DTO_Flag<List<DTO_Table>> GetTablesByOBJECT_ID(int _OBJECT_ID)
        {
            var ObjFlag = new DTO_Flag<List<DTO_Table>>(MethodBase.GetCurrentMethod().Name);
            SqlParameter para = new SqlParameter()
            {
                ParameterName = "OBJECT_ID",
                SqlDbType = System.Data.SqlDbType.Int,
                Value = _OBJECT_ID
            };

            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetTablesByObject_id, conn))
                {
                    comm.Parameters.Add(para);
                    ObjFlag.OBJ = ExecSqlDataReader<DTO_Table>(comm);
                }
                if (ObjFlag.OBJ.Count < 1)
                {
                    ObjFlag.ErrorMessages.Append("找不到資料表，（OBJECT_ID為：" + _OBJECT_ID + "）");
                    ObjFlag.ResultType |= ExceResultType.Failed;
                    return ObjFlag;
                }
            }
            catch (SqlException ex)
            {
                ObjFlag.SetError(ex);
            }
            catch (Exception ex)
            {
                ObjFlag.SetError(ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return ObjFlag;
        }

        internal DTO_Flag<int> SaveProperties(int _OBJECT_ID, List<DTO_prop> props)
        {
            var ObjFlag = new DTO_Flag<int>(MethodBase.GetCurrentMethod().Name);
            SqlParameter para_OBJECT_ID = new SqlParameter()
            {
                ParameterName = "OBJECT_ID",
                SqlDbType = System.Data.SqlDbType.Int,
                Value = _OBJECT_ID
            };

            string
                SCHEMA_NAME = "",
                OBJECT_NAME = "",
                TYPE = "",
                NewLine = Environment.NewLine;

            SqlConnection conn = new SqlConnection(ConnectionString);
            SqlCommand comm = new SqlCommand();

            try
            {
                comm.Connection = conn;
                comm.Parameters.Add(para_OBJECT_ID);
                conn.Open();

                #region GetSchema By OBJECT_ID
                comm.CommandText = SQLScripts.GetSchema_ByObject_id;
                using (SqlDataReader dr = comm.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OBJECT_NAME = dr["OBJECT_NAME"].ToString();
                        SCHEMA_NAME = dr["SCHEMA_NAME"].ToString();
                        TYPE = dr["TYPE"].ToString().Trim();
                    }
                }

                if (string.IsNullOrEmpty(OBJECT_NAME))
                {
                    ObjFlag.ErrorMessages.Append("找不到OBJECT_NAME，（OBJECT_ID為：" + _OBJECT_ID + "）");
                    ObjFlag.ResultType |= ExceResultType.Failed;
                    return ObjFlag;
                }
                if (string.IsNullOrEmpty(SCHEMA_NAME))
                {
                    ObjFlag.ErrorMessages.Append("找不到SCHEMA_NAME，（OBJECT_ID為：" + _OBJECT_ID + "）");
                    ObjFlag.ResultType |= ExceResultType.Failed;
                    return ObjFlag;
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
                        ObjFlag.ErrorMessages.Append("找不到TYPE，（OBJECT_ID為：" + _OBJECT_ID + "）");
                        ObjFlag.ResultType |= ExceResultType.Failed;
                        return ObjFlag;
                }
                #endregion

                #region Add/Update/Drop Prop
                SqlParameter[] paras = new SqlParameter[]
                {
                    new SqlParameter()
                    {
                        ParameterName = "SCHEMA_NAME",
                        SqlDbType = System.Data.SqlDbType.NVarChar,
                        Value = SCHEMA_NAME
                    },
                    new SqlParameter()
                    {
                        ParameterName = "OBJECT_NAME",
                        SqlDbType = System.Data.SqlDbType.NVarChar,
                        Value = OBJECT_NAME
                    },
                    new SqlParameter()
                    {
                        ParameterName = "TYPE",
                        SqlDbType = System.Data.SqlDbType.Char,
                        Value = TYPE
                    },
                    para_OBJECT_ID
                };
                using (TransactionScope transaction = new TransactionScope())
                {
                    foreach (DTO_prop prop in props)
                    {
                        comm.Parameters.Clear();
                        comm.Parameters.AddRange(paras);
                        switch (prop.Verb)
                        {
                            case PropVerb.add:
                                comm.Parameters.AddRange(
                                new SqlParameter[]
                                {
                                new SqlParameter()
                                {
                                    ParameterName = "COLUMN_ID",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Value = prop.COLUMN_ID
                                },
                                new SqlParameter()
                                {
                                    ParameterName = "PROP_NAME",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Value = prop.NAME
                                },
                                new SqlParameter()
                                {
                                    ParameterName = "PROP_VALUE",
                                    SqlDbType = System.Data.SqlDbType.Variant,
                                    Value = prop.VALUE
                                },
                                });
                                comm.CommandText = SQLScripts.Addextendedproperty;
                                ObjFlag.OBJ += comm.ExecuteNonQuery();
                                if (ObjFlag.OBJ == 0)
                                {
                                    ObjFlag.SetError("第" + prop.COLUMN_ID + "欄，擴充屬性新增失敗" + NewLine +
                                        "內容為：\"" + prop.VALUE + "\"");
                                    return ObjFlag;
                                }
                                break;
                            case PropVerb.update:
                                comm.Parameters.AddRange(
                                new SqlParameter[]
                                {
                                new SqlParameter()
                                {
                                    ParameterName = "COLUMN_ID",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Value = prop.COLUMN_ID
                                },
                                new SqlParameter()
                                {
                                    ParameterName = "PROP_NAME",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Value = prop.NAME
                                },
                                new SqlParameter()
                                {
                                    ParameterName = "PROP_VALUE",
                                    SqlDbType = System.Data.SqlDbType.Variant,
                                    Value = prop.VALUE
                                },
                                });
                                comm.CommandText = SQLScripts.Updateextendedproperty;
                                ObjFlag.OBJ += comm.ExecuteNonQuery();
                                if (ObjFlag.OBJ == 0)
                                {
                                    ObjFlag.SetError("第" + prop.COLUMN_ID + "欄，擴充屬性更新失敗" + NewLine +
                                        "內容為：\"" + prop.VALUE + "\"");
                                    return ObjFlag;
                                }
                                break;
                            case PropVerb.drop:
                                comm.Parameters.AddRange(
                                new SqlParameter[]
                                {
                                new SqlParameter()
                                {
                                    ParameterName = "COLUMN_ID",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Value = prop.COLUMN_ID
                                },
                                new SqlParameter()
                                {
                                    ParameterName = "PROP_NAME",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Value = prop.NAME
                                },
                                });
                                comm.CommandText = SQLScripts.Dropextendedproperty;
                                ObjFlag.OBJ += comm.ExecuteNonQuery();
                                if (ObjFlag.OBJ == 0)
                                {
                                    ObjFlag.SetError("第" + prop.COLUMN_ID + "欄，擴充屬性移除失敗");
                                    return ObjFlag;
                                }
                                break;
                        }
                    }
                    transaction.Complete();
                }
                #endregion
            }
            catch (SqlException ex)
            {
                ObjFlag.SetError(ex);
            }
            catch (Exception ex)
            {
                ObjFlag.SetError(ex);
            }
            finally
            {
                comm.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return ObjFlag;
        }

        public virtual List<T> ExecSqlDataReader2<T>(SqlCommand comm) where T : new()
        {
            using (SqlDataReader dr = comm.ExecuteReader())
            {
                if (dr.HasRows)
                    return dr.ReadAll2<T>().ToList();
                else
                    return new List<T>();
            }
        }

        internal void GetColumns2(ref List<DTO_Column> cols)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetColumns, conn))
                {
                    cols = ExecSqlDataReader2<DTO_Column>(comm);
                }
            }
        }

        internal void GetTables2(ref List<DTO_Table> tbls)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetTables, conn))
                {
                    tbls = ExecSqlDataReader2<DTO_Table>(comm);
                }
            }
        }

        internal void GetExtended_prop2(ref List<DTO_Extended_prop> props)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand(SQLScripts.GetExtended_prop, conn))
                {
                    props = ExecSqlDataReader2<DTO_Extended_prop>(comm);
                }
            }
        }
    }
}
