namespace Test.Markdown
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.Json;

    using SerializableDataTables;

    public static class Program
    {
        private static int _TestsPassed = 0;
        private static int _TestsFailed = 0;
        private static List<string> _FailedTests = new List<string>();

        public static void Main(string[] args)
        {
            Console.WriteLine("=== SerializableDataTable ToMarkdown() Test Program ===\n");

            try
            {
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

                // Test 6: Special character escaping
                TestSpecialCharacterEscaping();

                // Test 7: Null value handling
                TestNullValueHandling();

                // Print summary
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
            catch (Exception ex)
            {
                Console.WriteLine($"\nError occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
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

        private static void TestEmptyTable()
        {
            Console.WriteLine("Test 1: Empty table");
            Console.WriteLine("-------------------");

            // Test with no columns - should return null
            SerializableDataTable emptyTable = new SerializableDataTable("Empty Table");
            string markdown = emptyTable.ToMarkdown();
            AssertTest("Empty table (no columns) returns null", markdown == null);

            // Add columns but no rows - should return headers only
            emptyTable.Columns.Add(new SerializableColumn { Name = "Column1", Type = ColumnValueTypeEnum.String });
            emptyTable.Columns.Add(new SerializableColumn { Name = "Column2", Type = ColumnValueTypeEnum.Int32 });
            markdown = emptyTable.ToMarkdown();

            AssertTest("Table with columns but no rows returns non-null", markdown != null);
            AssertTest("Table with columns contains header row", markdown != null && markdown.Contains("| Column1 |"));
            AssertTest("Table with columns contains separator row", markdown != null && markdown.Contains("---"));

            Console.WriteLine();
        }

        private static void TestBasicTypes()
        {
            Console.WriteLine("Test 2: Basic types");
            Console.WriteLine("-------------------");

            SerializableDataTable basicTable = new SerializableDataTable("Basic Types Table");

            basicTable.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueTypeEnum.String });
            basicTable.Columns.Add(new SerializableColumn { Name = "Age", Type = ColumnValueTypeEnum.Int32 });
            basicTable.Columns.Add(new SerializableColumn { Name = "Balance", Type = ColumnValueTypeEnum.Decimal });
            basicTable.Columns.Add(new SerializableColumn { Name = "Active", Type = ColumnValueTypeEnum.Boolean });

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

            string markdown = basicTable.ToMarkdown();

            AssertTest("Basic types table returns non-null", markdown != null);
            AssertTest("Contains string value 'John Doe'", markdown != null && markdown.Contains("John Doe"));
            AssertTest("Contains integer value '30'", markdown != null && markdown.Contains("30"));
            AssertTest("Contains decimal value '1234.56'", markdown != null && markdown.Contains("1234.56"));
            AssertTest("Contains boolean 'True'", markdown != null && markdown.Contains("True"));
            AssertTest("Contains boolean 'False'", markdown != null && markdown.Contains("False"));

            // Test ConvertHeaders
            string headers = MarkdownConverter.ConvertHeaders(basicTable);
            AssertTest("ConvertHeaders returns non-null", headers != null);
            AssertTest("Headers contain column names", headers != null && headers.Contains("Name") && headers.Contains("Age"));

            // Test ConvertRow
            string row0 = MarkdownConverter.ConvertRow(basicTable, 0);
            AssertTest("ConvertRow returns non-null", row0 != null);
            AssertTest("Row 0 contains 'John Doe'", row0 != null && row0.Contains("John Doe"));

            Console.WriteLine();
        }

        private static void TestComplexTypes()
        {
            Console.WriteLine("Test 3: DateTime and complex types");
            Console.WriteLine("----------------------------------");

            SerializableDataTable complexTable = new SerializableDataTable("Complex Types Table");

            complexTable.Columns.Add(new SerializableColumn { Name = "ID", Type = ColumnValueTypeEnum.Guid });
            complexTable.Columns.Add(new SerializableColumn { Name = "CreatedAt", Type = ColumnValueTypeEnum.DateTime });
            complexTable.Columns.Add(new SerializableColumn { Name = "UpdatedAt", Type = ColumnValueTypeEnum.DateTimeOffset });

            DateTime testDate = new DateTime(2024, 6, 15, 14, 30, 45);
            DateTimeOffset testDateOffset = new DateTimeOffset(2024, 6, 15, 14, 30, 45, TimeSpan.FromHours(-5));
            Guid testGuid = Guid.NewGuid();

            complexTable.Rows.Add(new Dictionary<string, object>
            {
                { "ID", testGuid },
                { "CreatedAt", testDate },
                { "UpdatedAt", testDateOffset }
            });

            string markdown = complexTable.ToMarkdown();

            AssertTest("Complex types table returns non-null", markdown != null);
            AssertTest("Contains GUID value", markdown != null && markdown.Contains(testGuid.ToString()));
            AssertTest("Contains DateTime value", markdown != null && markdown.Contains("2024"));

            Console.WriteLine();
        }

        private static void TestAllSupportedTypes()
        {
            Console.WriteLine("Test 4: All supported types");
            Console.WriteLine("---------------------------");

            SerializableDataTable allTypesTable = new SerializableDataTable("All Types Table");

            allTypesTable.Columns.Add(new SerializableColumn { Name = "String", Type = ColumnValueTypeEnum.String });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Int32", Type = ColumnValueTypeEnum.Int32 });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Int64", Type = ColumnValueTypeEnum.Int64 });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Decimal", Type = ColumnValueTypeEnum.Decimal });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Double", Type = ColumnValueTypeEnum.Double });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Float", Type = ColumnValueTypeEnum.Float });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Boolean", Type = ColumnValueTypeEnum.Boolean });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Byte", Type = ColumnValueTypeEnum.Byte });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Char", Type = ColumnValueTypeEnum.Char });
            allTypesTable.Columns.Add(new SerializableColumn { Name = "Guid", Type = ColumnValueTypeEnum.Guid });

            Guid testGuid = Guid.NewGuid();

            allTypesTable.Rows.Add(new Dictionary<string, object>
            {
                { "String", "Test String" },
                { "Int32", 12345 },
                { "Int64", 9223372036854775807L },
                { "Decimal", 123.456789m },
                { "Double", 123.456789d },
                { "Float", 123.456f },
                { "Boolean", true },
                { "Byte", (byte)255 },
                { "Char", 'A' },
                { "Guid", testGuid }
            });

            string markdown = allTypesTable.ToMarkdown();

            AssertTest("All types table returns non-null", markdown != null);
            AssertTest("Contains String column header", markdown != null && markdown.Contains("| String |"));
            AssertTest("Contains string value", markdown != null && markdown.Contains("Test String"));
            AssertTest("Contains Int32 value", markdown != null && markdown.Contains("12345"));
            AssertTest("Contains Int64 value", markdown != null && markdown.Contains("9223372036854775807"));
            AssertTest("Contains Byte value '255'", markdown != null && markdown.Contains("255"));
            AssertTest("Contains Char value 'A'", markdown != null && markdown.Contains("| A |"));
            AssertTest("Contains Guid value", markdown != null && markdown.Contains(testGuid.ToString()));

            Console.WriteLine();
        }

        private static void TestArrayTypes()
        {
            Console.WriteLine("Test 5: Array types");
            Console.WriteLine("-------------------");

            SerializableDataTable arrayTable = new SerializableDataTable("Array Types Table");

            arrayTable.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueTypeEnum.String });
            arrayTable.Columns.Add(new SerializableColumn { Name = "FloatArray", Type = ColumnValueTypeEnum.Object });
            arrayTable.Columns.Add(new SerializableColumn { Name = "IntArray", Type = ColumnValueTypeEnum.Object });
            arrayTable.Columns.Add(new SerializableColumn { Name = "StringArray", Type = ColumnValueTypeEnum.Object });

            arrayTable.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "Document 1" },
                { "FloatArray", new float[] { 0.1f, 0.2f, 0.3f } },
                { "IntArray", new int[] { 1, 2, 3 } },
                { "StringArray", new string[] { "hello", "world" } }
            });

            string markdown = arrayTable.ToMarkdown();

            AssertTest("Array types table returns non-null", markdown != null);
            AssertTest("Contains Document 1", markdown != null && markdown.Contains("Document 1"));
            // Arrays should be serialized as JSON
            AssertTest("Contains array representation", markdown != null && (markdown.Contains("[") || markdown.Contains("0.1")));

            // Test from DataTable
            DataTable dt = new DataTable("DataTable with Arrays");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Embedding", typeof(object));
            dt.Rows.Add(1, new float[] { 0.1f, 0.2f, 0.3f });

            SerializableDataTable sdtFromDt = SerializableDataTable.FromDataTable(dt);
            string markdownFromDt = sdtFromDt.ToMarkdown();

            AssertTest("DataTable with arrays converts to markdown", markdownFromDt != null);

            Console.WriteLine();
        }

        private static void TestSpecialCharacterEscaping()
        {
            Console.WriteLine("Test 6: Special character escaping");
            Console.WriteLine("-----------------------------------");

            SerializableDataTable table = new SerializableDataTable("Escaping Test");

            table.Columns.Add(new SerializableColumn { Name = "Text", Type = ColumnValueTypeEnum.String });

            // Test pipe character which needs escaping in markdown tables
            table.Rows.Add(new Dictionary<string, object>
            {
                { "Text", "Value | With | Pipes" }
            });

            string markdown = table.ToMarkdown();

            AssertTest("Pipe escaping table returns non-null", markdown != null);
            // Pipes should be escaped as \|
            AssertTest("Pipes are escaped", markdown != null && markdown.Contains("\\|"));

            Console.WriteLine();
        }

        private static void TestNullValueHandling()
        {
            Console.WriteLine("Test 7: Null value handling");
            Console.WriteLine("---------------------------");

            SerializableDataTable table = new SerializableDataTable("Null Test");

            table.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueTypeEnum.String });
            table.Columns.Add(new SerializableColumn { Name = "Value", Type = ColumnValueTypeEnum.Int32 });

            table.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "Row with null" },
                { "Value", null }
            });

            table.Rows.Add(new Dictionary<string, object>
            {
                { "Name", null },
                { "Value", 42 }
            });

            string markdown = table.ToMarkdown();

            AssertTest("Null handling table returns non-null", markdown != null);
            AssertTest("Contains non-null value 'Row with null'", markdown != null && markdown.Contains("Row with null"));
            AssertTest("Contains non-null value '42'", markdown != null && markdown.Contains("42"));
            // Null values should result in empty cells or some representation
            AssertTest("Table has expected row count", markdown != null && markdown.Split('\n').Length >= 4);

            Console.WriteLine();
        }
    }
}
