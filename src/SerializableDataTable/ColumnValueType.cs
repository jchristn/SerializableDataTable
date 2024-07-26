namespace SerializableDataTables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    /// <summary>
    /// Column value type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ColumnValueType
    {
        /// <summary>
        /// String.
        /// </summary>
        [EnumMember(Value = "String")]
        String,
        /// <summary>
        /// Int32.
        /// </summary>
        [EnumMember(Value = "Int32")]
        Int32,
        /// <summary>
        /// Int64.
        /// </summary>
        [EnumMember(Value = "Int64")]
        Int64,
        /// <summary>
        /// Decimal.
        /// </summary>
        [EnumMember(Value = "Decimal")]
        Decimal,
        /// <summary>
        /// Double.
        /// </summary>
        [EnumMember(Value = "Double")]
        Double,
        /// <summary>
        /// Float.
        /// </summary>
        [EnumMember(Value = "Float")]
        Float,
        /// <summary>
        /// Boolean.
        /// </summary>
        [EnumMember(Value = "Boolean")]
        Boolean,
        /// <summary>
        /// DateTime.
        /// </summary>
        [EnumMember(Value = "DateTime")]
        DateTime,
        /// <summary>
        /// DateTimeOffset.
        /// </summary>
        [EnumMember(Value = "DateTimeOffset")]
        DateTimeOffset,
        /// <summary>
        /// Byte.
        /// </summary>
        [EnumMember(Value = "Byte")]
        Byte,
        /// <summary>
        /// ByteArray.
        /// </summary>
        [EnumMember(Value = "ByteArray")]
        ByteArray,
        /// <summary>
        /// Char.
        /// </summary>
        [EnumMember(Value = "Char")]
        Char,
        /// <summary>
        /// GUID.
        /// </summary>
        [EnumMember(Value = "Guid")]
        Guid,
        /// <summary>
        /// Object.
        /// </summary>
        [EnumMember(Value = "Object")]
        Object
    }
}
