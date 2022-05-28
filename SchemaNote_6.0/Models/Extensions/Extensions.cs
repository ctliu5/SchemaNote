using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
