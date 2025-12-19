# Plan: Exact Type Preservation for Array Elements

## Problem Statement

When serializing and deserializing array types (e.g., `float[]`), the exact element type is lost:
- **Original**: `float[]`
- **After round-trip**: `object[]` with `decimal` elements

This occurs because JSON numbers are parsed generically (as `int` or `decimal`), and the original element type information is not preserved.

## Proposed Solution

Add an `OriginalType` property to `SerializableColumn` that stores type metadata for array columns. This property will be nullable and only set when the column contains an array type that requires type preservation.

Additionally, rename `ColumnValueType` to `ColumnValueTypeEnum` for clarity and consistency with naming conventions.

## Implementation Steps

### Step 1: Rename ColumnValueType to ColumnValueTypeEnum

**File**: `src/SerializableDataTable/ColumnValueType.cs`

Rename the enum from `ColumnValueType` to `ColumnValueTypeEnum` and update all references throughout the codebase.

### Step 2: Modify SerializableColumn

Add a new nullable property to store the original element type:

**File**: `src/SerializableDataTable/SerializableColumn.cs`

```csharp
/// <summary>
/// Original element type for array columns.
/// Null for non-array columns or when type preservation is not required.
/// Stored as the assembly-qualified type name for serialization compatibility.
/// </summary>
public string? OriginalType { get; set; } = null;
```

Also update the `Type` property to use `ColumnValueTypeEnum`.

### Step 3: Modify FromDataTable to Capture Array Element Types

**File**: `src/SerializableDataTable/SerializableDataTable.cs`

Update the `FromDataTable` method to detect array types and store their element type:

```csharp
foreach (DataColumn col in dt.Columns)
{
    SerializableColumn serCol = new SerializableColumn
    {
        Name = col.ColumnName,
        Type = DataTypeToColumnValueTypeEnum(col.DataType)
    };

    // If the column type is Object, check if actual values are arrays
    // and capture the element type
    if (col.DataType == typeof(object))
    {
        Type? elementType = DetectArrayElementType(dt, col.ColumnName);
        if (elementType != null)
        {
            serCol.OriginalType = elementType.AssemblyQualifiedName;
        }
    }
    // For explicit array types like byte[], also store element type
    else if (col.DataType.IsArray)
    {
        Type? elementType = col.DataType.GetElementType();
        if (elementType != null && elementType != typeof(byte)) // byte[] is already handled
        {
            serCol.OriginalType = elementType.AssemblyQualifiedName;
        }
    }

    ret.Columns.Add(serCol);
}
```

Add a helper method to detect array element types from actual data:

```csharp
private static Type? DetectArrayElementType(DataTable dt, string columnName)
{
    foreach (DataRow row in dt.Rows)
    {
        object cellValue = row[columnName];
        if (cellValue != null && cellValue != DBNull.Value && cellValue.GetType().IsArray)
        {
            return cellValue.GetType().GetElementType();
        }
    }
    return null;
}
```

### Step 4: Modify Deserialization to Use Type Metadata

**File**: `src/SerializableDataTable/SerializableDataTable.cs`

Update `ToDataTable` to pass the original element type to the value conversion:

```csharp
foreach (KeyValuePair<string, object> val in dict)
{
    SerializableColumn? col = Columns.FirstOrDefault(c => c.Name.Equals(val.Key));
    if (col == null)
        throw new ArgumentException("No column exists with name '" + val.Key + "' as found in row " + i + ".");

    object value = GetValue(val.Value, col.OriginalType);
    row[val.Key] = value ?? DBNull.Value;
}
```

Update `GetValue` to accept and use the element type:

```csharp
private static object GetValue(object obj, string? originalType)
{
    if (obj == null) return null;

    if (obj is JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                return ParseJsonArray(element, originalType);
            // ... rest of cases unchanged
        }
    }

    return obj;
}
```

Update `ParseJsonArray` to create typed arrays:

```csharp
private static object ParseJsonArray(JsonElement arrayElement, string? originalType)
{
    int length = arrayElement.GetArrayLength();

    // If we have type metadata, create a properly typed array
    if (!string.IsNullOrEmpty(originalType))
    {
        Type? elementType = Type.GetType(originalType);
        if (elementType != null)
        {
            Array typedArray = Array.CreateInstance(elementType, length);
            int index = 0;
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                object value = GetValue(item, null);
                typedArray.SetValue(ConvertToType(value, elementType), index);
                index++;
            }
            return typedArray;
        }
    }

    // Fallback to object[] if no type metadata
    object[] result = new object[length];
    int idx = 0;
    foreach (JsonElement item in arrayElement.EnumerateArray())
    {
        result[idx] = GetValue(item, null);
        idx++;
    }
    return result;
}
```

Add a helper method for type conversion:

```csharp
private static object ConvertToType(object value, Type targetType)
{
    if (value == null) return null;

    if (targetType == typeof(float))
        return Convert.ToSingle(value);
    if (targetType == typeof(double))
        return Convert.ToDouble(value);
    if (targetType == typeof(decimal))
        return Convert.ToDecimal(value);
    if (targetType == typeof(int))
        return Convert.ToInt32(value);
    if (targetType == typeof(long))
        return Convert.ToInt64(value);
    if (targetType == typeof(short))
        return Convert.ToInt16(value);
    // Add other numeric types as needed

    return Convert.ChangeType(value, targetType);
}
```

## Serialization Format Impact

The serialized JSON will include the new property when set:

**Before**:
```json
{
  "Columns": [
    { "Name": "Embeddings", "Type": "Object" }
  ]
}
```

**After** (when array element type is detected):
```json
{
  "Columns": [
    {
      "Name": "Embeddings",
      "Type": "Object",
      "OriginalType": "System.Single, System.Private.CoreLib, ..."
    }
  ]
}
```

## Backward Compatibility

- **Existing serialized data**: Will deserialize correctly. If `OriginalType` is null/missing, the current behavior (object[] with decimal elements) is preserved.
- **New serialized data**: Will include type metadata when available, enabling exact type reconstruction.

## Testing Considerations

1. Round-trip test with `float[]` - should preserve exact type
2. Round-trip test with `double[]` - should preserve exact type
3. Round-trip test with `int[]` - should preserve exact type
4. Round-trip test with `long[]` - should preserve exact type
5. Round-trip test with `decimal[]` - should preserve exact type
6. Backward compatibility test - deserialize data without `OriginalType`
7. Mixed column test - some columns with arrays, some without
8. Null/empty array handling
9. Nested arrays (if applicable)
10. Value accuracy verification (ensure no precision loss)

## Files to Modify

1. `src/SerializableDataTable/ColumnValueType.cs` - Rename enum to `ColumnValueTypeEnum`
2. `src/SerializableDataTable/SerializableColumn.cs` - Add `OriginalType` property, update type reference
3. `src/SerializableDataTable/SerializableDataTable.cs` - Modify serialization/deserialization logic, update type references
4. `src/SerializableDataTable/MarkdownConverter.cs` - Update type references (if any)
5. `src/Test/Program.cs` - Add comprehensive test cases for array type preservation
