namespace SerializableDataTables
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Column for serializable data table.
    /// </summary>
    public class SerializableColumn
    {
        /// <summary>
        /// Name of the data table.
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Name));
                _Name = value;
            }
        }

        /// <summary>
        /// Column value type.
        /// </summary>
        public ColumnValueTypeEnum Type { get; set; } = ColumnValueTypeEnum.String;

        /// <summary>
        /// Original element type for array columns.
        /// Null for non-array columns or when type preservation is not required.
        /// Stored as the assembly-qualified type name for serialization compatibility.
        /// </summary>
        public string OriginalType { get; set; } = null;

        private string _Name = "MyTable";

        /// <summary>
        /// Instantiate.
        /// </summary>
        public SerializableColumn()
        {

        }
    }
}
