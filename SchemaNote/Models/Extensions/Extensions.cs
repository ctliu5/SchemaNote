using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.InteropServices;

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

        public static List<T> ReadAll3<T>(this SqlDataReader dr) where T : new()
        {
            List<T> DTOs = new List<T>();
            PropertyInfo[] propInfos = typeof(T).GetProperties();
            int FieldCount = dr.FieldCount;
            string[] Fields = new string[FieldCount];
            for (int i = 0; i < FieldCount; i++) { Fields[i] = dr.GetName(i); }

            while (dr.Read())
            {
                var dto = new T();
                foreach (PropertyInfo PropInfo in propInfos)
                {
                    foreach (string field in Fields)
                    {
                        Type PropType = PropInfo.PropertyType;
                        Type DataType = dr.GetFieldType(field);
                        if (PropType == DataType)
                        {
                            switch (Type.GetTypeCode(DataType))
                            {
                                case TypeCode.Boolean:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetBoolean(field));
                                    break;
                                case TypeCode.Char:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetChar(field));
                                    break;
                                case TypeCode.Byte:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetByte(field));
                                    break;
                                //case TypeCode.SByte: break;
                                //case TypeCode.UInt16: break;
                                case TypeCode.Int16:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetInt16(field));
                                    break;
                                //case TypeCode.UInt32: break;
                                case TypeCode.Int32:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetInt32(field));
                                    break;
                                //case TypeCode.UInt64: break;
                                case TypeCode.Int64:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetInt64(field));
                                    break;
                                case TypeCode.Single:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetFloat(field));
                                    break;
                                case TypeCode.Double:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetDouble(field));
                                    break;
                                case TypeCode.Decimal:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetDecimal(field));
                                    break;
                                case TypeCode.String:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetString(field).Trim());
                                    break;
                                case TypeCode.DateTime:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetDateTime(field));
                                    break;
                                default:
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr[field]);
                                    break;
                            }
                        }
                        else if (DataType.IsValueType)
                        {
                            if (PropType.IsEnum)
                            {
                                if (!dr.IsDBNull(field))
                                    PropInfo.SetValue(dto, Enum.ToObject(PropType, dr[field]));
                            }
                            else if (PropType.IsValueType)
                            {
                                bool CanAccommodate;
                                unsafe
                                {
                                    CanAccommodate = Marshal.SizeOf(PropType) <= Marshal.SizeOf(DataType);
                                }
                                if (CanAccommodate)
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, Convert.ChangeType(dr[field], PropType));
                                }
                                else throw new EvaluateException("實值型別[" + PropType.ToString() + "]的大小，小於資料庫欄位轉換後型別[" + DataType + "]的大小。");
                            }
                        }

                    }
                }
                DTOs.Add(dto);
            }
            return DTOs;
        }

        public static List<T> ReadAll4<T>(this SqlDataReader dr) where T : new()
        {
            List<T> DTOs = new List<T>();
            PropertyInfo[] propInfos = typeof(T).GetProperties();
            int FieldCount = dr.FieldCount;
            string[] Fields = new string[FieldCount];
            for (int i = 0; i < FieldCount; i++) { Fields[i] = dr.GetName(i); }

            while (dr.Read())
            {
                var dto = new T();
                foreach (PropertyInfo PropInfo in propInfos)
                {
                    foreach (string field in Fields)
                    {
                        Type PropType = PropInfo.PropertyType;
                        Type DataType = dr.GetFieldType(field);
                        if (PropType == DataType)
                        {
                            if (DataType.IsValueType)
                            {
                                if (PropType == typeof(bool))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetBoolean(field));
                                }
                                else if (PropType == typeof(char))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetChar(field));
                                }
                                else if (PropType == typeof(byte))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetByte(field));
                                }
                                else if (PropType == typeof(short))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetInt16(field));
                                }
                                else if (PropType == typeof(int))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetInt32(field));
                                }
                                else if (PropType == typeof(long))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetInt64(field));
                                }
                                else if (PropType == typeof(float))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetFloat(field));
                                }
                                else if (PropType == typeof(double))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetDouble(field));
                                }
                                else if (PropType == typeof(decimal))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetDecimal(field));
                                }
                                else if (PropType == typeof(DateTime))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetDateTime(field));
                                }
                            }
                            else
                            {
                                if (PropType == typeof(string))
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr.GetString(field).Trim());
                                }
                                else
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, dr[field]);
                                }
                            }
                        }
                        else if (DataType.IsValueType)
                        {
                            if (PropType.IsEnum)
                            {
                                if (!dr.IsDBNull(field))
                                    PropInfo.SetValue(dto, Enum.ToObject(PropType, dr[field]));
                            }
                            else if (PropType.IsValueType)
                            {
                                bool CanAccommodate;
                                unsafe
                                {
                                    CanAccommodate = Marshal.SizeOf(PropType) <= Marshal.SizeOf(DataType);
                                }
                                if (CanAccommodate)
                                {
                                    if (!dr.IsDBNull(field))
                                        PropInfo.SetValue(dto, Convert.ChangeType(dr[field], PropType));
                                }
                                else throw new EvaluateException("實值型別[" + PropType.ToString() + "]的大小，小於資料庫欄位轉換後型別[" + DataType + "]的大小。");
                            }
                        }
                    }
                }
                DTOs.Add(dto);
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
