using SchemaNote.DB_Tools.Models;
using SchemaNote.Models.DataTransferObject;
using SchemaNote.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SchemaNote.Models
{
    public static class DB_Access
    {
        internal static OverviewViewModel GetTables_Columns(string ConnectionString)
        {
            List<DTO_Column> cols = new List<DTO_Column>();
            List<DTO_Table> tbls = new List<DTO_Table>();
            List<DTO_Extended_prop> props = new List<DTO_Extended_prop>();

            ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);

            ADO.GetColumns(ref cols);
            ADO.GetTables(ref tbls);
            ADO.GetExtended_prop(ref props);

            var obj = new OverviewViewModel
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
                        NAME_CHT = pT.Where(p => p.NAME == "NAME_CHT").FirstOrDefault()?.VALUE.ToString(),
                        REMARK = pT.Where(p => p.NAME == "REMARK").FirstOrDefault()?.VALUE.ToString(),
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
                                NAME_CHT = pC.Where(p => p.NAME == "NAME_CHT").FirstOrDefault()?.VALUE.ToString(),
                                REMARK = pC.Where(p => p.NAME == "REMARK").FirstOrDefault()?.VALUE.ToString(),
                            };
                        }).ToList()
                    };
                }).ToList()
            };
            return obj;
        }

        internal static DTO_Flag<DetailsViewModel> GetTable_Columns(string ConnectionString, int _OBJECT_ID)
        {
            var Flag = new DTO_Flag<DetailsViewModel>(MethodBase.GetCurrentMethod().Name);

            List<DTO_Extended_prop> props = new List<DTO_Extended_prop>();
            List<DTO_Index> indexes = new List<DTO_Index>();

            ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);

            var Flag_col = ADO.GetColumnsByOBJECT_ID(_OBJECT_ID);
            if (Flag_col.ResultType != ExceResultType.Success)
            {
                Flag_col.Transfer(ref Flag);
                return Flag;
            }

            var Flag_tbl = ADO.GetTablesByOBJECT_ID(_OBJECT_ID);
            if (Flag_tbl.ResultType != ExceResultType.Success)
            {
                Flag_tbl.Transfer(ref Flag);
                return Flag;
            }

            ADO.GetExtended_prop(ref props);
            ADO.GetIndexes(ref indexes);

            var tbl = Flag_tbl.OBJ.FirstOrDefault();

            var pObj = props.Where(p => p.MAJOR_ID == tbl.OBJECT_ID);
            var pT = pObj.Where(p => p.MINOR_ID == 0);
            var iObj = indexes.Where(i => i.OBJECT_ID == tbl.OBJECT_ID);
            Flag.OBJ = new DetailsViewModel()
            {
                OBJECT_ID = tbl.OBJECT_ID,
                NAME = tbl.NAME,
                SCHEMA_NAME = tbl.SCHEMA_NAME,
                TYPE = tbl.TYPE,
                CREATE_DATE = tbl.CREATE_DATE,
                MODIFY_DATE = tbl.MODIFY_DATE,
                QTY = tbl.QTY,
                NAME_CHT = pT.Where(p => p.NAME == "NAME_CHT").FirstOrDefault()?.VALUE.ToString(),
                REMARK = pT.Where(p => p.NAME == "REMARK").FirstOrDefault()?.VALUE.ToString(),
                Columns = Flag_col.OBJ.Select(c =>
                {
                    var pC = pObj.Where(p => p.MINOR_ID == c.COLUMN_ID);
                    return new ColumnDetail()
                    {
                        OBJECT_ID = c.OBJECT_ID,
                        COLUMN_ID = c.COLUMN_ID,
                        NAME = c.NAME,
                        TYPE_NAME = c.TYPE_NAME,
                        LENGTH = c.LENGTH,
                        IS_PK = c.IS_PK,
                        DISALLOW_NULL = c.DISALLOW_NULL,
                        DEFUALT = c.DEFUALT,
                        NAME_CHT = pC.Where(p => p.NAME == "NAME_CHT").FirstOrDefault()?.VALUE.ToString(),
                        REMARK = pC.Where(p => p.NAME == "REMARK").FirstOrDefault()?.VALUE.ToString(),
                        Indexes = iObj.Where(i => i.COLUMN_ID == c.COLUMN_ID).Select(i => new Index()
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

        internal static DTO_Flag<int> SaveProperties(string ConnectionString, int id, ICollection<VM_Property> model)
        {
            #region 取得物件的所有自訂屬性
            var props = new List<DTO_Extended_prop>();
            new ADO_dot_NET(ConnectionString).GetExtended_prop(ref props);
            var curr_props = props.Where(p => p.MAJOR_ID == id).ToList();
            #endregion

            #region 整理出要新刪修的屬性
            List<DTO_prop> dto_prop = new List<DTO_prop>();

            foreach (VM_Property vm in model)
            {
                if (vm.NAME_CHT != null)
                    dto_prop.Add(
                    new DTO_prop
                    {
                        COLUMN_ID = vm.COLUMN_ID,
                        NAME = "NAME_CHT",
                        VALUE = vm.NAME_CHT,
                    });

                if (vm.REMARK != null)
                    dto_prop.Add(
                    new DTO_prop
                    {
                        COLUMN_ID = vm.COLUMN_ID,
                        NAME = "REMARK",
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
                     Original_VALUE = p.FirstOrDefault()?.VALUE.ToString().Trim(),
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
            #endregion

            ADO_dot_NET ADO = new ADO_dot_NET(ConnectionString);
            return ADO.SaveProperties(id, Properties);
        }
    }
}