namespace Test.MarkdownConverter
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using SerializableDataTables;

    public static class Program
    {
        private static int _TestsPassed = 0;
        private static int _TestsFailed = 0;
        private static List<string> _FailedTests = new List<string>();

        public static void Main(string[] args)
        {
            Console.WriteLine("=== MarkdownConverter Test Program ===\n");

            try
            {
                // Test 1: Newline character configuration
                TestNewlineCharacters();

                // Test 2: DataTable conversion
                TestDataTableConversion();

                // Test 3: SerializableDataTable conversion
                TestSerializableDataTableConversion();

                // Test 4: Empty table handling
                TestEmptyTables();

                // Test 5: Null input handling
                TestNullInputHandling();

                // Test 6: Invalid row index handling
                TestInvalidRowIndex();

                // Test 7: Array types
                TestArrayTypes();

                // Test 8: Pipe character escaping
                TestPipeEscaping();

                // Test 9: Row iteration
                TestRowIteration();

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

        private static void TestNewlineCharacters()
        {
            Console.WriteLine("Test 1: Newline character configuration");
            Console.WriteLine("----------------------------------------");

            DataTable dt = new DataTable("Newline Test");
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, "Test");

            // Test with default newline (Environment.NewLine)
            MarkdownConverter.NewlineCharacter = Environment.NewLine;
            string defaultResult = MarkdownConverter.Convert(dt);
            AssertTest("Default newline produces non-null result", defaultResult != null);

            // Test with custom newline (\n)
            MarkdownConverter.NewlineCharacter = "\n";
            string unixResult = MarkdownConverter.Convert(dt);
            AssertTest("Unix newline (\\n) produces non-null result", unixResult != null);
            AssertTest("Unix newline result contains \\n", unixResult != null && unixResult.Contains("\n"));

            // Test with custom newline (\r\n)
            MarkdownConverter.NewlineCharacter = "\r\n";
            string windowsResult = MarkdownConverter.Convert(dt);
            AssertTest("Windows newline (\\r\\n) produces non-null result", windowsResult != null);
            AssertTest("Windows newline result contains \\r\\n", windowsResult != null && windowsResult.Contains("\r\n"));

            // Compare lengths - Windows should be longer due to extra \r characters
            AssertTest("Windows result is longer than Unix result", windowsResult.Length > unixResult.Length);

            // Reset to default for other tests
            MarkdownConverter.NewlineCharacter = Environment.NewLine;

            Console.WriteLine();
        }

        private static void TestDataTableConversion()
        {
            Console.WriteLine("Test 2: DataTable conversion");
            Console.WriteLine("----------------------------");

            DataTable dt = CreateTestDataTable();

            // Test full conversion
            string fullMarkdown = MarkdownConverter.Convert(dt);
            AssertTest("Full DataTable conversion returns non-null", fullMarkdown != null);
            AssertTest("Full conversion contains header row", fullMarkdown != null && fullMarkdown.Contains("| ID |"));
            AssertTest("Full conversion contains separator", fullMarkdown != null && fullMarkdown.Contains("|---|"));
            AssertTest("Full conversion contains data row", fullMarkdown != null && fullMarkdown.Contains("John Doe"));

            // Test headers only
            string headers = MarkdownConverter.ConvertHeaders(dt);
            AssertTest("ConvertHeaders returns non-null", headers != null);
            AssertTest("Headers contain column names", headers != null && headers.Contains("ID") && headers.Contains("Name"));
            AssertTest("Headers contain separator line", headers != null && headers.Contains("|---|"));

            // Test specific row
            string row1 = MarkdownConverter.ConvertRow(dt, 1);
            AssertTest("ConvertRow returns non-null", row1 != null);
            AssertTest("Row 1 contains 'Jane Smith'", row1 != null && row1.Contains("Jane Smith"));

            Console.WriteLine();
        }

        private static void TestSerializableDataTableConversion()
        {
            Console.WriteLine("Test 3: SerializableDataTable conversion");
            Console.WriteLine("-----------------------------------------");

            SerializableDataTable sdt = CreateTestSerializableDataTable();

            // Test full conversion
            string fullMarkdown = MarkdownConverter.Convert(sdt);
            AssertTest("Full SerializableDataTable conversion returns non-null", fullMarkdown != null);
            AssertTest("Full conversion contains header row", fullMarkdown != null && fullMarkdown.Contains("| ID |"));
            AssertTest("Full conversion contains data row", fullMarkdown != null && fullMarkdown.Contains("John Doe"));

            // Test headers only
            string headers = MarkdownConverter.ConvertHeaders(sdt);
            AssertTest("ConvertHeaders for SDT returns non-null", headers != null);

            // Test specific row
            string row0 = MarkdownConverter.ConvertRow(sdt, 0);
            AssertTest("ConvertRow for SDT returns non-null", row0 != null);

            // Test ToMarkdown method matches Convert
            string toMarkdownResult = sdt.ToMarkdown();
            AssertTest("ToMarkdown returns non-null", toMarkdownResult != null);
            AssertTest("ToMarkdown matches Convert result", toMarkdownResult == fullMarkdown);

            Console.WriteLine();
        }

        private static void TestEmptyTables()
        {
            Console.WriteLine("Test 4: Empty table handling");
            Console.WriteLine("----------------------------");

            // Test empty DataTable (no columns)
            DataTable emptyDt = new DataTable("Empty DataTable");
            string emptyDtResult = MarkdownConverter.Convert(emptyDt);
            AssertTest("Empty DataTable (no columns) returns null", emptyDtResult == null);

            // Test DataTable with columns but no rows
            DataTable columnsOnlyDt = new DataTable("Columns Only");
            columnsOnlyDt.Columns.Add("Col1", typeof(string));
            columnsOnlyDt.Columns.Add("Col2", typeof(int));
            string columnsOnlyResult = MarkdownConverter.Convert(columnsOnlyDt);
            AssertTest("DataTable with columns but no rows returns non-null", columnsOnlyResult != null);
            AssertTest("Result contains column headers", columnsOnlyResult != null && columnsOnlyResult.Contains("Col1"));

            // Test empty SerializableDataTable (no columns)
            SerializableDataTable emptySdt = new SerializableDataTable("Empty SerializableDataTable");
            string emptySdtResult = MarkdownConverter.Convert(emptySdt);
            AssertTest("Empty SerializableDataTable (no columns) returns null", emptySdtResult == null);

            // Test SerializableDataTable with columns but no rows
            SerializableDataTable columnsOnlySdt = new SerializableDataTable("Columns Only");
            columnsOnlySdt.Columns.Add(new SerializableColumn { Name = "Col1", Type = ColumnValueTypeEnum.String });
            columnsOnlySdt.Columns.Add(new SerializableColumn { Name = "Col2", Type = ColumnValueTypeEnum.Int32 });
            string columnsOnlySdtResult = MarkdownConverter.Convert(columnsOnlySdt);
            AssertTest("SerializableDataTable with columns but no rows returns non-null", columnsOnlySdtResult != null);

            Console.WriteLine();
        }

        private static void TestNullInputHandling()
        {
            Console.WriteLine("Test 5: Null input handling");
            Console.WriteLine("---------------------------");

            // Test with null DataTable
            string nullDtResult = MarkdownConverter.Convert(null as DataTable);
            AssertTest("Null DataTable returns null", nullDtResult == null);

            // Test with null SerializableDataTable
            string nullSdtResult = MarkdownConverter.Convert(null as SerializableDataTable);
            AssertTest("Null SerializableDataTable returns null", nullSdtResult == null);

            Console.WriteLine();
        }

        private static void TestInvalidRowIndex()
        {
            Console.WriteLine("Test 6: Invalid row index handling");
            Console.WriteLine("-----------------------------------");

            // Test with DataTable
            DataTable dt = new DataTable();
            dt.Columns.Add("Test", typeof(string));
            dt.Rows.Add("Value");

            bool dtExceptionThrown = false;
            try
            {
                MarkdownConverter.ConvertRow(dt, 1); // Only has 1 row (index 0)
            }
            catch (ArgumentOutOfRangeException)
            {
                dtExceptionThrown = true;
            }
            AssertTest("DataTable invalid row index throws ArgumentOutOfRangeException", dtExceptionThrown);

            // Test with SerializableDataTable
            SerializableDataTable sdt = new SerializableDataTable();
            sdt.Columns.Add(new SerializableColumn { Name = "Test", Type = ColumnValueTypeEnum.String });
            sdt.Rows.Add(new Dictionary<string, object> { { "Test", "Value" } });

            bool sdtExceptionThrown = false;
            try
            {
                MarkdownConverter.ConvertRow(sdt, 1); // Only has 1 row (index 0)
            }
            catch (ArgumentOutOfRangeException)
            {
                sdtExceptionThrown = true;
            }
            AssertTest("SerializableDataTable invalid row index throws ArgumentOutOfRangeException", sdtExceptionThrown);

            Console.WriteLine();
        }

        private static void TestArrayTypes()
        {
            Console.WriteLine("Test 7: Array types");
            Console.WriteLine("-------------------");

            // Test DataTable with array columns
            DataTable dt = new DataTable("ArrayTest");
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("FloatArray", typeof(object));
            dt.Columns.Add("IntArray", typeof(object));
            dt.Columns.Add("StringArray", typeof(object));

            dt.Rows.Add(1, new float[] { 0.1f, 0.2f, 0.3f }, new int[] { 1, 2, 3 }, new string[] { "a", "b", "c" });
            dt.Rows.Add(2, null, null, null);

            string dtMarkdown = MarkdownConverter.Convert(dt);
            AssertTest("DataTable with arrays converts to markdown", dtMarkdown != null);
            AssertTest("Array markdown contains array representation", dtMarkdown != null && dtMarkdown.Contains("["));

            // Test SerializableDataTable with array columns
            SerializableDataTable sdt = new SerializableDataTable("SerializableArrayTest");
            sdt.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueTypeEnum.String });
            sdt.Columns.Add(new SerializableColumn { Name = "Embedding", Type = ColumnValueTypeEnum.Object });

            sdt.Rows.Add(new Dictionary<string, object>
            {
                { "Name", "Document 1" },
                { "Embedding", new float[] { 0.1f, 0.2f, 0.3f, 0.4f } }
            });

            string sdtMarkdown = MarkdownConverter.Convert(sdt);
            AssertTest("SerializableDataTable with arrays converts to markdown", sdtMarkdown != null);
            AssertTest("SDT array markdown contains 'Document 1'", sdtMarkdown != null && sdtMarkdown.Contains("Document 1"));

            Console.WriteLine();
        }

        private static void TestPipeEscaping()
        {
            Console.WriteLine("Test 8: Pipe character escaping");
            Console.WriteLine("--------------------------------");

            DataTable dt = new DataTable("PipeTest");
            dt.Columns.Add("Text", typeof(string));
            dt.Rows.Add("Value | With | Pipes");

            string markdown = MarkdownConverter.Convert(dt);
            AssertTest("Table with pipes converts to markdown", markdown != null);
            AssertTest("Pipes are escaped as \\|", markdown != null && markdown.Contains("\\|"));

            SerializableDataTable sdt = new SerializableDataTable("PipeTest");
            sdt.Columns.Add(new SerializableColumn { Name = "Text", Type = ColumnValueTypeEnum.String });
            sdt.Rows.Add(new Dictionary<string, object> { { "Text", "Another | Pipe | Test" } });

            string sdtMarkdown = MarkdownConverter.Convert(sdt);
            AssertTest("SDT with pipes converts to markdown", sdtMarkdown != null);
            AssertTest("SDT pipes are escaped", sdtMarkdown != null && sdtMarkdown.Contains("\\|"));

            Console.WriteLine();
        }

        private static void TestRowIteration()
        {
            Console.WriteLine("Test 9: Row iteration");
            Console.WriteLine("---------------------");

            DataTable dt = new DataTable("IterationTest");
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, "First");
            dt.Rows.Add(2, "Second");
            dt.Rows.Add(3, "Third");

            // Test DataTable row iteration
            List<string> dtRows = MarkdownConverter.IterateRows(dt).ToList();
            AssertTest("DataTable IterateRows returns correct count", dtRows.Count == 3);
            AssertTest("First iterated row contains 'First'", dtRows.Count > 0 && dtRows[0].Contains("First"));
            AssertTest("Last iterated row contains 'Third'", dtRows.Count == 3 && dtRows[2].Contains("Third"));

            // Test SerializableDataTable row iteration
            SerializableDataTable sdt = new SerializableDataTable("IterationTest");
            sdt.Columns.Add(new SerializableColumn { Name = "ID", Type = ColumnValueTypeEnum.Int32 });
            sdt.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueTypeEnum.String });
            sdt.Rows.Add(new Dictionary<string, object> { { "ID", 1 }, { "Name", "Alpha" } });
            sdt.Rows.Add(new Dictionary<string, object> { { "ID", 2 }, { "Name", "Beta" } });

            List<string> sdtRows = MarkdownConverter.IterateRows(sdt).ToList();
            AssertTest("SDT IterateRows returns correct count", sdtRows.Count == 2);
            AssertTest("First SDT iterated row contains 'Alpha'", sdtRows.Count > 0 && sdtRows[0].Contains("Alpha"));

            Console.WriteLine();
        }

        private static DataTable CreateTestDataTable()
        {
            DataTable dt = new DataTable("Test DataTable");

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("IsActive", typeof(bool));
            dt.Columns.Add("Balance", typeof(decimal));

            dt.Rows.Add(1, "John Doe", true, 1234.56m);
            dt.Rows.Add(2, "Jane Smith", false, 7890.12m);
            dt.Rows.Add(3, "Bob Jones", true, 500m);

            return dt;
        }

        private static SerializableDataTable CreateTestSerializableDataTable()
        {
            SerializableDataTable sdt = new SerializableDataTable("Test SerializableDataTable");

            sdt.Columns.Add(new SerializableColumn { Name = "ID", Type = ColumnValueTypeEnum.Int32 });
            sdt.Columns.Add(new SerializableColumn { Name = "Name", Type = ColumnValueTypeEnum.String });
            sdt.Columns.Add(new SerializableColumn { Name = "IsActive", Type = ColumnValueTypeEnum.Boolean });
            sdt.Columns.Add(new SerializableColumn { Name = "Balance", Type = ColumnValueTypeEnum.Decimal });

            sdt.Rows.Add(new Dictionary<string, object>
            {
                { "ID", 1 },
                { "Name", "John Doe" },
                { "IsActive", true },
                { "Balance", 1234.56m }
            });

            sdt.Rows.Add(new Dictionary<string, object>
            {
                { "ID", 2 },
                { "Name", "Jane Smith" },
                { "IsActive", false },
                { "Balance", 7890.12m }
            });

            return sdt;
        }
    }
}
