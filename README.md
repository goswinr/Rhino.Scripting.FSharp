![logo](https://raw.githubusercontent.com/goswinr/Rhino.Scripting.FSharp/main/Docs/img/logo.png)

# Rhino.Scripting.FSharp

[![Rhino.Scripting on nuget.org](https://img.shields.io/nuget/v/Rhino.Scripting.FSharp)](https://www.nuget.org/packages/Rhino.Scripting.FSharp/)
[![Build Status](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/build.yml/badge.svg)](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/build.yml)
[![Docs Build Status](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/docs.yml/badge.svg)](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/docs.yml)
[![Check dotnet tools](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/outdatedDotnetTool.yml/badge.svg)](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/outdatedDotnetTool.yml)
[![license](https://img.shields.io/github/license/goswinr/Rhino.Scripting.FSharp)](LICENSE.md)
![code size](https://img.shields.io/github/languages/code-size/goswinr/Rhino.Scripting.FSharp.svg)


Rhino.Scripting.FSharp is a set of useful extensions to the [Rhino.Scripting](https://github.com/goswinr/Rhino.Scripting) library.
This includes type extension for pretty printing of Rhino objects as well as implementations of commonly used functions in curried form for use with F#.

This library allows you to compose RhinoScript functions with pipelines:

Get started by opening the Rhino.Scripting namespaces.
Opening Rhino.Scripting.FSharp will extend
Rhino.Scripting and Rhino.Geometry types with additional static and member functions.

```fsharp
open Rhino.Scripting
open Rhino.Scripting.FSharp
type rs = RhinoScriptSyntax
```

```fsharp
rs.AddPoint( 1. , 2.,  3.)
|>! rs.setLayer "my points"
|>! rs.setUserText "id" "point123"
|>  rs.setName "123"
```

instead of

```fsharp
let guid = rs.AddPoint( 1. , 2.,  3.)
rs.ObjectLayer (guid, "my points")
rs.SetUserText (guid, "id", "point123")
rs.ObjectName (guid, "123")
```

The `|>!` operator is part of Rhino.Scripting.FSharp library.
It passes its input on as output. See [definition](https://github.com/goswinr/Rhino.Scripting.FSharp/blob/main/Src/Rhino.Scripting/Curried.fs#L16).

### Full API Documentation

[goswinr.github.io/Rhino.Scripting.FSharp](https://goswinr.github.io/Rhino.Scripting.FSharp)

## Thread Safety
While the main Rhino Document is officially not thread safe, this library can be used from any thread.<br>
If running async this library will automatically marshal all calls that affect the UI to the main Rhino UI thread <br>
and wait for switching back till completion on UI thread.<br>
Modifying the Rhino Document from a background thread is actually OK as long as there is only one thread doing it.<br>
The main reason to use this library async is to keep the Rhino UI and Fesh scripting editor UI responsive while doing long running operations.

## Contributing
Contributions are welcome even for small things like typos. If you have problems with this library please submit an issue.

### License
[MIT](https://github.com/goswinr/Rhino.Scripting.FSharp/blob/main/LICENSE.md)

### Changelog
see [CHANGELOG.md](https://github.com/goswinr/Rhino.Scripting.FSharp/blob/main/CHANGELOG.md)
