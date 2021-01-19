using SchemaNote.Models;
using SchemaNote.Models.DataTransferObject;
using SchemaNote.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Transactions;

namespace SchemaNote.DB_Tools.Models
{
    /// <summary>
    /// <see cref="https://stackoverflow.com/questions/4439409/open-close-sqlconnection-or-keep-open"/>
    /// </summary>
    public class ADO_dot_NET
    {
        string ConnectionString { get; set; }

        public ADO_dot_NET(string _ConnectionString)
        {
            ConnectionString = _ConnectionString;
        }

        List<T> ExecSqlDataReader<T>(SqlCommand comm) where T : new()
        {
            using (SqlDataReader dr = comm.ExecuteReader())
            {
                return dr.ReadAll<T>();
            }
        }

        public void GetColumns(ref List<DTO_Column> cols)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;

                    #region GetColumns
                    comm.CommandText = SQLScripts.GetColumns;

                    cols = ExecSqlDataReader<DTO_Column>(comm);
                    #endregion
                }
                conn.Close();
            }
        }

        public void GetTables(ref List<DTO_Table> tbls)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;

                    #region GetTables
                    comm.CommandText = SQLScripts.GetTables;

                    tbls = ExecSqlDataReader<DTO_Table>(comm);
                    #endregion
                }
                conn.Close();
            }
        }

        public void GetExtended_prop(ref List<DTO_Extended_prop> props)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;

                    #region GetExtended_prop
                    comm.CommandText = SQLScripts.GetExtended_prop;

                    props = ExecSqlDataReader<DTO_Extended_prop>(comm);
                    #endregion
                }
                conn.Close();
            }
        }

        public void GetIndexes(ref List<DTO_Index> indexes)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;

                    #region GetIndexes
                    comm.CommandText = SQLScripts.GetIndexes;

                    indexes = ExecSqlDataReader<DTO_Index>(comm);
                    #endregion
                }
                conn.Close();
            }
        }

        public DTO_Flag<List<DTO_Column>> GetColumnsByOBJECT_ID(int _OBJECT_ID)
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

                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;
                    comm.Parameters.Add(para);

                    #region GetColumns By OBJECT_ID
                    comm.CommandText = SQLScripts.GetColumnsByObject_id;

                    ObjFlag.OBJ = ExecSqlDataReader<DTO_Column>(comm);
                    #endregion
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

        public DTO_Flag<List<DTO_Table>> GetTablesByOBJECT_ID(int _OBJECT_ID)
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

                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;
                    comm.Parameters.Add(para);

                    #region GetTables By OBJECT_ID
                    comm.CommandText = SQLScripts.GetTablesByObject_id;

                    ObjFlag.OBJ = ExecSqlDataReader<DTO_Table>(comm);
                    #endregion
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

        public DTO_Flag<int> SaveProperties(int _OBJECT_ID, List<DTO_prop> props)
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
                    }
                }
                #endregion

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
                                    ParameterName = "@PROP_VALUE",
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
                                    ParameterName = "@PROP_VALUE",
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
    }
}
