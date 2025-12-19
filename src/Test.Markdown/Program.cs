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

            // Test 5: Array types (complex data types)
            TestArrayTypes();

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
            emptyTable.Columns.Add(new SerializableColumn { Name = "Column1", Type = ColumnValueTypeEnum.String });
            markdown = emptyTable.ToMarkdown();
            Console.WriteLine("Table with columns but no rows:");
            Console.WriteLine(markdown);
            Console.WriteLine();
        }

        private static void TestBasicTypes()
        {
            Console.WriteLine("Test 2a: Basic types");
            Console.WriteLine("--------------------");

            // Create a table with basic types
            SerializableDataTable basicTable = new SerializableDataTable("Basic Types Table");

            // Add columns
            basicTable.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueTypeEnum.String });
            basicTable.Columns.Add(new SerializableColumn { Name = "Age", Type = ColumnValueTypeEnum.Int32 });
            basicTable.Columns.Add(new SerializableColumn { Name = "Balance", Type = ColumnValueTypeEnum.Decimal });
            basicTable.Columns.Add(new SerializableColumn { Name = "Active", Type = ColumnValueTypeEnum.Boolean });

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

            Console.WriteLine("Test 2b: Converter");
            Console.WriteLine("------------------");
            string headers = MarkdownConverter.ConvertHeaders(basicTable);
            Console.WriteLine("Headers:" + Environment.NewLine + headers);

            for (int i = 0; i < basicTable.Rows.Count; i++)
            {
                string row = MarkdownConverter.ConvertRow(basicTable, i);
                Console.WriteLine("Row " + i + ":" + Environment.NewLine + row);
            }

            Console.WriteLine("");
            Console.WriteLine("Row 0 with header:");
            Console.WriteLine(headers + MarkdownConverter.ConvertRow(basicTable, 0));
            Console.WriteLine();
        }

        private static void TestComplexTypes()
        {
            Console.WriteLine("Test 3: DateTime and complex types");
            Console.WriteLine("----------------------------------");

            // Create a table with datetime and complex types
            SerializableDataTable complexTable = new SerializableDataTable("Complex Types Table");

            // Add columns
            complexTable.Columns.Add(new SerializableColumn { Name = "ID", Type = ColumnValueTypeEnum.Guid });
            complexTable.Columns.Add(new SerializableColumn { Name = "CreatedAt", Type = ColumnValueTypeEnum.DateTime });
            complexTable.Columns.Add(new SerializableColumn { Name = "UpdatedAt", Type = ColumnValueTypeEnum.DateTimeOffset });
            complexTable.Columns.Add(new SerializableColumn { Name = "Metadata", Type = ColumnValueTypeEnum.Object });

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
            allTypesTable.Columns.Add(new SerializableColumn { Name = "String", Type = ColumnValueTypeEnum.String });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Int32", Type = ColumnValueTypeEnum.Int32 });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Int64", Type = ColumnValueTypeEnum.Int64 });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Decimal", Type = ColumnValueTypeEnum.Decimal });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Double", Type = ColumnValueTypeEnum.Double });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Float", Type = ColumnValueTypeEnum.Float });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Boolean", Type = ColumnValueTypeEnum.Boolean });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "DateTime", Type = ColumnValueTypeEnum.DateTime });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "DateTimeOffset", Type = ColumnValueTypeEnum.DateTimeOffset });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Byte", Type = ColumnValueTypeEnum.Byte });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "ByteArray", Type = ColumnValueTypeEnum.ByteArray });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Char", Type = ColumnValueTypeEnum.Char });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Guid", Type = ColumnValueTypeEnum.Guid });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Object", Type = ColumnValueTypeEnum.Object });

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

        private static void TestArrayTypes()
        {
            Console.WriteLine("Test 5: Array types (complex data types)");
            Console.WriteLine("-----------------------------------------");

            // Create a table with array types
            SerializableDataTable arrayTable = new SerializableDataTable("Array Types Table");

            // Add columns
            arrayTable.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueTypeEnum.String });
            arrayTable.Columns.Add(new SerializableColumn { Name = "FloatArray", Type = ColumnValueTypeEnum.Object });
            arrayTable.Columns.Add(new SerializableColumn { Name = "IntArray", Type = ColumnValueTypeEnum.Object });
            arrayTable.Columns.Add(new SerializableColumn { Name = "StringArray", Type = ColumnValueTypeEnum.Object });
            arrayTable.Columns.Add(new SerializableColumn { Name = "BoolArray", Type = ColumnValueTypeEnum.Object });

            // Add rows with array data
            arrayTable.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "Document 1" },
                { "FloatArray", new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f } },
                { "IntArray", new int[] { 1, 2, 3, 4, 5 } },
                { "StringArray", new string[] { "hello", "world", "test" } },
                { "BoolArray", new bool[] { true, false, true } }
            });

            arrayTable.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "Document 2" },
                { "FloatArray", new float[] { -0.5f, 0f, 0.5f } },
                { "IntArray", new int[] { 10, 20, 30 } },
                { "StringArray", new string[] { "foo", "bar" } },
                { "BoolArray", new bool[] { false, false } }
            });

            arrayTable.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "Document 3 (nulls)" },
                { "FloatArray", null },
                { "IntArray", null },
                { "StringArray", null },
                { "BoolArray", null }
            });

            // Generate and display markdown
            string markdown = arrayTable.ToMarkdown();
            Console.WriteLine(markdown);
            Console.WriteLine();

            // Also test converting from DataTable
            Console.WriteLine("Test 5b: Array types from DataTable");
            Console.WriteLine("------------------------------------");

            DataTable dt = new DataTable("DataTable with Arrays");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Embedding", typeof(object));
            dt.Columns.Add("Tags", typeof(object));

            dt.Rows.Add(1, new float[] { 0.1f, 0.2f, 0.3f }, new string[] { "tag1", "tag2" });
            dt.Rows.Add(2, new double[] { 1.1, 2.2, 3.3 }, new int[] { 100, 200, 300 });

            SerializableDataTable sdtFromDt = SerializableDataTable.FromDataTable(dt);
            string markdownFromDt = sdtFromDt.ToMarkdown();
            Console.WriteLine(markdownFromDt);
        }
    }
}