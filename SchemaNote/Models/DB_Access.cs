using SchemaNote.Models.DataTransferObject;
using SchemaNote.Models.DB_Tools;
using SchemaNote.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SchemaNote.Models
{
    public static class DB_Access
    {
        internal static DTO_Flag<OverviewViewModel> GetTables_Columns(string ConnectionString, DB_tool db_Tool)
        {
            var Flag = new DTO_Flag<OverviewViewModel>(MethodBase.GetCurrentMethod().Name);

            List<DTO_Column> cols = new List<DTO_Column>();
            List<DTO_Table> tbls = new List<DTO_Table>();
            List<DTO_Extended_prop> props = new List<DTO_Extended_prop>();

            Stopwatch sw = new Stopwatch();
            /* 測試效能
            long ADO_dot_NET = 0, Dapper = 0, ADO_dot_NET2 = 0;
            */

            try
            {
                switch (db_Tool)
                {
                    case DB_tool.Dapper:
                        ORM_Dapper dapper = new ORM_Dapper(ConnectionString);
                        dapper.GetColumns(ref cols);
                        dapper.GetTables(ref tbls);
                        dapper.GetExtended_prop(ref props);
                        break;
                    default:
                        ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);
                        ADO.GetColumns(ref cols);
                        ADO.GetTables(ref tbls);
                        ADO.GetExtended_prop(ref props);
                        break;
                }
                /* 測試效能
                for (int i = 0; i < 200; i++)
                {
                    sw.Start();
                    ADO_dot_NET2 ADO2 = new ADO_dot_NET2(ConnectionString);
                    ADO2.GetColumns(ref cols);
                    ADO2.GetTables(ref tbls);
                    ADO2.GetExtended_prop(ref props);
                    sw.Stop();
                    ADO_dot_NET2 += sw.ElapsedMilliseconds;
                    sw.Reset();

                    sw.Start();
                    ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);
                    ADO.GetColumns(ref cols);
                    ADO.GetTables(ref tbls);
                    ADO.GetExtended_prop(ref props);
                    sw.Stop();
                    ADO_dot_NET += sw.ElapsedMilliseconds;
                    sw.Reset();

                    sw.Start();
                    ORM_Dapper dapper = new ORM_Dapper(ConnectionString);
                    dapper.GetColumns(ref cols);
                    dapper.GetTables(ref tbls);
                    dapper.GetExtended_prop(ref props);
                    sw.Stop();
                    Dapper += sw.ElapsedMilliseconds;
                    sw.Reset();
                }
                */
            }
            catch (SqlException ex)
            {
                Flag.SetError(ex);
                return Flag;
            }
            catch (Exception ex)
            {
                Flag.SetError(ex);
                return Flag;
            }


            Flag.OBJ = new OverviewViewModel
            {
                Tables = tbls.Select(t =>
                {
                    var pObj = props.Where(p => p.MAJOR_ID == t.OBJECT_ID);
                    var pT = pObj.Where(p => p.MINOR_ID == 0);        
                    return new Table
                    {
                        OBJECT_ID = t.OBJECT_ID,
                        NAME = t.NAME,
                        SCHEMA_NAME = t.SCHEMA_NAME,
                        TYPE = t.TYPE,
                        CREATE_DATE = t.CREATE_DATE,
                        MODIFY_DATE = t.MODIFY_DATE,
                        QTY = t.QTY,
                        MS_Description = (pT.Where(p => p.NAME == Common.MS_Desc).FirstOrDefault()?.VALUE) is object ms_Description ? ms_Description.ToString() : null,
                        REMARK = (pT.Where(p => p.NAME == Common.Remark).FirstOrDefault()?.VALUE) is object remark ? remark.ToString() : null,
                        Columns = cols.Where(c => c.OBJECT_ID == t.OBJECT_ID).Select(c =>
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
                                MS_Description = (pC.Where(p => p.NAME == Common.MS_Desc).FirstOrDefault()?.VALUE) is object ms_Description ? ms_Description.ToString() : null,
                                REMARK = (pC.Where(p => p.NAME == Common.Remark).FirstOrDefault()?.VALUE) is object remark ? remark.ToString() : null,
                            };
                        }).ToList()
                    };
                }).ToList()
            };
            /* 測試效能
            Flag.OBJ.ADO_dot_NET = ADO_dot_NET;
            Flag.OBJ.Dapper = Dapper;
            Flag.OBJ.ADO_dot_NET2 = ADO_dot_NET2;
            */
            return Flag;
        }

        internal static DTO_Flag<DetailsViewModel> GetTable_Columns(string ConnectionString, int _OBJECT_ID, DB_tool db_Tool)
        {
            var Flag = new DTO_Flag<DetailsViewModel>(MethodBase.GetCurrentMethod().Name);

            List<DTO_Extended_prop> props = new List<DTO_Extended_prop>();
            List<DTO_Index> indexes = new List<DTO_Index>();
            DTO_Table tbl = new DTO_Table();
            List<DTO_Column> cols = new List<DTO_Column>();

            try
            {
                switch (db_Tool)
                {
                    case DB_tool.Dapper:
                        ORM_Dapper dapper = new ORM_Dapper(ConnectionString);
                        var Flag_a_col = dapper.GetColumnsByOBJECT_ID(_OBJECT_ID);
                        if (Flag_a_col.ResultType != ExceResultType.Success)
                        {
                            Flag_a_col.Transfer(ref Flag);
                            return Flag;
                        }

                        var Flag_a_tbl = dapper.GetTablesByOBJECT_ID(_OBJECT_ID);
                        if (Flag_a_tbl.ResultType != ExceResultType.Success)
                        {
                            Flag_a_tbl.Transfer(ref Flag);
                            return Flag;
                        }

                        dapper.GetExtended_prop(ref props);
                        dapper.GetIndexes(ref indexes);
                        tbl = Flag_a_tbl.OBJ.FirstOrDefault();
                        cols = Flag_a_col.OBJ;
                        break;
                    default:
                        ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);
                        var Flag_b_col = ADO.GetColumnsByOBJECT_ID(_OBJECT_ID);
                        if (Flag_b_col.ResultType != ExceResultType.Success)
                        {
                            Flag_b_col.Transfer(ref Flag);
                            return Flag;
                        }

                        var Flag_b_tbl = ADO.GetTablesByOBJECT_ID(_OBJECT_ID);
                        if (Flag_b_tbl.ResultType != ExceResultType.Success)
                        {
                            Flag_b_tbl.Transfer(ref Flag);
                            return Flag;
                        }

                        ADO.GetExtended_prop(ref props);
                        ADO.GetIndexes(ref indexes);
                        tbl = Flag_b_tbl.OBJ.FirstOrDefault();
                        cols = Flag_b_col.OBJ;
                        break;
                }
            }
            catch (SqlException ex)
            {
                Flag.SetError(ex);
                return Flag;
            }
            catch (Exception ex)
            {
                Flag.SetError(ex);
                return Flag;
            }

            var pObj = props.Where(p => p.MAJOR_ID == tbl.OBJECT_ID);
            var pT = pObj.Where(p => p.MINOR_ID == 0);
            var iObj = indexes.Where(i => i.OBJECT_ID == tbl.OBJECT_ID);
            int SortNum = 0;
            Flag.OBJ = new DetailsViewModel()
            {
                OBJECT_ID = tbl.OBJECT_ID,
                NAME = tbl.NAME,
                SCHEMA_NAME = tbl.SCHEMA_NAME,
                TYPE = tbl.TYPE,
                CREATE_DATE = tbl.CREATE_DATE,
                MODIFY_DATE = tbl.MODIFY_DATE,
                QTY = tbl.QTY,
                MS_Description = (pT.Where(p => p.NAME == Common.MS_Desc).FirstOrDefault()?.VALUE) is object ms_Description ? ms_Description.ToString() : null,
                REMARK = (pT.Where(p => p.NAME == Common.Remark).FirstOrDefault()?.VALUE) is object remark ? remark.ToString() : null,
                Columns = cols.Select(c =>
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
                        MS_Description = (pC.Where(p => p.NAME == Common.MS_Desc).FirstOrDefault()?.VALUE) is object ms_Description ? ms_Description.ToString() : null,
                        REMARK = (pC.Where(p => p.NAME == Common.Remark).FirstOrDefault()?.VALUE) is object remark ? remark.ToString() : null,
                        Indexes = iObj.Where(i => i.COLUMN_ID == c.COLUMN_ID).Select(i => new IndexDetail()
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
                        }).ToList()
                    };
                }).ToList()
            };
            return Flag;
        }

        internal static DTO_Flag<int> SaveProperties(string ConnectionString, int id, ICollection<VM_Property> model, DB_tool db_Tool)
        {
            var ObjFlag = new DTO_Flag<int>(MethodBase.GetCurrentMethod().Name);

            #region 取得物件的所有自訂屬性
            var props = new List<DTO_Extended_prop>();
            try
            {
                switch (db_Tool)
                {
                    case DB_tool.Dapper:
                        ORM_Dapper dapper = new ORM_Dapper(ConnectionString);
                        dapper.GetExtended_prop(ref props);
                        return dapper.SaveProperties(id, Categorized(model, props.Where(p => p.MAJOR_ID == id).ToList()));
                    default:
                        ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);
                        ADO.GetExtended_prop(ref props);
                        return ADO.SaveProperties(id, Categorized(model, props.Where(p => p.MAJOR_ID == id).ToList()));
                }

            }
            catch (SqlException ex)
            {
                ObjFlag.SetError(ex);
                return ObjFlag;
            }
            catch (Exception ex)
            {
                ObjFlag.SetError(ex);
                return ObjFlag;
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
            List<DTO_prop> dto_prop = new List<DTO_prop>();

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
            }

            IEnumerable<DTO_prop> UnionProp = dto_prop.GroupJoin(
                 curr_props,
                 d => new { d.COLUMN_ID, d.NAME },
                 p => new { COLUMN_ID = p.MINOR_ID, p.NAME },
                 (d, p) => new DTO_prop
                 {
                     COLUMN_ID = d.COLUMN_ID,
                     NAME = d.NAME,
                     VALUE = d.VALUE.Trim(),
                     Original_VALUE = (p.FirstOrDefault()?.VALUE) is object obj ? obj.ToString().Trim() : null,
                 });

            List<DTO_prop> Properties = new List<DTO_prop>();

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

        internal static DTO_Flag<StringBuilder> ExportPropertiesScript(string ConnectionString, DB_tool db_Tool, bool ForDeleteEmptyData = false)
        {
            var ObjFlag = new DTO_Flag<StringBuilder>(MethodBase.GetCurrentMethod().Name);

            List<DTO_Object_prop> object_props = new List<DTO_Object_prop>();
            string scriptTemp = ForDeleteEmptyData ? SQLScripts.DeleteScript_Extended_prop : SQLScripts.SavingScript_Extended_prop, newLine = Environment.NewLine;
            StringBuilder stringBuilder = new StringBuilder(
                "DECLARE @id int," + newLine +
                "        @col_id int," + newLine +
                "        @name sysname," + newLine +
                "        @value sql_variant," + newLine +
                "        @level0name sysname," + newLine +
                "        @level1type sysname," + newLine +
                "        @level1name sysname," + newLine +
                "        @level2name sysname," + newLine +
                "        @propQty int;" + newLine
                );

            #region 匯出擴充屬性資料
            try
            {
                if (ForDeleteEmptyData)
                    switch (db_Tool)
                    {
                        case DB_tool.Dapper:
                            ORM_Dapper dapper = new ORM_Dapper(ConnectionString);
                            dapper.GetObjectExtendedProp_emptyValue(ref object_props);
                            break;
                        default:
                            ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);
                            ADO.GetObjectExtendedProp_emptyValue(ref object_props);
                            break;
                    }
                else
                    switch (db_Tool)
                    {
                        case DB_tool.Dapper:
                            ORM_Dapper dapper = new ORM_Dapper(ConnectionString);
                            dapper.GetObjectExtendedProp(ref object_props);
                            break;
                        default:
                            ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);
                            ADO.GetObjectExtendedProp(ref object_props);
                            break;
                    }
                if (object_props.Count < 1)
                {
                    ObjFlag.ErrorMessages.Append("找不到擴充屬性");
                    ObjFlag.ResultType |= ExceResultType.Failed;
                    return ObjFlag;
                }
                foreach (var op in object_props)
                {
                    stringBuilder.Append(
                    string.Format(scriptTemp,
                        op.PROP_NAME,
                        op.PROP_VALUE.ToString().Replace("'", "''"),
                        op.SCHEMA_NAME,
                        op.TYPE,
                        op.NAME,
                        op.COLUMN_NAME
                        ));
                }
                ObjFlag.OBJ = stringBuilder;
            }
            catch (SqlException ex)
            {
                ObjFlag.SetError(ex);
                return ObjFlag;
            }
            catch (Exception ex)
            {
                ObjFlag.SetError(ex);
                return ObjFlag;
            }
            return ObjFlag;
            #endregion

        }
    }
}