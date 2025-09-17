# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SerializableDataTable is a .NET library that provides an abstraction class for serializing and deserializing data to and from `DataTable` instances. The library supports conversion to JSON and markdown formats.

## Build and Development Commands

### Building the Solution
```bash
dotnet build SerializableDataTable.sln
```

### Building Specific Projects
```bash
# Build the main library
dotnet build SerializableDataTable/SerializableDataTables.csproj

# Build and run test programs
dotnet run --project Test/Test.csproj
dotnet run --project Test.Markdown/Test.Markdown.csproj
dotnet run --project Test.MarkdownConverter/Test.MarkdownConverter.csproj
```

### Package Management
```bash
# Restore NuGet packages
dotnet restore

# Create NuGet package (automatically done during build due to GeneratePackageOnBuild=true)
dotnet pack SerializableDataTable/SerializableDataTables.csproj
```

## Architecture

### Core Components

- **SerializableDataTable**: Main class that wraps `DataTable` functionality with JSON serialization support
- **SerializableColumn**: Represents column metadata including name and data type
- **ColumnValueType**: Enum defining supported data types (String, Int32, Int64, Decimal, Double, Float, Boolean, DateTime, DateTimeOffset, Byte, ByteArray, Char, Guid, Object)
- **MarkdownConverter**: Static utility class for converting tables to markdown format

### Key Features

1. **Bidirectional Conversion**: DataTable ↔ SerializableDataTable
2. **JSON Serialization**: Uses System.Text.Json for serialization
3. **Markdown Export**: Convert tables to markdown format with proper escaping
4. **Type Support**: Handles 13 different data types including nullable values and DBNull
5. **Multi-Target Framework**: Supports .NET Standard 2.0/2.1, .NET 6.0, and .NET 8.0

### Project Structure

- `SerializableDataTable/` - Main library project
- `Test/` - Basic test console application demonstrating library usage
- `Test.Markdown/` - Test application focusing on markdown conversion features
- `Test.MarkdownConverter/` - Additional markdown conversion testing

### Dependencies

- **System.Text.Json 8.0.5** - For JSON serialization (main library)
- **SerializationHelper 1.0.3** - Used in test projects for JSON operations

## Testing

Run test applications to verify functionality:
```bash
dotnet run --project Test/Test.csproj
```

The test program demonstrates:
- DataTable to SerializableDataTable conversion
- Round-trip conversion verification
- Direct SerializableDataTable creation
- Null value handling
- Various data type support

## NuGet Package

The project is configured for automatic NuGet package generation with version 1.0.3. Package metadata includes proper documentation, licensing, and repository information.

## Coding Standards and Style Rules

**THESE RULES MUST BE FOLLOWED STRICTLY**

### Code Organization

- **Namespace Declaration**: Always at the top, with using statements contained INSIDE the namespace block
- **Using Statement Order**: Microsoft/system libraries first (alphabetical), then other libraries (alphabetical)
- **File Structure**: Limit each file to exactly one class or exactly one enum - no nesting multiple classes/enums

### Documentation Standards

- **Public Members**: All public members, constructors, and public methods MUST have XML documentation
- **Private Members**: NO code documentation on private members or private methods
- **Exception Documentation**: Document exceptions using `/// <exception>` tags
- **Nullability**: Document nullability in XML comments
- **Thread Safety**: Document thread safety guarantees in XML comments
- **Default Values**: Outline default, minimum, maximum values and their effects where appropriate

### Variable and Property Standards

- **No var**: Do not use `var` - use actual types
- **Private Members**: Must start with underscore and be Pascal cased (e.g., `_FooBar`, not `_fooBar`)
- **Public Properties**: Use explicit getters/setters with backing variables when validation is required
- **Configurable Values**: Avoid constants - use public members with backing private members set to reasonable defaults

### Asynchronous Programming

- **ConfigureAwait**: Use `.ConfigureAwait(false)` where appropriate
- **CancellationToken**: Every async method should accept CancellationToken unless class has one as member
- **Cancellation Checks**: Check cancellation requests at appropriate places
- **IEnumerable Methods**: When implementing IEnumerable methods, also create async variants with CancellationToken

### Exception Handling

- **Specific Exceptions**: Use specific exception types rather than generic Exception
- **Meaningful Messages**: Always include meaningful error messages with context
- **Custom Exceptions**: Consider custom exception types for domain-specific errors
- **Exception Filters**: Use when appropriate: `catch (SqlException ex) when (ex.Number == 2601)`

### Resource Management

- **IDisposable**: Implement IDisposable/IAsyncDisposable when holding unmanaged resources
- **Using Statements**: Use 'using' statements or declarations for IDisposable objects
- **Dispose Pattern**: Follow full Dispose pattern with `protected virtual void Dispose(bool disposing)`
- **Base Disposal**: Always call `base.Dispose()` in derived classes

### Null Safety and Validation

- **Nullable Reference Types**: Enable `<Nullable>enable</Nullable>` in project files
- **Input Validation**: Validate parameters with guard clauses at method start
- **Null Checks**: Use `ArgumentNullException.ThrowIfNull()` for .NET 6+ or manual checks
- **Result Pattern**: Consider Result pattern or Option/Maybe types for methods that can fail
- **Proactive Null Safety**: Eliminate situations where null might cause exceptions

### Threading and Concurrency

- **Atomic Operations**: Use Interlocked operations for simple atomic operations
- **Read-Heavy Scenarios**: Prefer ReaderWriterLockSlim over lock for read-heavy scenarios

### LINQ and Collections

- **Readability**: Prefer LINQ methods over manual loops when readability is not compromised
- **Existence Checks**: Use `.Any()` instead of `.Count() > 0`
- **Multiple Enumeration**: Be aware of issues - consider `.ToList()` when needed
- **Safe Access**: Use `.FirstOrDefault()` with null checks rather than `.First()`

### Prohibited Practices

- **No Tuples**: Do not use tuples unless absolutely necessary
- **No Assumptions**: Do not assume class members/methods exist on opaque classes - ask for implementation
- **SQL Statements**: If manual SQL strings exist, assume there's a good reason

### Compilation Requirements

- **Error-Free**: Code must compile without errors or warnings
- **README Accuracy**: If README exists, ensure it remains accurate after changes