namespace SerializableDataTables
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// Static utility class to convert DataTable and SerializableDataTable to Markdown format
    /// </summary>
    public static class MarkdownConverter
    {
        #region Public-Members

        /// <summary>
        /// Newline character to use in all markdown conversion operations.
        /// Defaults to Environment.NewLine.
        /// </summary>
        public static string NewlineCharacter { get; set; } = Environment.NewLine;

        #endregion

        #region Public-Methods

        /// <summary>
        /// Convert an entire DataTable to markdown format.
        /// </summary>
        /// <param name="dt">The DataTable to convert.</param>
        /// <returns>Markdown formatted string representation of the table, or null if no columns are defined.</returns>
        public static string Convert(DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            // Add table name as header if available
            if (!string.IsNullOrEmpty(dt.TableName))
            {
                sb.Append($"# {dt.TableName}");
                sb.Append(NewlineCharacter);
                sb.Append(NewlineCharacter);
            }

            // Add headers
            sb.Append(ConvertHeaders(dt));

            // Add rows
            foreach (string rowMarkdown in IterateRows(dt))
            {
                sb.Append(rowMarkdown);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert an entire SerializableDataTable to markdown format.
        /// </summary>
        /// <param name="dt">The SerializableDataTable to convert.</param>
        /// <returns>Markdown formatted string representation of the table, or null if no columns are defined.</returns>
        public static string Convert(SerializableDataTable dt)
        {
            if (dt == null || dt.Columns == null || dt.Columns.Count == 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            // Add table name as header if available
            if (!string.IsNullOrEmpty(dt.Name))
            {
                sb.Append($"# {dt.Name}");
                sb.Append(NewlineCharacter);
                sb.Append(NewlineCharacter);
            }

            // Add headers
            sb.Append(ConvertHeaders(dt));

            // Add rows
            foreach (string rowMarkdown in IterateRows(dt))
            {
                sb.Append(rowMarkdown);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert just the headers of a DataTable to markdown format.
        /// </summary>
        /// <param name="dt">The DataTable whose headers to convert.</param>
        /// <returns>Markdown formatted string representation of the table headers.</returns>
        public static string ConvertHeaders(DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            // Build header row
            foreach (DataColumn col in dt.Columns)
            {
                sb.Append("| ");
                sb.Append(col.ColumnName);
                sb.Append(" ");
            }
            sb.Append("|");
            sb.Append(NewlineCharacter);

            // Build separator row
            foreach (DataColumn col in dt.Columns)
            {
                sb.Append("| ");
                sb.Append("---");
                sb.Append(" ");
            }
            sb.Append("|");
            sb.Append(NewlineCharacter);

            return sb.ToString();
        }

        /// <summary>
        /// Convert just the headers of a SerializableDataTable to markdown format.
        /// </summary>
        /// <param name="dt">The SerializableDataTable whose headers to convert.</param>
        /// <returns>Markdown formatted string representation of the table headers.</returns>
        public static string ConvertHeaders(SerializableDataTable dt)
        {
            if (dt == null || dt.Columns == null || dt.Columns.Count == 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            // Build header row
            foreach (SerializableColumn col in dt.Columns)
            {
                sb.Append("| ");
                sb.Append(col.Name);
                sb.Append(" ");
            }
            sb.Append("|");
            sb.Append(NewlineCharacter);

            // Build separator row
            foreach (SerializableColumn col in dt.Columns)
            {
                sb.Append("| ");
                sb.Append("---");
                sb.Append(" ");
            }
            sb.Append("|");
            sb.Append(NewlineCharacter);

            return sb.ToString();
        }

        /// <summary>
        /// Convert a specific row of a DataTable to markdown format.
        /// </summary>
        /// <param name="dt">The DataTable containing the row.</param>
        /// <param name="rowNumber">Zero-based index of the row to convert.</param>
        /// <returns>Markdown formatted string representation of the row.</returns>
        /// <exception cref="ArgumentNullException">Thrown when dt is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when rowNumber is outside the bounds of the rows collection.</exception>
        public static string ConvertRow(DataTable dt, int rowNumber)
        {
            if (dt == null)
            {
                throw new ArgumentNullException(nameof(dt));
            }

            if (rowNumber < 0 || rowNumber >= dt.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowNumber), "Row number is outside the bounds of the rows collection.");
            }

            StringBuilder sb = new StringBuilder();
            DataRow row = dt.Rows[rowNumber];

            foreach (DataColumn col in dt.Columns)
            {
                sb.Append("| ");

                object value = row[col];

                if (value == null || value == DBNull.Value)
                {
                    sb.Append("");
                }
                else if (col.DataType == typeof(byte[]))
                {
                    // For byte arrays, convert to Base64
                    if (value is byte[] byteArray)
                    {
                        sb.Append(System.Convert.ToBase64String(byteArray));
                    }
                    else
                    {
                        sb.Append(value.ToString());
                    }
                }
                else if (col.DataType == typeof(object))
                {
                    // For Object types, output JSON if not null
                    try
                    {
                        string json = JsonSerializer.Serialize(value);
                        sb.Append(json);
                    }
                    catch
                    {
                        sb.Append(value.ToString());
                    }
                }
                else
                {
                    // Format DateTime and DateTimeOffset with microsecond precision
                    if (value is DateTime dt_val)
                    {
                        sb.Append(dt_val.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                    }
                    else if (value is DateTimeOffset dto_val)
                    {
                        sb.Append(dto_val.ToString("yyyy-MM-dd HH:mm:ss.ffffff zzz"));
                    }
                    else
                    {
                        // Escape pipe characters in data to avoid breaking markdown table format
                        string displayValue = value.ToString().Replace("|", "\\|");
                        sb.Append(displayValue);
                    }
                }

                sb.Append(" ");
            }

            sb.Append("|");
            sb.Append(NewlineCharacter);

            return sb.ToString();
        }

        /// <summary>
        /// Convert a specific row of a SerializableDataTable to markdown format.
        /// </summary>
        /// <param name="dt">The SerializableDataTable containing the row.</param>
        /// <param name="rowNumber">Zero-based index of the row to convert.</param>
        /// <returns>Markdown formatted string representation of the row.</returns>
        /// <exception cref="ArgumentNullException">Thrown when dt is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when rowNumber is outside the bounds of the rows collection.</exception>
        public static string ConvertRow(SerializableDataTable dt, int rowNumber)
        {
            if (dt == null)
            {
                throw new ArgumentNullException(nameof(dt));
            }

            if (dt.Rows == null)
            {
                throw new ArgumentNullException(nameof(dt.Rows));
            }

            if (rowNumber < 0 || rowNumber >= dt.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowNumber), "Row number is outside the bounds of the rows collection.");
            }

            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> row = dt.Rows[rowNumber];

            foreach (SerializableColumn col in dt.Columns)
            {
                sb.Append("| ");

                if (row.TryGetValue(col.Name, out object value))
                {
                    if (value == null)
                    {
                        sb.Append("");
                    }
                    else if (col.Type == ColumnValueType.ByteArray)
                    {
                        // For byte arrays, convert to Base64
                        if (value is byte[] byteArray)
                        {
                            sb.Append(System.Convert.ToBase64String(byteArray));
                        }
                        else
                        {
                            sb.Append(value.ToString());
                        }
                    }
                    else if (col.Type == ColumnValueType.Object)
                    {
                        // For Object types, output JSON if not null
                        try
                        {
                            string json = JsonSerializer.Serialize(value);
                            sb.Append(json);
                        }
                        catch
                        {
                            sb.Append(value.ToString());
                        }
                    }
                    else
                    {
                        // Format DateTime and DateTimeOffset with microsecond precision
                        if (value is DateTime dt_val)
                        {
                            sb.Append(dt_val.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                        }
                        else if (value is DateTimeOffset dto_val)
                        {
                            sb.Append(dto_val.ToString("yyyy-MM-dd HH:mm:ss.ffffff zzz"));
                        }
                        else
                        {
                            // Escape pipe characters in data to avoid breaking markdown table format
                            string displayValue = value.ToString().Replace("|", "\\|");
                            sb.Append(displayValue);
                        }
                    }
                }
                else
                {
                    sb.Append("");
                }

                sb.Append(" ");
            }

            sb.Append("|");
            sb.Append(NewlineCharacter);

            return sb.ToString();
        }

        /// <summary>
        /// Iterate through rows of a DataTable, yielding the markdown representation of each row.
        /// </summary>
        /// <param name="dt">The DataTable to iterate.</param>
        /// <returns>IEnumerable of strings, each representing a row in markdown format.</returns>
        public static IEnumerable<string> IterateRows(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                yield break;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                yield return ConvertRow(dt, i);
            }
        }

        /// <summary>
        /// Iterate through rows of a SerializableDataTable, yielding the markdown representation of each row.
        /// </summary>
        /// <param name="dt">The SerializableDataTable to iterate.</param>
        /// <returns>IEnumerable of strings, each representing a row in markdown format.</returns>
        public static IEnumerable<string> IterateRows(SerializableDataTable dt)
        {
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
            {
                yield break;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                yield return ConvertRow(dt, i);
            }
        }

        #endregion
    }
}