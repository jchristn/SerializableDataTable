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
        #region Public-Members

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
        public ColumnValueType Type { get; set; } = ColumnValueType.String;

        #endregion

        #region Private-Members

        private string _Name = "MyTable";

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public SerializableColumn()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
