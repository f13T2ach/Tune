# Tune.Tests

Unit tests for the pure-logic parts of Tune. The suite has no mocks and no UI
dependencies; anything that touches the file system works inside a per-test
temporary directory.

## Running

The .NET 10 SDK needs an MTP runner opt-in, which lives in `global.json` at the
repository root. Run from the repository root:

```
dotnet test
```

To run only the test project:

```
dotnet test --project Tune.Tests/Tune.Tests.csproj
```

## Conventions

### Layout

A test type mirrors the path of the type it covers, with a `Tune.Tests` prefix.
`Tune/Helpers/ValueConverters/SecondsToTimeConverter.cs` is covered by
`Helpers/ValueConverters/SecondsToTimeConverterTests.cs` in namespace
`Tune.Tests.Helpers.ValueConverters`.

### Comments

- Every comment and XML doc is written in English.
- Every type and member carries exactly one single-line `<summary>`.
- A summary is one sentence, ends with a period, and describes what the test
  proves; it never restates the method name.
- `<param>`, `<typeparam>` and `<returns>` tags are not used, so the summary
  always has the same shape.
- A test body carries no comment unless an assertion is not self-explanatory,
  in which case it gets a single short line.

### Sections

Tests inside a type are grouped by aspect and delimited by a region banner:

```csharp
#region ==== Failure ====

#endregion
```

Section names are short PascalCase nouns, reused across the suite wherever the
aspect matches: `Success`, `Failure`, `Accumulation`, `Deconstruction`,
`Construction`, `Round Trip`, `Edge Cases`, `Formatting`, `Parsing`,
`Serialization`, `Deserialization`, `Listing`, `Write`, `Read`, `Modify`,
`Delete`, `Forward Conversion`, `Reverse Conversion`, `Resolution`.

### Naming

- Test types: `<TypeUnderTest>Tests`.
- Test methods: `<Subject>_<Scenario>_<ExpectedOutcome>`, for example
  `FailureResult_WithoutCustomMessage_UsesExceptionMessage`.
- Private helpers: verb-first PascalCase, for example `Format` or `Parse`.

### Layout Details

Allman braces, four-space indent, and a `namespace` block rather than a
file-scoped namespace, matching the Tune project.

`using` directives sit at the top: the `System.*` namespaces first, a blank
line, then the remaining namespaces in alphabetical order.

Assertions use the `Assert.Equal(expected, actual)` argument order.
