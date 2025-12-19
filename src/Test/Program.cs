namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using SerializableDataTables;

    public class Program
    {
        private static int _TestsPassed = 0;
        private static int _TestsFailed = 0;

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

                // Test 5: Test all supported data types
                Console.WriteLine("\n\nTest 5: All Supported Data Types");
                Console.WriteLine("---------------------------------");
                DataTable typeTestTable = CreateDataTableWithVariousTypes();
                SerializableDataTable typeSerializable = SerializableDataTable.FromDataTable(typeTestTable);
                PrettyPrintSerializableDataTable(typeSerializable);
                DataTable convertedTypes = typeSerializable.ToDataTable();
                Console.WriteLine("\nConverted back to DataTable:");
                PrintDataTable(convertedTypes);

                // Test 6: Test complex/unknown data types (arrays, custom types like pgvector)
                Console.WriteLine("\n\nTest 6: Complex/Unknown Data Types (Arrays, Custom Types)");
                Console.WriteLine("----------------------------------------------------------");
                DataTable complexTypeTable = CreateDataTableWithComplexTypes();
                PrintDataTable(complexTypeTable);
                SerializableDataTable complexSerializable = SerializableDataTable.FromDataTable(complexTypeTable);
                Console.WriteLine("\nConverted to SerializableDataTable:");
                PrettyPrintSerializableDataTable(complexSerializable);

                // Test round-trip through JSON
                Console.WriteLine("\nRound-trip test (serialize to JSON and back):");
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                string complexJson = JsonSerializer.Serialize(complexSerializable, jsonOptions);
                SerializableDataTable deserializedComplex = JsonSerializer.Deserialize<SerializableDataTable>(complexJson, jsonOptions);
                DataTable complexRoundTrip = deserializedComplex.ToDataTable();
                PrintDataTable(complexRoundTrip);

                // Test 7: Test non-numeric array types
                Console.WriteLine("\n\nTest 7: Non-Numeric Array Types (string[], bool[], Guid[], DateTime[])");
                Console.WriteLine("------------------------------------------------------------------------");
                TestNonNumericArrayTypes();

                // Test 8: Array Type Preservation Tests
                Console.WriteLine("\n\n=== Test 8: Array Type Preservation ===");
                Console.WriteLine("========================================");
                TestArrayTypePreservation();

                Console.WriteLine("\n\n=== All tests completed! ===");
                Console.WriteLine($"Passed: {_TestsPassed}");
                Console.WriteLine($"Failed: {_TestsFailed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static void TestArrayTypePreservation()
        {
            Console.WriteLine("\n--- float[] Round-Trip Preservation ---");
            TestFloatArrayPreservation();

            Console.WriteLine("\n--- double[] Round-Trip Preservation ---");
            TestDoubleArrayPreservation();

            Console.WriteLine("\n--- int[] Round-Trip Preservation ---");
            TestIntArrayPreservation();

            Console.WriteLine("\n--- long[] Round-Trip Preservation ---");
            TestLongArrayPreservation();

            Console.WriteLine("\n--- decimal[] Round-Trip Preservation ---");
            TestDecimalArrayPreservation();

            Console.WriteLine("\n--- short[] Round-Trip Preservation ---");
            TestShortArrayPreservation();

            Console.WriteLine("\n--- Mixed Array Types in Same Table ---");
            TestMixedArrayTypes();

            Console.WriteLine("\n--- Empty Array Handling ---");
            TestEmptyArrayHandling();

            Console.WriteLine("\n--- Null Array Handling ---");
            TestNullArrayHandling();

            Console.WriteLine("\n--- Value Accuracy Verification ---");
            TestValueAccuracy();

            Console.WriteLine("\n--- OriginalType Property Verification ---");
            TestOriginalTypeProperty();

            Console.WriteLine("\n--- Backward Compatibility (No OriginalType) ---");
            TestBackwardCompatibility();
        }

        private static void TestFloatArrayPreservation()
        {
            float[] original = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

            DataTable dt = new DataTable("FloatArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Embedding", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["Embedding"];
            bool isFloatArray = resultValue is float[];
            bool valuesMatch = false;

            if (isFloatArray)
            {
                float[] resultArray = (float[])resultValue;
                valuesMatch = resultArray.Length == original.Length;
                for (int i = 0; i < original.Length && valuesMatch; i++)
                {
                    valuesMatch = Math.Abs(resultArray[i] - original[i]) < 0.0001f;
                }
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is float[]: {isFloatArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("float[] type preserved", isFloatArray);
            AssertTest("float[] values match", valuesMatch);
        }

        private static void TestDoubleArrayPreservation()
        {
            double[] original = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5 };

            DataTable dt = new DataTable("DoubleArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Values", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["Values"];
            bool isDoubleArray = resultValue is double[];
            bool valuesMatch = false;

            if (isDoubleArray)
            {
                double[] resultArray = (double[])resultValue;
                valuesMatch = resultArray.Length == original.Length;
                for (int i = 0; i < original.Length && valuesMatch; i++)
                {
                    valuesMatch = Math.Abs(resultArray[i] - original[i]) < 0.0001;
                }
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is double[]: {isDoubleArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("double[] type preserved", isDoubleArray);
            AssertTest("double[] values match", valuesMatch);
        }

        private static void TestIntArrayPreservation()
        {
            int[] original = new int[] { 1, 2, 3, 4, 5 };

            DataTable dt = new DataTable("IntArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Tags", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["Tags"];
            bool isIntArray = resultValue is int[];
            bool valuesMatch = false;

            if (isIntArray)
            {
                int[] resultArray = (int[])resultValue;
                valuesMatch = resultArray.SequenceEqual(original);
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is int[]: {isIntArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("int[] type preserved", isIntArray);
            AssertTest("int[] values match", valuesMatch);
        }

        private static void TestLongArrayPreservation()
        {
            long[] original = new long[] { 100000000000L, 200000000000L, 300000000000L };

            DataTable dt = new DataTable("LongArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("BigNumbers", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["BigNumbers"];
            bool isLongArray = resultValue is long[];
            bool valuesMatch = false;

            if (isLongArray)
            {
                long[] resultArray = (long[])resultValue;
                valuesMatch = resultArray.SequenceEqual(original);
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is long[]: {isLongArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("long[] type preserved", isLongArray);
            AssertTest("long[] values match", valuesMatch);
        }

        private static void TestDecimalArrayPreservation()
        {
            decimal[] original = new decimal[] { 1.234567890123456789m, 9.876543210987654321m };

            DataTable dt = new DataTable("DecimalArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("PreciseValues", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["PreciseValues"];
            bool isDecimalArray = resultValue is decimal[];
            bool valuesMatch = false;

            if (isDecimalArray)
            {
                decimal[] resultArray = (decimal[])resultValue;
                valuesMatch = resultArray.SequenceEqual(original);
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is decimal[]: {isDecimalArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("decimal[] type preserved", isDecimalArray);
            AssertTest("decimal[] values match", valuesMatch);
        }

        private static void TestShortArrayPreservation()
        {
            short[] original = new short[] { 1, 2, 3, 4, 5 };

            DataTable dt = new DataTable("ShortArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("SmallNumbers", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["SmallNumbers"];
            bool isShortArray = resultValue is short[];
            bool valuesMatch = false;

            if (isShortArray)
            {
                short[] resultArray = (short[])resultValue;
                valuesMatch = resultArray.SequenceEqual(original);
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is short[]: {isShortArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("short[] type preserved", isShortArray);
            AssertTest("short[] values match", valuesMatch);
        }

        private static void TestMixedArrayTypes()
        {
            float[] floatArr = new float[] { 0.1f, 0.2f };
            int[] intArr = new int[] { 1, 2, 3 };
            double[] doubleArr = new double[] { 1.1, 2.2 };

            DataTable dt = new DataTable("MixedArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("FloatValues", typeof(object));
            dt.Columns.Add("IntValues", typeof(object));
            dt.Columns.Add("DoubleValues", typeof(object));
            dt.Rows.Add(1, floatArr, intArr, doubleArr);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object floatResult = resultDt.Rows[0]["FloatValues"];
            object intResult = resultDt.Rows[0]["IntValues"];
            object doubleResult = resultDt.Rows[0]["DoubleValues"];

            bool floatCorrect = floatResult is float[];
            bool intCorrect = intResult is int[];
            bool doubleCorrect = doubleResult is double[];

            Console.WriteLine($"  FloatValues type: {floatResult?.GetType().Name ?? "null"} - Correct: {floatCorrect}");
            Console.WriteLine($"  IntValues type: {intResult?.GetType().Name ?? "null"} - Correct: {intCorrect}");
            Console.WriteLine($"  DoubleValues type: {doubleResult?.GetType().Name ?? "null"} - Correct: {doubleCorrect}");

            AssertTest("Mixed types: float[] preserved", floatCorrect);
            AssertTest("Mixed types: int[] preserved", intCorrect);
            AssertTest("Mixed types: double[] preserved", doubleCorrect);
        }

        private static void TestEmptyArrayHandling()
        {
            float[] emptyArray = new float[0];

            DataTable dt = new DataTable("EmptyArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("EmptyFloats", typeof(object));
            dt.Rows.Add(1, emptyArray);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["EmptyFloats"];
            bool isFloatArray = resultValue is float[];
            bool isEmpty = isFloatArray && ((float[])resultValue).Length == 0;

            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is float[]: {isFloatArray}");
            Console.WriteLine($"  Is empty: {isEmpty}");

            AssertTest("Empty array type preserved", isFloatArray);
            AssertTest("Empty array length is 0", isEmpty);
        }

        private static void TestNullArrayHandling()
        {
            DataTable dt = new DataTable("NullArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("NullableArray", typeof(object));
            dt.Rows.Add(1, DBNull.Value);
            dt.Rows.Add(2, new float[] { 1.0f });

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object nullResult = resultDt.Rows[0]["NullableArray"];
            object arrayResult = resultDt.Rows[1]["NullableArray"];

            bool nullCorrect = nullResult == DBNull.Value;
            bool arrayCorrect = arrayResult is float[];

            Console.WriteLine($"  Row 0 (null) - Value is DBNull: {nullCorrect}");
            Console.WriteLine($"  Row 1 (array) - Type: {arrayResult?.GetType().Name ?? "null"} - Is float[]: {arrayCorrect}");

            AssertTest("Null array preserved as DBNull", nullCorrect);
            AssertTest("Non-null array in same column preserved", arrayCorrect);
        }

        private static void TestValueAccuracy()
        {
            float[] floatOriginal = new float[] { 0.123456789f, -0.987654321f, 0.0f, float.MaxValue, float.MinValue };

            DataTable dt = new DataTable("ValueAccuracyTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("PreciseFloats", typeof(object));
            dt.Rows.Add(1, floatOriginal);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            float[] resultArray = resultDt.Rows[0]["PreciseFloats"] as float[];
            bool allMatch = resultArray != null && resultArray.Length == floatOriginal.Length;

            if (allMatch)
            {
                for (int i = 0; i < floatOriginal.Length; i++)
                {
                    float diff = Math.Abs(resultArray[i] - floatOriginal[i]);
                    float tolerance = Math.Abs(floatOriginal[i]) * 0.0001f;
                    if (floatOriginal[i] == 0) tolerance = 0.0001f;
                    if (diff > tolerance)
                    {
                        Console.WriteLine($"  Value mismatch at index {i}: original={floatOriginal[i]}, result={resultArray[i]}");
                        allMatch = false;
                    }
                }
            }

            Console.WriteLine($"  All values preserved accurately: {allMatch}");
            AssertTest("Float values preserved with accuracy", allMatch);
        }

        private static void TestOriginalTypeProperty()
        {
            float[] floatArr = new float[] { 1.0f, 2.0f };

            DataTable dt = new DataTable("OriginalTypeTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Embedding", typeof(object));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, floatArr, "Test");

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);

            SerializableColumn embeddingCol = sdt.Columns.FirstOrDefault(c => c.Name == "Embedding");
            SerializableColumn nameCol = sdt.Columns.FirstOrDefault(c => c.Name == "Name");
            SerializableColumn idCol = sdt.Columns.FirstOrDefault(c => c.Name == "Id");

            bool embeddingHasOriginalType = embeddingCol?.OriginalType != null;
            bool nameHasNoOriginalType = nameCol?.OriginalType == null;
            bool idHasNoOriginalType = idCol?.OriginalType == null;

            Console.WriteLine($"  Embedding column OriginalType: {embeddingCol?.OriginalType ?? "null"}");
            Console.WriteLine($"  Name column OriginalType: {nameCol?.OriginalType ?? "null"}");
            Console.WriteLine($"  Id column OriginalType: {idCol?.OriginalType ?? "null"}");

            AssertTest("Array column has OriginalType set", embeddingHasOriginalType);
            AssertTest("String column has no OriginalType", nameHasNoOriginalType);
            AssertTest("Int column has no OriginalType", idHasNoOriginalType);
        }

        private static void TestBackwardCompatibility()
        {
            // Simulate old serialized data without OriginalType
            string oldFormatJson = @"{
                ""Name"": ""BackwardCompatTest"",
                ""Columns"": [
                    { ""Name"": ""Id"", ""Type"": ""Int32"" },
                    { ""Name"": ""Data"", ""Type"": ""Object"" }
                ],
                ""Rows"": [
                    { ""Id"": 1, ""Data"": [1.5, 2.5, 3.5] }
                ]
            }";

            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(oldFormatJson);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object dataValue = resultDt.Rows[0]["Data"];

            // Without OriginalType, it should fall back to object[] with decimals
            bool isObjectArray = dataValue is object[];

            Console.WriteLine($"  Result type (no OriginalType): {dataValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Falls back to object[]: {isObjectArray}");

            AssertTest("Backward compatible: old data without OriginalType works", isObjectArray);
        }

        private static void AssertTest(string testName, bool passed)
        {
            if (passed)
            {
                Console.WriteLine($"  ✓ PASS: {testName}");
                _TestsPassed++;
            }
            else
            {
                Console.WriteLine($"  ✗ FAIL: {testName}");
                _TestsFailed++;
            }
        }

        private static void TestNonNumericArrayTypes()
        {
            Console.WriteLine("\n--- string[] Round-Trip ---");
            TestStringArrayPreservation();

            Console.WriteLine("\n--- bool[] Round-Trip ---");
            TestBoolArrayPreservation();

            Console.WriteLine("\n--- Guid[] Round-Trip ---");
            TestGuidArrayPreservation();

            Console.WriteLine("\n--- DateTime[] Round-Trip ---");
            TestDateTimeArrayPreservation();
        }

        private static void TestStringArrayPreservation()
        {
            string[] original = new string[] { "hello", "world", "test", "data" };

            DataTable dt = new DataTable("StringArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Tags", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["Tags"];
            bool isStringArray = resultValue is string[];
            bool valuesMatch = false;

            if (isStringArray)
            {
                string[] resultArray = (string[])resultValue;
                valuesMatch = resultArray.SequenceEqual(original);
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is string[]: {isStringArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("string[] type preserved", isStringArray);
            AssertTest("string[] values match", valuesMatch);
        }

        private static void TestBoolArrayPreservation()
        {
            bool[] original = new bool[] { true, false, true, true, false };

            DataTable dt = new DataTable("BoolArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Flags", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["Flags"];
            bool isBoolArray = resultValue is bool[];
            bool valuesMatch = false;

            if (isBoolArray)
            {
                bool[] resultArray = (bool[])resultValue;
                valuesMatch = resultArray.SequenceEqual(original);
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is bool[]: {isBoolArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("bool[] type preserved", isBoolArray);
            AssertTest("bool[] values match", valuesMatch);
        }

        private static void TestGuidArrayPreservation()
        {
            Guid[] original = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            DataTable dt = new DataTable("GuidArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Identifiers", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["Identifiers"];
            bool isGuidArray = resultValue is Guid[];
            bool valuesMatch = false;

            if (isGuidArray)
            {
                Guid[] resultArray = (Guid[])resultValue;
                valuesMatch = resultArray.SequenceEqual(original);
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is Guid[]: {isGuidArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("Guid[] type preserved", isGuidArray);
            AssertTest("Guid[] values match", valuesMatch);
        }

        private static void TestDateTimeArrayPreservation()
        {
            DateTime[] original = new DateTime[]
            {
                new DateTime(2024, 1, 15, 10, 30, 0),
                new DateTime(2024, 6, 20, 14, 45, 30),
                new DateTime(2024, 12, 31, 23, 59, 59)
            };

            DataTable dt = new DataTable("DateTimeArrayTest");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Timestamps", typeof(object));
            dt.Rows.Add(1, original);

            SerializableDataTable sdt = SerializableDataTable.FromDataTable(dt);
            string json = JsonSerializer.Serialize(sdt);
            SerializableDataTable deserializedSdt = JsonSerializer.Deserialize<SerializableDataTable>(json);
            DataTable resultDt = deserializedSdt.ToDataTable();

            object resultValue = resultDt.Rows[0]["Timestamps"];
            bool isDateTimeArray = resultValue is DateTime[];
            bool valuesMatch = false;

            if (isDateTimeArray)
            {
                DateTime[] resultArray = (DateTime[])resultValue;
                valuesMatch = resultArray.SequenceEqual(original);
            }

            Console.WriteLine($"  Original type: {original.GetType().Name}");
            Console.WriteLine($"  Result type: {resultValue?.GetType().Name ?? "null"}");
            Console.WriteLine($"  Is DateTime[]: {isDateTimeArray}");
            Console.WriteLine($"  Values match: {valuesMatch}");

            AssertTest("DateTime[] type preserved", isDateTimeArray);
            AssertTest("DateTime[] values match", valuesMatch);
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
            dt.Columns.Add("Int16Col", typeof(Int16));
            dt.Columns.Add("Int32Col", typeof(int));
            dt.Columns.Add("Int64Col", typeof(long));
            dt.Columns.Add("UInt16Col", typeof(UInt16));
            dt.Columns.Add("UInt32Col", typeof(UInt32));
            dt.Columns.Add("UInt64Col", typeof(UInt64));
            dt.Columns.Add("DecimalCol", typeof(decimal));
            dt.Columns.Add("DoubleCol", typeof(double));
            dt.Columns.Add("FloatCol", typeof(float));
            dt.Columns.Add("BoolCol", typeof(bool));
            dt.Columns.Add("DateTimeCol", typeof(DateTime));
            dt.Columns.Add("TimeSpanCol", typeof(TimeSpan));
            dt.Columns.Add("GuidCol", typeof(Guid));
            dt.Columns.Add("CharCol", typeof(char));

            dt.Rows.Add(
                "Test String",
                (Int16)12345,
                42,
                9223372036854775807L,
                (UInt16)65000,
                (UInt32)4000000000,
                (UInt64)18000000000000000000,
                123.45m,
                3.14159265359,
                2.71828f,
                true,
                DateTime.Now,
                TimeSpan.FromHours(25.5),
                Guid.NewGuid(),
                'A'
            );

            return dt;
        }

        private static DataTable CreateDataTableWithComplexTypes()
        {
            DataTable dt = new DataTable("ComplexTypes");

            // Add columns with complex/unknown types (arrays)
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Embedding", typeof(object));   // float[] - simulates pgvector
            dt.Columns.Add("Tags", typeof(object));        // int[] - general array type

            // Create sample vectors (similar to pgvector embeddings)
            float[] embedding1 = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };
            float[] embedding2 = new float[] { -0.5f, 0.0f, 0.5f, 1.0f, -1.0f };

            int[] tags1 = new int[] { 1, 2, 3, 4, 5 };
            int[] tags2 = new int[] { 10, 20, 30 };

            dt.Rows.Add(1, "Document 1", embedding1, tags1);
            dt.Rows.Add(2, "Document 2", embedding2, tags2);
            dt.Rows.Add(3, "Document 3", DBNull.Value, DBNull.Value);

            return dt;
        }

        private static SerializableDataTable CreateSerializableDataTableDirectly()
        {
            var table = new SerializableDataTable("DirectlyCreated");

            // Add columns
            table.Columns.Add(new SerializableColumn { Name = "ProductId", Type = ColumnValueTypeEnum.Int32 });
            table.Columns.Add(new SerializableColumn { Name = "ProductName", Type = ColumnValueTypeEnum.String });
            table.Columns.Add(new SerializableColumn { Name = "Price", Type = ColumnValueTypeEnum.Decimal });
            table.Columns.Add(new SerializableColumn { Name = "InStock", Type = ColumnValueTypeEnum.Boolean });

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

            if (dt.Columns.Count == 0)
            {
                Console.WriteLine("No columns defined.");
                return;
            }

            // Calculate column widths
            int[] columnWidths = new int[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataColumn col = dt.Columns[i];
                string header = $"{col.ColumnName} ({col.DataType.Name})";
                columnWidths[i] = Math.Max(header.Length, 8); // Minimum width of 8

                // Check row data for wider content
                foreach (DataRow row in dt.Rows)
                {
                    string cellValue = FormatCellValue(row[i]);
                    columnWidths[i] = Math.Max(columnWidths[i], cellValue.Length);
                }

                // Add padding
                columnWidths[i] += 2;
            }

            // Print column headers
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string header = $"{dt.Columns[i].ColumnName} ({dt.Columns[i].DataType.Name})";
                Console.Write(header.PadRight(columnWidths[i]));
            }
            Console.WriteLine();

            // Print separator
            int totalWidth = columnWidths.Sum();
            Console.WriteLine(new string('-', totalWidth));

            // Print rows
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string cellValue = FormatCellValue(row[i]);
                    Console.Write(cellValue.PadRight(columnWidths[i]));
                }
                Console.WriteLine();
            }
        }

        private static string FormatCellValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";

            if (value is Array array)
                return JsonSerializer.Serialize(value);

            return value.ToString();
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

            if (sdt.Columns.Count == 0)
            {
                Console.WriteLine("No columns defined.");
                return;
            }

            // Calculate column widths
            int[] columnWidths = new int[sdt.Columns.Count];
            for (int i = 0; i < sdt.Columns.Count; i++)
            {
                SerializableColumn col = sdt.Columns[i];
                columnWidths[i] = Math.Max(col.Name.Length, 8); // Minimum width of 8

                // Check row data for wider content
                foreach (Dictionary<string, object> row in sdt.Rows)
                {
                    if (row.ContainsKey(col.Name))
                    {
                        string cellValue = FormatCellValue(row[col.Name]);
                        columnWidths[i] = Math.Max(columnWidths[i], cellValue.Length);
                    }
                    else
                    {
                        columnWidths[i] = Math.Max(columnWidths[i], 7); // "MISSING"
                    }
                }

                // Add padding
                columnWidths[i] += 2;
            }

            // Print column headers
            for (int i = 0; i < sdt.Columns.Count; i++)
            {
                Console.Write(sdt.Columns[i].Name.PadRight(columnWidths[i]));
            }
            Console.WriteLine();

            // Print separator
            int totalWidth = columnWidths.Sum();
            Console.WriteLine(new string('-', totalWidth));

            // Print rows
            foreach (Dictionary<string, object> row in sdt.Rows)
            {
                for (int i = 0; i < sdt.Columns.Count; i++)
                {
                    SerializableColumn col = sdt.Columns[i];
                    string cellValue;

                    if (row.ContainsKey(col.Name))
                    {
                        cellValue = FormatCellValue(row[col.Name]);
                    }
                    else
                    {
                        cellValue = "MISSING";
                    }

                    Console.Write(cellValue.PadRight(columnWidths[i]));
                }
                Console.WriteLine();
            }

            // Also show JSON representation for better visualization
            Console.WriteLine("\nJSON Representation:");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            string json = JsonSerializer.Serialize(sdt, options);
            Console.WriteLine(json);
        }
    }
}