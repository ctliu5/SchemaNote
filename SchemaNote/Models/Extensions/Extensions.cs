using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace SchemaNote.Models.Extensions
{
    public static class Extensions
    {
        public static List<T> ReadAll<T>(this SqlDataReader dr) where T : new()
        {
            PropertyInfo[] propInfos = typeof(T).GetProperties();
            List<T> DTOs = new List<T>();
            int FieldCount = dr.FieldCount, dataQTY = 0;
            if (propInfos.Length != FieldCount) throw new EvaluateException("SqlData與指定的DTO物件無法對應欄位！欄位數量不一致。");
            while (dr.Read())
            {
                var DTO = new T();
                for (int i = 0; i < FieldCount; i++)
                {
                    PropertyInfo propInfo = propInfos.Where(n => n.Name == dr.GetName(i)).FirstOrDefault();
                    //sqlData欄位，沒有收錄到DTO中，則略過。
                    if (propInfo == null) continue;

                    Type t = propInfo.PropertyType;

                    if (t.IsEnum)
                    {
                        if (!dr.IsDBNull(i) && int.TryParse(dr[i].ToString(), out int e))
                            propInfo.SetValue(DTO, Enum.ToObject(t, e), null);
                        else
                            throw new EvaluateException("SqlData資料！無法轉為列舉型別。");
                    }
                    else if (dataQTY == 0 && dr.GetFieldType(i) != t)
                    {
                        throw new EvaluateException("DTO無法接受對應的SqlData資料！因為型別不一致。");
                    }
                    else if (t == typeof(DateTime))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetDateTime(i), null);
                    }
                    else if (t == typeof(int))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetInt32(i), null);
                    }
                    else if (t == typeof(double))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetDouble(i), null);
                    }
                    else if (t == typeof(long))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetInt64(i), null);
                    }
                    else if (t == typeof(decimal))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetDecimal(i), null);
                    }
                    else if (t == typeof(string))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetString(i)?.Trim(), null);
                    }
                    else if (t == typeof(byte))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetByte(i), null);
                    }
                    else if (t == typeof(short))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetInt16(i), null);
                    }
                    else if (t == typeof(DateTimeOffset))
                    {
                        if (!dr.IsDBNull(i))
                            propInfo.SetValue(DTO, dr.GetDateTimeOffset(i), null);
                    }
                    else if (!dr.IsDBNull(i))
                    {
                        propInfo.SetValue(DTO, dr[i], null);
                    }
                }
                DTOs.Add(DTO);
                dataQTY++;
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
    }
}
