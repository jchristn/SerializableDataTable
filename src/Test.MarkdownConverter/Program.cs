namespace Test.MarkdownConverter
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Text.Json;
    using SerializableDataTables;

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Testing MarkdownConverter functionality");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            // Test with different newline characters
            TestNewlineCharacters();

            // Test with DataTable
            Console.WriteLine("\nTesting with DataTable:");
            Console.WriteLine("----------------------");
            DataTable dt = CreateTestDataTable();
            TestWithDataTable(dt);

            // Test with SerializableDataTable
            Console.WriteLine("\nTesting with SerializableDataTable:");
            Console.WriteLine("--------------------------------");
            SerializableDataTable sdt = CreateTestSerializableDataTable();
            TestWithSerializableDataTable(sdt);

            // Test with empty tables
            Console.WriteLine("\nTesting with empty tables:");
            Console.WriteLine("------------------------");
            TestEmptyTables();

            // Test error handling
            Console.WriteLine("\nTesting error handling:");
            Console.WriteLine("---------------------");
            TestErrorHandling();

            Console.WriteLine("\nAll tests completed.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        #region Test Methods

        private static void TestNewlineCharacters()
        {
            Console.WriteLine("Testing different newline characters:");
            Console.WriteLine("-----------------------------------");

            // Create a simple table for testing
            DataTable dt = new DataTable("Newline Test");
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, "Test");

            // Test with default newline (Environment.NewLine)
            Console.WriteLine("\nDefault newline (Environment.NewLine):");
            MarkdownConverter.NewlineCharacter = Environment.NewLine;
            string defaultResult = MarkdownConverter.Convert(dt);
            Console.WriteLine($"Result length: {defaultResult.Length}");
            Console.WriteLine(defaultResult);

            // Test with custom newline (\n)
            Console.WriteLine("\nCustom newline (\\n):");
            MarkdownConverter.NewlineCharacter = "\n";
            string unixResult = MarkdownConverter.Convert(dt);
            Console.WriteLine($"Result length: {unixResult.Length}");
            Console.WriteLine(unixResult);

            // Test with custom newline (\r\n)
            Console.WriteLine("\nCustom newline (\\r\\n):");
            MarkdownConverter.NewlineCharacter = "\r\n";
            string windowsResult = MarkdownConverter.Convert(dt);
            Console.WriteLine($"Result length: {windowsResult.Length}");
            Console.WriteLine(windowsResult);

            // Reset to default for other tests
            MarkdownConverter.NewlineCharacter = Environment.NewLine;
        }

        private static void TestWithDataTable(DataTable dt)
        {
            // Test full conversion
            Console.WriteLine("\nFull table conversion:");
            string fullMarkdown = MarkdownConverter.Convert(dt);
            Console.WriteLine(fullMarkdown);

            // Test headers only
            Console.WriteLine("\nHeaders only:");
            string headers = MarkdownConverter.ConvertHeaders(dt);
            Console.WriteLine(headers);

            // Test specific row
            Console.WriteLine("\nSpecific row (index 1):");
            string row = MarkdownConverter.ConvertRow(dt, 1);
            Console.WriteLine(row);

            // Test row iteration
            Console.WriteLine("\nIterating rows (first 2 rows):");
            int count = 0;
            foreach (string rowMarkdown in MarkdownConverter.IterateRows(dt))
            {
                Console.WriteLine(rowMarkdown);
                count++;
                if (count >= 2) break; // Just show first two rows
            }
        }

        private static void TestWithSerializableDataTable(SerializableDataTable sdt)
        {
            // Test full conversion
            Console.WriteLine("\nFull table conversion:");
            string fullMarkdown = MarkdownConverter.Convert(sdt);
            Console.WriteLine(fullMarkdown);

            // Test headers only
            Console.WriteLine("\nHeaders only:");
            string headers = MarkdownConverter.ConvertHeaders(sdt);
            Console.WriteLine(headers);

            // Test specific row
            Console.WriteLine("\nSpecific row (index 1):");
            string row = MarkdownConverter.ConvertRow(sdt, 1);
            Console.WriteLine(row);

            // Test row iteration
            Console.WriteLine("\nIterating rows (first 2 rows):");
            int count = 0;
            foreach (string rowMarkdown in MarkdownConverter.IterateRows(sdt))
            {
                Console.WriteLine(rowMarkdown);
                count++;
                if (count >= 2) break; // Just show first two rows
            }

            // Test ToMarkdown method that uses MarkdownConverter
            Console.WriteLine("\nTesting ToMarkdown method:");
            string toMarkdownResult = sdt.ToMarkdown();
            Console.WriteLine(toMarkdownResult);
            Console.WriteLine($"Convert and ToMarkdown results match: {toMarkdownResult == fullMarkdown}");
        }

        private static void TestEmptyTables()
        {
            // Test empty DataTable
            Console.WriteLine("\nEmpty DataTable:");
            DataTable emptyDt = new DataTable("Empty DataTable");
            string emptyDtResult = MarkdownConverter.Convert(emptyDt);
            Console.WriteLine($"Result: {(emptyDtResult == null ? "null (correct)" : "not null (incorrect)")}");

            // Test DataTable with columns but no rows
            Console.WriteLine("\nDataTable with columns but no rows:");
            DataTable columnsOnlyDt = new DataTable("Columns Only");
            columnsOnlyDt.Columns.Add("Col1", typeof(string));
            columnsOnlyDt.Columns.Add("Col2", typeof(int));
            string columnsOnlyResult = MarkdownConverter.Convert(columnsOnlyDt);
            Console.WriteLine(columnsOnlyResult);

            // Test empty SerializableDataTable
            Console.WriteLine("\nEmpty SerializableDataTable:");
            SerializableDataTable emptySdt = new SerializableDataTable("Empty SerializableDataTable");
            string emptySdtResult = MarkdownConverter.Convert(emptySdt);
            Console.WriteLine($"Result: {(emptySdtResult == null ? "null (correct)" : "not null (incorrect)")}");

            // Test SerializableDataTable with columns but no rows
            Console.WriteLine("\nSerializableDataTable with columns but no rows:");
            SerializableDataTable columnsOnlySdt = new SerializableDataTable("Columns Only");
            columnsOnlySdt.Columns.Add(new SerializableColumn { Name = "Col1", Type = ColumnValueType.String });
            columnsOnlySdt.Columns.Add(new SerializableColumn { Name = "Col2", Type = ColumnValueType.Int32 });
            string columnsOnlySdtResult = MarkdownConverter.Convert(columnsOnlySdt);
            Console.WriteLine(columnsOnlySdtResult);
        }

        private static void TestErrorHandling()
        {
            // Test with null DataTable
            Console.WriteLine("\nNull DataTable:");
            try
            {
                string result = MarkdownConverter.Convert(null as DataTable);
                Console.WriteLine($"Result: {(result == null ? "null" : "not null")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            // Test with null SerializableDataTable
            Console.WriteLine("\nNull SerializableDataTable:");
            try
            {
                string result = MarkdownConverter.Convert(null as SerializableDataTable);
                Console.WriteLine($"Result: {(result == null ? "null" : "not null")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            // Test ConvertRow with invalid row index for DataTable
            Console.WriteLine("\nInvalid row index for DataTable:");
            DataTable dt = new DataTable();
            dt.Columns.Add("Test", typeof(string));
            dt.Rows.Add("Value");
            try
            {
                string result = MarkdownConverter.ConvertRow(dt, 1); // Only has 1 row (index 0)
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception (expected): {ex.Message}");
            }

            // Test ConvertRow with invalid row index for SerializableDataTable
            Console.WriteLine("\nInvalid row index for SerializableDataTable:");
            SerializableDataTable sdt = new SerializableDataTable();
            sdt.Columns.Add(new SerializableColumn { Name = "Test", Type = ColumnValueType.String });
            sdt.Rows.Add(new Dictionary<string, object> { { "Test", "Value" } });
            try
            {
                string result = MarkdownConverter.ConvertRow(sdt, 1); // Only has 1 row (index 0)
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception (expected): {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private static DataTable CreateTestDataTable()
        {
            DataTable dt = new DataTable("Test DataTable");

            // Add columns with various types
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("IsActive", typeof(bool));
            dt.Columns.Add("Created", typeof(DateTime));
            dt.Columns.Add("LastLogin", typeof(DateTimeOffset));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("Data", typeof(byte[]));

            // Add rows with test data
            DateTime now = DateTime.Now;
            DateTimeOffset nowOffset = DateTimeOffset.Now;

            dt.Rows.Add(
                1,
                "John Doe",
                true,
                now,
                nowOffset,
                1234.56m,
                new byte[] { 1, 2, 3, 4, 5 }
            );

            dt.Rows.Add(
                2,
                "Jane Smith",
                false,
                now.AddDays(-1),
                nowOffset.AddDays(-1),
                7890.12m,
                Encoding.UTF8.GetBytes("Test binary data")
            );

            dt.Rows.Add(
                3,
                "Bob | Jones", // Test pipe character escaping
                true,
                now.AddDays(-2),
                nowOffset.AddDays(-2),
                500m,
                null
            );

            return dt;
        }

        private static SerializableDataTable CreateTestSerializableDataTable()
        {
            SerializableDataTable sdt = new SerializableDataTable("Test SerializableDataTable");

            // Add columns with various types
            sdt.Columns.Add(new SerializableColumn { Name = "ID", Type = ColumnValueType.Int32 });
            sdt.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueType.String });
            sdt.Columns.Add(new SerializableColumn { Name = "IsActive", Type = ColumnValueType.Boolean });
            sdt.Columns.Add(new SerializableColumn { Name = "Created", Type = ColumnValueType.DateTime });
            sdt.Columns.Add(new SerializableColumn { Name = "LastLogin", Type = ColumnValueType.DateTimeOffset });
            sdt.Columns.Add(new SerializableColumn { Name = "Balance", Type = ColumnValueType.Decimal });
            sdt.Columns.Add(new SerializableColumn { Name = "Data", Type = ColumnValueType.ByteArray });
            sdt.Columns.Add(new SerializableColumn { Name = "Metadata", Type = ColumnValueType.Object });

            // Add rows with test data
            DateTime now = DateTime.Now;
            DateTimeOffset nowOffset = DateTimeOffset.Now;

            // Add microsecond precision
            now = now.AddTicks(3456);
            nowOffset = nowOffset.AddTicks(7890);

            sdt.Rows.Add(new Dictionary<string, object>
            {
                { "ID", 1 },
                { "Name", "John Doe" },
                { "IsActive", true },
                { "Created", now },
                { "LastLogin", nowOffset },
                { "Balance", 1234.56m },
                { "Data", new byte[] { 1, 2, 3, 4, 5 } },
                { "Metadata", new { Tags = new[] { "vip", "premium" }, Notes = "Special customer" } }
            });

            sdt.Rows.Add(new Dictionary<string, object>
            {
                { "ID", 2 },
                { "Name", "Jane Smith" },
                { "IsActive", false },
                { "Created", now.AddDays(-1) },
                { "LastLogin", nowOffset.AddDays(-1) },
                { "Balance", 7890.12m },
                { "Data", Encoding.UTF8.GetBytes("Test binary data") },
                { "Metadata", new { Tags = new[] { "regular" }, Notes = "Standard account" } }
            });

            sdt.Rows.Add(new Dictionary<string, object>
            {
                { "ID", 3 },
                { "Name", "Bob | Jones" }, // Test pipe character escaping
                { "IsActive", true },
                { "Created", now.AddDays(-2) },
                { "LastLogin", nowOffset.AddDays(-2) },
                { "Balance", 500m },
                { "Data", null },
                { "Metadata", null }
            });

            return sdt;
        }

        #endregion
    }
}