﻿using Microsoft.AspNetCore.Http;
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
                    if (dataQTY == 0 && dr.GetFieldType(i) != t)
                    {
                        throw new EvaluateException("DTO無法接受對應的SqlData資料！因為型別不一致。");
                    }
                    if (t == typeof(DateTime))
                    {
                        propInfo.SetValue(DTO, dr.GetDateTime(i), null);
                    }
                    else if (t == typeof(decimal))
                    {
                        propInfo.SetValue(DTO, dr.GetDecimal(i), null);
                    }
                    else if (t == typeof(int))
                    {
                        propInfo.SetValue(DTO, dr.GetInt32(i), null);
                    }
                    else if (t == typeof(double))
                    {
                        propInfo.SetValue(DTO, dr.GetDouble(i), null);
                    }
                    else if (t == typeof(string))
                    {
                        propInfo.SetValue(DTO, dr.GetString(i)?.Trim(), null);
                    }
                    else if (t == typeof(long))
                    {
                        propInfo.SetValue(DTO, dr.GetInt64(i), null);
                    }
                    else if (t == typeof(short))
                    {
                        propInfo.SetValue(DTO, dr.GetInt16(i), null);
                    }
                    else if (t == typeof(DateTimeOffset))
                    {
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