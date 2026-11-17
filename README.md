# Cygna.MapLookupGen

Roslyn source generator that turns the string constants of a class into a hash lookup map. Marking a `partial` class with `[MapLookup]` generates a `Dictionary<ulong, string>` keyed by the XxHash64 of each constant's UTF-8 bytes, so a `ReadOnlySpan<byte>` can be matched against the constants without decoding it to a string first.

## Contents

| Project | Package | Purpose |
| --- | --- | --- |
| `Cygna.MapLookupGen.Core` | `Cygna.MapLookupGen.Core` | `MapLookupAttribute` and `HashMethod` (`net10.0`, `netstandard2.1`) |
| `Cygna.MapLookupGen.Generators` | `Cygna.MapLookupGen.Generators` | The incremental generator (`netstandard2.0`), built on `Cygna.Generators.Sdk` / `Cygna.Generators.Core` |
| `Cygna.MapLookupGen` | `Cygna.MapLookupGen` | Package that references the two above |
| `Cygna.MapLookupGen.Tests` | - | xUnit project; it currently holds a single placeholder test |

## Usage

```csharp
using Cygna.MapLookupGen;

[MapLookup]
public partial class Keywords
{
    public const string Select = "SELECT";
    public const string Insert = "INSERT";
}
```

The generated partial adds:

- `public static Dictionary<ulong, string> MapLookup` with one entry per constant field
- `public static ulong Hash(ReadOnlySpan<byte> data)` (XxHash64)
- `public static bool MapContains(ReadOnlySpan<byte> data, out string output)`

The generated code uses `System.IO.Hashing` and UTF-8 string literals, so the consuming project needs the `System.IO.Hashing` package and C# 11 or later. A class that is not `partial` produces error `MapLookup00001`.

`HashMethod.Crc32` exists on the attribute, but the generator only implements XxHash64: with `Crc32` the dictionary is still keyed by XxHash64 and `Hash` / `MapContains` are not generated.

## Requirements

- .NET 10 SDK.
- Access to the CygnaLabsCorp GitHub Packages feed, which hosts `Cygna.Generators.Sdk` and `Cygna.Generators.Core`. The repository has no `nuget.config`, so the feed must be configured locally.

## Build and test

```shell
dotnet build Cygna.MapLookupGen.sln --configuration Release
dotnet test Cygna.MapLookupGen.sln --configuration Release --no-build
```

## Publishing

The GitHub workflow runs on tags of the form `vX.Y.Z`: it builds, tests, packs and pushes the three packages to the CygnaLabsCorp GitHub Packages feed.
