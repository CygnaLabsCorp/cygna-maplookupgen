# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Cygna.MapLookupGen: a Roslyn incremental source generator. Marking a `partial` class `[MapLookup]` makes it generate a `Dictionary<ulong, string>` keyed by the XxHash64 of each string constant's UTF-8 bytes, plus `Hash(ReadOnlySpan<byte>)` and `MapContains(...)`. A UTF-8 span can then be matched against the constants without decoding it. The repo ships three NuGet packages on the CygnaLabsCorp GitHub Packages feed: `Cygna.MapLookupGen` (the meta package), `Cygna.MapLookupGen.Core` and `Cygna.MapLookupGen.Generators`.

## Related repos (Cygna source-generator family)

- `Cygna.Generators.Sdk` (pinned 1.0.25) packs the generator project as an analyzer.
- `Cygna.Generators.Core` (pinned 1.0.4, `PrivateAssets="all"`) supplies the helper classes (`IncrementalSourceGenerator`, metadata, writers) as source compiled into the generator.
- `cygna-codegen` (`Cygna.CodeGen`) is the older compiled-assembly version of the same helpers, used by `cygna-filecache`. This repo does not use it.
- `cygna-dotnet-compare` is standalone.

## Layout

- `Cygna.MapLookupGen.Core/` targets `net10.0;netstandard2.1` and holds `MapLookupAttribute` and `HashMethod` (namespace `Cygna.MapLookupGen`).
- `Cygna.MapLookupGen.Generators/` targets `netstandard2.0` and holds `LookupMapGenerator` (the entry point), `Generator/BaseGenerator.cs` (the code emitter), `Symbols.cs`, `Structure.cs` and `DiganosticDescriptors.cs` (the file name is misspelled).
- `Cygna.MapLookupGen/` is the meta package that references the two projects above. It has no code.
- `Cygna.MapLookupGen.Tests/` is an xUnit project on `net10.0`. Its only test body is commented out, because the `SyntaxTreeBuilder` helper it needs is commented out in `Cygna.Generators.Core`.

## Common commands

```powershell
dotnet build Cygna.MapLookupGen.sln --configuration Release
dotnet test Cygna.MapLookupGen.sln --configuration Release --no-build
dotnet test Cygna.MapLookupGen.Tests/Cygna.MapLookupGen.Tests.csproj --filter "FullyQualifiedName~GeneratorTests"   # single class/test
dotnet pack --configuration Release --no-build --output ./publish
```

- This needs the .NET 10 SDK. Restore also needs the CygnaLabsCorp GitHub Packages feed (`https://nuget.pkg.github.com/CygnaLabsCorp/index.json`) for `Cygna.Generators.*`. There is no `nuget.config` in the repo, so the feed must be configured on the machine.
- Releases: when you push a `vX.Y.Z` tag, `.github/workflows/main.yml` runs on a self-hosted runner (its steps use PowerShell syntax). It builds, tests and packs with `/p:Version=X.Y.Z`, then pushes all three packages. There is no CI build on PRs. Don't create or push tags unless asked.

## Things that would surprise you

- The generator finds targets by the metadata name string `"Cygna.MapLookupGen.MapLookupAttribute"`. If you rename or move the attribute, update that string too, because nothing fails at compile time.
- Generated code uses `System.IO.Hashing` and UTF-8 string literals, so consumers need that package and C# 11 or later. Keep emitted code within what consumers on `netstandard2.1` can compile.
- `HashMethod.Crc32` exists on the attribute but is not implemented. The map is still keyed by XxHash64, and `Hash`/`MapContains` are not emitted.
- Diagnostic `MapLookup00001` reports a non-`partial` class.
- `.idea/` and `Cygna.MapLookupGen.sln.DotSettings.user` are committed despite `.gitignore`. Don't commit more IDE state.

## Conventions

- Branches: `task/CLSD-<ticket>_<Description>` for regular tasks, `bugfix/CLSD-<ticket>_<Description>` for bug fixes, `feature/CLSD-<ticket>_<Description>` for Jira user stories. Prefix commit messages with the Jira ticket (`CLSD-1516 ...`).
- No Jira ticket numbers (`CLSD-…`) in code comments. The ticket belongs in the branch name and commit message.
- Pull requests: changes reach `main` (and `release/**`) only through a PR. A PR needs 2 approvals, including one from a code owner (`@CygnaLabsCorp/sac-cygna-authors`), and pushing new commits dismisses earlier approvals. Never push directly to `main`.
- C# style: there is no `.editorconfig`. The code consistently uses standard .NET spacing (`Foo(x, y)`, not the spaces-inside-parentheses style of some other Cygna repos), Allman braces and file-scoped namespaces.
- Line length: up to 200 characters is fine and 250 is the absolute max. Don't wrap lines earlier than that just for style.

## Technology and library policy

- **No paid/commercially-licensed libraries.** Use free/open-source dependencies only.
- The generator's `netstandard2.0` target is intentional and required, because generators must load in both the .NET Framework (Visual Studio) and .NET compiler hosts. Never retarget it. Under `Cygna.Generators.Sdk`, only the generator's own DLL is packed, so the generator can't take runtime package dependencies beyond Roslyn (`Microsoft.CodeAnalysis.CSharp` 5.0.0, private). Raising the Roslyn version raises the minimum compiler version for consumers.
- Core and tests are on .NET 10. Stay on current .NET and C# there.

## SonarQube

PRs are scanned by SonarQube, and a passing 'SonarQube Code Analysis' check is required to merge. Write code that passes the scan the first time:

- **No magic numbers or strings**: define them as named constants in the class (`private const ...`) rather than inline literals.
- **Group overloads together**: declare all methods (and constructors) that share a name consecutively, with nothing else between them. Sonar flags a separated overload (S4136), so insert a new overload directly next to the existing ones.
- **Cognitive Complexity limit is 20** (S3776). Nesting depth usually drives the score more than the raw branch count. An inline lambda's body counts against its enclosing method at the lambda's nesting depth, but a named local function or private method is scored separately and starts its own nesting count from zero. When a method built around a deeply nested lambda trips the rule, extract that logic to a local function. Simplifying individual conditions rarely moves the number enough.
- No unused variables or parameters, no commented-out code, no duplicated string literals.
