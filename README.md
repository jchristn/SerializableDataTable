<img src="https://github.com/jchristn/SerializableDataTable/blob/main/assets/icon.png?raw=true" data-canonical-src="https://github.com/jchristn/SerializableDataTable/blob/main/assets/icon.png?raw=true" width="128" height="128" />

# SerializableDataTable

SerializableDataTable is a library providing an abstraction class that allows you to serialize and deserialize data to and from a `DataTable` instance.

[![NuGet Version](https://img.shields.io/nuget/v/SerializableDataTable.svg?style=flat)](https://www.nuget.org/packages/SerializableDataTable/) [![NuGet](https://img.shields.io/nuget/dt/SerializableDataTable.svg)](https://www.nuget.org/packages/SerializableDataTable)

## Help, Feedback, Contribute

If you have any issues or feedback, please file an issue here in Github. We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v1.0.x

- Initial release
- Added markdown support

## Example

Refer to the ```Test``` project for exercising the library.  This is example is using the NuGet package [SerializationHelper](https://github.com/jchristn/serializationhelper) for simplicity purposes.

```csharp
using System.Data;
using SerializableDataTables;
using SerializationHelper;

// Create your DataTable and columns
DataTable dt1 = new DataTable();
dt1.Columns.Add(new DataColumn { ... });

// Create your DataRows
DataRow row = dt1.NewRow();
row["[name]"] = "[value]";
dt1.Rows.Add(row);

string json = Serializer.SerializeJson(SerializableDataTable.FromDataTable(dt), true);
Console.WriteLine("JSON: " + Environment.NewLine + json + Environment.NewLine);

DataTable dt2 = Serializer.DeserializeJson<SerializableDataTable>(json).ToDataTable();
```

## Supported Data Types

This library supports the following `DataTable` value types: `string`, `Int32`, `Int64`, `decimal`, `double`, `float`, `bool`, `DateTime`, `DateTimeOffset`, `byte`, `byte[]`, `char`, `Guid`, and `Object`.

## Conversion to Markdown

The `SerializableDataTable` class has a public method `ToMarkdown()` which returns a markdown string.

The static class `MarkdownConverter` can be used with a `DataTable` or a `SerializableDataTable` to produce a markdown string.  

```csharp
using System.Data;
using SerializableDataTables;

SerializableDataTable sdt = ...;
string md = sdt.ToMarkdown();

md = MarkdownConverter.Convert(sdt); // full markdown

Console.WriteLine(MarkdownConverter.ConvertHeaders(sdt)); // show only header row and separator row

foreach (string str in MarkdownConverter.IterateRows(sdt))
{
    Console.Write(str); // print each data row, which has a newline separator already added
}
```

## Version History

Refer to CHANGELOG.md for version history.
