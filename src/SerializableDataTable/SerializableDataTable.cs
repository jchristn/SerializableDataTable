namespace SerializableDataTables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// Serializable data table.
    /// </summary>
    public class SerializableDataTable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        /// <summary>
        /// Name of the data table.
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Columns.
        /// </summary>
        public List<SerializableColumn> Columns
        {
            get
            {
                return _Columns;
            }
            set
            {
                if (value == null) value = new List<SerializableColumn>();
                _Columns = value;
            }
        }

        /// <summary>
        /// Rows.
        /// </summary>
        public List<Dictionary<string, object>> Rows
        {
            get
            {
                return _Rows;
            }
            set
            {
                if (value == null) value = new List<Dictionary<string, object>>();
                _Rows = value;
            }
        }

        private List<SerializableColumn> _Columns = new List<SerializableColumn>();
        private List<Dictionary<string, object>> _Rows = new List<Dictionary<string, object>>();

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="name">Name.</param>
        public SerializableDataTable(string name = null)
        {
            if (!String.IsNullOrEmpty(name)) Name = name;
        }

        /// <summary>
        /// Convert from a DataTable object.
        /// </summary>
        /// <param name="dt">DataTable.</param>
        /// <returns>SerializableDataTable.</returns>
        public static SerializableDataTable FromDataTable(DataTable dt)
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt));

            SerializableDataTable ret = new SerializableDataTable();
            ret.Name = dt.TableName;

            foreach (DataColumn col in dt.Columns)
            {
                SerializableColumn serCol = new SerializableColumn
                {
                    Name = col.ColumnName,
                    Type = DataTypeToColumnValueTypeEnum(col.DataType)
                };

                // If the column type is Object, check if actual values are arrays
                // and capture the full array type
                if (col.DataType == typeof(object))
                {
                    Type arrayType = DetectArrayType(dt, col.ColumnName);
                    if (arrayType != null)
                    {
                        serCol.OriginalType = arrayType.AssemblyQualifiedName;
                    }
                }
                // For explicit array types, also store the full type (except byte[] which is already handled)
                else if (col.DataType.IsArray)
                {
                    Type elementType = col.DataType.GetElementType();
                    if (elementType != null && elementType != typeof(byte))
                    {
                        serCol.OriginalType = col.DataType.AssemblyQualifiedName;
                    }
                }

                ret.Columns.Add(serCol);
            }

            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> val = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    object cellValue = row[col.ColumnName];
                    if (cellValue == DBNull.Value || cellValue == null)
                    {
                        val.Add(col.ColumnName, null);
                    }
                    else
                    {
                        val.Add(col.ColumnName, cellValue);
                    }
                }

                ret.Rows.Add(val);
            }

            return ret;
        }

        /// <summary>
        /// Convert to a DataTable object.
        /// </summary>
        /// <returns>DataTable.</returns>
        public DataTable ToDataTable()
        {
            DataTable ret = new DataTable(Name);

            foreach (SerializableColumn col in Columns)
            {
                ret.Columns.Add(new DataColumn
                {
                    ColumnName = col.Name,
                    DataType = ColumnValueTypeEnumToDataType(col.Type)
                });
            }

            for (int i = 0; i < Rows.Count; i++)
            {
                Dictionary<string, object> dict = Rows[i];

                DataRow row = ret.NewRow();

                foreach (KeyValuePair<string, object> val in dict)
                {
                    SerializableColumn col = Columns.FirstOrDefault(c => c.Name.Equals(val.Key));
                    if (col == null)
                        throw new ArgumentException("No column exists with name '" + val.Key + "' as found in row " + i + ".");

                    object value = GetValue(val.Value, col.OriginalType);
                    row[val.Key] = value ?? DBNull.Value;
                }

                ret.Rows.Add(row);
            }

            return ret;
        }

        /// <summary>
        /// Convert to a markdown table string.
        /// </summary>
        /// <returns>Markdown formatted string representation of the table, or null if no columns are defined.</returns>
        public string ToMarkdown()
        {
            return MarkdownConverter.Convert(this);
        }

        private static ColumnValueTypeEnum DataTypeToColumnValueTypeEnum(Type t)
        {
            switch (t)
            {
                case Type _ when t == typeof(string):
                    return ColumnValueTypeEnum.String;
                case Type _ when t == typeof(Int16):
                    return ColumnValueTypeEnum.Int16;
                case Type _ when t == typeof(Int32):
                    return ColumnValueTypeEnum.Int32;
                case Type _ when t == typeof(Int64):
                    return ColumnValueTypeEnum.Int64;
                case Type _ when t == typeof(UInt16):
                    return ColumnValueTypeEnum.UInt16;
                case Type _ when t == typeof(UInt32):
                    return ColumnValueTypeEnum.UInt32;
                case Type _ when t == typeof(UInt64):
                    return ColumnValueTypeEnum.UInt64;
                case Type _ when t == typeof(decimal):
                    return ColumnValueTypeEnum.Decimal;
                case Type _ when t == typeof(double):
                    return ColumnValueTypeEnum.Double;
                case Type _ when t == typeof(float):
                    return ColumnValueTypeEnum.Float;
                case Type _ when t == typeof(bool):
                    return ColumnValueTypeEnum.Boolean;
                case Type _ when t == typeof(DateTime):
                    return ColumnValueTypeEnum.DateTime;
                case Type _ when t == typeof(DateTimeOffset):
                    return ColumnValueTypeEnum.DateTimeOffset;
                case Type _ when t == typeof(TimeSpan):
                    return ColumnValueTypeEnum.TimeSpan;
                case Type _ when t == typeof(byte):
                    return ColumnValueTypeEnum.Byte;
                case Type _ when t == typeof(sbyte):
                    return ColumnValueTypeEnum.SByte;
                case Type _ when t == typeof(byte[]):
                    return ColumnValueTypeEnum.ByteArray;
                case Type _ when t == typeof(char):
                    return ColumnValueTypeEnum.Char;
                case Type _ when t == typeof(Guid):
                    return ColumnValueTypeEnum.Guid;
                case Type _ when t == typeof(object):
                    return ColumnValueTypeEnum.Object;
                default:
                    // Fall back to Object for unknown types (e.g., custom database types like pgvector)
                    return ColumnValueTypeEnum.Object;
            }
        }

        private static Type ColumnValueTypeEnumToDataType(ColumnValueTypeEnum cvt)
        {
            switch (cvt)
            {
                case ColumnValueTypeEnum.String:
                    return typeof(string);
                case ColumnValueTypeEnum.Int16:
                    return typeof(Int16);
                case ColumnValueTypeEnum.Int32:
                    return typeof(Int32);
                case ColumnValueTypeEnum.Int64:
                    return typeof(Int64);
                case ColumnValueTypeEnum.UInt16:
                    return typeof(UInt16);
                case ColumnValueTypeEnum.UInt32:
                    return typeof(UInt32);
                case ColumnValueTypeEnum.UInt64:
                    return typeof(UInt64);
                case ColumnValueTypeEnum.Decimal:
                    return typeof(decimal);
                case ColumnValueTypeEnum.Double:
                    return typeof(double);
                case ColumnValueTypeEnum.Float:
                    return typeof(float);
                case ColumnValueTypeEnum.Boolean:
                    return typeof(bool);
                case ColumnValueTypeEnum.DateTime:
                    return typeof(DateTime);
                case ColumnValueTypeEnum.DateTimeOffset:
                    return typeof(DateTimeOffset);
                case ColumnValueTypeEnum.TimeSpan:
                    return typeof(TimeSpan);
                case ColumnValueTypeEnum.Byte:
                    return typeof(byte);
                case ColumnValueTypeEnum.SByte:
                    return typeof(sbyte);
                case ColumnValueTypeEnum.ByteArray:
                    return typeof(byte[]);
                case ColumnValueTypeEnum.Char:
                    return typeof(char);
                case ColumnValueTypeEnum.Guid:
                    return typeof(Guid);
                case ColumnValueTypeEnum.Object:
                    return typeof(object);
                default:
                    throw new ArgumentException("Unknown column value type '" + cvt.ToString() + "'.");
            }
        }

        private static Type DetectArrayType(DataTable dt, string columnName)
        {
            foreach (DataRow row in dt.Rows)
            {
                object cellValue = row[columnName];
                if (cellValue != null && cellValue != DBNull.Value && cellValue.GetType().IsArray)
                {
                    return cellValue.GetType();
                }
            }
            return null;
        }

        private static object GetValue(object obj, string originalType)
        {
            if (obj == null) return null;

            if (obj is JsonElement)
            {
                JsonElement element = (JsonElement)obj;
                switch (element.ValueKind)
                {
                    case JsonValueKind.Array:
                        return ParseJsonArray(element, originalType);
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.Null:
                        return null;
                    case JsonValueKind.Number:
                        string numStr = obj.ToString();
                        // Handle scientific notation (e.g., 3.4028235E+38) as double
                        if (numStr.Contains("E") || numStr.Contains("e"))
                            return Double.Parse(numStr);
                        if (numStr.Contains("."))
                            return Decimal.Parse(numStr);
                        else
                        {
                            // Try to parse as long to handle large integers
                            if (Int64.TryParse(numStr, out long longVal))
                                return longVal;
                            return Decimal.Parse(numStr);
                        }
                    case JsonValueKind.Object:
                        // Convert objects to compact JSON string
                        return JsonSerializer.Serialize(element);
                    case JsonValueKind.String:
                        return obj.ToString();
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.Undefined:
                        return null;
                    default:
                        // Fall back to compact JSON string for unknown types
                        return JsonSerializer.Serialize(element);
                }
            }

            return obj;
        }

        private static object ParseJsonArray(JsonElement arrayElement, string originalType)
        {
            int length = arrayElement.GetArrayLength();

            // If we have type metadata, create a properly typed array
            if (!String.IsNullOrEmpty(originalType))
            {
                Type arrayType = Type.GetType(originalType);
                if (arrayType != null && arrayType.IsArray)
                {
                    Type elementType = arrayType.GetElementType();
                    if (elementType != null)
                    {
                        Array typedArray = Array.CreateInstance(elementType, length);
                        int index = 0;
                        foreach (JsonElement item in arrayElement.EnumerateArray())
                        {
                            object value = GetValue(item, null);
                            typedArray.SetValue(ConvertToType(value, elementType), index);
                            index++;
                        }
                        return typedArray;
                    }
                }
            }

            // Fallback to object[] if no type metadata
            object[] result = new object[length];
            int idx = 0;
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                result[idx] = GetValue(item, null);
                idx++;
            }
            return result;
        }

        private static object ConvertToType(object value, Type targetType)
        {
            if (value == null) return null;

            if (targetType == typeof(float))
                return Convert.ToSingle(value);
            if (targetType == typeof(double))
                return Convert.ToDouble(value);
            if (targetType == typeof(decimal))
                return Convert.ToDecimal(value);
            if (targetType == typeof(int) || targetType == typeof(Int32))
                return Convert.ToInt32(value);
            if (targetType == typeof(long) || targetType == typeof(Int64))
                return Convert.ToInt64(value);
            if (targetType == typeof(short) || targetType == typeof(Int16))
                return Convert.ToInt16(value);
            if (targetType == typeof(uint) || targetType == typeof(UInt32))
                return Convert.ToUInt32(value);
            if (targetType == typeof(ulong) || targetType == typeof(UInt64))
                return Convert.ToUInt64(value);
            if (targetType == typeof(ushort) || targetType == typeof(UInt16))
                return Convert.ToUInt16(value);
            if (targetType == typeof(byte))
                return Convert.ToByte(value);
            if (targetType == typeof(sbyte))
                return Convert.ToSByte(value);
            if (targetType == typeof(bool))
                return Convert.ToBoolean(value);
            if (targetType == typeof(string))
                return Convert.ToString(value);
            if (targetType == typeof(Guid))
            {
                if (value is Guid guidVal)
                    return guidVal;
                return Guid.Parse(value.ToString());
            }
            if (targetType == typeof(DateTime))
            {
                if (value is DateTime dateTimeVal)
                    return dateTimeVal;
                return DateTime.Parse(value.ToString());
            }
            if (targetType == typeof(DateTimeOffset))
            {
                if (value is DateTimeOffset dateTimeOffsetVal)
                    return dateTimeOffsetVal;
                return DateTimeOffset.Parse(value.ToString());
            }
            if (targetType == typeof(TimeSpan))
            {
                if (value is TimeSpan timeSpanVal)
                    return timeSpanVal;
                return TimeSpan.Parse(value.ToString());
            }
            if (targetType == typeof(char))
            {
                if (value is char charVal)
                    return charVal;
                string strVal = value.ToString();
                return strVal.Length > 0 ? strVal[0] : '\0';
            }

            return Convert.ChangeType(value, targetType);
        }

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
