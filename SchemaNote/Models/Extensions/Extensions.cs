using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OfficeOpenXml;
using SchemaNote.ViewModels;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using SchemaNote.Models.myAttribute;

namespace SchemaNote.Models.Extensions
{
  public static class Extensions
  {
    public static List<T> ReadAll<T>(this SqlDataReader dr) where T : new()
    {
      PropertyInfo[] propInfos = typeof(T).GetProperties();
      List<T> DTOs = new List<T>();
      int FieldCount = dr.FieldCount;
      List<Mapper<T>> MappingRules = new List<Mapper<T>>();
      for (int i = 0; i < FieldCount; i++)
      {
        foreach (PropertyInfo propInfo in propInfos)
        {
          if (propInfo.Name == dr.GetName(i))
          {
            MappingRules.Add(new Mapper<T>(propInfo, dr.GetFieldType(i), i));
            break;
          }
        }
      }

      while (dr.Read())
      {
        var DTO = new T();

        //有對應到的欄位，嘗試指派
        foreach (Mapper<T> mapRule in MappingRules)
        {
          mapRule.Assign(DTO, dr);
        }
        //沒對應到的欄位，自然略過，使用預設值

        DTOs.Add(DTO);
      }
      return DTOs;
    }

    public static List<T> ReadAll2<T>(this SqlDataReader dr) where T : new()
    {
      PropertyInfo[] propInfos = typeof(T).GetProperties();
      List<T> DTOs = new List<T>();
      int FieldCount = dr.FieldCount;
      List<MappingSetting<T>> MappingRules = new List<MappingSetting<T>>();
      for (int i = 0; i < FieldCount; i++)
      {
        foreach (PropertyInfo propInfo in propInfos)
        {
          if (propInfo.Name == dr.GetName(i))
          {
            MappingRules.Add(new MappingSetting<T>(propInfo, dr.GetFieldType(i), i));
            break;
          }
        }
      }

      while (dr.Read())
      {
        var DTO = new T();

        //有對應到的欄位，嘗試指派
        foreach (MappingSetting<T> mapRule in MappingRules)
        {
          mapRule.Assign(DTO, dr);
        }
        //沒對應到的欄位，自然略過，使用預設值
        DTOs.Add(DTO);
      }
      return DTOs;
    }

    public static void SetObject<T>(this ISession session, string key, T value)
    {
      session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T GetObject<T>(this ISession session, string key)
    {
      var value = session.GetString(key);
      return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }

    public static ExcelPackage GetExcel(this OverviewViewModel p_source)
    {

      ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
      ExcelPackage result = new ExcelPackage();

      int columnLocation = 1;
      int rowLocation = 1;

      var Table_prop_forExcel = typeof(Table).GetProperties()
        .Where(n => n.IsDefined(typeof(ExcelAttribute)) == true)
        .OrderBy(n => ((ExcelAttribute)n.GetCustomAttribute(typeof(ExcelAttribute))).Rank);

      var Column_prop_forExcel = typeof(Column).GetProperties().Where(n => n.IsDefined(typeof(ExcelAttribute)) == true)
        .Where(n => n.IsDefined(typeof(ExcelAttribute)) == true)
        .OrderBy(n => ((ExcelAttribute)n.GetCustomAttribute(typeof(ExcelAttribute))).Rank);

      foreach (var table in p_source.Tables)
      {
        var sheet = result.Workbook.Worksheets.Add(table.NAME);
        sheet.Cells[rowLocation, columnLocation, rowLocation, columnLocation + Table_prop_forExcel.Count() - 1].Merge = true;
        sheet.Cells[rowLocation, columnLocation].Value = "Table";
        rowLocation++;

        foreach (var property in Table_prop_forExcel)
        {
          sheet.Cells[rowLocation, columnLocation].Value = property.Name;
          sheet.Cells[rowLocation + 1, columnLocation].Value = property.GetValue(table).ToString();

          columnLocation++;
        }
        columnLocation = 1;
        rowLocation += 2;

        sheet.Cells[rowLocation, columnLocation, rowLocation, columnLocation + Column_prop_forExcel.Count() - 1].Merge = true;
        sheet.Cells[rowLocation, columnLocation].Value = "Columns";
        rowLocation++;

        foreach (var property in Column_prop_forExcel)
        {

          sheet.Cells[rowLocation, columnLocation].Value = property.Name;
          int newRowLocation = 1;
          foreach (var column in table.Columns)
          {
            sheet.Cells[rowLocation + newRowLocation, columnLocation].Value = property.GetValue(column).ToString();
            newRowLocation++;
          }
          columnLocation++;
        }
        columnLocation = 1;
        rowLocation = 1;
      }
      return result;
    }
  }
}
