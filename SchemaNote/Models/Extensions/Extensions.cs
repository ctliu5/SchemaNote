using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SchemaNote.Models.Extensions
{
    public static class Extensions
    {
        class MappingSetting<T>
        {
            internal MappingSetting(PropertyInfo propInfo, Type dataType, int index)
            {
                PropInfo = propInfo;
                PropType = PropInfo.PropertyType;
                DataType = dataType;
                Index = index;

                if (PropType == DataType)
                {
                    if (DataType.IsValueType)
                    {
                        if (PropType == typeof(bool))
                        {
                            Assign = Assign_bool;
                        }
                        else if (PropType == typeof(char))
                        {
                            Assign = Assign_char;
                        }
                        else if (PropType == typeof(byte))
                        {
                            Assign = Assign_byte;
                        }
                        else if (PropType == typeof(short))
                        {
                            Assign = Assign_short;
                        }
                        else if (PropType == typeof(int))
                        {
                            Assign = Assign_int;
                        }
                        else if (PropType == typeof(long))
                        {
                            Assign = Assign_long;
                        }
                        else if (PropType == typeof(float))
                        {
                            Assign = Assign_float;
                        }
                        else if (PropType == typeof(double))
                        {
                            Assign = Assign_double;
                        }
                        else if (PropType == typeof(decimal))
                        {
                            Assign = Assign_decimal;
                        }
                    }
                    else
                    {
                        if (PropType == typeof(string))
                        {
                            Assign = Assign_String;
                        }
                        else if (PropType == typeof(DateTime))
                        {
                            Assign = Assign_DateTime;
                        }
                        else if (PropType == typeof(DateTimeOffset))
                        {
                            Assign = Assign_DateTimeOffset;
                        }
                        else if (PropType == typeof(TimeSpan))
                        {
                            Assign = Assign_TimeSpan;
                        }
                        else
                        {
                            Assign = Assign_SameType;
                        }
                    }
                }
                else if (DataType.IsValueType)
                {
                    if (PropType.IsEnum)
                    {
                        Assign = Assign_forEnum;
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
                            Assign = Assign_ValueType;
                        }
                        else throw new EvaluateException("實值型別[" + PropType.ToString() + "]的大小，小於資料庫欄位轉換後型別[" + DataType + "]的大小。");
                    }
                }
                else
                {
                    Assign = Assign_DifferentType;
                }
            }

            readonly PropertyInfo PropInfo;
            readonly Type PropType;
            readonly Type DataType;
            readonly int Index;
            internal Action<T, SqlDataReader> Assign { get; set; }

            void Assign_bool(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetBoolean(Index));
            }
            void Assign_char(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetChar(Index));
            }
            void Assign_byte(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetByte(Index));
            }
            void Assign_short(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetInt16(Index));
            }
            void Assign_int(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetInt32(Index));
            }
            void Assign_long(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetInt64(Index));
            }
            void Assign_float(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetFloat(Index));
            }
            void Assign_double(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetDouble(Index));
            }
            void Assign_decimal(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetDecimal(Index));
            }

            void Assign_String(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetString(Index).Trim());
            }
            void Assign_DateTime(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetDateTime(Index));
            }
            void Assign_DateTimeOffset(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetDateTimeOffset(Index));
            }
            void Assign_TimeSpan(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr.GetTimeSpan(Index));
            }
            void Assign_SameType(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, dr[Index]);
            }

            void Assign_forEnum(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, Enum.ToObject(PropType, dr[Index]));
            }
            void Assign_ValueType(T dto, SqlDataReader dr)
            {
                if (!dr.IsDBNull(Index))
                    PropInfo.SetValue(dto, Convert.ChangeType(dr[Index], PropType));
            }
            void Assign_DifferentType(T dto, SqlDataReader dr)
            {
                //do nothing.
            }
        }

        public static List<T> ReadAll<T>(this SqlDataReader dr) where T : new()
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

        public static IEnumerable<T> ReadAll2<T>(this SqlDataReader dr) where T : new()
        {
            PropertyInfo[] propInfos = typeof(T).GetProperties();
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
                yield return DTO;
            }
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
