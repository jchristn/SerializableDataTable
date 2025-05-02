namespace Test.Markdown
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.Json;
    using SerializableDataTables;

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Testing SerializableDataTable ToMarkdown() functionality");
            Console.WriteLine("======================================================");
            Console.WriteLine();

            // Test 1: Empty table
            TestEmptyTable();

            // Test 2: Simple string and numeric data
            TestBasicTypes();

            // Test 3: DateTime and complex types
            TestComplexTypes();

            // Test 4: Table with a mix of all supported types
            TestAllSupportedTypes();

            Console.WriteLine("All tests completed.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void TestEmptyTable()
        {
            Console.WriteLine("Test 1: Empty table");
            Console.WriteLine("-------------------");

            // Create an empty table
            SerializableDataTable emptyTable = new SerializableDataTable("Empty Table");

            // Test with no columns
            string markdown = emptyTable.ToMarkdown();
            Console.WriteLine($"Empty table result: {(markdown == null ? "null (correct)" : "not null (incorrect)")}");

            // Add columns but no rows
            emptyTable.Columns.Add(new SerializableColumn { Name = "Column1", Type = ColumnValueType.String });
            markdown = emptyTable.ToMarkdown();
            Console.WriteLine("Table with columns but no rows:");
            Console.WriteLine(markdown);
            Console.WriteLine();
        }

        private static void TestBasicTypes()
        {
            Console.WriteLine("Test 2: Basic types");
            Console.WriteLine("------------------");

            // Create a table with basic types
            SerializableDataTable basicTable = new SerializableDataTable("Basic Types Table");

            // Add columns
            basicTable.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueType.String });
            basicTable.Columns.Add(new SerializableColumn { Name = "Age", Type = ColumnValueType.Int32 });
            basicTable.Columns.Add(new SerializableColumn { Name = "Balance", Type = ColumnValueType.Decimal });
            basicTable.Columns.Add(new SerializableColumn { Name = "Active", Type = ColumnValueType.Boolean });

            // Add rows
            basicTable.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "John Doe" },
                { "Age", 30 },
                { "Balance", 1234.56m },
                { "Active", true }
            });

            basicTable.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "Jane Smith" },
                { "Age", 25 },
                { "Balance", 7890.12m },
                { "Active", false }
            });

            basicTable.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "Bob | Jones" }, // Test escaping pipes
                { "Age", null }, // Test null value
                { "Balance", 500 },
                { "Active", true }
            });

            // Generate and display markdown
            string markdown = basicTable.ToMarkdown();
            Console.WriteLine(markdown);
            Console.WriteLine();
        }

        private static void TestComplexTypes()
        {
            Console.WriteLine("Test 3: DateTime and complex types");
            Console.WriteLine("----------------------------------");

            // Create a table with datetime and complex types
            SerializableDataTable complexTable = new SerializableDataTable("Complex Types Table");

            // Add columns
            complexTable.Columns.Add(new SerializableColumn { Name = "ID", Type = ColumnValueType.Guid });
            complexTable.Columns.Add(new SerializableColumn { Name = "CreatedAt", Type = ColumnValueType.DateTime });
            complexTable.Columns.Add(new SerializableColumn { Name = "UpdatedAt", Type = ColumnValueType.DateTimeOffset });
            complexTable.Columns.Add(new SerializableColumn { Name = "Metadata", Type = ColumnValueType.Object });

            // Add rows
            DateTime now = DateTime.Now;
            DateTimeOffset nowOffset = DateTimeOffset.Now;

            // First row with microsecond precision DateTime
            complexTable.Rows.Add(new Dictionary<string, object>
            {
                { "ID", Guid.NewGuid() },
                { "CreatedAt", now },
                { "UpdatedAt", nowOffset },
                { "Metadata", new { Tags = new[] { "important", "urgent" }, Priority = 1 } }
            });

            // Second row with null complex object
            complexTable.Rows.Add(new Dictionary<string, object>
            {
                { "ID", Guid.NewGuid() },
                { "CreatedAt", now.AddDays(-1) },
                { "UpdatedAt", nowOffset.AddDays(-1) },
                { "Metadata", null }
            });

            // Generate and display markdown
            string markdown = complexTable.ToMarkdown();
            Console.WriteLine(markdown);
            Console.WriteLine();
        }

        private static void TestAllSupportedTypes()
        {
            Console.WriteLine("Test 4: All supported types");
            Console.WriteLine("--------------------------");

            // Create a table with all supported types
            SerializableDataTable allTypesTable = new SerializableDataTable("All Types Table");

            // Add one column for each supported type
            allTypesTable.Columns.Add(new SerializableColumn { Name = "String", Type = ColumnValueType.String });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Int32", Type = ColumnValueType.Int32 });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Int64", Type = ColumnValueType.Int64 });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Decimal", Type = ColumnValueType.Decimal });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Double", Type = ColumnValueType.Double });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Float", Type = ColumnValueType.Float });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Boolean", Type = ColumnValueType.Boolean });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "DateTime", Type = ColumnValueType.DateTime });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "DateTimeOffset", Type = ColumnValueType.DateTimeOffset });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Byte", Type = ColumnValueType.Byte });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "ByteArray", Type = ColumnValueType.ByteArray });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Char", Type = ColumnValueType.Char });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Guid", Type = ColumnValueType.Guid });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Object", Type = ColumnValueType.Object });

            // Add a row with values for each type
            DateTime preciseDateTime = new DateTime(2023, 4, 15, 13, 45, 30, 123);
            DateTimeOffset preciseDateTimeOffset = new DateTimeOffset(2023, 4, 15, 13, 45, 30, 456, TimeSpan.FromHours(-5));

            allTypesTable.Rows.Add(new Dictionary<string, object>
            {
                { "String", "Test String" },
                { "Int32", 12345 },
                { "Int64", 9223372036854775807L }, // Max long value
                { "Decimal", 123.456789m },
                { "Double", 123.456789d },
                { "Float", 123.456f },
                { "Boolean", true },
                { "DateTime", preciseDateTime },
                { "DateTimeOffset", preciseDateTimeOffset },
                { "Byte", (byte)255 },
                { "ByteArray", new byte[] { 1, 2, 3, 4, 5 } },
                { "Char", 'A' },
                { "Guid", Guid.NewGuid() },
                { "Object", new { Name = "Test Object", Value = 42 } }
            });

            // Generate and display markdown
            string markdown = allTypesTable.ToMarkdown();
            Console.WriteLine(markdown);
        }
    }
}