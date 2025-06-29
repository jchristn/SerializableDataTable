namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.Json;
    using SerializableDataTables;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== SerializableDataTable Test Program ===\n");

            try
            {
                // Test 1: Create a DataTable and convert to SerializableDataTable
                Console.WriteLine("Test 1: DataTable -> SerializableDataTable");
                Console.WriteLine("-------------------------------------------");
                DataTable originalDataTable = CreateSampleDataTable();
                PrintDataTable(originalDataTable);

                SerializableDataTable serializableTable = SerializableDataTable.FromDataTable(originalDataTable);
                Console.WriteLine("\nConverted to SerializableDataTable:");
                PrettyPrintSerializableDataTable(serializableTable);

                // Test 2: Convert SerializableDataTable back to DataTable
                Console.WriteLine("\n\nTest 2: SerializableDataTable -> DataTable");
                Console.WriteLine("-------------------------------------------");
                DataTable convertedBackTable = serializableTable.ToDataTable();
                PrintDataTable(convertedBackTable);

                // Test 3: Create SerializableDataTable directly
                Console.WriteLine("\n\nTest 3: Create SerializableDataTable Directly");
                Console.WriteLine("----------------------------------------------");
                SerializableDataTable directTable = CreateSerializableDataTableDirectly();
                PrettyPrintSerializableDataTable(directTable);

                // Test 4: Test with null values
                Console.WriteLine("\n\nTest 4: Handling Null Values");
                Console.WriteLine("-----------------------------");
                DataTable nullTestTable = CreateDataTableWithNulls();
                SerializableDataTable nullSerializable = SerializableDataTable.FromDataTable(nullTestTable);
                PrettyPrintSerializableDataTable(nullSerializable);

                // Test 5: Test various data types
                Console.WriteLine("\n\nTest 5: Various Data Types");
                Console.WriteLine("---------------------------");
                DataTable typeTestTable = CreateDataTableWithVariousTypes();
                SerializableDataTable typeSerializable = SerializableDataTable.FromDataTable(typeTestTable);
                PrettyPrintSerializableDataTable(typeSerializable);

                Console.WriteLine("\n\n=== All tests completed successfully! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static DataTable CreateSampleDataTable()
        {
            DataTable dt = new DataTable("Employees");

            // Add columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("Salary", typeof(decimal));
            dt.Columns.Add("IsActive", typeof(bool));

            // Add rows
            dt.Rows.Add(1, "John Doe", "Engineering", 75000.50m, true);
            dt.Rows.Add(2, "Jane Smith", "Marketing", 65000.00m, true);
            dt.Rows.Add(3, "Bob Johnson", "Sales", 55000.75m, false);
            dt.Rows.Add(4, "Alice Brown", "HR", 60000.25m, true);

            return dt;
        }

        private static DataTable CreateDataTableWithNulls()
        {
            DataTable dt = new DataTable("TestNulls");

            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("OptionalField", typeof(string));
            dt.Columns.Add("OptionalNumber", typeof(decimal));

            dt.Rows.Add(1, "Complete Row", "Has Value", 100.50m);
            dt.Rows.Add(2, "Partial Row", DBNull.Value, 200.00m);
            dt.Rows.Add(3, "Another Partial", "Has Value", DBNull.Value);
            dt.Rows.Add(4, DBNull.Value, DBNull.Value, DBNull.Value);

            return dt;
        }

        private static DataTable CreateDataTableWithVariousTypes()
        {
            DataTable dt = new DataTable("TypeTest");

            dt.Columns.Add("StringCol", typeof(string));
            dt.Columns.Add("Int32Col", typeof(int));
            dt.Columns.Add("Int64Col", typeof(long));
            dt.Columns.Add("DecimalCol", typeof(decimal));
            dt.Columns.Add("DoubleCol", typeof(double));
            dt.Columns.Add("FloatCol", typeof(float));
            dt.Columns.Add("BoolCol", typeof(bool));
            dt.Columns.Add("DateTimeCol", typeof(DateTime));
            dt.Columns.Add("GuidCol", typeof(Guid));
            dt.Columns.Add("CharCol", typeof(char));

            dt.Rows.Add(
                "Test String",
                42,
                9223372036854775807L,
                123.45m,
                3.14159265359,
                2.71828f,
                true,
                DateTime.Now,
                Guid.NewGuid(),
                'A'
            );

            return dt;
        }

        private static SerializableDataTable CreateSerializableDataTableDirectly()
        {
            var table = new SerializableDataTable("DirectlyCreated");

            // Add columns
            table.Columns.Add(new SerializableColumn { Name = "ProductId", Type = ColumnValueType.Int32 });
            table.Columns.Add(new SerializableColumn { Name = "ProductName", Type = ColumnValueType.String });
            table.Columns.Add(new SerializableColumn { Name = "Price", Type = ColumnValueType.Decimal });
            table.Columns.Add(new SerializableColumn { Name = "InStock", Type = ColumnValueType.Boolean });

            // Add rows
            table.Rows.Add(new Dictionary<string, object>
            {
                { "ProductId", 101 },
                { "ProductName", "Laptop" },
                { "Price", 999.99m },
                { "InStock", true }
            });

            table.Rows.Add(new Dictionary<string, object>
            {
                { "ProductId", 102 },
                { "ProductName", "Mouse" },
                { "Price", 29.99m },
                { "InStock", false }
            });

            table.Rows.Add(new Dictionary<string, object>
            {
                { "ProductId", 103 },
                { "ProductName", "Keyboard" },
                { "Price", 79.99m },
                { "InStock", true }
            });

            return table;
        }

        private static void PrintDataTable(DataTable dt)
        {
            Console.WriteLine($"DataTable: {dt.TableName}");
            Console.WriteLine($"Columns: {dt.Columns.Count}, Rows: {dt.Rows.Count}");
            Console.WriteLine();

            // Print column headers
            foreach (DataColumn col in dt.Columns)
            {
                Console.Write($"{col.ColumnName} ({col.DataType.Name})\t");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 80));

            // Print rows
            foreach (DataRow row in dt.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    if (item == DBNull.Value || item == null)
                        Console.Write("NULL\t\t");
                    else
                        Console.Write($"{item}\t");
                }
                Console.WriteLine();
            }
        }

        private static void PrettyPrintSerializableDataTable(SerializableDataTable sdt)
        {
            Console.WriteLine($"SerializableDataTable: {sdt.Name ?? "Unnamed"}");
            Console.WriteLine($"Columns: {sdt.Columns.Count}, Rows: {sdt.Rows.Count}");
            Console.WriteLine();

            // Print column information
            Console.WriteLine("Column Definitions:");
            for (int i = 0; i < sdt.Columns.Count; i++)
            {
                var col = sdt.Columns[i];
                Console.WriteLine($"  [{i}] {col.Name} (Type: {col.Type})");
            }

            Console.WriteLine("\nRow Data:");
            Console.WriteLine(new string('-', 80));

            // Print header
            foreach (var col in sdt.Columns)
            {
                Console.Write($"{col.Name}\t");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 80));

            // Print rows
            int rowIndex = 0;
            foreach (var row in sdt.Rows)
            {
                Console.Write($"[{rowIndex}] ");
                foreach (var col in sdt.Columns)
                {
                    if (row.ContainsKey(col.Name))
                    {
                        var value = row[col.Name];
                        if (value == null)
                            Console.Write("NULL\t");
                        else
                            Console.Write($"{value}\t");
                    }
                    else
                    {
                        Console.Write("MISSING\t");
                    }
                }
                Console.WriteLine();
                rowIndex++;
            }

            // Also show JSON representation for better visualization
            Console.WriteLine("\nJSON Representation:");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string json = JsonSerializer.Serialize(sdt, options);
            Console.WriteLine(json);
        }
    }
}