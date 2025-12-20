namespace Test.Pgvector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System.Text.Json;

    using Npgsql;
    using Pgvector;

    using SerializableDataTables;

    /// <summary>
    /// Test program for verifying SerializableDataTable works with pgvector data types.
    /// </summary>
    public class Program
    {
        private static int _TestsPassed = 0;
        private static int _TestsFailed = 0;
        private static List<string> _FailedTests = new List<string>();

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("=== SerializableDataTable Pgvector Test Program ===\n");

            string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=password";

            try
            {
                // Register pgvector type mapper
                NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                dataSourceBuilder.UseVector();
                NpgsqlDataSource dataSource = dataSourceBuilder.Build();

                using (NpgsqlConnection connection = dataSource.OpenConnection())
                {
                    Console.WriteLine("Connected to PostgreSQL database successfully.\n");

                    // Execute the query
                    string query = "SELECT * FROM public.embeddings;";
                    Console.WriteLine($"Executing query: {query}\n");

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable("embeddings");
                        adapter.Fill(dataTable);

                        Console.WriteLine("=== Original DataTable ===");
                        Console.WriteLine($"Rows: {dataTable.Rows.Count}, Columns: {dataTable.Columns.Count}\n");

                        // Log DataTable column info
                        Console.WriteLine("--- DataTable Column Definitions ---");
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Console.WriteLine($"  Column: {column.ColumnName}");
                            Console.WriteLine($"    DataType: {column.DataType.FullName}");
                            Console.WriteLine($"    AllowDBNull: {column.AllowDBNull}");
                        }
                        Console.WriteLine();

                        // Log DataTable row data with types
                        Console.WriteLine("--- DataTable Row Data ---");
                        int rowIndex = 0;
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Console.WriteLine($"Row {rowIndex}:");
                            foreach (DataColumn column in dataTable.Columns)
                            {
                                object value = row[column];
                                string valueStr = FormatValue(value);
                                string typeStr = GetValueType(value);
                                Console.WriteLine($"  {column.ColumnName}: {valueStr} ({typeStr})");
                            }
                            Console.WriteLine();
                            rowIndex++;
                        }

                        // Convert to SerializableDataTable
                        Console.WriteLine("\n=== Converting to SerializableDataTable ===\n");
                        SerializableDataTable serializableTable = SerializableDataTable.FromDataTable(dataTable);

                        // Log SerializableDataTable column info
                        Console.WriteLine("--- SerializableDataTable Column Definitions ---");
                        foreach (SerializableColumn column in serializableTable.Columns)
                        {
                            Console.WriteLine($"  Column: {column.Name}");
                            Console.WriteLine($"    Type: {column.Type}");
                            Console.WriteLine($"    OriginalType: {column.OriginalType ?? "null"}");
                        }
                        Console.WriteLine();

                        // Log SerializableDataTable row data with types
                        Console.WriteLine("--- SerializableDataTable Row Data ---");
                        rowIndex = 0;
                        foreach (Dictionary<string, object> row in serializableTable.Rows)
                        {
                            Console.WriteLine($"Row {rowIndex}:");
                            foreach (SerializableColumn column in serializableTable.Columns)
                            {
                                if (row.TryGetValue(column.Name, out object value))
                                {
                                    string valueStr = FormatValue(value);
                                    string typeStr = GetValueType(value);
                                    Console.WriteLine($"  {column.Name}: {valueStr} ({typeStr})");
                                }
                                else
                                {
                                    Console.WriteLine($"  {column.Name}: MISSING");
                                }
                            }
                            Console.WriteLine();
                            rowIndex++;
                        }

                        // Convert back to DataTable
                        Console.WriteLine("\n=== Converting SerializableDataTable back to DataTable ===\n");
                        DataTable convertedTable = serializableTable.ToDataTable();

                        // Log converted DataTable column info
                        Console.WriteLine("--- Converted DataTable Column Definitions ---");
                        foreach (DataColumn column in convertedTable.Columns)
                        {
                            Console.WriteLine($"  Column: {column.ColumnName}");
                            Console.WriteLine($"    DataType: {column.DataType.FullName}");
                        }
                        Console.WriteLine();

                        // Log converted DataTable row data with types
                        Console.WriteLine("--- Converted DataTable Row Data ---");
                        rowIndex = 0;
                        foreach (DataRow row in convertedTable.Rows)
                        {
                            Console.WriteLine($"Row {rowIndex}:");
                            foreach (DataColumn column in convertedTable.Columns)
                            {
                                object value = row[column];
                                string valueStr = FormatValue(value);
                                string typeStr = GetValueType(value);
                                Console.WriteLine($"  {column.ColumnName}: {valueStr} ({typeStr})");
                            }
                            Console.WriteLine();
                            rowIndex++;
                        }

                        // Compare original DataTable with converted DataTable (in-memory round-trip)
                        Console.WriteLine("\n=== COMPARISON 1: In-Memory Round-Trip (DataTable -> SerializableDataTable -> DataTable) ===\n");
                        CompareDataTables(dataTable, convertedTable);

                        // JSON Serialization Round-Trip Test
                        Console.WriteLine("\n=== JSON SERIALIZATION ROUND-TRIP ===\n");

                        // Serialize to JSON
                        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };
                        string json = JsonSerializer.Serialize(serializableTable, jsonOptions);
                        Console.WriteLine($"Serialized to JSON ({json.Length} characters)");
                        Console.WriteLine("First 500 characters of JSON:");
                        Console.WriteLine(json.Length > 500 ? json.Substring(0, 500) + "..." : json);
                        Console.WriteLine();

                        // Deserialize from JSON
                        SerializableDataTable deserializedTable = JsonSerializer.Deserialize<SerializableDataTable>(json, jsonOptions);
                        Console.WriteLine("Deserialized from JSON successfully.\n");

                        // Log deserialized SerializableDataTable column info
                        Console.WriteLine("--- Deserialized SerializableDataTable Column Definitions ---");
                        foreach (SerializableColumn column in deserializedTable.Columns)
                        {
                            Console.WriteLine($"  Column: {column.Name}");
                            Console.WriteLine($"    Type: {column.Type}");
                            Console.WriteLine($"    OriginalType: {column.OriginalType ?? "null"}");
                        }
                        Console.WriteLine();

                        // Log deserialized SerializableDataTable row data with types
                        Console.WriteLine("--- Deserialized SerializableDataTable Row Data ---");
                        rowIndex = 0;
                        foreach (Dictionary<string, object> row in deserializedTable.Rows)
                        {
                            Console.WriteLine($"Row {rowIndex}:");
                            foreach (SerializableColumn column in deserializedTable.Columns)
                            {
                                if (row.TryGetValue(column.Name, out object value))
                                {
                                    string valueStr = FormatValue(value);
                                    string typeStr = GetValueType(value);
                                    Console.WriteLine($"  {column.Name}: {valueStr} ({typeStr})");
                                }
                                else
                                {
                                    Console.WriteLine($"  {column.Name}: MISSING");
                                }
                            }
                            Console.WriteLine();
                            rowIndex++;
                        }

                        // Convert deserialized SerializableDataTable back to DataTable
                        Console.WriteLine("\n=== Converting Deserialized SerializableDataTable to DataTable ===\n");
                        DataTable jsonRoundTripTable = deserializedTable.ToDataTable();

                        // Log JSON round-trip DataTable column info
                        Console.WriteLine("--- JSON Round-Trip DataTable Column Definitions ---");
                        foreach (DataColumn column in jsonRoundTripTable.Columns)
                        {
                            Console.WriteLine($"  Column: {column.ColumnName}");
                            Console.WriteLine($"    DataType: {column.DataType.FullName}");
                        }
                        Console.WriteLine();

                        // Log JSON round-trip DataTable row data with types
                        Console.WriteLine("--- JSON Round-Trip DataTable Row Data ---");
                        rowIndex = 0;
                        foreach (DataRow row in jsonRoundTripTable.Rows)
                        {
                            Console.WriteLine($"Row {rowIndex}:");
                            foreach (DataColumn column in jsonRoundTripTable.Columns)
                            {
                                object value = row[column];
                                string valueStr = FormatValue(value);
                                string typeStr = GetValueType(value);
                                Console.WriteLine($"  {column.ColumnName}: {valueStr} ({typeStr})");
                            }
                            Console.WriteLine();
                            rowIndex++;
                        }

                        // Compare original DataTable with JSON round-trip DataTable
                        Console.WriteLine("\n=== COMPARISON 2: JSON Round-Trip (DataTable -> SerializableDataTable -> JSON -> SerializableDataTable -> DataTable) ===\n");
                        CompareDataTables(dataTable, jsonRoundTripTable);

                        // Display final results
                        Console.WriteLine("\n=== FINAL RESULTS ===");
                        Console.WriteLine($"Passed: {_TestsPassed}");
                        Console.WriteLine($"Failed: {_TestsFailed}");

                        if (_TestsFailed == 0)
                        {
                            Console.WriteLine("\n*** ALL TESTS PASSED ***");
                        }
                        else
                        {
                            Console.WriteLine("\n*** SOME TESTS FAILED ***");
                            Console.WriteLine("\nFailed tests:");
                            foreach (string failedTest in _FailedTests)
                            {
                                Console.WriteLine($"  - {failedTest}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("\n*** TEST FAILED - Exception occurred ***");
            }
        }

        private static void CompareDataTables(DataTable original, DataTable converted)
        {
            // Compare column counts
            bool columnCountMatch = original.Columns.Count == converted.Columns.Count;
            AssertTest($"Column count matches ({original.Columns.Count} == {converted.Columns.Count})", columnCountMatch);

            // Compare row counts
            bool rowCountMatch = original.Rows.Count == converted.Rows.Count;
            AssertTest($"Row count matches ({original.Rows.Count} == {converted.Rows.Count})", rowCountMatch);

            if (!columnCountMatch || !rowCountMatch)
            {
                Console.WriteLine("Cannot compare data - structure mismatch");
                return;
            }

            // Compare column names
            for (int colIdx = 0; colIdx < original.Columns.Count; colIdx++)
            {
                string origName = original.Columns[colIdx].ColumnName;
                string convName = converted.Columns[colIdx].ColumnName;
                bool nameMatch = origName == convName;
                AssertTest($"Column {colIdx} name matches ('{origName}' == '{convName}')", nameMatch);
            }

            // Compare row data
            for (int rowIdx = 0; rowIdx < original.Rows.Count; rowIdx++)
            {
                DataRow origRow = original.Rows[rowIdx];
                DataRow convRow = converted.Rows[rowIdx];

                for (int colIdx = 0; colIdx < original.Columns.Count; colIdx++)
                {
                    string colName = original.Columns[colIdx].ColumnName;
                    object origValue = origRow[colIdx];
                    object convValue = convRow[colIdx];

                    // Compare values
                    bool valueMatch = CompareValues(origValue, convValue);
                    string origStr = FormatValue(origValue);
                    string convStr = FormatValue(convValue);

                    if (origStr.Length > 50) origStr = origStr.Substring(0, 47) + "...";
                    if (convStr.Length > 50) convStr = convStr.Substring(0, 47) + "...";

                    AssertTest($"Row {rowIdx}, Column '{colName}' value matches", valueMatch);

                    if (!valueMatch)
                    {
                        Console.WriteLine($"    Original: {origStr}");
                        Console.WriteLine($"    Converted: {convStr}");
                    }

                    // Compare types (for non-null values)
                    if (origValue != null && origValue != DBNull.Value &&
                        convValue != null && convValue != DBNull.Value)
                    {
                        string origType = GetValueType(origValue);
                        string convType = GetValueType(convValue);

                        // For pgvector Vector type, it may be converted to float[]
                        bool typeMatch = origType == convType ||
                                        (origType.Contains("Vector") && convType.Contains("Single[]")) ||
                                        (origType.Contains("Vector") && convType.Contains("float[]"));

                        AssertTest($"Row {rowIdx}, Column '{colName}' type compatible ({origType} -> {convType})", typeMatch);
                    }
                }
            }
        }

        private static bool CompareValues(object origValue, object convValue)
        {
            // Handle nulls and DBNull
            bool origIsNull = origValue == null || origValue == DBNull.Value;
            bool convIsNull = convValue == null || convValue == DBNull.Value;

            if (origIsNull && convIsNull) return true;
            if (origIsNull != convIsNull) return false;

            // Handle pgvector Vector type - compare as float arrays using reflection
            float[] origVectorArray = TryGetVectorArray(origValue);
            if (origVectorArray != null)
            {
                if (convValue is float[] convArray)
                {
                    return CompareFloatArrays(origVectorArray, convArray);
                }

                float[] convVectorArray = TryGetVectorArray(convValue);
                if (convVectorArray != null)
                {
                    return CompareFloatArrays(origVectorArray, convVectorArray);
                }

                return false;
            }

            // Handle float arrays
            if (origValue is float[] origFloatArr && convValue is float[] convFloatArr)
            {
                return CompareFloatArrays(origFloatArr, convFloatArr);
            }

            // Handle other arrays
            if (origValue is Array origArr && convValue is Array convArr)
            {
                if (origArr.Length != convArr.Length) return false;

                for (int i = 0; i < origArr.Length; i++)
                {
                    if (!CompareValues(origArr.GetValue(i), convArr.GetValue(i)))
                        return false;
                }

                return true;
            }

            // For numeric types, compare with tolerance
            if (IsNumeric(origValue) && IsNumeric(convValue))
            {
                double origDouble = Convert.ToDouble(origValue);
                double convDouble = Convert.ToDouble(convValue);
                return Math.Abs(origDouble - convDouble) < 0.0001;
            }

            // Default comparison
            return origValue.Equals(convValue);
        }

        private static float[] TryGetVectorArray(object value)
        {
            if (value == null) return null;

            Type type = value.GetType();

            // Check if it's a pgvector Vector type by looking for ToArray method
            if (type.FullName != null && type.FullName.Contains("Pgvector"))
            {
                MethodInfo toArrayMethod = type.GetMethod("ToArray", BindingFlags.Public | BindingFlags.Instance);
                if (toArrayMethod != null)
                {
                    object result = toArrayMethod.Invoke(value, null);
                    if (result is float[] floatArray)
                    {
                        return floatArray;
                    }
                }
            }

            return null;
        }

        private static bool CompareFloatArrays(float[] arr1, float[] arr2)
        {
            if (arr1.Length != arr2.Length) return false;

            for (int i = 0; i < arr1.Length; i++)
            {
                if (Math.Abs(arr1[i] - arr2[i]) > 0.0001f)
                    return false;
            }

            return true;
        }

        private static bool IsNumeric(object value)
        {
            return value is sbyte || value is byte ||
                   value is short || value is ushort ||
                   value is int || value is uint ||
                   value is long || value is ulong ||
                   value is float || value is double ||
                   value is decimal;
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "null";
            if (value == DBNull.Value) return "DBNull";

            // Handle pgvector Vector type via reflection
            float[] vectorArray = TryGetVectorArray(value);
            if (vectorArray != null)
            {
                return $"Vector[{vectorArray.Length}]: {JsonSerializer.Serialize(vectorArray)}";
            }

            if (value is Array array)
            {
                return $"{value.GetType().Name}[{array.Length}]: {JsonSerializer.Serialize(value)}";
            }

            return value.ToString();
        }

        private static string GetValueType(object value)
        {
            if (value == null) return "null";
            if (value == DBNull.Value) return "DBNull";

            Type type = value.GetType();
            return type.FullName;
        }

        private static void AssertTest(string testName, bool passed)
        {
            if (passed)
            {
                Console.WriteLine($"  PASS: {testName}");
                _TestsPassed++;
            }
            else
            {
                Console.WriteLine($"  FAIL: {testName}");
                _TestsFailed++;
                _FailedTests.Add(testName);
            }
        }
    }
}
