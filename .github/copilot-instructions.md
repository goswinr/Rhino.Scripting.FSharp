# Rhino.Scripting.FSharp - AI Coding Guidelines

## Project Overview
F# extension library for [Rhino.Scripting](https://github.com/goswinr/Rhino.Scripting) that adds type extensions and curried functions for idiomatic F# pipeline composition in Rhino3D scripting. Targets .NET Framework 4.8 (Rhino 7) and .NET 8.0 (Rhino 8), the same as Rhino.Scripting. There are no tests in this project.

## Build & Release

```powershell
dotnet build  # Builds for both net48 and net8.0
```

- Version is extracted from [CHANGELOG.md](CHANGELOG.md) by `Ionide.KeepAChangelog.Tasks` — update the changelog to set the version.
- Release: commit, push to main, then `git tag 0.X.Y && git push origin 0.X.Y`. CI validates the tag matches the CHANGELOG version and publishes to NuGet.
- `FSharp.Core` is pinned at 6.0.7 — do not update.

## Architecture

### File Compile Order (critical in F#)
The `.fsproj` defines strict compile order — **file order matters**. New files must be inserted at the correct position:

1. [Src/UtilRhinoScriptingFSharp.fs](Src/UtilRhinoScriptingFSharp.fs) — tolerances, math helpers, `RhinoScriptingFSharpException`
2. [Src/Vector3d.fs](Src/Vector3d.fs), [Src/Point3d.fs](Src/Point3d.fs), [Src/Line.fs](Src/Line.fs), [Src/Plane.fs](Src/Plane.fs) — geometry type extensions
3. [Src/Rhino.Scripting/Printing.fs](Src/Rhino.Scripting/Printing.fs), [Selection.fs](Src/Rhino.Scripting/Selection.fs), [Curried.fs](Src/Rhino.Scripting/Curried.fs), [Vectors.fs](Src/Rhino.Scripting/Vectors.fs), [Curve.fs](Src/Rhino.Scripting/Curve.fs), [Brep.fs](Src/Rhino.Scripting/Brep.fs), [Mesh.fs](Src/Rhino.Scripting/Mesh.fs) — RhinoScriptSyntax curried wrappers
4. [Src/RhPoints.fs](Src/RhPoints.fs), [Src/RhTopology.fs](Src/RhTopology.fs) — higher-level abstractions (`[<RequireQualifiedAccess>]`)


### Key Patterns

**Type Extensions via AutoOpen Modules** — all geometry extensions follow this pattern (see [Line.fs](Src/Line.fs), [Vector3d.fs](Src/Vector3d.fs)):
```fsharp
namespace Rhino.Scripting.FSharp

[<AutoOpen>]
module AutoOpenLine =
    type Line with
        member inline ln.LengthSq = ...
```

**Never shadow RhinoCommon or Rhino.Scripting members** — F# always prefers an intrinsic member over an extension member with the same name and signature, even when the extension is opened later. Such an extension is silently never called (e.g. an extension `ln.Extend(a, b)` resolves to RhinoCommon's mutating `Line.Extend(a, b) : bool`). Use a different name, a different signature (e.g. a required `float<Cosine.cosine>` argument), or a static member like `Line.extend` instead.

**Curried RhinoScript Wrappers** — object ID is always the last parameter for pipeline use (see [Curried.fs](Src/Rhino.Scripting/Curried.fs)):
```fsharp
static member setLayer (layer:string) (objectId:Guid) : unit =
    RhinoScriptSyntax.ObjectLayer(objectId, layer, createLayerIfMissing=true)
```

**The `|>!` Operator** (defined in [Curried.fs](Src/Rhino.Scripting/Curried.fs)) — passes input through while executing a side effect:
```fsharp
let inline ( |>! ) x f = f x |> ignore; x
```

**Inline + Hidden Error Helpers** — performance-critical members use `inline`, with non-inlined error methods hidden via `[<Obsolete>]`:
```fsharp
[<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining...)")>]
member v.FailedUnitized() = RhinoScriptingFSharpException.Raise "..."
member inline v.Unitized = ...
```

**Error Handling** — use `RhinoScriptingFSharpException.Raise` with F# format strings (auto-prefixes `"Rhino.Scripting.FSharp."`, so don't add that prefix to the message). Always include failing values:
```fsharp
RhinoScriptingFSharpException.Raise "Line.unitTangent: x:%g, y:%g and z:%g are too small" v.X v.Y v.Z
```

**Tolerance Constants** (in [UtilRhinoScriptingFSharp.fs](Src/UtilRhinoScriptingFSharp.fs)):
- `zeroLengthTolerance = 1e-12` — for divisions/unitizing
- `isTooSmall` (threshold 1e-6), `isTooTiny` (threshold 1e-12) — NaN-safe via `not (x > threshold)` idiom

## Code Style

### Naming Conventions
- **Curried wrappers**: verb prefix — `setLayer`, `getLayer`, `setName`, `getName`, `hasUserText`, `matchLayer`, `tryGetName`
- **Geometry members**: PascalCase — `LengthSq`, `Tangent`, `IsXAligned`, `AsString`
- **Modifier members**: `With*` pattern — `WithX`, `WithY`, `WithLength`
- **AutoOpen modules**: `AutoOpen{TypeName}` (e.g., `AutoOpenLine`, `AutoOpenPnt`)
- **Static geometry functions**: camelCase — `createFromMembersXYZ`, `distance`, `angle180`
- **Higher-level modules**: `[<RequireQualifiedAccess>]` with PascalCase module name (`RhPoints`, `RhTopology`)

### Documentation
- XML doc comments (`///`) required on all public members — enforced by `--warnon:3390` and `FsDocsWarnOnMissingDocs`
- Curried wrappers use full `<summary>/<param>/<returns>` XML doc; type annotations in `<param>` as `(Type) Description`
- Return types always explicitly annotated on method signatures

### Compiler Warnings
`--warnon:3390` (XML doc validation), `--warnon:1182` (unused variables) are enabled.

## Dependencies
- **Rhino.Scripting** (0.14.0) — core RhinoScript wrapper
- **RhinoCommon** — v7.x for net48, v8.x for net8.0 (PrivateAssets, not redistributed)
- Code derived from [Euclid](https://github.com/goswinr/Euclid) geometry library (noted in source comments as "Copied from Euclid 0.16")

## Thread Safety
Library handles UI thread marshalling automatically — safe to call from background threads. Single-threaded background modification of the Rhino Document is OK.
