# Rhino.Scripting.FSharp - AI Coding Guidelines

## Project Overview
F# extension library for [Rhino.Scripting](https://github.com/goswinr/Rhino.Scripting) that adds type extensions and curried functions for idiomatic F# pipelines in Rhino3D scripting. Targets both .NET Framework 4.8 (Rhino 7) and .NET 7 (Rhino 8).

## Architecture

### File Structure & Compile Order
The `.fsproj` defines strict compile order - **file order matters in F#**:
1. `Src/UtilRHinoScriptingFSharp.fs` - Core utilities, tolerances, math helpers
2. `Src/Vector3d.fs`, `Point3d.fs`, `Line.fs`, `Plane.fs` - Geometry type extensions
3. `Src/Rhino.Scripting/*.fs` - RhinoScriptSyntax curried extensions
4. `Src/RhPoints.fs`, `RhTopology.fs` - Higher-level abstractions

### Key Patterns

**Type Extensions via AutoOpen Modules**
All geometry extensions use `[<AutoOpen>]` modules with `type X with` syntax:
```fsharp
[<AutoOpen>]
module AutoOpenLine =
    type Line with
        member inline ln.Length = ...
```

**Curried Functions for Pipelines**
The `|>!` operator (defined in [Curried.fs](Src/Rhino.Scripting/Curried.fs)) passes input through while executing side effects:
```fsharp
let inline ( |>! ) x f = f x |> ignore; x
```

**Error Handling**
Use `RhinoScriptingFSharpException.Raise` for formatted errors:
```fsharp
RhinoScriptingFSharpException.Raise "Line.UnitTangent: x:%g, y:%g..."
```

**Tolerance Constants** (in `UtilRHinoScriptingFSharp`):
- `zeroLengthTolerance = 1e-12` - For divisions/unitizing
- `isTooSmall` (1e-6), `isTooTiny` (1e-12) - Validation helpers

### Naming Conventions
- Curried RhinoScript wrappers: `setLayer`, `getLayer`, `setName` (verb prefixes)
- Geometry members: `AsString` (formatting), `UnitTangent`, `IsXAligned`
- Extension modules: `AutoOpen{TypeName}` pattern

## Build & Development

```powershell
dotnet build                    # Builds for both net48 and net7.0
dotnet pack                     # Creates NuGet package
```

Version is managed by `Ionide.KeepAChangelog.Tasks` from [CHANGELOG.md](CHANGELOG.md).

## Dependencies
- `Rhino.Scripting` - Core RhinoScript wrapper (main dependency)
- `RhinoCommon` - Rhino geometry types (version differs by target framework)
- Code derived from [Euclid](https://github.com/goswinr/Euclid) geometry library (noted in comments)

## Thread Safety
Library handles UI thread marshalling automatically - safe to call from background threads.
