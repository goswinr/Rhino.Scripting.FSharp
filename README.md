![logo](https://raw.githubusercontent.com/goswinr/Rhino.Scripting.FSharp/main/Docs/img/logo128.png)

# Rhino.Scripting.FSharp

[![Rhino.Scripting on nuget.org](https://img.shields.io/nuget/v/Rhino.Scripting.FSharp)](https://www.nuget.org/packages/Rhino.Scripting.FSharp/)
[![Build Status](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/build.yml/badge.svg)](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/build.yml)
[![Docs Build Status](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/docs.yml/badge.svg)](https://github.com/goswinr/Rhino.Scripting.FSharp/actions/workflows/docs.yml)
[![license](https://img.shields.io/github/license/goswinr/Rhino.Scripting.FSharp)](LICENSE.md)
![code size](https://img.shields.io/github/languages/code-size/goswinr/Rhino.Scripting.FSharp.svg)


Rhino.Scripting.FSharp is a set of useful extensions to the [Rhino.Scripting](https://github.com/goswinr/Rhino.Scripting) library.
This includes type extension for pretty printing of Rhino objects as well as implementations of commonly used functions in curried form for use with F#.

This library allows you to compose RhinoScript functions with pipelines:

Get started by opening the `Rhino.Scripting` namespaces.
Opening `Rhino.Scripting.FSharp` will extend
`Rhino.Scripting` and `Rhino.Geometry` types with additional static and member functions.

```fsharp
#r "nuget: Rhino.Scripting.FSharp"
open Rhino.Scripting
open Rhino.Scripting.FSharp
type rs = RhinoScriptSyntax
```

Now you can use the `|>` and `|>!` operator to chain RhinoScript functions together in a more F# idiomatic way.
The `|>!` operator is part of Rhino.Scripting.FSharp library.
It passes its input on as output. See [definition](https://github.com/goswinr/Rhino.Scripting.FSharp/blob/main/Src/Rhino.Scripting/Curried.fs#L16).

```fsharp
rs.AddPoint( 1. , 2.,  3.)
|>! rs.setLayer "my points"
|>! rs.setUserText "id" "point123"
|>  rs.setName "123"
```

instead of regular RhinoScript syntax like this:

```fsharp
let guid = rs.AddPoint( 1. , 2.,  3.)
rs.ObjectLayer (guid, "my points")
rs.SetUserText (guid, "id", "point123")
rs.ObjectName (guid, "123")
```


## Thread Safety

While the main Rhino Document is officially not thread safe, this library can be used from any thread.<br>
If running async this library will automatically marshal all calls that affect the UI to the main Rhino UI thread <br>
and wait for switching back till completion on UI thread.<br>
Modifying the Rhino Document from a background thread is actually OK as long as there is only one thread doing it.<br>
The main reason to use this library async is to keep the Rhino UI and Fesh scripting editor UI responsive while doing long running operations.

## More Examples

### Transforming Objects in Pipelines

```fsharp
// Move, scale, and color an object in one pipeline
objectId
|>! rs.move (Vector3d(10, 0, 0))
|>! rs.scale Point3d.Origin 1.5
|>! rs.setColor Drawing.Color.Blue
|>  rs.setLayer "Transformed"

// Batch operations on multiple objects
objectIds
|>! rs.setLayers "ProcessedObjects"
|>! rs.setColors Drawing.Color.Red

// Copy all properties from a template object to another
rs.matchAllProperties sourceId targetId
```

### Line Operations

```fsharp
let ln = Line(Point3d(0,0,0), Point3d(10,0,0))

// Divide a line into equal segments
let points = ln |> Line.divide 5    // 6 points (start + 5 divisions)

// Divide by maximum segment length
let pts = ln |> Line.divideMaxLength 3.0

// Extend and shrink
let longer  = ln |> Line.extend 2.0 3.0   // extend 2 at start, 3 at end
let shorter = ln |> Line.shrink 1.0 1.0   // remove 1 from each end

// Evaluate at parameter
let midPoint = ln.EvaluateAt 0.5

// Offset parallel in XY plane
let offsetLine = ln |> Line.offsetXY 2.0

// Find intersection of two infinite lines
let intersectionPt = Line.intersectInOnePoint lineA lineB
```

### Vector3d Extensions

```fsharp
let v = Vector3d(3, 4, 0)

v.Length            // 5.0
v.Unitized          // unit vector with error checking
v.Direction360InXY  // angle in degrees (0 to 360)
v.IsHorizontal      // true if Z component is near zero
v.IsParallelTo(other)
v.IsPerpendicularTo(other)

// Interpolation
let mid = Vector3d.lerp(vecA, vecB, 0.5)
let slerped = Vector3d.slerp(vecA, vecB, 0.5) // spherical interpolation
```

### Point3d Extensions

```fsharp
// Functional point manipulation
let pt =
    startPt
    |> Point3d.withZ 100.0   // set Z coordinate
    |> Point3d.moveX 10.0    // shift in X
    |> Point3d.moveY 5.0     // shift in Y

// Interpolation
let midPt = Point3d.lerp(ptA, ptB, 0.5)

// Point at a given distance toward another point
let ptAtDist = Point3d.distPt(fromPt, towardPt, 15.0)

// Distance queries
let dist = pt.DistanceTo(otherPt)
let closestOnLn = pt.ClosestPointOnLine(lnFrom, lnTo)
```

### Plane Operations

```fsharp
// Create from three points
let plane = Plane.createThreePoints(origin, ptOnXAxis, ptOnPlane)

// Evaluate in plane coordinates
let pt3d = plane.EvaluateAt(2.0, 3.0, 0.0)

// Find where a line pierces a plane
match Plane.intersectLine myLine myPlane with
| Some pt -> printfn "Intersection at %A" pt
| None    -> printfn "Line is parallel to plane"

// Find the line of intersection between two planes
match Plane.intersect planeA planeB with
| Some ln -> printfn "Intersection line: %A" ln
| None    -> printfn "Planes are parallel"
```

### Offset Polylines

```fsharp
// Offset a polyline with uniform distance
let offsetPts = rs.OffsetPoints(cornerPoints, 2.0, loop = true)

// Variable offset per segment
let offsetPts2 =
    rs.OffsetPoints(
        cornerPoints,
        [2.0; 3.0; 2.5; 2.0],   // different offset per segment
        [0.5; 0.5; 0.5; 0.5],   // optional normal offset
        loop = true
    )
```

### Fillet Polyline Corners

```fsharp
// Fillet specific corners with individual radii
let fillets = dict [(1, 2.0); (3, 1.5)]  // corner index -> radius
let filletedCurve = rs.FilletPolyline(fillets, polylinePoints)
```

### Point Set Queries

```fsharp
open Rhino.Scripting.FSharp

// Find nearest point in a set
let nearestIdx = RhPoints.closestPointIdx testPt pointList
let nearestPt  = RhPoints.closestPoint testPt pointList

// Remove near-duplicate points
let cleaned = RhPoints.cullDuplicatePointsInSeq 0.01 noisyPoints

// Find minimum distance between two point sets
let minDist = RhPoints.minDistBetweenPointSets setA setB
```

### Sort Line Segments into a Loop

```fsharp
open Rhino.Scripting.FSharp

// Order disconnected line segments into a continuous path
RhTopology.sortToLoop (fun seg -> seg) lineSegments

// With optional reversing of individual segments
RhTopology.sortToLoopWithReversing
    (fun seg -> seg.AsLine)
    (fun idx seg -> seg.Reverse())
    segments
```

### Remembered Selections

```fsharp
// User picks an object once; subsequent calls reuse the cached selection
let refId = rs.GetObjectAndRemember("Pick reference object")

// ... later in the script, same call returns cached result
let refId2 = rs.GetObjectAndRemember("Pick reference object")
```

### Debugging and Visualization

```fsharp
// Draw a vector as an arrow in the viewport
rs.DrawVector(myVector, fromPoint, "Vectors")

// Visualize a coordinate system (draws X, Y, Z axes)
rs.DrawPlane(myPlane, 5.0, "_Base", "Guides")
```

## Full API Documentation

[goswinr.github.io/Rhino.Scripting.FSharp](https://goswinr.github.io/Rhino.Scripting.FSharp/reference/rhino-scripting-fsharp.html)

### Use of AI and LLMs
All core function are are written by hand to ensure performance and correctness.<br>
However, AI tools have been used for code review, typo and grammar checking in documentation<br>
and to generate not all but many of the tests.

## Contributing
Contributions are welcome even for small things like typos. If you have problems with this library please submit an issue.

### License
[MIT](https://github.com/goswinr/Rhino.Scripting.FSharp/blob/main/LICENSE.md)

### Changelog
see [CHANGELOG.md](https://github.com/goswinr/Rhino.Scripting.FSharp/blob/main/CHANGELOG.md)
