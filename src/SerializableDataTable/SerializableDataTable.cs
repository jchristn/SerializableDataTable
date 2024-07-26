namespace SerializableDataTables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Serializable data table.
    /// </summary>
    public class SerializableDataTable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

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

        #endregion

        #region Private-Members

        private List<SerializableColumn> _Columns = new List<SerializableColumn>();
        private List<Dictionary<string, object>> _Rows = new List<Dictionary<string, object>>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="name">Name.</param>
        public SerializableDataTable(string name = null)
        {
            if (!String.IsNullOrEmpty(name)) Name = name;
        }

        #endregion

        #region Public-Methods

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
                    DataType = ColumnValueTypeToDataType(col.Type)
                });
            }

            for (int i = 0; i < Rows.Count; i++)
            {
                Dictionary<string, object> dict = Rows[i];

                DataRow row = ret.NewRow();

                foreach (KeyValuePair<string, object> val in dict)
                {
                    if (!Columns.Any(c => c.Name.Equals(val.Key)))
                        throw new ArgumentException("No column exists with name '" + val.Key + "' as found in row " + i + ".");

                    row[val.Key] = GetValue(val.Value);
                }

                ret.Rows.Add(row);
            }

            return ret;
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
                ret.Columns.Add(new SerializableColumn
                {
                    Name = col.ColumnName,
                    Type = DataTypeToColumnValueType(col.DataType)
                });
            }

            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> val = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                    val.Add(col.ColumnName, row[col.ColumnName]);

                ret.Rows.Add(val);
            }

            return ret;
        }

        #endregion

        #region Private-Methods

        private static ColumnValueType DataTypeToColumnValueType(Type t)
        {
            switch (t)
            {
                case Type _ when t == typeof(string):
                    return ColumnValueType.String;
                case Type _ when t == typeof(Int32):
                    return ColumnValueType.Int32;
                case Type _ when t == typeof(Int64):
                    return ColumnValueType.Int64;
                case Type _ when t == typeof(decimal):
                    return ColumnValueType.Decimal;
                case Type _ when t == typeof(double):
                    return ColumnValueType.Double;
                case Type _ when t == typeof(float):
                    return ColumnValueType.Float;
                case Type _ when t == typeof(bool):
                    return ColumnValueType.Boolean;
                case Type _ when t == typeof(DateTime):
                    return ColumnValueType.DateTime;
                case Type _ when t == typeof(DateTimeOffset):
                    return ColumnValueType.DateTimeOffset;
                case Type _ when t == typeof(byte):
                    return ColumnValueType.Byte;
                case Type _ when t == typeof(byte[]):
                    return ColumnValueType.ByteArray;
                case Type _ when t == typeof(char):
                    return ColumnValueType.Char;
                case Type _ when t == typeof(Guid):
                    return ColumnValueType.Guid;
                case Type _ when t == typeof(object):
                    return ColumnValueType.Object;
                default:
                    throw new ArgumentException("Unsupported data type '" + t.Name + "'.");
            }
        }

        private static Type ColumnValueTypeToDataType(ColumnValueType cvt)
        {
            switch (cvt)
            {
                case ColumnValueType.String:
                    return typeof(string);
                case ColumnValueType.Int32:
                    return typeof(Int32);
                case ColumnValueType.Int64:
                    return typeof(Int64);
                case ColumnValueType.Decimal:
                    return typeof(decimal);
                case ColumnValueType.Double:
                    return typeof(double);
                case ColumnValueType.Float:
                    return typeof(float);
                case ColumnValueType.Boolean:
                    return typeof(bool);
                case ColumnValueType.DateTime:
                    return typeof(DateTime);
                case ColumnValueType.DateTimeOffset:
                    return typeof(DateTimeOffset);
                case ColumnValueType.Byte:
                    return typeof(byte);
                case ColumnValueType.ByteArray:
                    return typeof(byte[]);
                case ColumnValueType.Char:
                    return typeof(char);
                case ColumnValueType.Guid:
                    return typeof(Guid);
                case ColumnValueType.Object:
                    return typeof(object);
                default:
                    throw new ArgumentException("Unknown column value type '" + cvt.ToString() + "'.");
            }
        }

        private static object GetValue(object obj)
        {
            if (obj == null) return null;

            if (obj is JsonElement)
            {
                switch (((JsonElement)obj).ValueKind)
                {
                    case JsonValueKind.Array:
                        throw new ArgumentException("Arrays are not supported.");
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.Null:
                        return null;
                    case JsonValueKind.Number:
                        if (obj.ToString().Contains(".")) return Decimal.Parse(obj.ToString());
                        else return int.Parse(obj.ToString());
                    case JsonValueKind.Object:
                        throw new ArgumentException("Nested objects are not supported.");
                    case JsonValueKind.String:
                        return obj.ToString();
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.Undefined:
                        return null;
                    default:
                        throw new ArgumentException("Unsupported JSON element value kind '" + ((JsonElement)obj).ValueKind.ToString() + "'.");
                }
            }

            return obj;
        }

        #endregion

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
