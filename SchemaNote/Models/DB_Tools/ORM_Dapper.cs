using Dapper;
using SchemaNote.Models.DataTransferObject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Transactions;

namespace SchemaNote.Models.DB_Tools
{
    public class ORM_Dapper
    {
        string ConnectionString { get; set; }

        public ORM_Dapper(string _ConnectionString)
        {
            ConnectionString = _ConnectionString;
        }

        internal void GetColumns(ref List<DTO_Column> cols)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                cols = conn.Query<DTO_Column>(SQLScripts.GetColumns).ToList();
            }
        }

        internal void GetTables(ref List<DTO_Table> tbls)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                tbls = conn.Query<DTO_Table>(SQLScripts.GetTables).ToList();
            }
        }

        internal void GetExtended_prop(ref List<DTO_Extended_prop> props)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                props = conn.Query<DTO_Extended_prop>(SQLScripts.GetExtended_prop).ToList();
            }
        }

        internal void GetIndexes(ref List<DTO_Index> indexes)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                indexes = conn.Query<DTO_Index>(SQLScripts.GetIndexes).ToList();
            }
        }

        internal DTO_Flag<List<DTO_Column>> GetColumnsByOBJECT_ID(int _OBJECT_ID)
        {
            var ObjFlag = new DTO_Flag<List<DTO_Column>>(MethodBase.GetCurrentMethod().Name);
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                conn.Open();

                ObjFlag.OBJ = conn.Query<DTO_Column>(SQLScripts.GetColumnsByObject_id, new { OBJECT_ID = _OBJECT_ID }).ToList();
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
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                conn.Open();

                ObjFlag.OBJ = conn.Query<DTO_Table>(SQLScripts.GetTablesByObject_id, new { OBJECT_ID = _OBJECT_ID }).ToList();
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

        internal DTO_Flag<int> SaveProperties(int _OBJECT_ID, List<DTO_prop> props)
        {
            var ObjFlag = new DTO_Flag<int>(MethodBase.GetCurrentMethod().Name);

            string TYPE = "", NewLine = Environment.NewLine;

            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            using (IDbTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    #region GetSchema By OBJECT_ID
                    var schemaInfo = conn.QueryFirst<dynamic>(SQLScripts.GetSchema_ByObject_id, new { OBJECT_ID = _OBJECT_ID }, transaction, 3, CommandType.Text);
                    if (!(schemaInfo.OBJECT_NAME is string OBJECT_NAME) || string.IsNullOrEmpty(OBJECT_NAME))
                    {
                        ObjFlag.ErrorMessages.Append("找不到OBJECT_NAME，（OBJECT_ID為：" + _OBJECT_ID + "）");
                        ObjFlag.ResultType |= ExceResultType.Failed;
                        return ObjFlag;
                    }
                    if (!(schemaInfo.SCHEMA_NAME is string SCHEMA_NAME) || string.IsNullOrEmpty(SCHEMA_NAME))
                    {
                        ObjFlag.ErrorMessages.Append("找不到SCHEMA_NAME，（OBJECT_ID為：" + _OBJECT_ID + "）");
                        ObjFlag.ResultType |= ExceResultType.Failed;
                        return ObjFlag;
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
                            ObjFlag.ErrorMessages.Append("找不到TYPE，（OBJECT_ID為：" + _OBJECT_ID + "）");
                            ObjFlag.ResultType |= ExceResultType.Failed;
                            return ObjFlag;
                    }
                    #endregion

                    #region  Add/Update/Drop Prop
                    foreach (DTO_prop prop in props)
                    {
                        switch (prop.Verb)
                        {
                            case PropVerb.add:
                                ObjFlag.OBJ += conn.Execute(SQLScripts.Addextendedproperty, new
                                {
                                    OBJECT_ID = _OBJECT_ID,
                                    SCHEMA_NAME,
                                    OBJECT_NAME,
                                    TYPE,
                                    prop.COLUMN_ID,
                                    PROP_NAME = prop.NAME,
                                    PROP_VALUE = prop.VALUE
                                }, transaction, 3, CommandType.Text);
                                if (ObjFlag.OBJ == 0)
                                {
                                    transaction.Rollback();
                                    ObjFlag.SetError("第" + prop.COLUMN_ID + "欄，擴充屬性新增失敗" + NewLine +
                                        "內容為：\"" + prop.VALUE + "\"");
                                    return ObjFlag;
                                }
                                break;
                            case PropVerb.update:
                                ObjFlag.OBJ += conn.Execute(SQLScripts.Updateextendedproperty, new
                                {
                                    OBJECT_ID = _OBJECT_ID,
                                    SCHEMA_NAME,
                                    OBJECT_NAME,
                                    TYPE,
                                    prop.COLUMN_ID,
                                    PROP_NAME = prop.NAME,
                                    PROP_VALUE = prop.VALUE
                                }, transaction, 3, CommandType.Text);
                                if (ObjFlag.OBJ == 0)
                                {
                                    transaction.Rollback();
                                    ObjFlag.SetError("第" + prop.COLUMN_ID + "欄，擴充屬性更新失敗" + NewLine +
                                        "內容為：\"" + prop.VALUE + "\"");
                                    return ObjFlag;
                                }
                                break;
                            case PropVerb.drop:
                                ObjFlag.OBJ += conn.Execute(SQLScripts.Dropextendedproperty, new
                                {
                                    OBJECT_ID = _OBJECT_ID,
                                    SCHEMA_NAME,
                                    OBJECT_NAME,
                                    TYPE,
                                    prop.COLUMN_ID,
                                    PROP_NAME = prop.NAME,
                                }, transaction, 3, CommandType.Text);
                                if (ObjFlag.OBJ == 0)
                                {
                                    transaction.Rollback();
                                    ObjFlag.SetError("第" + prop.COLUMN_ID + "欄，擴充屬性移除失敗" + NewLine +
                                        "內容為：\"" + prop.VALUE + "\"");
                                    return ObjFlag;
                                }
                                break;
                        }
                    }
                    #endregion
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    ObjFlag.SetError(ex);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ObjFlag.SetError(ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return ObjFlag;
        }
    }
}
